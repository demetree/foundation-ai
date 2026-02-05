//
// Admin Notification Preferences Component
//
// Administrative screen for managing notification preferences for all users.
// AI-assisted development - February 2026
//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

import { UserNotificationPreferenceService, UserNotificationPreferenceData, UserNotificationPreferenceSubmitData } from '../../alerting-data-services/user-notification-preference.service';
import { UserNotificationChannelPreferenceService, UserNotificationChannelPreferenceData, UserNotificationChannelPreferenceSubmitData } from '../../alerting-data-services/user-notification-channel-preference.service';
import { NotificationChannelTypeService, NotificationChannelTypeData } from '../../alerting-data-services/notification-channel-type.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { NavigationService } from '../../utility-services/navigation.service';

/**
 * Editable channel preference model for UI
 */
interface EditableChannelPreference {
    channelTypeId: number;
    channelName: string;
    channelDescription: string;
    icon: string;
    isEnabled: boolean;
    priority: number;
    defaultPriority: number;
    existingPreferenceId: number | null;
}

/**
 * User with their preference status
 */
interface UserWithPreferenceStatus extends AlertingUser {
    hasPreferences: boolean;
    preferenceId: number | null;
}

@Component({
    selector: 'app-admin-notification-preferences',
    standalone: false,
    templateUrl: './admin-notification-preferences.component.html',
    styleUrls: ['./admin-notification-preferences.component.scss']
})
export class AdminNotificationPreferencesComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    //
    // Authorization
    //
    isAuthorized = false;
    authError: string | null = null;

    //
    // User List
    //
    users: UserWithPreferenceStatus[] = [];
    filteredUsers: UserWithPreferenceStatus[] = [];
    searchQuery = '';
    selectedUser: UserWithPreferenceStatus | null = null;
    isLoadingUsers = true;

    //
    // Preference Editor
    //
    isLoadingPreferences = false;
    isSaving = false;
    errorMessage: string | null = null;
    preference: UserNotificationPreferenceData | null = null;
    isNewPreference = false;
    channelPreferences: EditableChannelPreference[] = [];
    channelTypes: NotificationChannelTypeData[] = [];

    // Form fields
    formTimeZoneId = 'UTC';
    formQuietHoursEnabled = false;
    formQuietHoursStart = '22:00';
    formQuietHoursEnd = '08:00';
    formDoNotDisturb = false;
    formDoNotDisturbPermanent = false;
    formDoNotDisturbUntil: string | null = null;

    // Track unsaved changes
    hasUnsavedChanges = false;

    // Common timezones
    commonTimezones = [
        { id: 'UTC', label: 'UTC' },
        { id: 'America/New_York', label: 'Eastern Time (US)' },
        { id: 'America/Chicago', label: 'Central Time (US)' },
        { id: 'America/Denver', label: 'Mountain Time (US)' },
        { id: 'America/Los_Angeles', label: 'Pacific Time (US)' },
        { id: 'America/Toronto', label: 'Eastern Time (Canada)' },
        { id: 'America/St_Johns', label: 'Newfoundland Time' },
        { id: 'Europe/London', label: 'London (GMT/BST)' },
        { id: 'Europe/Paris', label: 'Paris (CET)' },
        { id: 'Europe/Berlin', label: 'Berlin (CET)' },
        { id: 'Asia/Tokyo', label: 'Tokyo (JST)' },
        { id: 'Asia/Shanghai', label: 'Shanghai (CST)' },
        { id: 'Australia/Sydney', label: 'Sydney (AEST)' }
    ];

    // Channel icon mapping
    private channelIcons: { [key: string]: string } = {
        'Email': 'bi-envelope-fill',
        'SMS': 'bi-chat-text-fill',
        'VoiceCall': 'bi-telephone-fill',
        'MobilePush': 'bi-phone-fill',
        'WebPush': 'bi-bell-fill',
        'Teams': 'bi-chat-square-dots-fill'
    };

    constructor(
        private router: Router,
        private userService: AlertingUserService,
        private preferenceService: UserNotificationPreferenceService,
        private channelPreferenceService: UserNotificationChannelPreferenceService,
        private channelTypeService: NotificationChannelTypeService,
        private authService: AuthService,
        private alertService: AlertService,
        private navigationService: NavigationService
    ) { }

    ngOnInit(): void {
        this.checkAuthorization();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    //
    // Authorization
    //
    private checkAuthorization(): void {
        if (!this.authService.isAlertingAdministrator) {
            this.isAuthorized = false;
            this.authError = 'This screen requires Alerting Administrator access.';
            return;
        }
        this.isAuthorized = true;
        this.loadData();
    }

    //
    // Data Loading
    //
    private loadData(): void {
        this.isLoadingUsers = true;

        // Load users and channel types in parallel
        forkJoin({
            users: this.userService.getUsers(),
            channelTypes: this.channelTypeService.GetNotificationChannelTypeList({ active: true, deleted: false }),
            allPreferences: this.preferenceService.GetUserNotificationPreferenceList({ active: true, deleted: false })
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: ({ users, channelTypes, allPreferences }) => {
                    this.channelTypes = channelTypes;

                    // Build user list with preference status
                    const prefMap = new Map<string, UserNotificationPreferenceData>();
                    for (const pref of allPreferences) {
                        if (pref.securityUserObjectGuid) {
                            prefMap.set(pref.securityUserObjectGuid, pref);
                        }
                    }

                    this.users = users.map(u => ({
                        ...u,
                        hasPreferences: prefMap.has(u.objectGuid),
                        preferenceId: prefMap.get(u.objectGuid)?.id as number ?? null
                    }));

                    // Sort by name
                    this.users.sort((a, b) => (a.displayName || '').localeCompare(b.displayName || ''));
                    this.filterUsers();
                    this.isLoadingUsers = false;
                },
                error: (err) => {
                    console.error('Error loading data:', err);
                    this.authError = 'Failed to load user data.';
                    this.isLoadingUsers = false;
                }
            });
    }

    //
    // User List
    //
    filterUsers(): void {
        const query = this.searchQuery.toLowerCase().trim();
        if (!query) {
            this.filteredUsers = [...this.users];
        } else {
            this.filteredUsers = this.users.filter(u =>
                (u.displayName?.toLowerCase().includes(query)) ||
                (u.emailAddress?.toLowerCase().includes(query)) ||
                (u.accountName?.toLowerCase().includes(query))
            );
        }
    }

    selectUser(user: UserWithPreferenceStatus): void {
        if (this.hasUnsavedChanges) {
            if (!confirm('You have unsaved changes. Discard and switch user?')) {
                return;
            }
        }

        this.selectedUser = user;
        this.hasUnsavedChanges = false;
        this.loadUserPreferences(user);
    }

    //
    // Preference Loading
    //
    private loadUserPreferences(user: UserWithPreferenceStatus): void {
        this.isLoadingPreferences = true;
        this.errorMessage = null;

        this.preferenceService.GetUserNotificationPreferenceList({
            securityUserObjectGuid: user.objectGuid,
            active: true,
            deleted: false
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (preferences) => {
                    if (preferences.length > 0) {
                        this.preference = preferences[0];
                        this.isNewPreference = false;
                        this.populateFormFromPreference();
                        this.loadChannelPreferences();
                    } else {
                        this.preference = null;
                        this.isNewPreference = true;
                        this.resetFormToDefaults();
                        this.createDefaultChannelPreferences();
                        this.isLoadingPreferences = false;
                    }
                },
                error: (err) => {
                    console.error('Error loading user preferences:', err);
                    this.errorMessage = 'Failed to load preferences for this user.';
                    this.isLoadingPreferences = false;
                }
            });
    }

    private populateFormFromPreference(): void {
        if (!this.preference) return;

        this.formTimeZoneId = this.preference.timeZoneId || 'UTC';
        this.formQuietHoursEnabled = !!(this.preference.quietHoursStart && this.preference.quietHoursEnd);
        this.formQuietHoursStart = this.preference.quietHoursStart || '22:00';
        this.formQuietHoursEnd = this.preference.quietHoursEnd || '08:00';
        this.formDoNotDisturb = this.preference.isDoNotDisturb;
        this.formDoNotDisturbPermanent = this.preference.isDoNotDisturbPermanent;
        this.formDoNotDisturbUntil = this.preference.doNotDisturbUntil;
    }

    private resetFormToDefaults(): void {
        this.formTimeZoneId = 'UTC';
        this.formQuietHoursEnabled = false;
        this.formQuietHoursStart = '22:00';
        this.formQuietHoursEnd = '08:00';
        this.formDoNotDisturb = false;
        this.formDoNotDisturbPermanent = false;
        this.formDoNotDisturbUntil = null;
    }

    private loadChannelPreferences(): void {
        if (!this.preference) return;

        this.channelPreferenceService.GetUserNotificationChannelPreferenceList({
            userNotificationPreferenceId: this.preference.id,
            active: true,
            deleted: false,
            includeRelations: true
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (existingPrefs) => {
                    this.buildChannelPreferencesList(existingPrefs);
                    this.isLoadingPreferences = false;
                },
                error: (err) => {
                    console.error('Error loading channel preferences:', err);
                    this.createDefaultChannelPreferences();
                    this.isLoadingPreferences = false;
                }
            });
    }

    private buildChannelPreferencesList(existingPrefs: UserNotificationChannelPreferenceData[]): void {
        this.channelPreferences = this.channelTypes.map(ct => {
            const existing = existingPrefs.find(p => p.notificationChannelTypeId === ct.id);
            return {
                channelTypeId: ct.id as number,
                channelName: ct.name,
                channelDescription: ct.description || '',
                icon: this.channelIcons[ct.name] || 'bi-bell-fill',
                isEnabled: existing ? existing.isEnabled : true,
                priority: existing?.priorityOverride as number || ct.defaultPriority as number,
                defaultPriority: ct.defaultPriority as number,
                existingPreferenceId: existing ? existing.id as number : null
            };
        });

        this.channelPreferences.sort((a, b) => a.priority - b.priority);
        this.channelPreferences.forEach((cp, idx) => {
            cp.priority = idx + 1;
        });
    }

    private createDefaultChannelPreferences(): void {
        this.channelPreferences = this.channelTypes
            .sort((a, b) => (a.defaultPriority as number) - (b.defaultPriority as number))
            .map((ct, idx) => ({
                channelTypeId: ct.id as number,
                channelName: ct.name,
                channelDescription: ct.description || '',
                icon: this.channelIcons[ct.name] || 'bi-bell-fill',
                isEnabled: true,
                priority: idx + 1,
                defaultPriority: ct.defaultPriority as number,
                existingPreferenceId: null
            }));
    }

    //
    // Editor Actions
    //
    onChannelDrop(event: CdkDragDrop<EditableChannelPreference[]>): void {
        moveItemInArray(this.channelPreferences, event.previousIndex, event.currentIndex);
        this.channelPreferences.forEach((cp, idx) => {
            cp.priority = idx + 1;
        });
        this.markAsModified();
    }

    toggleChannel(channel: EditableChannelPreference): void {
        channel.isEnabled = !channel.isEnabled;
        this.markAsModified();
    }

    toggleQuietHours(): void {
        this.formQuietHoursEnabled = !this.formQuietHoursEnabled;
        this.markAsModified();
    }

    toggleDnd(): void {
        this.formDoNotDisturb = !this.formDoNotDisturb;
        if (!this.formDoNotDisturb) {
            this.formDoNotDisturbPermanent = false;
            this.formDoNotDisturbUntil = null;
        }
        this.markAsModified();
    }

    toggleDndPermanent(): void {
        this.formDoNotDisturbPermanent = !this.formDoNotDisturbPermanent;
        if (this.formDoNotDisturbPermanent) {
            this.formDoNotDisturbUntil = null;
        }
        this.markAsModified();
    }

    markAsModified(): void {
        this.hasUnsavedChanges = true;
    }

    //
    // Save
    //
    async save(): Promise<void> {
        if (!this.selectedUser) return;

        this.isSaving = true;
        this.errorMessage = null;

        try {
            const preferenceData = this.buildPreferenceSubmitData();

            if (this.isNewPreference) {
                const created = await this.preferenceService.PostUserNotificationPreference(preferenceData).toPromise();
                this.preference = created ?? null;
                this.isNewPreference = false;

                // Update user status in list
                this.selectedUser.hasPreferences = true;
                this.selectedUser.preferenceId = this.preference?.id as number ?? null;
            } else if (this.preference) {
                preferenceData.id = this.preference.id;
                preferenceData.versionNumber = this.preference.versionNumber;
                await this.preferenceService.PutUserNotificationPreference(this.preference.id, preferenceData).toPromise();
            }

            await this.saveChannelPreferences();

            this.hasUnsavedChanges = false;
            this.alertService.showSuccessMessage(`Preferences saved for ${this.selectedUser.displayName}`, null);

            // Reload to get fresh data
            this.loadUserPreferences(this.selectedUser);
        } catch (err: any) {
            console.error('Error saving preferences:', err);
            this.errorMessage = err?.message || 'Failed to save notification preferences';
        } finally {
            this.isSaving = false;
        }
    }

    private buildPreferenceSubmitData(): UserNotificationPreferenceSubmitData {
        const data = new UserNotificationPreferenceSubmitData();
        data.id = this.preference?.id ?? 0;
        data.securityUserObjectGuid = this.selectedUser?.objectGuid || '';
        data.timeZoneId = this.formTimeZoneId;
        data.quietHoursStart = this.formQuietHoursEnabled ? this.formQuietHoursStart : null;
        data.quietHoursEnd = this.formQuietHoursEnabled ? this.formQuietHoursEnd : null;
        data.isDoNotDisturb = this.formDoNotDisturb;
        data.isDoNotDisturbPermanent = this.formDoNotDisturbPermanent;
        data.doNotDisturbUntil = this.formDoNotDisturbUntil;
        data.customSettingsJson = null;
        data.versionNumber = this.preference?.versionNumber ?? 1;
        data.active = true;
        data.deleted = false;

        return data;
    }

    private async saveChannelPreferences(): Promise<void> {
        if (!this.preference) return;

        for (const channel of this.channelPreferences) {
            const submitData = new UserNotificationChannelPreferenceSubmitData();
            submitData.userNotificationPreferenceId = this.preference.id;
            submitData.notificationChannelTypeId = channel.channelTypeId;
            submitData.isEnabled = channel.isEnabled;
            submitData.priorityOverride = channel.priority;
            submitData.active = true;
            submitData.deleted = false;

            if (channel.existingPreferenceId) {
                submitData.id = channel.existingPreferenceId;
                const existing = await this.channelPreferenceService.GetUserNotificationChannelPreference(channel.existingPreferenceId).toPromise();
                submitData.versionNumber = existing?.versionNumber ?? 1;
                await this.channelPreferenceService.PutUserNotificationChannelPreference(channel.existingPreferenceId, submitData).toPromise();
            } else {
                submitData.id = 0;
                submitData.versionNumber = 1;
                const created = await this.channelPreferenceService.PostUserNotificationChannelPreference(submitData).toPromise();
                channel.existingPreferenceId = created?.id as number ?? null;
            }
        }
    }

    //
    // Reset to Defaults
    //
    async resetToDefaults(): Promise<void> {
        if (!this.selectedUser) return;

        if (!confirm(`Reset notification preferences for ${this.selectedUser.displayName} to system defaults?`)) {
            return;
        }

        // Just reset the form fields and channel preferences to defaults
        this.resetFormToDefaults();
        this.createDefaultChannelPreferences();
        this.markAsModified();
    }

    //
    // Navigation
    //
    canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    goBack(): void {
        this.navigationService.goBack();
    }

    //
    // Helpers
    //
    hasEnabledChannels(): boolean {
        return this.channelPreferences.some(c => c.isEnabled);
    }

    getEnabledChannelCount(): number {
        return this.channelPreferences.filter(c => c.isEnabled).length;
    }

    formatDndUntil(): string {
        if (!this.formDoNotDisturbUntil) return '';
        try {
            return new Date(this.formDoNotDisturbUntil).toLocaleString();
        } catch {
            return this.formDoNotDisturbUntil;
        }
    }

    getConfiguredCount(): number {
        return this.users.filter(u => u.hasPreferences).length;
    }
}

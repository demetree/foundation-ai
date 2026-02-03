import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

import { UserNotificationPreferenceService, UserNotificationPreferenceData, UserNotificationPreferenceSubmitData } from '../../alerting-data-services/user-notification-preference.service';
import { UserNotificationChannelPreferenceService, UserNotificationChannelPreferenceData, UserNotificationChannelPreferenceSubmitData } from '../../alerting-data-services/user-notification-channel-preference.service';
import { NotificationChannelTypeService, NotificationChannelTypeData } from '../../alerting-data-services/notification-channel-type.service';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';

/**
 * Editable channel preference model for UI
 */
export interface EditableChannelPreference {
    channelTypeId: number;
    channelName: string;
    channelDescription: string;
    icon: string;
    isEnabled: boolean;
    priority: number;
    defaultPriority: number;
    existingPreferenceId: number | null;
}

@Component({
    selector: 'app-notification-preferences-editor',
    templateUrl: './notification-preferences-editor.component.html',
    styleUrls: ['./notification-preferences-editor.component.scss']
})
export class NotificationPreferencesEditorComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Loading states
    isLoading = true;
    isSaving = false;
    errorMessage: string | null = null;

    // User preference data
    preference: UserNotificationPreferenceData | null = null;
    isNewPreference = false;

    // Channel preferences (ordered by priority)
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
        'MobilePush': 'bi-phone-fill'
    };

    constructor(
        private router: Router,
        private preferenceService: UserNotificationPreferenceService,
        private channelPreferenceService: UserNotificationChannelPreferenceService,
        private channelTypeService: NotificationChannelTypeService,
        private authService: AuthService,
        private alertService: AlertService
    ) { }

    ngOnInit(): void {
        this.loadData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Load all necessary data
     */
    loadData(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const currentUserGuid = this.authService.currentUser?.id;
        if (!currentUserGuid) {
            this.errorMessage = 'Unable to determine current user';
            this.isLoading = false;
            return;
        }

        // Load channel types and user's preference in parallel
        forkJoin({
            channelTypes: this.channelTypeService.GetNotificationChannelTypeList({ active: true, deleted: false }),
            preferences: this.preferenceService.GetUserNotificationPreferenceList({
                securityUserObjectGuid: currentUserGuid,
                active: true,
                deleted: false
            })
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: ({ channelTypes, preferences }) => {
                    this.channelTypes = channelTypes;

                    if (preferences.length > 0) {
                        // Existing preference found
                        this.preference = preferences[0];
                        this.isNewPreference = false;
                        this.populateFormFromPreference();
                        this.loadChannelPreferences();
                    } else {
                        // No preference exists - create default
                        this.isNewPreference = true;
                        this.createDefaultChannelPreferences();
                        this.isLoading = false;
                    }
                },
                error: (err) => {
                    console.error('Error loading data:', err);
                    this.errorMessage = 'Failed to load notification preferences';
                    this.isLoading = false;
                }
            });
    }

    /**
     * Populate form fields from existing preference
     */
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

    /**
     * Load existing channel preferences
     */
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
                    this.isLoading = false;
                },
                error: (err) => {
                    console.error('Error loading channel preferences:', err);
                    this.createDefaultChannelPreferences();
                    this.isLoading = false;
                }
            });
    }

    /**
     * Build editable channel preferences list, merging existing with defaults
     */
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

        // Sort by priority (lower = higher priority)
        this.channelPreferences.sort((a, b) => a.priority - b.priority);

        // Assign sequential priorities for cleaner display
        this.channelPreferences.forEach((cp, idx) => {
            cp.priority = idx + 1;
        });
    }

    /**
     * Create default channel preferences for new users
     */
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

    /**
     * Handle channel card drag and drop
     */
    onChannelDrop(event: CdkDragDrop<EditableChannelPreference[]>): void {
        moveItemInArray(this.channelPreferences, event.previousIndex, event.currentIndex);

        // Update priorities after reorder
        this.channelPreferences.forEach((cp, idx) => {
            cp.priority = idx + 1;
        });

        this.markAsModified();
    }

    /**
     * Toggle channel enabled state
     */
    toggleChannel(channel: EditableChannelPreference): void {
        channel.isEnabled = !channel.isEnabled;
        this.markAsModified();
    }

    /**
     * Toggle quiet hours
     */
    toggleQuietHours(): void {
        this.formQuietHoursEnabled = !this.formQuietHoursEnabled;
        this.markAsModified();
    }

    /**
     * Toggle Do Not Disturb
     */
    toggleDnd(): void {
        this.formDoNotDisturb = !this.formDoNotDisturb;
        if (!this.formDoNotDisturb) {
            this.formDoNotDisturbPermanent = false;
            this.formDoNotDisturbUntil = null;
        }
        this.markAsModified();
    }

    /**
     * Toggle permanent DND
     */
    toggleDndPermanent(): void {
        this.formDoNotDisturbPermanent = !this.formDoNotDisturbPermanent;
        if (this.formDoNotDisturbPermanent) {
            this.formDoNotDisturbUntil = null;
        }
        this.markAsModified();
    }

    /**
     * Mark form as having unsaved changes
     */
    markAsModified(): void {
        this.hasUnsavedChanges = true;
    }

    /**
     * Save all preferences
     */
    async save(): Promise<void> {
        this.isSaving = true;
        this.errorMessage = null;

        try {
            // Save or create main preference
            const preferenceData = this.buildPreferenceSubmitData();

            if (this.isNewPreference) {
                const created = await this.preferenceService.PostUserNotificationPreference(preferenceData).toPromise();
                this.preference = created ?? null;
                this.isNewPreference = false;
            } else if (this.preference) {
                preferenceData.id = this.preference.id;
                preferenceData.versionNumber = this.preference.versionNumber;
                await this.preferenceService.PutUserNotificationPreference(this.preference.id, preferenceData).toPromise();
            }

            // Save channel preferences
            await this.saveChannelPreferences();

            this.hasUnsavedChanges = false;
            this.alertService.showSuccessMessage('Notification preferences saved successfully', null);

            // Reload to get fresh data
            this.loadData();
        } catch (err: any) {
            console.error('Error saving preferences:', err);
            this.errorMessage = err?.message || 'Failed to save notification preferences';
        } finally {
            this.isSaving = false;
        }
    }

    /**
     * Build submit data for main preference
     */
    private buildPreferenceSubmitData(): UserNotificationPreferenceSubmitData {
        const currentUserGuid = this.authService.currentUser?.id || '';

        const data = new UserNotificationPreferenceSubmitData();
        data.id = this.preference?.id ?? 0;
        data.securityUserObjectGuid = currentUserGuid;
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

    /**
     * Save channel preferences using relational sync pattern
     */
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
                // Update existing
                submitData.id = channel.existingPreferenceId;
                // Need to get version number
                const existing = await this.channelPreferenceService.GetUserNotificationChannelPreference(channel.existingPreferenceId).toPromise();
                submitData.versionNumber = existing?.versionNumber ?? 1;
                await this.channelPreferenceService.PutUserNotificationChannelPreference(channel.existingPreferenceId, submitData).toPromise();
            } else {
                // Create new
                submitData.id = 0;
                submitData.versionNumber = 1;
                const created = await this.channelPreferenceService.PostUserNotificationChannelPreference(submitData).toPromise();
                channel.existingPreferenceId = created?.id as number ?? null;
            }
        }
    }

    /**
     * Navigate back to previous page
     */
    goBack(): void {
        this.router.navigate(['/']);
    }

    /**
     * Check if there are any enabled channels
     */
    hasEnabledChannels(): boolean {
        return this.channelPreferences.some(c => c.isEnabled);
    }

    /**
     * Get the first enabled channel name for preview
     */
    getFirstEnabledChannel(): string {
        const first = this.channelPreferences.find(c => c.isEnabled);
        return first ? first.channelName : 'None';
    }

    /**
     * Get enabled channels count
     */
    getEnabledChannelCount(): number {
        return this.channelPreferences.filter(c => c.isEnabled).length;
    }

    /**
     * Format DND until time for display
     */
    formatDndUntil(): string {
        if (!this.formDoNotDisturbUntil) return '';
        try {
            return new Date(this.formDoNotDisturbUntil).toLocaleString();
        } catch {
            return this.formDoNotDisturbUntil;
        }
    }

    /**
     * UnsavedChangesGuard support
     */
    canDeactivate(): boolean {
        if (this.hasUnsavedChanges) {
            return confirm('You have unsaved changes. Are you sure you want to leave?');
        }
        return true;
    }
}

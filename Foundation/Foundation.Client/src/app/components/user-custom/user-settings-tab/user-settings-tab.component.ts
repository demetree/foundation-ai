//
// User Settings Tab Component
//
// Displays and manages user-specific settings stored in SecurityUser.settings.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityUserData } from '../../../security-data-services/security-user.service';
import { UserSettingsService } from '../../../services/user-settings.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

export interface SettingItem {
    key: string;
    value: string;
    isEditing?: boolean;
}

@Component({
    selector: 'app-user-settings-tab',
    templateUrl: './user-settings-tab.component.html',
    styleUrls: ['./user-settings-tab.component.scss']
})
export class UserSettingsTabComponent implements OnInit, OnChanges {

    @Input() user: SecurityUserData | null = null;

    settings: SettingItem[] = [];
    isLoading = false;
    isSaving = false;

    // For add/edit modal
    showModal = false;
    modalMode: 'add' | 'edit' = 'add';
    editingKey = '';
    editingValue = '';
    originalKey = '';

    constructor(
        private userSettingsService: UserSettingsService,
        private alertService: AlertService,
        private modalService: NgbModal
    ) { }


    ngOnInit(): void {
        this.loadSettings();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['user'] && !changes['user'].firstChange) {
            this.loadSettings();
        }
    }


    /**
     * Load all settings for the user
     */
    loadSettings(): void {
        if (!this.user) return;

        this.isLoading = true;
        this.userSettingsService.getAllSettings().subscribe({
            next: (data) => {
                this.settings = Object.entries(data)
                    .filter(([key, value]) => value !== null)
                    .map(([key, value]) => ({
                        key,
                        value: typeof value === 'string' ? value : JSON.stringify(value)
                    }))
                    .sort((a, b) => a.key.localeCompare(b.key));
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading settings:', err);
                this.alertService.showMessage('Error', 'Failed to load settings', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    /**
     * Open modal to add a new setting
     */
    openAddModal(): void {
        this.modalMode = 'add';
        this.editingKey = '';
        this.editingValue = '';
        this.originalKey = '';
        this.showModal = true;
    }


    /**
     * Open modal to edit an existing setting
     */
    openEditModal(setting: SettingItem): void {
        this.modalMode = 'edit';
        this.editingKey = setting.key;
        this.editingValue = setting.value;
        this.originalKey = setting.key;
        this.showModal = true;
    }


    /**
     * Close the modal
     */
    closeModal(): void {
        this.showModal = false;
        this.editingKey = '';
        this.editingValue = '';
        this.originalKey = '';
    }


    /**
     * Save the setting (add or update)
     */
    saveSetting(): void {
        if (!this.editingKey.trim()) {
            this.alertService.showMessage('Validation Error', 'Setting key cannot be empty', MessageSeverity.warn);
            return;
        }

        // Check for duplicate key when adding
        if (this.modalMode === 'add' && this.settings.some(s => s.key === this.editingKey)) {
            this.alertService.showMessage('Validation Error', 'A setting with this key already exists', MessageSeverity.warn);
            return;
        }

        this.isSaving = true;

        // If we're editing and the key changed, we need to delete the old key first
        if (this.modalMode === 'edit' && this.originalKey !== this.editingKey) {
            this.userSettingsService.deleteSetting(this.originalKey).subscribe({
                next: () => {
                    this.saveNewSetting();
                },
                error: () => {
                    this.alertService.showMessage('Error', 'Failed to update setting key', MessageSeverity.error);
                    this.isSaving = false;
                }
            });
        } else {
            this.saveNewSetting();
        }
    }


    private saveNewSetting(): void {
        this.userSettingsService.setSetting(this.editingKey, this.editingValue).subscribe({
            next: (success) => {
                if (success) {
                    this.alertService.showMessage('Success', 'Setting saved successfully', MessageSeverity.success);
                    this.closeModal();
                    this.loadSettings();
                } else {
                    this.alertService.showMessage('Error', 'Failed to save setting', MessageSeverity.error);
                }
                this.isSaving = false;
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to save setting', MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }


    /**
     * Delete a setting
     */
    deleteSetting(setting: SettingItem): void {
        if (!confirm(`Are you sure you want to delete the setting "${setting.key}"?`)) {
            return;
        }

        this.userSettingsService.deleteSetting(setting.key).subscribe({
            next: (success) => {
                if (success) {
                    this.alertService.showMessage('Success', 'Setting deleted successfully', MessageSeverity.success);
                    this.loadSettings();
                } else {
                    this.alertService.showMessage('Error', 'Failed to delete setting', MessageSeverity.error);
                }
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to delete setting', MessageSeverity.error);
            }
        });
    }


    /**
     * Truncate long values for display
     */
    truncateValue(value: string, maxLength: number = 50): string {
        if (value.length <= maxLength) return value;
        return value.substring(0, maxLength) + '...';
    }
}

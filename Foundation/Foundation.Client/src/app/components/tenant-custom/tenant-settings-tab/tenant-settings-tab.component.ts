//
// Tenant Settings Tab Component
//
// Displays and manages tenant-level settings stored in SecurityTenant.settings.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { TenantSettingsService } from '../../../services/tenant-settings.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

export interface TenantSettingItem {
    key: string;
    value: string;
}

@Component({
    selector: 'app-tenant-settings-tab',
    templateUrl: './tenant-settings-tab.component.html',
    styleUrls: ['./tenant-settings-tab.component.scss']
})
export class TenantSettingsTabComponent implements OnInit, OnChanges {

    @Input() tenant: SecurityTenantData | null = null;

    settings: TenantSettingItem[] = [];
    isLoading = false;
    isSaving = false;

    // For add/edit modal
    showModal = false;
    modalMode: 'add' | 'edit' = 'add';
    editingKey = '';
    editingValue = '';
    originalKey = '';

    constructor(
        private tenantSettingsService: TenantSettingsService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.loadSettings();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['tenant'] && !changes['tenant'].firstChange) {
            this.loadSettings();
        }
    }


    /**
     * Load all settings for the tenant
     */
    loadSettings(): void {
        if (!this.tenant) return;

        this.isLoading = true;
        this.tenantSettingsService.getAllSettings(Number(this.tenant.id)).subscribe({
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
                console.error('Error loading tenant settings:', err);
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
    openEditModal(setting: TenantSettingItem): void {
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
        if (!this.tenant) return;

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
            this.tenantSettingsService.deleteSetting(Number(this.tenant.id), this.originalKey).subscribe({
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
        if (!this.tenant) return;

        this.tenantSettingsService.setSetting(Number(this.tenant.id), this.editingKey, this.editingValue).subscribe({
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
    deleteSetting(setting: TenantSettingItem): void {
        if (!this.tenant) return;

        if (!confirm(`Are you sure you want to delete the setting "${setting.key}"?`)) {
            return;
        }

        this.tenantSettingsService.deleteSetting(Number(this.tenant.id), setting.key).subscribe({
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

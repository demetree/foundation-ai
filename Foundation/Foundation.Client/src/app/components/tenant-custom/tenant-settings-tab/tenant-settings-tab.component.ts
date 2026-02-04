//
// Tenant Settings Tab Component
//
// Displays and manages tenant-level settings stored in SecurityTenant.settings.
// Uses TenantSettings API endpoints with tenantId to read and write settings.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { TenantSettingsService } from '../../../services/tenant-settings.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

export interface SettingItem {
    key: string;
    value: string;
    isEditing?: boolean;
}

@Component({
    selector: 'app-tenant-settings-tab',
    templateUrl: './tenant-settings-tab.component.html',
    styleUrls: ['./tenant-settings-tab.component.scss']
})
export class TenantSettingsTabComponent implements OnInit, OnChanges {

    @Input() tenant: SecurityTenantData | null = null;

    settings: SettingItem[] = [];
    isLoading = false;
    isSaving = false;

    // Modal state
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
     * Load all settings for the tenant via API
     */
    loadSettings(): void {
        if (!this.tenant) {
            this.settings = [];
            return;
        }

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
                console.error('Error loading settings:', err);
                this.alertService.showMessage('Error', 'Failed to load settings', MessageSeverity.error);
                this.isLoading = false;
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


    /**
     * Open modal for adding a new setting
     */
    openAddModal(): void {
        this.modalMode = 'add';
        this.editingKey = '';
        this.editingValue = '';
        this.originalKey = '';
        this.showModal = true;
    }


    /**
     * Open modal for editing an existing setting
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
        if (!this.tenant || !this.editingKey.trim()) {
            this.alertService.showMessage('Error', 'Setting key is required', MessageSeverity.error);
            return;
        }

        this.isSaving = true;
        const tenantId = Number(this.tenant.id);

        // If editing and key changed, delete old key first
        if (this.modalMode === 'edit' && this.originalKey !== this.editingKey) {
            this.tenantSettingsService.deleteSetting(tenantId, this.originalKey).subscribe({
                next: () => {
                    this.saveNewSetting();
                },
                error: (err) => {
                    console.error('Error deleting old key:', err);
                    this.isSaving = false;
                    this.alertService.showMessage('Error', 'Failed to update setting key', MessageSeverity.error);
                }
            });
        } else {
            this.saveNewSetting();
        }
    }


    private saveNewSetting(): void {
        if (!this.tenant) return;

        const tenantId = Number(this.tenant.id);

        this.tenantSettingsService.setSetting(tenantId, this.editingKey.trim(), this.editingValue).subscribe({
            next: (success) => {
                this.isSaving = false;
                if (success) {
                    this.alertService.showMessage('Success', 'Setting saved', MessageSeverity.success);
                    this.closeModal();
                    this.loadSettings();
                } else {
                    this.alertService.showMessage('Error', 'Failed to save setting', MessageSeverity.error);
                }
            },
            error: (err) => {
                this.isSaving = false;
                console.error('Error saving setting:', err);
                this.alertService.showMessage('Error', 'Failed to save setting', MessageSeverity.error);
            }
        });
    }


    /**
     * Delete a setting
     */
    deleteSetting(setting: SettingItem): void {
        if (!this.tenant) return;

        if (!confirm(`Delete setting "${setting.key}"?`)) {
            return;
        }

        const tenantId = Number(this.tenant.id);

        this.tenantSettingsService.deleteSetting(tenantId, setting.key).subscribe({
            next: (success) => {
                if (success) {
                    this.alertService.showMessage('Success', 'Setting deleted', MessageSeverity.success);
                    this.loadSettings();
                } else {
                    this.alertService.showMessage('Error', 'Failed to delete setting', MessageSeverity.error);
                }
            },
            error: (err) => {
                console.error('Error deleting setting:', err);
                this.alertService.showMessage('Error', 'Failed to delete setting', MessageSeverity.error);
            }
        });
    }
}

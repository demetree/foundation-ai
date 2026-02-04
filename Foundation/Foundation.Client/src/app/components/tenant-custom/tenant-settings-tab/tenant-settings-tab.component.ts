//
// Tenant Settings Tab Component
//
// Displays tenant-level settings stored in SecurityTenant.settings.
// For admin viewing: parses settings directly from the tenant object.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

export interface SettingItem {
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

    settings: SettingItem[] = [];
    isLoading = false;

    constructor(
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
     * Load all settings for the tenant by parsing the settings JSON from the tenant object
     */
    loadSettings(): void {
        if (!this.tenant) {
            this.settings = [];
            return;
        }

        this.isLoading = true;

        try {
            // Parse settings directly from the tenant object's settings JSON field
            if (this.tenant.settings && this.tenant.settings.trim()) {
                const parsedSettings = JSON.parse(this.tenant.settings);
                this.settings = Object.entries(parsedSettings)
                    .filter(([key, value]) => value !== null)
                    .map(([key, value]) => ({
                        key,
                        value: typeof value === 'string' ? value : JSON.stringify(value)
                    }))
                    .sort((a, b) => a.key.localeCompare(b.key));
            } else {
                this.settings = [];
            }
        } catch (error) {
            console.error('Error parsing tenant settings:', error);
            this.alertService.showMessage('Error', 'Failed to parse settings', MessageSeverity.error);
            this.settings = [];
        }

        this.isLoading = false;
    }


    /**
     * Truncate long values for display
     */
    truncateValue(value: string, maxLength: number = 50): string {
        if (value.length <= maxLength) return value;
        return value.substring(0, maxLength) + '...';
    }
}

//
// User Settings Tab Component
//
// Displays and manages user-specific settings stored in SecurityUser.settings.
// For admin viewing: parses settings directly from the user object.
// For mutations: uses the UserSettings API (currently only supports current user).
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityUserData } from '../../../security-data-services/security-user.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

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

    constructor(
        private alertService: AlertService
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
     * Load all settings for the user by parsing the settings JSON from the user object
     */
    loadSettings(): void {
        if (!this.user) {
            this.settings = [];
            return;
        }

        this.isLoading = true;

        try {
            // Parse settings directly from the user object's settings JSON field
            if (this.user.settings && this.user.settings.trim()) {
                const parsedSettings = JSON.parse(this.user.settings);
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
            console.error('Error parsing user settings:', error);
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

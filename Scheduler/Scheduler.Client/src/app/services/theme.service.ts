//
// Theme Service
//
// AI-DEVELOPED: Manages the active UI theme for the Scheduler application.
//
// Uses a two-tier resolution approach:
// 1. Tenant-level default — read from TenantSettings via TenantThemeService
// 2. User-level override — read/written via UserSettingsService
//
// Applies a [data-theme] attribute on <body> that activates the matching
// CSS custom property block from _themes.scss.
//
// Uses localStorage for instant theme application on page load (no flash of
// unstyled content). Server-side settings are the source of truth, synced
// in the background after login.
//

import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { UserSettingsService } from './user-settings.service';
import { TenantThemeService } from './tenant-theme.service';


/**
 * Theme definition used by the theme picker UI.
 */
export interface ThemeDefinition {
    id: string;
    label: string;
    icon: string;
    type: 'light' | 'dark';
    swatchColors: string[];
}


/**
 * ThemeService — manages the active UI theme for the Scheduler.
 *
 * On startup, applies the last theme from localStorage immediately.
 * After login, resolves the correct theme via tenant default + user override.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {

    private static readonly STORAGE_KEY = 'scheduler-theme';
    private static readonly SETTINGS_KEY = 'scheduler-theme';
    private static readonly DEFAULT_THEME = 'default';

    private themeSubject = new BehaviorSubject<string>(ThemeService.DEFAULT_THEME);


    /** Observable of the current theme id. */
    public currentTheme$ = this.themeSubject.asObservable();


    /** Snapshot of the current theme id. */
    public get currentTheme(): string {
        return this.themeSubject.value;
    }


    /** All available themes for the theme picker. */
    public readonly availableThemes: ThemeDefinition[] = [
        {
            id: 'default',
            label: 'Default',
            icon: 'bi bi-sun',
            type: 'light',
            swatchColors: ['#2563eb', '#60a5fa', '#f1f5f9']
        },
        {
            id: 'warm',
            label: 'Warm',
            icon: 'bi bi-sunrise',
            type: 'light',
            swatchColors: ['#d97706', '#fbbf24', '#faf5ee']
        },
        {
            id: 'midnight',
            label: 'Midnight',
            icon: 'bi bi-moon-stars',
            type: 'dark',
            swatchColors: ['#60a5fa', '#818cf8', '#0b1120']
        },
        {
            id: 'slate',
            label: 'Slate',
            icon: 'bi bi-moon',
            type: 'dark',
            swatchColors: ['#2dd4bf', '#94a3b8', '#0f1419']
        },
        {
            id: 'ocean',
            label: 'Ocean',
            icon: 'bi bi-water',
            type: 'dark',
            swatchColors: ['#06b6d4', '#3b82f6', '#061018']
        }
    ];


    constructor(
        private userSettingsService: UserSettingsService,
        private tenantThemeService: TenantThemeService
    ) {

        //
        // Restore the previously saved theme from localStorage immediately.
        // This prevents a flash of unstyled content before the server responds.
        //
        let saved = localStorage.getItem(ThemeService.STORAGE_KEY);
        let initial = ThemeService.DEFAULT_THEME;

        if (saved !== null && this.isValidTheme(saved) === true) {
            initial = saved;
        }

        this.applyTheme(initial);
    }


    /**
     * Initialize the theme after login.
     *
     * Resolves the correct theme using this priority:
     * 1. User-level override (from UserSettings)
     * 2. Tenant-level default (from TenantSettings)
     * 3. Built-in default ('default')
     */
    public initializeAfterLogin(): void {

        //
        // First, try to get the user's personal theme preference
        //
        this.userSettingsService.getStringSetting(ThemeService.SETTINGS_KEY).subscribe(userTheme => {

            if (userTheme !== null && this.isValidTheme(userTheme) === true) {

                //
                // User has a personal theme preference — use it
                //
                this.applyTheme(userTheme);
                localStorage.setItem(ThemeService.STORAGE_KEY, userTheme);

                return;
            }

            //
            // No user preference — try the tenant default
            //
            this.tenantThemeService.getTenantDefaultTheme().subscribe(tenantTheme => {

                if (tenantTheme !== null && this.isValidTheme(tenantTheme) === true) {

                    //
                    // Tenant has a default theme — use it
                    //
                    this.applyTheme(tenantTheme);
                    localStorage.setItem(ThemeService.STORAGE_KEY, tenantTheme);
                }

                //
                // If neither user nor tenant have a theme, the constructor's localStorage
                // restore or DEFAULT_THEME is already applied — nothing more to do
                //
            });
        });
    }


    /**
     * Switch to the given theme.
     *
     * Updates the DOM, emits to subscribers, persists to localStorage,
     * and saves to the user's server-side settings.
     */
    public setTheme(themeId: string): void {

        if (this.isValidTheme(themeId) === false) {
            return;
        }

        //
        // Apply the theme immediately for instant visual feedback
        //
        this.applyTheme(themeId);
        localStorage.setItem(ThemeService.STORAGE_KEY, themeId);

        //
        // Persist to the server in the background
        //
        this.userSettingsService.setStringSetting(ThemeService.SETTINGS_KEY, themeId).subscribe();
    }


    /**
     * Check if the current theme is a dark theme.
     */
    public get isDarkTheme(): boolean {

        let currentDefinition = this.availableThemes.find(t => t.id === this.currentTheme);

        if (currentDefinition != null) {
            return currentDefinition.type === 'dark';
        }

        return false;
    }


    // ── Private ──────────────────────────────────────────────────────────


    /**
     * Apply the given theme by setting the data-theme attribute on the body element.
     */
    private applyTheme(themeId: string): void {
        document.body.setAttribute('data-theme', themeId);
        this.themeSubject.next(themeId);
    }


    /**
     * Check if the given theme id corresponds to a valid available theme.
     */
    private isValidTheme(themeId: string): boolean {
        return this.availableThemes.some(t => t.id === themeId);
    }
}

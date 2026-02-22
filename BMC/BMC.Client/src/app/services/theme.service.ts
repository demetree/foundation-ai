// ======================================
// AI-DEVELOPED: Theme service ported from BasecampDataCollector.client and adapted for BMC.
// Manages the active UI theme via [data-theme] attribute on <body>.
// Persists the user's choice in localStorage.
// ======================================

import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';


/**
 * Theme definition used by the theme picker UI.
 */
export interface ThemeDefinition {
    id: string;
    label: string;
    icon: string;
    swatchColors: string[];
}


/**
 * ThemeService — manages the active UI theme for BMC.
 *
 * Applies a [data-theme] attribute on <body> that activates the
 * matching CSS custom property block from _themes.scss.
 * Persists the user's choice in localStorage.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {

    private static readonly STORAGE_KEY = 'bmc-theme';
    private static readonly DEFAULT_THEME = 'brick-builder';

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
            id: 'brick-builder',
            label: 'Brick Builder',
            icon: 'fas fa-shapes',
            swatchColors: ['#E3000B', '#006CB7', '#FFCD03']
        },
        {
            id: 'miami-vice',
            label: 'Miami Vice',
            icon: 'fas fa-sun',
            swatchColors: ['#ff6ec7', '#ff8a00', '#1a0a2e']
        },
        {
            id: 'matrix',
            label: 'Matrix',
            icon: 'fas fa-terminal',
            swatchColors: ['#00ff41', '#00cc33', '#0a0a0a']
        },
        {
            id: 'cyberpunk',
            label: 'Cyberpunk',
            icon: 'fas fa-bolt',
            swatchColors: ['#fcee09', '#ff003c', '#121212']
        },
        {
            id: 'retro-arcade',
            label: 'Retro Arcade',
            icon: 'fas fa-gamepad',
            swatchColors: ['#ff00ff', '#00e5ff', '#050505']
        },
        {
            id: 'default',
            label: 'BMC',
            icon: 'fas fa-cubes',
            swatchColors: ['#ffa726', '#ff8f00', '#0f0a04']
        },
        {
            id: 'blueprint',
            label: 'Blueprint',
            icon: 'fas fa-drafting-compass',
            swatchColors: ['#4fc3f7', '#b0bec5', '#0a1628']
        },
        {
            id: 'pastel-dreams',
            label: 'Pastel Dreams',
            icon: 'fas fa-moon',
            swatchColors: ['#b388ff', '#f48fb1', '#1a1025']
        },
        {
            id: 'blueprint-light',
            label: 'Blueprint Light',
            icon: 'fas fa-ruler-combined',
            swatchColors: ['#0288d1', '#546e7a', '#eceff1']
        },
        {
            id: 'pastel-light',
            label: 'Pastel Light',
            icon: 'fas fa-star',
            swatchColors: ['#7c4dff', '#ec407a', '#f3eff8']
        }
    ];


    constructor() {

        //
        // Restore the previously saved theme from localStorage, or fall back to default.
        //
        let saved = localStorage.getItem(ThemeService.STORAGE_KEY);
        let initial = ThemeService.DEFAULT_THEME;

        if (saved !== null && this.isValidTheme(saved) === true) {
            initial = saved;
        }

        this.applyTheme(initial);
    }


    /**
     * Switch to the given theme.
     * Updates the DOM, emits to subscribers, and persists to localStorage.
     */
    public setTheme(themeId: string): void {

        if (this.isValidTheme(themeId) === false) {
            return;
        }

        this.applyTheme(themeId);
        localStorage.setItem(ThemeService.STORAGE_KEY, themeId);
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

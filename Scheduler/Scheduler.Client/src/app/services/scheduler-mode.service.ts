import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { UserSettingsService } from './user-settings.service';


//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Hierarchical Simple/Advanced mode service for the Scheduler UI.
//
// Provides a global mode toggle plus per-component overrides, so a user can
// stay in "simple" mode overall but unlock advanced features for specific
// areas (e.g., the event editor) as they grow into needing them.
//
// Mode state is persisted per-user via UserSettingsService.
//


//
// Known component keys for entity-level overrides
//
export type SchedulerModeComponentKey = 'eventEditor' | 'sidebar' | 'overview' | 'recurrence';


//
// Mode values
//
export type SchedulerMode = 'simple' | 'advanced';


//
// Setting key constants
//
const GLOBAL_MODE_KEY = 'scheduler.uiMode';
const COMPONENT_KEY_PREFIX = 'scheduler.uiMode.';
const DEFAULT_MODE: SchedulerMode = 'simple';


@Injectable({
    providedIn: 'root'
})
export class SchedulerModeService implements OnDestroy {

    //
    // Global mode subject — defaults to 'simple'
    //
    private _globalMode$ = new BehaviorSubject<SchedulerMode>(DEFAULT_MODE);

    //
    // Per-component overrides — null means "inherit from global"
    //
    private _componentOverrides = new Map<SchedulerModeComponentKey, BehaviorSubject<SchedulerMode | null>>();

    //
    // Cleanup
    //
    private _destroy$ = new Subject<void>();

    //
    // Track whether the initial load has completed
    //
    private _initialized = false;


    constructor(private userSettingsService: UserSettingsService) {

        //
        // Load the global mode from persisted user settings
        //
        this.loadGlobalMode();
    }


    ngOnDestroy(): void {
        this._destroy$.next();
        this._destroy$.complete();
    }


    // ─────────────────────────────────────────────────────────────────
    // Public Observables
    // ─────────────────────────────────────────────────────────────────

    /**
     * Observable of the global mode value.
     */
    public get globalMode$(): Observable<SchedulerMode> {
        return this._globalMode$.asObservable();
    }


    /**
     * Returns an observable boolean indicating if the effective mode is 'simple'
     * for a given component. If no component key is provided, returns the global mode.
     *
     * When a component override exists, it takes precedence over the global mode.
     * When no override exists, the global mode is used.
     */
    public isSimpleMode(componentKey?: SchedulerModeComponentKey): Observable<boolean> {

        if (componentKey != null) {
            return this.getEffectiveMode(componentKey).pipe(
                map(mode => mode === 'simple')
            );
        }

        return this._globalMode$.pipe(
            map(mode => mode === 'simple')
        );
    }


    /**
     * Returns an observable boolean indicating if the effective mode is 'advanced'
     * for a given component. If no component key is provided, returns the global mode.
     */
    public isAdvancedMode(componentKey?: SchedulerModeComponentKey): Observable<boolean> {

        if (componentKey != null) {
            return this.getEffectiveMode(componentKey).pipe(
                map(mode => mode === 'advanced')
            );
        }

        return this._globalMode$.pipe(
            map(mode => mode === 'advanced')
        );
    }


    // ─────────────────────────────────────────────────────────────────
    // Public Synchronous Getters (for component init)
    // ─────────────────────────────────────────────────────────────────

    /**
     * Returns the current global mode value synchronously.
     */
    public get currentGlobalMode(): SchedulerMode {
        return this._globalMode$.value;
    }


    /**
     * Returns true if the current effective mode is 'simple' for the given component.
     */
    public isSimpleModeSync(componentKey?: SchedulerModeComponentKey): boolean {

        if (componentKey != null) {

            let override = this._componentOverrides.get(componentKey);

            if (override != null && override.value != null) {
                return override.value === 'simple';
            }
        }

        return this._globalMode$.value === 'simple';
    }


    // ─────────────────────────────────────────────────────────────────
    // Mode Setters
    // ─────────────────────────────────────────────────────────────────

    /**
     * Sets the global mode and persists it.
     */
    public setGlobalMode(mode: SchedulerMode): void {

        this._globalMode$.next(mode);

        //
        // Persist the global mode to user settings
        //
        this.userSettingsService.setStringSetting(GLOBAL_MODE_KEY, mode)
            .pipe(takeUntil(this._destroy$))
            .subscribe();
    }


    /**
     * Toggles the global mode between simple and advanced.
     */
    public toggleGlobalMode(): void {

        let newMode: SchedulerMode = this._globalMode$.value === 'simple' ? 'advanced' : 'simple';

        this.setGlobalMode(newMode);
    }


    /**
     * Sets a component-level override. Pass null to clear the override
     * and inherit from the global mode.
     */
    public setComponentOverride(componentKey: SchedulerModeComponentKey, mode: SchedulerMode | null): void {

        let subject = this._componentOverrides.get(componentKey);

        if (subject == null) {
            subject = new BehaviorSubject<SchedulerMode | null>(mode);
            this._componentOverrides.set(componentKey, subject);
        } else {
            subject.next(mode);
        }

        //
        // Persist or clear the override
        //
        let settingKey = COMPONENT_KEY_PREFIX + componentKey;

        this.userSettingsService.setStringSetting(settingKey, mode)
            .pipe(takeUntil(this._destroy$))
            .subscribe();
    }


    // ─────────────────────────────────────────────────────────────────
    // Private Helpers
    // ─────────────────────────────────────────────────────────────────

    /**
     * Loads the global mode and any known component overrides from persisted settings.
     */
    private loadGlobalMode(): void {

        this.userSettingsService.getStringSetting(GLOBAL_MODE_KEY)
            .pipe(takeUntil(this._destroy$))
            .subscribe(value => {

                let mode: SchedulerMode = DEFAULT_MODE;

                if (value === 'simple' || value === 'advanced') {
                    mode = value;
                }

                this._globalMode$.next(mode);
                this._initialized = true;
            });
    }


    /**
     * Gets the effective mode for a component, combining the override with the global mode.
     * Returns an observable that reacts to changes in either the override or the global mode.
     */
    private getEffectiveMode(componentKey: SchedulerModeComponentKey): Observable<SchedulerMode> {

        let subject = this._componentOverrides.get(componentKey);

        if (subject == null) {
            subject = new BehaviorSubject<SchedulerMode | null>(null);
            this._componentOverrides.set(componentKey, subject);
        }

        //
        // Capture in a local const so TypeScript knows it's not undefined
        //
        const overrideSubject = subject;

        //
        // Combine the component override with the global mode.
        // If the override is set, use it. Otherwise, fall back to global.
        //
        return new Observable<SchedulerMode>(subscriber => {

            let lastOverride: SchedulerMode | null = overrideSubject.value;
            let lastGlobal: SchedulerMode = this._globalMode$.value;

            //
            // Helper to emit the effective value
            //
            let emit = () => {

                let effective: SchedulerMode = lastOverride != null ? lastOverride : lastGlobal;

                subscriber.next(effective);
            };

            //
            // Subscribe to override changes
            //
            let overrideSub = overrideSubject.subscribe(overrideValue => {
                lastOverride = overrideValue;
                emit();
            });

            //
            // Subscribe to global mode changes
            //
            let globalSub = this._globalMode$.subscribe(globalValue => {
                lastGlobal = globalValue;
                emit();
            });

            //
            // Cleanup
            //
            return () => {
                overrideSub.unsubscribe();
                globalSub.unsubscribe();
            };
        });
    }
}

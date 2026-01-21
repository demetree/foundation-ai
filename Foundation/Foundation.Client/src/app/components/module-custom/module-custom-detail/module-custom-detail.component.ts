//
// Module Custom Detail Component
//
// Premium detail view for managing a single Module with tabbed interface.
// Modeled after user-custom-detail pattern.
//

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ModuleService, ModuleData } from '../../../security-data-services/module.service';
import { AlertService } from '../../../services/alert.service';

@Component({
    selector: 'app-module-custom-detail',
    templateUrl: './module-custom-detail.component.html',
    styleUrls: ['./module-custom-detail.component.scss']
})
export class ModuleCustomDetailComponent implements OnInit, OnDestroy {

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();

    //
    // Data
    //
    public moduleData: ModuleData | null = null;
    public isLoading$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
    public notFound: boolean = false;

    //
    // Tab state
    //
    public activeTab: number = 1;

    //
    // Quick action states
    //
    public isToggling: boolean = false;


    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
        private moduleService: ModuleService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.route.params
            .pipe(takeUntil(this.destroy$))
            .subscribe(params => {
                const id = params['id'];
                if (id) {
                    this.loadModule(Number(id));
                }
            });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Data Loading
    //

    private loadModule(id: number): void {
        this.isLoading$.next(true);
        this.notFound = false;

        this.moduleService.GetModule(id, true)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (module) => {
                    this.moduleData = module;
                    this.isLoading$.next(false);
                },
                error: (err) => {
                    console.error('Error loading module:', err);
                    this.notFound = true;
                    this.isLoading$.next(false);
                }
            });
    }


    //
    // Display Helpers
    //

    getStatusBadgeClass(): string {
        if (!this.moduleData) return '';
        if (this.moduleData.deleted) return 'badge-deleted';
        return this.moduleData.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(): string {
        if (!this.moduleData) return '';
        if (this.moduleData.deleted) return 'Deleted';
        return this.moduleData.active ? 'Active' : 'Inactive';
    }


    //
    // Quick Actions
    //

    toggleActive(): void {
        if (!this.moduleData || this.isToggling) return;

        this.isToggling = true;
        const newActiveState = !this.moduleData.active;

        const submitData = this.moduleData.ConvertToSubmitData();
        submitData.active = newActiveState;

        this.moduleService.PutModule(Number(this.moduleData.id), submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (updated) => {
                    this.moduleData = updated;
                    this.isToggling = false;
                    this.alertService.showSuccessMessage(
                        'Module Updated',
                        `Module is now ${newActiveState ? 'active' : 'inactive'}.`
                    );
                },
                error: (err) => {
                    console.error('Error toggling module active state:', err);
                    this.isToggling = false;
                    this.alertService.showErrorMessage(
                        'Update Failed',
                        'Failed to update module status. Please try again.'
                    );
                }
            });
    }


    //
    // Navigation
    //

    goBack(): void {
        this.location.back();
    }


    canGoBack(): boolean {
        return window.history.length > 1;
    }


    navigateToModules(): void {
        this.router.navigate(['/modules']);
    }


    //
    // Permission Helpers
    //

    canEdit(): boolean {
        return this.moduleService.userIsSecurityModuleWriter();
    }
}

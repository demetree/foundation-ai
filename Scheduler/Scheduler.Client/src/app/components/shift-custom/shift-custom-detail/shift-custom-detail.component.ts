/**
 * ShiftCustomDetailComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Detail page for a single Resource Shift (/resourceshift/:id).
 * Premium header with resource name, day badge, and time info.
 * Two tabs: Overview (read-only fields + edit button) and Change History.
 */
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { ResourceShiftService, ResourceShiftData } from '../../../scheduler-data-services/resource-shift.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ShiftCustomAddEditComponent } from '../shift-custom-add-edit/shift-custom-add-edit.component';
import { Subject, takeUntil } from 'rxjs';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';

@Component({
    selector: 'app-shift-custom-detail',
    templateUrl: './shift-custom-detail.component.html',
    styleUrls: ['./shift-custom-detail.component.scss']
})
export class ShiftCustomDetailComponent implements OnInit, OnDestroy, CanComponentDeactivate {
    @ViewChild(ShiftCustomAddEditComponent) addEditComponent!: ShiftCustomAddEditComponent;

    public shiftId: string | null = null;
    public shift: ResourceShiftData | null = null;
    public isLoading = true;
    public error: string | null = null;
    public activeTab = 'overview';

    // Change history
    public changeHistory: any[] | null = null;
    public isLoadingHistory = false;

    private destroy$ = new Subject<void>();

    // Day helpers
    public readonly dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    public readonly dayAbbrevs = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    public readonly dayColors: { [key: number]: string } = {
        0: '#94a3b8', 1: '#6366f1', 2: '#8b5cf6', 3: '#06b6d4',
        4: '#f59e0b', 5: '#14b8a6', 6: '#f472b6'
    };

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private shiftService: ResourceShiftService,
        private alertService: AlertService,
        private navigationService: NavigationService
    ) { }

    ngOnInit(): void {
        this.route.paramMap
            .pipe(takeUntil(this.destroy$))
            .subscribe(params => {
                this.shiftId = params.get('resourceShiftId');
                if (this.shiftId) {
                    this.loadData();
                }
            });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    canDeactivate(): boolean {
        return !(this.addEditComponent?.modalIsDisplayed === true);
    }

    // =========================================================================
    // Data Loading
    // =========================================================================

    public loadData(): void {
        if (!this.shiftId) return;

        this.isLoading = true;
        this.error = null;

        this.shiftService.GetResourceShift(Number(this.shiftId), true).subscribe({
            next: (data) => {
                if (!data) {
                    this.error = `Shift #${this.shiftId} not found`;
                    this.isLoading = false;
                    return;
                }
                this.shift = data;
                this.isLoading = false;
            },
            error: (err) => {
                this.error = 'Failed to load shift details';
                this.alertService.showMessage('Error loading shift', err.message || 'Unknown error', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }

    public loadChangeHistory(): void {
        if (!this.shiftId || this.changeHistory != null) return;

        this.isLoadingHistory = true;

        this.shiftService.GetResourceShiftAuditHistory(Number(this.shiftId)).subscribe({
            next: (data: any[]) => {
                this.changeHistory = data || [];
                this.isLoadingHistory = false;
            },
            error: () => {
                this.changeHistory = [];
                this.isLoadingHistory = false;
            }
        });
    }


    // =========================================================================
    // Tab handling
    // =========================================================================

    onTabChange(event: any): void {
        this.activeTab = event?.nextId || 'overview';
        if (this.activeTab === 'history' && this.changeHistory == null) {
            this.loadChangeHistory();
        }
    }


    // =========================================================================
    // Actions
    // =========================================================================

    openEditModal(): void {
        if (!this.shift || !this.addEditComponent) return;
        this.addEditComponent.openModal(this.shift);
    }

    onShiftChanged(): void {
        this.loadData();
        this.changeHistory = null; // Force reload on next tab visit
    }


    // =========================================================================
    // Display Helpers
    // =========================================================================

    getDayNum(): number {
        return Number(this.shift?.dayOfWeek ?? 0);
    }

    formatTime(timeStr: string | null | undefined): string {
        if (!timeStr) return '';
        const parts = String(timeStr).split(':');
        let hours = Number(parts[0]) || 0;
        const minutes = String(parts[1] || '00').padStart(2, '0');
        const ampm = hours >= 12 ? 'PM' : 'AM';
        if (hours === 0) hours = 12;
        else if (hours > 12) hours -= 12;
        return `${hours}:${minutes} ${ampm}`;
    }

    formatEndTime(): string {
        if (!this.shift?.startTime || this.shift.hours == null) return '';
        const startParts = String(this.shift.startTime).split(':');
        const startDecimal = (Number(startParts[0]) || 0) + (Number(startParts[1]) || 0) / 60;
        const endDecimal = startDecimal + Number(this.shift.hours);
        const endH = Math.floor(endDecimal);
        const endM = Math.round((endDecimal % 1) * 60);
        let display = endH;
        const ampm = display >= 12 ? 'PM' : 'AM';
        if (display === 0) display = 12;
        else if (display > 12) display -= 12;
        return `${display}:${String(endM).padStart(2, '0')} ${ampm}`;
    }

    formatDuration(hours: number | null | undefined): string {
        if (hours == null) return '';
        const h = Number(hours);
        return h % 1 === 0 ? `${h}h` : `${h.toFixed(1)}h`;
    }


    // =========================================================================
    // Navigation
    // =========================================================================

    goBack(): void {
        this.navigationService.goBack();
    }

    canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    navigateToResource(): void {
        if (this.shift?.resourceId) {
            this.router.navigate(['/resource', this.shift.resourceId]);
        }
    }

    public userIsSchedulerResourceShiftWriter(): boolean {
        return this.shiftService.userIsSchedulerResourceShiftWriter();
    }
}

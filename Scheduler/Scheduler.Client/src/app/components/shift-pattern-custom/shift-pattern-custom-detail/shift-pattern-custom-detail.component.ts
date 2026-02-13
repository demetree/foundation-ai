/**
 * ShiftPatternCustomDetailComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Detail page for a single ShiftPattern. Features:
 *  - Premium header with pattern name, color, status
 *  - Tabs: Overview · Days · Assigned Resources · Change History
 *  - Inline CRUD for pattern days via modal
 */
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subject, takeUntil, switchMap, tap } from 'rxjs';
import { ShiftPatternService, ShiftPatternData } from '../../../scheduler-data-services/shift-pattern.service';
import { ShiftPatternDayService, ShiftPatternDayData } from '../../../scheduler-data-services/shift-pattern-day.service';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { ShiftPatternCustomAddEditComponent } from '../shift-pattern-custom-add-edit/shift-pattern-custom-add-edit.component';
import { ShiftPatternDayAddEditModalComponent } from '../shift-pattern-day-add-edit-modal/shift-pattern-day-add-edit-modal.component';
import { ConfirmationService } from '../../../services/confirmation-service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-shift-pattern-custom-detail',
    templateUrl: './shift-pattern-custom-detail.component.html',
    styleUrls: ['./shift-pattern-custom-detail.component.scss']
})
export class ShiftPatternCustomDetailComponent implements OnInit, OnDestroy {

    @ViewChild('addEditComponent') addEditComponent!: ShiftPatternCustomAddEditComponent;

    pattern: ShiftPatternData | null = null;
    days: ShiftPatternDayData[] = [];
    resources: ResourceData[] = [];
    auditHistory: any[] = [];

    activeTab: 'overview' | 'days' | 'resources' | 'history' = 'overview';
    isLoading = true;
    isDaysLoading = false;
    isResourcesLoading = false;
    isHistoryLoading = false;

    daysOfWeek = ['', 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    dayColors = ['', '#ef4444', '#f59e0b', '#10b981', '#3b82f6', '#8b5cf6', '#ec4899', '#f97316'];

    private destroy$ = new Subject<void>();

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private patternService: ShiftPatternService,
        private dayService: ShiftPatternDayService,
        private modalService: NgbModal,
        private confirmationService: ConfirmationService,
        private alertService: AlertService,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        this.route.params.pipe(
            takeUntil(this.destroy$),
            switchMap(params => {
                this.isLoading = true;
                return this.patternService.GetShiftPattern(params['shiftPatternId'], true);
            })
        ).subscribe({
            next: (data) => {
                this.pattern = data;
                this.isLoading = false;
                this.loadDays();
            },
            error: () => {
                this.isLoading = false;
                this.alertService.showMessage('Shift Pattern not found', '', MessageSeverity.error);
                this.router.navigate(['/shiftpatterns']);
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    // ── Tab Navigation ──

    setTab(tab: 'overview' | 'days' | 'resources' | 'history'): void {
        this.activeTab = tab;
        if (tab === 'days' && this.days.length === 0) this.loadDays();
        if (tab === 'resources' && this.resources.length === 0) this.loadResources();
        if (tab === 'history' && this.auditHistory.length === 0) this.loadHistory();
    }

    // ── Data Loading ──

    loadDays(): void {
        if (!this.pattern) return;
        this.isDaysLoading = true;
        this.patternService.GetShiftPatternDaysForShiftPattern(this.pattern.id as number)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data) => {
                    this.days = data.sort((a, b) => (a.dayOfWeek as number) - (b.dayOfWeek as number));
                    this.isDaysLoading = false;
                },
                error: () => { this.isDaysLoading = false; }
            });
    }

    loadResources(): void {
        if (!this.pattern) return;
        this.isResourcesLoading = true;
        this.patternService.GetResourcesForShiftPattern(this.pattern.id as number)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data) => {
                    this.resources = data;
                    this.isResourcesLoading = false;
                },
                error: () => { this.isResourcesLoading = false; }
            });
    }

    loadHistory(): void {
        if (!this.pattern) return;
        this.isHistoryLoading = true;
        this.patternService.GetShiftPatternAuditHistory(this.pattern.id as number, true)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data: any[]) => {
                    this.auditHistory = data;
                    this.isHistoryLoading = false;
                },
                error: () => { this.isHistoryLoading = false; }
            });
    }

    // ── Pattern Actions ──

    editPattern(): void {
        if (this.pattern) {
            this.addEditComponent?.openModal(this.pattern);
        }
    }

    onPatternChanged(): void {
        // Reload the pattern after edit
        if (this.pattern) {
            this.patternService.GetShiftPattern(this.pattern.id as number, true)
                .pipe(takeUntil(this.destroy$))
                .subscribe(p => this.pattern = p);
        }
    }

    // ── Day Actions ──

    addDay(): void {
        if (!this.pattern) return;
        const modalRef = this.modalService.open(ShiftPatternDayAddEditModalComponent, {
            size: 'md', backdrop: 'static'
        });
        modalRef.componentInstance.shiftPatternId = this.pattern.id;

        modalRef.result.then(
            () => this.loadDays(),
            () => { }
        );
    }

    editDay(day: ShiftPatternDayData): void {
        if (!this.pattern) return;
        const modalRef = this.modalService.open(ShiftPatternDayAddEditModalComponent, {
            size: 'md', backdrop: 'static'
        });
        modalRef.componentInstance.shiftPatternId = this.pattern.id;
        modalRef.componentInstance.existingDay = day;

        modalRef.result.then(
            () => this.loadDays(),
            () => { }
        );
    }

    deleteDay(day: ShiftPatternDayData): void {
        this.confirmationService.confirm(
            `Delete ${this.daysOfWeek[day.dayOfWeek as number] || 'this day'} from the pattern?`,
            'Delete Day'
        ).then(confirmed => {
            if (confirmed) {
                this.dayService.DeleteShiftPatternDay(day.id)
                    .pipe(takeUntil(this.destroy$))
                    .subscribe({
                        next: () => {
                            this.dayService.ClearAllCaches();
                            this.alertService.showMessage('Day removed', '', MessageSeverity.success);
                            this.loadDays();
                        },
                        error: (err) => {
                            this.alertService.showMessage('Error removing day', err?.message || '', MessageSeverity.error);
                        }
                    });
            }
        });
    }

    navigateToResource(r: ResourceData): void {
        this.router.navigate(['/resource', r.id]);
    }

    goBack(): void {
        this.router.navigate(['/shiftpatterns']);
    }

    // ── Helpers ──

    formatTime(isoTime: string | null): string {
        if (!isoTime) return '—';
        const match = isoTime.match(/(\d{2}):(\d{2})/);
        if (!match) return isoTime;
        const h = parseInt(match[1], 10);
        const m = match[2];
        const ampm = h >= 12 ? 'PM' : 'AM';
        const h12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
        return `${h12}:${m} ${ampm}`;
    }

    formatDuration(hours: number): string {
        const h = Math.floor(hours);
        const m = Math.round((hours - h) * 60);
        if (m === 0) return `${h}h`;
        return `${h}h ${m}m`;
    }

    getTotalWeeklyHours(): number {
        return this.days.reduce((sum, d) => sum + (d.hours || 0), 0);
    }

    getDayColor(dow: number | bigint): string {
        return this.dayColors[Number(dow)] || '#6b7280';
    }

    getDayName(dow: number | bigint): string {
        return this.daysOfWeek[Number(dow)] || '?';
    }

    hasDaySlot(dow: number): boolean {
        return this.days.some(d => d.dayOfWeek === dow);
    }

    userIsWriter(): boolean {
        return this.patternService.userIsSchedulerShiftPatternWriter();
    }
}

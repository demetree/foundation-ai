/**
 * ShiftCustomTableComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Virtual-scroll table (desktop) and card view (mobile) for ResourceShift records.
 * Displays enriched columns: Resource name (link), Day (badge), Start Time (AM/PM),
 * Duration, and Label.  Follows the resource-custom-table pattern.
 */
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, ViewChild } from '@angular/core';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { ResourceShiftService, ResourceShiftData, ResourceShiftSubmitData } from '../../../scheduler-data-services/resource-shift.service';
import { ShiftCustomAddEditComponent } from '../shift-custom-add-edit/shift-custom-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';

interface ShiftTableColumn {
    key: string;
    label: string;
    width?: string;
    template?: 'link' | 'badge' | 'time' | 'duration' | 'boolean' | 'text';
    sortable?: boolean;
    mobileVisible?: boolean;
    linkPath?: string[];
}

@Component({
    selector: 'app-shift-custom-table',
    templateUrl: './shift-custom-table.component.html',
    styleUrls: ['./shift-custom-table.component.scss']
})
export class ShiftCustomTableComponent implements OnInit, AfterViewInit, OnChanges {
    @ViewChild(ShiftCustomAddEditComponent) addEditShiftComponent!: ShiftCustomAddEditComponent;

    @Input() ResourceShifts: ResourceShiftData[] | null = null;
    @Input() isSmallScreen = false;
    @Input() filterText: string | null = null;

    public filteredShifts: ResourceShiftData[] = [];
    public isLoading$ = new BehaviorSubject<boolean>(true);
    private destroy$ = new Subject<void>();

    // Sorting
    public sortColumn = 'dayOfWeek';
    public sortDirection: 'asc' | 'desc' = 'asc';

    // Day names
    public readonly dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    public readonly dayAbbrevs = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    public readonly dayColors: { [key: number]: string } = {
        0: '#94a3b8', 1: '#6366f1', 2: '#8b5cf6', 3: '#06b6d4',
        4: '#f59e0b', 5: '#14b8a6', 6: '#f472b6'
    };

    // Columns definition
    public columns: ShiftTableColumn[] = [];

    constructor(
        private shiftService: ResourceShiftService,
        private authService: AuthService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit(): void {
        this.buildColumns();
        this.loadData();
    }

    ngAfterViewInit(): void {
        if (this.addEditShiftComponent) {
            this.addEditShiftComponent.resourceShiftChanged
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                    next: () => {
                        this.loadData();
                    },
                    error: (err: any) => {
                        this.alertService.showMessage('Error during Shift changed notification', JSON.stringify(err), MessageSeverity.error);
                    }
                });
        }
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['filterText'] && !changes['filterText'].firstChange) {
            this.loadData();
        }
        if (changes['ResourceShifts'] && this.ResourceShifts) {
            this.filteredShifts = [...this.ResourceShifts];
            this.applyFiltersAndSort();
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private buildColumns(): void {
        this.columns = [
            { key: 'resource.name', label: 'Resource', template: 'link', linkPath: ['/resource'], width: '20%', sortable: true, mobileVisible: true },
            { key: 'dayOfWeek', label: 'Day', template: 'badge', width: '12%', sortable: true, mobileVisible: true },
            { key: 'startTime', label: 'Start Time', template: 'time', width: '14%', sortable: true, mobileVisible: true },
            { key: 'hours', label: 'Duration', template: 'duration', width: '12%', sortable: true, mobileVisible: true },
            { key: 'label', label: 'Label', template: 'text', width: '20%', sortable: true, mobileVisible: true },
            { key: 'timeZone.name', label: 'Time Zone', template: 'text', width: '18%', sortable: true, mobileVisible: false }
        ];
    }

    public sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = 'asc';
        }
        this.applyFiltersAndSort();
    }

    public loadData(): void {
        this.isLoading$.next(true);

        const queryParams: any = {
            active: true,
            deleted: false,
            includeRelations: true,
            anyStringContains: this.filterText || undefined,
            sortBy: this.sortColumn || 'dayOfWeek',
            sortDirection: this.sortDirection || 'asc'
        };

        this.shiftService.GetResourceShiftList(queryParams).subscribe({
            next: (shifts) => {
                this.ResourceShifts = shifts;
                this.filteredShifts = shifts ? [...shifts] : [];
                this.applyFiltersAndSort();
                this.isLoading$.next(false);
            },
            error: (err) => {
                this.alertService.showMessage('Failed to load shifts', err.message || 'Unknown error', MessageSeverity.error);
                this.ResourceShifts = [];
                this.filteredShifts = [];
                this.isLoading$.next(false);
            }
        });
    }

    private applyFiltersAndSort(): void {
        if (!this.ResourceShifts) {
            this.filteredShifts = [];
            return;
        }

        let result = [...this.ResourceShifts];

        // Text filter
        if (this.filterText) {
            const lower = this.filterText.toLowerCase();
            result = result.filter(s => {
                const resourceName = s.resource?.name?.toLowerCase() || '';
                const label = (s.label || '').toLowerCase();
                const dayName = this.dayNames[Number(s.dayOfWeek)]?.toLowerCase() || '';
                const startTime = (s.startTime || '').toLowerCase();
                return resourceName.includes(lower) || label.includes(lower) || dayName.includes(lower) || startTime.includes(lower);
            });
        }

        // Sort
        const getVal = (obj: any, path: string): any => {
            return path.split('.').reduce((o, k) => o?.[k], obj);
        };

        result.sort((a, b) => {
            let valA = getVal(a, this.sortColumn);
            let valB = getVal(b, this.sortColumn);

            // Special handling for dayOfWeek — sort Mon→Sun
            if (this.sortColumn === 'dayOfWeek') {
                const order = [1, 2, 3, 4, 5, 6, 0];
                valA = order.indexOf(Number(valA));
                valB = order.indexOf(Number(valB));
            }

            if (valA == null) return 1;
            if (valB == null) return -1;
            if (typeof valA === 'string') valA = valA.toLowerCase();
            if (typeof valB === 'string') valB = valB.toLowerCase();

            const cmp = valA < valB ? -1 : valA > valB ? 1 : 0;
            return this.sortDirection === 'asc' ? cmp : -cmp;
        });

        this.filteredShifts = result;
    }


    // =========================================================================
    // CRUD Actions
    // =========================================================================

    handleEdit(shift: ResourceShiftData): void {
        if (!this.addEditShiftComponent) return;
        this.addEditShiftComponent.openModal(shift);
    }

    handleDelete(shift: ResourceShiftData): void {
        const dayName = this.dayNames[Number(shift.dayOfWeek)] || 'Unknown';
        this.confirmationService.confirm(
            'Confirm Delete',
            `Delete the ${dayName} shift (${this.formatTime(shift.startTime)} – ${this.formatDuration(Number(shift.hours))})?`
        ).then((confirmed) => {
            if (confirmed) this.deleteShift(shift);
        });
    }

    private deleteShift(shift: ResourceShiftData): void {
        const submitData = shift.ConvertToSubmitData();
        submitData.active = false;
        submitData.deleted = true;
        this.shiftService.PutResourceShift(Number(shift.id), submitData).subscribe({
            next: () => {
                this.shiftService.ClearAllCaches();
                this.alertService.showMessage('Shift deleted', '', MessageSeverity.success);
                this.loadData();
            },
            error: (err) => {
                this.alertService.showMessage('Failed to delete shift', err.message || 'Unknown error', MessageSeverity.error);
            }
        });
    }

    handleUndelete(shift: ResourceShiftData): void {
        const dayName = this.dayNames[Number(shift.dayOfWeek)] || 'Unknown';
        this.confirmationService.confirm(
            'Confirm Restore',
            `Restore the ${dayName} shift?`
        ).then((confirmed) => {
            if (confirmed) this.undeleteShift(shift);
        });
    }

    private undeleteShift(shift: ResourceShiftData): void {
        const submitData = shift.ConvertToSubmitData();
        submitData.active = true;
        submitData.deleted = false;
        this.shiftService.PutResourceShift(Number(shift.id), submitData).subscribe({
            next: () => {
                this.shiftService.ClearAllCaches();
                this.alertService.showMessage('Shift restored', '', MessageSeverity.success);
                this.loadData();
            },
            error: (err) => {
                this.alertService.showMessage('Failed to restore shift', err.message || 'Unknown error', MessageSeverity.error);
            }
        });
    }


    // =========================================================================
    // Display Helpers
    // =========================================================================

    public getNestedValue(obj: any, path: string): any {
        return path.split('.').reduce((o, k) => o?.[k], obj);
    }

    public getShiftId(index: number, shift: any): number {
        return Number(shift.id);
    }

    public getDayNum(shift: ResourceShiftData): number {
        return Number(shift.dayOfWeek);
    }

    public formatTime(timeStr: string | null | undefined): string {
        if (!timeStr) return '';
        const parts = String(timeStr).split(':');
        let hours = Number(parts[0]) || 0;
        const minutes = String(parts[1] || '00').padStart(2, '0');
        const ampm = hours >= 12 ? 'PM' : 'AM';
        if (hours === 0) hours = 12;
        else if (hours > 12) hours -= 12;
        return `${hours}:${minutes} ${ampm}`;
    }

    public formatDuration(hours: number | null | undefined): string {
        if (hours == null) return '';
        const h = Number(hours);
        return h % 1 === 0 ? `${h}h` : `${h.toFixed(1)}h`;
    }

    public mobileColumns(): ShiftTableColumn[] {
        return this.columns.filter(c => c.mobileVisible !== false);
    }

    public prominentColumn(): ShiftTableColumn | null {
        return this.columns.find(c => c.template === 'link') || this.columns[0] || null;
    }

    public userIsSchedulerResourceShiftReader(): boolean {
        return this.shiftService.userIsSchedulerResourceShiftReader();
    }

    public userIsSchedulerResourceShiftWriter(): boolean {
        return this.shiftService.userIsSchedulerResourceShiftWriter();
    }
}

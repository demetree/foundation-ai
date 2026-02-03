//
// System Setting Custom Table Component
//
// Table for displaying System Settings with edit/delete actions.
// Emits editRequested event to parent for modal handling.
//

import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';

import { SystemSettingService, SystemSettingData, SystemSettingQueryParameters } from '../../../security-data-services/system-setting.service';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';

@Component({
    selector: 'app-system-setting-custom-table',
    templateUrl: './system-setting-custom-table.component.html',
    styleUrls: ['./system-setting-custom-table.component.scss']
})
export class SystemSettingCustomTableComponent implements OnInit, AfterViewInit, OnChanges {

    //
    // Inputs
    //
    @Input() filterText: string = '';
    @Input() isSmallScreen: boolean = false;

    //
    // Outputs
    //
    @Output() editRequested = new EventEmitter<SystemSettingData>();

    //
    // Data state
    //
    public settings: SystemSettingData[] = [];
    public filteredSettings: SystemSettingData[] = [];
    public loading: boolean = true;
    public errorState: boolean = false;

    //
    // Sorting
    //
    public sortColumn: string = 'name';
    public sortDirection: 'asc' | 'desc' = 'asc';

    //
    // Action tracking
    //
    public actionInProgress: { [key: number]: boolean } = {};


    constructor(
        private router: Router,
        private systemSettingService: SystemSettingService,
        private authService: AuthService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService
    ) { }


    ngOnInit(): void {
        // Initial load happens in ngAfterViewInit
    }


    ngAfterViewInit(): void {
        this.loadData();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['filterText'] && !changes['filterText'].firstChange) {
            this.applyFiltersAndSort();
        }
    }


    //
    // Data Loading
    //
    private loadData(): void {
        this.loading = true;
        this.errorState = false;

        const params = new SystemSettingQueryParameters();
        params.deleted = false;
        params.active = true;

        this.systemSettingService.GetSystemSettingList(params).subscribe({
            next: (settings) => {
                this.settings = settings ?? [];
                this.applyFiltersAndSort();
                this.loading = false;
            },
            error: (err) => {
                console.error('Failed to load settings', err);
                this.settings = [];
                this.filteredSettings = [];
                this.loading = false;
                this.errorState = true;
                this.alertService.showMessage('Error', 'Failed to load system settings', MessageSeverity.error);
            }
        });
    }


    //
    // Filtering and Sorting
    //
    private applyFiltersAndSort(): void {
        let result = [...this.settings];

        //
        // Apply text filter
        //
        if (this.filterText && this.filterText.trim() !== '') {
            const searchLower = this.filterText.toLowerCase();
            result = result.filter(setting => {
                const searchableFields = [
                    setting.name,
                    setting.description,
                    setting.value
                ];
                return searchableFields.some(field =>
                    field && field.toLowerCase().includes(searchLower)
                );
            });
        }

        //
        // Apply sort
        //
        result.sort((a, b) => {
            let aVal = this.getNestedValue(a, this.sortColumn);
            let bVal = this.getNestedValue(b, this.sortColumn);

            if (aVal == null) aVal = '';
            if (bVal == null) bVal = '';

            if (typeof aVal === 'string') aVal = aVal.toLowerCase();
            if (typeof bVal === 'string') bVal = bVal.toLowerCase();

            let comparison = 0;
            if (aVal < bVal) comparison = -1;
            if (aVal > bVal) comparison = 1;

            return this.sortDirection === 'asc' ? comparison : -comparison;
        });

        this.filteredSettings = result;
    }


    private getNestedValue(obj: any, path: string): any {
        return path.split('.').reduce((o, p) => o && o[p], obj);
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


    //
    // Display helpers
    //
    public truncateValue(value: string | null, maxLength: number = 50): string {
        if (!value) return '—';
        if (value.length <= maxLength) return value;
        return value.substring(0, maxLength) + '...';
    }


    //
    // Actions
    //
    public editSetting(setting: SystemSettingData, event: Event): void {
        event.stopPropagation();
        this.editRequested.emit(setting);
    }


    public async deleteSetting(setting: SystemSettingData, event: Event): Promise<void> {
        event.stopPropagation();

        const confirmed = await this.confirmationService.confirm(
            'Delete Setting',
            `Are you sure you want to delete "${setting.name}"?`
        );

        if (confirmed !== true) {
            return;
        }

        const settingId = Number(setting.id);
        if (this.actionInProgress[settingId]) return;

        this.actionInProgress[settingId] = true;

        this.systemSettingService.DeleteSystemSetting(setting.id).subscribe({
            next: () => {
                this.actionInProgress[settingId] = false;
                this.systemSettingService.ClearAllCaches();
                this.loadData();
                this.alertService.showMessage('Success', 'Setting deleted', MessageSeverity.success);
            },
            error: (err) => {
                this.actionInProgress[settingId] = false;
                this.alertService.showMessage('Error', 'Failed to delete setting', MessageSeverity.error);
            }
        });
    }


    public navigateToDetail(setting: SystemSettingData): void {
        this.router.navigate(['/systemsetting', setting.id]);
    }


    public isActionInProgress(setting: SystemSettingData): boolean {
        return this.actionInProgress[Number(setting.id)] === true;
    }


    //
    // Public refresh method for parent to call after edits
    //
    public refreshData(): void {
        this.loadData();
    }


    //
    // Permissions
    //
    public userIsSystemSettingWriter(): boolean {
        return this.systemSettingService.userIsSecuritySystemSettingWriter();
    }


    //
    // TrackBy
    //
    public trackBySettingId(index: number, setting: SystemSettingData): number {
        return Number(setting.id);
    }
}

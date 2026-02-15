import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { VolunteerProfileService, VolunteerProfileData, VolunteerProfileQueryParameters } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerCustomAddEditComponent } from '../volunteer-custom-add-edit/volunteer-custom-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
    selector: 'app-volunteer-custom-table',
    templateUrl: './volunteer-custom-table.component.html',
    styleUrls: ['./volunteer-custom-table.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class VolunteerCustomTableComponent implements OnInit, OnChanges, AfterViewInit {
    @ViewChild(VolunteerCustomAddEditComponent) addEditComponent!: VolunteerCustomAddEditComponent;

    @Input() Volunteers: VolunteerProfileData[] | null = null;
    @Input() isSmallScreen: boolean = false;
    @Input() statusFilter: number | null = null;
    @Input() bgCheckFilter: boolean | null = null;
    @Input() activeFilter: boolean | null = null;
    @Input() filterText: string | null = null;
    @Input() queryParams: Partial<VolunteerProfileQueryParameters> = {};

    @Input() disableDefaultEdit: boolean = false;
    @Input() disableDefaultDelete: boolean = false;
    @Input() disableDefaultUndelete: boolean = false;

    @Output() edit = new EventEmitter<VolunteerProfileData>();
    @Output() delete = new EventEmitter<VolunteerProfileData>();
    @Output() undelete = new EventEmitter<VolunteerProfileData>();

    @Input() columns: TableColumn[] = [];

    public filteredVolunteers: VolunteerProfileData[] | null = null;

    public sortColumn: string | null = null;
    public sortDirection: 'asc' | 'desc' = 'asc';

    private isLoadingSubject = new BehaviorSubject<boolean>(true);
    public isLoading$ = this.isLoadingSubject.asObservable();

    private isManagingData: boolean = false;
    private debounceTimeout: any;
    private destroy$ = new Subject<void>();

    constructor(
        private volunteerProfileService: VolunteerProfileService,
        private authService: AuthService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit(): void {
        if (this.columns.length === 0) {
            this.buildDefaultColumns();
        }

        if (!this.Volunteers) {
            this.isManagingData = true;
            this.loadData();
        } else {
            this.applyFiltersAndSort();
            this.isLoadingSubject.next(false);
        }
    }

    ngAfterViewInit(): void {
        if (this.addEditComponent && !this.disableDefaultEdit) {
            this.addEditComponent.volunteerProfileChanged.subscribe({
                next: () => this.loadData(),
                error: (err: any) => this.alertService.showMessage("Error during Volunteer changed notification", JSON.stringify(err), MessageSeverity.error)
            });
        }
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['Volunteers'] && this.Volunteers) {
            this.applyFiltersAndSort();
        }

        if (changes['filterText']) {
            clearTimeout(this.debounceTimeout);
            this.debounceTimeout = setTimeout(() => {
                if (this.isManagingData) {
                    this.loadData();
                } else {
                    this.applyFiltersAndSort();
                }
            }, 200);
        }

        if (changes['statusFilter'] || changes['bgCheckFilter'] || changes['activeFilter']) {
            this.applyFiltersAndSort();
        }

        if (changes['isSmallScreen'] && !changes['isSmallScreen'].isFirstChange()) {
            // Re-apply if switching between mobile/desktop
        }

        if (changes['queryParams']) {
            this.loadData();
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private buildDefaultColumns(): void {
        const defaultColumns: TableColumn[] = [
            { key: 'resource.name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/volunteers', 'id'] },
            { key: 'volunteerStatus.name', label: 'Status', width: '150px' },
            { key: 'totalHoursServed', label: 'Hours', width: '100px' },
            { key: 'onboardedDate', label: 'Onboarded', width: '140px', template: 'date' },
            { key: 'backgroundCheckCompleted', label: 'BG Check', width: '100px', template: 'boolean' },
        ];

        const isWriter = this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
        const isAdmin = this.authService.isSchedulerAdministrator;

        if (isAdmin) {
            defaultColumns.push(
                { key: 'versionNumber', label: 'Version', width: undefined },
                { key: 'active', label: 'Active', width: '100px', template: 'boolean' },
                { key: 'deleted', label: 'Deleted', width: '100px', template: 'boolean' }
            );
        } else if (isWriter) {
            defaultColumns.push(
                { key: 'active', label: 'Active', width: '100px', template: 'boolean' }
            );
        }

        this.columns = defaultColumns;
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
        if (!this.isManagingData) return;

        if (!this.volunteerProfileService.userIsSchedulerVolunteerProfileReader()) {
            this.alertService.showMessage(this.authService.currentUser?.userName + " does not have permission to read Volunteers", '', MessageSeverity.info);
            return;
        }

        const queryParams = {
            ...this.queryParams,
            includeRelations: true,
            anyStringContains: this.filterText || undefined
        };

        this.volunteerProfileService.GetVolunteerProfileList(queryParams).subscribe({
            next: (list) => {
                this.Volunteers = list || [];
                this.applyFiltersAndSort();
                this.isLoadingSubject.next(false);
            },
            error: (err) => {
                this.isLoadingSubject.next(false);
                this.alertService.showMessage("Error getting Volunteer data", JSON.stringify(err), MessageSeverity.error);
            }
        });
    }

    private applyFiltersAndSort(): void {
        if (!this.Volunteers) {
            this.filteredVolunteers = null;
            return;
        }

        const getNestedValue = (obj: any, path: string): any => {
            return path.split('.').reduce((current, key) => {
                return current && current[key] !== undefined ? current[key] : '';
            }, obj);
        };

        let result = [...this.Volunteers];

        if (this.filterText) {
            const searchText = this.filterText.toLowerCase().trim();
            if (searchText) {
                const filterFields = [
                    'resource.name',
                    'volunteerStatus.name',
                    'availabilityPreferences',
                    'interestsAndSkillsNotes',
                    'emergencyContactNotes',
                ];
                result = result.filter((v) =>
                    filterFields.some((field) => {
                        const value = getNestedValue(v, field);
                        return value && value.toString().toLowerCase().includes(searchText);
                    })
                );
            }
        }

        // Apply dropdown filters
        if (this.statusFilter !== null && this.statusFilter !== undefined) {
            result = result.filter(v => Number(v.volunteerStatusId) === this.statusFilter);
        }

        if (this.bgCheckFilter !== null && this.bgCheckFilter !== undefined) {
            result = result.filter(v => v.backgroundCheckCompleted === this.bgCheckFilter);
        }

        if (this.activeFilter !== null && this.activeFilter !== undefined) {
            result = result.filter(v => v.active === this.activeFilter);
        }

        if (this.sortColumn) {
            result.sort((a, b) => {
                const aValue = getNestedValue(a, this.sortColumn!);
                const bValue = getNestedValue(b, this.sortColumn!);

                if (typeof aValue === 'number' && typeof bValue === 'number') {
                    return this.sortDirection === 'asc' ? aValue - bValue : bValue - aValue;
                }

                const aStr = aValue ? aValue.toString() : '';
                const bStr = bValue ? bValue.toString() : '';
                const comparison = aStr.localeCompare(bStr, undefined, { sensitivity: 'base' });
                return this.sortDirection === 'asc' ? comparison : -comparison;
            });
        }

        this.filteredVolunteers = result;
    }

    public handleEdit(volunteer: VolunteerProfileData): void {
        if (this.disableDefaultEdit) {
            this.edit.emit(volunteer);
        } else if (this.addEditComponent) {
            this.addEditComponent.openModal(volunteer);
        } else {
            this.alertService.showMessage('Edit functionality unavailable', 'Add/Edit component not initialized', MessageSeverity.warn);
        }
    }

    public handleDelete(volunteer: VolunteerProfileData): void {
        if (this.disableDefaultDelete) {
            this.delete.emit(volunteer);
        } else {
            this.confirmationService
                .confirm('Delete Volunteer', 'Are you sure you want to delete this volunteer profile?')
                .then((result) => { if (result) this.deleteVolunteer(volunteer); })
                .catch(() => { });
        }
    }

    private deleteVolunteer(data: VolunteerProfileData): void {
        this.volunteerProfileService.DeleteVolunteerProfile(data.id).subscribe({
            next: () => {
                this.volunteerProfileService.ClearAllCaches();
                this.loadData();
            },
            error: (err) => this.alertService.showMessage("Error deleting Volunteer", JSON.stringify(err), MessageSeverity.error)
        });
    }

    public handleUndelete(volunteer: VolunteerProfileData): void {
        if (this.disableDefaultUndelete) {
            this.undelete.emit(volunteer);
        } else {
            this.confirmationService
                .confirm('Undelete Volunteer', 'Are you sure you want to undelete this volunteer profile?')
                .then((result) => { if (result) this.undeleteVolunteer(volunteer); })
                .catch(() => { });
        }
    }

    private undeleteVolunteer(data: VolunteerProfileData): void {
        var submitData = this.volunteerProfileService.ConvertToVolunteerProfileSubmitData(data);
        submitData.deleted = false;

        this.volunteerProfileService.PutVolunteerProfile(submitData.id, submitData).subscribe({
            next: () => {
                this.volunteerProfileService.ClearAllCaches();
                this.loadData();
            },
            error: (err) => this.alertService.showMessage("Error undeleting Volunteer", JSON.stringify(err), MessageSeverity.error)
        });
    }

    public getVolunteerId(index: number, volunteer: any): number {
        return volunteer.id;
    }

    public userIsVolunteerReader(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileReader();
    }

    public userIsVolunteerWriter(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
    }

    public getNestedValue(obj: any, path: string): any {
        return path.split('.').reduce((acc, part) => acc && acc[part], obj);
    }

    public buildLink(item: any, path: string[]): any[] {
        return path.map(segment => segment.startsWith('/') ? segment : item[segment]);
    }

    get mobileColumns(): TableColumn[] {
        return this.columns.filter(col => col.mobile !== 'hidden');
    }

    get prominentColumn(): TableColumn | null {
        return this.columns.find(col => col.mobile === 'prominent') || null;
    }
}

import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { VolunteerGroupService, VolunteerGroupData, VolunteerGroupQueryParameters } from '../../../scheduler-data-services/volunteer-group.service';
import { VolunteerGroupCustomAddEditComponent } from '../volunteer-group-custom-add-edit/volunteer-group-custom-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
    selector: 'app-volunteer-group-custom-table',
    templateUrl: './volunteer-group-custom-table.component.html',
    styleUrls: ['./volunteer-group-custom-table.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class VolunteerGroupCustomTableComponent implements OnInit, OnChanges, AfterViewInit {
    @ViewChild(VolunteerGroupCustomAddEditComponent) addEditComponent!: VolunteerGroupCustomAddEditComponent;

    @Input() Groups: VolunteerGroupData[] | null = null;
    @Input() isSmallScreen: boolean = false;
    @Input() filterText: string | null = null;
    @Input() queryParams: Partial<VolunteerGroupQueryParameters> = {};

    @Input() disableDefaultEdit: boolean = false;
    @Input() disableDefaultDelete: boolean = false;
    @Input() disableDefaultUndelete: boolean = false;

    @Output() edit = new EventEmitter<VolunteerGroupData>();
    @Output() delete = new EventEmitter<VolunteerGroupData>();
    @Output() undelete = new EventEmitter<VolunteerGroupData>();

    @Input() columns: TableColumn[] = [];

    public filteredGroups: VolunteerGroupData[] | null = null;

    public sortColumn: string | null = null;
    public sortDirection: 'asc' | 'desc' = 'asc';

    private isLoadingSubject = new BehaviorSubject<boolean>(true);
    public isLoading$ = this.isLoadingSubject.asObservable();

    private isManagingData: boolean = false;
    private debounceTimeout: any;
    private destroy$ = new Subject<void>();

    constructor(
        private volunteerGroupService: VolunteerGroupService,
        private authService: AuthService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit(): void {
        if (this.columns.length === 0) {
            this.buildDefaultColumns();
        }

        if (!this.Groups) {
            this.isManagingData = true;
            this.loadData();
        } else {
            this.applyFiltersAndSort();
            this.isLoadingSubject.next(false);
        }
    }

    ngAfterViewInit(): void {
        if (this.addEditComponent && !this.disableDefaultEdit) {
            this.addEditComponent.volunteerGroupChanged.subscribe({
                next: () => this.loadData(),
                error: (err: any) => this.alertService.showMessage("Error during Group changed notification", JSON.stringify(err), MessageSeverity.error)
            });
        }
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['filterText'] && this.isManagingData) {
            clearTimeout(this.debounceTimeout);
            this.debounceTimeout = setTimeout(() => {
                if (this.isManagingData) {
                    this.loadData();
                } else {
                    this.applyFiltersAndSort();
                }
            }, 200);
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
            { key: 'name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/volunteergroups', 'id'] },
            { key: 'description', label: 'Description', width: '250px' },
            { key: 'office.name', label: 'Office', width: '150px' },
            { key: 'volunteerStatus.name', label: 'Status', width: '120px' },
            { key: 'maxMembers', label: 'Max Members', width: '120px' },
            { key: 'color', label: 'Color', width: '80px', template: 'color' },
        ];

        const isAdmin = this.authService.isSchedulerAdministrator;

        if (isAdmin) {
            defaultColumns.push(
                { key: 'versionNumber', label: 'Version', width: undefined },
                { key: 'active', label: 'Active', width: '80px', template: 'boolean' },
                { key: 'deleted', label: 'Deleted', width: '80px', template: 'boolean' }
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

        if (!this.volunteerGroupService.userIsSchedulerVolunteerGroupReader()) {
            this.alertService.showMessage(this.authService.currentUser?.userName + " does not have permission to read Volunteer Groups", '', MessageSeverity.info);
            return;
        }

        const queryParams = {
            ...this.queryParams,
            includeRelations: true,
            anyStringContains: this.filterText || undefined
        };

        this.volunteerGroupService.GetVolunteerGroupList(queryParams).subscribe({
            next: (list) => {
                this.Groups = list || [];
                this.applyFiltersAndSort();
                this.isLoadingSubject.next(false);
            },
            error: (err) => {
                this.isLoadingSubject.next(false);
                this.alertService.showMessage("Error getting Volunteer Group data", JSON.stringify(err), MessageSeverity.error);
            }
        });
    }

    private applyFiltersAndSort(): void {
        if (!this.Groups) {
            this.filteredGroups = null;
            return;
        }

        const getNestedValue = (obj: any, path: string): any => {
            return path.split('.').reduce((current, key) => {
                return current && current[key] !== undefined ? current[key] : '';
            }, obj);
        };

        let result = [...this.Groups];

        if (this.filterText) {
            const searchText = this.filterText.toLowerCase().trim();
            if (searchText) {
                result = result.filter((g) =>
                    ['name', 'description', 'purpose', 'notes', 'office.name', 'volunteerStatus.name'].some(field => {
                        const value = getNestedValue(g, field);
                        return value && value.toString().toLowerCase().includes(searchText);
                    })
                );
            }
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

        this.filteredGroups = result;
    }

    public handleEdit(group: VolunteerGroupData): void {
        if (this.disableDefaultEdit) {
            this.edit.emit(group);
        } else if (this.addEditComponent) {
            this.addEditComponent.openModal(group);
        }
    }

    public handleDelete(group: VolunteerGroupData): void {
        if (this.disableDefaultDelete) {
            this.delete.emit(group);
        } else {
            this.confirmationService
                .confirm('Delete Group', 'Are you sure you want to delete this volunteer group?')
                .then((result) => { if (result) this.deleteGroup(group); })
                .catch(() => { });
        }
    }

    private deleteGroup(data: VolunteerGroupData): void {
        this.volunteerGroupService.DeleteVolunteerGroup(data.id).subscribe({
            next: () => {
                this.volunteerGroupService.ClearAllCaches();
                this.loadData();
            },
            error: (err) => this.alertService.showMessage("Error deleting Group", JSON.stringify(err), MessageSeverity.error)
        });
    }

    public handleUndelete(group: VolunteerGroupData): void {
        if (this.disableDefaultUndelete) {
            this.undelete.emit(group);
        } else {
            this.confirmationService
                .confirm('Undelete Group', 'Are you sure you want to undelete this volunteer group?')
                .then((result) => { if (result) this.undeleteGroup(group); })
                .catch(() => { });
        }
    }

    private undeleteGroup(data: VolunteerGroupData): void {
        var submitData = this.volunteerGroupService.ConvertToVolunteerGroupSubmitData(data);
        submitData.deleted = false;

        this.volunteerGroupService.PutVolunteerGroup(submitData.id, submitData).subscribe({
            next: () => {
                this.volunteerGroupService.ClearAllCaches();
                this.loadData();
            },
            error: (err) => this.alertService.showMessage("Error undeleting Group", JSON.stringify(err), MessageSeverity.error)
        });
    }

    public getGroupId(index: number, group: any): number {
        return group.id;
    }

    public userIsGroupReader(): boolean {
        return this.volunteerGroupService.userIsSchedulerVolunteerGroupReader();
    }

    public userIsGroupWriter(): boolean {
        return this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter();
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

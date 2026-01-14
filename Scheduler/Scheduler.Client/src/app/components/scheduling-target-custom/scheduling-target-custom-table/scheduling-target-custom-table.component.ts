
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { SchedulingTargetService, SchedulingTargetData, SchedulingTargetQueryParameters } from '../../../scheduler-data-services/scheduling-target.service';
import { SchedulingTargetAddEditComponent } from '../../../scheduler-data-components/scheduling-target/scheduling-target-add-edit/scheduling-target-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
    selector: 'app-scheduling-target-custom-table',
    templateUrl: './scheduling-target-custom-table.component.html',
    styleUrls: ['./scheduling-target-custom-table.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SchedulingTargetCustomTableComponent implements OnInit, OnChanges, AfterViewInit {
    @ViewChild(SchedulingTargetAddEditComponent) addEditSchedulingTargetComponent!: SchedulingTargetAddEditComponent;

    @Input() SchedulingTargets: SchedulingTargetData[] | null = null;
    @Input() isSmallScreen: boolean = false;
    @Input() filterText: string | null = null;
    @Input() queryParams: Partial<SchedulingTargetQueryParameters> = {}

    @Input() disableDefaultEdit: boolean = false;
    @Input() disableDefaultDelete: boolean = false;
    @Input() disableDefaultUndelete: boolean = false;

    @Output() edit = new EventEmitter<SchedulingTargetData>();
    @Output() delete = new EventEmitter<SchedulingTargetData>();
    @Output() undelete = new EventEmitter<SchedulingTargetData>();

    @Input() columns: TableColumn[] = [];

    public filteredSchedulingTargets: SchedulingTargetData[] | null = null;

    public sortColumn: string | null = null;
    public sortDirection: 'asc' | 'desc' = 'asc';

    private isLoadingSubject = new BehaviorSubject<boolean>(true);
    public isLoading$ = this.isLoadingSubject.asObservable();

    private isManagingData: boolean = false;
    private debounceTimeout: any;
    private inErrorState: boolean = false;
    private errorResetTimeout: any;

    constructor(private schedulingTargetService: SchedulingTargetService,
        private authService: AuthService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService) { }

    ngOnInit(): void {

        if (this.columns.length === 0) {
            this.buildDefaultColumns();
        }

        if (!this.SchedulingTargets) {

            this.isManagingData = true;
            this.loadData();

        } else {

            this.applyFiltersAndSort();
            this.isLoadingSubject.next(false);

        }
    }

    ngAfterViewInit(): void {
        if (this.addEditSchedulingTargetComponent && !this.disableDefaultEdit) {
            this.addEditSchedulingTargetComponent.schedulingTargetChanged.subscribe({
                next: (result: SchedulingTargetData[] | null) => {
                    this.loadData();
                },
                error: (err: any) => {
                    this.alertService.showMessage("Error during Scheduling Target changed notification", JSON.stringify(err), MessageSeverity.error);
                }
            });
        }
    }

    ngOnChanges(changes: SimpleChanges): void {

        if (this.inErrorState == true) {
            return;
        }

        if (changes['filterText'] && this.isManagingData == true) {
            clearTimeout(this.debounceTimeout);
            this.debounceTimeout = setTimeout(() => {

                if (this.isManagingData) {
                    this.loadData();
                }
                else {
                    this.applyFiltersAndSort();
                }

            }, 200);
        }

        if (changes['queryParams']) {
            this.loadData()
        }
    }


    private buildDefaultColumns(): void {
        const defaultColumns: TableColumn[] = [
            { key: 'name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/schedulingtarget', 'id'] },
            { key: 'description', label: 'Description', width: undefined },
            { key: 'office.name', label: 'Office', width: undefined, template: 'link', linkPath: ['/office', 'officeId'] },
            { key: 'client.name', label: 'Client', width: undefined, template: 'link', linkPath: ['/client', 'clientId'] },
            { key: 'schedulingTargetType.name', label: 'Type', width: undefined, template: 'link', linkPath: ['/schedulingtargettype', 'schedulingTargetTypeId'] },
            // { key: 'timeZone.name', label: 'Time Zone', width: undefined, template: 'link', linkPath: ['/timezone', 'timeZoneId'] },
            // { key: 'calendar.name', label: 'Calendar', width: undefined, template: 'link', linkPath: ['/calendar', 'calendarId'] },
            { key: 'color', label: 'Color', width: "50px", template: 'color' },
        ];

        const isWriter = this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
        const isAdmin = this.authService.isSchedulerAdministrator;

        if (isAdmin) {
            //  defaultColumns.push({ key: 'versionNumber', label: 'Version Number', width: undefined });
            defaultColumns.push({ key: 'active', label: 'Active', width: '80px', template: 'boolean' });
            //  defaultColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });
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

        if (!this.isManagingData) {
            return;
        }

        if (this.schedulingTargetService.userIsSchedulerSchedulingTargetReader() == false) {
            this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Scheduling Targets", '', MessageSeverity.info);
            return;
        }

        const schedulingTargetQueryParams = {
            ...this.queryParams,
            anyStringContains: this.filterText || undefined
        };

        this.schedulingTargetService.GetSchedulingTargetList(schedulingTargetQueryParams).subscribe({
            next: (SchedulingTargetList) => {
                if (SchedulingTargetList) {
                    this.SchedulingTargets = SchedulingTargetList;
                } else {
                    this.SchedulingTargets = [];
                }

                this.applyFiltersAndSort();
                this.isLoadingSubject.next(false);
                this.inErrorState = false;

            },
            error: (err) => {
                this.isLoadingSubject.next(false);
                this.setErrorState();
                this.alertService.showMessage("Error getting Scheduling Target data", JSON.stringify(err), MessageSeverity.error);
            }
        });
    }


    private setErrorState(): void {
        this.inErrorState = true;
        clearTimeout(this.errorResetTimeout);
        this.errorResetTimeout = setTimeout(() => {
            this.inErrorState = false;
        }, 15000);
    }


    private applyFiltersAndSort(): void {

        if (!this.SchedulingTargets) {
            this.filteredSchedulingTargets = null;
            return;
        }

        const getNestedValue = (obj: any, path: string): any => {
            return path.split('.').reduce((current, key) => {
                return current && current[key] !== undefined ? current[key] : '';
            }, obj);
        };


        let result = [...this.SchedulingTargets];

        if (this.filterText) {

            const searchText = this.filterText.toLowerCase().trim();

            if (searchText) {

                const filterFields = [
                    'name',
                    'description',
                    'office.name',
                    'client.name',
                    'schedulingTargetType.name',
                    'timeZone.name',
                    'calendar.name',
                    'notes',
                    'externalId',
                    'color',
                    'attributes',
                ];

                result = result.filter((schedulingTarget) =>
                    filterFields.some((field) => {
                        const value = getNestedValue(schedulingTarget, field);
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

        this.filteredSchedulingTargets = result;
    }


    public handleEdit(schedulingTarget: SchedulingTargetData): void {
        if (this.disableDefaultEdit) {
            this.edit.emit(schedulingTarget);
        }
        else if (this.addEditSchedulingTargetComponent) {
            this.addEditSchedulingTargetComponent.openModal(schedulingTarget);
        }
        else {
            this.alertService.showMessage(
                'Edit functionality unavailable',
                'Add/Edit component not initialized',
                MessageSeverity.warn
            );
        }
    }


    public handleDelete(schedulingTarget: SchedulingTargetData): void {
        if (this.disableDefaultDelete) {
            this.delete.emit(schedulingTarget);
        }
        else {
            this.confirmationService
                .confirm('Delete SchedulingTarget', 'Are you sure you want to delete this Scheduling Target?')
                .then((result) => {
                    if (result) {
                        this.deleteSchedulingTarget(schedulingTarget);
                    }
                })
                .catch(() => { });
        }
    }


    private deleteSchedulingTarget(schedulingTargetData: SchedulingTargetData): void {
        this.schedulingTargetService.DeleteSchedulingTarget(schedulingTargetData.id).subscribe({
            next: () => {
                this.schedulingTargetService.ClearAllCaches();
                this.loadData();
            },
            error: (err) => {
                this.alertService.showMessage("Error deleting Scheduling Target", JSON.stringify(err), MessageSeverity.error);
            }
        });
    }


    public handleUndelete(schedulingTarget: SchedulingTargetData): void {
        if (this.disableDefaultUndelete) {
            this.undelete.emit(schedulingTarget);
        }
        else {
            this.confirmationService
                .confirm('Undelete SchedulingTarget', 'Are you sure you want to undelete this Scheduling Target?')
                .then((result) => {
                    if (result) {
                        this.undeleteSchedulingTarget(schedulingTarget);
                    }
                })
                .catch(() => { });
        }
    }


    private undeleteSchedulingTarget(schedulingTargetData: SchedulingTargetData): void {

        var schedulingTargetToSubmit = this.schedulingTargetService.ConvertToSchedulingTargetSubmitData(schedulingTargetData);
        schedulingTargetToSubmit.deleted = false;

        this.schedulingTargetService.PutSchedulingTarget(schedulingTargetToSubmit.id, schedulingTargetToSubmit).subscribe({
            next: () => {
                this.schedulingTargetService.ClearAllCaches();
                this.loadData();
            },
            error: (err) => {
                this.alertService.showMessage("Error undeleting Scheduling Target", JSON.stringify(err), MessageSeverity.error);
            }
        });
    }


    public getSchedulingTargetId(index: number, schedulingTarget: any): number {
        return schedulingTarget.id;
    }


    public userIsSchedulerSchedulingTargetReader(): boolean {
        return this.schedulingTargetService.userIsSchedulerSchedulingTargetReader();
    }

    public userIsSchedulerSchedulingTargetWriter(): boolean {
        return this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
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

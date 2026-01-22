import { Component, Output, EventEmitter, ViewChild, TemplateRef } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbOffcanvas, NgbOffcanvasRef } from '@ng-bootstrap/ng-bootstrap';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationData } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentService, SecurityDepartmentData, SecurityDepartmentQueryParameters } from '../../../security-data-services/security-department.service';
import { SecurityUserService, SecurityUserData, SecurityUserQueryParameters } from '../../../security-data-services/security-user.service';
import { DepartmentDetailPanelComponent } from '../department-detail-panel/department-detail-panel.component';
import { DepartmentAddEditComponent } from '../department-add-edit/department-add-edit.component';

@Component({
    selector: 'app-organization-detail-panel',
    templateUrl: './organization-detail-panel.component.html',
    styleUrls: ['./organization-detail-panel.component.scss']
})
export class OrganizationDetailPanelComponent {
    @ViewChild('panelTemplate') panelTemplate!: TemplateRef<any>;
    @ViewChild('deptPanel') deptPanel!: DepartmentDetailPanelComponent;
    @ViewChild('deptAddEdit') deptAddEdit!: DepartmentAddEditComponent;
    @Output() closed = new EventEmitter<void>();
    @Output() changed = new EventEmitter<void>();

    private destroy$ = new Subject<void>();
    private offcanvasRef: NgbOffcanvasRef | null = null;

    organization: SecurityOrganizationData | null = null;
    tenant: SecurityTenantData | null = null;
    activeTab: string = 'overview';

    departments$ = new BehaviorSubject<SecurityDepartmentData[]>([]);
    users$ = new BehaviorSubject<SecurityUserData[]>([]);
    isLoadingDepartments$ = new BehaviorSubject<boolean>(false);
    isLoadingUsers$ = new BehaviorSubject<boolean>(false);

    selectedDepartment: SecurityDepartmentData | null = null;

    constructor(
        private offcanvasService: NgbOffcanvas,
        private securityDepartmentService: SecurityDepartmentService,
        private securityUserService: SecurityUserService
    ) { }

    open(org: SecurityOrganizationData, tenant: SecurityTenantData): void {
        this.organization = org;
        this.tenant = tenant;
        this.activeTab = 'overview';

        this.offcanvasRef = this.offcanvasService.open(this.panelTemplate, {
            position: 'end',
            panelClass: 'org-detail-panel',
            backdrop: 'static'
        });

        this.offcanvasRef.dismissed.subscribe(() => {
            this.closed.emit();
        });

        this.loadDepartments();
        this.loadUsers();
    }

    close(): void {
        if (this.offcanvasRef) {
            this.offcanvasRef.close();
            this.offcanvasRef = null;
        }
        this.closed.emit();
    }

    private loadDepartments(): void {
        if (!this.organization) return;

        this.isLoadingDepartments$.next(true);

        const params = new SecurityDepartmentQueryParameters();
        params.securityOrganizationId = this.organization.id;

        this.securityDepartmentService.GetSecurityDepartmentList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (depts: SecurityDepartmentData[]) => {
                this.departments$.next(depts);
                this.isLoadingDepartments$.next(false);
            },
            error: () => {
                this.departments$.next([]);
                this.isLoadingDepartments$.next(false);
            }
        });
    }

    private loadUsers(): void {
        if (!this.organization) return;

        this.isLoadingUsers$.next(true);

        const params = new SecurityUserQueryParameters();
        params.securityOrganizationId = this.organization.id;

        this.securityUserService.GetSecurityUserList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (users: SecurityUserData[]) => {
                this.users$.next(users);
                this.isLoadingUsers$.next(false);
            },
            error: () => {
                this.users$.next([]);
                this.isLoadingUsers$.next(false);
            }
        });
    }

    openDepartment(dept: SecurityDepartmentData): void {
        this.selectedDepartment = dept;
        if (this.deptPanel && this.organization) {
            this.deptPanel.open(dept, this.organization, this.tenant!);
        }
    }

    addDepartment(): void {
        if (this.deptAddEdit && this.organization) {
            this.deptAddEdit.openForCreate(this.organization);
        }
    }

    onDepartmentSaved(dept: SecurityDepartmentData): void {
        this.loadDepartments();
        this.changed.emit();
    }

    onDepartmentClosed(): void {
        this.selectedDepartment = null;
    }

    onDepartmentChanged(): void {
        this.loadDepartments();
        this.changed.emit();
    }

    setActiveTab(tab: string): void {
        this.activeTab = tab;
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}

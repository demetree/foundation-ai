import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationService, SecurityOrganizationData, SecurityOrganizationQueryParameters } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentService, SecurityDepartmentData, SecurityDepartmentQueryParameters } from '../../../security-data-services/security-department.service';
import { SecurityTeamService, SecurityTeamData, SecurityTeamQueryParameters } from '../../../security-data-services/security-team.service';
import { OrganizationDetailPanelComponent } from '../organization-detail-panel/organization-detail-panel.component';
import { OrganizationAddEditComponent } from '../organization-add-edit/organization-add-edit.component';
import { DepartmentDetailPanelComponent } from '../department-detail-panel/department-detail-panel.component';
import { DepartmentAddEditComponent } from '../department-add-edit/department-add-edit.component';
import { TeamDetailPanelComponent } from '../team-detail-panel/team-detail-panel.component';
import { TeamAddEditComponent } from '../team-add-edit/team-add-edit.component';

//
// Hierarchy data structures
//
interface DepartmentWithTeams {
    department: SecurityDepartmentData;
    teams: SecurityTeamData[];
    isExpanded: boolean;
    isLoadingTeams: boolean;
}

interface OrganizationWithHierarchy {
    organization: SecurityOrganizationData;
    departments: DepartmentWithTeams[];
    isExpanded: boolean;
    isLoadingDepartments: boolean;
}

@Component({
    selector: 'app-tenant-organizations-tab',
    templateUrl: './tenant-organizations-tab.component.html',
    styleUrls: ['./tenant-organizations-tab.component.scss']
})
export class TenantOrganizationsTabComponent implements OnInit, OnDestroy {
    @Input() tenant!: SecurityTenantData;
    @ViewChild('orgPanel') orgPanel!: OrganizationDetailPanelComponent;
    @ViewChild('orgAddEdit') orgAddEdit!: OrganizationAddEditComponent;
    @ViewChild('deptPanel') deptPanel!: DepartmentDetailPanelComponent;
    @ViewChild('deptAddEdit') deptAddEdit!: DepartmentAddEditComponent;
    @ViewChild('teamPanel') teamPanel!: TeamDetailPanelComponent;
    @ViewChild('teamAddEdit') teamAddEdit!: TeamAddEditComponent;

    private destroy$ = new Subject<void>();

    organizations$ = new BehaviorSubject<SecurityOrganizationData[]>([]);
    organizationHierarchy$ = new BehaviorSubject<OrganizationWithHierarchy[]>([]);
    isLoading$ = new BehaviorSubject<boolean>(true);
    selectedOrganization: SecurityOrganizationData | null = null;

    constructor(
        private securityOrganizationService: SecurityOrganizationService,
        private securityDepartmentService: SecurityDepartmentService,
        private securityTeamService: SecurityTeamService
    ) { }

    ngOnInit(): void {
        this.loadOrganizations();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadOrganizations(): void {
        if (!this.tenant) return;

        this.isLoading$.next(true);

        const params = new SecurityOrganizationQueryParameters();
        params.securityTenantId = this.tenant.id;

        this.securityOrganizationService.GetSecurityOrganizationList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (orgs: SecurityOrganizationData[]) => {
                this.organizations$.next(orgs);

                //
                // Build hierarchy structure
                //
                const hierarchy: OrganizationWithHierarchy[] = orgs.map(org => ({
                    organization: org,
                    departments: [],
                    isExpanded: false,
                    isLoadingDepartments: false
                }));
                this.organizationHierarchy$.next(hierarchy);
                this.isLoading$.next(false);
            },
            error: () => {
                this.organizations$.next([]);
                this.organizationHierarchy$.next([]);
                this.isLoading$.next(false);
            }
        });
    }

    //
    // Expansion toggling
    //
    toggleOrganization(orgHierarchy: OrganizationWithHierarchy, event: Event): void {
        event.stopPropagation();

        if (orgHierarchy.isExpanded) {
            orgHierarchy.isExpanded = false;
        } else {
            orgHierarchy.isExpanded = true;
            if (orgHierarchy.departments.length === 0) {
                this.loadDepartmentsForOrg(orgHierarchy);
            }
        }
    }

    toggleDepartment(deptWithTeams: DepartmentWithTeams, event: Event): void {
        event.stopPropagation();

        if (deptWithTeams.isExpanded) {
            deptWithTeams.isExpanded = false;
        } else {
            deptWithTeams.isExpanded = true;
            if (deptWithTeams.teams.length === 0) {
                this.loadTeamsForDept(deptWithTeams);
            }
        }
    }

    //
    // Data loading for hierarchy
    //
    private loadDepartmentsForOrg(orgHierarchy: OrganizationWithHierarchy): void {
        orgHierarchy.isLoadingDepartments = true;

        const params = new SecurityDepartmentQueryParameters();
        params.securityOrganizationId = orgHierarchy.organization.id;

        this.securityDepartmentService.GetSecurityDepartmentList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (depts: SecurityDepartmentData[]) => {
                orgHierarchy.departments = depts.map(dept => ({
                    department: dept,
                    teams: [],
                    isExpanded: false,
                    isLoadingTeams: false
                }));
                orgHierarchy.isLoadingDepartments = false;
            },
            error: () => {
                orgHierarchy.departments = [];
                orgHierarchy.isLoadingDepartments = false;
            }
        });
    }

    private loadTeamsForDept(deptWithTeams: DepartmentWithTeams): void {
        deptWithTeams.isLoadingTeams = true;

        const params = new SecurityTeamQueryParameters();
        params.securityDepartmentId = deptWithTeams.department.id;

        this.securityTeamService.GetSecurityTeamList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (teams: SecurityTeamData[]) => {
                deptWithTeams.teams = teams;
                deptWithTeams.isLoadingTeams = false;
            },
            error: () => {
                deptWithTeams.teams = [];
                deptWithTeams.isLoadingTeams = false;
            }
        });
    }

    //
    // Navigation actions
    //
    openOrganization(org: SecurityOrganizationData, event: Event): void {
        event.stopPropagation();
        this.selectedOrganization = org;
        if (this.orgPanel) {
            this.orgPanel.open(org, this.tenant);
        }
    }

    openDepartment(dept: SecurityDepartmentData, org: SecurityOrganizationData, event: Event): void {
        event.stopPropagation();
        if (this.deptPanel) {
            this.deptPanel.open(dept, org, this.tenant);
        }
    }

    openTeam(team: SecurityTeamData, dept: SecurityDepartmentData, org: SecurityOrganizationData, event: Event): void {
        event.stopPropagation();
        if (this.teamPanel) {
            this.teamPanel.open(team, dept, org, this.tenant);
        }
    }

    //
    // Add actions
    //
    addOrganization(): void {
        if (this.orgAddEdit && this.tenant) {
            this.orgAddEdit.openForCreate(this.tenant);
        }
    }

    addDepartment(org: SecurityOrganizationData, event: Event): void {
        event.stopPropagation();
        if (this.deptAddEdit) {
            this.deptAddEdit.openForCreate(org);
        }
    }

    addTeam(dept: SecurityDepartmentData, event: Event): void {
        event.stopPropagation();
        if (this.teamAddEdit) {
            this.teamAddEdit.openForCreate(dept);
        }
    }

    //
    // Callbacks
    //
    onOrganizationSaved(org: SecurityOrganizationData): void {
        this.loadOrganizations();
    }

    onOrganizationClosed(): void {
        this.selectedOrganization = null;
    }

    onOrganizationChanged(): void {
        this.loadOrganizations();
    }

    onDepartmentSaved(dept: SecurityDepartmentData): void {
        this.loadOrganizations();
    }

    onDepartmentClosed(): void {
        // Optional: handle department panel closed
    }

    onDepartmentChanged(): void {
        this.loadOrganizations();
    }

    onTeamSaved(team: SecurityTeamData): void {
        this.loadOrganizations();
    }

    onTeamClosed(): void {
        // Optional: handle team panel closed
    }

    onTeamChanged(): void {
        this.loadOrganizations();
    }
}

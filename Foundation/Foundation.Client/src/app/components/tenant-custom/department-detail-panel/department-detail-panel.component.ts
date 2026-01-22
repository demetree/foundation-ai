import { Component, Output, EventEmitter, ViewChild, TemplateRef } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbOffcanvas, NgbOffcanvasRef } from '@ng-bootstrap/ng-bootstrap';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationData } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentData } from '../../../security-data-services/security-department.service';
import { SecurityTeamService, SecurityTeamData, SecurityTeamQueryParameters } from '../../../security-data-services/security-team.service';
import { SecurityUserService, SecurityUserData, SecurityUserQueryParameters } from '../../../security-data-services/security-user.service';
import { TeamDetailPanelComponent } from '../team-detail-panel/team-detail-panel.component';
import { TeamAddEditComponent } from '../team-add-edit/team-add-edit.component';

@Component({
    selector: 'app-department-detail-panel',
    templateUrl: './department-detail-panel.component.html',
    styleUrls: ['./department-detail-panel.component.scss']
})
export class DepartmentDetailPanelComponent {
    @ViewChild('panelTemplate') panelTemplate!: TemplateRef<any>;
    @ViewChild('teamPanel') teamPanel!: TeamDetailPanelComponent;
    @ViewChild('teamAddEdit') teamAddEdit!: TeamAddEditComponent;
    @Output() closed = new EventEmitter<void>();
    @Output() changed = new EventEmitter<void>();

    private destroy$ = new Subject<void>();
    private offcanvasRef: NgbOffcanvasRef | null = null;

    department: SecurityDepartmentData | null = null;
    organization: SecurityOrganizationData | null = null;
    tenant: SecurityTenantData | null = null;
    activeTab: string = 'overview';

    teams$ = new BehaviorSubject<SecurityTeamData[]>([]);
    users$ = new BehaviorSubject<SecurityUserData[]>([]);
    isLoadingTeams$ = new BehaviorSubject<boolean>(false);
    isLoadingUsers$ = new BehaviorSubject<boolean>(false);

    selectedTeam: SecurityTeamData | null = null;

    constructor(
        private offcanvasService: NgbOffcanvas,
        private securityTeamService: SecurityTeamService,
        private securityUserService: SecurityUserService
    ) { }

    open(dept: SecurityDepartmentData, org: SecurityOrganizationData, tenant: SecurityTenantData): void {
        this.department = dept;
        this.organization = org;
        this.tenant = tenant;
        this.activeTab = 'overview';

        this.offcanvasRef = this.offcanvasService.open(this.panelTemplate, {
            position: 'end',
            panelClass: 'dept-detail-panel',
            backdrop: 'static'
        });

        this.offcanvasRef.dismissed.subscribe(() => {
            this.closed.emit();
        });

        this.loadTeams();
        this.loadUsers();
    }

    close(): void {
        if (this.offcanvasRef) {
            this.offcanvasRef.close();
            this.offcanvasRef = null;
        }
        this.closed.emit();
    }

    private loadTeams(): void {
        if (!this.department) return;

        this.isLoadingTeams$.next(true);

        const params = new SecurityTeamQueryParameters();
        params.securityDepartmentId = this.department.id;

        this.securityTeamService.GetSecurityTeamList(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (teams: SecurityTeamData[]) => {
                this.teams$.next(teams);
                this.isLoadingTeams$.next(false);
            },
            error: () => {
                this.teams$.next([]);
                this.isLoadingTeams$.next(false);
            }
        });
    }

    private loadUsers(): void {
        if (!this.department) return;

        this.isLoadingUsers$.next(true);

        const params = new SecurityUserQueryParameters();
        params.securityDepartmentId = this.department.id;

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

    openTeam(team: SecurityTeamData): void {
        this.selectedTeam = team;
        if (this.teamPanel && this.department) {
            this.teamPanel.open(team, this.department, this.organization!, this.tenant!);
        }
    }

    addTeam(): void {
        if (this.teamAddEdit && this.department) {
            this.teamAddEdit.openForCreate(this.department);
        }
    }

    onTeamSaved(team: SecurityTeamData): void {
        this.loadTeams();
        this.changed.emit();
    }

    onTeamClosed(): void {
        this.selectedTeam = null;
    }

    onTeamChanged(): void {
        this.loadTeams();
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

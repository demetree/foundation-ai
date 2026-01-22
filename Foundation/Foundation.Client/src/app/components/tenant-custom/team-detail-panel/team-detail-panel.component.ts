import { Component, Output, EventEmitter, ViewChild, TemplateRef } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgbOffcanvas, NgbOffcanvasRef } from '@ng-bootstrap/ng-bootstrap';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationData } from '../../../security-data-services/security-organization.service';
import { SecurityDepartmentData } from '../../../security-data-services/security-department.service';
import { SecurityTeamData } from '../../../security-data-services/security-team.service';
import { SecurityUserService, SecurityUserData, SecurityUserQueryParameters } from '../../../security-data-services/security-user.service';

@Component({
    selector: 'app-team-detail-panel',
    templateUrl: './team-detail-panel.component.html',
    styleUrls: ['./team-detail-panel.component.scss']
})
export class TeamDetailPanelComponent {
    @ViewChild('panelTemplate') panelTemplate!: TemplateRef<any>;
    @Output() closed = new EventEmitter<void>();
    @Output() changed = new EventEmitter<void>();

    private destroy$ = new Subject<void>();
    private offcanvasRef: NgbOffcanvasRef | null = null;

    team: SecurityTeamData | null = null;
    department: SecurityDepartmentData | null = null;
    organization: SecurityOrganizationData | null = null;
    tenant: SecurityTenantData | null = null;
    activeTab: string = 'overview';

    users$ = new BehaviorSubject<SecurityUserData[]>([]);
    isLoadingUsers$ = new BehaviorSubject<boolean>(false);

    constructor(
        private offcanvasService: NgbOffcanvas,
        private securityUserService: SecurityUserService
    ) { }

    open(team: SecurityTeamData, dept: SecurityDepartmentData, org: SecurityOrganizationData, tenant: SecurityTenantData): void {
        this.team = team;
        this.department = dept;
        this.organization = org;
        this.tenant = tenant;
        this.activeTab = 'overview';

        this.offcanvasRef = this.offcanvasService.open(this.panelTemplate, {
            position: 'end',
            panelClass: 'team-detail-panel',
            backdrop: 'static'
        });

        this.offcanvasRef.dismissed.subscribe(() => {
            this.closed.emit();
        });

        this.loadUsers();
    }

    close(): void {
        if (this.offcanvasRef) {
            this.offcanvasRef.close();
            this.offcanvasRef = null;
        }
        this.closed.emit();
    }

    private loadUsers(): void {
        if (!this.team) return;

        this.isLoadingUsers$.next(true);

        const params = new SecurityUserQueryParameters();
        params.securityTeamId = this.team.id;

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

    setActiveTab(tab: string): void {
        this.activeTab = tab;
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}

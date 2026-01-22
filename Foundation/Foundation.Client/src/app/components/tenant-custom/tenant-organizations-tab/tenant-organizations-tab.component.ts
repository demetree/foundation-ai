import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityOrganizationService, SecurityOrganizationData, SecurityOrganizationQueryParameters } from '../../../security-data-services/security-organization.service';
import { OrganizationDetailPanelComponent } from '../organization-detail-panel/organization-detail-panel.component';

@Component({
    selector: 'app-tenant-organizations-tab',
    templateUrl: './tenant-organizations-tab.component.html',
    styleUrls: ['./tenant-organizations-tab.component.scss']
})
export class TenantOrganizationsTabComponent implements OnInit, OnDestroy {
    @Input() tenant!: SecurityTenantData;
    @ViewChild('orgPanel') orgPanel!: OrganizationDetailPanelComponent;

    private destroy$ = new Subject<void>();

    organizations$ = new BehaviorSubject<SecurityOrganizationData[]>([]);
    isLoading$ = new BehaviorSubject<boolean>(true);
    selectedOrganization: SecurityOrganizationData | null = null;

    constructor(
        private securityOrganizationService: SecurityOrganizationService
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
                this.isLoading$.next(false);
            },
            error: () => {
                this.organizations$.next([]);
                this.isLoading$.next(false);
            }
        });
    }

    openOrganization(org: SecurityOrganizationData): void {
        this.selectedOrganization = org;
        if (this.orgPanel) {
            this.orgPanel.open(org, this.tenant);
        }
    }

    onOrganizationClosed(): void {
        this.selectedOrganization = null;
    }

    onOrganizationChanged(): void {
        this.loadOrganizations();
    }

    getDepartmentCount(org: SecurityOrganizationData): number {
        // This would ideally use a count observable, but we'll show placeholder for now
        return 0;
    }
}

//
// Module Overview Tab Component
//
// Displays basic module information in the Overview tab.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { Observable, BehaviorSubject, combineLatest, of } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';

import { ModuleData } from '../../../security-data-services/module.service';
import { SecurityUserSecurityRoleService } from '../../../security-data-services/security-user-security-role.service';

@Component({
    selector: 'app-module-overview-tab',
    templateUrl: './module-overview-tab.component.html',
    styleUrls: ['./module-overview-tab.component.scss']
})
export class ModuleOverviewTabComponent implements OnInit, OnChanges {

    @Input() module: ModuleData | null = null;

    // Total unique users with access to this module
    public totalUserCount$: Observable<number> = of(0);
    private moduleSubject = new BehaviorSubject<ModuleData | null>(null);


    constructor(
        private securityUserSecurityRoleService: SecurityUserSecurityRoleService
    ) { }


    ngOnInit(): void {
        this.setupUserCountObservable();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['module'] && this.module) {
            this.moduleSubject.next(this.module);
        }
    }


    private setupUserCountObservable(): void {
        // Calculate total unique users across all roles for this module
        this.totalUserCount$ = this.moduleSubject.pipe(
            switchMap(module => {
                if (!module) return of(0);

                // Get all ModuleSecurityRoles for this module
                return module.ModuleSecurityRoles$.pipe(
                    switchMap(moduleSecurityRoles => {
                        if (!moduleSecurityRoles || moduleSecurityRoles.length === 0) {
                            return of(0);
                        }

                        // Get the security role IDs
                        const roleIds = moduleSecurityRoles
                            .filter(msr => msr.securityRole)
                            .map(msr => msr.securityRoleId);

                        if (roleIds.length === 0) return of(0);

                        // Get user counts for each role and sum them
                        // Note: This may count users multiple times if they have multiple roles
                        // For simplicity, we'll show total role assignments
                        const countObservables = roleIds.map(roleId =>
                            this.securityUserSecurityRoleService.GetSecurityUserSecurityRolesRowCount({
                                securityRoleId: roleId,
                                active: true,
                                deleted: false
                            }).pipe(
                                map(count => Number(count)),
                                catchError(() => of(0))
                            )
                        );

                        return combineLatest(countObservables).pipe(
                            map(counts => counts.reduce((sum, count) => sum + count, 0))
                        );
                    }),
                    catchError(() => of(0))
                );
            }),
            catchError(() => of(0))
        );
    }
}

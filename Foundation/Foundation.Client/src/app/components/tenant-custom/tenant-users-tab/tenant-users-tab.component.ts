//
// Tenant Users Tab Component
//
// Displays users associated with this tenant.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';
import { SecurityUserData } from '../../../security-data-services/security-user.service';

@Component({
    selector: 'app-tenant-users-tab',
    templateUrl: './tenant-users-tab.component.html',
    styleUrls: ['./tenant-users-tab.component.scss']
})
export class TenantUsersTabComponent implements OnInit, OnChanges, OnDestroy {

    @Input() tenant: SecurityTenantData | null = null;

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();

    //
    // Data
    //
    public users: SecurityUserData[] = [];
    public isLoading: boolean = true;
    public errorMessage: string | null = null;


    constructor(
        private router: Router
    ) { }


    ngOnInit(): void {
        if (this.tenant) {
            this.loadUsers();
        }
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['tenant'] && this.tenant) {
            this.loadUsers();
        }
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Data Loading
    //

    private loadUsers(): void {
        if (!this.tenant) return;

        this.isLoading = true;
        this.errorMessage = null;

        this.tenant.SecurityUsers
            .then(users => {
                this.users = users || [];
                this.isLoading = false;
            })
            .catch(err => {
                console.error('Error loading tenant users:', err);
                this.errorMessage = 'Failed to load users. Please try again.';
                this.isLoading = false;
            });
    }


    //
    // Navigation
    //

    navigateToUser(user: SecurityUserData): void {
        this.router.navigate(['/user', user.id]);
    }


    //
    // Display Helpers
    //

    getStatusBadgeClass(user: SecurityUserData): string {
        if (user.deleted) return 'badge-deleted';
        return user.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(user: SecurityUserData): string {
        if (user.deleted) return 'Deleted';
        return user.active ? 'Active' : 'Inactive';
    }


    getInitials(user: SecurityUserData): string {
        const firstName = user.firstName || '';
        const lastName = user.lastName || '';

        if (firstName && lastName) {
            return (firstName.charAt(0) + lastName.charAt(0)).toUpperCase();
        }
        if (firstName) return firstName.charAt(0).toUpperCase();
        if (user.accountName) return user.accountName.charAt(0).toUpperCase();
        return '?';
    }


    getDisplayName(user: SecurityUserData): string {
        const firstName = user.firstName || '';
        const lastName = user.lastName || '';

        if (firstName && lastName) {
            return `${firstName} ${lastName}`;
        }
        return user.accountName || 'Unknown';
    }


    trackByUserId(index: number, user: SecurityUserData): number | bigint {
        return user.id;
    }
}

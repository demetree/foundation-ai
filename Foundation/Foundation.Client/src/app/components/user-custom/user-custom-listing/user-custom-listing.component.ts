//
// User Custom Listing Component
//
// Premium listing view for managing Security Users with smart status badges,
// search, and quick actions. Modeled after Scheduler.Client contact-custom-listing pattern.
//

import { Component, OnInit, OnDestroy, HostListener, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, BehaviorSubject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../services/auth.service';
import { SecurityUserService, SecurityUserQueryParameters, SecurityUserData } from '../../../security-data-services/security-user.service';
import { UserCustomAddEditComponent } from '../user-custom-add-edit/user-custom-add-edit.component';
import { UserCustomTableComponent } from '../user-custom-table/user-custom-table.component';

@Component({
    selector: 'app-user-custom-listing',
    templateUrl: './user-custom-listing.component.html',
    styleUrls: ['./user-custom-listing.component.scss']
})
export class UserCustomListingComponent implements OnInit, OnDestroy {

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();

    //
    // Loading and counts
    //
    public loadingTotalCount: boolean = true;
    public loadingFilteredCount: boolean = false;
    public totalUserCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);
    public filteredUserCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);

    //
    // Filter state
    //
    public filterText: string = '';
    private filterTextSubject = new Subject<string>();

    //
    // Responsive state
    //
    public isSmallScreen: boolean = false;
    private readonly SMALL_SCREEN_BREAKPOINT = 768;

    //
    // Add/Edit component reference
    //
    @ViewChild('userAddEdit') userAddEdit!: UserCustomAddEditComponent;
    @ViewChild(UserCustomTableComponent) userTable!: UserCustomTableComponent;


    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
        private authService: AuthService,
        private securityUserService: SecurityUserService
    ) { }


    ngOnInit(): void {
        this.checkScreenSize();
        this.loadTotalCount();

        //
        // Setup debounced filter
        //
        this.filterTextSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(filterText => {
            this.loadFilteredCount(filterText);
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Screen size handling
    //
    @HostListener('window:resize')
    onResize(): void {
        this.checkScreenSize();
    }


    private checkScreenSize(): void {
        this.isSmallScreen = window.innerWidth < this.SMALL_SCREEN_BREAKPOINT;
    }


    //
    // Count loading
    //
    private loadTotalCount(): void {
        this.loadingTotalCount = true;

        const params = new SecurityUserQueryParameters();
        params.deleted = false;

        this.securityUserService.GetSecurityUsersRowCount(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (count) => {
                this.totalUserCount$.next(Number(count));
                this.loadingTotalCount = false;
            },
            error: () => {
                this.totalUserCount$.next(0);
                this.loadingTotalCount = false;
            }
        });
    }


    private loadFilteredCount(filterText: string): void {
        if (filterText == null || filterText.trim() === '') {
            this.filteredUserCount$.next(null);
            this.loadingFilteredCount = false;
            return;
        }

        this.loadingFilteredCount = true;

        const params = new SecurityUserQueryParameters();
        params.deleted = false;
        params.anyStringContains = filterText;

        this.securityUserService.GetSecurityUsersRowCount(params).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (count) => {
                this.filteredUserCount$.next(Number(count));
                this.loadingFilteredCount = false;
            },
            error: () => {
                this.filteredUserCount$.next(0);
                this.loadingFilteredCount = false;
            }
        });
    }


    //
    // Filter handling
    //
    public onFilterChange(): void {
        this.filterTextSubject.next(this.filterText);
    }


    //
    // Navigation
    //
    public goBack(): void {
        this.location.back();
    }


    public canGoBack(): boolean {
        return window.history.length > 1;
    }


    //
    // Permissions
    //
    public userIsSecurityUserReader(): boolean {
        return this.securityUserService.userIsSecuritySecurityUserReader();
    }


    public userIsSecurityUserWriter(): boolean {
        return this.securityUserService.userIsSecuritySecurityUserWriter();
    }


    //
    // Add User
    //
    public addUser(): void {
        if (this.userAddEdit) {
            this.userAddEdit.openModal();
        }
    }


    public onUserChanged(changedUser: SecurityUserData): void {
        // Refresh the count and table after user is added/edited
        this.loadTotalCount();
        if (this.filterText) {
            this.loadFilteredCount(this.filterText);
        }
        // Refresh the table to show updated data
        if (this.userTable) {
            this.userTable.refreshData();
        }
    }


    //
    // Edit User (called from table component)
    //
    public editUser(user: SecurityUserData): void {
        if (this.userAddEdit) {
            this.userAddEdit.openModal(user);
        }
    }
}

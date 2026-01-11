import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SecurityOrganizationUserService, SecurityOrganizationUserData } from '../../../security-data-services/security-organization-user.service';
import { SecurityOrganizationUserAddEditComponent } from '../security-organization-user-add-edit/security-organization-user-add-edit.component';
import { SecurityOrganizationUserTableComponent } from '../security-organization-user-table/security-organization-user-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-security-organization-user-listing',
  templateUrl: './security-organization-user-listing.component.html',
  styleUrls: ['./security-organization-user-listing.component.scss']
})
export class SecurityOrganizationUserListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SecurityOrganizationUserAddEditComponent) addEditSecurityOrganizationUserComponent!: SecurityOrganizationUserAddEditComponent;
  @ViewChild(SecurityOrganizationUserTableComponent) securityOrganizationUserTableComponent!: SecurityOrganizationUserTableComponent;

  public SecurityOrganizationUsers: SecurityOrganizationUserData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSecurityOrganizationUserCount$ : Observable<number> | null = null;
  public filteredSecurityOrganizationUserCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private securityOrganizationUserService: SecurityOrganizationUserService,
              private alertService: AlertService,
              private navigationService: NavigationService,
              private breakpointObserver: BreakpointObserver) { }

  ngOnInit(): void {

    this.breakpointObserver
      .observe(['(max-width: 1100px)']) // this size is specified to try and find a balance so tablets and phone see cards, but wider screens get a table.
      .subscribe((result) => {
        this.isSmallScreen = result.matches;
      });

    this.loadCounts();
  }


  ngAfterViewInit(): void {
    //
    // Subscribe to the securityOrganizationUserChanged observable on the add/edit component so that when a SecurityOrganizationUser changes we can reload the list.
    //
    this.addEditSecurityOrganizationUserComponent.securityOrganizationUserChanged.subscribe({
      next: (result: SecurityOrganizationUserData[] | null) => {
        this.securityOrganizationUserTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Security Organization User changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSecurityOrganizationUserComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSecurityOrganizationUserCount$ = this.securityOrganizationUserService.GetSecurityOrganizationUsersRowCount({
      active: true,
      deleted: false
    }).pipe(
      map(c => Number(c ?? 0)),
      startWith(0),
      finalize(() => {
        this.loadingTotalCount = false;
      }),
      shareReplay(1)
    );

    if (this.filterText) {

      this.filteredSecurityOrganizationUserCount$ = this.securityOrganizationUserService.GetSecurityOrganizationUsersRowCount({
        active: true,
        deleted: false,
        anyStringContains: this.filterText || undefined
      }).pipe(
        map(c => Number(c ?? 0)),
        startWith(0),
        finalize(() => {
          this.loadingFilteredCount = false;
        }),
        shareReplay(1)
      )
    } else {

      this.filteredSecurityOrganizationUserCount$ = this.totalSecurityOrganizationUserCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSecurityOrganizationUserCount$.subscribe();

    if (this.filteredSecurityOrganizationUserCount$ != this.totalSecurityOrganizationUserCount$) {
      this.filteredSecurityOrganizationUserCount$.subscribe();
    }
  }


  public goBack(): void {
    this.navigationService.goBack();
   }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public clearFilter() {
    this.filterText = '';
  }


  //
  // Update the counts when the filter change
  //
  public onFilterChange(): void {

    clearTimeout(this.debounceTimeout);

    this.debounceTimeout = setTimeout(() => {
      this.securityOrganizationUserTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecuritySecurityOrganizationUserReader(): boolean {
    return this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserReader();
  }

  public userIsSecuritySecurityOrganizationUserWriter(): boolean {
    return this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserWriter();
  }
}

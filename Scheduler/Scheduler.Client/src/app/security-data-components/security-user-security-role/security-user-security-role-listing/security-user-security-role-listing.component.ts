/*
   GENERATED FORM FOR THE SECURITYUSERSECURITYROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUserSecurityRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-security-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SecurityUserSecurityRoleService, SecurityUserSecurityRoleData } from '../../../security-data-services/security-user-security-role.service';
import { SecurityUserSecurityRoleAddEditComponent } from '../security-user-security-role-add-edit/security-user-security-role-add-edit.component';
import { SecurityUserSecurityRoleTableComponent } from '../security-user-security-role-table/security-user-security-role-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-security-user-security-role-listing',
  templateUrl: './security-user-security-role-listing.component.html',
  styleUrls: ['./security-user-security-role-listing.component.scss']
})
export class SecurityUserSecurityRoleListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SecurityUserSecurityRoleAddEditComponent) addEditSecurityUserSecurityRoleComponent!: SecurityUserSecurityRoleAddEditComponent;
  @ViewChild(SecurityUserSecurityRoleTableComponent) securityUserSecurityRoleTableComponent!: SecurityUserSecurityRoleTableComponent;

  public SecurityUserSecurityRoles: SecurityUserSecurityRoleData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSecurityUserSecurityRoleCount$ : Observable<number> | null = null;
  public filteredSecurityUserSecurityRoleCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private securityUserSecurityRoleService: SecurityUserSecurityRoleService,
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
    // Subscribe to the securityUserSecurityRoleChanged observable on the add/edit component so that when a SecurityUserSecurityRole changes we can reload the list.
    //
    this.addEditSecurityUserSecurityRoleComponent.securityUserSecurityRoleChanged.subscribe({
      next: (result: SecurityUserSecurityRoleData[] | null) => {
        this.securityUserSecurityRoleTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Security User Security Role changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSecurityUserSecurityRoleComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSecurityUserSecurityRoleCount$ = this.securityUserSecurityRoleService.GetSecurityUserSecurityRolesRowCount({
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

      this.filteredSecurityUserSecurityRoleCount$ = this.securityUserSecurityRoleService.GetSecurityUserSecurityRolesRowCount({
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

      this.filteredSecurityUserSecurityRoleCount$ = this.totalSecurityUserSecurityRoleCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSecurityUserSecurityRoleCount$.subscribe();

    if (this.filteredSecurityUserSecurityRoleCount$ != this.totalSecurityUserSecurityRoleCount$) {
      this.filteredSecurityUserSecurityRoleCount$.subscribe();
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
      this.securityUserSecurityRoleTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecuritySecurityUserSecurityRoleReader(): boolean {
    return this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleReader();
  }

  public userIsSecuritySecurityUserSecurityRoleWriter(): boolean {
    return this.securityUserSecurityRoleService.userIsSecuritySecurityUserSecurityRoleWriter();
  }
}

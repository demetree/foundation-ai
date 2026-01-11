import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SecurityRoleService, SecurityRoleData } from '../../../security-data-services/security-role.service';
import { SecurityRoleAddEditComponent } from '../security-role-add-edit/security-role-add-edit.component';
import { SecurityRoleTableComponent } from '../security-role-table/security-role-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-security-role-listing',
  templateUrl: './security-role-listing.component.html',
  styleUrls: ['./security-role-listing.component.scss']
})
export class SecurityRoleListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SecurityRoleAddEditComponent) addEditSecurityRoleComponent!: SecurityRoleAddEditComponent;
  @ViewChild(SecurityRoleTableComponent) securityRoleTableComponent!: SecurityRoleTableComponent;

  public SecurityRoles: SecurityRoleData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSecurityRoleCount$ : Observable<number> | null = null;
  public filteredSecurityRoleCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private securityRoleService: SecurityRoleService,
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
    // Subscribe to the securityRoleChanged observable on the add/edit component so that when a SecurityRole changes we can reload the list.
    //
    this.addEditSecurityRoleComponent.securityRoleChanged.subscribe({
      next: (result: SecurityRoleData[] | null) => {
        this.securityRoleTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Security Role changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSecurityRoleComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSecurityRoleCount$ = this.securityRoleService.GetSecurityRolesRowCount({
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

      this.filteredSecurityRoleCount$ = this.securityRoleService.GetSecurityRolesRowCount({
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

      this.filteredSecurityRoleCount$ = this.totalSecurityRoleCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSecurityRoleCount$.subscribe();

    if (this.filteredSecurityRoleCount$ != this.totalSecurityRoleCount$) {
      this.filteredSecurityRoleCount$.subscribe();
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
      this.securityRoleTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecuritySecurityRoleReader(): boolean {
    return this.securityRoleService.userIsSecuritySecurityRoleReader();
  }

  public userIsSecuritySecurityRoleWriter(): boolean {
    return this.securityRoleService.userIsSecuritySecurityRoleWriter();
  }
}

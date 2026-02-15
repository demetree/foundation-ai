/*
   GENERATED FORM FOR THE USERPROFILELINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { UserProfileLinkService, UserProfileLinkData } from '../../../bmc-data-services/user-profile-link.service';
import { UserProfileLinkAddEditComponent } from '../user-profile-link-add-edit/user-profile-link-add-edit.component';
import { UserProfileLinkTableComponent } from '../user-profile-link-table/user-profile-link-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-user-profile-link-listing',
  templateUrl: './user-profile-link-listing.component.html',
  styleUrls: ['./user-profile-link-listing.component.scss']
})
export class UserProfileLinkListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(UserProfileLinkAddEditComponent) addEditUserProfileLinkComponent!: UserProfileLinkAddEditComponent;
  @ViewChild(UserProfileLinkTableComponent) userProfileLinkTableComponent!: UserProfileLinkTableComponent;

  public UserProfileLinks: UserProfileLinkData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalUserProfileLinkCount$ : Observable<number> | null = null;
  public filteredUserProfileLinkCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private userProfileLinkService: UserProfileLinkService,
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
    // Subscribe to the userProfileLinkChanged observable on the add/edit component so that when a UserProfileLink changes we can reload the list.
    //
    this.addEditUserProfileLinkComponent.userProfileLinkChanged.subscribe({
      next: (result: UserProfileLinkData[] | null) => {
        this.userProfileLinkTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during User Profile Link changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditUserProfileLinkComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalUserProfileLinkCount$ = this.userProfileLinkService.GetUserProfileLinksRowCount({
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

      this.filteredUserProfileLinkCount$ = this.userProfileLinkService.GetUserProfileLinksRowCount({
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

      this.filteredUserProfileLinkCount$ = this.totalUserProfileLinkCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalUserProfileLinkCount$.subscribe();

    if (this.filteredUserProfileLinkCount$ != this.totalUserProfileLinkCount$) {
      this.filteredUserProfileLinkCount$.subscribe();
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
      this.userProfileLinkTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCUserProfileLinkReader(): boolean {
    return this.userProfileLinkService.userIsBMCUserProfileLinkReader();
  }

  public userIsBMCUserProfileLinkWriter(): boolean {
    return this.userProfileLinkService.userIsBMCUserProfileLinkWriter();
  }
}

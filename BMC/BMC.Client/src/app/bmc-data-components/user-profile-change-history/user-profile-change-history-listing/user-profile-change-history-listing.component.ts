/*
   GENERATED FORM FOR THE USERPROFILECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { UserProfileChangeHistoryService, UserProfileChangeHistoryData } from '../../../bmc-data-services/user-profile-change-history.service';
import { UserProfileChangeHistoryAddEditComponent } from '../user-profile-change-history-add-edit/user-profile-change-history-add-edit.component';
import { UserProfileChangeHistoryTableComponent } from '../user-profile-change-history-table/user-profile-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-user-profile-change-history-listing',
  templateUrl: './user-profile-change-history-listing.component.html',
  styleUrls: ['./user-profile-change-history-listing.component.scss']
})
export class UserProfileChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(UserProfileChangeHistoryAddEditComponent) addEditUserProfileChangeHistoryComponent!: UserProfileChangeHistoryAddEditComponent;
  @ViewChild(UserProfileChangeHistoryTableComponent) userProfileChangeHistoryTableComponent!: UserProfileChangeHistoryTableComponent;

  public UserProfileChangeHistories: UserProfileChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalUserProfileChangeHistoryCount$ : Observable<number> | null = null;
  public filteredUserProfileChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private userProfileChangeHistoryService: UserProfileChangeHistoryService,
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
    // Subscribe to the userProfileChangeHistoryChanged observable on the add/edit component so that when a UserProfileChangeHistory changes we can reload the list.
    //
    this.addEditUserProfileChangeHistoryComponent.userProfileChangeHistoryChanged.subscribe({
      next: (result: UserProfileChangeHistoryData[] | null) => {
        this.userProfileChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during User Profile Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditUserProfileChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalUserProfileChangeHistoryCount$ = this.userProfileChangeHistoryService.GetUserProfileChangeHistoriesRowCount({
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

      this.filteredUserProfileChangeHistoryCount$ = this.userProfileChangeHistoryService.GetUserProfileChangeHistoriesRowCount({
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

      this.filteredUserProfileChangeHistoryCount$ = this.totalUserProfileChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalUserProfileChangeHistoryCount$.subscribe();

    if (this.filteredUserProfileChangeHistoryCount$ != this.totalUserProfileChangeHistoryCount$) {
      this.filteredUserProfileChangeHistoryCount$.subscribe();
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
      this.userProfileChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.userProfileChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCUserProfileChangeHistoryReader(): boolean {
    return this.userProfileChangeHistoryService.userIsBMCUserProfileChangeHistoryReader();
  }

  public userIsBMCUserProfileChangeHistoryWriter(): boolean {
    return this.userProfileChangeHistoryService.userIsBMCUserProfileChangeHistoryWriter();
  }
}

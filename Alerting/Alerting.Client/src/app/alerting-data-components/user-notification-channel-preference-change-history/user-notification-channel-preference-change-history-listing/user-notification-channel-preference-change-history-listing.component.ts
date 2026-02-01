/*
   GENERATED FORM FOR THE USERNOTIFICATIONCHANNELPREFERENCECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserNotificationChannelPreferenceChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-notification-channel-preference-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { UserNotificationChannelPreferenceChangeHistoryService, UserNotificationChannelPreferenceChangeHistoryData } from '../../../alerting-data-services/user-notification-channel-preference-change-history.service';
import { UserNotificationChannelPreferenceChangeHistoryAddEditComponent } from '../user-notification-channel-preference-change-history-add-edit/user-notification-channel-preference-change-history-add-edit.component';
import { UserNotificationChannelPreferenceChangeHistoryTableComponent } from '../user-notification-channel-preference-change-history-table/user-notification-channel-preference-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-user-notification-channel-preference-change-history-listing',
  templateUrl: './user-notification-channel-preference-change-history-listing.component.html',
  styleUrls: ['./user-notification-channel-preference-change-history-listing.component.scss']
})
export class UserNotificationChannelPreferenceChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(UserNotificationChannelPreferenceChangeHistoryAddEditComponent) addEditUserNotificationChannelPreferenceChangeHistoryComponent!: UserNotificationChannelPreferenceChangeHistoryAddEditComponent;
  @ViewChild(UserNotificationChannelPreferenceChangeHistoryTableComponent) userNotificationChannelPreferenceChangeHistoryTableComponent!: UserNotificationChannelPreferenceChangeHistoryTableComponent;

  public UserNotificationChannelPreferenceChangeHistories: UserNotificationChannelPreferenceChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalUserNotificationChannelPreferenceChangeHistoryCount$ : Observable<number> | null = null;
  public filteredUserNotificationChannelPreferenceChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private userNotificationChannelPreferenceChangeHistoryService: UserNotificationChannelPreferenceChangeHistoryService,
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
    // Subscribe to the userNotificationChannelPreferenceChangeHistoryChanged observable on the add/edit component so that when a UserNotificationChannelPreferenceChangeHistory changes we can reload the list.
    //
    this.addEditUserNotificationChannelPreferenceChangeHistoryComponent.userNotificationChannelPreferenceChangeHistoryChanged.subscribe({
      next: (result: UserNotificationChannelPreferenceChangeHistoryData[] | null) => {
        this.userNotificationChannelPreferenceChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during User Notification Channel Preference Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditUserNotificationChannelPreferenceChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalUserNotificationChannelPreferenceChangeHistoryCount$ = this.userNotificationChannelPreferenceChangeHistoryService.GetUserNotificationChannelPreferenceChangeHistoriesRowCount({
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

      this.filteredUserNotificationChannelPreferenceChangeHistoryCount$ = this.userNotificationChannelPreferenceChangeHistoryService.GetUserNotificationChannelPreferenceChangeHistoriesRowCount({
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

      this.filteredUserNotificationChannelPreferenceChangeHistoryCount$ = this.totalUserNotificationChannelPreferenceChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalUserNotificationChannelPreferenceChangeHistoryCount$.subscribe();

    if (this.filteredUserNotificationChannelPreferenceChangeHistoryCount$ != this.totalUserNotificationChannelPreferenceChangeHistoryCount$) {
      this.filteredUserNotificationChannelPreferenceChangeHistoryCount$.subscribe();
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
      this.userNotificationChannelPreferenceChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader(): boolean {
    return this.userNotificationChannelPreferenceChangeHistoryService.userIsAlertingUserNotificationChannelPreferenceChangeHistoryReader();
  }

  public userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter(): boolean {
    return this.userNotificationChannelPreferenceChangeHistoryService.userIsAlertingUserNotificationChannelPreferenceChangeHistoryWriter();
  }
}

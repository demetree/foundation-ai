/*
   GENERATED FORM FOR THE NOTIFICATIONSUBSCRIPTIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationSubscriptionChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-subscription-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { NotificationSubscriptionChangeHistoryService, NotificationSubscriptionChangeHistoryData } from '../../../scheduler-data-services/notification-subscription-change-history.service';
import { NotificationSubscriptionChangeHistoryAddEditComponent } from '../notification-subscription-change-history-add-edit/notification-subscription-change-history-add-edit.component';
import { NotificationSubscriptionChangeHistoryTableComponent } from '../notification-subscription-change-history-table/notification-subscription-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-notification-subscription-change-history-listing',
  templateUrl: './notification-subscription-change-history-listing.component.html',
  styleUrls: ['./notification-subscription-change-history-listing.component.scss']
})
export class NotificationSubscriptionChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(NotificationSubscriptionChangeHistoryAddEditComponent) addEditNotificationSubscriptionChangeHistoryComponent!: NotificationSubscriptionChangeHistoryAddEditComponent;
  @ViewChild(NotificationSubscriptionChangeHistoryTableComponent) notificationSubscriptionChangeHistoryTableComponent!: NotificationSubscriptionChangeHistoryTableComponent;

  public NotificationSubscriptionChangeHistories: NotificationSubscriptionChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalNotificationSubscriptionChangeHistoryCount$ : Observable<number> | null = null;
  public filteredNotificationSubscriptionChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private notificationSubscriptionChangeHistoryService: NotificationSubscriptionChangeHistoryService,
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
    // Subscribe to the notificationSubscriptionChangeHistoryChanged observable on the add/edit component so that when a NotificationSubscriptionChangeHistory changes we can reload the list.
    //
    this.addEditNotificationSubscriptionChangeHistoryComponent.notificationSubscriptionChangeHistoryChanged.subscribe({
      next: (result: NotificationSubscriptionChangeHistoryData[] | null) => {
        this.notificationSubscriptionChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Notification Subscription Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditNotificationSubscriptionChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalNotificationSubscriptionChangeHistoryCount$ = this.notificationSubscriptionChangeHistoryService.GetNotificationSubscriptionChangeHistoriesRowCount({
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

      this.filteredNotificationSubscriptionChangeHistoryCount$ = this.notificationSubscriptionChangeHistoryService.GetNotificationSubscriptionChangeHistoriesRowCount({
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

      this.filteredNotificationSubscriptionChangeHistoryCount$ = this.totalNotificationSubscriptionChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalNotificationSubscriptionChangeHistoryCount$.subscribe();

    if (this.filteredNotificationSubscriptionChangeHistoryCount$ != this.totalNotificationSubscriptionChangeHistoryCount$) {
      this.filteredNotificationSubscriptionChangeHistoryCount$.subscribe();
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
      this.notificationSubscriptionChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.notificationSubscriptionChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerNotificationSubscriptionChangeHistoryReader(): boolean {
    return this.notificationSubscriptionChangeHistoryService.userIsSchedulerNotificationSubscriptionChangeHistoryReader();
  }

  public userIsSchedulerNotificationSubscriptionChangeHistoryWriter(): boolean {
    return this.notificationSubscriptionChangeHistoryService.userIsSchedulerNotificationSubscriptionChangeHistoryWriter();
  }
}

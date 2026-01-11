/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplateChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ScheduledEventTemplateChangeHistoryService, ScheduledEventTemplateChangeHistoryData } from '../../../scheduler-data-services/scheduled-event-template-change-history.service';
import { ScheduledEventTemplateChangeHistoryAddEditComponent } from '../scheduled-event-template-change-history-add-edit/scheduled-event-template-change-history-add-edit.component';
import { ScheduledEventTemplateChangeHistoryTableComponent } from '../scheduled-event-template-change-history-table/scheduled-event-template-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-scheduled-event-template-change-history-listing',
  templateUrl: './scheduled-event-template-change-history-listing.component.html',
  styleUrls: ['./scheduled-event-template-change-history-listing.component.scss']
})
export class ScheduledEventTemplateChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ScheduledEventTemplateChangeHistoryAddEditComponent) addEditScheduledEventTemplateChangeHistoryComponent!: ScheduledEventTemplateChangeHistoryAddEditComponent;
  @ViewChild(ScheduledEventTemplateChangeHistoryTableComponent) scheduledEventTemplateChangeHistoryTableComponent!: ScheduledEventTemplateChangeHistoryTableComponent;

  public ScheduledEventTemplateChangeHistories: ScheduledEventTemplateChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalScheduledEventTemplateChangeHistoryCount$ : Observable<number> | null = null;
  public filteredScheduledEventTemplateChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private scheduledEventTemplateChangeHistoryService: ScheduledEventTemplateChangeHistoryService,
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
    // Subscribe to the scheduledEventTemplateChangeHistoryChanged observable on the add/edit component so that when a ScheduledEventTemplateChangeHistory changes we can reload the list.
    //
    this.addEditScheduledEventTemplateChangeHistoryComponent.scheduledEventTemplateChangeHistoryChanged.subscribe({
      next: (result: ScheduledEventTemplateChangeHistoryData[] | null) => {
        this.scheduledEventTemplateChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Scheduled Event Template Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditScheduledEventTemplateChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalScheduledEventTemplateChangeHistoryCount$ = this.scheduledEventTemplateChangeHistoryService.GetScheduledEventTemplateChangeHistoriesRowCount({
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

      this.filteredScheduledEventTemplateChangeHistoryCount$ = this.scheduledEventTemplateChangeHistoryService.GetScheduledEventTemplateChangeHistoriesRowCount({
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

      this.filteredScheduledEventTemplateChangeHistoryCount$ = this.totalScheduledEventTemplateChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalScheduledEventTemplateChangeHistoryCount$.subscribe();

    if (this.filteredScheduledEventTemplateChangeHistoryCount$ != this.totalScheduledEventTemplateChangeHistoryCount$) {
      this.filteredScheduledEventTemplateChangeHistoryCount$.subscribe();
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
      this.scheduledEventTemplateChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerScheduledEventTemplateChangeHistoryReader(): boolean {
    return this.scheduledEventTemplateChangeHistoryService.userIsSchedulerScheduledEventTemplateChangeHistoryReader();
  }

  public userIsSchedulerScheduledEventTemplateChangeHistoryWriter(): boolean {
    return this.scheduledEventTemplateChangeHistoryService.userIsSchedulerScheduledEventTemplateChangeHistoryWriter();
  }
}

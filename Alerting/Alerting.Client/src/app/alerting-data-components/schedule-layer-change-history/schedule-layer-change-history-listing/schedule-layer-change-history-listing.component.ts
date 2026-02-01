/*
   GENERATED FORM FOR THE SCHEDULELAYERCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleLayerChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-layer-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ScheduleLayerChangeHistoryService, ScheduleLayerChangeHistoryData } from '../../../alerting-data-services/schedule-layer-change-history.service';
import { ScheduleLayerChangeHistoryAddEditComponent } from '../schedule-layer-change-history-add-edit/schedule-layer-change-history-add-edit.component';
import { ScheduleLayerChangeHistoryTableComponent } from '../schedule-layer-change-history-table/schedule-layer-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-schedule-layer-change-history-listing',
  templateUrl: './schedule-layer-change-history-listing.component.html',
  styleUrls: ['./schedule-layer-change-history-listing.component.scss']
})
export class ScheduleLayerChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ScheduleLayerChangeHistoryAddEditComponent) addEditScheduleLayerChangeHistoryComponent!: ScheduleLayerChangeHistoryAddEditComponent;
  @ViewChild(ScheduleLayerChangeHistoryTableComponent) scheduleLayerChangeHistoryTableComponent!: ScheduleLayerChangeHistoryTableComponent;

  public ScheduleLayerChangeHistories: ScheduleLayerChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalScheduleLayerChangeHistoryCount$ : Observable<number> | null = null;
  public filteredScheduleLayerChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private scheduleLayerChangeHistoryService: ScheduleLayerChangeHistoryService,
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
    // Subscribe to the scheduleLayerChangeHistoryChanged observable on the add/edit component so that when a ScheduleLayerChangeHistory changes we can reload the list.
    //
    this.addEditScheduleLayerChangeHistoryComponent.scheduleLayerChangeHistoryChanged.subscribe({
      next: (result: ScheduleLayerChangeHistoryData[] | null) => {
        this.scheduleLayerChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Schedule Layer Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditScheduleLayerChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalScheduleLayerChangeHistoryCount$ = this.scheduleLayerChangeHistoryService.GetScheduleLayerChangeHistoriesRowCount({
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

      this.filteredScheduleLayerChangeHistoryCount$ = this.scheduleLayerChangeHistoryService.GetScheduleLayerChangeHistoriesRowCount({
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

      this.filteredScheduleLayerChangeHistoryCount$ = this.totalScheduleLayerChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalScheduleLayerChangeHistoryCount$.subscribe();

    if (this.filteredScheduleLayerChangeHistoryCount$ != this.totalScheduleLayerChangeHistoryCount$) {
      this.filteredScheduleLayerChangeHistoryCount$.subscribe();
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
      this.scheduleLayerChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAlertingScheduleLayerChangeHistoryReader(): boolean {
    return this.scheduleLayerChangeHistoryService.userIsAlertingScheduleLayerChangeHistoryReader();
  }

  public userIsAlertingScheduleLayerChangeHistoryWriter(): boolean {
    return this.scheduleLayerChangeHistoryService.userIsAlertingScheduleLayerChangeHistoryWriter();
  }
}

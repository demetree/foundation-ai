/*
   GENERATED FORM FOR THE RATESHEETCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RateSheetChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rate-sheet-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { RateSheetChangeHistoryService, RateSheetChangeHistoryData } from '../../../scheduler-data-services/rate-sheet-change-history.service';
import { RateSheetChangeHistoryAddEditComponent } from '../rate-sheet-change-history-add-edit/rate-sheet-change-history-add-edit.component';
import { RateSheetChangeHistoryTableComponent } from '../rate-sheet-change-history-table/rate-sheet-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-rate-sheet-change-history-listing',
  templateUrl: './rate-sheet-change-history-listing.component.html',
  styleUrls: ['./rate-sheet-change-history-listing.component.scss']
})
export class RateSheetChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(RateSheetChangeHistoryAddEditComponent) addEditRateSheetChangeHistoryComponent!: RateSheetChangeHistoryAddEditComponent;
  @ViewChild(RateSheetChangeHistoryTableComponent) rateSheetChangeHistoryTableComponent!: RateSheetChangeHistoryTableComponent;

  public RateSheetChangeHistories: RateSheetChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalRateSheetChangeHistoryCount$ : Observable<number> | null = null;
  public filteredRateSheetChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private rateSheetChangeHistoryService: RateSheetChangeHistoryService,
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
    // Subscribe to the rateSheetChangeHistoryChanged observable on the add/edit component so that when a RateSheetChangeHistory changes we can reload the list.
    //
    this.addEditRateSheetChangeHistoryComponent.rateSheetChangeHistoryChanged.subscribe({
      next: (result: RateSheetChangeHistoryData[] | null) => {
        this.rateSheetChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Rate Sheet Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditRateSheetChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalRateSheetChangeHistoryCount$ = this.rateSheetChangeHistoryService.GetRateSheetChangeHistoriesRowCount({
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

      this.filteredRateSheetChangeHistoryCount$ = this.rateSheetChangeHistoryService.GetRateSheetChangeHistoriesRowCount({
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

      this.filteredRateSheetChangeHistoryCount$ = this.totalRateSheetChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalRateSheetChangeHistoryCount$.subscribe();

    if (this.filteredRateSheetChangeHistoryCount$ != this.totalRateSheetChangeHistoryCount$) {
      this.filteredRateSheetChangeHistoryCount$.subscribe();
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
      this.rateSheetChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerRateSheetChangeHistoryReader(): boolean {
    return this.rateSheetChangeHistoryService.userIsSchedulerRateSheetChangeHistoryReader();
  }

  public userIsSchedulerRateSheetChangeHistoryWriter(): boolean {
    return this.rateSheetChangeHistoryService.userIsSchedulerRateSheetChangeHistoryWriter();
  }
}

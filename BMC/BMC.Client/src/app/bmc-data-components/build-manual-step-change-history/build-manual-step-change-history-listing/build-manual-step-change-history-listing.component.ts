/*
   GENERATED FORM FOR THE BUILDMANUALSTEPCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManualStepChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-step-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { BuildManualStepChangeHistoryService, BuildManualStepChangeHistoryData } from '../../../bmc-data-services/build-manual-step-change-history.service';
import { BuildManualStepChangeHistoryAddEditComponent } from '../build-manual-step-change-history-add-edit/build-manual-step-change-history-add-edit.component';
import { BuildManualStepChangeHistoryTableComponent } from '../build-manual-step-change-history-table/build-manual-step-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-build-manual-step-change-history-listing',
  templateUrl: './build-manual-step-change-history-listing.component.html',
  styleUrls: ['./build-manual-step-change-history-listing.component.scss']
})
export class BuildManualStepChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(BuildManualStepChangeHistoryAddEditComponent) addEditBuildManualStepChangeHistoryComponent!: BuildManualStepChangeHistoryAddEditComponent;
  @ViewChild(BuildManualStepChangeHistoryTableComponent) buildManualStepChangeHistoryTableComponent!: BuildManualStepChangeHistoryTableComponent;

  public BuildManualStepChangeHistories: BuildManualStepChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalBuildManualStepChangeHistoryCount$ : Observable<number> | null = null;
  public filteredBuildManualStepChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private buildManualStepChangeHistoryService: BuildManualStepChangeHistoryService,
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
    // Subscribe to the buildManualStepChangeHistoryChanged observable on the add/edit component so that when a BuildManualStepChangeHistory changes we can reload the list.
    //
    this.addEditBuildManualStepChangeHistoryComponent.buildManualStepChangeHistoryChanged.subscribe({
      next: (result: BuildManualStepChangeHistoryData[] | null) => {
        this.buildManualStepChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Build Manual Step Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditBuildManualStepChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalBuildManualStepChangeHistoryCount$ = this.buildManualStepChangeHistoryService.GetBuildManualStepChangeHistoriesRowCount({
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

      this.filteredBuildManualStepChangeHistoryCount$ = this.buildManualStepChangeHistoryService.GetBuildManualStepChangeHistoriesRowCount({
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

      this.filteredBuildManualStepChangeHistoryCount$ = this.totalBuildManualStepChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalBuildManualStepChangeHistoryCount$.subscribe();

    if (this.filteredBuildManualStepChangeHistoryCount$ != this.totalBuildManualStepChangeHistoryCount$) {
      this.filteredBuildManualStepChangeHistoryCount$.subscribe();
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
      this.buildManualStepChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.buildManualStepChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCBuildManualStepChangeHistoryReader(): boolean {
    return this.buildManualStepChangeHistoryService.userIsBMCBuildManualStepChangeHistoryReader();
  }

  public userIsBMCBuildManualStepChangeHistoryWriter(): boolean {
    return this.buildManualStepChangeHistoryService.userIsBMCBuildManualStepChangeHistoryWriter();
  }
}

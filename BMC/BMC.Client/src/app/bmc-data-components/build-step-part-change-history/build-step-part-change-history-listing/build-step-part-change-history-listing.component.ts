/*
   GENERATED FORM FOR THE BUILDSTEPPARTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildStepPartChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-step-part-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { BuildStepPartChangeHistoryService, BuildStepPartChangeHistoryData } from '../../../bmc-data-services/build-step-part-change-history.service';
import { BuildStepPartChangeHistoryAddEditComponent } from '../build-step-part-change-history-add-edit/build-step-part-change-history-add-edit.component';
import { BuildStepPartChangeHistoryTableComponent } from '../build-step-part-change-history-table/build-step-part-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-build-step-part-change-history-listing',
  templateUrl: './build-step-part-change-history-listing.component.html',
  styleUrls: ['./build-step-part-change-history-listing.component.scss']
})
export class BuildStepPartChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(BuildStepPartChangeHistoryAddEditComponent) addEditBuildStepPartChangeHistoryComponent!: BuildStepPartChangeHistoryAddEditComponent;
  @ViewChild(BuildStepPartChangeHistoryTableComponent) buildStepPartChangeHistoryTableComponent!: BuildStepPartChangeHistoryTableComponent;

  public BuildStepPartChangeHistories: BuildStepPartChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalBuildStepPartChangeHistoryCount$ : Observable<number> | null = null;
  public filteredBuildStepPartChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private buildStepPartChangeHistoryService: BuildStepPartChangeHistoryService,
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
    // Subscribe to the buildStepPartChangeHistoryChanged observable on the add/edit component so that when a BuildStepPartChangeHistory changes we can reload the list.
    //
    this.addEditBuildStepPartChangeHistoryComponent.buildStepPartChangeHistoryChanged.subscribe({
      next: (result: BuildStepPartChangeHistoryData[] | null) => {
        this.buildStepPartChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Build Step Part Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditBuildStepPartChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalBuildStepPartChangeHistoryCount$ = this.buildStepPartChangeHistoryService.GetBuildStepPartChangeHistoriesRowCount({
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

      this.filteredBuildStepPartChangeHistoryCount$ = this.buildStepPartChangeHistoryService.GetBuildStepPartChangeHistoriesRowCount({
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

      this.filteredBuildStepPartChangeHistoryCount$ = this.totalBuildStepPartChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalBuildStepPartChangeHistoryCount$.subscribe();

    if (this.filteredBuildStepPartChangeHistoryCount$ != this.totalBuildStepPartChangeHistoryCount$) {
      this.filteredBuildStepPartChangeHistoryCount$.subscribe();
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
      this.buildStepPartChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.buildStepPartChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCBuildStepPartChangeHistoryReader(): boolean {
    return this.buildStepPartChangeHistoryService.userIsBMCBuildStepPartChangeHistoryReader();
  }

  public userIsBMCBuildStepPartChangeHistoryWriter(): boolean {
    return this.buildStepPartChangeHistoryService.userIsBMCBuildStepPartChangeHistoryWriter();
  }
}

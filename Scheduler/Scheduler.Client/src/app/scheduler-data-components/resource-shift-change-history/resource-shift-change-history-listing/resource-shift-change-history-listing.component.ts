/*
   GENERATED FORM FOR THE RESOURCESHIFTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceShiftChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-shift-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ResourceShiftChangeHistoryService, ResourceShiftChangeHistoryData } from '../../../scheduler-data-services/resource-shift-change-history.service';
import { ResourceShiftChangeHistoryAddEditComponent } from '../resource-shift-change-history-add-edit/resource-shift-change-history-add-edit.component';
import { ResourceShiftChangeHistoryTableComponent } from '../resource-shift-change-history-table/resource-shift-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-resource-shift-change-history-listing',
  templateUrl: './resource-shift-change-history-listing.component.html',
  styleUrls: ['./resource-shift-change-history-listing.component.scss']
})
export class ResourceShiftChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ResourceShiftChangeHistoryAddEditComponent) addEditResourceShiftChangeHistoryComponent!: ResourceShiftChangeHistoryAddEditComponent;
  @ViewChild(ResourceShiftChangeHistoryTableComponent) resourceShiftChangeHistoryTableComponent!: ResourceShiftChangeHistoryTableComponent;

  public ResourceShiftChangeHistories: ResourceShiftChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalResourceShiftChangeHistoryCount$ : Observable<number> | null = null;
  public filteredResourceShiftChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private resourceShiftChangeHistoryService: ResourceShiftChangeHistoryService,
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
    // Subscribe to the resourceShiftChangeHistoryChanged observable on the add/edit component so that when a ResourceShiftChangeHistory changes we can reload the list.
    //
    this.addEditResourceShiftChangeHistoryComponent.resourceShiftChangeHistoryChanged.subscribe({
      next: (result: ResourceShiftChangeHistoryData[] | null) => {
        this.resourceShiftChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Resource Shift Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditResourceShiftChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalResourceShiftChangeHistoryCount$ = this.resourceShiftChangeHistoryService.GetResourceShiftChangeHistoriesRowCount({
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

      this.filteredResourceShiftChangeHistoryCount$ = this.resourceShiftChangeHistoryService.GetResourceShiftChangeHistoriesRowCount({
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

      this.filteredResourceShiftChangeHistoryCount$ = this.totalResourceShiftChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalResourceShiftChangeHistoryCount$.subscribe();

    if (this.filteredResourceShiftChangeHistoryCount$ != this.totalResourceShiftChangeHistoryCount$) {
      this.filteredResourceShiftChangeHistoryCount$.subscribe();
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
      this.resourceShiftChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerResourceShiftChangeHistoryReader(): boolean {
    return this.resourceShiftChangeHistoryService.userIsSchedulerResourceShiftChangeHistoryReader();
  }

  public userIsSchedulerResourceShiftChangeHistoryWriter(): boolean {
    return this.resourceShiftChangeHistoryService.userIsSchedulerResourceShiftChangeHistoryWriter();
  }
}

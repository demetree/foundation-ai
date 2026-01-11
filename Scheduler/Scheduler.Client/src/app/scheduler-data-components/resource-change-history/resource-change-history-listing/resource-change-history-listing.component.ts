/*
   GENERATED FORM FOR THE RESOURCECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ResourceChangeHistoryService, ResourceChangeHistoryData } from '../../../scheduler-data-services/resource-change-history.service';
import { ResourceChangeHistoryAddEditComponent } from '../resource-change-history-add-edit/resource-change-history-add-edit.component';
import { ResourceChangeHistoryTableComponent } from '../resource-change-history-table/resource-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-resource-change-history-listing',
  templateUrl: './resource-change-history-listing.component.html',
  styleUrls: ['./resource-change-history-listing.component.scss']
})
export class ResourceChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ResourceChangeHistoryAddEditComponent) addEditResourceChangeHistoryComponent!: ResourceChangeHistoryAddEditComponent;
  @ViewChild(ResourceChangeHistoryTableComponent) resourceChangeHistoryTableComponent!: ResourceChangeHistoryTableComponent;

  public ResourceChangeHistories: ResourceChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalResourceChangeHistoryCount$ : Observable<number> | null = null;
  public filteredResourceChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private resourceChangeHistoryService: ResourceChangeHistoryService,
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
    // Subscribe to the resourceChangeHistoryChanged observable on the add/edit component so that when a ResourceChangeHistory changes we can reload the list.
    //
    this.addEditResourceChangeHistoryComponent.resourceChangeHistoryChanged.subscribe({
      next: (result: ResourceChangeHistoryData[] | null) => {
        this.resourceChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Resource Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditResourceChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalResourceChangeHistoryCount$ = this.resourceChangeHistoryService.GetResourceChangeHistoriesRowCount({
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

      this.filteredResourceChangeHistoryCount$ = this.resourceChangeHistoryService.GetResourceChangeHistoriesRowCount({
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

      this.filteredResourceChangeHistoryCount$ = this.totalResourceChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalResourceChangeHistoryCount$.subscribe();

    if (this.filteredResourceChangeHistoryCount$ != this.totalResourceChangeHistoryCount$) {
      this.filteredResourceChangeHistoryCount$.subscribe();
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
      this.resourceChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerResourceChangeHistoryReader(): boolean {
    return this.resourceChangeHistoryService.userIsSchedulerResourceChangeHistoryReader();
  }

  public userIsSchedulerResourceChangeHistoryWriter(): boolean {
    return this.resourceChangeHistoryService.userIsSchedulerResourceChangeHistoryWriter();
  }
}

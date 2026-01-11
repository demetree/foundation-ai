/*
   GENERATED FORM FOR THE SCHEDULINGTARGETADDRESSCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetAddressChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-address-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SchedulingTargetAddressChangeHistoryService, SchedulingTargetAddressChangeHistoryData } from '../../../scheduler-data-services/scheduling-target-address-change-history.service';
import { SchedulingTargetAddressChangeHistoryAddEditComponent } from '../scheduling-target-address-change-history-add-edit/scheduling-target-address-change-history-add-edit.component';
import { SchedulingTargetAddressChangeHistoryTableComponent } from '../scheduling-target-address-change-history-table/scheduling-target-address-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-scheduling-target-address-change-history-listing',
  templateUrl: './scheduling-target-address-change-history-listing.component.html',
  styleUrls: ['./scheduling-target-address-change-history-listing.component.scss']
})
export class SchedulingTargetAddressChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SchedulingTargetAddressChangeHistoryAddEditComponent) addEditSchedulingTargetAddressChangeHistoryComponent!: SchedulingTargetAddressChangeHistoryAddEditComponent;
  @ViewChild(SchedulingTargetAddressChangeHistoryTableComponent) schedulingTargetAddressChangeHistoryTableComponent!: SchedulingTargetAddressChangeHistoryTableComponent;

  public SchedulingTargetAddressChangeHistories: SchedulingTargetAddressChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSchedulingTargetAddressChangeHistoryCount$ : Observable<number> | null = null;
  public filteredSchedulingTargetAddressChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private schedulingTargetAddressChangeHistoryService: SchedulingTargetAddressChangeHistoryService,
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
    // Subscribe to the schedulingTargetAddressChangeHistoryChanged observable on the add/edit component so that when a SchedulingTargetAddressChangeHistory changes we can reload the list.
    //
    this.addEditSchedulingTargetAddressChangeHistoryComponent.schedulingTargetAddressChangeHistoryChanged.subscribe({
      next: (result: SchedulingTargetAddressChangeHistoryData[] | null) => {
        this.schedulingTargetAddressChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Scheduling Target Address Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSchedulingTargetAddressChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSchedulingTargetAddressChangeHistoryCount$ = this.schedulingTargetAddressChangeHistoryService.GetSchedulingTargetAddressChangeHistoriesRowCount({
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

      this.filteredSchedulingTargetAddressChangeHistoryCount$ = this.schedulingTargetAddressChangeHistoryService.GetSchedulingTargetAddressChangeHistoriesRowCount({
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

      this.filteredSchedulingTargetAddressChangeHistoryCount$ = this.totalSchedulingTargetAddressChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSchedulingTargetAddressChangeHistoryCount$.subscribe();

    if (this.filteredSchedulingTargetAddressChangeHistoryCount$ != this.totalSchedulingTargetAddressChangeHistoryCount$) {
      this.filteredSchedulingTargetAddressChangeHistoryCount$.subscribe();
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
      this.schedulingTargetAddressChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerSchedulingTargetAddressChangeHistoryReader(): boolean {
    return this.schedulingTargetAddressChangeHistoryService.userIsSchedulerSchedulingTargetAddressChangeHistoryReader();
  }

  public userIsSchedulerSchedulingTargetAddressChangeHistoryWriter(): boolean {
    return this.schedulingTargetAddressChangeHistoryService.userIsSchedulerSchedulingTargetAddressChangeHistoryWriter();
  }
}

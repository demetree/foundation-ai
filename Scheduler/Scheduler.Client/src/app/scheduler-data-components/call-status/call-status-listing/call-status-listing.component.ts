/*
   GENERATED FORM FOR THE CALLSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CallStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to call-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { CallStatusService, CallStatusData } from '../../../scheduler-data-services/call-status.service';
import { CallStatusAddEditComponent } from '../call-status-add-edit/call-status-add-edit.component';
import { CallStatusTableComponent } from '../call-status-table/call-status-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-call-status-listing',
  templateUrl: './call-status-listing.component.html',
  styleUrls: ['./call-status-listing.component.scss']
})
export class CallStatusListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(CallStatusAddEditComponent) addEditCallStatusComponent!: CallStatusAddEditComponent;
  @ViewChild(CallStatusTableComponent) callStatusTableComponent!: CallStatusTableComponent;

  public CallStatuses: CallStatusData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalCallStatusCount$ : Observable<number> | null = null;
  public filteredCallStatusCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private callStatusService: CallStatusService,
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
    // Subscribe to the callStatusChanged observable on the add/edit component so that when a CallStatus changes we can reload the list.
    //
    this.addEditCallStatusComponent.callStatusChanged.subscribe({
      next: (result: CallStatusData[] | null) => {
        this.callStatusTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Call Status changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditCallStatusComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalCallStatusCount$ = this.callStatusService.GetCallStatusesRowCount({
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

      this.filteredCallStatusCount$ = this.callStatusService.GetCallStatusesRowCount({
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

      this.filteredCallStatusCount$ = this.totalCallStatusCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalCallStatusCount$.subscribe();

    if (this.filteredCallStatusCount$ != this.totalCallStatusCount$) {
      this.filteredCallStatusCount$.subscribe();
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
      this.callStatusTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.callStatusTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerCallStatusReader(): boolean {
    return this.callStatusService.userIsSchedulerCallStatusReader();
  }

  public userIsSchedulerCallStatusWriter(): boolean {
    return this.callStatusService.userIsSchedulerCallStatusWriter();
  }
}

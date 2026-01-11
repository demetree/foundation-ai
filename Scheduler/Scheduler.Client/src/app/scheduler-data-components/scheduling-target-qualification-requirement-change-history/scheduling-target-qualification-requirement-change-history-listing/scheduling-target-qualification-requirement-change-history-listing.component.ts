/*
   GENERATED FORM FOR THE SCHEDULINGTARGETQUALIFICATIONREQUIREMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetQualificationRequirementChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-qualification-requirement-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SchedulingTargetQualificationRequirementChangeHistoryService, SchedulingTargetQualificationRequirementChangeHistoryData } from '../../../scheduler-data-services/scheduling-target-qualification-requirement-change-history.service';
import { SchedulingTargetQualificationRequirementChangeHistoryAddEditComponent } from '../scheduling-target-qualification-requirement-change-history-add-edit/scheduling-target-qualification-requirement-change-history-add-edit.component';
import { SchedulingTargetQualificationRequirementChangeHistoryTableComponent } from '../scheduling-target-qualification-requirement-change-history-table/scheduling-target-qualification-requirement-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-scheduling-target-qualification-requirement-change-history-listing',
  templateUrl: './scheduling-target-qualification-requirement-change-history-listing.component.html',
  styleUrls: ['./scheduling-target-qualification-requirement-change-history-listing.component.scss']
})
export class SchedulingTargetQualificationRequirementChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SchedulingTargetQualificationRequirementChangeHistoryAddEditComponent) addEditSchedulingTargetQualificationRequirementChangeHistoryComponent!: SchedulingTargetQualificationRequirementChangeHistoryAddEditComponent;
  @ViewChild(SchedulingTargetQualificationRequirementChangeHistoryTableComponent) schedulingTargetQualificationRequirementChangeHistoryTableComponent!: SchedulingTargetQualificationRequirementChangeHistoryTableComponent;

  public SchedulingTargetQualificationRequirementChangeHistories: SchedulingTargetQualificationRequirementChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSchedulingTargetQualificationRequirementChangeHistoryCount$ : Observable<number> | null = null;
  public filteredSchedulingTargetQualificationRequirementChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private schedulingTargetQualificationRequirementChangeHistoryService: SchedulingTargetQualificationRequirementChangeHistoryService,
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
    // Subscribe to the schedulingTargetQualificationRequirementChangeHistoryChanged observable on the add/edit component so that when a SchedulingTargetQualificationRequirementChangeHistory changes we can reload the list.
    //
    this.addEditSchedulingTargetQualificationRequirementChangeHistoryComponent.schedulingTargetQualificationRequirementChangeHistoryChanged.subscribe({
      next: (result: SchedulingTargetQualificationRequirementChangeHistoryData[] | null) => {
        this.schedulingTargetQualificationRequirementChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Scheduling Target Qualification Requirement Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSchedulingTargetQualificationRequirementChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSchedulingTargetQualificationRequirementChangeHistoryCount$ = this.schedulingTargetQualificationRequirementChangeHistoryService.GetSchedulingTargetQualificationRequirementChangeHistoriesRowCount({
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

      this.filteredSchedulingTargetQualificationRequirementChangeHistoryCount$ = this.schedulingTargetQualificationRequirementChangeHistoryService.GetSchedulingTargetQualificationRequirementChangeHistoriesRowCount({
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

      this.filteredSchedulingTargetQualificationRequirementChangeHistoryCount$ = this.totalSchedulingTargetQualificationRequirementChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSchedulingTargetQualificationRequirementChangeHistoryCount$.subscribe();

    if (this.filteredSchedulingTargetQualificationRequirementChangeHistoryCount$ != this.totalSchedulingTargetQualificationRequirementChangeHistoryCount$) {
      this.filteredSchedulingTargetQualificationRequirementChangeHistoryCount$.subscribe();
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
      this.schedulingTargetQualificationRequirementChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader(): boolean {
    return this.schedulingTargetQualificationRequirementChangeHistoryService.userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryReader();
  }

  public userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter(): boolean {
    return this.schedulingTargetQualificationRequirementChangeHistoryService.userIsSchedulerSchedulingTargetQualificationRequirementChangeHistoryWriter();
  }
}

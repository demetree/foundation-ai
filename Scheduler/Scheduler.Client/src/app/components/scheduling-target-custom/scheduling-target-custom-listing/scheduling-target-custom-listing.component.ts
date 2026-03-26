
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SchedulingTargetService, SchedulingTargetData } from '../../../scheduler-data-services/scheduling-target.service';
import { SchedulingTargetCustomAddEditComponent } from '../scheduling-target-custom-add-edit/scheduling-target-custom-add-edit.component';
import { SchedulingTargetCustomTableComponent } from '../scheduling-target-custom-table/scheduling-target-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-scheduling-target-custom-listing',
  templateUrl: './scheduling-target-custom-listing.component.html',
  styleUrls: ['./scheduling-target-custom-listing.component.scss']
})
export class SchedulingTargetCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SchedulingTargetCustomAddEditComponent) addEditSchedulingTargetComponent!: SchedulingTargetCustomAddEditComponent;
  @ViewChild(SchedulingTargetCustomTableComponent) schedulingTargetCustomTableComponent!: SchedulingTargetCustomTableComponent;

  public SchedulingTargets: SchedulingTargetData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSchedulingTargetCount$ : Observable<number> | null = null;
  public filteredSchedulingTargetCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private schedulingTargetService: SchedulingTargetService,
              private alertService: AlertService,
              private navigationService: NavigationService,
              private breakpointObserver: BreakpointObserver) { }

  ngOnInit(): void {

    this.breakpointObserver
      .observe(['(max-width: 1100px)']) 
      .subscribe((result) => {
        this.isSmallScreen = result.matches;
      });

    this.loadCounts();
  }


  ngAfterViewInit(): void {
    //
    // Subscribe to the schedulingTargetChanged observable on the add/edit component so that when a SchedulingTarget changes we can reload the list.
    //
    this.addEditSchedulingTargetComponent.schedulingTargetChanged.subscribe({
      next: (result: SchedulingTargetData[] | null) => {
        this.schedulingTargetCustomTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Scheduling Target changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSchedulingTargetComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSchedulingTargetCount$ = this.schedulingTargetService.GetSchedulingTargetsRowCount({
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

      this.filteredSchedulingTargetCount$ = this.schedulingTargetService.GetSchedulingTargetsRowCount({
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

      this.filteredSchedulingTargetCount$ = this.totalSchedulingTargetCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSchedulingTargetCount$.subscribe();

    if (this.filteredSchedulingTargetCount$ != this.totalSchedulingTargetCount$) {
      this.filteredSchedulingTargetCount$.subscribe();
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
      this.schedulingTargetCustomTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerSchedulingTargetReader(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetReader();
  }

  public userIsSchedulerSchedulingTargetWriter(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
  }
}

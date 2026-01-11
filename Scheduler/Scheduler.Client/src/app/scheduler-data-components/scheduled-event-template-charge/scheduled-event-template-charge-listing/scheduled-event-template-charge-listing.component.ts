/*
   GENERATED FORM FOR THE SCHEDULEDEVENTTEMPLATECHARGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventTemplateCharge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-template-charge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ScheduledEventTemplateChargeService, ScheduledEventTemplateChargeData } from '../../../scheduler-data-services/scheduled-event-template-charge.service';
import { ScheduledEventTemplateChargeAddEditComponent } from '../scheduled-event-template-charge-add-edit/scheduled-event-template-charge-add-edit.component';
import { ScheduledEventTemplateChargeTableComponent } from '../scheduled-event-template-charge-table/scheduled-event-template-charge-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-scheduled-event-template-charge-listing',
  templateUrl: './scheduled-event-template-charge-listing.component.html',
  styleUrls: ['./scheduled-event-template-charge-listing.component.scss']
})
export class ScheduledEventTemplateChargeListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ScheduledEventTemplateChargeAddEditComponent) addEditScheduledEventTemplateChargeComponent!: ScheduledEventTemplateChargeAddEditComponent;
  @ViewChild(ScheduledEventTemplateChargeTableComponent) scheduledEventTemplateChargeTableComponent!: ScheduledEventTemplateChargeTableComponent;

  public ScheduledEventTemplateCharges: ScheduledEventTemplateChargeData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalScheduledEventTemplateChargeCount$ : Observable<number> | null = null;
  public filteredScheduledEventTemplateChargeCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
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
    // Subscribe to the scheduledEventTemplateChargeChanged observable on the add/edit component so that when a ScheduledEventTemplateCharge changes we can reload the list.
    //
    this.addEditScheduledEventTemplateChargeComponent.scheduledEventTemplateChargeChanged.subscribe({
      next: (result: ScheduledEventTemplateChargeData[] | null) => {
        this.scheduledEventTemplateChargeTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Scheduled Event Template Charge changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditScheduledEventTemplateChargeComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalScheduledEventTemplateChargeCount$ = this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargesRowCount({
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

      this.filteredScheduledEventTemplateChargeCount$ = this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargesRowCount({
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

      this.filteredScheduledEventTemplateChargeCount$ = this.totalScheduledEventTemplateChargeCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalScheduledEventTemplateChargeCount$.subscribe();

    if (this.filteredScheduledEventTemplateChargeCount$ != this.totalScheduledEventTemplateChargeCount$) {
      this.filteredScheduledEventTemplateChargeCount$.subscribe();
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
      this.scheduledEventTemplateChargeTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerScheduledEventTemplateChargeReader(): boolean {
    return this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeReader();
  }

  public userIsSchedulerScheduledEventTemplateChargeWriter(): boolean {
    return this.scheduledEventTemplateChargeService.userIsSchedulerScheduledEventTemplateChargeWriter();
  }
}

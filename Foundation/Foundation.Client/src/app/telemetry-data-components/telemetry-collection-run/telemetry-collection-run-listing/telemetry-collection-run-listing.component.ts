/*
   GENERATED FORM FOR THE TELEMETRYCOLLECTIONRUN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryCollectionRun table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-collection-run-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { TelemetryCollectionRunService, TelemetryCollectionRunData } from '../../../telemetry-data-services/telemetry-collection-run.service';
import { TelemetryCollectionRunAddEditComponent } from '../telemetry-collection-run-add-edit/telemetry-collection-run-add-edit.component';
import { TelemetryCollectionRunTableComponent } from '../telemetry-collection-run-table/telemetry-collection-run-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-telemetry-collection-run-listing',
  templateUrl: './telemetry-collection-run-listing.component.html',
  styleUrls: ['./telemetry-collection-run-listing.component.scss']
})
export class TelemetryCollectionRunListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(TelemetryCollectionRunAddEditComponent) addEditTelemetryCollectionRunComponent!: TelemetryCollectionRunAddEditComponent;
  @ViewChild(TelemetryCollectionRunTableComponent) telemetryCollectionRunTableComponent!: TelemetryCollectionRunTableComponent;

  public TelemetryCollectionRuns: TelemetryCollectionRunData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalTelemetryCollectionRunCount$ : Observable<number> | null = null;
  public filteredTelemetryCollectionRunCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private telemetryCollectionRunService: TelemetryCollectionRunService,
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
    // Subscribe to the telemetryCollectionRunChanged observable on the add/edit component so that when a TelemetryCollectionRun changes we can reload the list.
    //
    this.addEditTelemetryCollectionRunComponent.telemetryCollectionRunChanged.subscribe({
      next: (result: TelemetryCollectionRunData[] | null) => {
        this.telemetryCollectionRunTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Telemetry Collection Run changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditTelemetryCollectionRunComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalTelemetryCollectionRunCount$ = this.telemetryCollectionRunService.GetTelemetryCollectionRunsRowCount({
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

      this.filteredTelemetryCollectionRunCount$ = this.telemetryCollectionRunService.GetTelemetryCollectionRunsRowCount({
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

      this.filteredTelemetryCollectionRunCount$ = this.totalTelemetryCollectionRunCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalTelemetryCollectionRunCount$.subscribe();

    if (this.filteredTelemetryCollectionRunCount$ != this.totalTelemetryCollectionRunCount$) {
      this.filteredTelemetryCollectionRunCount$.subscribe();
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
      this.telemetryCollectionRunTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsTelemetryTelemetryCollectionRunReader(): boolean {
    return this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunReader();
  }

  public userIsTelemetryTelemetryCollectionRunWriter(): boolean {
    return this.telemetryCollectionRunService.userIsTelemetryTelemetryCollectionRunWriter();
  }
}

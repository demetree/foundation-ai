/*
   GENERATED FORM FOR THE TELEMETRYSESSIONSNAPSHOT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetrySessionSnapshot table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-session-snapshot-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { TelemetrySessionSnapshotService, TelemetrySessionSnapshotData } from '../../../telemetry-data-services/telemetry-session-snapshot.service';
import { TelemetrySessionSnapshotAddEditComponent } from '../telemetry-session-snapshot-add-edit/telemetry-session-snapshot-add-edit.component';
import { TelemetrySessionSnapshotTableComponent } from '../telemetry-session-snapshot-table/telemetry-session-snapshot-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-telemetry-session-snapshot-listing',
  templateUrl: './telemetry-session-snapshot-listing.component.html',
  styleUrls: ['./telemetry-session-snapshot-listing.component.scss']
})
export class TelemetrySessionSnapshotListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(TelemetrySessionSnapshotAddEditComponent) addEditTelemetrySessionSnapshotComponent!: TelemetrySessionSnapshotAddEditComponent;
  @ViewChild(TelemetrySessionSnapshotTableComponent) telemetrySessionSnapshotTableComponent!: TelemetrySessionSnapshotTableComponent;

  public TelemetrySessionSnapshots: TelemetrySessionSnapshotData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalTelemetrySessionSnapshotCount$ : Observable<number> | null = null;
  public filteredTelemetrySessionSnapshotCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private telemetrySessionSnapshotService: TelemetrySessionSnapshotService,
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
    // Subscribe to the telemetrySessionSnapshotChanged observable on the add/edit component so that when a TelemetrySessionSnapshot changes we can reload the list.
    //
    this.addEditTelemetrySessionSnapshotComponent.telemetrySessionSnapshotChanged.subscribe({
      next: (result: TelemetrySessionSnapshotData[] | null) => {
        this.telemetrySessionSnapshotTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Telemetry Session Snapshot changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditTelemetrySessionSnapshotComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalTelemetrySessionSnapshotCount$ = this.telemetrySessionSnapshotService.GetTelemetrySessionSnapshotsRowCount({
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

      this.filteredTelemetrySessionSnapshotCount$ = this.telemetrySessionSnapshotService.GetTelemetrySessionSnapshotsRowCount({
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

      this.filteredTelemetrySessionSnapshotCount$ = this.totalTelemetrySessionSnapshotCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalTelemetrySessionSnapshotCount$.subscribe();

    if (this.filteredTelemetrySessionSnapshotCount$ != this.totalTelemetrySessionSnapshotCount$) {
      this.filteredTelemetrySessionSnapshotCount$.subscribe();
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
      this.telemetrySessionSnapshotTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsTelemetryTelemetrySessionSnapshotReader(): boolean {
    return this.telemetrySessionSnapshotService.userIsTelemetryTelemetrySessionSnapshotReader();
  }

  public userIsTelemetryTelemetrySessionSnapshotWriter(): boolean {
    return this.telemetrySessionSnapshotService.userIsTelemetryTelemetrySessionSnapshotWriter();
  }
}

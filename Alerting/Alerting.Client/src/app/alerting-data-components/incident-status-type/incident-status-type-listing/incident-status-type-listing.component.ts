/*
   GENERATED FORM FOR THE INCIDENTSTATUSTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentStatusType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-status-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { IncidentStatusTypeService, IncidentStatusTypeData } from '../../../alerting-data-services/incident-status-type.service';
import { IncidentStatusTypeAddEditComponent } from '../incident-status-type-add-edit/incident-status-type-add-edit.component';
import { IncidentStatusTypeTableComponent } from '../incident-status-type-table/incident-status-type-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-incident-status-type-listing',
  templateUrl: './incident-status-type-listing.component.html',
  styleUrls: ['./incident-status-type-listing.component.scss']
})
export class IncidentStatusTypeListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(IncidentStatusTypeAddEditComponent) addEditIncidentStatusTypeComponent!: IncidentStatusTypeAddEditComponent;
  @ViewChild(IncidentStatusTypeTableComponent) incidentStatusTypeTableComponent!: IncidentStatusTypeTableComponent;

  public IncidentStatusTypes: IncidentStatusTypeData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalIncidentStatusTypeCount$ : Observable<number> | null = null;
  public filteredIncidentStatusTypeCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private incidentStatusTypeService: IncidentStatusTypeService,
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
    // Subscribe to the incidentStatusTypeChanged observable on the add/edit component so that when a IncidentStatusType changes we can reload the list.
    //
    this.addEditIncidentStatusTypeComponent.incidentStatusTypeChanged.subscribe({
      next: (result: IncidentStatusTypeData[] | null) => {
        this.incidentStatusTypeTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Incident Status Type changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditIncidentStatusTypeComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalIncidentStatusTypeCount$ = this.incidentStatusTypeService.GetIncidentStatusTypesRowCount({
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

      this.filteredIncidentStatusTypeCount$ = this.incidentStatusTypeService.GetIncidentStatusTypesRowCount({
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

      this.filteredIncidentStatusTypeCount$ = this.totalIncidentStatusTypeCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalIncidentStatusTypeCount$.subscribe();

    if (this.filteredIncidentStatusTypeCount$ != this.totalIncidentStatusTypeCount$) {
      this.filteredIncidentStatusTypeCount$.subscribe();
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
      this.incidentStatusTypeTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAlertingIncidentStatusTypeReader(): boolean {
    return this.incidentStatusTypeService.userIsAlertingIncidentStatusTypeReader();
  }

  public userIsAlertingIncidentStatusTypeWriter(): boolean {
    return this.incidentStatusTypeService.userIsAlertingIncidentStatusTypeWriter();
  }
}

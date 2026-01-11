/*
   GENERATED FORM FOR THE RESOURCEAVAILABILITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceAvailability table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-availability-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ResourceAvailabilityService, ResourceAvailabilityData } from '../../../scheduler-data-services/resource-availability.service';
import { ResourceAvailabilityAddEditComponent } from '../resource-availability-add-edit/resource-availability-add-edit.component';
import { ResourceAvailabilityTableComponent } from '../resource-availability-table/resource-availability-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-resource-availability-listing',
  templateUrl: './resource-availability-listing.component.html',
  styleUrls: ['./resource-availability-listing.component.scss']
})
export class ResourceAvailabilityListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ResourceAvailabilityAddEditComponent) addEditResourceAvailabilityComponent!: ResourceAvailabilityAddEditComponent;
  @ViewChild(ResourceAvailabilityTableComponent) resourceAvailabilityTableComponent!: ResourceAvailabilityTableComponent;

  public ResourceAvailabilities: ResourceAvailabilityData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalResourceAvailabilityCount$ : Observable<number> | null = null;
  public filteredResourceAvailabilityCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private resourceAvailabilityService: ResourceAvailabilityService,
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
    // Subscribe to the resourceAvailabilityChanged observable on the add/edit component so that when a ResourceAvailability changes we can reload the list.
    //
    this.addEditResourceAvailabilityComponent.resourceAvailabilityChanged.subscribe({
      next: (result: ResourceAvailabilityData[] | null) => {
        this.resourceAvailabilityTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Resource Availability changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditResourceAvailabilityComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalResourceAvailabilityCount$ = this.resourceAvailabilityService.GetResourceAvailabilitiesRowCount({
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

      this.filteredResourceAvailabilityCount$ = this.resourceAvailabilityService.GetResourceAvailabilitiesRowCount({
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

      this.filteredResourceAvailabilityCount$ = this.totalResourceAvailabilityCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalResourceAvailabilityCount$.subscribe();

    if (this.filteredResourceAvailabilityCount$ != this.totalResourceAvailabilityCount$) {
      this.filteredResourceAvailabilityCount$.subscribe();
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
      this.resourceAvailabilityTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerResourceAvailabilityReader(): boolean {
    return this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityReader();
  }

  public userIsSchedulerResourceAvailabilityWriter(): boolean {
    return this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityWriter();
  }
}

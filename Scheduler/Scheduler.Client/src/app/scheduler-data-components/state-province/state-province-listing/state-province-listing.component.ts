/*
   GENERATED FORM FOR THE STATEPROVINCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from StateProvince table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to state-province-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { StateProvinceService, StateProvinceData } from '../../../scheduler-data-services/state-province.service';
import { StateProvinceAddEditComponent } from '../state-province-add-edit/state-province-add-edit.component';
import { StateProvinceTableComponent } from '../state-province-table/state-province-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-state-province-listing',
  templateUrl: './state-province-listing.component.html',
  styleUrls: ['./state-province-listing.component.scss']
})
export class StateProvinceListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(StateProvinceAddEditComponent) addEditStateProvinceComponent!: StateProvinceAddEditComponent;
  @ViewChild(StateProvinceTableComponent) stateProvinceTableComponent!: StateProvinceTableComponent;

  public StateProvinces: StateProvinceData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalStateProvinceCount$ : Observable<number> | null = null;
  public filteredStateProvinceCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private stateProvinceService: StateProvinceService,
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
    // Subscribe to the stateProvinceChanged observable on the add/edit component so that when a StateProvince changes we can reload the list.
    //
    this.addEditStateProvinceComponent.stateProvinceChanged.subscribe({
      next: (result: StateProvinceData[] | null) => {
        this.stateProvinceTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during State Province changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditStateProvinceComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalStateProvinceCount$ = this.stateProvinceService.GetStateProvincesRowCount({
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

      this.filteredStateProvinceCount$ = this.stateProvinceService.GetStateProvincesRowCount({
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

      this.filteredStateProvinceCount$ = this.totalStateProvinceCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalStateProvinceCount$.subscribe();

    if (this.filteredStateProvinceCount$ != this.totalStateProvinceCount$) {
      this.filteredStateProvinceCount$.subscribe();
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
      this.stateProvinceTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerStateProvinceReader(): boolean {
    return this.stateProvinceService.userIsSchedulerStateProvinceReader();
  }

  public userIsSchedulerStateProvinceWriter(): boolean {
    return this.stateProvinceService.userIsSchedulerStateProvinceWriter();
  }
}

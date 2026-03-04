/*
   GENERATED FORM FOR THE SHIFTPATTERN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ShiftPattern table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to shift-pattern-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ShiftPatternService, ShiftPatternData } from '../../../scheduler-data-services/shift-pattern.service';
import { ShiftPatternAddEditComponent } from '../shift-pattern-add-edit/shift-pattern-add-edit.component';
import { ShiftPatternTableComponent } from '../shift-pattern-table/shift-pattern-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-shift-pattern-listing',
  templateUrl: './shift-pattern-listing.component.html',
  styleUrls: ['./shift-pattern-listing.component.scss']
})
export class ShiftPatternListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ShiftPatternAddEditComponent) addEditShiftPatternComponent!: ShiftPatternAddEditComponent;
  @ViewChild(ShiftPatternTableComponent) shiftPatternTableComponent!: ShiftPatternTableComponent;

  public ShiftPatterns: ShiftPatternData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalShiftPatternCount$ : Observable<number> | null = null;
  public filteredShiftPatternCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private shiftPatternService: ShiftPatternService,
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
    // Subscribe to the shiftPatternChanged observable on the add/edit component so that when a ShiftPattern changes we can reload the list.
    //
    this.addEditShiftPatternComponent.shiftPatternChanged.subscribe({
      next: (result: ShiftPatternData[] | null) => {
        this.shiftPatternTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Shift Pattern changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditShiftPatternComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalShiftPatternCount$ = this.shiftPatternService.GetShiftPatternsRowCount({
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

      this.filteredShiftPatternCount$ = this.shiftPatternService.GetShiftPatternsRowCount({
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

      this.filteredShiftPatternCount$ = this.totalShiftPatternCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalShiftPatternCount$.subscribe();

    if (this.filteredShiftPatternCount$ != this.totalShiftPatternCount$) {
      this.filteredShiftPatternCount$.subscribe();
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
      this.shiftPatternTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.shiftPatternTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerShiftPatternReader(): boolean {
    return this.shiftPatternService.userIsSchedulerShiftPatternReader();
  }

  public userIsSchedulerShiftPatternWriter(): boolean {
    return this.shiftPatternService.userIsSchedulerShiftPatternWriter();
  }
}

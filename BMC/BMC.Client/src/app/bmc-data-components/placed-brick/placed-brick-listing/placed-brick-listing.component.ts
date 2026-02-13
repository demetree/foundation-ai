/*
   GENERATED FORM FOR THE PLACEDBRICK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PlacedBrick table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to placed-brick-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { PlacedBrickService, PlacedBrickData } from '../../../bmc-data-services/placed-brick.service';
import { PlacedBrickAddEditComponent } from '../placed-brick-add-edit/placed-brick-add-edit.component';
import { PlacedBrickTableComponent } from '../placed-brick-table/placed-brick-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-placed-brick-listing',
  templateUrl: './placed-brick-listing.component.html',
  styleUrls: ['./placed-brick-listing.component.scss']
})
export class PlacedBrickListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(PlacedBrickAddEditComponent) addEditPlacedBrickComponent!: PlacedBrickAddEditComponent;
  @ViewChild(PlacedBrickTableComponent) placedBrickTableComponent!: PlacedBrickTableComponent;

  public PlacedBricks: PlacedBrickData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalPlacedBrickCount$ : Observable<number> | null = null;
  public filteredPlacedBrickCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private placedBrickService: PlacedBrickService,
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
    // Subscribe to the placedBrickChanged observable on the add/edit component so that when a PlacedBrick changes we can reload the list.
    //
    this.addEditPlacedBrickComponent.placedBrickChanged.subscribe({
      next: (result: PlacedBrickData[] | null) => {
        this.placedBrickTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Placed Brick changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditPlacedBrickComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalPlacedBrickCount$ = this.placedBrickService.GetPlacedBricksRowCount({
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

      this.filteredPlacedBrickCount$ = this.placedBrickService.GetPlacedBricksRowCount({
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

      this.filteredPlacedBrickCount$ = this.totalPlacedBrickCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalPlacedBrickCount$.subscribe();

    if (this.filteredPlacedBrickCount$ != this.totalPlacedBrickCount$) {
      this.filteredPlacedBrickCount$.subscribe();
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
      this.placedBrickTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCPlacedBrickReader(): boolean {
    return this.placedBrickService.userIsBMCPlacedBrickReader();
  }

  public userIsBMCPlacedBrickWriter(): boolean {
    return this.placedBrickService.userIsBMCPlacedBrickWriter();
  }
}

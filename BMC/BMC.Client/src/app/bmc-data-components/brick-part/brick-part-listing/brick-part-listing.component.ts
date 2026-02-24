/*
   GENERATED FORM FOR THE BRICKPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { BrickPartService, BrickPartData } from '../../../bmc-data-services/brick-part.service';
import { BrickPartAddEditComponent } from '../brick-part-add-edit/brick-part-add-edit.component';
import { BrickPartTableComponent } from '../brick-part-table/brick-part-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-brick-part-listing',
  templateUrl: './brick-part-listing.component.html',
  styleUrls: ['./brick-part-listing.component.scss']
})
export class BrickPartListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(BrickPartAddEditComponent) addEditBrickPartComponent!: BrickPartAddEditComponent;
  @ViewChild(BrickPartTableComponent) brickPartTableComponent!: BrickPartTableComponent;

  public BrickParts: BrickPartData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalBrickPartCount$: Observable<number> | null = null;
  public filteredBrickPartCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private brickPartService: BrickPartService,
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
    // Subscribe to the brickPartChanged observable on the add/edit component so that when a BrickPart changes we can reload the list.
    //
    this.addEditBrickPartComponent.brickPartChanged.subscribe({
      next: (result: BrickPartData[] | null) => {
        this.brickPartTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
        this.alertService.showMessage("Error during Brick Part changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditBrickPartComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalBrickPartCount$ = this.brickPartService.GetBrickPartsRowCount({
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

      this.filteredBrickPartCount$ = this.brickPartService.GetBrickPartsRowCount({
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

      this.filteredBrickPartCount$ = this.totalBrickPartCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalBrickPartCount$.subscribe();

    if (this.filteredBrickPartCount$ != this.totalBrickPartCount$) {
      this.filteredBrickPartCount$.subscribe();
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
      this.brickPartTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.brickPartTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCBrickPartReader(): boolean {
    return this.brickPartService.userIsBMCBrickPartReader();
  }

  public userIsBMCBrickPartWriter(): boolean {
    return this.brickPartService.userIsBMCBrickPartWriter();
  }
}

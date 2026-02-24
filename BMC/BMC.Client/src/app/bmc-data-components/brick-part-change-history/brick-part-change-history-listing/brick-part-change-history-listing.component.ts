/*
   GENERATED FORM FOR THE BRICKPARTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPartChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { BrickPartChangeHistoryService, BrickPartChangeHistoryData } from '../../../bmc-data-services/brick-part-change-history.service';
import { BrickPartChangeHistoryAddEditComponent } from '../brick-part-change-history-add-edit/brick-part-change-history-add-edit.component';
import { BrickPartChangeHistoryTableComponent } from '../brick-part-change-history-table/brick-part-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-brick-part-change-history-listing',
  templateUrl: './brick-part-change-history-listing.component.html',
  styleUrls: ['./brick-part-change-history-listing.component.scss']
})
export class BrickPartChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(BrickPartChangeHistoryAddEditComponent) addEditBrickPartChangeHistoryComponent!: BrickPartChangeHistoryAddEditComponent;
  @ViewChild(BrickPartChangeHistoryTableComponent) brickPartChangeHistoryTableComponent!: BrickPartChangeHistoryTableComponent;

  public BrickPartChangeHistories: BrickPartChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalBrickPartChangeHistoryCount$ : Observable<number> | null = null;
  public filteredBrickPartChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private brickPartChangeHistoryService: BrickPartChangeHistoryService,
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
    // Subscribe to the brickPartChangeHistoryChanged observable on the add/edit component so that when a BrickPartChangeHistory changes we can reload the list.
    //
    this.addEditBrickPartChangeHistoryComponent.brickPartChangeHistoryChanged.subscribe({
      next: (result: BrickPartChangeHistoryData[] | null) => {
        this.brickPartChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Brick Part Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditBrickPartChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalBrickPartChangeHistoryCount$ = this.brickPartChangeHistoryService.GetBrickPartChangeHistoriesRowCount({
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

      this.filteredBrickPartChangeHistoryCount$ = this.brickPartChangeHistoryService.GetBrickPartChangeHistoriesRowCount({
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

      this.filteredBrickPartChangeHistoryCount$ = this.totalBrickPartChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalBrickPartChangeHistoryCount$.subscribe();

    if (this.filteredBrickPartChangeHistoryCount$ != this.totalBrickPartChangeHistoryCount$) {
      this.filteredBrickPartChangeHistoryCount$.subscribe();
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
      this.brickPartChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.brickPartChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCBrickPartChangeHistoryReader(): boolean {
    return this.brickPartChangeHistoryService.userIsBMCBrickPartChangeHistoryReader();
  }

  public userIsBMCBrickPartChangeHistoryWriter(): boolean {
    return this.brickPartChangeHistoryService.userIsBMCBrickPartChangeHistoryWriter();
  }
}

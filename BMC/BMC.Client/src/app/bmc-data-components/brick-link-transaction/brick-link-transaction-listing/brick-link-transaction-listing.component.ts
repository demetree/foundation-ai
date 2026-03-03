/*
   GENERATED FORM FOR THE BRICKLINKTRANSACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickLinkTransaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-link-transaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { BrickLinkTransactionService, BrickLinkTransactionData } from '../../../bmc-data-services/brick-link-transaction.service';
import { BrickLinkTransactionAddEditComponent } from '../brick-link-transaction-add-edit/brick-link-transaction-add-edit.component';
import { BrickLinkTransactionTableComponent } from '../brick-link-transaction-table/brick-link-transaction-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-brick-link-transaction-listing',
  templateUrl: './brick-link-transaction-listing.component.html',
  styleUrls: ['./brick-link-transaction-listing.component.scss']
})
export class BrickLinkTransactionListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(BrickLinkTransactionAddEditComponent) addEditBrickLinkTransactionComponent!: BrickLinkTransactionAddEditComponent;
  @ViewChild(BrickLinkTransactionTableComponent) brickLinkTransactionTableComponent!: BrickLinkTransactionTableComponent;

  public BrickLinkTransactions: BrickLinkTransactionData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalBrickLinkTransactionCount$ : Observable<number> | null = null;
  public filteredBrickLinkTransactionCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private brickLinkTransactionService: BrickLinkTransactionService,
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
    // Subscribe to the brickLinkTransactionChanged observable on the add/edit component so that when a BrickLinkTransaction changes we can reload the list.
    //
    this.addEditBrickLinkTransactionComponent.brickLinkTransactionChanged.subscribe({
      next: (result: BrickLinkTransactionData[] | null) => {
        this.brickLinkTransactionTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Brick Link Transaction changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditBrickLinkTransactionComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalBrickLinkTransactionCount$ = this.brickLinkTransactionService.GetBrickLinkTransactionsRowCount({
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

      this.filteredBrickLinkTransactionCount$ = this.brickLinkTransactionService.GetBrickLinkTransactionsRowCount({
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

      this.filteredBrickLinkTransactionCount$ = this.totalBrickLinkTransactionCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalBrickLinkTransactionCount$.subscribe();

    if (this.filteredBrickLinkTransactionCount$ != this.totalBrickLinkTransactionCount$) {
      this.filteredBrickLinkTransactionCount$.subscribe();
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
      this.brickLinkTransactionTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.brickLinkTransactionTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCBrickLinkTransactionReader(): boolean {
    return this.brickLinkTransactionService.userIsBMCBrickLinkTransactionReader();
  }

  public userIsBMCBrickLinkTransactionWriter(): boolean {
    return this.brickLinkTransactionService.userIsBMCBrickLinkTransactionWriter();
  }
}

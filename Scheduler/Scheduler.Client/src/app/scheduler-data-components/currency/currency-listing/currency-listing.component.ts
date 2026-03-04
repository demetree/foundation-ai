/*
   GENERATED FORM FOR THE CURRENCY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Currency table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to currency-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { CurrencyService, CurrencyData } from '../../../scheduler-data-services/currency.service';
import { CurrencyAddEditComponent } from '../currency-add-edit/currency-add-edit.component';
import { CurrencyTableComponent } from '../currency-table/currency-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-currency-listing',
  templateUrl: './currency-listing.component.html',
  styleUrls: ['./currency-listing.component.scss']
})
export class CurrencyListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(CurrencyAddEditComponent) addEditCurrencyComponent!: CurrencyAddEditComponent;
  @ViewChild(CurrencyTableComponent) currencyTableComponent!: CurrencyTableComponent;

  public Currencies: CurrencyData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalCurrencyCount$ : Observable<number> | null = null;
  public filteredCurrencyCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private currencyService: CurrencyService,
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
    // Subscribe to the currencyChanged observable on the add/edit component so that when a Currency changes we can reload the list.
    //
    this.addEditCurrencyComponent.currencyChanged.subscribe({
      next: (result: CurrencyData[] | null) => {
        this.currencyTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Currency changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditCurrencyComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalCurrencyCount$ = this.currencyService.GetCurrenciesRowCount({
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

      this.filteredCurrencyCount$ = this.currencyService.GetCurrenciesRowCount({
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

      this.filteredCurrencyCount$ = this.totalCurrencyCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalCurrencyCount$.subscribe();

    if (this.filteredCurrencyCount$ != this.totalCurrencyCount$) {
      this.filteredCurrencyCount$.subscribe();
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
      this.currencyTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.currencyTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerCurrencyReader(): boolean {
    return this.currencyService.userIsSchedulerCurrencyReader();
  }

  public userIsSchedulerCurrencyWriter(): boolean {
    return this.currencyService.userIsSchedulerCurrencyWriter();
  }
}

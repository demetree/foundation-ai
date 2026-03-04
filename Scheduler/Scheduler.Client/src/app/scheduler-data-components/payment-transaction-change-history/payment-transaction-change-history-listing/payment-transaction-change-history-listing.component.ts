/*
   GENERATED FORM FOR THE PAYMENTTRANSACTIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PaymentTransactionChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to payment-transaction-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { PaymentTransactionChangeHistoryService, PaymentTransactionChangeHistoryData } from '../../../scheduler-data-services/payment-transaction-change-history.service';
import { PaymentTransactionChangeHistoryAddEditComponent } from '../payment-transaction-change-history-add-edit/payment-transaction-change-history-add-edit.component';
import { PaymentTransactionChangeHistoryTableComponent } from '../payment-transaction-change-history-table/payment-transaction-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-payment-transaction-change-history-listing',
  templateUrl: './payment-transaction-change-history-listing.component.html',
  styleUrls: ['./payment-transaction-change-history-listing.component.scss']
})
export class PaymentTransactionChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(PaymentTransactionChangeHistoryAddEditComponent) addEditPaymentTransactionChangeHistoryComponent!: PaymentTransactionChangeHistoryAddEditComponent;
  @ViewChild(PaymentTransactionChangeHistoryTableComponent) paymentTransactionChangeHistoryTableComponent!: PaymentTransactionChangeHistoryTableComponent;

  public PaymentTransactionChangeHistories: PaymentTransactionChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalPaymentTransactionChangeHistoryCount$ : Observable<number> | null = null;
  public filteredPaymentTransactionChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private paymentTransactionChangeHistoryService: PaymentTransactionChangeHistoryService,
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
    // Subscribe to the paymentTransactionChangeHistoryChanged observable on the add/edit component so that when a PaymentTransactionChangeHistory changes we can reload the list.
    //
    this.addEditPaymentTransactionChangeHistoryComponent.paymentTransactionChangeHistoryChanged.subscribe({
      next: (result: PaymentTransactionChangeHistoryData[] | null) => {
        this.paymentTransactionChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Payment Transaction Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditPaymentTransactionChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalPaymentTransactionChangeHistoryCount$ = this.paymentTransactionChangeHistoryService.GetPaymentTransactionChangeHistoriesRowCount({
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

      this.filteredPaymentTransactionChangeHistoryCount$ = this.paymentTransactionChangeHistoryService.GetPaymentTransactionChangeHistoriesRowCount({
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

      this.filteredPaymentTransactionChangeHistoryCount$ = this.totalPaymentTransactionChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalPaymentTransactionChangeHistoryCount$.subscribe();

    if (this.filteredPaymentTransactionChangeHistoryCount$ != this.totalPaymentTransactionChangeHistoryCount$) {
      this.filteredPaymentTransactionChangeHistoryCount$.subscribe();
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
      this.paymentTransactionChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.paymentTransactionChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerPaymentTransactionChangeHistoryReader(): boolean {
    return this.paymentTransactionChangeHistoryService.userIsSchedulerPaymentTransactionChangeHistoryReader();
  }

  public userIsSchedulerPaymentTransactionChangeHistoryWriter(): boolean {
    return this.paymentTransactionChangeHistoryService.userIsSchedulerPaymentTransactionChangeHistoryWriter();
  }
}

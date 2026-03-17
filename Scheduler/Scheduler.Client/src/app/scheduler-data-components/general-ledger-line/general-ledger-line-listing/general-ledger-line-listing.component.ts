/*
   GENERATED FORM FOR THE GENERALLEDGERLINE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from GeneralLedgerLine table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to general-ledger-line-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { GeneralLedgerLineService, GeneralLedgerLineData } from '../../../scheduler-data-services/general-ledger-line.service';
import { GeneralLedgerLineAddEditComponent } from '../general-ledger-line-add-edit/general-ledger-line-add-edit.component';
import { GeneralLedgerLineTableComponent } from '../general-ledger-line-table/general-ledger-line-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-general-ledger-line-listing',
  templateUrl: './general-ledger-line-listing.component.html',
  styleUrls: ['./general-ledger-line-listing.component.scss']
})
export class GeneralLedgerLineListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(GeneralLedgerLineAddEditComponent) addEditGeneralLedgerLineComponent!: GeneralLedgerLineAddEditComponent;
  @ViewChild(GeneralLedgerLineTableComponent) generalLedgerLineTableComponent!: GeneralLedgerLineTableComponent;

  public GeneralLedgerLines: GeneralLedgerLineData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalGeneralLedgerLineCount$ : Observable<number> | null = null;
  public filteredGeneralLedgerLineCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private generalLedgerLineService: GeneralLedgerLineService,
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
    // Subscribe to the generalLedgerLineChanged observable on the add/edit component so that when a GeneralLedgerLine changes we can reload the list.
    //
    this.addEditGeneralLedgerLineComponent.generalLedgerLineChanged.subscribe({
      next: (result: GeneralLedgerLineData[] | null) => {
        this.generalLedgerLineTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during General Ledger Line changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditGeneralLedgerLineComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalGeneralLedgerLineCount$ = this.generalLedgerLineService.GetGeneralLedgerLinesRowCount({
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

      this.filteredGeneralLedgerLineCount$ = this.generalLedgerLineService.GetGeneralLedgerLinesRowCount({
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

      this.filteredGeneralLedgerLineCount$ = this.totalGeneralLedgerLineCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalGeneralLedgerLineCount$.subscribe();

    if (this.filteredGeneralLedgerLineCount$ != this.totalGeneralLedgerLineCount$) {
      this.filteredGeneralLedgerLineCount$.subscribe();
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
      this.generalLedgerLineTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.generalLedgerLineTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerGeneralLedgerLineReader(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineReader();
  }

  public userIsSchedulerGeneralLedgerLineWriter(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter();
  }
}

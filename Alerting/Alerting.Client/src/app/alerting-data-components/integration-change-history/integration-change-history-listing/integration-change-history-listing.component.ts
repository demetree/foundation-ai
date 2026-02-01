/*
   GENERATED FORM FOR THE INTEGRATIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IntegrationChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to integration-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { IntegrationChangeHistoryService, IntegrationChangeHistoryData } from '../../../alerting-data-services/integration-change-history.service';
import { IntegrationChangeHistoryAddEditComponent } from '../integration-change-history-add-edit/integration-change-history-add-edit.component';
import { IntegrationChangeHistoryTableComponent } from '../integration-change-history-table/integration-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-integration-change-history-listing',
  templateUrl: './integration-change-history-listing.component.html',
  styleUrls: ['./integration-change-history-listing.component.scss']
})
export class IntegrationChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(IntegrationChangeHistoryAddEditComponent) addEditIntegrationChangeHistoryComponent!: IntegrationChangeHistoryAddEditComponent;
  @ViewChild(IntegrationChangeHistoryTableComponent) integrationChangeHistoryTableComponent!: IntegrationChangeHistoryTableComponent;

  public IntegrationChangeHistories: IntegrationChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalIntegrationChangeHistoryCount$ : Observable<number> | null = null;
  public filteredIntegrationChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private integrationChangeHistoryService: IntegrationChangeHistoryService,
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
    // Subscribe to the integrationChangeHistoryChanged observable on the add/edit component so that when a IntegrationChangeHistory changes we can reload the list.
    //
    this.addEditIntegrationChangeHistoryComponent.integrationChangeHistoryChanged.subscribe({
      next: (result: IntegrationChangeHistoryData[] | null) => {
        this.integrationChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Integration Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditIntegrationChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalIntegrationChangeHistoryCount$ = this.integrationChangeHistoryService.GetIntegrationChangeHistoriesRowCount({
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

      this.filteredIntegrationChangeHistoryCount$ = this.integrationChangeHistoryService.GetIntegrationChangeHistoriesRowCount({
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

      this.filteredIntegrationChangeHistoryCount$ = this.totalIntegrationChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalIntegrationChangeHistoryCount$.subscribe();

    if (this.filteredIntegrationChangeHistoryCount$ != this.totalIntegrationChangeHistoryCount$) {
      this.filteredIntegrationChangeHistoryCount$.subscribe();
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
      this.integrationChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAlertingIntegrationChangeHistoryReader(): boolean {
    return this.integrationChangeHistoryService.userIsAlertingIntegrationChangeHistoryReader();
  }

  public userIsAlertingIntegrationChangeHistoryWriter(): boolean {
    return this.integrationChangeHistoryService.userIsAlertingIntegrationChangeHistoryWriter();
  }
}

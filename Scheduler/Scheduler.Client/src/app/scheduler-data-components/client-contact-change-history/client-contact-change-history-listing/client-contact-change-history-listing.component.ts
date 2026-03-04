/*
   GENERATED FORM FOR THE CLIENTCONTACTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ClientContactChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to client-contact-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ClientContactChangeHistoryService, ClientContactChangeHistoryData } from '../../../scheduler-data-services/client-contact-change-history.service';
import { ClientContactChangeHistoryAddEditComponent } from '../client-contact-change-history-add-edit/client-contact-change-history-add-edit.component';
import { ClientContactChangeHistoryTableComponent } from '../client-contact-change-history-table/client-contact-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-client-contact-change-history-listing',
  templateUrl: './client-contact-change-history-listing.component.html',
  styleUrls: ['./client-contact-change-history-listing.component.scss']
})
export class ClientContactChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ClientContactChangeHistoryAddEditComponent) addEditClientContactChangeHistoryComponent!: ClientContactChangeHistoryAddEditComponent;
  @ViewChild(ClientContactChangeHistoryTableComponent) clientContactChangeHistoryTableComponent!: ClientContactChangeHistoryTableComponent;

  public ClientContactChangeHistories: ClientContactChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalClientContactChangeHistoryCount$ : Observable<number> | null = null;
  public filteredClientContactChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private clientContactChangeHistoryService: ClientContactChangeHistoryService,
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
    // Subscribe to the clientContactChangeHistoryChanged observable on the add/edit component so that when a ClientContactChangeHistory changes we can reload the list.
    //
    this.addEditClientContactChangeHistoryComponent.clientContactChangeHistoryChanged.subscribe({
      next: (result: ClientContactChangeHistoryData[] | null) => {
        this.clientContactChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Client Contact Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditClientContactChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalClientContactChangeHistoryCount$ = this.clientContactChangeHistoryService.GetClientContactChangeHistoriesRowCount({
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

      this.filteredClientContactChangeHistoryCount$ = this.clientContactChangeHistoryService.GetClientContactChangeHistoriesRowCount({
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

      this.filteredClientContactChangeHistoryCount$ = this.totalClientContactChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalClientContactChangeHistoryCount$.subscribe();

    if (this.filteredClientContactChangeHistoryCount$ != this.totalClientContactChangeHistoryCount$) {
      this.filteredClientContactChangeHistoryCount$.subscribe();
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
      this.clientContactChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.clientContactChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerClientContactChangeHistoryReader(): boolean {
    return this.clientContactChangeHistoryService.userIsSchedulerClientContactChangeHistoryReader();
  }

  public userIsSchedulerClientContactChangeHistoryWriter(): boolean {
    return this.clientContactChangeHistoryService.userIsSchedulerClientContactChangeHistoryWriter();
  }
}

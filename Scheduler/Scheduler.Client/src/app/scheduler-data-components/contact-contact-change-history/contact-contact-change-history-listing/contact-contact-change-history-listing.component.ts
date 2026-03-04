/*
   GENERATED FORM FOR THE CONTACTCONTACTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactContactChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-contact-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ContactContactChangeHistoryService, ContactContactChangeHistoryData } from '../../../scheduler-data-services/contact-contact-change-history.service';
import { ContactContactChangeHistoryAddEditComponent } from '../contact-contact-change-history-add-edit/contact-contact-change-history-add-edit.component';
import { ContactContactChangeHistoryTableComponent } from '../contact-contact-change-history-table/contact-contact-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-contact-contact-change-history-listing',
  templateUrl: './contact-contact-change-history-listing.component.html',
  styleUrls: ['./contact-contact-change-history-listing.component.scss']
})
export class ContactContactChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ContactContactChangeHistoryAddEditComponent) addEditContactContactChangeHistoryComponent!: ContactContactChangeHistoryAddEditComponent;
  @ViewChild(ContactContactChangeHistoryTableComponent) contactContactChangeHistoryTableComponent!: ContactContactChangeHistoryTableComponent;

  public ContactContactChangeHistories: ContactContactChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalContactContactChangeHistoryCount$ : Observable<number> | null = null;
  public filteredContactContactChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private contactContactChangeHistoryService: ContactContactChangeHistoryService,
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
    // Subscribe to the contactContactChangeHistoryChanged observable on the add/edit component so that when a ContactContactChangeHistory changes we can reload the list.
    //
    this.addEditContactContactChangeHistoryComponent.contactContactChangeHistoryChanged.subscribe({
      next: (result: ContactContactChangeHistoryData[] | null) => {
        this.contactContactChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Contact Contact Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditContactContactChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalContactContactChangeHistoryCount$ = this.contactContactChangeHistoryService.GetContactContactChangeHistoriesRowCount({
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

      this.filteredContactContactChangeHistoryCount$ = this.contactContactChangeHistoryService.GetContactContactChangeHistoriesRowCount({
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

      this.filteredContactContactChangeHistoryCount$ = this.totalContactContactChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalContactContactChangeHistoryCount$.subscribe();

    if (this.filteredContactContactChangeHistoryCount$ != this.totalContactContactChangeHistoryCount$) {
      this.filteredContactContactChangeHistoryCount$.subscribe();
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
      this.contactContactChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.contactContactChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerContactContactChangeHistoryReader(): boolean {
    return this.contactContactChangeHistoryService.userIsSchedulerContactContactChangeHistoryReader();
  }

  public userIsSchedulerContactContactChangeHistoryWriter(): boolean {
    return this.contactContactChangeHistoryService.userIsSchedulerContactContactChangeHistoryWriter();
  }
}

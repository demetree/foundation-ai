/*
   GENERATED FORM FOR THE CONTACTINTERACTIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactInteractionChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-interaction-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ContactInteractionChangeHistoryService, ContactInteractionChangeHistoryData } from '../../../scheduler-data-services/contact-interaction-change-history.service';
import { ContactInteractionChangeHistoryAddEditComponent } from '../contact-interaction-change-history-add-edit/contact-interaction-change-history-add-edit.component';
import { ContactInteractionChangeHistoryTableComponent } from '../contact-interaction-change-history-table/contact-interaction-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-contact-interaction-change-history-listing',
  templateUrl: './contact-interaction-change-history-listing.component.html',
  styleUrls: ['./contact-interaction-change-history-listing.component.scss']
})
export class ContactInteractionChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ContactInteractionChangeHistoryAddEditComponent) addEditContactInteractionChangeHistoryComponent!: ContactInteractionChangeHistoryAddEditComponent;
  @ViewChild(ContactInteractionChangeHistoryTableComponent) contactInteractionChangeHistoryTableComponent!: ContactInteractionChangeHistoryTableComponent;

  public ContactInteractionChangeHistories: ContactInteractionChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalContactInteractionChangeHistoryCount$ : Observable<number> | null = null;
  public filteredContactInteractionChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private contactInteractionChangeHistoryService: ContactInteractionChangeHistoryService,
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
    // Subscribe to the contactInteractionChangeHistoryChanged observable on the add/edit component so that when a ContactInteractionChangeHistory changes we can reload the list.
    //
    this.addEditContactInteractionChangeHistoryComponent.contactInteractionChangeHistoryChanged.subscribe({
      next: (result: ContactInteractionChangeHistoryData[] | null) => {
        this.contactInteractionChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Contact Interaction Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditContactInteractionChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalContactInteractionChangeHistoryCount$ = this.contactInteractionChangeHistoryService.GetContactInteractionChangeHistoriesRowCount({
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

      this.filteredContactInteractionChangeHistoryCount$ = this.contactInteractionChangeHistoryService.GetContactInteractionChangeHistoriesRowCount({
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

      this.filteredContactInteractionChangeHistoryCount$ = this.totalContactInteractionChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalContactInteractionChangeHistoryCount$.subscribe();

    if (this.filteredContactInteractionChangeHistoryCount$ != this.totalContactInteractionChangeHistoryCount$) {
      this.filteredContactInteractionChangeHistoryCount$.subscribe();
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
      this.contactInteractionChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.contactInteractionChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerContactInteractionChangeHistoryReader(): boolean {
    return this.contactInteractionChangeHistoryService.userIsSchedulerContactInteractionChangeHistoryReader();
  }

  public userIsSchedulerContactInteractionChangeHistoryWriter(): boolean {
    return this.contactInteractionChangeHistoryService.userIsSchedulerContactInteractionChangeHistoryWriter();
  }
}

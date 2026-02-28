/*
   GENERATED FORM FOR THE USERSETLISTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSetListChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-set-list-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { UserSetListChangeHistoryService, UserSetListChangeHistoryData } from '../../../bmc-data-services/user-set-list-change-history.service';
import { UserSetListChangeHistoryAddEditComponent } from '../user-set-list-change-history-add-edit/user-set-list-change-history-add-edit.component';
import { UserSetListChangeHistoryTableComponent } from '../user-set-list-change-history-table/user-set-list-change-history-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-user-set-list-change-history-listing',
  templateUrl: './user-set-list-change-history-listing.component.html',
  styleUrls: ['./user-set-list-change-history-listing.component.scss']
})
export class UserSetListChangeHistoryListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(UserSetListChangeHistoryAddEditComponent) addEditUserSetListChangeHistoryComponent!: UserSetListChangeHistoryAddEditComponent;
  @ViewChild(UserSetListChangeHistoryTableComponent) userSetListChangeHistoryTableComponent!: UserSetListChangeHistoryTableComponent;

  public UserSetListChangeHistories: UserSetListChangeHistoryData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalUserSetListChangeHistoryCount$ : Observable<number> | null = null;
  public filteredUserSetListChangeHistoryCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private userSetListChangeHistoryService: UserSetListChangeHistoryService,
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
    // Subscribe to the userSetListChangeHistoryChanged observable on the add/edit component so that when a UserSetListChangeHistory changes we can reload the list.
    //
    this.addEditUserSetListChangeHistoryComponent.userSetListChangeHistoryChanged.subscribe({
      next: (result: UserSetListChangeHistoryData[] | null) => {
        this.userSetListChangeHistoryTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during User Set List Change History changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditUserSetListChangeHistoryComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalUserSetListChangeHistoryCount$ = this.userSetListChangeHistoryService.GetUserSetListChangeHistoriesRowCount({
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

      this.filteredUserSetListChangeHistoryCount$ = this.userSetListChangeHistoryService.GetUserSetListChangeHistoriesRowCount({
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

      this.filteredUserSetListChangeHistoryCount$ = this.totalUserSetListChangeHistoryCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalUserSetListChangeHistoryCount$.subscribe();

    if (this.filteredUserSetListChangeHistoryCount$ != this.totalUserSetListChangeHistoryCount$) {
      this.filteredUserSetListChangeHistoryCount$.subscribe();
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
      this.userSetListChangeHistoryTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.userSetListChangeHistoryTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCUserSetListChangeHistoryReader(): boolean {
    return this.userSetListChangeHistoryService.userIsBMCUserSetListChangeHistoryReader();
  }

  public userIsBMCUserSetListChangeHistoryWriter(): boolean {
    return this.userSetListChangeHistoryService.userIsBMCUserSetListChangeHistoryWriter();
  }
}

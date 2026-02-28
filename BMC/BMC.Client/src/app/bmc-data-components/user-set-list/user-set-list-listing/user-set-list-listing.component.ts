/*
   GENERATED FORM FOR THE USERSETLIST TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSetList table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-set-list-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { UserSetListService, UserSetListData } from '../../../bmc-data-services/user-set-list.service';
import { UserSetListAddEditComponent } from '../user-set-list-add-edit/user-set-list-add-edit.component';
import { UserSetListTableComponent } from '../user-set-list-table/user-set-list-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-user-set-list-listing',
  templateUrl: './user-set-list-listing.component.html',
  styleUrls: ['./user-set-list-listing.component.scss']
})
export class UserSetListListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(UserSetListAddEditComponent) addEditUserSetListComponent!: UserSetListAddEditComponent;
  @ViewChild(UserSetListTableComponent) userSetListTableComponent!: UserSetListTableComponent;

  public UserSetLists: UserSetListData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalUserSetListCount$ : Observable<number> | null = null;
  public filteredUserSetListCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private userSetListService: UserSetListService,
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
    // Subscribe to the userSetListChanged observable on the add/edit component so that when a UserSetList changes we can reload the list.
    //
    this.addEditUserSetListComponent.userSetListChanged.subscribe({
      next: (result: UserSetListData[] | null) => {
        this.userSetListTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during User Set List changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditUserSetListComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalUserSetListCount$ = this.userSetListService.GetUserSetListsRowCount({
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

      this.filteredUserSetListCount$ = this.userSetListService.GetUserSetListsRowCount({
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

      this.filteredUserSetListCount$ = this.totalUserSetListCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalUserSetListCount$.subscribe();

    if (this.filteredUserSetListCount$ != this.totalUserSetListCount$) {
      this.filteredUserSetListCount$.subscribe();
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
      this.userSetListTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.userSetListTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCUserSetListReader(): boolean {
    return this.userSetListService.userIsBMCUserSetListReader();
  }

  public userIsBMCUserSetListWriter(): boolean {
    return this.userSetListService.userIsBMCUserSetListWriter();
  }
}

/*
   GENERATED FORM FOR THE USERCOLLECTIONSETIMPORT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserCollectionSetImport table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-collection-set-import-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { UserCollectionSetImportService, UserCollectionSetImportData } from '../../../bmc-data-services/user-collection-set-import.service';
import { UserCollectionSetImportAddEditComponent } from '../user-collection-set-import-add-edit/user-collection-set-import-add-edit.component';
import { UserCollectionSetImportTableComponent } from '../user-collection-set-import-table/user-collection-set-import-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-user-collection-set-import-listing',
  templateUrl: './user-collection-set-import-listing.component.html',
  styleUrls: ['./user-collection-set-import-listing.component.scss']
})
export class UserCollectionSetImportListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(UserCollectionSetImportAddEditComponent) addEditUserCollectionSetImportComponent!: UserCollectionSetImportAddEditComponent;
  @ViewChild(UserCollectionSetImportTableComponent) userCollectionSetImportTableComponent!: UserCollectionSetImportTableComponent;

  public UserCollectionSetImports: UserCollectionSetImportData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalUserCollectionSetImportCount$ : Observable<number> | null = null;
  public filteredUserCollectionSetImportCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private userCollectionSetImportService: UserCollectionSetImportService,
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
    // Subscribe to the userCollectionSetImportChanged observable on the add/edit component so that when a UserCollectionSetImport changes we can reload the list.
    //
    this.addEditUserCollectionSetImportComponent.userCollectionSetImportChanged.subscribe({
      next: (result: UserCollectionSetImportData[] | null) => {
        this.userCollectionSetImportTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during User Collection Set Import changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditUserCollectionSetImportComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalUserCollectionSetImportCount$ = this.userCollectionSetImportService.GetUserCollectionSetImportsRowCount({
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

      this.filteredUserCollectionSetImportCount$ = this.userCollectionSetImportService.GetUserCollectionSetImportsRowCount({
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

      this.filteredUserCollectionSetImportCount$ = this.totalUserCollectionSetImportCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalUserCollectionSetImportCount$.subscribe();

    if (this.filteredUserCollectionSetImportCount$ != this.totalUserCollectionSetImportCount$) {
      this.filteredUserCollectionSetImportCount$.subscribe();
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
      this.userCollectionSetImportTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.userCollectionSetImportTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCUserCollectionSetImportReader(): boolean {
    return this.userCollectionSetImportService.userIsBMCUserCollectionSetImportReader();
  }

  public userIsBMCUserCollectionSetImportWriter(): boolean {
    return this.userCollectionSetImportService.userIsBMCUserCollectionSetImportWriter();
  }
}

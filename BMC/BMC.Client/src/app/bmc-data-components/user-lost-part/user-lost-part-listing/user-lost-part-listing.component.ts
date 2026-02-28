/*
   GENERATED FORM FOR THE USERLOSTPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserLostPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-lost-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { UserLostPartService, UserLostPartData } from '../../../bmc-data-services/user-lost-part.service';
import { UserLostPartAddEditComponent } from '../user-lost-part-add-edit/user-lost-part-add-edit.component';
import { UserLostPartTableComponent } from '../user-lost-part-table/user-lost-part-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-user-lost-part-listing',
  templateUrl: './user-lost-part-listing.component.html',
  styleUrls: ['./user-lost-part-listing.component.scss']
})
export class UserLostPartListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(UserLostPartAddEditComponent) addEditUserLostPartComponent!: UserLostPartAddEditComponent;
  @ViewChild(UserLostPartTableComponent) userLostPartTableComponent!: UserLostPartTableComponent;

  public UserLostParts: UserLostPartData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalUserLostPartCount$ : Observable<number> | null = null;
  public filteredUserLostPartCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private userLostPartService: UserLostPartService,
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
    // Subscribe to the userLostPartChanged observable on the add/edit component so that when a UserLostPart changes we can reload the list.
    //
    this.addEditUserLostPartComponent.userLostPartChanged.subscribe({
      next: (result: UserLostPartData[] | null) => {
        this.userLostPartTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during User Lost Part changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditUserLostPartComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalUserLostPartCount$ = this.userLostPartService.GetUserLostPartsRowCount({
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

      this.filteredUserLostPartCount$ = this.userLostPartService.GetUserLostPartsRowCount({
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

      this.filteredUserLostPartCount$ = this.totalUserLostPartCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalUserLostPartCount$.subscribe();

    if (this.filteredUserLostPartCount$ != this.totalUserLostPartCount$) {
      this.filteredUserLostPartCount$.subscribe();
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
      this.userLostPartTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.userLostPartTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCUserLostPartReader(): boolean {
    return this.userLostPartService.userIsBMCUserLostPartReader();
  }

  public userIsBMCUserLostPartWriter(): boolean {
    return this.userLostPartService.userIsBMCUserLostPartWriter();
  }
}

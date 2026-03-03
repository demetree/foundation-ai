/*
   GENERATED FORM FOR THE BRICKLINKUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickLinkUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-link-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { BrickLinkUserLinkService, BrickLinkUserLinkData } from '../../../bmc-data-services/brick-link-user-link.service';
import { BrickLinkUserLinkAddEditComponent } from '../brick-link-user-link-add-edit/brick-link-user-link-add-edit.component';
import { BrickLinkUserLinkTableComponent } from '../brick-link-user-link-table/brick-link-user-link-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-brick-link-user-link-listing',
  templateUrl: './brick-link-user-link-listing.component.html',
  styleUrls: ['./brick-link-user-link-listing.component.scss']
})
export class BrickLinkUserLinkListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(BrickLinkUserLinkAddEditComponent) addEditBrickLinkUserLinkComponent!: BrickLinkUserLinkAddEditComponent;
  @ViewChild(BrickLinkUserLinkTableComponent) brickLinkUserLinkTableComponent!: BrickLinkUserLinkTableComponent;

  public BrickLinkUserLinks: BrickLinkUserLinkData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalBrickLinkUserLinkCount$ : Observable<number> | null = null;
  public filteredBrickLinkUserLinkCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private brickLinkUserLinkService: BrickLinkUserLinkService,
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
    // Subscribe to the brickLinkUserLinkChanged observable on the add/edit component so that when a BrickLinkUserLink changes we can reload the list.
    //
    this.addEditBrickLinkUserLinkComponent.brickLinkUserLinkChanged.subscribe({
      next: (result: BrickLinkUserLinkData[] | null) => {
        this.brickLinkUserLinkTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Brick Link User Link changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditBrickLinkUserLinkComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalBrickLinkUserLinkCount$ = this.brickLinkUserLinkService.GetBrickLinkUserLinksRowCount({
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

      this.filteredBrickLinkUserLinkCount$ = this.brickLinkUserLinkService.GetBrickLinkUserLinksRowCount({
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

      this.filteredBrickLinkUserLinkCount$ = this.totalBrickLinkUserLinkCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalBrickLinkUserLinkCount$.subscribe();

    if (this.filteredBrickLinkUserLinkCount$ != this.totalBrickLinkUserLinkCount$) {
      this.filteredBrickLinkUserLinkCount$.subscribe();
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
      this.brickLinkUserLinkTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.brickLinkUserLinkTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCBrickLinkUserLinkReader(): boolean {
    return this.brickLinkUserLinkService.userIsBMCBrickLinkUserLinkReader();
  }

  public userIsBMCBrickLinkUserLinkWriter(): boolean {
    return this.brickLinkUserLinkService.userIsBMCBrickLinkUserLinkWriter();
  }
}

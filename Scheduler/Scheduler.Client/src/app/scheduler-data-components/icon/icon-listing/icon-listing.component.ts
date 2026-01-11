/*
   GENERATED FORM FOR THE ICON TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Icon table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to icon-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { IconService, IconData } from '../../../scheduler-data-services/icon.service';
import { IconAddEditComponent } from '../icon-add-edit/icon-add-edit.component';
import { IconTableComponent } from '../icon-table/icon-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-icon-listing',
  templateUrl: './icon-listing.component.html',
  styleUrls: ['./icon-listing.component.scss']
})
export class IconListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(IconAddEditComponent) addEditIconComponent!: IconAddEditComponent;
  @ViewChild(IconTableComponent) iconTableComponent!: IconTableComponent;

  public Icons: IconData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalIconCount$ : Observable<number> | null = null;
  public filteredIconCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private iconService: IconService,
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
    // Subscribe to the iconChanged observable on the add/edit component so that when a Icon changes we can reload the list.
    //
    this.addEditIconComponent.iconChanged.subscribe({
      next: (result: IconData[] | null) => {
        this.iconTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Icon changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditIconComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalIconCount$ = this.iconService.GetIconsRowCount({
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

      this.filteredIconCount$ = this.iconService.GetIconsRowCount({
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

      this.filteredIconCount$ = this.totalIconCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalIconCount$.subscribe();

    if (this.filteredIconCount$ != this.totalIconCount$) {
      this.filteredIconCount$.subscribe();
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
      this.iconTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerIconReader(): boolean {
    return this.iconService.userIsSchedulerIconReader();
  }

  public userIsSchedulerIconWriter(): boolean {
    return this.iconService.userIsSchedulerIconWriter();
  }
}

/*
   GENERATED FORM FOR THE TRIBUTETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TributeType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tribute-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { TributeTypeService, TributeTypeData } from '../../../scheduler-data-services/tribute-type.service';
import { TributeTypeAddEditComponent } from '../tribute-type-add-edit/tribute-type-add-edit.component';
import { TributeTypeTableComponent } from '../tribute-type-table/tribute-type-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-tribute-type-listing',
  templateUrl: './tribute-type-listing.component.html',
  styleUrls: ['./tribute-type-listing.component.scss']
})
export class TributeTypeListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(TributeTypeAddEditComponent) addEditTributeTypeComponent!: TributeTypeAddEditComponent;
  @ViewChild(TributeTypeTableComponent) tributeTypeTableComponent!: TributeTypeTableComponent;

  public TributeTypes: TributeTypeData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalTributeTypeCount$ : Observable<number> | null = null;
  public filteredTributeTypeCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private tributeTypeService: TributeTypeService,
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
    // Subscribe to the tributeTypeChanged observable on the add/edit component so that when a TributeType changes we can reload the list.
    //
    this.addEditTributeTypeComponent.tributeTypeChanged.subscribe({
      next: (result: TributeTypeData[] | null) => {
        this.tributeTypeTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Tribute Type changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditTributeTypeComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalTributeTypeCount$ = this.tributeTypeService.GetTributeTypesRowCount({
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

      this.filteredTributeTypeCount$ = this.tributeTypeService.GetTributeTypesRowCount({
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

      this.filteredTributeTypeCount$ = this.totalTributeTypeCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalTributeTypeCount$.subscribe();

    if (this.filteredTributeTypeCount$ != this.totalTributeTypeCount$) {
      this.filteredTributeTypeCount$.subscribe();
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
      this.tributeTypeTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.tributeTypeTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerTributeTypeReader(): boolean {
    return this.tributeTypeService.userIsSchedulerTributeTypeReader();
  }

  public userIsSchedulerTributeTypeWriter(): boolean {
    return this.tributeTypeService.userIsSchedulerTributeTypeWriter();
  }
}

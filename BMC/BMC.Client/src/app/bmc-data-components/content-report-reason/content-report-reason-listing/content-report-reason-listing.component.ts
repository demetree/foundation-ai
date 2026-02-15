/*
   GENERATED FORM FOR THE CONTENTREPORTREASON TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContentReportReason table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to content-report-reason-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ContentReportReasonService, ContentReportReasonData } from '../../../bmc-data-services/content-report-reason.service';
import { ContentReportReasonAddEditComponent } from '../content-report-reason-add-edit/content-report-reason-add-edit.component';
import { ContentReportReasonTableComponent } from '../content-report-reason-table/content-report-reason-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-content-report-reason-listing',
  templateUrl: './content-report-reason-listing.component.html',
  styleUrls: ['./content-report-reason-listing.component.scss']
})
export class ContentReportReasonListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ContentReportReasonAddEditComponent) addEditContentReportReasonComponent!: ContentReportReasonAddEditComponent;
  @ViewChild(ContentReportReasonTableComponent) contentReportReasonTableComponent!: ContentReportReasonTableComponent;

  public ContentReportReasons: ContentReportReasonData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalContentReportReasonCount$ : Observable<number> | null = null;
  public filteredContentReportReasonCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private contentReportReasonService: ContentReportReasonService,
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
    // Subscribe to the contentReportReasonChanged observable on the add/edit component so that when a ContentReportReason changes we can reload the list.
    //
    this.addEditContentReportReasonComponent.contentReportReasonChanged.subscribe({
      next: (result: ContentReportReasonData[] | null) => {
        this.contentReportReasonTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Content Report Reason changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditContentReportReasonComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalContentReportReasonCount$ = this.contentReportReasonService.GetContentReportReasonsRowCount({
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

      this.filteredContentReportReasonCount$ = this.contentReportReasonService.GetContentReportReasonsRowCount({
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

      this.filteredContentReportReasonCount$ = this.totalContentReportReasonCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalContentReportReasonCount$.subscribe();

    if (this.filteredContentReportReasonCount$ != this.totalContentReportReasonCount$) {
      this.filteredContentReportReasonCount$.subscribe();
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
      this.contentReportReasonTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCContentReportReasonReader(): boolean {
    return this.contentReportReasonService.userIsBMCContentReportReasonReader();
  }

  public userIsBMCContentReportReasonWriter(): boolean {
    return this.contentReportReasonService.userIsBMCContentReportReasonWriter();
  }
}

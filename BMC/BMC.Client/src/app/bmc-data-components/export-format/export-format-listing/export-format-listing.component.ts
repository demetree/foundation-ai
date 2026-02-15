/*
   GENERATED FORM FOR THE EXPORTFORMAT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ExportFormat table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to export-format-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ExportFormatService, ExportFormatData } from '../../../bmc-data-services/export-format.service';
import { ExportFormatAddEditComponent } from '../export-format-add-edit/export-format-add-edit.component';
import { ExportFormatTableComponent } from '../export-format-table/export-format-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-export-format-listing',
  templateUrl: './export-format-listing.component.html',
  styleUrls: ['./export-format-listing.component.scss']
})
export class ExportFormatListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ExportFormatAddEditComponent) addEditExportFormatComponent!: ExportFormatAddEditComponent;
  @ViewChild(ExportFormatTableComponent) exportFormatTableComponent!: ExportFormatTableComponent;

  public ExportFormats: ExportFormatData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalExportFormatCount$ : Observable<number> | null = null;
  public filteredExportFormatCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private exportFormatService: ExportFormatService,
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
    // Subscribe to the exportFormatChanged observable on the add/edit component so that when a ExportFormat changes we can reload the list.
    //
    this.addEditExportFormatComponent.exportFormatChanged.subscribe({
      next: (result: ExportFormatData[] | null) => {
        this.exportFormatTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Export Format changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditExportFormatComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalExportFormatCount$ = this.exportFormatService.GetExportFormatsRowCount({
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

      this.filteredExportFormatCount$ = this.exportFormatService.GetExportFormatsRowCount({
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

      this.filteredExportFormatCount$ = this.totalExportFormatCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalExportFormatCount$.subscribe();

    if (this.filteredExportFormatCount$ != this.totalExportFormatCount$) {
      this.filteredExportFormatCount$.subscribe();
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
      this.exportFormatTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCExportFormatReader(): boolean {
    return this.exportFormatService.userIsBMCExportFormatReader();
  }

  public userIsBMCExportFormatWriter(): boolean {
    return this.exportFormatService.userIsBMCExportFormatWriter();
  }
}

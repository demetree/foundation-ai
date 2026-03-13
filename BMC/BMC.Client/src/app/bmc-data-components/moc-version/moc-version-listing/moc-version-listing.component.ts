/*
   GENERATED FORM FOR THE MOCVERSION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocVersion table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-version-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { MocVersionService, MocVersionData } from '../../../bmc-data-services/moc-version.service';
import { MocVersionAddEditComponent } from '../moc-version-add-edit/moc-version-add-edit.component';
import { MocVersionTableComponent } from '../moc-version-table/moc-version-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-moc-version-listing',
  templateUrl: './moc-version-listing.component.html',
  styleUrls: ['./moc-version-listing.component.scss']
})
export class MocVersionListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(MocVersionAddEditComponent) addEditMocVersionComponent!: MocVersionAddEditComponent;
  @ViewChild(MocVersionTableComponent) mocVersionTableComponent!: MocVersionTableComponent;

  public MocVersions: MocVersionData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalMocVersionCount$ : Observable<number> | null = null;
  public filteredMocVersionCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private mocVersionService: MocVersionService,
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
    // Subscribe to the mocVersionChanged observable on the add/edit component so that when a MocVersion changes we can reload the list.
    //
    this.addEditMocVersionComponent.mocVersionChanged.subscribe({
      next: (result: MocVersionData[] | null) => {
        this.mocVersionTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Moc Version changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditMocVersionComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalMocVersionCount$ = this.mocVersionService.GetMocVersionsRowCount({
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

      this.filteredMocVersionCount$ = this.mocVersionService.GetMocVersionsRowCount({
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

      this.filteredMocVersionCount$ = this.totalMocVersionCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalMocVersionCount$.subscribe();

    if (this.filteredMocVersionCount$ != this.totalMocVersionCount$) {
      this.filteredMocVersionCount$.subscribe();
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
      this.mocVersionTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.mocVersionTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsBMCMocVersionReader(): boolean {
    return this.mocVersionService.userIsBMCMocVersionReader();
  }

  public userIsBMCMocVersionWriter(): boolean {
    return this.mocVersionService.userIsBMCMocVersionWriter();
  }
}

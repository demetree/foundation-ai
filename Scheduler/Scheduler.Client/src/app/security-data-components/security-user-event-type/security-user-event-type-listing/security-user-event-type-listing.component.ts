import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SecurityUserEventTypeService, SecurityUserEventTypeData } from '../../../security-data-services/security-user-event-type.service';
import { SecurityUserEventTypeAddEditComponent } from '../security-user-event-type-add-edit/security-user-event-type-add-edit.component';
import { SecurityUserEventTypeTableComponent } from '../security-user-event-type-table/security-user-event-type-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-security-user-event-type-listing',
  templateUrl: './security-user-event-type-listing.component.html',
  styleUrls: ['./security-user-event-type-listing.component.scss']
})
export class SecurityUserEventTypeListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SecurityUserEventTypeAddEditComponent) addEditSecurityUserEventTypeComponent!: SecurityUserEventTypeAddEditComponent;
  @ViewChild(SecurityUserEventTypeTableComponent) securityUserEventTypeTableComponent!: SecurityUserEventTypeTableComponent;

  public SecurityUserEventTypes: SecurityUserEventTypeData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSecurityUserEventTypeCount$ : Observable<number> | null = null;
  public filteredSecurityUserEventTypeCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private securityUserEventTypeService: SecurityUserEventTypeService,
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
    // Subscribe to the securityUserEventTypeChanged observable on the add/edit component so that when a SecurityUserEventType changes we can reload the list.
    //
    this.addEditSecurityUserEventTypeComponent.securityUserEventTypeChanged.subscribe({
      next: (result: SecurityUserEventTypeData[] | null) => {
        this.securityUserEventTypeTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Security User Event Type changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSecurityUserEventTypeComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSecurityUserEventTypeCount$ = this.securityUserEventTypeService.GetSecurityUserEventTypesRowCount({
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

      this.filteredSecurityUserEventTypeCount$ = this.securityUserEventTypeService.GetSecurityUserEventTypesRowCount({
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

      this.filteredSecurityUserEventTypeCount$ = this.totalSecurityUserEventTypeCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSecurityUserEventTypeCount$.subscribe();

    if (this.filteredSecurityUserEventTypeCount$ != this.totalSecurityUserEventTypeCount$) {
      this.filteredSecurityUserEventTypeCount$.subscribe();
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
      this.securityUserEventTypeTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecuritySecurityUserEventTypeReader(): boolean {
    return this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeReader();
  }

  public userIsSecuritySecurityUserEventTypeWriter(): boolean {
    return this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeWriter();
  }
}

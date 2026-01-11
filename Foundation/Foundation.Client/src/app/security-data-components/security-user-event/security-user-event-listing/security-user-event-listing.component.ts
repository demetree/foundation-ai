import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SecurityUserEventService, SecurityUserEventData } from '../../../security-data-services/security-user-event.service';
import { SecurityUserEventAddEditComponent } from '../security-user-event-add-edit/security-user-event-add-edit.component';
import { SecurityUserEventTableComponent } from '../security-user-event-table/security-user-event-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-security-user-event-listing',
  templateUrl: './security-user-event-listing.component.html',
  styleUrls: ['./security-user-event-listing.component.scss']
})
export class SecurityUserEventListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SecurityUserEventAddEditComponent) addEditSecurityUserEventComponent!: SecurityUserEventAddEditComponent;
  @ViewChild(SecurityUserEventTableComponent) securityUserEventTableComponent!: SecurityUserEventTableComponent;

  public SecurityUserEvents: SecurityUserEventData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSecurityUserEventCount$ : Observable<number> | null = null;
  public filteredSecurityUserEventCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private securityUserEventService: SecurityUserEventService,
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
    // Subscribe to the securityUserEventChanged observable on the add/edit component so that when a SecurityUserEvent changes we can reload the list.
    //
    this.addEditSecurityUserEventComponent.securityUserEventChanged.subscribe({
      next: (result: SecurityUserEventData[] | null) => {
        this.securityUserEventTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Security User Event changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSecurityUserEventComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSecurityUserEventCount$ = this.securityUserEventService.GetSecurityUserEventsRowCount({
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

      this.filteredSecurityUserEventCount$ = this.securityUserEventService.GetSecurityUserEventsRowCount({
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

      this.filteredSecurityUserEventCount$ = this.totalSecurityUserEventCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSecurityUserEventCount$.subscribe();

    if (this.filteredSecurityUserEventCount$ != this.totalSecurityUserEventCount$) {
      this.filteredSecurityUserEventCount$.subscribe();
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
      this.securityUserEventTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecuritySecurityUserEventReader(): boolean {
    return this.securityUserEventService.userIsSecuritySecurityUserEventReader();
  }

  public userIsSecuritySecurityUserEventWriter(): boolean {
    return this.securityUserEventService.userIsSecuritySecurityUserEventWriter();
  }
}

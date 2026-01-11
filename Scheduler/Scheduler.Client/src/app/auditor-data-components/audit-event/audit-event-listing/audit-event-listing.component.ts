import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AuditEventService, AuditEventData } from '../../../auditor-data-services/audit-event.service';
import { AuditEventAddEditComponent } from '../audit-event-add-edit/audit-event-add-edit.component';
import { AuditEventTableComponent } from '../audit-event-table/audit-event-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-audit-event-listing',
  templateUrl: './audit-event-listing.component.html',
  styleUrls: ['./audit-event-listing.component.scss']
})
export class AuditEventListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(AuditEventAddEditComponent) addEditAuditEventComponent!: AuditEventAddEditComponent;
  @ViewChild(AuditEventTableComponent) auditEventTableComponent!: AuditEventTableComponent;

  public AuditEvents: AuditEventData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalAuditEventCount$ : Observable<number> | null = null;
  public filteredAuditEventCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private auditEventService: AuditEventService,
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
    // Subscribe to the auditEventChanged observable on the add/edit component so that when a AuditEvent changes we can reload the list.
    //
    this.addEditAuditEventComponent.auditEventChanged.subscribe({
      next: (result: AuditEventData[] | null) => {
        this.auditEventTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Audit Event changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditAuditEventComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalAuditEventCount$ = this.auditEventService.GetAuditEventsRowCount({
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

      this.filteredAuditEventCount$ = this.auditEventService.GetAuditEventsRowCount({
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

      this.filteredAuditEventCount$ = this.totalAuditEventCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalAuditEventCount$.subscribe();

    if (this.filteredAuditEventCount$ != this.totalAuditEventCount$) {
      this.filteredAuditEventCount$.subscribe();
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
      this.auditEventTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAuditorAuditEventReader(): boolean {
    return this.auditEventService.userIsAuditorAuditEventReader();
  }

  public userIsAuditorAuditEventWriter(): boolean {
    return this.auditEventService.userIsAuditorAuditEventWriter();
  }
}

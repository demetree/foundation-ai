import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AuditUserAgentService, AuditUserAgentData } from '../../../auditor-data-services/audit-user-agent.service';
import { AuditUserAgentAddEditComponent } from '../audit-user-agent-add-edit/audit-user-agent-add-edit.component';
import { AuditUserAgentTableComponent } from '../audit-user-agent-table/audit-user-agent-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-audit-user-agent-listing',
  templateUrl: './audit-user-agent-listing.component.html',
  styleUrls: ['./audit-user-agent-listing.component.scss']
})
export class AuditUserAgentListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(AuditUserAgentAddEditComponent) addEditAuditUserAgentComponent!: AuditUserAgentAddEditComponent;
  @ViewChild(AuditUserAgentTableComponent) auditUserAgentTableComponent!: AuditUserAgentTableComponent;

  public AuditUserAgents: AuditUserAgentData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalAuditUserAgentCount$ : Observable<number> | null = null;
  public filteredAuditUserAgentCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private auditUserAgentService: AuditUserAgentService,
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
    // Subscribe to the auditUserAgentChanged observable on the add/edit component so that when a AuditUserAgent changes we can reload the list.
    //
    this.addEditAuditUserAgentComponent.auditUserAgentChanged.subscribe({
      next: (result: AuditUserAgentData[] | null) => {
        this.auditUserAgentTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Audit User Agent changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditAuditUserAgentComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalAuditUserAgentCount$ = this.auditUserAgentService.GetAuditUserAgentsRowCount({
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

      this.filteredAuditUserAgentCount$ = this.auditUserAgentService.GetAuditUserAgentsRowCount({
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

      this.filteredAuditUserAgentCount$ = this.totalAuditUserAgentCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalAuditUserAgentCount$.subscribe();

    if (this.filteredAuditUserAgentCount$ != this.totalAuditUserAgentCount$) {
      this.filteredAuditUserAgentCount$.subscribe();
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
      this.auditUserAgentTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAuditorAuditUserAgentReader(): boolean {
    return this.auditUserAgentService.userIsAuditorAuditUserAgentReader();
  }

  public userIsAuditorAuditUserAgentWriter(): boolean {
    return this.auditUserAgentService.userIsAuditorAuditUserAgentWriter();
  }
}

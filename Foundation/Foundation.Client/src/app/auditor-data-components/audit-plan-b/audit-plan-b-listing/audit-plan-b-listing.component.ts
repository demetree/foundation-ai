import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AuditPlanBService, AuditPlanBData } from '../../../auditor-data-services/audit-plan-b.service';
import { AuditPlanBAddEditComponent } from '../audit-plan-b-add-edit/audit-plan-b-add-edit.component';
import { AuditPlanBTableComponent } from '../audit-plan-b-table/audit-plan-b-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-audit-plan-b-listing',
  templateUrl: './audit-plan-b-listing.component.html',
  styleUrls: ['./audit-plan-b-listing.component.scss']
})
export class AuditPlanBListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(AuditPlanBAddEditComponent) addEditAuditPlanBComponent!: AuditPlanBAddEditComponent;
  @ViewChild(AuditPlanBTableComponent) auditPlanBTableComponent!: AuditPlanBTableComponent;

  public AuditPlanBs: AuditPlanBData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalAuditPlanBCount$ : Observable<number> | null = null;
  public filteredAuditPlanBCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private auditPlanBService: AuditPlanBService,
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
    // Subscribe to the auditPlanBChanged observable on the add/edit component so that when a AuditPlanB changes we can reload the list.
    //
    this.addEditAuditPlanBComponent.auditPlanBChanged.subscribe({
      next: (result: AuditPlanBData[] | null) => {
        this.auditPlanBTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Audit Plan B changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditAuditPlanBComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalAuditPlanBCount$ = this.auditPlanBService.GetAuditPlanBsRowCount({
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

      this.filteredAuditPlanBCount$ = this.auditPlanBService.GetAuditPlanBsRowCount({
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

      this.filteredAuditPlanBCount$ = this.totalAuditPlanBCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalAuditPlanBCount$.subscribe();

    if (this.filteredAuditPlanBCount$ != this.totalAuditPlanBCount$) {
      this.filteredAuditPlanBCount$.subscribe();
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
      this.auditPlanBTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAuditorAuditPlanBReader(): boolean {
    return this.auditPlanBService.userIsAuditorAuditPlanBReader();
  }

  public userIsAuditorAuditPlanBWriter(): boolean {
    return this.auditPlanBService.userIsAuditorAuditPlanBWriter();
  }
}

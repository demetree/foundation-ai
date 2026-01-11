import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AuditModuleService, AuditModuleData } from '../../../auditor-data-services/audit-module.service';
import { AuditModuleAddEditComponent } from '../audit-module-add-edit/audit-module-add-edit.component';
import { AuditModuleTableComponent } from '../audit-module-table/audit-module-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-audit-module-listing',
  templateUrl: './audit-module-listing.component.html',
  styleUrls: ['./audit-module-listing.component.scss']
})
export class AuditModuleListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(AuditModuleAddEditComponent) addEditAuditModuleComponent!: AuditModuleAddEditComponent;
  @ViewChild(AuditModuleTableComponent) auditModuleTableComponent!: AuditModuleTableComponent;

  public AuditModules: AuditModuleData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalAuditModuleCount$ : Observable<number> | null = null;
  public filteredAuditModuleCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private auditModuleService: AuditModuleService,
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
    // Subscribe to the auditModuleChanged observable on the add/edit component so that when a AuditModule changes we can reload the list.
    //
    this.addEditAuditModuleComponent.auditModuleChanged.subscribe({
      next: (result: AuditModuleData[] | null) => {
        this.auditModuleTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Audit Module changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditAuditModuleComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalAuditModuleCount$ = this.auditModuleService.GetAuditModulesRowCount({
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

      this.filteredAuditModuleCount$ = this.auditModuleService.GetAuditModulesRowCount({
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

      this.filteredAuditModuleCount$ = this.totalAuditModuleCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalAuditModuleCount$.subscribe();

    if (this.filteredAuditModuleCount$ != this.totalAuditModuleCount$) {
      this.filteredAuditModuleCount$.subscribe();
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
      this.auditModuleTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAuditorAuditModuleReader(): boolean {
    return this.auditModuleService.userIsAuditorAuditModuleReader();
  }

  public userIsAuditorAuditModuleWriter(): boolean {
    return this.auditModuleService.userIsAuditorAuditModuleWriter();
  }
}

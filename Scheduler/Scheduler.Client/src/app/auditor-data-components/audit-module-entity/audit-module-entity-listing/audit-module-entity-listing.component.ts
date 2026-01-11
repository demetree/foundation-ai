import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AuditModuleEntityService, AuditModuleEntityData } from '../../../auditor-data-services/audit-module-entity.service';
import { AuditModuleEntityAddEditComponent } from '../audit-module-entity-add-edit/audit-module-entity-add-edit.component';
import { AuditModuleEntityTableComponent } from '../audit-module-entity-table/audit-module-entity-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-audit-module-entity-listing',
  templateUrl: './audit-module-entity-listing.component.html',
  styleUrls: ['./audit-module-entity-listing.component.scss']
})
export class AuditModuleEntityListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(AuditModuleEntityAddEditComponent) addEditAuditModuleEntityComponent!: AuditModuleEntityAddEditComponent;
  @ViewChild(AuditModuleEntityTableComponent) auditModuleEntityTableComponent!: AuditModuleEntityTableComponent;

  public AuditModuleEntities: AuditModuleEntityData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalAuditModuleEntityCount$ : Observable<number> | null = null;
  public filteredAuditModuleEntityCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private auditModuleEntityService: AuditModuleEntityService,
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
    // Subscribe to the auditModuleEntityChanged observable on the add/edit component so that when a AuditModuleEntity changes we can reload the list.
    //
    this.addEditAuditModuleEntityComponent.auditModuleEntityChanged.subscribe({
      next: (result: AuditModuleEntityData[] | null) => {
        this.auditModuleEntityTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Audit Module Entity changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditAuditModuleEntityComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalAuditModuleEntityCount$ = this.auditModuleEntityService.GetAuditModuleEntitiesRowCount({
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

      this.filteredAuditModuleEntityCount$ = this.auditModuleEntityService.GetAuditModuleEntitiesRowCount({
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

      this.filteredAuditModuleEntityCount$ = this.totalAuditModuleEntityCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalAuditModuleEntityCount$.subscribe();

    if (this.filteredAuditModuleEntityCount$ != this.totalAuditModuleEntityCount$) {
      this.filteredAuditModuleEntityCount$.subscribe();
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
      this.auditModuleEntityTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAuditorAuditModuleEntityReader(): boolean {
    return this.auditModuleEntityService.userIsAuditorAuditModuleEntityReader();
  }

  public userIsAuditorAuditModuleEntityWriter(): boolean {
    return this.auditModuleEntityService.userIsAuditorAuditModuleEntityWriter();
  }
}

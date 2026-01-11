import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { EntityDataTokenEventTypeService, EntityDataTokenEventTypeData } from '../../../security-data-services/entity-data-token-event-type.service';
import { EntityDataTokenEventTypeAddEditComponent } from '../entity-data-token-event-type-add-edit/entity-data-token-event-type-add-edit.component';
import { EntityDataTokenEventTypeTableComponent } from '../entity-data-token-event-type-table/entity-data-token-event-type-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-entity-data-token-event-type-listing',
  templateUrl: './entity-data-token-event-type-listing.component.html',
  styleUrls: ['./entity-data-token-event-type-listing.component.scss']
})
export class EntityDataTokenEventTypeListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(EntityDataTokenEventTypeAddEditComponent) addEditEntityDataTokenEventTypeComponent!: EntityDataTokenEventTypeAddEditComponent;
  @ViewChild(EntityDataTokenEventTypeTableComponent) entityDataTokenEventTypeTableComponent!: EntityDataTokenEventTypeTableComponent;

  public EntityDataTokenEventTypes: EntityDataTokenEventTypeData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalEntityDataTokenEventTypeCount$ : Observable<number> | null = null;
  public filteredEntityDataTokenEventTypeCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private entityDataTokenEventTypeService: EntityDataTokenEventTypeService,
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
    // Subscribe to the entityDataTokenEventTypeChanged observable on the add/edit component so that when a EntityDataTokenEventType changes we can reload the list.
    //
    this.addEditEntityDataTokenEventTypeComponent.entityDataTokenEventTypeChanged.subscribe({
      next: (result: EntityDataTokenEventTypeData[] | null) => {
        this.entityDataTokenEventTypeTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Entity Data Token Event Type changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditEntityDataTokenEventTypeComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalEntityDataTokenEventTypeCount$ = this.entityDataTokenEventTypeService.GetEntityDataTokenEventTypesRowCount({
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

      this.filteredEntityDataTokenEventTypeCount$ = this.entityDataTokenEventTypeService.GetEntityDataTokenEventTypesRowCount({
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

      this.filteredEntityDataTokenEventTypeCount$ = this.totalEntityDataTokenEventTypeCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalEntityDataTokenEventTypeCount$.subscribe();

    if (this.filteredEntityDataTokenEventTypeCount$ != this.totalEntityDataTokenEventTypeCount$) {
      this.filteredEntityDataTokenEventTypeCount$.subscribe();
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
      this.entityDataTokenEventTypeTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecurityEntityDataTokenEventTypeReader(): boolean {
    return this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeReader();
  }

  public userIsSecurityEntityDataTokenEventTypeWriter(): boolean {
    return this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeWriter();
  }
}

import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { EntityDataTokenService, EntityDataTokenData } from '../../../security-data-services/entity-data-token.service';
import { EntityDataTokenAddEditComponent } from '../entity-data-token-add-edit/entity-data-token-add-edit.component';
import { EntityDataTokenTableComponent } from '../entity-data-token-table/entity-data-token-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-entity-data-token-listing',
  templateUrl: './entity-data-token-listing.component.html',
  styleUrls: ['./entity-data-token-listing.component.scss']
})
export class EntityDataTokenListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(EntityDataTokenAddEditComponent) addEditEntityDataTokenComponent!: EntityDataTokenAddEditComponent;
  @ViewChild(EntityDataTokenTableComponent) entityDataTokenTableComponent!: EntityDataTokenTableComponent;

  public EntityDataTokens: EntityDataTokenData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalEntityDataTokenCount$ : Observable<number> | null = null;
  public filteredEntityDataTokenCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private entityDataTokenService: EntityDataTokenService,
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
    // Subscribe to the entityDataTokenChanged observable on the add/edit component so that when a EntityDataToken changes we can reload the list.
    //
    this.addEditEntityDataTokenComponent.entityDataTokenChanged.subscribe({
      next: (result: EntityDataTokenData[] | null) => {
        this.entityDataTokenTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Entity Data Token changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditEntityDataTokenComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalEntityDataTokenCount$ = this.entityDataTokenService.GetEntityDataTokensRowCount({
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

      this.filteredEntityDataTokenCount$ = this.entityDataTokenService.GetEntityDataTokensRowCount({
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

      this.filteredEntityDataTokenCount$ = this.totalEntityDataTokenCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalEntityDataTokenCount$.subscribe();

    if (this.filteredEntityDataTokenCount$ != this.totalEntityDataTokenCount$) {
      this.filteredEntityDataTokenCount$.subscribe();
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
      this.entityDataTokenTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecurityEntityDataTokenReader(): boolean {
    return this.entityDataTokenService.userIsSecurityEntityDataTokenReader();
  }

  public userIsSecurityEntityDataTokenWriter(): boolean {
    return this.entityDataTokenService.userIsSecurityEntityDataTokenWriter();
  }
}

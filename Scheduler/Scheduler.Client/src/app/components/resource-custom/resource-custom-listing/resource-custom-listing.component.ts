import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs'
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { ResourceCustomAddEditComponent } from '../resource-custom-add-edit/resource-custom-add-edit.component';
import { ResourceCustomTableComponent } from '..//resource-custom-table/resource-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-resource-listing',
  templateUrl: './resource-custom-listing.component.html',
  styleUrls: ['./resource-custom-listing.component.scss']
})
export class ResourceCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ResourceCustomAddEditComponent) addEditResourceComponent!: ResourceCustomAddEditComponent;
  @ViewChild(ResourceCustomTableComponent) resourceTableComponent!: ResourceCustomTableComponent;

  public Resources: ResourceData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalResourceCount$: Observable<number> | null = null;
  public filteredResourceCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;


  constructor(private resourceService: ResourceService,
    private alertService: AlertService,
    private authService: AuthService,
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
    // Subscribe to the resourceChanged observable on the add/edit component so that when a Resource changes we can reload the list.
    //
    this.addEditResourceComponent.resourceChanged.subscribe({
      next: (result: ResourceData[] | null) => {
        this.resourceTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Resource changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditResourceComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalResourceCount$ = this.resourceService.GetResourcesRowCount({
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

      this.filteredResourceCount$ = this.resourceService.GetResourcesRowCount({
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

      this.filteredResourceCount$ = this.totalResourceCount$; // No filter → same as total
      this.loadingFilteredCount = false;
    }


    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, fast filtering operations can get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalResourceCount$.subscribe();

    if (this.filteredResourceCount$ != this.totalResourceCount$) {
      this.filteredResourceCount$.subscribe();
    }
  }

  public reload() {
    this.resourceTableComponent.loadData();
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
  onFilterChange(): void {

    clearTimeout(this.debounceTimeout);

    this.debounceTimeout = setTimeout(() => {
      this.resourceTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);
  }

  public userIsSchedulerResourceReader(): boolean {
    return this.resourceService.userIsSchedulerResourceReader();
  }

  public userIsSchedulerResourceWriter(): boolean {
    return this.resourceService.userIsSchedulerResourceWriter();
  }
}

/*
   GENERATED FORM FOR THE IPADDRESSLOCATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IpAddressLocation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to ip-address-location-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { IpAddressLocationService, IpAddressLocationData } from '../../../security-data-services/ip-address-location.service';
import { IpAddressLocationAddEditComponent } from '../ip-address-location-add-edit/ip-address-location-add-edit.component';
import { IpAddressLocationTableComponent } from '../ip-address-location-table/ip-address-location-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-ip-address-location-listing',
  templateUrl: './ip-address-location-listing.component.html',
  styleUrls: ['./ip-address-location-listing.component.scss']
})
export class IpAddressLocationListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(IpAddressLocationAddEditComponent) addEditIpAddressLocationComponent!: IpAddressLocationAddEditComponent;
  @ViewChild(IpAddressLocationTableComponent) ipAddressLocationTableComponent!: IpAddressLocationTableComponent;

  public IpAddressLocations: IpAddressLocationData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalIpAddressLocationCount$ : Observable<number> | null = null;
  public filteredIpAddressLocationCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private ipAddressLocationService: IpAddressLocationService,
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
    // Subscribe to the ipAddressLocationChanged observable on the add/edit component so that when a IpAddressLocation changes we can reload the list.
    //
    this.addEditIpAddressLocationComponent.ipAddressLocationChanged.subscribe({
      next: (result: IpAddressLocationData[] | null) => {
        this.ipAddressLocationTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Ip Address Location changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditIpAddressLocationComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalIpAddressLocationCount$ = this.ipAddressLocationService.GetIpAddressLocationsRowCount({
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

      this.filteredIpAddressLocationCount$ = this.ipAddressLocationService.GetIpAddressLocationsRowCount({
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

      this.filteredIpAddressLocationCount$ = this.totalIpAddressLocationCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalIpAddressLocationCount$.subscribe();

    if (this.filteredIpAddressLocationCount$ != this.totalIpAddressLocationCount$) {
      this.filteredIpAddressLocationCount$.subscribe();
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
      this.ipAddressLocationTableComponent.resetToFirstPage(); // Reset to page 1 on filter change
      this.ipAddressLocationTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecurityIpAddressLocationReader(): boolean {
    return this.ipAddressLocationService.userIsSecurityIpAddressLocationReader();
  }

  public userIsSecurityIpAddressLocationWriter(): boolean {
    return this.ipAddressLocationService.userIsSecurityIpAddressLocationWriter();
  }
}

import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs'
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { OfficeService, OfficeData } from '../../../scheduler-data-services/office.service';
import { OfficeCustomAddEditComponent } from '../office-custom-add-edit/office-custom-add-edit.component';
import { OfficeCustomTableComponent } from '../office-custom-table/office-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { TableColumn } from '../../../utility/foundation.utility';


@Component({
  selector: 'app-office-custom-listing',
  templateUrl: './office-custom-listing.component.html',
  styleUrls: ['./office-custom-listing.component.scss']
})
export class OfficeCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(OfficeCustomAddEditComponent) addEditOfficeComponent!: OfficeCustomAddEditComponent;
  @ViewChild(OfficeCustomTableComponent) officeTableComponent!: OfficeCustomTableComponent;

  public Offices: OfficeData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalOfficeCount$: Observable<number> | null = null;
  public filteredOfficeCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private officeService: OfficeService,
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
    // Subscribe to the officeChanged observable on the add/edit component so that when a Office changes we can reload the list.
    //
    this.addEditOfficeComponent.officeChanged.subscribe({
      next: (result: OfficeData[] | null) => {
        this.officeTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Office changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditOfficeComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  public buildCustomColumns(): TableColumn[] {

    //
    // Add the fields to the column config.  Refinements to widths and such can be made easily by custom users by tweaking the input as need be.
    //
    // Start with the common columns that everyone sees
    //
    const customColumns: TableColumn[] = [

      { key: 'name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/office', 'id'] },
      { key: 'description', label: 'Description', width: undefined },
      { key: 'officeType.name', label: 'Office Type', width: undefined, template: 'link', linkPath: ['/officetype', 'officeTypeId'] },
      { key: 'phone', label: 'Phone', width: undefined },
//      { key: 'addressLine1', label: 'Address Line 1', width: undefined },
//      { key: 'addressLine2', label: 'Address Line 2', width: undefined },
      { key: 'city', label: 'City', width: undefined },
//      { key: 'postalCode', label: 'Postal Code', width: undefined },
//      { key: 'stateProvince.name', label: 'State Province', width: undefined, template: 'link', linkPath: ['/stateprovince', 'stateProvinceId'] },
//      { key: 'country.name', label: 'Country', width: undefined, template: 'link', linkPath: ['/country', 'countryId'] },
//      { key: 'timeZone.name', label: 'Time Zone', width: undefined, template: 'link', linkPath: ['/timezone', 'timeZoneId'] },
//      { key: 'currency.name', label: 'Currency', width: undefined, template: 'link', linkPath: ['/currency', 'currencyId'] },
      { key: 'color', label: 'Color', width: "50px", template: 'color' },
      
      //{ key: 'externalId', label: 'External Id', width: undefined },
      //{ key: 'attributes', label: 'Attributes', width: undefined },
      //{ key: 'avatarFileName', label: 'Avatar File Name', width: undefined },
      //{ key: 'avatarSize', label: 'Avatar Size', width: undefined },
      //{ key: 'avatarData', label: 'Avatar Data', width: undefined },
      //{ key: 'avatarMimeType', label: 'Avatar Mime Type', width: undefined },

    ];

    const isWriter = this.officeService.userIsSchedulerOfficeWriter();
    const isAdmin = this.authService.isSchedulerAdministrator;

    if (isAdmin) {
      customColumns.push({ key: 'versionNumber', label: 'Version Number', width: undefined });
      customColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
      customColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });

    }
    else if (isWriter) {
      customColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
      customColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });
    }


    // Assign the built array as the active columns
    return customColumns;
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalOfficeCount$ = this.officeService.GetOfficesRowCount({
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

      this.filteredOfficeCount$ = this.officeService.GetOfficesRowCount({
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

      this.filteredOfficeCount$ = this.totalOfficeCount$; // No filter → same as total
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, fast filtering operations can get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalOfficeCount$.subscribe();

    if (this.filteredOfficeCount$ != this.totalOfficeCount$) {
      this.filteredOfficeCount$.subscribe();
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


  onFilterChange(): void {

    this.debounceTimeout = setTimeout(() => {
      this.officeTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 100);

  }


  public userIsSchedulerOfficeReader(): boolean {
    return this.officeService.userIsSchedulerOfficeReader();
  }

  public userIsSchedulerOfficeWriter(): boolean {
    return this.officeService.userIsSchedulerOfficeWriter();
  }
}

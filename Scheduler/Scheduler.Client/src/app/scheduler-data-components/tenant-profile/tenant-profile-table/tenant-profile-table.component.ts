/*
   GENERATED FORM FOR THE TENANTPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TenantProfile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tenant-profile-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TenantProfileService, TenantProfileData, TenantProfileQueryParameters } from '../../../scheduler-data-services/tenant-profile.service';
import { TenantProfileAddEditComponent } from '../tenant-profile-add-edit/tenant-profile-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-tenant-profile-table',
  templateUrl: './tenant-profile-table.component.html',
  styleUrls: ['./tenant-profile-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TenantProfileTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(TenantProfileAddEditComponent) addEditTenantProfileComponent!: TenantProfileAddEditComponent;

  @Input() TenantProfiles: TenantProfileData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<TenantProfileQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<TenantProfileData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<TenantProfileData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<TenantProfileData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredTenantProfiles: TenantProfileData[] | null = null;        // Stores the filtered/sorted data

  // Sorting properties
  public sortColumn: string | null = null;
  public sortDirection: 'asc' | 'desc' = 'asc';


  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  private isManagingData: boolean = false; // Tracks if component is managing data loading

  private debounceTimeout: any;


  //
  // Error state tracking to help suppress endless loop scenarios in case of server error or request path errors
  //
  private inErrorState: boolean = false;
  private errorResetTimeout: any;


  constructor(private tenantProfileService: TenantProfileService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.TenantProfiles) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the tenantProfileChanged observable on the add/edit component so that when a TenantProfile changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditTenantProfileComponent && !this.disableDefaultEdit) {
        this.addEditTenantProfileComponent.tenantProfileChanged.subscribe({
        next: (result: TenantProfileData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Tenant Profile changed notification", JSON.stringify(err), MessageSeverity.error);
        }
        });
     }
  }

  ngOnChanges(changes: SimpleChanges): void {


    if (this.inErrorState == true) {
      //
      // Circuit breaker for endless loop prevention.
      //
      return;
    }

    //
    // Reset the whole page - note that this only makes sense when this component is managing the loading of data.  Don't use the filterText input property when you are providing your own data via the data input.
    //
    if (changes['filterText'] && this.isManagingData == true)
    {
       clearTimeout(this.debounceTimeout);
       this.debounceTimeout = setTimeout(() => {

         if (this.isManagingData)
         {
             this.loadData();
         }
         else
         {
             this.applyFiltersAndSort();
         }

       }, 200); // 200ms debounce delay
    }

    if (changes['queryParams'])
    {
        this.loadData()
    }
  }


 /**
   * Construct the default column array based on user entitlements.
   */
  private buildDefaultColumns(): void {

    //
    // Add the fields to the column config.  Refinements to widths and such can be made easily by custom users by tweaking the input as need be.
    //
    // This is the default set to start with something for customization
    //
    // Note width purposely set to undefined so the reader knows the property is there for use.  It is to be overridden as appropriate by users 
    // providing custom column specs.
    //
    // Start with the common columns that everyone sees
    //
    const defaultColumns: TableColumn[] = [
    { key: 'name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/tenantprofile', 'id']  },
    { key: 'description', label: 'Description', width: undefined },
    // { key: 'companyLogoFileName', label: 'Company Logo File Name', width: undefined },
    // { key: 'companyLogoSize', label: 'Company Logo Size', width: undefined },
    // { key: 'companyLogoData', label: 'Company Logo Data', width: undefined },
    // { key: 'companyLogoMimeType', label: 'Company Logo Mime Type', width: undefined },
    { key: 'addressLine1', label: 'Address Line 1', width: undefined },
    { key: 'addressLine2', label: 'Address Line 2', width: undefined },
    { key: 'addressLine3', label: 'Address Line 3', width: undefined },
    { key: 'city', label: 'City', width: undefined },
    { key: 'postalCode', label: 'Postal Code', width: undefined },
    { key: 'stateProvince.name', label: 'State Province', width: undefined, template: 'link', linkPath: ['/stateprovince', 'stateProvinceId'] },
    { key: 'country.name', label: 'Country', width: undefined, template: 'link', linkPath: ['/country', 'countryId'] },
    { key: 'timeZone.name', label: 'Time Zone', width: undefined, template: 'link', linkPath: ['/timezone', 'timeZoneId'] },
    { key: 'phoneNumber', label: 'Phone Number', width: undefined },
    { key: 'email', label: 'Email', width: undefined },
    { key: 'website', label: 'Website', width: undefined },
    { key: 'latitude', label: 'Latitude', width: undefined },
    { key: 'longitude', label: 'Longitude', width: undefined },
    { key: 'primaryColor', label: 'Primary Color', width: "50px", template: 'color' },
    { key: 'secondaryColor', label: 'Secondary Color', width: "50px", template: 'color' },
    { key: 'displaysMetric', label: 'Displays Metric', width: '120px', template: 'boolean' },
    { key: 'displaysUSTerms', label: 'Displays U S Terms', width: '120px', template: 'boolean' },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.tenantProfileService.userIsSchedulerTenantProfileWriter();
    const isAdmin = this.authService.isSchedulerAdministrator; 

    if (isAdmin) {
     defaultColumns.push({ key: 'versionNumber', label: 'Version Number', width: undefined });
     defaultColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
     defaultColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });

    }
    else if (isWriter) {
     defaultColumns.push({ key: 'versionNumber', label: 'Version Number', width: undefined });
    }

    
    // Assign the built array as the active columns
    this.columns = defaultColumns;
  }


  public sortBy(column: string) : void {

    if (this.sortColumn === column) {
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
        this.sortColumn = column;
        this.sortDirection = 'asc';
    }

    this.applyFiltersAndSort();
  }


  public loadData(): void {

    if (!this.isManagingData) {
      return; // Skip if parent is providing data
    }

    if (this.tenantProfileService.userIsSchedulerTenantProfileReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Tenant Profiles", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const tenantProfileQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.tenantProfileService.GetTenantProfileList(tenantProfileQueryParams).subscribe({
      next: (TenantProfileList) => {
        if (TenantProfileList) {
          this.TenantProfiles = TenantProfileList;
        } else {
          this.TenantProfiles = [];
        }

        //
        // Apply the sort.  Filtering of data done already done in the data we receive from the service.
        //
        this.applyFiltersAndSort();

        //
        // Clear the loading spinner
        //
        this.isLoadingSubject.next(false);

        //
        // Reset the error state
        //
        this.inErrorState = false;

      },
      error: (err) => {

        //
        // Clear the loading spinner
        //
        this.isLoadingSubject.next(false);

        //
        // Turn the error state on
        //
        this.setErrorState();

         this.alertService.showMessage("Error getting Tenant Profile data", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }




  private setErrorState(): void {

    // Turn error state flag on
    this.inErrorState = true;

    // wipe any existing timeout
    clearTimeout(this.errorResetTimeout);

    // Reset error state after 15 seconds
    this.errorResetTimeout = setTimeout(() => {
      this.inErrorState = false;
    }, 15000); // Allow retry after 15s
  }


   private applyFiltersAndSort(): void {

    if (!this.TenantProfiles) {
      this.filteredTenantProfiles = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.TenantProfiles];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'name',
                      'description',
                      'companyLogoFileName',
                      'companyLogoSize',
                      'companyLogoData',
                      'companyLogoMimeType',
                      'addressLine1',
                      'addressLine2',
                      'addressLine3',
                      'city',
                      'postalCode',
                      'stateProvince.name',
                      'country.name',
                      'timeZone.name',
                      'phoneNumber',
                      'email',
                      'website',
                      'latitude',
                      'longitude',
                      'primaryColor',
                      'secondaryColor',
                      'displaysMetric',
                      'displaysUSTerms',
        ];

        result = result.filter((tenantProfile) =>

        filterFields.some((field) => {
        const value = getNestedValue(tenantProfile, field);
            return value && value.toString().toLowerCase().includes(searchText);
          })
          );
      }
    }

    // Apply sorting
    if (this.sortColumn) {
      result.sort((a, b) => {

        const aValue = getNestedValue(a, this.sortColumn!);
        const bValue = getNestedValue(b, this.sortColumn!);

        if (typeof aValue === 'number' && typeof bValue === 'number') {
          return this.sortDirection === 'asc' ? aValue - bValue : bValue - aValue;
        }

        const aStr = aValue ? aValue.toString() : '';
        const bStr = bValue ? bValue.toString() : '';
        const comparison = aStr.localeCompare(bStr, undefined, {sensitivity: 'base' });
        return this.sortDirection === 'asc' ? comparison : -comparison;
      });
    }

    this.filteredTenantProfiles = result;
  }


  public handleEdit(tenantProfile: TenantProfileData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(tenantProfile); // Let parent handle edit
    }
    else if (this.addEditTenantProfileComponent)
    {
        this.addEditTenantProfileComponent.openModal(tenantProfile); // Default edit behavior
    }
    else
    {
        this.alertService.showMessage(
          'Edit functionality unavailable',
          'Add/Edit component not initialized',
          MessageSeverity.warn
        );
    }
}


  public handleDelete(tenantProfile: TenantProfileData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(tenantProfile); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete TenantProfile', 'Are you sure you want to delete this Tenant Profile?')
          .then((result) => {
              if (result)
              {
                  this.deleteTenantProfile(tenantProfile);
              }
          })
          .catch(() => { });
    }
  }


  private deleteTenantProfile(tenantProfileData: TenantProfileData): void {
    this.tenantProfileService.DeleteTenantProfile(tenantProfileData.id).subscribe({
      next: () => {
       this.tenantProfileService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Tenant Profile", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(tenantProfile: TenantProfileData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(tenantProfile); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete TenantProfile', 'Are you sure you want to undelete this Tenant Profile?')
          .then((result) => {
              if (result)
              {
                  this.undeleteTenantProfile(tenantProfile);
              }
          })
          .catch(() => { });
    }
}


  private undeleteTenantProfile(tenantProfileData: TenantProfileData): void {

      var tenantProfileToSubmit = this.tenantProfileService.ConvertToTenantProfileSubmitData(tenantProfileData); // Convert TenantProfile data to post object for undeleting
      tenantProfileToSubmit.deleted = false;

      this.tenantProfileService.PutTenantProfile(tenantProfileToSubmit.id, tenantProfileToSubmit).subscribe({
      next: () => {
       this.tenantProfileService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Tenant Profile", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getTenantProfileId(index: number, tenantProfile: any): number {
    return tenantProfile.id;
  }


  public userIsSchedulerTenantProfileReader(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileReader();
  }

  public userIsSchedulerTenantProfileWriter(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/tenantProfile', tenantProfileId]
  public buildLink(item: any, path: string[]): any[] {
    //
    // Expect a starting item in the path array with a slash to indicate the route.  After that, the other items in path are expected to be properties of the item.  Tyically one, but more are technically supported.
    //
    // The example usage is ['/route', 'routeId'], where route is the name of the route, and 'routeId' is the property name on the item object that indexes the route.
    //
    return path.map(segment => segment.startsWith('/') ? segment : item[segment]);
  }


  // Returns only columns that should appear on mobile
  get mobileColumns(): TableColumn[] {
    return this.columns.filter(col => col.mobile !== 'hidden');
  }


  // First "prominent" column for mobile view
  get prominentColumn(): TableColumn | null {
    return this.columns.find(col => col.mobile === 'prominent') || null;
  }

}

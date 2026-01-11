/*
   GENERATED FORM FOR THE SCHEDULINGTARGETADDRESS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetAddress table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-address-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { SchedulingTargetAddressService, SchedulingTargetAddressData, SchedulingTargetAddressQueryParameters } from '../../../scheduler-data-services/scheduling-target-address.service';
import { SchedulingTargetAddressAddEditComponent } from '../scheduling-target-address-add-edit/scheduling-target-address-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-scheduling-target-address-table',
  templateUrl: './scheduling-target-address-table.component.html',
  styleUrls: ['./scheduling-target-address-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SchedulingTargetAddressTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(SchedulingTargetAddressAddEditComponent) addEditSchedulingTargetAddressComponent!: SchedulingTargetAddressAddEditComponent;

  @Input() SchedulingTargetAddresses: SchedulingTargetAddressData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<SchedulingTargetAddressQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<SchedulingTargetAddressData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<SchedulingTargetAddressData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<SchedulingTargetAddressData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredSchedulingTargetAddresses: SchedulingTargetAddressData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private schedulingTargetAddressService: SchedulingTargetAddressService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.SchedulingTargetAddresses) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the schedulingTargetAddressChanged observable on the add/edit component so that when a SchedulingTargetAddress changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditSchedulingTargetAddressComponent && !this.disableDefaultEdit) {
        this.addEditSchedulingTargetAddressComponent.schedulingTargetAddressChanged.subscribe({
        next: (result: SchedulingTargetAddressData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Scheduling Target Address changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'schedulingTarget.name', label: 'Scheduling Target', width: undefined, template: 'link', linkPath: ['/schedulingtarget', 'schedulingTargetId'] },
    { key: 'client.name', label: 'Client', width: undefined, template: 'link', linkPath: ['/client', 'clientId'] },
    { key: 'addressLine1', label: 'Address Line 1', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/schedulingtargetaddress', 'id']  },
    { key: 'addressLine2', label: 'Address Line 2', width: undefined },
    { key: 'city', label: 'City', width: undefined },
    { key: 'postalCode', label: 'Postal Code', width: undefined },
    { key: 'stateProvince.name', label: 'State Province', width: undefined, template: 'link', linkPath: ['/stateprovince', 'stateProvinceId'] },
    { key: 'country.name', label: 'Country', width: undefined, template: 'link', linkPath: ['/country', 'countryId'] },
    { key: 'latitude', label: 'Latitude', width: undefined },
    { key: 'longitude', label: 'Longitude', width: undefined },
    { key: 'label', label: 'Label', width: undefined },
    { key: 'isPrimary', label: 'Is Primary', width: '120px', template: 'boolean' },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressWriter();
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

    if (this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Scheduling Target Addresses", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const schedulingTargetAddressQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.schedulingTargetAddressService.GetSchedulingTargetAddressList(schedulingTargetAddressQueryParams).subscribe({
      next: (SchedulingTargetAddressList) => {
        if (SchedulingTargetAddressList) {
          this.SchedulingTargetAddresses = SchedulingTargetAddressList;
        } else {
          this.SchedulingTargetAddresses = [];
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

         this.alertService.showMessage("Error getting Scheduling Target Address data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.SchedulingTargetAddresses) {
      this.filteredSchedulingTargetAddresses = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.SchedulingTargetAddresses];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'schedulingTarget.name',
                      'client.name',
                      'addressLine1',
                      'addressLine2',
                      'city',
                      'postalCode',
                      'stateProvince.name',
                      'country.name',
                      'latitude',
                      'longitude',
                      'label',
                      'isPrimary',
        ];

        result = result.filter((schedulingTargetAddress) =>

        filterFields.some((field) => {
        const value = getNestedValue(schedulingTargetAddress, field);
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

    this.filteredSchedulingTargetAddresses = result;
  }


  public handleEdit(schedulingTargetAddress: SchedulingTargetAddressData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(schedulingTargetAddress); // Let parent handle edit
    }
    else if (this.addEditSchedulingTargetAddressComponent)
    {
        this.addEditSchedulingTargetAddressComponent.openModal(schedulingTargetAddress); // Default edit behavior
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


  public handleDelete(schedulingTargetAddress: SchedulingTargetAddressData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(schedulingTargetAddress); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete SchedulingTargetAddress', 'Are you sure you want to delete this Scheduling Target Address?')
          .then((result) => {
              if (result)
              {
                  this.deleteSchedulingTargetAddress(schedulingTargetAddress);
              }
          })
          .catch(() => { });
    }
  }


  private deleteSchedulingTargetAddress(schedulingTargetAddressData: SchedulingTargetAddressData): void {
    this.schedulingTargetAddressService.DeleteSchedulingTargetAddress(schedulingTargetAddressData.id).subscribe({
      next: () => {
       this.schedulingTargetAddressService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Scheduling Target Address", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(schedulingTargetAddress: SchedulingTargetAddressData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(schedulingTargetAddress); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete SchedulingTargetAddress', 'Are you sure you want to undelete this Scheduling Target Address?')
          .then((result) => {
              if (result)
              {
                  this.undeleteSchedulingTargetAddress(schedulingTargetAddress);
              }
          })
          .catch(() => { });
    }
}


  private undeleteSchedulingTargetAddress(schedulingTargetAddressData: SchedulingTargetAddressData): void {

      var schedulingTargetAddressToSubmit = this.schedulingTargetAddressService.ConvertToSchedulingTargetAddressSubmitData(schedulingTargetAddressData); // Convert SchedulingTargetAddress data to post object for undeleting
      schedulingTargetAddressToSubmit.deleted = false;

      this.schedulingTargetAddressService.PutSchedulingTargetAddress(schedulingTargetAddressToSubmit.id, schedulingTargetAddressToSubmit).subscribe({
      next: () => {
       this.schedulingTargetAddressService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Scheduling Target Address", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getSchedulingTargetAddressId(index: number, schedulingTargetAddress: any): number {
    return schedulingTargetAddress.id;
  }


  public userIsSchedulerSchedulingTargetAddressReader(): boolean {
    return this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressReader();
  }

  public userIsSchedulerSchedulingTargetAddressWriter(): boolean {
    return this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/schedulingTargetAddress', schedulingTargetAddressId]
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

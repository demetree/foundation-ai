/*
   GENERATED FORM FOR THE TELEMETRYERROREVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryErrorEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-error-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TelemetryErrorEventService, TelemetryErrorEventData, TelemetryErrorEventQueryParameters } from '../../../telemetry-data-services/telemetry-error-event.service';
import { TelemetryErrorEventAddEditComponent } from '../telemetry-error-event-add-edit/telemetry-error-event-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-telemetry-error-event-table',
  templateUrl: './telemetry-error-event-table.component.html',
  styleUrls: ['./telemetry-error-event-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TelemetryErrorEventTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(TelemetryErrorEventAddEditComponent) addEditTelemetryErrorEventComponent!: TelemetryErrorEventAddEditComponent;

  @Input() TelemetryErrorEvents: TelemetryErrorEventData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<TelemetryErrorEventQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior

  @Output() edit = new EventEmitter<TelemetryErrorEventData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<TelemetryErrorEventData>(); // Emitted for custom delete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredTelemetryErrorEvents: TelemetryErrorEventData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private telemetryErrorEventService: TelemetryErrorEventService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.TelemetryErrorEvents) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the telemetryErrorEventChanged observable on the add/edit component so that when a TelemetryErrorEvent changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditTelemetryErrorEventComponent && !this.disableDefaultEdit) {
        this.addEditTelemetryErrorEventComponent.telemetryErrorEventChanged.subscribe({
        next: (result: TelemetryErrorEventData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Telemetry Error Event changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'telemetryApplication.name', label: 'Telemetry Application', width: undefined, template: 'link', linkPath: ['/telemetryapplication', 'telemetryApplicationId'] },
    { key: 'telemetrySnapshot.name', label: 'Telemetry Snapshot', width: undefined, template: 'link', linkPath: ['/telemetrysnapshot', 'telemetrySnapshotId'] },
    { key: 'originalAuditEventId', label: 'Original Audit Event Id', width: undefined },
    { key: 'occurredAt', label: 'Occurred At', width: undefined, template: 'date' },
    { key: 'auditTypeName', label: 'Audit Type Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/telemetryerrorevent', 'id']  },
    { key: 'moduleName', label: 'Module Name', width: undefined },
    { key: 'entityName', label: 'Entity Name', width: undefined },
    { key: 'userName', label: 'User Name', width: undefined },
    { key: 'message', label: 'Message', width: undefined },
    { key: 'errorMessage', label: 'Error Message', width: undefined },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventWriter();
    const isAdmin = this.authService.isTelemetryAdministrator; 

    if (isAdmin) {

    }
    else if (isWriter) {
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

    if (this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Telemetry Error Events", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const telemetryErrorEventQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.telemetryErrorEventService.GetTelemetryErrorEventList(telemetryErrorEventQueryParams).subscribe({
      next: (TelemetryErrorEventList) => {
        if (TelemetryErrorEventList) {
          this.TelemetryErrorEvents = TelemetryErrorEventList;
        } else {
          this.TelemetryErrorEvents = [];
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

         this.alertService.showMessage("Error getting Telemetry Error Event data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.TelemetryErrorEvents) {
      this.filteredTelemetryErrorEvents = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.TelemetryErrorEvents];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'telemetryApplication.name',
                      'telemetrySnapshot.name',
                      'originalAuditEventId',
                      'occurredAt',
                      'auditTypeName',
                      'moduleName',
                      'entityName',
                      'userName',
                      'message',
                      'errorMessage',
        ];

        result = result.filter((telemetryErrorEvent) =>

        filterFields.some((field) => {
        const value = getNestedValue(telemetryErrorEvent, field);
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

    this.filteredTelemetryErrorEvents = result;
  }


  public handleEdit(telemetryErrorEvent: TelemetryErrorEventData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(telemetryErrorEvent); // Let parent handle edit
    }
    else if (this.addEditTelemetryErrorEventComponent)
    {
        this.addEditTelemetryErrorEventComponent.openModal(telemetryErrorEvent); // Default edit behavior
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


  public handleDelete(telemetryErrorEvent: TelemetryErrorEventData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(telemetryErrorEvent); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete TelemetryErrorEvent', 'Are you sure you want to delete this Telemetry Error Event?')
          .then((result) => {
              if (result)
              {
                  this.deleteTelemetryErrorEvent(telemetryErrorEvent);
              }
          })
          .catch(() => { });
    }
  }


  private deleteTelemetryErrorEvent(telemetryErrorEventData: TelemetryErrorEventData): void {
    this.telemetryErrorEventService.DeleteTelemetryErrorEvent(telemetryErrorEventData.id).subscribe({
      next: () => {
       this.telemetryErrorEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Telemetry Error Event", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getTelemetryErrorEventId(index: number, telemetryErrorEvent: any): number {
    return telemetryErrorEvent.id;
  }


  public userIsTelemetryTelemetryErrorEventReader(): boolean {
    return this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventReader();
  }

  public userIsTelemetryTelemetryErrorEventWriter(): boolean {
    return this.telemetryErrorEventService.userIsTelemetryTelemetryErrorEventWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/telemetryErrorEvent', telemetryErrorEventId]
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

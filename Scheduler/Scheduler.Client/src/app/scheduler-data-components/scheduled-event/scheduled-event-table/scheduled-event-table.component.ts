/*
   GENERATED FORM FOR THE SCHEDULEDEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ScheduledEventService, ScheduledEventData, ScheduledEventQueryParameters } from '../../../scheduler-data-services/scheduled-event.service';
import { ScheduledEventAddEditComponent } from '../scheduled-event-add-edit/scheduled-event-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-scheduled-event-table',
  templateUrl: './scheduled-event-table.component.html',
  styleUrls: ['./scheduled-event-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ScheduledEventTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(ScheduledEventAddEditComponent) addEditScheduledEventComponent!: ScheduledEventAddEditComponent;

  @Input() ScheduledEvents: ScheduledEventData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<ScheduledEventQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<ScheduledEventData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<ScheduledEventData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<ScheduledEventData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredScheduledEvents: ScheduledEventData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private scheduledEventService: ScheduledEventService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.ScheduledEvents) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the scheduledEventChanged observable on the add/edit component so that when a ScheduledEvent changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditScheduledEventComponent && !this.disableDefaultEdit) {
        this.addEditScheduledEventComponent.scheduledEventChanged.subscribe({
        next: (result: ScheduledEventData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Scheduled Event changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'office.name', label: 'Office', width: undefined, template: 'link', linkPath: ['/office', 'officeId'] },
    { key: 'client.name', label: 'Client', width: undefined, template: 'link', linkPath: ['/client', 'clientId'] },
    { key: 'scheduledEventTemplate.name', label: 'Scheduled Event Template', width: undefined, template: 'link', linkPath: ['/scheduledeventtemplate', 'scheduledEventTemplateId'] },
    { key: 'recurrenceRule.name', label: 'Recurrence Rule', width: undefined, template: 'link', linkPath: ['/recurrencerule', 'recurrenceRuleId'] },
    { key: 'schedulingTarget.name', label: 'Scheduling Target', width: undefined, template: 'link', linkPath: ['/schedulingtarget', 'schedulingTargetId'] },
    { key: 'timeZone.name', label: 'Time Zone', width: undefined, template: 'link', linkPath: ['/timezone', 'timeZoneId'] },
    { key: 'parentScheduledEvent.name', label: 'Scheduled Event', width: undefined, template: 'link', linkPath: ['/scheduledevent', 'parentScheduledEventId'] },
    { key: 'recurrenceInstanceDate', label: 'Recurrence Instance Date', width: undefined, template: 'date' },
    { key: 'name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/scheduledevent', 'id']  },
    { key: 'description', label: 'Description', width: undefined },
    { key: 'isAllDay', label: 'Is All Day', width: '120px', template: 'boolean' },
    { key: 'startDateTime', label: 'Start Date Time', width: undefined, template: 'date' },
    { key: 'endDateTime', label: 'End Date Time', width: undefined, template: 'date' },
    { key: 'location', label: 'Location', width: undefined },
    { key: 'eventStatus.name', label: 'Event Status', width: undefined, template: 'link', linkPath: ['/eventstatus', 'eventStatusId'] },
    { key: 'resource.name', label: 'Resource', width: undefined, template: 'link', linkPath: ['/resource', 'resourceId'] },
    { key: 'crew.name', label: 'Crew', width: undefined, template: 'link', linkPath: ['/crew', 'crewId'] },
    { key: 'priority.name', label: 'Priority', width: undefined, template: 'link', linkPath: ['/priority', 'priorityId'] },
    { key: 'bookingSourceType.name', label: 'Booking Source Type', width: undefined, template: 'link', linkPath: ['/bookingsourcetype', 'bookingSourceTypeId'] },
    { key: 'partySize', label: 'Party Size', width: undefined },
    { key: 'notes', label: 'Notes', width: undefined },
    { key: 'color', label: 'Color', width: "50px", template: 'color' },
    // { key: 'externalId', label: 'External Id', width: undefined },
    // { key: 'attributes', label: 'Attributes', width: undefined },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.scheduledEventService.userIsSchedulerScheduledEventWriter();
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

    if (this.scheduledEventService.userIsSchedulerScheduledEventReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Scheduled Events", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const scheduledEventQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.scheduledEventService.GetScheduledEventList(scheduledEventQueryParams).subscribe({
      next: (ScheduledEventList) => {
        if (ScheduledEventList) {
          this.ScheduledEvents = ScheduledEventList;
        } else {
          this.ScheduledEvents = [];
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

         this.alertService.showMessage("Error getting Scheduled Event data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.ScheduledEvents) {
      this.filteredScheduledEvents = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.ScheduledEvents];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'office.name',
                      'client.name',
                      'scheduledEventTemplate.name',
                      'recurrenceRule.name',
                      'schedulingTarget.name',
                      'timeZone.name',
                      'parentScheduledEvent.name',
                      'recurrenceInstanceDate',
                      'name',
                      'description',
                      'isAllDay',
                      'startDateTime',
                      'endDateTime',
                      'location',
                      'eventStatus.name',
                      'resource.name',
                      'crew.name',
                      'priority.name',
                      'bookingSourceType.name',
                      'partySize',
                      'notes',
                      'color',
                      'externalId',
                      'attributes',
        ];

        result = result.filter((scheduledEvent) =>

        filterFields.some((field) => {
        const value = getNestedValue(scheduledEvent, field);
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

    this.filteredScheduledEvents = result;
  }


  public handleEdit(scheduledEvent: ScheduledEventData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(scheduledEvent); // Let parent handle edit
    }
    else if (this.addEditScheduledEventComponent)
    {
        this.addEditScheduledEventComponent.openModal(scheduledEvent); // Default edit behavior
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


  public handleDelete(scheduledEvent: ScheduledEventData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(scheduledEvent); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete ScheduledEvent', 'Are you sure you want to delete this Scheduled Event?')
          .then((result) => {
              if (result)
              {
                  this.deleteScheduledEvent(scheduledEvent);
              }
          })
          .catch(() => { });
    }
  }


  private deleteScheduledEvent(scheduledEventData: ScheduledEventData): void {
    this.scheduledEventService.DeleteScheduledEvent(scheduledEventData.id).subscribe({
      next: () => {
       this.scheduledEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Scheduled Event", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(scheduledEvent: ScheduledEventData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(scheduledEvent); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete ScheduledEvent', 'Are you sure you want to undelete this Scheduled Event?')
          .then((result) => {
              if (result)
              {
                  this.undeleteScheduledEvent(scheduledEvent);
              }
          })
          .catch(() => { });
    }
}


  private undeleteScheduledEvent(scheduledEventData: ScheduledEventData): void {

      var scheduledEventToSubmit = this.scheduledEventService.ConvertToScheduledEventSubmitData(scheduledEventData); // Convert ScheduledEvent data to post object for undeleting
      scheduledEventToSubmit.deleted = false;

      this.scheduledEventService.PutScheduledEvent(scheduledEventToSubmit.id, scheduledEventToSubmit).subscribe({
      next: () => {
       this.scheduledEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Scheduled Event", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getScheduledEventId(index: number, scheduledEvent: any): number {
    return scheduledEvent.id;
  }


  public userIsSchedulerScheduledEventReader(): boolean {
    return this.scheduledEventService.userIsSchedulerScheduledEventReader();
  }

  public userIsSchedulerScheduledEventWriter(): boolean {
    return this.scheduledEventService.userIsSchedulerScheduledEventWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/scheduledEvent', scheduledEventId]
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

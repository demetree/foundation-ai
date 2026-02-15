import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ClientService, ClientData, ClientQueryParameters } from '../../../scheduler-data-services/client.service';
import { ClientCustomAddEditComponent } from '../client-custom-add-edit/client-custom-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-client-custom-table',
  templateUrl: './client-custom-table.component.html',
  styleUrls: ['./client-custom-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientCustomTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(ClientCustomAddEditComponent) addEditClientComponent!: ClientCustomAddEditComponent;

  @Input() Clients: ClientData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<ClientQueryParameters> = {} // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<ClientData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<ClientData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<ClientData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredClients: ClientData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private clientService: ClientService,
    private authService: AuthService,
    private alertService: AlertService,
    private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.Clients) {

      this.isManagingData = true; // Component is managing data loading
      this.loadData(); // Load data on initialization

    } else {

      this.applyFiltersAndSort();
      this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the clientChanged observable on the add/edit component so that when a Client changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditClientComponent && !this.disableDefaultEdit) {
      this.addEditClientComponent.clientChanged.subscribe({
        next: (result: ClientData[] | null) => {
          this.loadData();
        },
        error: (err: any) => {
          this.alertService.showMessage("Error during Client changed notification", JSON.stringify(err), MessageSeverity.error);
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
    if (changes['filterText'] && this.isManagingData == true) {
      clearTimeout(this.debounceTimeout);
      this.debounceTimeout = setTimeout(() => {

        if (this.isManagingData) {
          this.loadData();
        }
        else {
          this.applyFiltersAndSort();
        }

      }, 200); // 200ms debounce delay
    }

    if (changes['queryParams']) {
      this.loadData()
    }
  }


  /**
   * Construct the default column array based on user entitlements.
   */
  private buildDefaultColumns(): void {

    //
    // Core columns visible to all readers — a scannable summary of the client
    //
    const defaultColumns: TableColumn[] = [
      { key: 'name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/client', 'id'] },
      { key: 'clientType.name', label: 'Type', width: undefined, template: 'link', linkPath: ['/clienttype', 'clientTypeId'] },
      { key: 'addressLine1', label: 'Address', width: undefined },
      { key: 'city', label: 'City', width: undefined },
      { key: 'stateProvince.name', label: 'State', width: undefined, template: 'link', linkPath: ['/stateprovince', 'stateProvinceId'] },
      { key: 'country.name', label: 'Country', width: undefined, template: 'link', linkPath: ['/country', 'countryId'] },
      { key: 'phone', label: 'Phone', width: undefined },
      { key: 'email', label: 'Email', width: undefined },
      { key: 'color', label: 'Color', width: "50px", template: 'color' },
    ];


    //
    // Note that CSS styling shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.clientService.userIsSchedulerClientWriter();
    const isAdmin = this.authService.isSchedulerAdministrator;

    //
    // Writers get additional detail columns
    //
    if (isWriter) {
      // Insert secondary detail columns before 'color' (which is always last of the common set)
      const colorIdx = defaultColumns.findIndex(c => c.key === 'color');
      const writerColumns: TableColumn[] = [
        { key: 'description', label: 'Description', width: undefined },
        { key: 'currency.name', label: 'Currency', width: undefined, template: 'link', linkPath: ['/currency', 'currencyId'] },
        { key: 'timeZone.name', label: 'Time Zone', width: undefined, template: 'link', linkPath: ['/timezone', 'timeZoneId'] },
        { key: 'calendar.name', label: 'Calendar', width: undefined, template: 'link', linkPath: ['/calendar', 'calendarId'] },
      ];
      defaultColumns.splice(colorIdx, 0, ...writerColumns);
    }

    if (isAdmin) {
      defaultColumns.push({ key: 'versionNumber', label: 'Version', width: undefined });
      defaultColumns.push({ key: 'active', label: 'Active', width: '80px', template: 'boolean' });
      defaultColumns.push({ key: 'deleted', label: 'Deleted', width: '80px', template: 'boolean' });
    }
    else if (isWriter) {
      defaultColumns.push({ key: 'versionNumber', label: 'Version', width: undefined });
    }


    // Assign the built array as the active columns
    this.columns = defaultColumns;
  }


  public sortBy(column: string): void {

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

    if (this.clientService.userIsSchedulerClientReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Clients", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const clientQueryParams = {
      ...this.queryParams,
      anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.clientService.GetClientList(clientQueryParams).subscribe({
      next: (ClientList) => {
        if (ClientList) {
          this.Clients = ClientList;
        } else {
          this.Clients = [];
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

        this.alertService.showMessage("Error getting Client data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.Clients) {
      this.filteredClients = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.Clients];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
          'name',
          'description',
          'clientType.name',
          'currency.name',
          'timeZone.name',
          'calendar.name',
          'addressLine1',
          'addressLine2',
          'city',
          'postalCode',
          'stateProvince.name',
          'country.name',
          'phone',
          'email',
          'latitude',
          'longitude',
          'notes',
          'externalId',
          'color',
          'attributes',
          'avatarFileName',
          'avatarSize',
          'avatarData',
          'avatarMimeType',
        ];

        result = result.filter((client) =>

          filterFields.some((field) => {
            const value = getNestedValue(client, field);
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
        const comparison = aStr.localeCompare(bStr, undefined, { sensitivity: 'base' });
        return this.sortDirection === 'asc' ? comparison : -comparison;
      });
    }

    this.filteredClients = result;
  }


  public handleEdit(client: ClientData): void {
    if (this.disableDefaultEdit) {
      this.edit.emit(client); // Let parent handle edit
    }
    else if (this.addEditClientComponent) {
      this.addEditClientComponent.openModal(client); // Default edit behavior
    }
    else {
      this.alertService.showMessage(
        'Edit functionality unavailable',
        'Add/Edit component not initialized',
        MessageSeverity.warn
      );
    }
  }


  public handleDelete(client: ClientData): void {
    if (this.disableDefaultDelete) {
      this.delete.emit(client); // Let parent handle delete
    }
    else {
      this.confirmationService
        .confirm('Delete Client', 'Are you sure you want to delete this Client?')
        .then((result) => {
          if (result) {
            this.deleteClient(client);
          }
        })
        .catch(() => { });
    }
  }


  private deleteClient(clientData: ClientData): void {
    this.clientService.DeleteClient(clientData.id).subscribe({
      next: () => {
        this.clientService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
        this.alertService.showMessage("Error deleting Client", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(client: ClientData): void {
    if (this.disableDefaultUndelete) {
      this.undelete.emit(client); // Let parent handle undelete
    }
    else {
      this.confirmationService
        .confirm('Undelete Client', 'Are you sure you want to undelete this Client?')
        .then((result) => {
          if (result) {
            this.undeleteClient(client);
          }
        })
        .catch(() => { });
    }
  }


  private undeleteClient(clientData: ClientData): void {

    var clientToSubmit = this.clientService.ConvertToClientSubmitData(clientData); // Convert Client data to post object for undeleting
    clientToSubmit.deleted = false;

    this.clientService.PutClient(clientToSubmit.id, clientToSubmit).subscribe({
      next: () => {
        this.clientService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
        this.alertService.showMessage("Error undeleting Client", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getClientId(index: number, client: any): number {
    return client.id;
  }


  public userIsSchedulerClientReader(): boolean {
    return this.clientService.userIsSchedulerClientReader();
  }

  public userIsSchedulerClientWriter(): boolean {
    return this.clientService.userIsSchedulerClientWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/client', clientId]
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

import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ContactService, ContactData, ContactQueryParameters } from '../../../scheduler-data-services/contact.service';
import { ContactCustomAddEditComponent } from '../contact-custom-add-edit/contact-custom-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-contact-custom-table',
  templateUrl: './contact-custom-table.component.html',
  styleUrls: ['./contact-custom-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactCustomTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(ContactCustomAddEditComponent) addEditContactComponent!: ContactCustomAddEditComponent;

  @Input() Contacts: ContactData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<ContactQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<ContactData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<ContactData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<ContactData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredContacts: ContactData[] | null = null;        // Stores the filtered/sorted data

  // Sorting properties
  public sortColumn: string | null = null;
  public sortDirection: 'asc' | 'desc' = 'asc';


  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  private isManagingData: boolean = false; // Tracks if component is managing data loading

  private debounceTimeout: any;

  constructor(private contactService: ContactService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.Contacts) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the contactChanged observable on the add/edit component so that when a Contact changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditContactComponent && !this.disableDefaultEdit) {
        this.addEditContactComponent.contactChanged.subscribe({
        next: (result: ContactData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Contact changed notification", JSON.stringify(err), MessageSeverity.error);
        }
        });
     }
  }

  ngOnChanges(changes: SimpleChanges): void {

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
    { key: 'contactType.name', label: 'Contact Type', width: undefined, template: 'link', linkPath: ['/contacttype', 'contactTypeId'] },
    { key: 'firstName', label: 'First Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/contact', 'id']  },
    { key: 'middleName', label: 'Middle Name', width: undefined },
    { key: 'lastName', label: 'Last Name', width: undefined },
    { key: 'salutation.name', label: 'Salutation', width: undefined, template: 'link', linkPath: ['/salutation', 'salutationId'] },
    { key: 'title', label: 'Title', width: undefined },
    { key: 'birthDate', label: 'Birth Date', width: undefined, template: 'date' },
    { key: 'company', label: 'Company', width: undefined },
    { key: 'email', label: 'Email', width: undefined },
    { key: 'phone', label: 'Phone', width: undefined },
    { key: 'mobile', label: 'Mobile', width: undefined },
    { key: 'position', label: 'Position', width: undefined },
    { key: 'webSite', label: 'Web Site', width: undefined },
    { key: 'contactMethod.name', label: 'Contact Method', width: undefined, template: 'link', linkPath: ['/contactmethod', 'contactMethodId'] },
    { key: 'timeZone.name', label: 'Time Zone', width: undefined, template: 'link', linkPath: ['/timezone', 'timeZoneId'] },
    { key: 'notes', label: 'Notes', width: undefined },
    { key: 'icon.name', label: 'Icon', width: undefined, template: 'link', linkPath: ['/icon', 'iconId'] },
    { key: 'color', label: 'Color', width: "50px", template: 'color' },
    // { key: 'avatarFileName', label: 'Avatar File Name', width: undefined },
    // { key: 'avatarSize', label: 'Avatar Size', width: undefined },
    // { key: 'avatarData', label: 'Avatar Data', width: undefined },
    // { key: 'avatarMimeType', label: 'Avatar Mime Type', width: undefined },
    // { key: 'externalId', label: 'External Id', width: undefined },

    ];

    const isWriter = this.contactService.userIsSchedulerContactWriter();
    const isAdmin = this.authService.isSchedulerAdministrator; 

    if (isAdmin) {
     defaultColumns.push({ key: 'versionNumber', label: 'Version Number', width: undefined });
     defaultColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
     defaultColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });

    }
    else if (isWriter) {
     defaultColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
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

    if (this.contactService.userIsSchedulerContactReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Contacts", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const contactQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.contactService.GetContactList(contactQueryParams).subscribe({
      next: (ContactList) => {
        if (ContactList) {
          this.Contacts = ContactList;
        } else {
          this.Contacts = [];
        }

        //
        // Apply the sort.  Filtering of data done already done in the data we receive from the service.
        //
        this.applyFiltersAndSort();

        //
        // Clear the loading spinner
        //
        this.isLoadingSubject.next(false);

      },
      error: (err) => {

        //
        // Clear the loading spinner
        //
        this.isLoadingSubject.next(false);

         this.alertService.showMessage("Error getting Contact data", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


   private applyFiltersAndSort(): void {

    if (!this.Contacts) {
      this.filteredContacts = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.Contacts];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'contactType.name',
                      'firstName',
                      'middleName',
                      'lastName',
                      'salutation.name',
                      'title',
                      'birthDate',
                      'company',
                      'email',
                      'phone',
                      'mobile',
                      'position',
                      'webSite',
                      'contactMethod.name',
                      'notes',
                      'icon.name',
                      'color',
                      'avatarFileName',
                      'avatarSize',
                      'avatarData',
                      'avatarMimeType',
                      'externalId',
        ];

        result = result.filter((contact) =>

        filterFields.some((field) => {
        const value = getNestedValue(contact, field);
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

    this.filteredContacts = result;
  }


  public handleEdit(contact: ContactData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(contact); // Let parent handle edit
    }
    else if (this.addEditContactComponent)
    {
        this.addEditContactComponent.openModal(contact); // Default edit behavior
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


  public handleDelete(contact: ContactData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(contact); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete Contact', 'Are you sure you want to delete this Contact?')
          .then((result) => {
              if (result)
              {
                  this.deleteContact(contact);
              }
          })
          .catch(() => { });
    }
  }


  private deleteContact(contactData: ContactData): void {
    this.contactService.DeleteContact(contactData.id).subscribe({
      next: () => {
       this.contactService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Contact", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(contact: ContactData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(contact); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete Contact', 'Are you sure you want to undelete this Contact?')
          .then((result) => {
              if (result)
              {
                  this.undeleteContact(contact);
              }
          })
          .catch(() => { });
    }
}


  private undeleteContact(contactData: ContactData): void {

      var contactToSubmit = this.contactService.ConvertToContactSubmitData(contactData); // Convert Contact data to post object for undeleting
      contactToSubmit.deleted = false;

      this.contactService.PutContact(contactToSubmit.id, contactToSubmit).subscribe({
      next: () => {
       this.contactService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Contact", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getContactId(index: number, contact: any): number {
    return contact.id;
  }


  public userIsSchedulerContactReader(): boolean {
    return this.contactService.userIsSchedulerContactReader();
  }

  public userIsSchedulerContactWriter(): boolean {
    return this.contactService.userIsSchedulerContactWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/contact', contactId]
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

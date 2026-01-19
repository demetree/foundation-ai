/*
   GENERATED FORM FOR THE SECURITYUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { SecurityUserService, SecurityUserData, SecurityUserQueryParameters } from '../../../security-data-services/security-user.service';
import { SecurityUserAddEditComponent } from '../security-user-add-edit/security-user-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-user-table',
  templateUrl: './security-user-table.component.html',
  styleUrls: ['./security-user-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SecurityUserTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(SecurityUserAddEditComponent) addEditSecurityUserComponent!: SecurityUserAddEditComponent;

  @Input() SecurityUsers: SecurityUserData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<SecurityUserQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<SecurityUserData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<SecurityUserData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<SecurityUserData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredSecurityUsers: SecurityUserData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private securityUserService: SecurityUserService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.SecurityUsers) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the securityUserChanged observable on the add/edit component so that when a SecurityUser changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditSecurityUserComponent && !this.disableDefaultEdit) {
        this.addEditSecurityUserComponent.securityUserChanged.subscribe({
        next: (result: SecurityUserData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Security User changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'accountName', label: 'Account Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/securityuser', 'id']  },
    { key: 'activeDirectoryAccount', label: 'Active Directory Account', width: '120px', template: 'boolean' },
    { key: 'canLogin', label: 'Can Login', width: '120px', template: 'boolean' },
    { key: 'mustChangePassword', label: 'Must Change Password', width: '120px', template: 'boolean' },
    { key: 'firstName', label: 'First Name', width: undefined },
    { key: 'middleName', label: 'Middle Name', width: undefined },
    { key: 'lastName', label: 'Last Name', width: undefined },
    { key: 'dateOfBirth', label: 'Date Of Birth', width: undefined, template: 'date' },
    { key: 'emailAddress', label: 'Email Address', width: undefined },
    { key: 'cellPhoneNumber', label: 'Cell Phone Number', width: undefined },
    { key: 'phoneNumber', label: 'Phone Number', width: undefined },
    { key: 'phoneExtension', label: 'Phone Extension', width: undefined },
    { key: 'description', label: 'Description', width: undefined },
    { key: 'securityUserTitle.name', label: 'Security User Title', width: undefined, template: 'link', linkPath: ['/securityusertitle', 'securityUserTitleId'] },
    { key: 'reportsToSecurityUser.name', label: 'Security User', width: undefined, template: 'link', linkPath: ['/securityuser', 'reportsToSecurityUserId'] },
    { key: 'authenticationDomain', label: 'Authentication Domain', width: undefined },
    { key: 'failedLoginCount', label: 'Failed Login Count', width: undefined },
    { key: 'lastLoginAttempt', label: 'Last Login Attempt', width: undefined, template: 'date' },
    { key: 'mostRecentActivity', label: 'Most Recent Activity', width: undefined, template: 'date' },
    { key: 'alternateIdentifier', label: 'Alternate Identifier', width: undefined },
    { key: 'image', label: 'Image', width: undefined },
    { key: 'settings', label: 'Settings', width: undefined },
    { key: 'securityTenant.name', label: 'Security Tenant', width: undefined, template: 'link', linkPath: ['/securitytenant', 'securityTenantId'] },
    { key: 'readPermissionLevel', label: 'Read Permission Level', width: undefined },
    { key: 'writePermissionLevel', label: 'Write Permission Level', width: undefined },
    { key: 'securityOrganization.name', label: 'Security Organization', width: undefined, template: 'link', linkPath: ['/securityorganization', 'securityOrganizationId'] },
    { key: 'securityDepartment.name', label: 'Security Department', width: undefined, template: 'link', linkPath: ['/securitydepartment', 'securityDepartmentId'] },
    { key: 'securityTeam.name', label: 'Security Team', width: undefined, template: 'link', linkPath: ['/securityteam', 'securityTeamId'] },
    { key: 'authenticationToken', label: 'Authentication Token', width: undefined },
    { key: 'authenticationTokenExpiry', label: 'Authentication Token Expiry', width: undefined, template: 'date' },
    { key: 'twoFactorToken', label: 'Two Factor Token', width: undefined },
    { key: 'twoFactorTokenExpiry', label: 'Two Factor Token Expiry', width: undefined, template: 'date' },
    { key: 'twoFactorSendByEmail', label: 'Two Factor Send By Email', width: '120px', template: 'boolean' },
    { key: 'twoFactorSendBySMS', label: 'Two Factor Send By S M S', width: '120px', template: 'boolean' },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.securityUserService.userIsSecuritySecurityUserWriter();
    const isAdmin = this.authService.isSecurityAdministrator; 

    if (isAdmin) {
     defaultColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
     defaultColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });

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

    if (this.securityUserService.userIsSecuritySecurityUserReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Security Users", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const securityUserQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.securityUserService.GetSecurityUserList(securityUserQueryParams).subscribe({
      next: (SecurityUserList) => {
        if (SecurityUserList) {
          this.SecurityUsers = SecurityUserList;
        } else {
          this.SecurityUsers = [];
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

         this.alertService.showMessage("Error getting Security User data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.SecurityUsers) {
      this.filteredSecurityUsers = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.SecurityUsers];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'accountName',
                      'activeDirectoryAccount',
                      'canLogin',
                      'mustChangePassword',
                      'firstName',
                      'middleName',
                      'lastName',
                      'dateOfBirth',
                      'emailAddress',
                      'cellPhoneNumber',
                      'phoneNumber',
                      'phoneExtension',
                      'description',
                      'securityUserTitle.name',
                      'reportsToSecurityUser.name',
                      'authenticationDomain',
                      'failedLoginCount',
                      'lastLoginAttempt',
                      'mostRecentActivity',
                      'alternateIdentifier',
                      'image',
                      'settings',
                      'securityTenant.name',
                      'readPermissionLevel',
                      'writePermissionLevel',
                      'securityOrganization.name',
                      'securityDepartment.name',
                      'securityTeam.name',
                      'authenticationToken',
                      'authenticationTokenExpiry',
                      'twoFactorToken',
                      'twoFactorTokenExpiry',
                      'twoFactorSendByEmail',
                      'twoFactorSendBySMS',
        ];

        result = result.filter((securityUser) =>

        filterFields.some((field) => {
        const value = getNestedValue(securityUser, field);
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

    this.filteredSecurityUsers = result;
  }


  public handleEdit(securityUser: SecurityUserData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(securityUser); // Let parent handle edit
    }
    else if (this.addEditSecurityUserComponent)
    {
        this.addEditSecurityUserComponent.openModal(securityUser); // Default edit behavior
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


  public handleDelete(securityUser: SecurityUserData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(securityUser); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete SecurityUser', 'Are you sure you want to delete this Security User?')
          .then((result) => {
              if (result)
              {
                  this.deleteSecurityUser(securityUser);
              }
          })
          .catch(() => { });
    }
  }


  private deleteSecurityUser(securityUserData: SecurityUserData): void {
    this.securityUserService.DeleteSecurityUser(securityUserData.id).subscribe({
      next: () => {
       this.securityUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Security User", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(securityUser: SecurityUserData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(securityUser); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete SecurityUser', 'Are you sure you want to undelete this Security User?')
          .then((result) => {
              if (result)
              {
                  this.undeleteSecurityUser(securityUser);
              }
          })
          .catch(() => { });
    }
}


  private undeleteSecurityUser(securityUserData: SecurityUserData): void {

      var securityUserToSubmit = this.securityUserService.ConvertToSecurityUserSubmitData(securityUserData); // Convert SecurityUser data to post object for undeleting
      securityUserToSubmit.deleted = false;

      this.securityUserService.PutSecurityUser(securityUserToSubmit.id, securityUserToSubmit).subscribe({
      next: () => {
       this.securityUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Security User", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getSecurityUserId(index: number, securityUser: any): number {
    return securityUser.id;
  }


  public userIsSecuritySecurityUserReader(): boolean {
    return this.securityUserService.userIsSecuritySecurityUserReader();
  }

  public userIsSecuritySecurityUserWriter(): boolean {
    return this.securityUserService.userIsSecuritySecurityUserWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/securityUser', securityUserId]
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

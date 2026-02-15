/*
   GENERATED FORM FOR THE USERWISHLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserWishlistItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-wishlist-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { UserWishlistItemService, UserWishlistItemData, UserWishlistItemQueryParameters } from '../../../bmc-data-services/user-wishlist-item.service';
import { UserWishlistItemAddEditComponent } from '../user-wishlist-item-add-edit/user-wishlist-item-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-user-wishlist-item-table',
  templateUrl: './user-wishlist-item-table.component.html',
  styleUrls: ['./user-wishlist-item-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserWishlistItemTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(UserWishlistItemAddEditComponent) addEditUserWishlistItemComponent!: UserWishlistItemAddEditComponent;

  @Input() UserWishlistItems: UserWishlistItemData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<UserWishlistItemQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<UserWishlistItemData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<UserWishlistItemData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<UserWishlistItemData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredUserWishlistItems: UserWishlistItemData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private userWishlistItemService: UserWishlistItemService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.UserWishlistItems) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the userWishlistItemChanged observable on the add/edit component so that when a UserWishlistItem changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditUserWishlistItemComponent && !this.disableDefaultEdit) {
        this.addEditUserWishlistItemComponent.userWishlistItemChanged.subscribe({
        next: (result: UserWishlistItemData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during User Wishlist Item changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'userCollection.name', label: 'User Collection', width: undefined, template: 'link', linkPath: ['/usercollection', 'userCollectionId'] },
    { key: 'brickPart.name', label: 'Brick Part', width: undefined, template: 'link', linkPath: ['/brickpart', 'brickPartId'] },
    { key: 'brickColour.name', label: 'Brick Colour', width: undefined, template: 'link', linkPath: ['/brickcolour', 'brickColourId'] },
    { key: 'quantityDesired', label: 'Quantity Desired', width: undefined },
    { key: 'notes', label: 'Notes', width: undefined },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.userWishlistItemService.userIsBMCUserWishlistItemWriter();
    const isAdmin = this.authService.isBMCAdministrator; 

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

    if (this.userWishlistItemService.userIsBMCUserWishlistItemReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from User Wishlist Items", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const userWishlistItemQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.userWishlistItemService.GetUserWishlistItemList(userWishlistItemQueryParams).subscribe({
      next: (UserWishlistItemList) => {
        if (UserWishlistItemList) {
          this.UserWishlistItems = UserWishlistItemList;
        } else {
          this.UserWishlistItems = [];
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

         this.alertService.showMessage("Error getting User Wishlist Item data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.UserWishlistItems) {
      this.filteredUserWishlistItems = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.UserWishlistItems];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'userCollection.name',
                      'brickPart.name',
                      'brickColour.name',
                      'quantityDesired',
                      'notes',
        ];

        result = result.filter((userWishlistItem) =>

        filterFields.some((field) => {
        const value = getNestedValue(userWishlistItem, field);
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

    this.filteredUserWishlistItems = result;
  }


  public handleEdit(userWishlistItem: UserWishlistItemData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(userWishlistItem); // Let parent handle edit
    }
    else if (this.addEditUserWishlistItemComponent)
    {
        this.addEditUserWishlistItemComponent.openModal(userWishlistItem); // Default edit behavior
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


  public handleDelete(userWishlistItem: UserWishlistItemData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(userWishlistItem); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete UserWishlistItem', 'Are you sure you want to delete this User Wishlist Item?')
          .then((result) => {
              if (result)
              {
                  this.deleteUserWishlistItem(userWishlistItem);
              }
          })
          .catch(() => { });
    }
  }


  private deleteUserWishlistItem(userWishlistItemData: UserWishlistItemData): void {
    this.userWishlistItemService.DeleteUserWishlistItem(userWishlistItemData.id).subscribe({
      next: () => {
       this.userWishlistItemService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting User Wishlist Item", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(userWishlistItem: UserWishlistItemData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(userWishlistItem); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete UserWishlistItem', 'Are you sure you want to undelete this User Wishlist Item?')
          .then((result) => {
              if (result)
              {
                  this.undeleteUserWishlistItem(userWishlistItem);
              }
          })
          .catch(() => { });
    }
}


  private undeleteUserWishlistItem(userWishlistItemData: UserWishlistItemData): void {

      var userWishlistItemToSubmit = this.userWishlistItemService.ConvertToUserWishlistItemSubmitData(userWishlistItemData); // Convert UserWishlistItem data to post object for undeleting
      userWishlistItemToSubmit.deleted = false;

      this.userWishlistItemService.PutUserWishlistItem(userWishlistItemToSubmit.id, userWishlistItemToSubmit).subscribe({
      next: () => {
       this.userWishlistItemService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting User Wishlist Item", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getUserWishlistItemId(index: number, userWishlistItem: any): number {
    return userWishlistItem.id;
  }


  public userIsBMCUserWishlistItemReader(): boolean {
    return this.userWishlistItemService.userIsBMCUserWishlistItemReader();
  }

  public userIsBMCUserWishlistItemWriter(): boolean {
    return this.userWishlistItemService.userIsBMCUserWishlistItemWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/userWishlistItem', userWishlistItemId]
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

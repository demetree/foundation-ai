/*
   GENERATED FORM FOR THE MOCCOLLABORATOR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocCollaborator table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-collaborator-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { MocCollaboratorService, MocCollaboratorData, MocCollaboratorQueryParameters } from '../../../bmc-data-services/moc-collaborator.service';
import { MocCollaboratorAddEditComponent } from '../moc-collaborator-add-edit/moc-collaborator-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-moc-collaborator-table',
  templateUrl: './moc-collaborator-table.component.html',
  styleUrls: ['./moc-collaborator-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MocCollaboratorTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(MocCollaboratorAddEditComponent) addEditMocCollaboratorComponent!: MocCollaboratorAddEditComponent;

  @Input() MocCollaborators: MocCollaboratorData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<MocCollaboratorQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Input() showAddButton: boolean = false;              // Forward to embedded add-edit component
  @Input() preSeededData: any = null;                   // Forward to embedded add-edit component
  @Input() hiddenFields: string[] = [];                 // Forward to embedded add-edit component

  @Output() edit = new EventEmitter<MocCollaboratorData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<MocCollaboratorData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<MocCollaboratorData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredMocCollaborators: MocCollaboratorData[] | null = null;        // Stores the filtered/sorted data

  // Sorting properties
  public sortColumn: string | null = null;
  public sortDirection: 'asc' | 'desc' = 'asc';

  // Pagination
  @Input() totalRowCount: number = 0;
  public currentPage: number = 1;
  public pageSize: number = 50;


  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  private isManagingData: boolean = false; // Tracks if component is managing data loading

  private debounceTimeout: any;


  //
  // Error state tracking to help suppress endless loop scenarios in case of server error or request path errors
  //
  private inErrorState: boolean = false;
  private errorResetTimeout: any;


  constructor(private mocCollaboratorService: MocCollaboratorService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.MocCollaborators) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the mocCollaboratorChanged observable on the add/edit component so that when a MocCollaborator changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditMocCollaboratorComponent && !this.disableDefaultEdit) {
        this.addEditMocCollaboratorComponent.mocCollaboratorChanged.subscribe({
        next: (result: MocCollaboratorData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Moc Collaborator changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'publishedMoc.name', label: 'Published Moc', width: undefined, template: 'link', linkPath: ['/publishedmoc', 'publishedMocId'] },
    { key: 'collaboratorTenantGuid', label: 'Collaborator Tenant Guid', width: undefined },
    { key: 'accessLevel', label: 'Access Level', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/moccollaborator', 'id']  },
    { key: 'invitedDate', label: 'Invited Date', width: undefined, template: 'date' },
    { key: 'acceptedDate', label: 'Accepted Date', width: undefined, template: 'date' },
    { key: 'isAccepted', label: 'Is Accepted', width: '120px', template: 'boolean' },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.mocCollaboratorService.userIsBMCMocCollaboratorWriter();
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

    if (this.mocCollaboratorService.userIsBMCMocCollaboratorReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Moc Collaborators", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const mocCollaboratorQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined,
        pageSize: this.pageSize,
        pageNumber: this.currentPage
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.mocCollaboratorService.GetMocCollaboratorList(mocCollaboratorQueryParams).subscribe({
      next: (MocCollaboratorList) => {
        if (MocCollaboratorList) {
          this.MocCollaborators = MocCollaboratorList;
        } else {
          this.MocCollaborators = [];
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

         this.alertService.showMessage("Error getting Moc Collaborator data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.MocCollaborators) {
      this.filteredMocCollaborators = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.MocCollaborators];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'publishedMoc.name',
                      'collaboratorTenantGuid',
                      'accessLevel',
                      'invitedDate',
                      'acceptedDate',
                      'isAccepted',
        ];

        result = result.filter((mocCollaborator) =>

        filterFields.some((field) => {
        const value = getNestedValue(mocCollaborator, field);
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

    this.filteredMocCollaborators = result;
  }


  public handleEdit(mocCollaborator: MocCollaboratorData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(mocCollaborator); // Let parent handle edit
    }
    else if (this.addEditMocCollaboratorComponent)
    {
        this.addEditMocCollaboratorComponent.openModal(mocCollaborator); // Default edit behavior
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


  public handleAdd(): void {
    if (this.addEditMocCollaboratorComponent)
    {
        this.addEditMocCollaboratorComponent.openModal(); // Open in add mode (no data)
    }
}


  public handleDelete(mocCollaborator: MocCollaboratorData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(mocCollaborator); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete MocCollaborator', 'Are you sure you want to delete this Moc Collaborator?')
          .then((result) => {
              if (result)
              {
                  this.deleteMocCollaborator(mocCollaborator);
              }
          })
          .catch(() => { });
    }
  }


  private deleteMocCollaborator(mocCollaboratorData: MocCollaboratorData): void {
    this.mocCollaboratorService.DeleteMocCollaborator(mocCollaboratorData.id).subscribe({
      next: () => {
       this.mocCollaboratorService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Moc Collaborator", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(mocCollaborator: MocCollaboratorData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(mocCollaborator); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete MocCollaborator', 'Are you sure you want to undelete this Moc Collaborator?')
          .then((result) => {
              if (result)
              {
                  this.undeleteMocCollaborator(mocCollaborator);
              }
          })
          .catch(() => { });
    }
}


  private undeleteMocCollaborator(mocCollaboratorData: MocCollaboratorData): void {

      var mocCollaboratorToSubmit = this.mocCollaboratorService.ConvertToMocCollaboratorSubmitData(mocCollaboratorData); // Convert MocCollaborator data to post object for undeleting
      mocCollaboratorToSubmit.deleted = false;

      this.mocCollaboratorService.PutMocCollaborator(mocCollaboratorToSubmit.id, mocCollaboratorToSubmit).subscribe({
      next: () => {
       this.mocCollaboratorService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Moc Collaborator", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getMocCollaboratorId(index: number, mocCollaborator: any): number {
    return mocCollaborator.id;
  }


  public userIsBMCMocCollaboratorReader(): boolean {
    return this.mocCollaboratorService.userIsBMCMocCollaboratorReader();
  }

  public userIsBMCMocCollaboratorWriter(): boolean {
    return this.mocCollaboratorService.userIsBMCMocCollaboratorWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/mocCollaborator', mocCollaboratorId]
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


  //
  // Pagination
  //
  public get totalPages(): number {
    if (this.totalRowCount <= 0 || this.pageSize <= 0) {
      return 1;
    }
    return Math.ceil(this.totalRowCount / this.pageSize);
  }

  public nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadData();
    }
  }

  public previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadData();
    }
  }

  public goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadData();
    }
  }

  public resetToFirstPage(): void {
    this.currentPage = 1;
  }

}

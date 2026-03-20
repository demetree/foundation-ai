/*
   GENERATED FORM FOR THE DOCUMENTSHARELINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DocumentShareLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to document-share-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { DocumentShareLinkService, DocumentShareLinkData, DocumentShareLinkQueryParameters } from '../../../scheduler-data-services/document-share-link.service';
import { DocumentShareLinkAddEditComponent } from '../document-share-link-add-edit/document-share-link-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-document-share-link-table',
  templateUrl: './document-share-link-table.component.html',
  styleUrls: ['./document-share-link-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocumentShareLinkTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(DocumentShareLinkAddEditComponent) addEditDocumentShareLinkComponent!: DocumentShareLinkAddEditComponent;

  @Input() DocumentShareLinks: DocumentShareLinkData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<DocumentShareLinkQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Input() showAddButton: boolean = false;              // Forward to embedded add-edit component
  @Input() preSeededData: any = null;                   // Forward to embedded add-edit component
  @Input() hiddenFields: string[] = [];                 // Forward to embedded add-edit component

  @Output() edit = new EventEmitter<DocumentShareLinkData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<DocumentShareLinkData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<DocumentShareLinkData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredDocumentShareLinks: DocumentShareLinkData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private documentShareLinkService: DocumentShareLinkService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.DocumentShareLinks) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the documentShareLinkChanged observable on the add/edit component so that when a DocumentShareLink changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditDocumentShareLinkComponent && !this.disableDefaultEdit) {
        this.addEditDocumentShareLinkComponent.documentShareLinkChanged.subscribe({
        next: (result: DocumentShareLinkData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Document Share Link changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'document.name', label: 'Document', width: undefined, template: 'link', linkPath: ['/document', 'documentId'] },
    { key: 'token', label: 'Token', width: undefined },
    { key: 'passwordHash', label: 'Password Hash', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/documentsharelink', 'id']  },
    { key: 'expiresAt', label: 'Expires At', width: undefined, template: 'date' },
    { key: 'maxDownloads', label: 'Max Downloads', width: undefined },
    { key: 'downloadCount', label: 'Download Count', width: undefined },
    { key: 'createdBy', label: 'Created By', width: undefined },
    { key: 'createdDate', label: 'Created Date', width: undefined, template: 'date' },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.documentShareLinkService.userIsSchedulerDocumentShareLinkWriter();
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

    if (this.documentShareLinkService.userIsSchedulerDocumentShareLinkReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Document Share Links", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const documentShareLinkQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined,
        pageSize: this.pageSize,
        pageNumber: this.currentPage
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.documentShareLinkService.GetDocumentShareLinkList(documentShareLinkQueryParams).subscribe({
      next: (DocumentShareLinkList) => {
        if (DocumentShareLinkList) {
          this.DocumentShareLinks = DocumentShareLinkList;
        } else {
          this.DocumentShareLinks = [];
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

         this.alertService.showMessage("Error getting Document Share Link data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.DocumentShareLinks) {
      this.filteredDocumentShareLinks = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.DocumentShareLinks];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'document.name',
                      'token',
                      'passwordHash',
                      'expiresAt',
                      'maxDownloads',
                      'downloadCount',
                      'createdBy',
                      'createdDate',
        ];

        result = result.filter((documentShareLink) =>

        filterFields.some((field) => {
        const value = getNestedValue(documentShareLink, field);
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

    this.filteredDocumentShareLinks = result;
  }


  public handleEdit(documentShareLink: DocumentShareLinkData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(documentShareLink); // Let parent handle edit
    }
    else if (this.addEditDocumentShareLinkComponent)
    {
        this.addEditDocumentShareLinkComponent.openModal(documentShareLink); // Default edit behavior
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
    if (this.addEditDocumentShareLinkComponent)
    {
        this.addEditDocumentShareLinkComponent.openModal(); // Open in add mode (no data)
    }
}


  public handleDelete(documentShareLink: DocumentShareLinkData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(documentShareLink); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete DocumentShareLink', 'Are you sure you want to delete this Document Share Link?')
          .then((result) => {
              if (result)
              {
                  this.deleteDocumentShareLink(documentShareLink);
              }
          })
          .catch(() => { });
    }
  }


  private deleteDocumentShareLink(documentShareLinkData: DocumentShareLinkData): void {
    this.documentShareLinkService.DeleteDocumentShareLink(documentShareLinkData.id).subscribe({
      next: () => {
       this.documentShareLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Document Share Link", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(documentShareLink: DocumentShareLinkData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(documentShareLink); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete DocumentShareLink', 'Are you sure you want to undelete this Document Share Link?')
          .then((result) => {
              if (result)
              {
                  this.undeleteDocumentShareLink(documentShareLink);
              }
          })
          .catch(() => { });
    }
}


  private undeleteDocumentShareLink(documentShareLinkData: DocumentShareLinkData): void {

      var documentShareLinkToSubmit = this.documentShareLinkService.ConvertToDocumentShareLinkSubmitData(documentShareLinkData); // Convert DocumentShareLink data to post object for undeleting
      documentShareLinkToSubmit.deleted = false;

      this.documentShareLinkService.PutDocumentShareLink(documentShareLinkToSubmit.id, documentShareLinkToSubmit).subscribe({
      next: () => {
       this.documentShareLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Document Share Link", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getDocumentShareLinkId(index: number, documentShareLink: any): number {
    return documentShareLink.id;
  }


  public userIsSchedulerDocumentShareLinkReader(): boolean {
    return this.documentShareLinkService.userIsSchedulerDocumentShareLinkReader();
  }

  public userIsSchedulerDocumentShareLinkWriter(): boolean {
    return this.documentShareLinkService.userIsSchedulerDocumentShareLinkWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/documentShareLink', documentShareLinkId]
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

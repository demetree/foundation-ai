/*
   GENERATED FORM FOR THE GENERALLEDGERLINE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from GeneralLedgerLine table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to general-ledger-line-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { GeneralLedgerLineService, GeneralLedgerLineData, GeneralLedgerLineQueryParameters } from '../../../scheduler-data-services/general-ledger-line.service';
import { GeneralLedgerLineAddEditComponent } from '../general-ledger-line-add-edit/general-ledger-line-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-general-ledger-line-table',
  templateUrl: './general-ledger-line-table.component.html',
  styleUrls: ['./general-ledger-line-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GeneralLedgerLineTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(GeneralLedgerLineAddEditComponent) addEditGeneralLedgerLineComponent!: GeneralLedgerLineAddEditComponent;

  @Input() GeneralLedgerLines: GeneralLedgerLineData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<GeneralLedgerLineQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior

  @Input() showAddButton: boolean = false;              // Forward to embedded add-edit component
  @Input() preSeededData: any = null;                   // Forward to embedded add-edit component
  @Input() hiddenFields: string[] = [];                 // Forward to embedded add-edit component

  @Output() edit = new EventEmitter<GeneralLedgerLineData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<GeneralLedgerLineData>(); // Emitted for custom delete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredGeneralLedgerLines: GeneralLedgerLineData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private generalLedgerLineService: GeneralLedgerLineService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.GeneralLedgerLines) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the generalLedgerLineChanged observable on the add/edit component so that when a GeneralLedgerLine changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditGeneralLedgerLineComponent && !this.disableDefaultEdit) {
        this.addEditGeneralLedgerLineComponent.generalLedgerLineChanged.subscribe({
        next: (result: GeneralLedgerLineData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during General Ledger Line changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'generalLedgerEntry.name', label: 'General Ledger Entry', width: undefined, template: 'link', linkPath: ['/generalledgerentry', 'generalLedgerEntryId'] },
    { key: 'financialCategory.name', label: 'Financial Category', width: undefined, template: 'link', linkPath: ['/financialcategory', 'financialCategoryId'] },
    { key: 'debitAmount', label: 'Debit Amount', width: undefined },
    { key: 'creditAmount', label: 'Credit Amount', width: undefined },
    { key: 'description', label: 'Description', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/generalledgerline', 'id']  },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter();
    const isAdmin = this.authService.isSchedulerAdministrator; 

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

    if (this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from General Ledger Lines", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const generalLedgerLineQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined,
        pageSize: this.pageSize,
        pageNumber: this.currentPage
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.generalLedgerLineService.GetGeneralLedgerLineList(generalLedgerLineQueryParams).subscribe({
      next: (GeneralLedgerLineList) => {
        if (GeneralLedgerLineList) {
          this.GeneralLedgerLines = GeneralLedgerLineList;
        } else {
          this.GeneralLedgerLines = [];
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

         this.alertService.showMessage("Error getting General Ledger Line data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.GeneralLedgerLines) {
      this.filteredGeneralLedgerLines = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.GeneralLedgerLines];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'generalLedgerEntry.name',
                      'financialCategory.name',
                      'debitAmount',
                      'creditAmount',
                      'description',
        ];

        result = result.filter((generalLedgerLine) =>

        filterFields.some((field) => {
        const value = getNestedValue(generalLedgerLine, field);
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

    this.filteredGeneralLedgerLines = result;
  }


  public handleEdit(generalLedgerLine: GeneralLedgerLineData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(generalLedgerLine); // Let parent handle edit
    }
    else if (this.addEditGeneralLedgerLineComponent)
    {
        this.addEditGeneralLedgerLineComponent.openModal(generalLedgerLine); // Default edit behavior
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
    if (this.addEditGeneralLedgerLineComponent)
    {
        this.addEditGeneralLedgerLineComponent.openModal(); // Open in add mode (no data)
    }
}


  public handleDelete(generalLedgerLine: GeneralLedgerLineData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(generalLedgerLine); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete GeneralLedgerLine', 'Are you sure you want to delete this General Ledger Line?')
          .then((result) => {
              if (result)
              {
                  this.deleteGeneralLedgerLine(generalLedgerLine);
              }
          })
          .catch(() => { });
    }
  }


  private deleteGeneralLedgerLine(generalLedgerLineData: GeneralLedgerLineData): void {
    this.generalLedgerLineService.DeleteGeneralLedgerLine(generalLedgerLineData.id).subscribe({
      next: () => {
       this.generalLedgerLineService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting General Ledger Line", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getGeneralLedgerLineId(index: number, generalLedgerLine: any): number {
    return generalLedgerLine.id;
  }


  public userIsSchedulerGeneralLedgerLineReader(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineReader();
  }

  public userIsSchedulerGeneralLedgerLineWriter(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/generalLedgerLine', generalLedgerLineId]
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

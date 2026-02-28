/*
   GENERATED FORM FOR THE MODELSTEPPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModelStepPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to model-step-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ModelStepPartService, ModelStepPartData, ModelStepPartQueryParameters } from '../../../bmc-data-services/model-step-part.service';
import { ModelStepPartAddEditComponent } from '../model-step-part-add-edit/model-step-part-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-model-step-part-table',
  templateUrl: './model-step-part-table.component.html',
  styleUrls: ['./model-step-part-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModelStepPartTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(ModelStepPartAddEditComponent) addEditModelStepPartComponent!: ModelStepPartAddEditComponent;

  @Input() ModelStepParts: ModelStepPartData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<ModelStepPartQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Input() showAddButton: boolean = false;              // Forward to embedded add-edit component
  @Input() preSeededData: any = null;                   // Forward to embedded add-edit component
  @Input() hiddenFields: string[] = [];                 // Forward to embedded add-edit component

  @Output() edit = new EventEmitter<ModelStepPartData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<ModelStepPartData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<ModelStepPartData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredModelStepParts: ModelStepPartData[] | null = null;        // Stores the filtered/sorted data

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


  constructor(private modelStepPartService: ModelStepPartService,
              private authService: AuthService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.ModelStepParts) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the modelStepPartChanged observable on the add/edit component so that when a ModelStepPart changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditModelStepPartComponent && !this.disableDefaultEdit) {
        this.addEditModelStepPartComponent.modelStepPartChanged.subscribe({
        next: (result: ModelStepPartData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Model Step Part changed notification", JSON.stringify(err), MessageSeverity.error);
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
    { key: 'modelBuildStep.name', label: 'Model Build Step', width: undefined, template: 'link', linkPath: ['/modelbuildstep', 'modelBuildStepId'] },
    { key: 'brickPart.name', label: 'Brick Part', width: undefined, template: 'link', linkPath: ['/brickpart', 'brickPartId'] },
    { key: 'brickColour.name', label: 'Brick Colour', width: undefined, template: 'link', linkPath: ['/brickcolour', 'brickColourId'] },
    { key: 'partFileName', label: 'Part File Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/modelsteppart', 'id']  },
    { key: 'colorCode', label: 'Color Code', width: undefined },
    { key: 'positionX', label: 'Position X', width: undefined },
    { key: 'positionY', label: 'Position Y', width: undefined },
    { key: 'positionZ', label: 'Position Z', width: undefined },
    { key: 'transformMatrix', label: 'Transform Matrix', width: undefined },
    { key: 'sequence', label: 'Sequence', width: undefined },

    ];


    //
    // Note that CSS stylng shows deleted rows with a strike through, and inactive as italicized, both with transparency so they stand out, regardless of if there are active/deleted columns
    //
    const isWriter = this.modelStepPartService.userIsBMCModelStepPartWriter();
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

    if (this.modelStepPartService.userIsBMCModelStepPartReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Model Step Parts", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const modelStepPartQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined,
        pageSize: this.pageSize,
        pageNumber: this.currentPage
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.modelStepPartService.GetModelStepPartList(modelStepPartQueryParams).subscribe({
      next: (ModelStepPartList) => {
        if (ModelStepPartList) {
          this.ModelStepParts = ModelStepPartList;
        } else {
          this.ModelStepParts = [];
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

         this.alertService.showMessage("Error getting Model Step Part data", JSON.stringify(err), MessageSeverity.error);
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

    if (!this.ModelStepParts) {
      this.filteredModelStepParts = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.ModelStepParts];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'modelBuildStep.name',
                      'brickPart.name',
                      'brickColour.name',
                      'partFileName',
                      'colorCode',
                      'positionX',
                      'positionY',
                      'positionZ',
                      'transformMatrix',
                      'sequence',
        ];

        result = result.filter((modelStepPart) =>

        filterFields.some((field) => {
        const value = getNestedValue(modelStepPart, field);
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

    this.filteredModelStepParts = result;
  }


  public handleEdit(modelStepPart: ModelStepPartData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(modelStepPart); // Let parent handle edit
    }
    else if (this.addEditModelStepPartComponent)
    {
        this.addEditModelStepPartComponent.openModal(modelStepPart); // Default edit behavior
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
    if (this.addEditModelStepPartComponent)
    {
        this.addEditModelStepPartComponent.openModal(); // Open in add mode (no data)
    }
}


  public handleDelete(modelStepPart: ModelStepPartData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(modelStepPart); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete ModelStepPart', 'Are you sure you want to delete this Model Step Part?')
          .then((result) => {
              if (result)
              {
                  this.deleteModelStepPart(modelStepPart);
              }
          })
          .catch(() => { });
    }
  }


  private deleteModelStepPart(modelStepPartData: ModelStepPartData): void {
    this.modelStepPartService.DeleteModelStepPart(modelStepPartData.id).subscribe({
      next: () => {
       this.modelStepPartService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Model Step Part", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(modelStepPart: ModelStepPartData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(modelStepPart); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete ModelStepPart', 'Are you sure you want to undelete this Model Step Part?')
          .then((result) => {
              if (result)
              {
                  this.undeleteModelStepPart(modelStepPart);
              }
          })
          .catch(() => { });
    }
}


  private undeleteModelStepPart(modelStepPartData: ModelStepPartData): void {

      var modelStepPartToSubmit = this.modelStepPartService.ConvertToModelStepPartSubmitData(modelStepPartData); // Convert ModelStepPart data to post object for undeleting
      modelStepPartToSubmit.deleted = false;

      this.modelStepPartService.PutModelStepPart(modelStepPartToSubmit.id, modelStepPartToSubmit).subscribe({
      next: () => {
       this.modelStepPartService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Model Step Part", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getModelStepPartId(index: number, modelStepPart: any): number {
    return modelStepPart.id;
  }


  public userIsBMCModelStepPartReader(): boolean {
    return this.modelStepPartService.userIsBMCModelStepPartReader();
  }

  public userIsBMCModelStepPartWriter(): boolean {
    return this.modelStepPartService.userIsBMCModelStepPartWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/modelStepPart', modelStepPartId]
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

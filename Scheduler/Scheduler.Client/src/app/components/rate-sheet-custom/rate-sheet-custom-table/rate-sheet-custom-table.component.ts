import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { Subject, BehaviorSubject, takeUntil } from 'rxjs';
import { RateSheetService, RateSheetData, RateSheetQueryParameters } from '../../../scheduler-data-services/rate-sheet.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { RateSheetCustomAddEditComponent } from '../rate-sheet-custom-add-edit/rate-sheet-custom-add-edit.component'

import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-rate-sheet-custom-table',
  templateUrl: './rate-sheet-custom-table.component.html',
  styleUrls: ['./rate-sheet-custom-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RateSheetCustomTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(RateSheetCustomAddEditComponent) addEditRateSheetComponent!: RateSheetCustomAddEditComponent;

  @Input() RateSheets: RateSheetData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<RateSheetQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<RateSheetData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<RateSheetData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<RateSheetData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];

  /*
    //
    // Default column set — can be overridden completely by parent to control all aspects of the table shown.  This default set is all fields, with no width specified.  To force that, set width to an acceptable width value, for example '100px'.
    //
    { key: 'scope', label: 'Scope', width: '180px', template: 'default' },
    { key: 'office.name', label: 'Office', width: undefined, template: 'link', linkPath: ['/office', 'officeId'] },
    { key: 'assignmentRole.name', label: 'Role', width: undefined, template: 'link', linkPath: ['/assignmentrole', 'assignmentRoleId'] },
    { key: 'resource.name', label: 'Resource', width: undefined, template: 'link', linkPath: ['/resource', 'resourceId'] },
    { key: 'schedulingTarget.name', label: 'Target', width: undefined, template: 'link', linkPath: ['/schedulingtarget', 'schedulingTargetId'] },
    { key: 'rateType.name', label: 'Rate Type', width: undefined, template: 'link', linkPath: ['/ratetype', 'rateTypeId'] },
    { key: 'effectiveDate', label: 'Effective Date', width: undefined, template: 'date' },
    { key: 'currency.name', label: 'Currency', width: undefined, template: 'link', linkPath: ['/currency', 'currencyId'] },
    { key: 'costRate', label: 'Cost', width: undefined },
    { key: 'billingRate', label: 'Billing ', width: undefined },
    { key: 'notes', label: 'Notes', width: "100px" },
    //{ key: 'versionNumber', label: 'Version Number', width: undefined },
    //{ key: 'active', label: 'Active', width: '120px', template: 'boolean' },
    //{ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' },
  ];
  */

  public filteredRateSheets: RateSheetData[] | null = null;        // Stores the filtered/sorted data

  // Sorting properties
  public sortColumn: string | null = null;
  public sortDirection: 'asc' | 'desc' = 'asc';

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  private isManagingData: boolean = false; // Tracks if component is managing data loading

  private debounceTimeout: any;

  private destroy$ = new Subject<void>();

  constructor(private rateSheetService: RateSheetService,
    private authService: AuthService,
    private alertService: AlertService,
    private schedulerHelperService: SchedulerHelperService,
    private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.RateSheets) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the rateSheetChanged observable on the add/edit component so that when a RateSheet changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditRateSheetComponent && !this.disableDefaultEdit) {
        this.addEditRateSheetComponent.rateSheetChanged.subscribe({
        next: (result: RateSheetData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Rate Sheet changed notification", JSON.stringify(err), MessageSeverity.error);
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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  /**
  * Construct the default column array based on user entitlements.
  */
  private buildDefaultColumns(): void {

    // Start with a reasonable default — assume office column should be shown
    // We'll override if count comes back as 0
    let showOfficeColumn = true;


    // Load office count to determine if the Office column should be visible
    this.schedulerHelperService.ActiveOfficeCount$.pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (count: bigint | number) => {
        const countNumber: number = Number(count ?? 0);

        // Only hide the office column if there are literally zero active offices
        showOfficeColumn = countNumber > 0;

        // Now build the columns with the correct visibility
        this.buildColumnsWithOfficeVisibility(showOfficeColumn);
      },
      error: (err) => {
        // On error, be conservative: show the column (user can still filter manually)
        console.warn('Failed to load office count for column visibility — showing Office column by default', err);
        this.buildColumnsWithOfficeVisibility(true);
      }
    });
  }

  private buildColumnsWithOfficeVisibility(showOfficeColumn: boolean): void {

    const defaultColumns: TableColumn[] = [
      { key: 'scope', label: 'Scope', width: '180px', template: 'default' },
    ];

    // Conditionally add the Office column
    if (showOfficeColumn) {
      defaultColumns.push({
        key: 'office.name',
        label: 'Office',
        width: undefined,
        template: 'link',
        linkPath: ['/office', 'officeId']
      });
    }

    // Always-visible columns
    defaultColumns.push(
      { key: 'assignmentRole.name', label: 'Role', width: undefined, template: 'link', linkPath: ['/assignmentrole', 'assignmentRoleId'] },
      { key: 'resource.name', label: 'Resource', width: undefined, template: 'link', linkPath: ['/resource', 'resourceId'] },
      { key: 'schedulingTarget.name', label: 'Target', width: undefined, template: 'link', linkPath: ['/schedulingtarget', 'schedulingTargetId'] },
      { key: 'rateType.name', label: 'Rate Type', width: undefined, template: 'link', linkPath: ['/ratetype', 'rateTypeId'] },
      { key: 'effectiveDate', label: 'Effective Date', width: undefined, template: 'date' },
      { key: 'currency.name', label: 'Currency', width: undefined, template: 'link', linkPath: ['/currency', 'currencyId'] },
      { key: 'costRate', label: 'Cost', width: undefined },
      { key: 'billingRate', label: 'Billing ', width: undefined },
      { key: 'notes', label: 'Notes', width: "100px" },
    );

    // Permission-based columns
    const isWriter = this.rateSheetService.userIsSchedulerRateSheetWriter();
    const isAdmin = this.authService.isSchedulerAdministrator;

    if (isAdmin) {
      defaultColumns.push(
        { key: 'versionNumber', label: 'Version Number', width: undefined },
        { key: 'active', label: 'Active', width: '120px', template: 'boolean' },
        { key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' }
      );
    } else if (isWriter) {
      defaultColumns.push(
        { key: 'active', label: 'Active', width: '120px', template: 'boolean' }
      );
    }

    // Final assignment
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

    if (this.rateSheetService.userIsSchedulerRateSheetReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Rate Sheets", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const rateSheetQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.rateSheetService.GetRateSheetList(rateSheetQueryParams).subscribe({
      next: (RateSheetList) => {
        if (RateSheetList) {
          this.RateSheets = RateSheetList;
        } else {
          this.RateSheets = [];
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

         this.alertService.showMessage("Error getting Rate Sheet data", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


   private applyFiltersAndSort(): void {

    if (!this.RateSheets) {
      this.filteredRateSheets = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.RateSheets];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'assignmentRole.name',
                      'resource.name',
                      'schedulingTarget.name',
                      'rateType.name',
                      'effectiveDate',
                      'currency.name',
                      'costRate',
                      'billingRate',
        ];

        result = result.filter((rateSheet) =>

        filterFields.some((field) => {
        const value = getNestedValue(rateSheet, field);
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

    this.filteredRateSheets = result;
  }


  public handleEdit(rateSheet: RateSheetData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(rateSheet); // Let parent handle edit
    }
    else if (this.addEditRateSheetComponent)
    {
        this.addEditRateSheetComponent.openModal(rateSheet); // Default edit behavior
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


  public handleDelete(rateSheet: RateSheetData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(rateSheet); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete RateSheet', 'Are you sure you want to delete this Rate Sheet?')
          .then((result) => {
              if (result)
              {
                  this.deleteRateSheet(rateSheet);
              }
          })
          .catch(() => { });
    }
  }


  private deleteRateSheet(rateSheetData: RateSheetData): void {
    this.rateSheetService.DeleteRateSheet(rateSheetData.id).subscribe({
      next: () => {
       this.rateSheetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Rate Sheet", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(rateSheet: RateSheetData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(rateSheet); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete RateSheet', 'Are you sure you want to undelete this Rate Sheet?')
          .then((result) => {
              if (result)
              {
                  this.undeleteRateSheet(rateSheet);
              }
          })
          .catch(() => { });
    }
}


  private undeleteRateSheet(rateSheetData: RateSheetData): void {

      var rateSheetToSubmit = this.rateSheetService.ConvertToRateSheetSubmitData(rateSheetData); // Convert RateSheet data to post object for undeleting
      rateSheetToSubmit.deleted = false;

      this.rateSheetService.PutRateSheet(rateSheetToSubmit.id, rateSheetToSubmit).subscribe({
      next: () => {
       this.rateSheetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Rate Sheet", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getRateSheetId(index: number, rateSheet: any): number {
    return rateSheet.id;
  }


  public userIsSchedulerRateSheetReader(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetReader();
  }

  public userIsSchedulerRateSheetWriter(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/rateSheet', rateSheetId]
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

  /**
   * Calculates indentation level for content cells only (not action buttons)
   * 0 = Global
   * 1 = Role/Resource global or Project + Role/Resource
   * 2 = Project-specific with both Role and Resource (most specific)
   */
  public getContentIndentLevel(rateSheet: RateSheetData): number {
    if (!rateSheet) return 0;

    let level = 0;

    // Project-specific = base indent
    if (rateSheet.schedulingTargetId) {
      level = 1;
    }

    // Extra indent if both Role and Resource specified (highest specificity)
    if (rateSheet.assignmentRoleId && rateSheet.resourceId) {
      level += 1;
    }

    return level;
  }

  // Build the scope string for the custom first column
  public getScope(rateSheet: RateSheetData): string {
    if (!rateSheet) return '—';

    const hasTarget = !!rateSheet.schedulingTarget?.name;
    const hasRole = !!rateSheet.assignmentRole?.name;
    const hasResource = !!rateSheet.resource?.name;

    if (hasTarget && hasResource) {
      return `Target + Resource: ${rateSheet.schedulingTarget?.name}`;
    }
    if (hasTarget && hasRole) {
      return `Target + Role: ${rateSheet.schedulingTarget?.name}`;
    }
    if (hasResource) {
      return `Resource: ${rateSheet.resource?.name}`;
    }
    if (hasRole) {
      return `Role: ${rateSheet.assignmentRole?.name}`;
    }
    return 'Global (Invalid)';
  }
}

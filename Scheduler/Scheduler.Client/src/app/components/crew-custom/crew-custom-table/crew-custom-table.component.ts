import { Component, OnInit, AfterViewInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { CrewService, CrewData, CrewQueryParameters } from '../../../scheduler-data-services/crew.service';
import { CrewCustomAddEditComponent } from '../crew-custom-add-edit/crew-custom-add-edit.component';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-crew-custom-table',
  templateUrl: './crew-custom-table.component.html',
  styleUrls: ['./crew-custom-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CrewCustomTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild(CrewCustomAddEditComponent) addEditCrewComponent!: CrewCustomAddEditComponent;

  @Input() Crews: CrewData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null; // Optional filter text 
  @Input() queryParams: Partial<CrewQueryParameters> = { } // Optional query parameters

  @Input() disableDefaultEdit: boolean = false;         // Allow parent to disable default edit behavior
  @Input() disableDefaultDelete: boolean = false;       // Allow parent to disable default delete behavior
  @Input() disableDefaultUndelete: boolean = false; // Allow parent to disable default undelete behavior

  @Output() edit = new EventEmitter<CrewData>(); // Emitted for custom edit handling
  @Output() delete = new EventEmitter<CrewData>(); // Emitted for custom delete handling
  @Output() undelete = new EventEmitter<CrewData>(); // Emitted for custom undelete handling

  @Input() columns: TableColumn[] = [];     // Default set built in ngOnInit

  public filteredCrews: CrewData[] | null = null;        // Stores the filtered/sorted data

  // Sorting properties
  public sortColumn: string | null = null;
  public sortDirection: 'asc' | 'desc' = 'asc';


  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  private isManagingData: boolean = false; // Tracks if component is managing data loading

  private debounceTimeout: any;

  private destroy$ = new Subject<void>();

  constructor(private crewService: CrewService,
    private authService: AuthService,
    private alertService: AlertService,
    private officeService: OfficeService,
    private schedulerHelperService: SchedulerHelperService,
    private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    // If the parent has not provided custom columns, build the defaults with entitlement checks
    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.Crews) {

        this.isManagingData = true; // Component is managing data loading
        this.loadData(); // Load data on initialization

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the crewChanged observable on the add/edit component so that when a Crew changes we can reload the list, if component is available and not disabled..
    //
    if (this.addEditCrewComponent && !this.disableDefaultEdit) {
        this.addEditCrewComponent.crewChanged.subscribe({
        next: (result: CrewData[] | null) => {
            this.loadData();
        },
        error: (err: any) => {
             this.alertService.showMessage("Error during Crew changed notification", JSON.stringify(err), MessageSeverity.error);
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
      { key: 'name', label: 'Name', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/crew', 'id'] },
      { key: 'description', label: 'Description', width: undefined },
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
      { key: 'icon.name', label: 'Icon', width: undefined, template: 'link', linkPath: ['/icon', 'iconId'] },
      { key: 'color', label: 'Color', width: undefined, template: 'color' },

      // Don't show these
      //{ key: 'avatarFileName', label: 'Avatar File Name', width: undefined },
      //{ key: 'avatarSize', label: 'Avatar Size', width: undefined },
      //{ key: 'avatarData', label: 'Avatar Data', width: undefined },
      //{ key: 'avatarMimeType', label: 'Avatar Mime Type', width: undefined }
    );

    // Permission-based columns
    const isWriter = this.crewService.userIsSchedulerCrewWriter();
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

    if (this.crewService.userIsSchedulerCrewReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Crews", '', MessageSeverity.info);
      return;
    }

    //
    // Server side filtering using the any string contains parameter
    //
    const crewQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined
    };

    //
    // Note that we are not clearing the data service cache here.  Fresh data will be loaded if necessary, or cached data will be returned if no changes to it have been detected.
    //
    this.crewService.GetCrewList(crewQueryParams).subscribe({
      next: (CrewList) => {
        if (CrewList) {
          this.Crews = CrewList;
        } else {
          this.Crews = [];
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

         this.alertService.showMessage("Error getting Crew data", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


   private applyFiltersAndSort(): void {

    if (!this.Crews) {
      this.filteredCrews = null;
      return;
    }

    // Helper function to safely access nested properties used by sorting and filtering.
    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };


    let result = [...this.Crews];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        // Define fields to filter on, including nested properties
        const filterFields = [
                      'name',
                      'description',
                      'office.name',
                      'icon.name',
                      'color',
                      'avatarFileName',
                      'avatarSize',
                      'avatarData',
                      'avatarMimeType',
        ];

        result = result.filter((crew) =>

        filterFields.some((field) => {
        const value = getNestedValue(crew, field);
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

    this.filteredCrews = result;
  }


  public handleEdit(crew: CrewData): void {
    if (this.disableDefaultEdit)
    {
        this.edit.emit(crew); // Let parent handle edit
    }
    else if (this.addEditCrewComponent)
    {
        this.addEditCrewComponent.openModal(crew); // Default edit behavior
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


  public handleDelete(crew: CrewData): void {
    if (this.disableDefaultDelete)
    {
        this.delete.emit(crew); // Let parent handle delete
    }
    else
    {
        this.confirmationService
          .confirm('Delete Crew', 'Are you sure you want to delete this Crew?')
          .then((result) => {
              if (result)
              {
                  this.deleteCrew(crew);
              }
          })
          .catch(() => { });
    }
  }


  private deleteCrew(crewData: CrewData): void {
    this.crewService.DeleteCrew(crewData.id).subscribe({
      next: () => {
       this.crewService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Crew", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(crew: CrewData): void {
    if (this.disableDefaultUndelete)
    {
        this.undelete.emit(crew); // Let parent handle undelete
    }
    else
    {
        this.confirmationService
          .confirm('Undelete Crew', 'Are you sure you want to undelete this Crew?')
          .then((result) => {
              if (result)
              {
                  this.undeleteCrew(crew);
              }
          })
          .catch(() => { });
    }
}


  private undeleteCrew(crewData: CrewData): void {

      var crewToSubmit = this.crewService.ConvertToCrewSubmitData(crewData); // Convert Crew data to post object for undeleting
      crewToSubmit.deleted = false;

      this.crewService.PutCrew(crewToSubmit.id, crewToSubmit).subscribe({
      next: () => {
       this.crewService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.
        this.loadData(); // Reload the data list after un-deletion
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Crew", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getCrewId(index: number, crew: any): number {
    return crew.id;
  }


  public userIsSchedulerCrewReader(): boolean {
    return this.crewService.userIsSchedulerCrewReader();
  }

  public userIsSchedulerCrewWriter(): boolean {
    return this.crewService.userIsSchedulerCrewWriter();
  }




  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/crew', crewId]
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

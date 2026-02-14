import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeService, OfficeData, OfficeSubmitData } from '../../../scheduler-data-services/office.service';
import { AuthService } from '../../../services/auth.service';
import { Observable, BehaviorSubject, Subject, takeUntil, of, combineLatest, shareReplay } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { OfficeCustomAddEditComponent } from '../office-custom-add-edit/office-custom-add-edit.component';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { OfficeContactService } from '../../../scheduler-data-services/office-contact.service';


@Component({
  selector: 'app-office-custom-detail',
  templateUrl: './office-custom-detail.component.html',
  styleUrls: ['./office-custom-detail.component.scss']
})

export class OfficeCustomDetailComponent implements OnInit {

  @ViewChild(OfficeCustomAddEditComponent) addEditComponent!: OfficeCustomAddEditComponent;

  public officeId: string | null = null;
  public office: OfficeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  // Custom row count observable that we can't get directly from the object because we do some math on it.
  public assignmentCount$: Observable<bigint | number> = of(0);

  public error: string | null = null;
  public activeTab = 'overview';

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  // Change history
  public auditHistory: any[] | null = null;
  public isLoadingHistory = false;

  private destroy$ = new Subject<void>();

  constructor(
    public officeService: OfficeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private alertService: AlertService,
    private navigationService: NavigationService,
    private crewService: CrewService,
    private officeContactService: OfficeContactService,
    private resourceService: ResourceService,
    private calendarService: CalendarService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private rateSheetService: RateSheetService,
    private router: Router,
    private scheduledEventService: ScheduledEventService) {

  }

  ngOnInit(): void {

    // Get the officeId from the route parameters
    this.officeId = this.route.snapshot.paramMap.get('officeId');

    // Handle tab state from query params
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
      }
    });

    if (this.officeId === 'new' ||
      this.officeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.office = null;

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Office';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Office';

      // Load the data from the server
      this.loadData(false);
    }
  }


  ngAfterViewInit(): void {

    //
    // Open the add/edit modal in add mode after the view is initialized so we have the reference properly
    //
    if (this.isEditMode == false) {
      this.addEditComponent.openModal();      // Open add modal
    }

    //
    // Subscribe to the observable on the add/edit component so that when there are changes we can reload the list.
    //
    this.addEditComponent.officeChanged.subscribe({
      next: (result: OfficeData[] | null) => {
        this.loadData();

      },
      error: (err: any) => {
        this.alertService.showMessage("Error during Contact changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });

  }


  public onTabChange(event: any) {
    this.activeTab = event.nextId;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
    if (this.activeTab === 'history') this.loadHistory();
  }

  public loadHistory(): void {
    if (this.auditHistory != null || !this.office) return;
    this.isLoadingHistory = true;
    this.officeService.GetOfficeAuditHistory(this.office.id as number, true).subscribe({
      next: (data) => { this.auditHistory = data || []; this.isLoadingHistory = false; },
      error: () => { this.auditHistory = []; this.isLoadingHistory = false; }
    });
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public GetQueryParameters(): any {

    if (this.officeId != null && this.officeId !== 'new') {

      const id = parseInt(this.officeId, 10);

      if (!isNaN(id)) {
        return { officeId: id };
      }
    }

    return null;
  }


  /*
    * Loads the Office data for the current officeId.
    *
    * Fully respects the OfficeService caching strategy and error handling strategy.
    *
    * @param forceLoadAndDisplaySuccessAlert
    *   - true  will bypass cache entirely and show success alert message
    *   - false/null will use cache if available, no alert message
    */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.officeService.userIsSchedulerOfficeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Offices.`,
        'Access Denied',
        MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate officeId
    //
    if (!this.officeId) {

      this.alertService.showMessage('No Office ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const officeId = Number(this.officeId);

    if (isNaN(officeId) || officeId <= 0) {

      this.alertService.showMessage(`Invalid Office ID: "${this.officeId}"`,
        'Invalid ID',
        MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this Office + relations

      this.officeService.ClearRecordCache(officeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.officeService.GetOffice(officeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (officeData) => {

        //
        // Success path — officeData can legitimately be null if 404'd but request succeeded
        //
        if (!officeData) {

          this.handleOfficeNotFound(officeId);

        } else {

          this.loadOfficeComplete(officeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Office loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleOfficeLoadError(error, officeId);
        this.isLoadingSubject.next(false);
      }
    });
  }

  private loadOfficeComplete(office: OfficeData): void {
    this.office = office;

    this.refreshRowCountObservables();
  }


  /**
   * Refreshes all row count observables used for tab badges.
   * 
   * Called after loading the resource and after any mutation that could change counts.
   */
  public refreshRowCountObservables() {

    if (this.office == null) {
      return;
    }

    //
    // Revive the office object from itself to refresh all the observables.
    //
    this.office = this.officeService.ReviveOffice(this.office);


    //
    // Assign fresh row count osbservables. - Only get counts for active and deleted rows to use in the banners.
    //
    const officeId = this.office.id;


    // === COMBINED ASSIGNMENT COUNT ===
    // We need the sum of:
    // 1. Direct assignments via EventResourceAssignment (most common)
    // 2. Events where this resource is the primary/lead (ScheduledEvent.resourceId)
    //
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      officeId: officeId,
      active: true,
      deleted: false
    });

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      officeId: officeId,
      active: true,
      deleted: false
    });

    // Combine both counts and sum them
    this.assignmentCount$ = combineLatest([
      directAssignments$,
      leadEvents$
    ]).pipe(
      map(([directCount, leadCount]) => {
        // Convert bigint | number to number for addition (safe for UI counts)
        const direct = Number(directCount ?? 0);
        const lead = Number(leadCount ?? 0);
        return direct + lead;
      }),
      // Start with 0 while loading
      startWith(0),
      // Share the result so multiple template subscriptions don't re-trigger
      shareReplay(1)
    );

  }



  private handleOfficeNotFound(officeId: number): void {

    this.office = null;

    this.alertService.showMessage(
      `Office #${officeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleOfficeLoadError(error: any, officeId: number): void {

    let message = 'Failed to load Office.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Office.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Office #${officeId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Office load failed (ID: ${officeId})`, error);

    //
    // Reset UI to safe state
    //
    this.office = null;

    this.alertService.showMessage(message, title, severity);
  }


  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public officeChanged(officeData: OfficeData[]) {

    officeData[0].Reload().then(o => {
      this.office = o;
    });
  }


  openEditModal(): void {
    if (this.office) {
      this.addEditComponent.openModal(this.office);
    }
  }


  public userIsSchedulerOfficeReader(): boolean {
    return this.officeService.userIsSchedulerOfficeReader();
  }

  public userIsSchedulerOfficeWriter(): boolean {
    return this.officeService.userIsSchedulerOfficeWriter();
  }
}

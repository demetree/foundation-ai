import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CalendarService, CalendarData, CalendarSubmitData } from '../../../scheduler-data-services/calendar.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { AuthService } from '../../../services/auth.service';
import { Observable, BehaviorSubject, Subject, takeUntil, combineLatest, shareReplay } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { CalendarCustomAddEditComponent } from '../calendar-custom-add-edit/calendar-custom-add-edit.component';

@Component({
  selector: 'app-calendar-custom-detail',
  templateUrl: './calendar-custom-detail.component.html',
  styleUrls: ['./calendar-custom-detail.component.scss']
})

export class CalendarCustomDetailComponent implements OnInit {

  @ViewChild(CalendarCustomAddEditComponent) addEditComponent!: CalendarCustomAddEditComponent;

  public calendarId: string | null = null;
  public calendar: CalendarData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public error: string | null = null;
  public activeTab = 'overview';

  public CalendarAssignmentCount$: Observable<bigint | number> | null = null;


  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  private destroy$ = new Subject<void>();

  constructor(
    public calendarService: CalendarService,
    private authService: AuthService,
    private scheduledEventService: ScheduledEventService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private route: ActivatedRoute,
    private router: Router,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the calendarId from the route parameters
    this.calendarId = this.route.snapshot.paramMap.get('calendarId');

    if (this.calendarId === 'new' ||
        this.calendarId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.calendar = null;

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Calendar';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Calendar';

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
    this.addEditComponent.calendarChanged.subscribe({
      next: (result: CalendarData[] | null) => {
        this.loadData();

      },
      error: (err: any) => {
        this.alertService.showMessage("Error during Contact changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


 public GetQueryParameters(): any {

    if (this.calendarId != null && this.calendarId !== 'new') {

      const id = parseInt(this.calendarId, 10);

      if (!isNaN(id)) {
        return { calendarId: id };
      }
    }

    return null;
  }


/*
  * Loads the Calendar data for the current calendarId.
  *
  * Fully respects the CalendarService caching strategy and error handling strategy.
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
    if (!this.calendarService.userIsSchedulerCalendarReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Calendars.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.error = `${userName} does not have permission to read Calendars.`;

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate calendarId
    //
    if (!this.calendarId) {

      this.alertService.showMessage('No Calendar ID provided.', 'Missing ID', MessageSeverity.error);

      this.error = 'No Calendar ID provided.', 'Missing ID';

      this.isLoadingSubject.next(false);

      return;
    }

    const calendarId = Number(this.calendarId);

    if (isNaN(calendarId) || calendarId <= 0) {

      this.alertService.showMessage(`Invalid Calendar ID: "${this.calendarId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.error = `Invalid Calendar ID: "${this.calendarId}"`;

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this Calendar + relations

      this.calendarService.ClearRecordCache(calendarId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.calendarService.GetCalendar(calendarId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (calendarData) => {

        //
        // Success path — CalendarData can legitimately be null if 404'd but request succeeded
        //
        if (!calendarData) {

          this.handleCalendarNotFound(calendarId);

        } else {

          this.loadCalendarComplete(calendarData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Calendar loaded successfully',
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
        this.handleCalendarLoadError(error, calendarId);
        this.isLoadingSubject.next(false);

        this.error = error?.error?.details;
      }
    });
  }


  private loadCalendarComplete(calendar: CalendarData): void {

    this.calendar = calendar;

    this.refreshRowCountObservables();
  }


  /**
   * Refreshes all row count observables used for tab badges.
   * 
   * Called after loading the calendar and after any mutation that could change counts.
   */
  public refreshRowCountObservables() {

    if (this.calendar == null) {
      return;
    }

    //
    // Assign fresh row count osbservables. - Only get counts for active and deleted rows to use in the banners.
    //
    const calendarId = this.calendar.id;

    //// Individual row count observables (active only, non-deleted)
    //this.CalendarMemberCount$ = this.calendarMemberService.GetCalendarMembersRowCount({
    //  calendarId: calendarId,
    //  active: true,
    //  deleted: false
    //});

    // === COMBINED ASSIGNMENT COUNT ===
    // We need the sum of:
    // 1. Direct assignments via EventResourceAssignment (most common)
    // 2. Events where this resource is the primary/lead (ScheduledEvent.resourceId)
    //
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      calendarId: calendarId,
      active: true,
      deleted: false
    });

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      calendarId: calendarId,
      active: true,
      deleted: false
    });

    // Combine both counts and sum them
    this.CalendarAssignmentCount$ = combineLatest([
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

  private handleCalendarNotFound(calendarId: number): void {

    this.calendar = null;

    this.alertService.showMessage(
      `Calendar #${calendarId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCalendarLoadError(error: any, calendarId: number): void {

    let message = 'Failed to load Calendar.';
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
          message = 'You do not have permission to view this Calendar.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Calendar #${calendarId} was not found.`;
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

    console.error(`Calendar load failed (ID: ${calendarId})`, error);

    //
    // Reset UI to safe state
    //
    this.calendar = null;

    this.alertService.showMessage(message, title, severity);
  }


  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public calendarChanged(calendarData: CalendarData[]) {

    calendarData[0].Reload().then(rd => {
      this.calendar = rd;
    });
  }


  openEditModal(): void {
    if (this.calendar) {
      this.addEditComponent.openModal(this.calendar);
    }
  }


  public navigateToOffice(officeId: number | bigint | null | undefined): void {

    //
    // This routes to the offce details page.
    //
    if (officeId) {
      this.router.navigate(['/office', officeId]);
    }
  }


  public userIsSchedulerCalendarReader(): boolean {
    return this.calendarService.userIsSchedulerCalendarReader();
  }

  public userIsSchedulerCalendarWriter(): boolean {
    return this.calendarService.userIsSchedulerCalendarWriter();
  }
}

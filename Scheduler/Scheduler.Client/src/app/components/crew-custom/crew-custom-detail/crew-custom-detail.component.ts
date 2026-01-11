import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CrewService, CrewData, CrewSubmitData } from '../../../scheduler-data-services/crew.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { AuthService } from '../../../services/auth.service';
import { Observable, BehaviorSubject, Subject, takeUntil, combineLatest, shareReplay } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { CrewCustomAddEditComponent } from '../crew-custom-add-edit/crew-custom-add-edit.component';

@Component({
  selector: 'app-crew-custom-detail',
  templateUrl: './crew-custom-detail.component.html',
  styleUrls: ['./crew-custom-detail.component.scss']
})

export class CrewCustomDetailComponent implements OnInit {

  @ViewChild(CrewCustomAddEditComponent) addEditComponent!: CrewCustomAddEditComponent;

  public crewId: string | null = null;
  public crew: CrewData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public error: string | null = null;
  public activeTab = 'overview';

  // Custom row count observable that we can't get directly from the object because we do some math on it.
  public CrewAssignmentCount$: Observable<bigint | number> | null = null;

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  private destroy$ = new Subject<void>();

  constructor(
    public crewService: CrewService,
    private authService: AuthService,
    private crewMemberService: CrewMemberService,
    private scheduledEventService: ScheduledEventService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private officeService: OfficeService,
    private schedulerHelperService: SchedulerHelperService,
    private route: ActivatedRoute,
    private router: Router,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the crewId from the route parameters
    this.crewId = this.route.snapshot.paramMap.get('crewId');

    if (this.crewId === 'new' ||
        this.crewId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.crew = null;

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Crew';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Crew';

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
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


 public GetQueryParameters(): any {

    if (this.crewId != null && this.crewId !== 'new') {

      const id = parseInt(this.crewId, 10);

      if (!isNaN(id)) {
        return { crewId: id };
      }
    }

    return null;
  }


/*
  * Loads the Crew data for the current crewId.
  *
  * Fully respects the CrewService caching strategy and error handling strategy.
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
    if (!this.crewService.userIsSchedulerCrewReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Crews.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.error = `${userName} does not have permission to read Crews.`;

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate crewId
    //
    if (!this.crewId) {

      this.alertService.showMessage('No Crew ID provided.', 'Missing ID', MessageSeverity.error);

      this.error = 'No Crew ID provided.', 'Missing ID';

      this.isLoadingSubject.next(false);

      return;
    }

    const crewId = Number(this.crewId);

    if (isNaN(crewId) || crewId <= 0) {

      this.alertService.showMessage(`Invalid Crew ID: "${this.crewId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.error = `Invalid Crew ID: "${this.crewId}"`;

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this Crew + relations

      this.crewService.ClearRecordCache(crewId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.crewService.GetCrew(crewId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (crewData) => {

        //
        // Success path — CrewData can legitimately be null if 404'd but request succeeded
        //
        if (!crewData) {

          this.handleCrewNotFound(crewId);

        } else {

          this.loadCrewComplete(crewData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Crew loaded successfully',
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
        this.handleCrewLoadError(error, crewId);
        this.isLoadingSubject.next(false);

        this.error = error?.error?.details;
      }
    });
  }


  private loadCrewComplete(crew: CrewData): void {

    this.crew = crew;

    this.refreshRowCountObservables();
  }


  /**
   * Refreshes all row count observables used for tab badges.
   * 
   * Called after loading the crew and after any mutation that could change counts.
   */
  public refreshRowCountObservables() {

    if (this.crew == null) {
      return;
    }

    //
    // Revive the crew object from itself to refresh all the observables.
    //
    this.crew = this.crewService.ReviveCrew(this.crew);


    //
    // Assign fresh row count osbservables. - Only get counts for active and deleted rows to use in the banners.
    //
    const crewId = this.crew.id;

    // === COMBINED ASSIGNMENT COUNT ===
    // We need the sum of:
    // 1. Direct assignments via EventResourceAssignment (most common)
    // 2. Events where this resource is the primary/lead (ScheduledEvent.resourceId)
    //
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      crewId: crewId,
      active: true,
      deleted: false
    });

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      crewId: crewId,
      active: true,
      deleted: false
    });

    // Combine both counts and sum them
    this.CrewAssignmentCount$ = combineLatest([
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

  private handleCrewNotFound(crewId: number): void {

    this.crew = null;

    this.alertService.showMessage(
      `Crew #${crewId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCrewLoadError(error: any, crewId: number): void {

    let message = 'Failed to load Crew.';
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
          message = 'You do not have permission to view this Crew.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Crew #${crewId} was not found.`;
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

    console.error(`Crew load failed (ID: ${crewId})`, error);

    //
    // Reset UI to safe state
    //
    this.crew = null;

    this.alertService.showMessage(message, title, severity);
  }


  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public crewChanged(crewData: CrewData[]) {

    crewData[0].Reload().then(rd => {
      this.crew = rd;
    });
  }


  openEditModal(): void {
    if (this.crew) {
      this.addEditComponent.openModal(this.crew);
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


  public userIsSchedulerCrewReader(): boolean {
    return this.crewService.userIsSchedulerCrewReader();
  }

  public userIsSchedulerCrewWriter(): boolean {
    return this.crewService.userIsSchedulerCrewWriter();
  }
}

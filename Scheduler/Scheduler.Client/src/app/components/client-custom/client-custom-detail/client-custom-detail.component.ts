import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ClientService, ClientData, ClientSubmitData } from '../../../scheduler-data-services/client.service';
import { AuthService } from '../../../services/auth.service';
import { Observable, BehaviorSubject, Subject, takeUntil, of, combineLatest, shareReplay } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { ClientCustomAddEditComponent } from '../client-custom-add-edit/client-custom-add-edit.component';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { ClientContactService } from '../../../scheduler-data-services/client-contact.service';


@Component({
  selector: 'app-client-custom-detail',
  templateUrl: './client-custom-detail.component.html',
  styleUrls: ['./client-custom-detail.component.scss']
})

export class ClientCustomDetailComponent implements OnInit {

  @ViewChild(ClientCustomAddEditComponent) addEditComponent!: ClientCustomAddEditComponent;

  public clientId: string | null = null;
  public client: ClientData | null = null;

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
    public clientService: ClientService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private alertService: AlertService,
    private navigationService: NavigationService,
    private crewService: CrewService,
    private clientContactService: ClientContactService,
    private resourceService: ResourceService,
    private calendarService: CalendarService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private rateSheetService: RateSheetService,
    private router: Router,
    private scheduledEventService: ScheduledEventService) {

  }

  ngOnInit(): void {

    // Get the clientId from the route parameters
    this.clientId = this.route.snapshot.paramMap.get('clientId');

    // Handle tab state from query params
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
      }
    });

    if (this.clientId === 'new' ||
      this.clientId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.client = null;

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Client';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Client';

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
    this.addEditComponent.clientChanged.subscribe({
      next: (result: ClientData[] | null) => {
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
    if (this.auditHistory != null || !this.client) return;
    this.isLoadingHistory = true;
    this.clientService.GetClientAuditHistory(this.client.id as number, true).subscribe({
      next: (data) => { this.auditHistory = data || []; this.isLoadingHistory = false; },
      error: () => { this.auditHistory = []; this.isLoadingHistory = false; }
    });
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public GetQueryParameters(): any {

    if (this.clientId != null && this.clientId !== 'new') {

      const id = parseInt(this.clientId, 10);

      if (!isNaN(id)) {
        return { clientId: id };
      }
    }

    return null;
  }


  /*
    * Loads the Client data for the current clientId.
    *
    * Fully respects the ClientService caching strategy and error handling strategy.
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
    if (!this.clientService.userIsSchedulerClientReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Clients.`,
        'Access Denied',
        MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate clientId
    //
    if (!this.clientId) {

      this.alertService.showMessage('No Client ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const clientId = Number(this.clientId);

    if (isNaN(clientId) || clientId <= 0) {

      this.alertService.showMessage(`Invalid Client ID: "${this.clientId}"`,
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
      // This is the most targeted way: clear only this Client + relations

      this.clientService.ClearRecordCache(clientId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.clientService.GetClient(clientId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (clientData) => {

        //
        // Success path — clientData can legitimately be null if 404'd but request succeeded
        //
        if (!clientData) {

          this.handleClientNotFound(clientId);

        } else {

          this.loadClientComplete(clientData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Client loaded successfully',
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
        this.handleClientLoadError(error, clientId);
        this.isLoadingSubject.next(false);
      }
    });
  }

  private loadClientComplete(client: ClientData): void {
    this.client = client;

    this.refreshRowCountObservables();
  }


  /**
   * Refreshes all row count observables used for tab badges.
   * 
   * Called after loading the resource and after any mutation that could change counts.
   */
  public refreshRowCountObservables() {

    if (this.client == null) {
      return;
    }

    //
    // Revive the client object from itself to refresh all the observables.
    //
    this.client = this.clientService.ReviveClient(this.client);


    //
    // Assign fresh row count osbservables. - Only get counts for active and deleted rows to use in the banners.
    //
    const clientId = this.client.id;


    // === COMBINED ASSIGNMENT COUNT ===
    // We need the sum of:
    // 1. Direct assignments via EventResourceAssignment (most common)
    // 2. Events where this resource is the primary/lead (ScheduledEvent.resourceId)
    //
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      clientId: clientId,
      active: true,
      deleted: false
    });

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      clientId: clientId,
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



  private handleClientNotFound(clientId: number): void {

    this.client = null;

    this.alertService.showMessage(
      `Client #${clientId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleClientLoadError(error: any, clientId: number): void {

    let message = 'Failed to load Client.';
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
          message = 'You do not have permission to view this Client.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Client #${clientId} was not found.`;
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

    console.error(`Client load failed (ID: ${clientId})`, error);

    //
    // Reset UI to safe state
    //
    this.client = null;

    this.alertService.showMessage(message, title, severity);
  }


  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public clientChanged(clientData: ClientData[]) {

    clientData[0].Reload().then(o => {
      this.client = o;
    });
  }


  openEditModal(): void {
    if (this.client) {
      this.addEditComponent.openModal(this.client);
    }
  }


  public userIsSchedulerClientReader(): boolean {
    return this.clientService.userIsSchedulerClientReader();
  }

  public userIsSchedulerClientWriter(): boolean {
    return this.clientService.userIsSchedulerClientWriter();
  }
}

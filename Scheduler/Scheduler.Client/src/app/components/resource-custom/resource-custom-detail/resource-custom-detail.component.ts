import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { ResourceQualificationService } from '../../../scheduler-data-services/resource-qualification.service';
import { NotificationSubscriptionService } from '../../../scheduler-data-services/notification-subscription.service';
import { ResourceContactService } from '../../../scheduler-data-services/resource-contact.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { ResourceAvailabilityService } from '../../../scheduler-data-services/resource-availability.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
import { AuthService } from '../../../services/auth.service';
import { Observable, BehaviorSubject, Subject, takeUntil, combineLatest, shareReplay } from 'rxjs';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { map, startWith } from 'rxjs/operators';
import { ResourceCustomAddEditComponent } from '../resource-custom-add-edit/resource-custom-add-edit.component';

@Component({
  selector: 'app-resource-custom-detail',
  templateUrl: './resource-custom-detail.component.html',
  styleUrls: ['./resource-custom-detail.component.scss']
})

export class ResourceCustomDetailComponent implements OnInit {

  @ViewChild(ResourceCustomAddEditComponent) addEditComponent!: ResourceCustomAddEditComponent;


  public resourceId: string | null = null;
  public resource: ResourceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  public ResourceAssignmentCount$: Observable<bigint | number> | null = null;
  public DocumentCount$: Observable<bigint | number> | null = null;

  public error: string | null = null;
  public activeTab = 'overview';

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  // Change history
  public auditHistory: any[] | null = null;
  public isLoadingHistory = false;

  private destroy$ = new Subject<void>();

  constructor(
    public resourceService: ResourceService,
    private crewMemberService: CrewMemberService,
    private resourceQualificationService: ResourceQualificationService,
    private notificationSubscriptionService: NotificationSubscriptionService,
    private resourceContactService: ResourceContactService,
    private rateSheetService: RateSheetService,
    private resourceAvailabilityService: ResourceAvailabilityService,
    private scheduledEventService: ScheduledEventService,
    private eventResourceAssignmentService: EventResourceAssignmentService,
    private documentService: DocumentService,
    private schedulerHelperService: SchedulerHelperService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private alertService: AlertService,
    private navigationService: NavigationService) {

  }

  ngOnInit(): void {

    // Get the resourceId from the route parameters
    this.resourceId = this.route.snapshot.paramMap.get('resourceId');

    // Handle tab state from query params
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
      }
    });

    if (this.resourceId === 'new' ||
      this.resourceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;

      this.resource = null;

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Resource';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Resource';

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
    this.addEditComponent.resourceChanged.subscribe({
      next: (result: ResourceData[] | null) => {
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
    if (this.auditHistory != null || !this.resource) return;
    this.isLoadingHistory = true;
    this.resourceService.GetResourceAuditHistory(this.resource.id as number, true).subscribe({
      next: (data) => { this.auditHistory = data || []; this.isLoadingHistory = false; },
      error: () => { this.auditHistory = []; this.isLoadingHistory = false; }
    });
  }



  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public GetQueryParameters(): any {

    if (this.resourceId != null && this.resourceId !== 'new') {

      const id = parseInt(this.resourceId, 10);

      if (!isNaN(id)) {
        return { resourceId: id };
      }
    }

    return null;
  }


  /*
    * Loads the Resource data for the current resourceId.
    *
    * Fully respects the ResourceService caching strategy and error handling strategy.
    *
    * @param forceLoadAndDisplaySuccessAlert
    *   - true  will bypass cache entirely and show success alert message
    *   - false/null will use cache if available, no alert message
    */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    // Invalidate history cache so it re-fetches after edits
    this.auditHistory = null;

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.resourceService.userIsSchedulerResourceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Resources.`,
        'Access Denied',
        MessageSeverity.warn
      );

      this.error = `${userName} does not have permission to read Resources.`;

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate resourceId
    //
    if (!this.resourceId) {

      this.alertService.showMessage('No Resource ID provided.', 'Missing ID', MessageSeverity.error);

      this.error = 'No Resource ID provided.', 'Missing ID';

      this.isLoadingSubject.next(false);

      return;
    }

    const resourceId = Number(this.resourceId);

    if (isNaN(resourceId) || resourceId <= 0) {

      this.alertService.showMessage(`Invalid Resource ID: "${this.resourceId}"`,
        'Invalid ID',
        MessageSeverity.error
      );

      this.error = `Invalid Resource ID: "${this.resourceId}"`;

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this Resource + relations

      this.resourceService.ClearRecordCache(resourceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.resourceService.GetResource(resourceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (resourceData) => {

        //
        // Success path — resourceData can legitimately be null if 404'd but request succeeded
        //
        if (!resourceData) {

          this.handleResourceNotFound(resourceId);

        } else {

          this.loadResourceComplete(resourceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Resource loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);

        // If navigated directly to the history tab, trigger load
        if (this.activeTab === 'history') this.loadHistory();
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleResourceLoadError(error, resourceId);
        this.isLoadingSubject.next(false);

        this.error = error?.error?.details;
      }
    });
  }


  private loadResourceComplete(resource: ResourceData): void {

    this.resource = resource;

    this.refreshRowCountObservables();
  }


  /**
   * Refreshes all row count observables used for tab badges.
   * 
   * Called after loading the resource and after any mutation that could change counts.
   */
  public refreshRowCountObservables() {

    if (this.resource == null) {
      return;
    }

    //
    // Revive the resource object from itself to refresh all the observables.
    //
    this.resource = this.resourceService.ReviveResource(this.resource);


    //
    // Assign fresh row count osbservables. - Only get counts for active and deleted rows to use in the banners.
    //
    const resourceId = this.resource.id;

    // === COMBINED ASSIGNMENT COUNT ===
    // We need the sum of:
    // 1. Direct assignments via EventResourceAssignment (most common)
    // 2. Events where this resource is the primary/lead (ScheduledEvent.resourceId)
    //
    const directAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    });

    const leadEvents$ = this.scheduledEventService.GetScheduledEventsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    });

    // Combine both counts and sum them
    this.DocumentCount$ = this.documentService.GetDocumentsRowCount({
      resourceId: resourceId,
      active: true,
      deleted: false
    });

    this.ResourceAssignmentCount$ = combineLatest([
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


  private handleResourceNotFound(resourceId: number): void {

    this.resource = null;

    this.alertService.showMessage(
      `Resource #${resourceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleResourceLoadError(error: any, resourceId: number): void {

    let message = 'Failed to load Resource.';
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
          message = 'You do not have permission to view this Resource.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Resource #${resourceId} was not found.`;
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

    console.error(`Resource load failed (ID: ${resourceId})`, error);

    //
    // Reset UI to safe state
    //
    this.resource = null;

    this.alertService.showMessage(message, title, severity);
  }


  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public resourceChanged(resourceData: ResourceData[]) {

    resourceData[0].Reload().then(rd => {
      this.resource = rd;
    });
  }


  openEditModal(): void {
    if (this.resource) {
      this.addEditComponent.openModal(this.resource);
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


  public userIsSchedulerResourceReader(): boolean {
    return this.resourceService.userIsSchedulerResourceReader();
  }

  public userIsSchedulerResourceWriter(): boolean {
    return this.resourceService.userIsSchedulerResourceWriter();
  }
}

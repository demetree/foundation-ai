/*
   GENERATED FORM FOR THE INCIDENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Incident table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentService, IncidentData, IncidentSubmitData } from '../../../alerting-data-services/incident.service';
import { ServiceService } from '../../../alerting-data-services/service.service';
import { SeverityTypeService } from '../../../alerting-data-services/severity-type.service';
import { IncidentStatusTypeService } from '../../../alerting-data-services/incident-status-type.service';
import { EscalationRuleService } from '../../../alerting-data-services/escalation-rule.service';
import { IncidentChangeHistoryService } from '../../../alerting-data-services/incident-change-history.service';
import { IncidentTimelineEventService } from '../../../alerting-data-services/incident-timeline-event.service';
import { IncidentNoteService } from '../../../alerting-data-services/incident-note.service';
import { IncidentNotificationService } from '../../../alerting-data-services/incident-notification.service';
import { WebhookDeliveryAttemptService } from '../../../alerting-data-services/webhook-delivery-attempt.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IncidentFormValues {
  incidentKey: string,
  serviceId: number | bigint,       // For FK link number
  title: string,
  description: string | null,
  severityTypeId: number | bigint,       // For FK link number
  incidentStatusTypeId: number | bigint,       // For FK link number
  createdAt: string,
  escalationRuleId: number | bigint | null,       // For FK link number
  currentRepeatCount: string | null,     // Stored as string for form input, converted to number on submit.
  nextEscalationAt: string | null,
  acknowledgedAt: string | null,
  resolvedAt: string | null,
  currentAssigneeObjectGuid: string | null,
  sourcePayloadJson: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-incident-detail',
  templateUrl: './incident-detail.component.html',
  styleUrls: ['./incident-detail.component.scss']
})

export class IncidentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentForm: FormGroup = this.fb.group({
        incidentKey: ['', Validators.required],
        serviceId: [null, Validators.required],
        title: ['', Validators.required],
        description: [''],
        severityTypeId: [null, Validators.required],
        incidentStatusTypeId: [null, Validators.required],
        createdAt: ['', Validators.required],
        escalationRuleId: [null],
        currentRepeatCount: [''],
        nextEscalationAt: [''],
        acknowledgedAt: [''],
        resolvedAt: [''],
        currentAssigneeObjectGuid: [''],
        sourcePayloadJson: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public incidentId: string | null = null;
  public incidentData: IncidentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  incidents$ = this.incidentService.GetIncidentList();
  public services$ = this.serviceService.GetServiceList();
  public severityTypes$ = this.severityTypeService.GetSeverityTypeList();
  public incidentStatusTypes$ = this.incidentStatusTypeService.GetIncidentStatusTypeList();
  public escalationRules$ = this.escalationRuleService.GetEscalationRuleList();
  public incidentChangeHistories$ = this.incidentChangeHistoryService.GetIncidentChangeHistoryList();
  public incidentTimelineEvents$ = this.incidentTimelineEventService.GetIncidentTimelineEventList();
  public incidentNotes$ = this.incidentNoteService.GetIncidentNoteList();
  public incidentNotifications$ = this.incidentNotificationService.GetIncidentNotificationList();
  public webhookDeliveryAttempts$ = this.webhookDeliveryAttemptService.GetWebhookDeliveryAttemptList();

  private destroy$ = new Subject<void>();

  constructor(
    public incidentService: IncidentService,
    public serviceService: ServiceService,
    public severityTypeService: SeverityTypeService,
    public incidentStatusTypeService: IncidentStatusTypeService,
    public escalationRuleService: EscalationRuleService,
    public incidentChangeHistoryService: IncidentChangeHistoryService,
    public incidentTimelineEventService: IncidentTimelineEventService,
    public incidentNoteService: IncidentNoteService,
    public incidentNotificationService: IncidentNotificationService,
    public webhookDeliveryAttemptService: WebhookDeliveryAttemptService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the incidentId from the route parameters
    this.incidentId = this.route.snapshot.paramMap.get('incidentId');

    if (this.incidentId === 'new' ||
        this.incidentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.incidentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.incidentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Incident';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Incident';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.incidentForm.dirty) {
      return confirm('You have unsaved Incident changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.incidentId != null && this.incidentId !== 'new') {

      const id = parseInt(this.incidentId, 10);

      if (!isNaN(id)) {
        return { incidentId: id };
      }
    }

    return null;
  }


/*
  * Loads the Incident data for the current incidentId.
  *
  * Fully respects the IncidentService caching strategy and error handling strategy.
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
    if (!this.incidentService.userIsAlertingIncidentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Incidents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate incidentId
    //
    if (!this.incidentId) {

      this.alertService.showMessage('No Incident ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const incidentId = Number(this.incidentId);

    if (isNaN(incidentId) || incidentId <= 0) {

      this.alertService.showMessage(`Invalid Incident ID: "${this.incidentId}"`,
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
      // This is the most targeted way: clear only this Incident + relations

      this.incidentService.ClearRecordCache(incidentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.incidentService.GetIncident(incidentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (incidentData) => {

        //
        // Success path — incidentData can legitimately be null if 404'd but request succeeded
        //
        if (!incidentData) {

          this.handleIncidentNotFound(incidentId);

        } else {

          this.incidentData = incidentData;
          this.buildFormValues(this.incidentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Incident loaded successfully',
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
        this.handleIncidentLoadError(error, incidentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIncidentNotFound(incidentId: number): void {

    this.incidentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Incident #${incidentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIncidentLoadError(error: any, incidentId: number): void {

    let message = 'Failed to load Incident.';
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
          message = 'You do not have permission to view this Incident.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Incident #${incidentId} was not found.`;
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

    console.error(`Incident load failed (ID: ${incidentId})`, error);

    //
    // Reset UI to safe state
    //
    this.incidentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(incidentData: IncidentData | null) {

    if (incidentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentForm.reset({
        incidentKey: '',
        serviceId: null,
        title: '',
        description: '',
        severityTypeId: null,
        incidentStatusTypeId: null,
        createdAt: '',
        escalationRuleId: null,
        currentRepeatCount: '',
        nextEscalationAt: '',
        acknowledgedAt: '',
        resolvedAt: '',
        currentAssigneeObjectGuid: '',
        sourcePayloadJson: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.incidentForm.reset({
        incidentKey: incidentData.incidentKey ?? '',
        serviceId: incidentData.serviceId,
        title: incidentData.title ?? '',
        description: incidentData.description ?? '',
        severityTypeId: incidentData.severityTypeId,
        incidentStatusTypeId: incidentData.incidentStatusTypeId,
        createdAt: isoUtcStringToDateTimeLocal(incidentData.createdAt) ?? '',
        escalationRuleId: incidentData.escalationRuleId,
        currentRepeatCount: incidentData.currentRepeatCount?.toString() ?? '',
        nextEscalationAt: isoUtcStringToDateTimeLocal(incidentData.nextEscalationAt) ?? '',
        acknowledgedAt: isoUtcStringToDateTimeLocal(incidentData.acknowledgedAt) ?? '',
        resolvedAt: isoUtcStringToDateTimeLocal(incidentData.resolvedAt) ?? '',
        currentAssigneeObjectGuid: incidentData.currentAssigneeObjectGuid ?? '',
        sourcePayloadJson: incidentData.sourcePayloadJson ?? '',
        versionNumber: incidentData.versionNumber?.toString() ?? '',
        active: incidentData.active ?? true,
        deleted: incidentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentForm.markAsPristine();
    this.incidentForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.incidentService.userIsAlertingIncidentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Incidents", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.incidentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentSubmitData: IncidentSubmitData = {
        id: this.incidentData?.id || 0,
        incidentKey: formValue.incidentKey!.trim(),
        serviceId: Number(formValue.serviceId),
        title: formValue.title!.trim(),
        description: formValue.description?.trim() || null,
        severityTypeId: Number(formValue.severityTypeId),
        incidentStatusTypeId: Number(formValue.incidentStatusTypeId),
        createdAt: dateTimeLocalToIsoUtc(formValue.createdAt!.trim())!,
        escalationRuleId: formValue.escalationRuleId ? Number(formValue.escalationRuleId) : null,
        currentRepeatCount: formValue.currentRepeatCount ? Number(formValue.currentRepeatCount) : null,
        nextEscalationAt: formValue.nextEscalationAt ? dateTimeLocalToIsoUtc(formValue.nextEscalationAt.trim()) : null,
        acknowledgedAt: formValue.acknowledgedAt ? dateTimeLocalToIsoUtc(formValue.acknowledgedAt.trim()) : null,
        resolvedAt: formValue.resolvedAt ? dateTimeLocalToIsoUtc(formValue.resolvedAt.trim()) : null,
        currentAssigneeObjectGuid: formValue.currentAssigneeObjectGuid?.trim() || null,
        sourcePayloadJson: formValue.sourcePayloadJson?.trim() || null,
        versionNumber: this.incidentData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.incidentService.PutIncident(incidentSubmitData.id, incidentSubmitData)
      : this.incidentService.PostIncident(incidentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIncidentData) => {

        this.incidentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Incident's detail page
          //
          this.incidentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.incidentForm.markAsUntouched();

          this.router.navigate(['/incidents', savedIncidentData.id]);
          this.alertService.showMessage('Incident added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.incidentData = savedIncidentData;
          this.buildFormValues(this.incidentData);

          this.alertService.showMessage("Incident saved successfully", '', MessageSeverity.success);
        }
      },
      error: (err) => {

            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Incident.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingIncidentReader(): boolean {
    return this.incidentService.userIsAlertingIncidentReader();
  }

  public userIsAlertingIncidentWriter(): boolean {
    return this.incidentService.userIsAlertingIncidentWriter();
  }
}

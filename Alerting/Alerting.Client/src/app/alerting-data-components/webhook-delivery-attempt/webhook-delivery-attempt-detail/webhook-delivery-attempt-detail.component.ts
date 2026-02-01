/*
   GENERATED FORM FOR THE WEBHOOKDELIVERYATTEMPT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from WebhookDeliveryAttempt table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to webhook-delivery-attempt-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { WebhookDeliveryAttemptService, WebhookDeliveryAttemptData, WebhookDeliveryAttemptSubmitData } from '../../../alerting-data-services/webhook-delivery-attempt.service';
import { IncidentService } from '../../../alerting-data-services/incident.service';
import { IntegrationService } from '../../../alerting-data-services/integration.service';
import { IncidentTimelineEventService } from '../../../alerting-data-services/incident-timeline-event.service';
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
interface WebhookDeliveryAttemptFormValues {
  incidentId: number | bigint,       // For FK link number
  integrationId: number | bigint,       // For FK link number
  incidentTimelineEventId: number | bigint | null,       // For FK link number
  attemptNumber: string,     // Stored as string for form input, converted to number on submit.
  attemptedAt: string,
  httpStatusCode: string | null,     // Stored as string for form input, converted to number on submit.
  success: boolean,
  payloadJson: string | null,
  responseBody: string | null,
  errorMessage: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-webhook-delivery-attempt-detail',
  templateUrl: './webhook-delivery-attempt-detail.component.html',
  styleUrls: ['./webhook-delivery-attempt-detail.component.scss']
})

export class WebhookDeliveryAttemptDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<WebhookDeliveryAttemptFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public webhookDeliveryAttemptForm: FormGroup = this.fb.group({
        incidentId: [null, Validators.required],
        integrationId: [null, Validators.required],
        incidentTimelineEventId: [null],
        attemptNumber: ['', Validators.required],
        attemptedAt: ['', Validators.required],
        httpStatusCode: [''],
        success: [false],
        payloadJson: [''],
        responseBody: [''],
        errorMessage: [''],
        active: [true],
        deleted: [false],
      });


  public webhookDeliveryAttemptId: string | null = null;
  public webhookDeliveryAttemptData: WebhookDeliveryAttemptData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  webhookDeliveryAttempts$ = this.webhookDeliveryAttemptService.GetWebhookDeliveryAttemptList();
  public incidents$ = this.incidentService.GetIncidentList();
  public integrations$ = this.integrationService.GetIntegrationList();
  public incidentTimelineEvents$ = this.incidentTimelineEventService.GetIncidentTimelineEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public webhookDeliveryAttemptService: WebhookDeliveryAttemptService,
    public incidentService: IncidentService,
    public integrationService: IntegrationService,
    public incidentTimelineEventService: IncidentTimelineEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the webhookDeliveryAttemptId from the route parameters
    this.webhookDeliveryAttemptId = this.route.snapshot.paramMap.get('webhookDeliveryAttemptId');

    if (this.webhookDeliveryAttemptId === 'new' ||
        this.webhookDeliveryAttemptId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.webhookDeliveryAttemptData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.webhookDeliveryAttemptForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.webhookDeliveryAttemptForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Webhook Delivery Attempt';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Webhook Delivery Attempt';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.webhookDeliveryAttemptForm.dirty) {
      return confirm('You have unsaved Webhook Delivery Attempt changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.webhookDeliveryAttemptId != null && this.webhookDeliveryAttemptId !== 'new') {

      const id = parseInt(this.webhookDeliveryAttemptId, 10);

      if (!isNaN(id)) {
        return { webhookDeliveryAttemptId: id };
      }
    }

    return null;
  }


/*
  * Loads the WebhookDeliveryAttempt data for the current webhookDeliveryAttemptId.
  *
  * Fully respects the WebhookDeliveryAttemptService caching strategy and error handling strategy.
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
    if (!this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read WebhookDeliveryAttempts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate webhookDeliveryAttemptId
    //
    if (!this.webhookDeliveryAttemptId) {

      this.alertService.showMessage('No WebhookDeliveryAttempt ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const webhookDeliveryAttemptId = Number(this.webhookDeliveryAttemptId);

    if (isNaN(webhookDeliveryAttemptId) || webhookDeliveryAttemptId <= 0) {

      this.alertService.showMessage(`Invalid Webhook Delivery Attempt ID: "${this.webhookDeliveryAttemptId}"`,
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
      // This is the most targeted way: clear only this WebhookDeliveryAttempt + relations

      this.webhookDeliveryAttemptService.ClearRecordCache(webhookDeliveryAttemptId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.webhookDeliveryAttemptService.GetWebhookDeliveryAttempt(webhookDeliveryAttemptId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (webhookDeliveryAttemptData) => {

        //
        // Success path — webhookDeliveryAttemptData can legitimately be null if 404'd but request succeeded
        //
        if (!webhookDeliveryAttemptData) {

          this.handleWebhookDeliveryAttemptNotFound(webhookDeliveryAttemptId);

        } else {

          this.webhookDeliveryAttemptData = webhookDeliveryAttemptData;
          this.buildFormValues(this.webhookDeliveryAttemptData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'WebhookDeliveryAttempt loaded successfully',
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
        this.handleWebhookDeliveryAttemptLoadError(error, webhookDeliveryAttemptId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleWebhookDeliveryAttemptNotFound(webhookDeliveryAttemptId: number): void {

    this.webhookDeliveryAttemptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `WebhookDeliveryAttempt #${webhookDeliveryAttemptId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleWebhookDeliveryAttemptLoadError(error: any, webhookDeliveryAttemptId: number): void {

    let message = 'Failed to load Webhook Delivery Attempt.';
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
          message = 'You do not have permission to view this Webhook Delivery Attempt.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Webhook Delivery Attempt #${webhookDeliveryAttemptId} was not found.`;
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

    console.error(`Webhook Delivery Attempt load failed (ID: ${webhookDeliveryAttemptId})`, error);

    //
    // Reset UI to safe state
    //
    this.webhookDeliveryAttemptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(webhookDeliveryAttemptData: WebhookDeliveryAttemptData | null) {

    if (webhookDeliveryAttemptData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.webhookDeliveryAttemptForm.reset({
        incidentId: null,
        integrationId: null,
        incidentTimelineEventId: null,
        attemptNumber: '',
        attemptedAt: '',
        httpStatusCode: '',
        success: false,
        payloadJson: '',
        responseBody: '',
        errorMessage: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.webhookDeliveryAttemptForm.reset({
        incidentId: webhookDeliveryAttemptData.incidentId,
        integrationId: webhookDeliveryAttemptData.integrationId,
        incidentTimelineEventId: webhookDeliveryAttemptData.incidentTimelineEventId,
        attemptNumber: webhookDeliveryAttemptData.attemptNumber?.toString() ?? '',
        attemptedAt: isoUtcStringToDateTimeLocal(webhookDeliveryAttemptData.attemptedAt) ?? '',
        httpStatusCode: webhookDeliveryAttemptData.httpStatusCode?.toString() ?? '',
        success: webhookDeliveryAttemptData.success ?? false,
        payloadJson: webhookDeliveryAttemptData.payloadJson ?? '',
        responseBody: webhookDeliveryAttemptData.responseBody ?? '',
        errorMessage: webhookDeliveryAttemptData.errorMessage ?? '',
        active: webhookDeliveryAttemptData.active ?? true,
        deleted: webhookDeliveryAttemptData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.webhookDeliveryAttemptForm.markAsPristine();
    this.webhookDeliveryAttemptForm.markAsUntouched();
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

    if (this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Webhook Delivery Attempts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.webhookDeliveryAttemptForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.webhookDeliveryAttemptForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.webhookDeliveryAttemptForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const webhookDeliveryAttemptSubmitData: WebhookDeliveryAttemptSubmitData = {
        id: this.webhookDeliveryAttemptData?.id || 0,
        incidentId: Number(formValue.incidentId),
        integrationId: Number(formValue.integrationId),
        incidentTimelineEventId: formValue.incidentTimelineEventId ? Number(formValue.incidentTimelineEventId) : null,
        attemptNumber: Number(formValue.attemptNumber),
        attemptedAt: dateTimeLocalToIsoUtc(formValue.attemptedAt!.trim())!,
        httpStatusCode: formValue.httpStatusCode ? Number(formValue.httpStatusCode) : null,
        success: !!formValue.success,
        payloadJson: formValue.payloadJson?.trim() || null,
        responseBody: formValue.responseBody?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.webhookDeliveryAttemptService.PutWebhookDeliveryAttempt(webhookDeliveryAttemptSubmitData.id, webhookDeliveryAttemptSubmitData)
      : this.webhookDeliveryAttemptService.PostWebhookDeliveryAttempt(webhookDeliveryAttemptSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedWebhookDeliveryAttemptData) => {

        this.webhookDeliveryAttemptService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Webhook Delivery Attempt's detail page
          //
          this.webhookDeliveryAttemptForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.webhookDeliveryAttemptForm.markAsUntouched();

          this.router.navigate(['/webhookdeliveryattempts', savedWebhookDeliveryAttemptData.id]);
          this.alertService.showMessage('Webhook Delivery Attempt added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.webhookDeliveryAttemptData = savedWebhookDeliveryAttemptData;
          this.buildFormValues(this.webhookDeliveryAttemptData);

          this.alertService.showMessage("Webhook Delivery Attempt saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Webhook Delivery Attempt.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Webhook Delivery Attempt.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Webhook Delivery Attempt could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingWebhookDeliveryAttemptReader(): boolean {
    return this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptReader();
  }

  public userIsAlertingWebhookDeliveryAttemptWriter(): boolean {
    return this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptWriter();
  }
}

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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { WebhookDeliveryAttemptService, WebhookDeliveryAttemptData, WebhookDeliveryAttemptSubmitData } from '../../../alerting-data-services/webhook-delivery-attempt.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IncidentService } from '../../../alerting-data-services/incident.service';
import { IntegrationService } from '../../../alerting-data-services/integration.service';
import { IncidentTimelineEventService } from '../../../alerting-data-services/incident-timeline-event.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-webhook-delivery-attempt-add-edit',
  templateUrl: './webhook-delivery-attempt-add-edit.component.html',
  styleUrls: ['./webhook-delivery-attempt-add-edit.component.scss']
})
export class WebhookDeliveryAttemptAddEditComponent {
  @ViewChild('webhookDeliveryAttemptModal') webhookDeliveryAttemptModal!: TemplateRef<any>;
  @Output() webhookDeliveryAttemptChanged = new Subject<WebhookDeliveryAttemptData[]>();
  @Input() webhookDeliveryAttemptSubmitData: WebhookDeliveryAttemptSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  webhookDeliveryAttempts$ = this.webhookDeliveryAttemptService.GetWebhookDeliveryAttemptList();
  incidents$ = this.incidentService.GetIncidentList();
  integrations$ = this.integrationService.GetIntegrationList();
  incidentTimelineEvents$ = this.incidentTimelineEventService.GetIncidentTimelineEventList();

  constructor(
    private modalService: NgbModal,
    private webhookDeliveryAttemptService: WebhookDeliveryAttemptService,
    private incidentService: IncidentService,
    private integrationService: IntegrationService,
    private incidentTimelineEventService: IncidentTimelineEventService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(webhookDeliveryAttemptData?: WebhookDeliveryAttemptData) {

    if (webhookDeliveryAttemptData != null) {

      if (!this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Webhook Delivery Attempts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.webhookDeliveryAttemptSubmitData = this.webhookDeliveryAttemptService.ConvertToWebhookDeliveryAttemptSubmitData(webhookDeliveryAttemptData);
      this.isEditMode = true;
      this.objectGuid = webhookDeliveryAttemptData.objectGuid;

      this.buildFormValues(webhookDeliveryAttemptData);

    } else {

      if (!this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Webhook Delivery Attempts`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.webhookDeliveryAttemptForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.webhookDeliveryAttemptModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Webhook Delivery Attempts`,
        '',
        MessageSeverity.info
      );
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
        id: this.webhookDeliveryAttemptSubmitData?.id || 0,
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

      if (this.isEditMode) {
        this.updateWebhookDeliveryAttempt(webhookDeliveryAttemptSubmitData);
      } else {
        this.addWebhookDeliveryAttempt(webhookDeliveryAttemptSubmitData);
      }
  }

  private addWebhookDeliveryAttempt(webhookDeliveryAttemptData: WebhookDeliveryAttemptSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    webhookDeliveryAttemptData.active = true;
    webhookDeliveryAttemptData.deleted = false;
    this.webhookDeliveryAttemptService.PostWebhookDeliveryAttempt(webhookDeliveryAttemptData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newWebhookDeliveryAttempt) => {

        this.webhookDeliveryAttemptService.ClearAllCaches();

        this.webhookDeliveryAttemptChanged.next([newWebhookDeliveryAttempt]);

        this.alertService.showMessage("Webhook Delivery Attempt added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/webhookdeliveryattempt', newWebhookDeliveryAttempt.id]);
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


  private updateWebhookDeliveryAttempt(webhookDeliveryAttemptData: WebhookDeliveryAttemptSubmitData) {
    this.webhookDeliveryAttemptService.PutWebhookDeliveryAttempt(webhookDeliveryAttemptData.id, webhookDeliveryAttemptData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedWebhookDeliveryAttempt) => {

        this.webhookDeliveryAttemptService.ClearAllCaches();

        this.webhookDeliveryAttemptChanged.next([updatedWebhookDeliveryAttempt]);

        this.alertService.showMessage("Webhook Delivery Attempt updated successfully", '', MessageSeverity.success);

        this.closeModal();
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


  public userIsAlertingWebhookDeliveryAttemptReader(): boolean {
    return this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptReader();
  }

  public userIsAlertingWebhookDeliveryAttemptWriter(): boolean {
    return this.webhookDeliveryAttemptService.userIsAlertingWebhookDeliveryAttemptWriter();
  }
}

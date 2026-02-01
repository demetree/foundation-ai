/*
   GENERATED FORM FOR THE INCIDENTNOTIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IncidentNotification table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-notification-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentNotificationService, IncidentNotificationData, IncidentNotificationSubmitData } from '../../../alerting-data-services/incident-notification.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IncidentService } from '../../../alerting-data-services/incident.service';
import { EscalationRuleService } from '../../../alerting-data-services/escalation-rule.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IncidentNotificationFormValues {
  incidentId: number | bigint,       // For FK link number
  escalationRuleId: number | bigint | null,       // For FK link number
  userObjectGuid: string,
  firstNotifiedAt: string,
  lastNotifiedAt: string | null,
  acknowledgedAt: string | null,
  acknowledgedByObjectGuid: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-incident-notification-add-edit',
  templateUrl: './incident-notification-add-edit.component.html',
  styleUrls: ['./incident-notification-add-edit.component.scss']
})
export class IncidentNotificationAddEditComponent {
  @ViewChild('incidentNotificationModal') incidentNotificationModal!: TemplateRef<any>;
  @Output() incidentNotificationChanged = new Subject<IncidentNotificationData[]>();
  @Input() incidentNotificationSubmitData: IncidentNotificationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentNotificationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentNotificationForm: FormGroup = this.fb.group({
        incidentId: [null, Validators.required],
        escalationRuleId: [null],
        userObjectGuid: ['', Validators.required],
        firstNotifiedAt: ['', Validators.required],
        lastNotifiedAt: [''],
        acknowledgedAt: [''],
        acknowledgedByObjectGuid: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  incidentNotifications$ = this.incidentNotificationService.GetIncidentNotificationList();
  incidents$ = this.incidentService.GetIncidentList();
  escalationRules$ = this.escalationRuleService.GetEscalationRuleList();

  constructor(
    private modalService: NgbModal,
    private incidentNotificationService: IncidentNotificationService,
    private incidentService: IncidentService,
    private escalationRuleService: EscalationRuleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(incidentNotificationData?: IncidentNotificationData) {

    if (incidentNotificationData != null) {

      if (!this.incidentNotificationService.userIsAlertingIncidentNotificationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Incident Notifications`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.incidentNotificationSubmitData = this.incidentNotificationService.ConvertToIncidentNotificationSubmitData(incidentNotificationData);
      this.isEditMode = true;
      this.objectGuid = incidentNotificationData.objectGuid;

      this.buildFormValues(incidentNotificationData);

    } else {

      if (!this.incidentNotificationService.userIsAlertingIncidentNotificationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Incident Notifications`,
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
        this.incidentNotificationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentNotificationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.incidentNotificationModal, {
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

    if (this.incidentNotificationService.userIsAlertingIncidentNotificationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Incident Notifications`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.incidentNotificationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentNotificationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentNotificationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentNotificationSubmitData: IncidentNotificationSubmitData = {
        id: this.incidentNotificationSubmitData?.id || 0,
        incidentId: Number(formValue.incidentId),
        escalationRuleId: formValue.escalationRuleId ? Number(formValue.escalationRuleId) : null,
        userObjectGuid: formValue.userObjectGuid!.trim(),
        firstNotifiedAt: dateTimeLocalToIsoUtc(formValue.firstNotifiedAt!.trim())!,
        lastNotifiedAt: formValue.lastNotifiedAt ? dateTimeLocalToIsoUtc(formValue.lastNotifiedAt.trim()) : null,
        acknowledgedAt: formValue.acknowledgedAt ? dateTimeLocalToIsoUtc(formValue.acknowledgedAt.trim()) : null,
        acknowledgedByObjectGuid: formValue.acknowledgedByObjectGuid?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIncidentNotification(incidentNotificationSubmitData);
      } else {
        this.addIncidentNotification(incidentNotificationSubmitData);
      }
  }

  private addIncidentNotification(incidentNotificationData: IncidentNotificationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    incidentNotificationData.active = true;
    incidentNotificationData.deleted = false;
    this.incidentNotificationService.PostIncidentNotification(incidentNotificationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIncidentNotification) => {

        this.incidentNotificationService.ClearAllCaches();

        this.incidentNotificationChanged.next([newIncidentNotification]);

        this.alertService.showMessage("Incident Notification added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/incidentnotification', newIncidentNotification.id]);
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
                                   'You do not have permission to save this Incident Notification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Notification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Notification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIncidentNotification(incidentNotificationData: IncidentNotificationSubmitData) {
    this.incidentNotificationService.PutIncidentNotification(incidentNotificationData.id, incidentNotificationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIncidentNotification) => {

        this.incidentNotificationService.ClearAllCaches();

        this.incidentNotificationChanged.next([updatedIncidentNotification]);

        this.alertService.showMessage("Incident Notification updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Incident Notification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident Notification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident Notification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(incidentNotificationData: IncidentNotificationData | null) {

    if (incidentNotificationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentNotificationForm.reset({
        incidentId: null,
        escalationRuleId: null,
        userObjectGuid: '',
        firstNotifiedAt: '',
        lastNotifiedAt: '',
        acknowledgedAt: '',
        acknowledgedByObjectGuid: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.incidentNotificationForm.reset({
        incidentId: incidentNotificationData.incidentId,
        escalationRuleId: incidentNotificationData.escalationRuleId,
        userObjectGuid: incidentNotificationData.userObjectGuid ?? '',
        firstNotifiedAt: isoUtcStringToDateTimeLocal(incidentNotificationData.firstNotifiedAt) ?? '',
        lastNotifiedAt: isoUtcStringToDateTimeLocal(incidentNotificationData.lastNotifiedAt) ?? '',
        acknowledgedAt: isoUtcStringToDateTimeLocal(incidentNotificationData.acknowledgedAt) ?? '',
        acknowledgedByObjectGuid: incidentNotificationData.acknowledgedByObjectGuid ?? '',
        active: incidentNotificationData.active ?? true,
        deleted: incidentNotificationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentNotificationForm.markAsPristine();
    this.incidentNotificationForm.markAsUntouched();
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


  public userIsAlertingIncidentNotificationReader(): boolean {
    return this.incidentNotificationService.userIsAlertingIncidentNotificationReader();
  }

  public userIsAlertingIncidentNotificationWriter(): boolean {
    return this.incidentNotificationService.userIsAlertingIncidentNotificationWriter();
  }
}

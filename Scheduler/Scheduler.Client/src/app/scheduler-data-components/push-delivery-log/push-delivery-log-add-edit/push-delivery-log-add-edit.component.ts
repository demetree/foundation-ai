/*
   GENERATED FORM FOR THE PUSHDELIVERYLOG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PushDeliveryLog table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to push-delivery-log-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PushDeliveryLogService, PushDeliveryLogData, PushDeliveryLogSubmitData } from '../../../scheduler-data-services/push-delivery-log.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PushDeliveryLogFormValues {
  userId: string,     // Stored as string for form input, converted to number on submit.
  providerId: string,
  destination: string | null,
  sourceType: string | null,
  sourceNotificationId: string | null,     // Stored as string for form input, converted to number on submit.
  sourceConversationMessageId: string | null,     // Stored as string for form input, converted to number on submit.
  success: boolean,
  externalId: string | null,
  errorMessage: string | null,
  attemptNumber: string,     // Stored as string for form input, converted to number on submit.
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-push-delivery-log-add-edit',
  templateUrl: './push-delivery-log-add-edit.component.html',
  styleUrls: ['./push-delivery-log-add-edit.component.scss']
})
export class PushDeliveryLogAddEditComponent {
  @ViewChild('pushDeliveryLogModal') pushDeliveryLogModal!: TemplateRef<any>;
  @Output() pushDeliveryLogChanged = new Subject<PushDeliveryLogData[]>();
  @Input() pushDeliveryLogSubmitData: PushDeliveryLogSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PushDeliveryLogFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pushDeliveryLogForm: FormGroup = this.fb.group({
        userId: ['', Validators.required],
        providerId: ['', Validators.required],
        destination: [''],
        sourceType: [''],
        sourceNotificationId: [''],
        sourceConversationMessageId: [''],
        success: [false],
        externalId: [''],
        errorMessage: [''],
        attemptNumber: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  pushDeliveryLogs$ = this.pushDeliveryLogService.GetPushDeliveryLogList();

  constructor(
    private modalService: NgbModal,
    private pushDeliveryLogService: PushDeliveryLogService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(pushDeliveryLogData?: PushDeliveryLogData) {

    if (pushDeliveryLogData != null) {

      if (!this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Push Delivery Logs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.pushDeliveryLogSubmitData = this.pushDeliveryLogService.ConvertToPushDeliveryLogSubmitData(pushDeliveryLogData);
      this.isEditMode = true;
      this.objectGuid = pushDeliveryLogData.objectGuid;

      this.buildFormValues(pushDeliveryLogData);

    } else {

      if (!this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Push Delivery Logs`,
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
        this.pushDeliveryLogForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pushDeliveryLogForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.pushDeliveryLogModal, {
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

    if (this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Push Delivery Logs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.pushDeliveryLogForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pushDeliveryLogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pushDeliveryLogForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pushDeliveryLogSubmitData: PushDeliveryLogSubmitData = {
        id: this.pushDeliveryLogSubmitData?.id || 0,
        userId: Number(formValue.userId),
        providerId: formValue.providerId!.trim(),
        destination: formValue.destination?.trim() || null,
        sourceType: formValue.sourceType?.trim() || null,
        sourceNotificationId: formValue.sourceNotificationId ? Number(formValue.sourceNotificationId) : null,
        sourceConversationMessageId: formValue.sourceConversationMessageId ? Number(formValue.sourceConversationMessageId) : null,
        success: !!formValue.success,
        externalId: formValue.externalId?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
        attemptNumber: Number(formValue.attemptNumber),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePushDeliveryLog(pushDeliveryLogSubmitData);
      } else {
        this.addPushDeliveryLog(pushDeliveryLogSubmitData);
      }
  }

  private addPushDeliveryLog(pushDeliveryLogData: PushDeliveryLogSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    pushDeliveryLogData.active = true;
    pushDeliveryLogData.deleted = false;
    this.pushDeliveryLogService.PostPushDeliveryLog(pushDeliveryLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPushDeliveryLog) => {

        this.pushDeliveryLogService.ClearAllCaches();

        this.pushDeliveryLogChanged.next([newPushDeliveryLog]);

        this.alertService.showMessage("Push Delivery Log added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/pushdeliverylog', newPushDeliveryLog.id]);
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
                                   'You do not have permission to save this Push Delivery Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Push Delivery Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Push Delivery Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePushDeliveryLog(pushDeliveryLogData: PushDeliveryLogSubmitData) {
    this.pushDeliveryLogService.PutPushDeliveryLog(pushDeliveryLogData.id, pushDeliveryLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPushDeliveryLog) => {

        this.pushDeliveryLogService.ClearAllCaches();

        this.pushDeliveryLogChanged.next([updatedPushDeliveryLog]);

        this.alertService.showMessage("Push Delivery Log updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Push Delivery Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Push Delivery Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Push Delivery Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(pushDeliveryLogData: PushDeliveryLogData | null) {

    if (pushDeliveryLogData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pushDeliveryLogForm.reset({
        userId: '',
        providerId: '',
        destination: '',
        sourceType: '',
        sourceNotificationId: '',
        sourceConversationMessageId: '',
        success: false,
        externalId: '',
        errorMessage: '',
        attemptNumber: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pushDeliveryLogForm.reset({
        userId: pushDeliveryLogData.userId?.toString() ?? '',
        providerId: pushDeliveryLogData.providerId ?? '',
        destination: pushDeliveryLogData.destination ?? '',
        sourceType: pushDeliveryLogData.sourceType ?? '',
        sourceNotificationId: pushDeliveryLogData.sourceNotificationId?.toString() ?? '',
        sourceConversationMessageId: pushDeliveryLogData.sourceConversationMessageId?.toString() ?? '',
        success: pushDeliveryLogData.success ?? false,
        externalId: pushDeliveryLogData.externalId ?? '',
        errorMessage: pushDeliveryLogData.errorMessage ?? '',
        attemptNumber: pushDeliveryLogData.attemptNumber?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(pushDeliveryLogData.dateTimeCreated) ?? '',
        active: pushDeliveryLogData.active ?? true,
        deleted: pushDeliveryLogData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.pushDeliveryLogForm.markAsPristine();
    this.pushDeliveryLogForm.markAsUntouched();
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


  public userIsSchedulerPushDeliveryLogReader(): boolean {
    return this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogReader();
  }

  public userIsSchedulerPushDeliveryLogWriter(): boolean {
    return this.pushDeliveryLogService.userIsSchedulerPushDeliveryLogWriter();
  }
}

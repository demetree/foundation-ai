/*
   GENERATED FORM FOR THE MESSAGINGAUDITLOG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MessagingAuditLog table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to messaging-audit-log-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MessagingAuditLogService, MessagingAuditLogData, MessagingAuditLogSubmitData } from '../../../scheduler-data-services/messaging-audit-log.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MessagingAuditLogFormValues {
  performedByUserId: string,     // Stored as string for form input, converted to number on submit.
  action: string,
  entityType: string | null,
  entityId: string | null,     // Stored as string for form input, converted to number on submit.
  details: string | null,
  ipAddress: string | null,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-messaging-audit-log-add-edit',
  templateUrl: './messaging-audit-log-add-edit.component.html',
  styleUrls: ['./messaging-audit-log-add-edit.component.scss']
})
export class MessagingAuditLogAddEditComponent {
  @ViewChild('messagingAuditLogModal') messagingAuditLogModal!: TemplateRef<any>;
  @Output() messagingAuditLogChanged = new Subject<MessagingAuditLogData[]>();
  @Input() messagingAuditLogSubmitData: MessagingAuditLogSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MessagingAuditLogFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public messagingAuditLogForm: FormGroup = this.fb.group({
        performedByUserId: ['', Validators.required],
        action: ['', Validators.required],
        entityType: [''],
        entityId: [''],
        details: [''],
        ipAddress: [''],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  messagingAuditLogs$ = this.messagingAuditLogService.GetMessagingAuditLogList();

  constructor(
    private modalService: NgbModal,
    private messagingAuditLogService: MessagingAuditLogService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(messagingAuditLogData?: MessagingAuditLogData) {

    if (messagingAuditLogData != null) {

      if (!this.messagingAuditLogService.userIsSchedulerMessagingAuditLogReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Messaging Audit Logs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.messagingAuditLogSubmitData = this.messagingAuditLogService.ConvertToMessagingAuditLogSubmitData(messagingAuditLogData);
      this.isEditMode = true;
      this.objectGuid = messagingAuditLogData.objectGuid;

      this.buildFormValues(messagingAuditLogData);

    } else {

      if (!this.messagingAuditLogService.userIsSchedulerMessagingAuditLogWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Messaging Audit Logs`,
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
        this.messagingAuditLogForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.messagingAuditLogForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.messagingAuditLogModal, {
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

    if (this.messagingAuditLogService.userIsSchedulerMessagingAuditLogWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Messaging Audit Logs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.messagingAuditLogForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.messagingAuditLogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.messagingAuditLogForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const messagingAuditLogSubmitData: MessagingAuditLogSubmitData = {
        id: this.messagingAuditLogSubmitData?.id || 0,
        performedByUserId: Number(formValue.performedByUserId),
        action: formValue.action!.trim(),
        entityType: formValue.entityType?.trim() || null,
        entityId: formValue.entityId ? Number(formValue.entityId) : null,
        details: formValue.details?.trim() || null,
        ipAddress: formValue.ipAddress?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMessagingAuditLog(messagingAuditLogSubmitData);
      } else {
        this.addMessagingAuditLog(messagingAuditLogSubmitData);
      }
  }

  private addMessagingAuditLog(messagingAuditLogData: MessagingAuditLogSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    messagingAuditLogData.active = true;
    messagingAuditLogData.deleted = false;
    this.messagingAuditLogService.PostMessagingAuditLog(messagingAuditLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMessagingAuditLog) => {

        this.messagingAuditLogService.ClearAllCaches();

        this.messagingAuditLogChanged.next([newMessagingAuditLog]);

        this.alertService.showMessage("Messaging Audit Log added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/messagingauditlog', newMessagingAuditLog.id]);
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
                                   'You do not have permission to save this Messaging Audit Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Messaging Audit Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Messaging Audit Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMessagingAuditLog(messagingAuditLogData: MessagingAuditLogSubmitData) {
    this.messagingAuditLogService.PutMessagingAuditLog(messagingAuditLogData.id, messagingAuditLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMessagingAuditLog) => {

        this.messagingAuditLogService.ClearAllCaches();

        this.messagingAuditLogChanged.next([updatedMessagingAuditLog]);

        this.alertService.showMessage("Messaging Audit Log updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Messaging Audit Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Messaging Audit Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Messaging Audit Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(messagingAuditLogData: MessagingAuditLogData | null) {

    if (messagingAuditLogData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.messagingAuditLogForm.reset({
        performedByUserId: '',
        action: '',
        entityType: '',
        entityId: '',
        details: '',
        ipAddress: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.messagingAuditLogForm.reset({
        performedByUserId: messagingAuditLogData.performedByUserId?.toString() ?? '',
        action: messagingAuditLogData.action ?? '',
        entityType: messagingAuditLogData.entityType ?? '',
        entityId: messagingAuditLogData.entityId?.toString() ?? '',
        details: messagingAuditLogData.details ?? '',
        ipAddress: messagingAuditLogData.ipAddress ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(messagingAuditLogData.dateTimeCreated) ?? '',
        active: messagingAuditLogData.active ?? true,
        deleted: messagingAuditLogData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.messagingAuditLogForm.markAsPristine();
    this.messagingAuditLogForm.markAsUntouched();
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


  public userIsSchedulerMessagingAuditLogReader(): boolean {
    return this.messagingAuditLogService.userIsSchedulerMessagingAuditLogReader();
  }

  public userIsSchedulerMessagingAuditLogWriter(): boolean {
    return this.messagingAuditLogService.userIsSchedulerMessagingAuditLogWriter();
  }
}

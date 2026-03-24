/*
   GENERATED FORM FOR THE MESSAGEFLAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MessageFlag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to message-flag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MessageFlagService, MessageFlagData, MessageFlagSubmitData } from '../../../scheduler-data-services/message-flag.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConversationMessageService } from '../../../scheduler-data-services/conversation-message.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MessageFlagFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  flaggedByUserId: string,     // Stored as string for form input, converted to number on submit.
  reason: string,
  details: string | null,
  status: string,
  reviewedByUserId: string | null,     // Stored as string for form input, converted to number on submit.
  dateTimeReviewed: string | null,
  resolutionNotes: string | null,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-message-flag-add-edit',
  templateUrl: './message-flag-add-edit.component.html',
  styleUrls: ['./message-flag-add-edit.component.scss']
})
export class MessageFlagAddEditComponent {
  @ViewChild('messageFlagModal') messageFlagModal!: TemplateRef<any>;
  @Output() messageFlagChanged = new Subject<MessageFlagData[]>();
  @Input() messageFlagSubmitData: MessageFlagSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MessageFlagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public messageFlagForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        flaggedByUserId: ['', Validators.required],
        reason: ['', Validators.required],
        details: [''],
        status: ['', Validators.required],
        reviewedByUserId: [''],
        dateTimeReviewed: [''],
        resolutionNotes: [''],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  messageFlags$ = this.messageFlagService.GetMessageFlagList();
  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  constructor(
    private modalService: NgbModal,
    private messageFlagService: MessageFlagService,
    private conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(messageFlagData?: MessageFlagData) {

    if (messageFlagData != null) {

      if (!this.messageFlagService.userIsSchedulerMessageFlagReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Message Flags`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.messageFlagSubmitData = this.messageFlagService.ConvertToMessageFlagSubmitData(messageFlagData);
      this.isEditMode = true;
      this.objectGuid = messageFlagData.objectGuid;

      this.buildFormValues(messageFlagData);

    } else {

      if (!this.messageFlagService.userIsSchedulerMessageFlagWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Message Flags`,
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
        this.messageFlagForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.messageFlagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.messageFlagModal, {
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

    if (this.messageFlagService.userIsSchedulerMessageFlagWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Message Flags`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.messageFlagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.messageFlagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.messageFlagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const messageFlagSubmitData: MessageFlagSubmitData = {
        id: this.messageFlagSubmitData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        flaggedByUserId: Number(formValue.flaggedByUserId),
        reason: formValue.reason!.trim(),
        details: formValue.details?.trim() || null,
        status: formValue.status!.trim(),
        reviewedByUserId: formValue.reviewedByUserId ? Number(formValue.reviewedByUserId) : null,
        dateTimeReviewed: formValue.dateTimeReviewed ? dateTimeLocalToIsoUtc(formValue.dateTimeReviewed.trim()) : null,
        resolutionNotes: formValue.resolutionNotes?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMessageFlag(messageFlagSubmitData);
      } else {
        this.addMessageFlag(messageFlagSubmitData);
      }
  }

  private addMessageFlag(messageFlagData: MessageFlagSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    messageFlagData.active = true;
    messageFlagData.deleted = false;
    this.messageFlagService.PostMessageFlag(messageFlagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMessageFlag) => {

        this.messageFlagService.ClearAllCaches();

        this.messageFlagChanged.next([newMessageFlag]);

        this.alertService.showMessage("Message Flag added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/messageflag', newMessageFlag.id]);
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
                                   'You do not have permission to save this Message Flag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Message Flag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Message Flag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMessageFlag(messageFlagData: MessageFlagSubmitData) {
    this.messageFlagService.PutMessageFlag(messageFlagData.id, messageFlagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMessageFlag) => {

        this.messageFlagService.ClearAllCaches();

        this.messageFlagChanged.next([updatedMessageFlag]);

        this.alertService.showMessage("Message Flag updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Message Flag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Message Flag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Message Flag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(messageFlagData: MessageFlagData | null) {

    if (messageFlagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.messageFlagForm.reset({
        conversationMessageId: null,
        flaggedByUserId: '',
        reason: '',
        details: '',
        status: '',
        reviewedByUserId: '',
        dateTimeReviewed: '',
        resolutionNotes: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.messageFlagForm.reset({
        conversationMessageId: messageFlagData.conversationMessageId,
        flaggedByUserId: messageFlagData.flaggedByUserId?.toString() ?? '',
        reason: messageFlagData.reason ?? '',
        details: messageFlagData.details ?? '',
        status: messageFlagData.status ?? '',
        reviewedByUserId: messageFlagData.reviewedByUserId?.toString() ?? '',
        dateTimeReviewed: isoUtcStringToDateTimeLocal(messageFlagData.dateTimeReviewed) ?? '',
        resolutionNotes: messageFlagData.resolutionNotes ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(messageFlagData.dateTimeCreated) ?? '',
        active: messageFlagData.active ?? true,
        deleted: messageFlagData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.messageFlagForm.markAsPristine();
    this.messageFlagForm.markAsUntouched();
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


  public userIsSchedulerMessageFlagReader(): boolean {
    return this.messageFlagService.userIsSchedulerMessageFlagReader();
  }

  public userIsSchedulerMessageFlagWriter(): boolean {
    return this.messageFlagService.userIsSchedulerMessageFlagWriter();
  }
}

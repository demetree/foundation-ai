/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageService, ConversationMessageData, ConversationMessageSubmitData } from '../../../scheduler-data-services/conversation-message.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConversationService } from '../../../scheduler-data-services/conversation.service';
import { ConversationChannelService } from '../../../scheduler-data-services/conversation-channel.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConversationMessageFormValues {
  conversationId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  parentConversationMessageId: number | bigint | null,       // For FK link number
  conversationChannelId: number | bigint | null,       // For FK link number
  dateTimeCreated: string,
  message: string,
  messageType: string | null,
  entity: string | null,
  entityId: string | null,     // Stored as string for form input, converted to number on submit.
  externalURL: string | null,
  forwardedFromMessageId: string | null,     // Stored as string for form input, converted to number on submit.
  forwardedFromUserId: string | null,     // Stored as string for form input, converted to number on submit.
  isScheduled: boolean,
  scheduledDateTime: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-message-add-edit',
  templateUrl: './conversation-message-add-edit.component.html',
  styleUrls: ['./conversation-message-add-edit.component.scss']
})
export class ConversationMessageAddEditComponent {
  @ViewChild('conversationMessageModal') conversationMessageModal!: TemplateRef<any>;
  @Output() conversationMessageChanged = new Subject<ConversationMessageData[]>();
  @Input() conversationMessageSubmitData: ConversationMessageSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        userId: ['', Validators.required],
        parentConversationMessageId: [null],
        conversationChannelId: [null],
        dateTimeCreated: ['', Validators.required],
        message: ['', Validators.required],
        messageType: [''],
        entity: [''],
        entityId: [''],
        externalURL: [''],
        forwardedFromMessageId: [''],
        forwardedFromUserId: [''],
        isScheduled: [false],
        scheduledDateTime: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();
  conversations$ = this.conversationService.GetConversationList();
  conversationChannels$ = this.conversationChannelService.GetConversationChannelList();

  constructor(
    private modalService: NgbModal,
    private conversationMessageService: ConversationMessageService,
    private conversationService: ConversationService,
    private conversationChannelService: ConversationChannelService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationMessageData?: ConversationMessageData) {

    if (conversationMessageData != null) {

      if (!this.conversationMessageService.userIsSchedulerConversationMessageReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Messages`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationMessageSubmitData = this.conversationMessageService.ConvertToConversationMessageSubmitData(conversationMessageData);
      this.isEditMode = true;
      this.objectGuid = conversationMessageData.objectGuid;

      this.buildFormValues(conversationMessageData);

    } else {

      if (!this.conversationMessageService.userIsSchedulerConversationMessageWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Messages`,
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
        this.conversationMessageForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationMessageModal, {
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

    if (this.conversationMessageService.userIsSchedulerConversationMessageWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Messages`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationMessageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageSubmitData: ConversationMessageSubmitData = {
        id: this.conversationMessageSubmitData?.id || 0,
        conversationId: Number(formValue.conversationId),
        userId: Number(formValue.userId),
        parentConversationMessageId: formValue.parentConversationMessageId ? Number(formValue.parentConversationMessageId) : null,
        conversationChannelId: formValue.conversationChannelId ? Number(formValue.conversationChannelId) : null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        message: formValue.message!.trim(),
        messageType: formValue.messageType?.trim() || null,
        entity: formValue.entity?.trim() || null,
        entityId: formValue.entityId ? Number(formValue.entityId) : null,
        externalURL: formValue.externalURL?.trim() || null,
        forwardedFromMessageId: formValue.forwardedFromMessageId ? Number(formValue.forwardedFromMessageId) : null,
        forwardedFromUserId: formValue.forwardedFromUserId ? Number(formValue.forwardedFromUserId) : null,
        isScheduled: !!formValue.isScheduled,
        scheduledDateTime: formValue.scheduledDateTime ? dateTimeLocalToIsoUtc(formValue.scheduledDateTime.trim()) : null,
        versionNumber: this.conversationMessageSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationMessage(conversationMessageSubmitData);
      } else {
        this.addConversationMessage(conversationMessageSubmitData);
      }
  }

  private addConversationMessage(conversationMessageData: ConversationMessageSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationMessageData.versionNumber = 0;
    conversationMessageData.active = true;
    conversationMessageData.deleted = false;
    this.conversationMessageService.PostConversationMessage(conversationMessageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationMessage) => {

        this.conversationMessageService.ClearAllCaches();

        this.conversationMessageChanged.next([newConversationMessage]);

        this.alertService.showMessage("Conversation Message added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationmessage', newConversationMessage.id]);
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
                                   'You do not have permission to save this Conversation Message.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationMessage(conversationMessageData: ConversationMessageSubmitData) {
    this.conversationMessageService.PutConversationMessage(conversationMessageData.id, conversationMessageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationMessage) => {

        this.conversationMessageService.ClearAllCaches();

        this.conversationMessageChanged.next([updatedConversationMessage]);

        this.alertService.showMessage("Conversation Message updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Message.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationMessageData: ConversationMessageData | null) {

    if (conversationMessageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageForm.reset({
        conversationId: null,
        userId: '',
        parentConversationMessageId: null,
        conversationChannelId: null,
        dateTimeCreated: '',
        message: '',
        messageType: '',
        entity: '',
        entityId: '',
        externalURL: '',
        forwardedFromMessageId: '',
        forwardedFromUserId: '',
        isScheduled: false,
        scheduledDateTime: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageForm.reset({
        conversationId: conversationMessageData.conversationId,
        userId: conversationMessageData.userId?.toString() ?? '',
        parentConversationMessageId: conversationMessageData.parentConversationMessageId,
        conversationChannelId: conversationMessageData.conversationChannelId,
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationMessageData.dateTimeCreated) ?? '',
        message: conversationMessageData.message ?? '',
        messageType: conversationMessageData.messageType ?? '',
        entity: conversationMessageData.entity ?? '',
        entityId: conversationMessageData.entityId?.toString() ?? '',
        externalURL: conversationMessageData.externalURL ?? '',
        forwardedFromMessageId: conversationMessageData.forwardedFromMessageId?.toString() ?? '',
        forwardedFromUserId: conversationMessageData.forwardedFromUserId?.toString() ?? '',
        isScheduled: conversationMessageData.isScheduled ?? false,
        scheduledDateTime: isoUtcStringToDateTimeLocal(conversationMessageData.scheduledDateTime) ?? '',
        versionNumber: conversationMessageData.versionNumber?.toString() ?? '',
        active: conversationMessageData.active ?? true,
        deleted: conversationMessageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageForm.markAsPristine();
    this.conversationMessageForm.markAsUntouched();
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


  public userIsSchedulerConversationMessageReader(): boolean {
    return this.conversationMessageService.userIsSchedulerConversationMessageReader();
  }

  public userIsSchedulerConversationMessageWriter(): boolean {
    return this.conversationMessageService.userIsSchedulerConversationMessageWriter();
  }
}

/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGEREACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessageReaction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-reaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageReactionService, ConversationMessageReactionData, ConversationMessageReactionSubmitData } from '../../../scheduler-data-services/conversation-message-reaction.service';
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
interface ConversationMessageReactionFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  reaction: string,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-message-reaction-add-edit',
  templateUrl: './conversation-message-reaction-add-edit.component.html',
  styleUrls: ['./conversation-message-reaction-add-edit.component.scss']
})
export class ConversationMessageReactionAddEditComponent {
  @ViewChild('conversationMessageReactionModal') conversationMessageReactionModal!: TemplateRef<any>;
  @Output() conversationMessageReactionChanged = new Subject<ConversationMessageReactionData[]>();
  @Input() conversationMessageReactionSubmitData: ConversationMessageReactionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageReactionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageReactionForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        userId: ['', Validators.required],
        reaction: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationMessageReactions$ = this.conversationMessageReactionService.GetConversationMessageReactionList();
  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  constructor(
    private modalService: NgbModal,
    private conversationMessageReactionService: ConversationMessageReactionService,
    private conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationMessageReactionData?: ConversationMessageReactionData) {

    if (conversationMessageReactionData != null) {

      if (!this.conversationMessageReactionService.userIsSchedulerConversationMessageReactionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Message Reactions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationMessageReactionSubmitData = this.conversationMessageReactionService.ConvertToConversationMessageReactionSubmitData(conversationMessageReactionData);
      this.isEditMode = true;
      this.objectGuid = conversationMessageReactionData.objectGuid;

      this.buildFormValues(conversationMessageReactionData);

    } else {

      if (!this.conversationMessageReactionService.userIsSchedulerConversationMessageReactionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Reactions`,
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
        this.conversationMessageReactionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageReactionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationMessageReactionModal, {
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

    if (this.conversationMessageReactionService.userIsSchedulerConversationMessageReactionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Reactions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationMessageReactionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageReactionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageReactionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageReactionSubmitData: ConversationMessageReactionSubmitData = {
        id: this.conversationMessageReactionSubmitData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        userId: Number(formValue.userId),
        reaction: formValue.reaction!.trim(),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationMessageReaction(conversationMessageReactionSubmitData);
      } else {
        this.addConversationMessageReaction(conversationMessageReactionSubmitData);
      }
  }

  private addConversationMessageReaction(conversationMessageReactionData: ConversationMessageReactionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationMessageReactionData.active = true;
    conversationMessageReactionData.deleted = false;
    this.conversationMessageReactionService.PostConversationMessageReaction(conversationMessageReactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationMessageReaction) => {

        this.conversationMessageReactionService.ClearAllCaches();

        this.conversationMessageReactionChanged.next([newConversationMessageReaction]);

        this.alertService.showMessage("Conversation Message Reaction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationmessagereaction', newConversationMessageReaction.id]);
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
                                   'You do not have permission to save this Conversation Message Reaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Reaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Reaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationMessageReaction(conversationMessageReactionData: ConversationMessageReactionSubmitData) {
    this.conversationMessageReactionService.PutConversationMessageReaction(conversationMessageReactionData.id, conversationMessageReactionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationMessageReaction) => {

        this.conversationMessageReactionService.ClearAllCaches();

        this.conversationMessageReactionChanged.next([updatedConversationMessageReaction]);

        this.alertService.showMessage("Conversation Message Reaction updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Message Reaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Reaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Reaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationMessageReactionData: ConversationMessageReactionData | null) {

    if (conversationMessageReactionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageReactionForm.reset({
        conversationMessageId: null,
        userId: '',
        reaction: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageReactionForm.reset({
        conversationMessageId: conversationMessageReactionData.conversationMessageId,
        userId: conversationMessageReactionData.userId?.toString() ?? '',
        reaction: conversationMessageReactionData.reaction ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationMessageReactionData.dateTimeCreated) ?? '',
        active: conversationMessageReactionData.active ?? true,
        deleted: conversationMessageReactionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageReactionForm.markAsPristine();
    this.conversationMessageReactionForm.markAsUntouched();
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


  public userIsSchedulerConversationMessageReactionReader(): boolean {
    return this.conversationMessageReactionService.userIsSchedulerConversationMessageReactionReader();
  }

  public userIsSchedulerConversationMessageReactionWriter(): boolean {
    return this.conversationMessageReactionService.userIsSchedulerConversationMessageReactionWriter();
  }
}

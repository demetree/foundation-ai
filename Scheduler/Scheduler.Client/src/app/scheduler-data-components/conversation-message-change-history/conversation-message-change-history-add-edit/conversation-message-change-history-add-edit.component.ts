/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessageChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageChangeHistoryService, ConversationMessageChangeHistoryData, ConversationMessageChangeHistorySubmitData } from '../../../scheduler-data-services/conversation-message-change-history.service';
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
interface ConversationMessageChangeHistoryFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-conversation-message-change-history-add-edit',
  templateUrl: './conversation-message-change-history-add-edit.component.html',
  styleUrls: ['./conversation-message-change-history-add-edit.component.scss']
})
export class ConversationMessageChangeHistoryAddEditComponent {
  @ViewChild('conversationMessageChangeHistoryModal') conversationMessageChangeHistoryModal!: TemplateRef<any>;
  @Output() conversationMessageChangeHistoryChanged = new Subject<ConversationMessageChangeHistoryData[]>();
  @Input() conversationMessageChangeHistorySubmitData: ConversationMessageChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageChangeHistoryForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationMessageChangeHistories$ = this.conversationMessageChangeHistoryService.GetConversationMessageChangeHistoryList();
  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  constructor(
    private modalService: NgbModal,
    private conversationMessageChangeHistoryService: ConversationMessageChangeHistoryService,
    private conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationMessageChangeHistoryData?: ConversationMessageChangeHistoryData) {

    if (conversationMessageChangeHistoryData != null) {

      if (!this.conversationMessageChangeHistoryService.userIsSchedulerConversationMessageChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Message Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationMessageChangeHistorySubmitData = this.conversationMessageChangeHistoryService.ConvertToConversationMessageChangeHistorySubmitData(conversationMessageChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(conversationMessageChangeHistoryData);

    } else {

      if (!this.conversationMessageChangeHistoryService.userIsSchedulerConversationMessageChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Change Histories`,
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
        this.conversationMessageChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationMessageChangeHistoryModal, {
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

    if (this.conversationMessageChangeHistoryService.userIsSchedulerConversationMessageChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationMessageChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageChangeHistorySubmitData: ConversationMessageChangeHistorySubmitData = {
        id: this.conversationMessageChangeHistorySubmitData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        versionNumber: this.conversationMessageChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateConversationMessageChangeHistory(conversationMessageChangeHistorySubmitData);
      } else {
        this.addConversationMessageChangeHistory(conversationMessageChangeHistorySubmitData);
      }
  }

  private addConversationMessageChangeHistory(conversationMessageChangeHistoryData: ConversationMessageChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationMessageChangeHistoryData.versionNumber = 0;
    this.conversationMessageChangeHistoryService.PostConversationMessageChangeHistory(conversationMessageChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationMessageChangeHistory) => {

        this.conversationMessageChangeHistoryService.ClearAllCaches();

        this.conversationMessageChangeHistoryChanged.next([newConversationMessageChangeHistory]);

        this.alertService.showMessage("Conversation Message Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationmessagechangehistory', newConversationMessageChangeHistory.id]);
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
                                   'You do not have permission to save this Conversation Message Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationMessageChangeHistory(conversationMessageChangeHistoryData: ConversationMessageChangeHistorySubmitData) {
    this.conversationMessageChangeHistoryService.PutConversationMessageChangeHistory(conversationMessageChangeHistoryData.id, conversationMessageChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationMessageChangeHistory) => {

        this.conversationMessageChangeHistoryService.ClearAllCaches();

        this.conversationMessageChangeHistoryChanged.next([updatedConversationMessageChangeHistory]);

        this.alertService.showMessage("Conversation Message Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Message Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationMessageChangeHistoryData: ConversationMessageChangeHistoryData | null) {

    if (conversationMessageChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageChangeHistoryForm.reset({
        conversationMessageId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageChangeHistoryForm.reset({
        conversationMessageId: conversationMessageChangeHistoryData.conversationMessageId,
        versionNumber: conversationMessageChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(conversationMessageChangeHistoryData.timeStamp) ?? '',
        userId: conversationMessageChangeHistoryData.userId?.toString() ?? '',
        data: conversationMessageChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.conversationMessageChangeHistoryForm.markAsPristine();
    this.conversationMessageChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerConversationMessageChangeHistoryReader(): boolean {
    return this.conversationMessageChangeHistoryService.userIsSchedulerConversationMessageChangeHistoryReader();
  }

  public userIsSchedulerConversationMessageChangeHistoryWriter(): boolean {
    return this.conversationMessageChangeHistoryService.userIsSchedulerConversationMessageChangeHistoryWriter();
  }
}

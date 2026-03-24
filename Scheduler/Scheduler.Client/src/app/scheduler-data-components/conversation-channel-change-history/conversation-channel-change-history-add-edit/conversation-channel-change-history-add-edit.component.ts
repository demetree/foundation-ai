/*
   GENERATED FORM FOR THE CONVERSATIONCHANNELCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationChannelChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-channel-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationChannelChangeHistoryService, ConversationChannelChangeHistoryData, ConversationChannelChangeHistorySubmitData } from '../../../scheduler-data-services/conversation-channel-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConversationChannelService } from '../../../scheduler-data-services/conversation-channel.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConversationChannelChangeHistoryFormValues {
  conversationChannelId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-conversation-channel-change-history-add-edit',
  templateUrl: './conversation-channel-change-history-add-edit.component.html',
  styleUrls: ['./conversation-channel-change-history-add-edit.component.scss']
})
export class ConversationChannelChangeHistoryAddEditComponent {
  @ViewChild('conversationChannelChangeHistoryModal') conversationChannelChangeHistoryModal!: TemplateRef<any>;
  @Output() conversationChannelChangeHistoryChanged = new Subject<ConversationChannelChangeHistoryData[]>();
  @Input() conversationChannelChangeHistorySubmitData: ConversationChannelChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationChannelChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationChannelChangeHistoryForm: FormGroup = this.fb.group({
        conversationChannelId: [null, Validators.required],
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

  conversationChannelChangeHistories$ = this.conversationChannelChangeHistoryService.GetConversationChannelChangeHistoryList();
  conversationChannels$ = this.conversationChannelService.GetConversationChannelList();

  constructor(
    private modalService: NgbModal,
    private conversationChannelChangeHistoryService: ConversationChannelChangeHistoryService,
    private conversationChannelService: ConversationChannelService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationChannelChangeHistoryData?: ConversationChannelChangeHistoryData) {

    if (conversationChannelChangeHistoryData != null) {

      if (!this.conversationChannelChangeHistoryService.userIsSchedulerConversationChannelChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Channel Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationChannelChangeHistorySubmitData = this.conversationChannelChangeHistoryService.ConvertToConversationChannelChangeHistorySubmitData(conversationChannelChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(conversationChannelChangeHistoryData);

    } else {

      if (!this.conversationChannelChangeHistoryService.userIsSchedulerConversationChannelChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Channel Change Histories`,
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
        this.conversationChannelChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationChannelChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationChannelChangeHistoryModal, {
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

    if (this.conversationChannelChangeHistoryService.userIsSchedulerConversationChannelChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Channel Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationChannelChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationChannelChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationChannelChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationChannelChangeHistorySubmitData: ConversationChannelChangeHistorySubmitData = {
        id: this.conversationChannelChangeHistorySubmitData?.id || 0,
        conversationChannelId: Number(formValue.conversationChannelId),
        versionNumber: this.conversationChannelChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateConversationChannelChangeHistory(conversationChannelChangeHistorySubmitData);
      } else {
        this.addConversationChannelChangeHistory(conversationChannelChangeHistorySubmitData);
      }
  }

  private addConversationChannelChangeHistory(conversationChannelChangeHistoryData: ConversationChannelChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationChannelChangeHistoryData.versionNumber = 0;
    this.conversationChannelChangeHistoryService.PostConversationChannelChangeHistory(conversationChannelChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationChannelChangeHistory) => {

        this.conversationChannelChangeHistoryService.ClearAllCaches();

        this.conversationChannelChangeHistoryChanged.next([newConversationChannelChangeHistory]);

        this.alertService.showMessage("Conversation Channel Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationchannelchangehistory', newConversationChannelChangeHistory.id]);
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
                                   'You do not have permission to save this Conversation Channel Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Channel Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Channel Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationChannelChangeHistory(conversationChannelChangeHistoryData: ConversationChannelChangeHistorySubmitData) {
    this.conversationChannelChangeHistoryService.PutConversationChannelChangeHistory(conversationChannelChangeHistoryData.id, conversationChannelChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationChannelChangeHistory) => {

        this.conversationChannelChangeHistoryService.ClearAllCaches();

        this.conversationChannelChangeHistoryChanged.next([updatedConversationChannelChangeHistory]);

        this.alertService.showMessage("Conversation Channel Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Channel Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Channel Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Channel Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationChannelChangeHistoryData: ConversationChannelChangeHistoryData | null) {

    if (conversationChannelChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationChannelChangeHistoryForm.reset({
        conversationChannelId: null,
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
        this.conversationChannelChangeHistoryForm.reset({
        conversationChannelId: conversationChannelChangeHistoryData.conversationChannelId,
        versionNumber: conversationChannelChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(conversationChannelChangeHistoryData.timeStamp) ?? '',
        userId: conversationChannelChangeHistoryData.userId?.toString() ?? '',
        data: conversationChannelChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.conversationChannelChangeHistoryForm.markAsPristine();
    this.conversationChannelChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerConversationChannelChangeHistoryReader(): boolean {
    return this.conversationChannelChangeHistoryService.userIsSchedulerConversationChannelChangeHistoryReader();
  }

  public userIsSchedulerConversationChannelChangeHistoryWriter(): boolean {
    return this.conversationChannelChangeHistoryService.userIsSchedulerConversationChannelChangeHistoryWriter();
  }
}

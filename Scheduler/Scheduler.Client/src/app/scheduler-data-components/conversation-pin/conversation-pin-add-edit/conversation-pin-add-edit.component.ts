/*
   GENERATED FORM FOR THE CONVERSATIONPIN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationPin table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-pin-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationPinService, ConversationPinData, ConversationPinSubmitData } from '../../../scheduler-data-services/conversation-pin.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConversationService } from '../../../scheduler-data-services/conversation.service';
import { ConversationMessageService } from '../../../scheduler-data-services/conversation-message.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConversationPinFormValues {
  conversationId: number | bigint,       // For FK link number
  conversationMessageId: number | bigint,       // For FK link number
  pinnedByUserId: string,     // Stored as string for form input, converted to number on submit.
  dateTimePinned: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-pin-add-edit',
  templateUrl: './conversation-pin-add-edit.component.html',
  styleUrls: ['./conversation-pin-add-edit.component.scss']
})
export class ConversationPinAddEditComponent {
  @ViewChild('conversationPinModal') conversationPinModal!: TemplateRef<any>;
  @Output() conversationPinChanged = new Subject<ConversationPinData[]>();
  @Input() conversationPinSubmitData: ConversationPinSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationPinFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationPinForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        conversationMessageId: [null, Validators.required],
        pinnedByUserId: ['', Validators.required],
        dateTimePinned: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationPins$ = this.conversationPinService.GetConversationPinList();
  conversations$ = this.conversationService.GetConversationList();
  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  constructor(
    private modalService: NgbModal,
    private conversationPinService: ConversationPinService,
    private conversationService: ConversationService,
    private conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationPinData?: ConversationPinData) {

    if (conversationPinData != null) {

      if (!this.conversationPinService.userIsSchedulerConversationPinReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Pins`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationPinSubmitData = this.conversationPinService.ConvertToConversationPinSubmitData(conversationPinData);
      this.isEditMode = true;
      this.objectGuid = conversationPinData.objectGuid;

      this.buildFormValues(conversationPinData);

    } else {

      if (!this.conversationPinService.userIsSchedulerConversationPinWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Pins`,
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
        this.conversationPinForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationPinForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationPinModal, {
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

    if (this.conversationPinService.userIsSchedulerConversationPinWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Pins`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationPinForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationPinForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationPinForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationPinSubmitData: ConversationPinSubmitData = {
        id: this.conversationPinSubmitData?.id || 0,
        conversationId: Number(formValue.conversationId),
        conversationMessageId: Number(formValue.conversationMessageId),
        pinnedByUserId: Number(formValue.pinnedByUserId),
        dateTimePinned: dateTimeLocalToIsoUtc(formValue.dateTimePinned!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationPin(conversationPinSubmitData);
      } else {
        this.addConversationPin(conversationPinSubmitData);
      }
  }

  private addConversationPin(conversationPinData: ConversationPinSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationPinData.active = true;
    conversationPinData.deleted = false;
    this.conversationPinService.PostConversationPin(conversationPinData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationPin) => {

        this.conversationPinService.ClearAllCaches();

        this.conversationPinChanged.next([newConversationPin]);

        this.alertService.showMessage("Conversation Pin added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationpin', newConversationPin.id]);
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
                                   'You do not have permission to save this Conversation Pin.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Pin.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Pin could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationPin(conversationPinData: ConversationPinSubmitData) {
    this.conversationPinService.PutConversationPin(conversationPinData.id, conversationPinData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationPin) => {

        this.conversationPinService.ClearAllCaches();

        this.conversationPinChanged.next([updatedConversationPin]);

        this.alertService.showMessage("Conversation Pin updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Pin.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Pin.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Pin could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationPinData: ConversationPinData | null) {

    if (conversationPinData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationPinForm.reset({
        conversationId: null,
        conversationMessageId: null,
        pinnedByUserId: '',
        dateTimePinned: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationPinForm.reset({
        conversationId: conversationPinData.conversationId,
        conversationMessageId: conversationPinData.conversationMessageId,
        pinnedByUserId: conversationPinData.pinnedByUserId?.toString() ?? '',
        dateTimePinned: isoUtcStringToDateTimeLocal(conversationPinData.dateTimePinned) ?? '',
        active: conversationPinData.active ?? true,
        deleted: conversationPinData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationPinForm.markAsPristine();
    this.conversationPinForm.markAsUntouched();
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


  public userIsSchedulerConversationPinReader(): boolean {
    return this.conversationPinService.userIsSchedulerConversationPinReader();
  }

  public userIsSchedulerConversationPinWriter(): boolean {
    return this.conversationPinService.userIsSchedulerConversationPinWriter();
  }
}

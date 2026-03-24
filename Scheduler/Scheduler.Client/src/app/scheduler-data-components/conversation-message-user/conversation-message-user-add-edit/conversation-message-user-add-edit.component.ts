/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGEUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessageUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageUserService, ConversationMessageUserData, ConversationMessageUserSubmitData } from '../../../scheduler-data-services/conversation-message-user.service';
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
interface ConversationMessageUserFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  dateTimeCreated: string,
  acknowledged: boolean,
  dateTimeAcknowledged: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-message-user-add-edit',
  templateUrl: './conversation-message-user-add-edit.component.html',
  styleUrls: ['./conversation-message-user-add-edit.component.scss']
})
export class ConversationMessageUserAddEditComponent {
  @ViewChild('conversationMessageUserModal') conversationMessageUserModal!: TemplateRef<any>;
  @Output() conversationMessageUserChanged = new Subject<ConversationMessageUserData[]>();
  @Input() conversationMessageUserSubmitData: ConversationMessageUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageUserForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        userId: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        acknowledged: [false],
        dateTimeAcknowledged: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationMessageUsers$ = this.conversationMessageUserService.GetConversationMessageUserList();
  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  constructor(
    private modalService: NgbModal,
    private conversationMessageUserService: ConversationMessageUserService,
    private conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationMessageUserData?: ConversationMessageUserData) {

    if (conversationMessageUserData != null) {

      if (!this.conversationMessageUserService.userIsSchedulerConversationMessageUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Message Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationMessageUserSubmitData = this.conversationMessageUserService.ConvertToConversationMessageUserSubmitData(conversationMessageUserData);
      this.isEditMode = true;
      this.objectGuid = conversationMessageUserData.objectGuid;

      this.buildFormValues(conversationMessageUserData);

    } else {

      if (!this.conversationMessageUserService.userIsSchedulerConversationMessageUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Users`,
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
        this.conversationMessageUserForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationMessageUserModal, {
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

    if (this.conversationMessageUserService.userIsSchedulerConversationMessageUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationMessageUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageUserSubmitData: ConversationMessageUserSubmitData = {
        id: this.conversationMessageUserSubmitData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        userId: Number(formValue.userId),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        acknowledged: !!formValue.acknowledged,
        dateTimeAcknowledged: dateTimeLocalToIsoUtc(formValue.dateTimeAcknowledged!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationMessageUser(conversationMessageUserSubmitData);
      } else {
        this.addConversationMessageUser(conversationMessageUserSubmitData);
      }
  }

  private addConversationMessageUser(conversationMessageUserData: ConversationMessageUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationMessageUserData.active = true;
    conversationMessageUserData.deleted = false;
    this.conversationMessageUserService.PostConversationMessageUser(conversationMessageUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationMessageUser) => {

        this.conversationMessageUserService.ClearAllCaches();

        this.conversationMessageUserChanged.next([newConversationMessageUser]);

        this.alertService.showMessage("Conversation Message User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationmessageuser', newConversationMessageUser.id]);
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
                                   'You do not have permission to save this Conversation Message User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationMessageUser(conversationMessageUserData: ConversationMessageUserSubmitData) {
    this.conversationMessageUserService.PutConversationMessageUser(conversationMessageUserData.id, conversationMessageUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationMessageUser) => {

        this.conversationMessageUserService.ClearAllCaches();

        this.conversationMessageUserChanged.next([updatedConversationMessageUser]);

        this.alertService.showMessage("Conversation Message User updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Message User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationMessageUserData: ConversationMessageUserData | null) {

    if (conversationMessageUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageUserForm.reset({
        conversationMessageId: null,
        userId: '',
        dateTimeCreated: '',
        acknowledged: false,
        dateTimeAcknowledged: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageUserForm.reset({
        conversationMessageId: conversationMessageUserData.conversationMessageId,
        userId: conversationMessageUserData.userId?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationMessageUserData.dateTimeCreated) ?? '',
        acknowledged: conversationMessageUserData.acknowledged ?? false,
        dateTimeAcknowledged: isoUtcStringToDateTimeLocal(conversationMessageUserData.dateTimeAcknowledged) ?? '',
        active: conversationMessageUserData.active ?? true,
        deleted: conversationMessageUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageUserForm.markAsPristine();
    this.conversationMessageUserForm.markAsUntouched();
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


  public userIsSchedulerConversationMessageUserReader(): boolean {
    return this.conversationMessageUserService.userIsSchedulerConversationMessageUserReader();
  }

  public userIsSchedulerConversationMessageUserWriter(): boolean {
    return this.conversationMessageUserService.userIsSchedulerConversationMessageUserWriter();
  }
}

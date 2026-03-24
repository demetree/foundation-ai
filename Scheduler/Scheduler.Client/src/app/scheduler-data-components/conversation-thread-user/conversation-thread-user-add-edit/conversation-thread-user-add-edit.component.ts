/*
   GENERATED FORM FOR THE CONVERSATIONTHREADUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationThreadUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-thread-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationThreadUserService, ConversationThreadUserData, ConversationThreadUserSubmitData } from '../../../scheduler-data-services/conversation-thread-user.service';
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
interface ConversationThreadUserFormValues {
  conversationId: number | bigint,       // For FK link number
  parentConversationMessageId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  lastReadMessageId: string | null,     // Stored as string for form input, converted to number on submit.
  lastReadDateTime: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-thread-user-add-edit',
  templateUrl: './conversation-thread-user-add-edit.component.html',
  styleUrls: ['./conversation-thread-user-add-edit.component.scss']
})
export class ConversationThreadUserAddEditComponent {
  @ViewChild('conversationThreadUserModal') conversationThreadUserModal!: TemplateRef<any>;
  @Output() conversationThreadUserChanged = new Subject<ConversationThreadUserData[]>();
  @Input() conversationThreadUserSubmitData: ConversationThreadUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationThreadUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationThreadUserForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        parentConversationMessageId: [null, Validators.required],
        userId: ['', Validators.required],
        lastReadMessageId: [''],
        lastReadDateTime: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationThreadUsers$ = this.conversationThreadUserService.GetConversationThreadUserList();
  conversations$ = this.conversationService.GetConversationList();
  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  constructor(
    private modalService: NgbModal,
    private conversationThreadUserService: ConversationThreadUserService,
    private conversationService: ConversationService,
    private conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationThreadUserData?: ConversationThreadUserData) {

    if (conversationThreadUserData != null) {

      if (!this.conversationThreadUserService.userIsSchedulerConversationThreadUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Thread Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationThreadUserSubmitData = this.conversationThreadUserService.ConvertToConversationThreadUserSubmitData(conversationThreadUserData);
      this.isEditMode = true;
      this.objectGuid = conversationThreadUserData.objectGuid;

      this.buildFormValues(conversationThreadUserData);

    } else {

      if (!this.conversationThreadUserService.userIsSchedulerConversationThreadUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Thread Users`,
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
        this.conversationThreadUserForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationThreadUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationThreadUserModal, {
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

    if (this.conversationThreadUserService.userIsSchedulerConversationThreadUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Thread Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationThreadUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationThreadUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationThreadUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationThreadUserSubmitData: ConversationThreadUserSubmitData = {
        id: this.conversationThreadUserSubmitData?.id || 0,
        conversationId: Number(formValue.conversationId),
        parentConversationMessageId: Number(formValue.parentConversationMessageId),
        userId: Number(formValue.userId),
        lastReadMessageId: formValue.lastReadMessageId ? Number(formValue.lastReadMessageId) : null,
        lastReadDateTime: formValue.lastReadDateTime ? dateTimeLocalToIsoUtc(formValue.lastReadDateTime.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationThreadUser(conversationThreadUserSubmitData);
      } else {
        this.addConversationThreadUser(conversationThreadUserSubmitData);
      }
  }

  private addConversationThreadUser(conversationThreadUserData: ConversationThreadUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationThreadUserData.active = true;
    conversationThreadUserData.deleted = false;
    this.conversationThreadUserService.PostConversationThreadUser(conversationThreadUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationThreadUser) => {

        this.conversationThreadUserService.ClearAllCaches();

        this.conversationThreadUserChanged.next([newConversationThreadUser]);

        this.alertService.showMessage("Conversation Thread User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationthreaduser', newConversationThreadUser.id]);
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
                                   'You do not have permission to save this Conversation Thread User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Thread User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Thread User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationThreadUser(conversationThreadUserData: ConversationThreadUserSubmitData) {
    this.conversationThreadUserService.PutConversationThreadUser(conversationThreadUserData.id, conversationThreadUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationThreadUser) => {

        this.conversationThreadUserService.ClearAllCaches();

        this.conversationThreadUserChanged.next([updatedConversationThreadUser]);

        this.alertService.showMessage("Conversation Thread User updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Thread User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Thread User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Thread User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationThreadUserData: ConversationThreadUserData | null) {

    if (conversationThreadUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationThreadUserForm.reset({
        conversationId: null,
        parentConversationMessageId: null,
        userId: '',
        lastReadMessageId: '',
        lastReadDateTime: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationThreadUserForm.reset({
        conversationId: conversationThreadUserData.conversationId,
        parentConversationMessageId: conversationThreadUserData.parentConversationMessageId,
        userId: conversationThreadUserData.userId?.toString() ?? '',
        lastReadMessageId: conversationThreadUserData.lastReadMessageId?.toString() ?? '',
        lastReadDateTime: isoUtcStringToDateTimeLocal(conversationThreadUserData.lastReadDateTime) ?? '',
        active: conversationThreadUserData.active ?? true,
        deleted: conversationThreadUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationThreadUserForm.markAsPristine();
    this.conversationThreadUserForm.markAsUntouched();
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


  public userIsSchedulerConversationThreadUserReader(): boolean {
    return this.conversationThreadUserService.userIsSchedulerConversationThreadUserReader();
  }

  public userIsSchedulerConversationThreadUserWriter(): boolean {
    return this.conversationThreadUserService.userIsSchedulerConversationThreadUserWriter();
  }
}

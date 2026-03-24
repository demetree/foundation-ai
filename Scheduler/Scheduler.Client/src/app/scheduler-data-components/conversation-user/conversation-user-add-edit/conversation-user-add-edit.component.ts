/*
   GENERATED FORM FOR THE CONVERSATIONUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationUserService, ConversationUserData, ConversationUserSubmitData } from '../../../scheduler-data-services/conversation-user.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConversationService } from '../../../scheduler-data-services/conversation.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConversationUserFormValues {
  conversationId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  role: string,
  dateTimeAdded: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-user-add-edit',
  templateUrl: './conversation-user-add-edit.component.html',
  styleUrls: ['./conversation-user-add-edit.component.scss']
})
export class ConversationUserAddEditComponent {
  @ViewChild('conversationUserModal') conversationUserModal!: TemplateRef<any>;
  @Output() conversationUserChanged = new Subject<ConversationUserData[]>();
  @Input() conversationUserSubmitData: ConversationUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationUserForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        userId: ['', Validators.required],
        role: ['', Validators.required],
        dateTimeAdded: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationUsers$ = this.conversationUserService.GetConversationUserList();
  conversations$ = this.conversationService.GetConversationList();

  constructor(
    private modalService: NgbModal,
    private conversationUserService: ConversationUserService,
    private conversationService: ConversationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationUserData?: ConversationUserData) {

    if (conversationUserData != null) {

      if (!this.conversationUserService.userIsSchedulerConversationUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationUserSubmitData = this.conversationUserService.ConvertToConversationUserSubmitData(conversationUserData);
      this.isEditMode = true;
      this.objectGuid = conversationUserData.objectGuid;

      this.buildFormValues(conversationUserData);

    } else {

      if (!this.conversationUserService.userIsSchedulerConversationUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Users`,
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
        this.conversationUserForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationUserModal, {
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

    if (this.conversationUserService.userIsSchedulerConversationUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationUserSubmitData: ConversationUserSubmitData = {
        id: this.conversationUserSubmitData?.id || 0,
        conversationId: Number(formValue.conversationId),
        userId: Number(formValue.userId),
        role: formValue.role!.trim(),
        dateTimeAdded: dateTimeLocalToIsoUtc(formValue.dateTimeAdded!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationUser(conversationUserSubmitData);
      } else {
        this.addConversationUser(conversationUserSubmitData);
      }
  }

  private addConversationUser(conversationUserData: ConversationUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationUserData.active = true;
    conversationUserData.deleted = false;
    this.conversationUserService.PostConversationUser(conversationUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationUser) => {

        this.conversationUserService.ClearAllCaches();

        this.conversationUserChanged.next([newConversationUser]);

        this.alertService.showMessage("Conversation User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationuser', newConversationUser.id]);
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
                                   'You do not have permission to save this Conversation User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationUser(conversationUserData: ConversationUserSubmitData) {
    this.conversationUserService.PutConversationUser(conversationUserData.id, conversationUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationUser) => {

        this.conversationUserService.ClearAllCaches();

        this.conversationUserChanged.next([updatedConversationUser]);

        this.alertService.showMessage("Conversation User updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationUserData: ConversationUserData | null) {

    if (conversationUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationUserForm.reset({
        conversationId: null,
        userId: '',
        role: '',
        dateTimeAdded: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationUserForm.reset({
        conversationId: conversationUserData.conversationId,
        userId: conversationUserData.userId?.toString() ?? '',
        role: conversationUserData.role ?? '',
        dateTimeAdded: isoUtcStringToDateTimeLocal(conversationUserData.dateTimeAdded) ?? '',
        active: conversationUserData.active ?? true,
        deleted: conversationUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationUserForm.markAsPristine();
    this.conversationUserForm.markAsUntouched();
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


  public userIsSchedulerConversationUserReader(): boolean {
    return this.conversationUserService.userIsSchedulerConversationUserReader();
  }

  public userIsSchedulerConversationUserWriter(): boolean {
    return this.conversationUserService.userIsSchedulerConversationUserWriter();
  }
}

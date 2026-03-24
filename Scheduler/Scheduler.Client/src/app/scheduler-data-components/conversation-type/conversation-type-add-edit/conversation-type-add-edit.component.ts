/*
   GENERATED FORM FOR THE CONVERSATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationTypeService, ConversationTypeData, ConversationTypeSubmitData } from '../../../scheduler-data-services/conversation-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConversationTypeFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-type-add-edit',
  templateUrl: './conversation-type-add-edit.component.html',
  styleUrls: ['./conversation-type-add-edit.component.scss']
})
export class ConversationTypeAddEditComponent {
  @ViewChild('conversationTypeModal') conversationTypeModal!: TemplateRef<any>;
  @Output() conversationTypeChanged = new Subject<ConversationTypeData[]>();
  @Input() conversationTypeSubmitData: ConversationTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationTypes$ = this.conversationTypeService.GetConversationTypeList();

  constructor(
    private modalService: NgbModal,
    private conversationTypeService: ConversationTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationTypeData?: ConversationTypeData) {

    if (conversationTypeData != null) {

      if (!this.conversationTypeService.userIsSchedulerConversationTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationTypeSubmitData = this.conversationTypeService.ConvertToConversationTypeSubmitData(conversationTypeData);
      this.isEditMode = true;
      this.objectGuid = conversationTypeData.objectGuid;

      this.buildFormValues(conversationTypeData);

    } else {

      if (!this.conversationTypeService.userIsSchedulerConversationTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Types`,
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
        this.conversationTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationTypeModal, {
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

    if (this.conversationTypeService.userIsSchedulerConversationTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationTypeSubmitData: ConversationTypeSubmitData = {
        id: this.conversationTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationType(conversationTypeSubmitData);
      } else {
        this.addConversationType(conversationTypeSubmitData);
      }
  }

  private addConversationType(conversationTypeData: ConversationTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationTypeData.active = true;
    conversationTypeData.deleted = false;
    this.conversationTypeService.PostConversationType(conversationTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationType) => {

        this.conversationTypeService.ClearAllCaches();

        this.conversationTypeChanged.next([newConversationType]);

        this.alertService.showMessage("Conversation Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationtype', newConversationType.id]);
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
                                   'You do not have permission to save this Conversation Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationType(conversationTypeData: ConversationTypeSubmitData) {
    this.conversationTypeService.PutConversationType(conversationTypeData.id, conversationTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationType) => {

        this.conversationTypeService.ClearAllCaches();

        this.conversationTypeChanged.next([updatedConversationType]);

        this.alertService.showMessage("Conversation Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationTypeData: ConversationTypeData | null) {

    if (conversationTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationTypeForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationTypeForm.reset({
        name: conversationTypeData.name ?? '',
        description: conversationTypeData.description ?? '',
        active: conversationTypeData.active ?? true,
        deleted: conversationTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationTypeForm.markAsPristine();
    this.conversationTypeForm.markAsUntouched();
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


  public userIsSchedulerConversationTypeReader(): boolean {
    return this.conversationTypeService.userIsSchedulerConversationTypeReader();
  }

  public userIsSchedulerConversationTypeWriter(): boolean {
    return this.conversationTypeService.userIsSchedulerConversationTypeWriter();
  }
}

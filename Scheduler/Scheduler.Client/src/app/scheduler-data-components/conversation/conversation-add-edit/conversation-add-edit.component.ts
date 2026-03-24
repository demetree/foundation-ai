/*
   GENERATED FORM FOR THE CONVERSATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Conversation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationService, ConversationData, ConversationSubmitData } from '../../../scheduler-data-services/conversation.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConversationTypeService } from '../../../scheduler-data-services/conversation-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConversationFormValues {
  createdByUserId: string | null,     // Stored as string for form input, converted to number on submit.
  conversationTypeId: number | bigint | null,       // For FK link number
  priority: string,     // Stored as string for form input, converted to number on submit.
  dateTimeCreated: string,
  entity: string | null,
  entityId: string | null,     // Stored as string for form input, converted to number on submit.
  externalURL: string | null,
  name: string | null,
  description: string | null,
  isPublic: boolean | null,
  userId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-add-edit',
  templateUrl: './conversation-add-edit.component.html',
  styleUrls: ['./conversation-add-edit.component.scss']
})
export class ConversationAddEditComponent {
  @ViewChild('conversationModal') conversationModal!: TemplateRef<any>;
  @Output() conversationChanged = new Subject<ConversationData[]>();
  @Input() conversationSubmitData: ConversationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationForm: FormGroup = this.fb.group({
        createdByUserId: [''],
        conversationTypeId: [null],
        priority: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        entity: [''],
        entityId: [''],
        externalURL: [''],
        name: [''],
        description: [''],
        isPublic: [false],
        userId: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversations$ = this.conversationService.GetConversationList();
  conversationTypes$ = this.conversationTypeService.GetConversationTypeList();

  constructor(
    private modalService: NgbModal,
    private conversationService: ConversationService,
    private conversationTypeService: ConversationTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationData?: ConversationData) {

    if (conversationData != null) {

      if (!this.conversationService.userIsSchedulerConversationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationSubmitData = this.conversationService.ConvertToConversationSubmitData(conversationData);
      this.isEditMode = true;
      this.objectGuid = conversationData.objectGuid;

      this.buildFormValues(conversationData);

    } else {

      if (!this.conversationService.userIsSchedulerConversationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversations`,
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
        this.conversationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationModal, {
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

    if (this.conversationService.userIsSchedulerConversationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationSubmitData: ConversationSubmitData = {
        id: this.conversationSubmitData?.id || 0,
        createdByUserId: formValue.createdByUserId ? Number(formValue.createdByUserId) : null,
        conversationTypeId: formValue.conversationTypeId ? Number(formValue.conversationTypeId) : null,
        priority: Number(formValue.priority),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        entity: formValue.entity?.trim() || null,
        entityId: formValue.entityId ? Number(formValue.entityId) : null,
        externalURL: formValue.externalURL?.trim() || null,
        name: formValue.name?.trim() || null,
        description: formValue.description?.trim() || null,
        isPublic: formValue.isPublic == true ? true : formValue.isPublic == false ? false : null,
        userId: formValue.userId ? Number(formValue.userId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversation(conversationSubmitData);
      } else {
        this.addConversation(conversationSubmitData);
      }
  }

  private addConversation(conversationData: ConversationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationData.active = true;
    conversationData.deleted = false;
    this.conversationService.PostConversation(conversationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversation) => {

        this.conversationService.ClearAllCaches();

        this.conversationChanged.next([newConversation]);

        this.alertService.showMessage("Conversation added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversation', newConversation.id]);
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
                                   'You do not have permission to save this Conversation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversation(conversationData: ConversationSubmitData) {
    this.conversationService.PutConversation(conversationData.id, conversationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversation) => {

        this.conversationService.ClearAllCaches();

        this.conversationChanged.next([updatedConversation]);

        this.alertService.showMessage("Conversation updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationData: ConversationData | null) {

    if (conversationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationForm.reset({
        createdByUserId: '',
        conversationTypeId: null,
        priority: '',
        dateTimeCreated: '',
        entity: '',
        entityId: '',
        externalURL: '',
        name: '',
        description: '',
        isPublic: false,
        userId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationForm.reset({
        createdByUserId: conversationData.createdByUserId?.toString() ?? '',
        conversationTypeId: conversationData.conversationTypeId,
        priority: conversationData.priority?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationData.dateTimeCreated) ?? '',
        entity: conversationData.entity ?? '',
        entityId: conversationData.entityId?.toString() ?? '',
        externalURL: conversationData.externalURL ?? '',
        name: conversationData.name ?? '',
        description: conversationData.description ?? '',
        isPublic: conversationData.isPublic ?? false,
        userId: conversationData.userId?.toString() ?? '',
        active: conversationData.active ?? true,
        deleted: conversationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationForm.markAsPristine();
    this.conversationForm.markAsUntouched();
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


  public userIsSchedulerConversationReader(): boolean {
    return this.conversationService.userIsSchedulerConversationReader();
  }

  public userIsSchedulerConversationWriter(): boolean {
    return this.conversationService.userIsSchedulerConversationWriter();
  }
}

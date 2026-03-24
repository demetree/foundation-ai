/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGELINKPREVIEW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessageLinkPreview table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-link-preview-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageLinkPreviewService, ConversationMessageLinkPreviewData, ConversationMessageLinkPreviewSubmitData } from '../../../scheduler-data-services/conversation-message-link-preview.service';
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
interface ConversationMessageLinkPreviewFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  url: string,
  title: string | null,
  description: string | null,
  imageUrl: string | null,
  siteName: string | null,
  fetchedDateTime: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-conversation-message-link-preview-add-edit',
  templateUrl: './conversation-message-link-preview-add-edit.component.html',
  styleUrls: ['./conversation-message-link-preview-add-edit.component.scss']
})
export class ConversationMessageLinkPreviewAddEditComponent {
  @ViewChild('conversationMessageLinkPreviewModal') conversationMessageLinkPreviewModal!: TemplateRef<any>;
  @Output() conversationMessageLinkPreviewChanged = new Subject<ConversationMessageLinkPreviewData[]>();
  @Input() conversationMessageLinkPreviewSubmitData: ConversationMessageLinkPreviewSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageLinkPreviewFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageLinkPreviewForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        url: ['', Validators.required],
        title: [''],
        description: [''],
        imageUrl: [''],
        siteName: [''],
        fetchedDateTime: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  conversationMessageLinkPreviews$ = this.conversationMessageLinkPreviewService.GetConversationMessageLinkPreviewList();
  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  constructor(
    private modalService: NgbModal,
    private conversationMessageLinkPreviewService: ConversationMessageLinkPreviewService,
    private conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(conversationMessageLinkPreviewData?: ConversationMessageLinkPreviewData) {

    if (conversationMessageLinkPreviewData != null) {

      if (!this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Conversation Message Link Previews`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.conversationMessageLinkPreviewSubmitData = this.conversationMessageLinkPreviewService.ConvertToConversationMessageLinkPreviewSubmitData(conversationMessageLinkPreviewData);
      this.isEditMode = true;
      this.objectGuid = conversationMessageLinkPreviewData.objectGuid;

      this.buildFormValues(conversationMessageLinkPreviewData);

    } else {

      if (!this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Link Previews`,
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
        this.conversationMessageLinkPreviewForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageLinkPreviewForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.conversationMessageLinkPreviewModal, {
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

    if (this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Conversation Message Link Previews`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.conversationMessageLinkPreviewForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageLinkPreviewForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageLinkPreviewForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageLinkPreviewSubmitData: ConversationMessageLinkPreviewSubmitData = {
        id: this.conversationMessageLinkPreviewSubmitData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        url: formValue.url!.trim(),
        title: formValue.title?.trim() || null,
        description: formValue.description?.trim() || null,
        imageUrl: formValue.imageUrl?.trim() || null,
        siteName: formValue.siteName?.trim() || null,
        fetchedDateTime: dateTimeLocalToIsoUtc(formValue.fetchedDateTime!.trim())!,
        versionNumber: this.conversationMessageLinkPreviewSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConversationMessageLinkPreview(conversationMessageLinkPreviewSubmitData);
      } else {
        this.addConversationMessageLinkPreview(conversationMessageLinkPreviewSubmitData);
      }
  }

  private addConversationMessageLinkPreview(conversationMessageLinkPreviewData: ConversationMessageLinkPreviewSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    conversationMessageLinkPreviewData.versionNumber = 0;
    conversationMessageLinkPreviewData.active = true;
    conversationMessageLinkPreviewData.deleted = false;
    this.conversationMessageLinkPreviewService.PostConversationMessageLinkPreview(conversationMessageLinkPreviewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConversationMessageLinkPreview) => {

        this.conversationMessageLinkPreviewService.ClearAllCaches();

        this.conversationMessageLinkPreviewChanged.next([newConversationMessageLinkPreview]);

        this.alertService.showMessage("Conversation Message Link Preview added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/conversationmessagelinkpreview', newConversationMessageLinkPreview.id]);
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
                                   'You do not have permission to save this Conversation Message Link Preview.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Link Preview.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Link Preview could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConversationMessageLinkPreview(conversationMessageLinkPreviewData: ConversationMessageLinkPreviewSubmitData) {
    this.conversationMessageLinkPreviewService.PutConversationMessageLinkPreview(conversationMessageLinkPreviewData.id, conversationMessageLinkPreviewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConversationMessageLinkPreview) => {

        this.conversationMessageLinkPreviewService.ClearAllCaches();

        this.conversationMessageLinkPreviewChanged.next([updatedConversationMessageLinkPreview]);

        this.alertService.showMessage("Conversation Message Link Preview updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Conversation Message Link Preview.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Link Preview.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Link Preview could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(conversationMessageLinkPreviewData: ConversationMessageLinkPreviewData | null) {

    if (conversationMessageLinkPreviewData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageLinkPreviewForm.reset({
        conversationMessageId: null,
        url: '',
        title: '',
        description: '',
        imageUrl: '',
        siteName: '',
        fetchedDateTime: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageLinkPreviewForm.reset({
        conversationMessageId: conversationMessageLinkPreviewData.conversationMessageId,
        url: conversationMessageLinkPreviewData.url ?? '',
        title: conversationMessageLinkPreviewData.title ?? '',
        description: conversationMessageLinkPreviewData.description ?? '',
        imageUrl: conversationMessageLinkPreviewData.imageUrl ?? '',
        siteName: conversationMessageLinkPreviewData.siteName ?? '',
        fetchedDateTime: isoUtcStringToDateTimeLocal(conversationMessageLinkPreviewData.fetchedDateTime) ?? '',
        versionNumber: conversationMessageLinkPreviewData.versionNumber?.toString() ?? '',
        active: conversationMessageLinkPreviewData.active ?? true,
        deleted: conversationMessageLinkPreviewData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageLinkPreviewForm.markAsPristine();
    this.conversationMessageLinkPreviewForm.markAsUntouched();
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


  public userIsSchedulerConversationMessageLinkPreviewReader(): boolean {
    return this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewReader();
  }

  public userIsSchedulerConversationMessageLinkPreviewWriter(): boolean {
    return this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewWriter();
  }
}

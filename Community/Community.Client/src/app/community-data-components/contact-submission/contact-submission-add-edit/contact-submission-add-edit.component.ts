/*
   GENERATED FORM FOR THE CONTACTSUBMISSION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactSubmission table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-submission-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactSubmissionService, ContactSubmissionData, ContactSubmissionSubmitData } from '../../../community-data-services/contact-submission.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ContactSubmissionFormValues {
  name: string,
  email: string,
  subject: string | null,
  message: string,
  submittedDate: string,
  isRead: boolean,
  isArchived: boolean,
  adminNotes: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-contact-submission-add-edit',
  templateUrl: './contact-submission-add-edit.component.html',
  styleUrls: ['./contact-submission-add-edit.component.scss']
})
export class ContactSubmissionAddEditComponent {
  @ViewChild('contactSubmissionModal') contactSubmissionModal!: TemplateRef<any>;
  @Output() contactSubmissionChanged = new Subject<ContactSubmissionData[]>();
  @Input() contactSubmissionSubmitData: ContactSubmissionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactSubmissionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactSubmissionForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        email: ['', Validators.required],
        subject: [''],
        message: ['', Validators.required],
        submittedDate: ['', Validators.required],
        isRead: [false],
        isArchived: [false],
        adminNotes: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  contactSubmissions$ = this.contactSubmissionService.GetContactSubmissionList();

  constructor(
    private modalService: NgbModal,
    private contactSubmissionService: ContactSubmissionService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contactSubmissionData?: ContactSubmissionData) {

    if (contactSubmissionData != null) {

      if (!this.contactSubmissionService.userIsCommunityContactSubmissionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Contact Submissions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contactSubmissionSubmitData = this.contactSubmissionService.ConvertToContactSubmissionSubmitData(contactSubmissionData);
      this.isEditMode = true;
      this.objectGuid = contactSubmissionData.objectGuid;

      this.buildFormValues(contactSubmissionData);

    } else {

      if (!this.contactSubmissionService.userIsCommunityContactSubmissionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Contact Submissions`,
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
        this.contactSubmissionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactSubmissionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contactSubmissionModal, {
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

    if (this.contactSubmissionService.userIsCommunityContactSubmissionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Contact Submissions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contactSubmissionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactSubmissionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactSubmissionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactSubmissionSubmitData: ContactSubmissionSubmitData = {
        id: this.contactSubmissionSubmitData?.id || 0,
        name: formValue.name!.trim(),
        email: formValue.email!.trim(),
        subject: formValue.subject?.trim() || null,
        message: formValue.message!.trim(),
        submittedDate: dateTimeLocalToIsoUtc(formValue.submittedDate!.trim())!,
        isRead: !!formValue.isRead,
        isArchived: !!formValue.isArchived,
        adminNotes: formValue.adminNotes?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContactSubmission(contactSubmissionSubmitData);
      } else {
        this.addContactSubmission(contactSubmissionSubmitData);
      }
  }

  private addContactSubmission(contactSubmissionData: ContactSubmissionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contactSubmissionData.active = true;
    contactSubmissionData.deleted = false;
    this.contactSubmissionService.PostContactSubmission(contactSubmissionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContactSubmission) => {

        this.contactSubmissionService.ClearAllCaches();

        this.contactSubmissionChanged.next([newContactSubmission]);

        this.alertService.showMessage("Contact Submission added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contactsubmission', newContactSubmission.id]);
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
                                   'You do not have permission to save this Contact Submission.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Submission.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Submission could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContactSubmission(contactSubmissionData: ContactSubmissionSubmitData) {
    this.contactSubmissionService.PutContactSubmission(contactSubmissionData.id, contactSubmissionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContactSubmission) => {

        this.contactSubmissionService.ClearAllCaches();

        this.contactSubmissionChanged.next([updatedContactSubmission]);

        this.alertService.showMessage("Contact Submission updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Contact Submission.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Submission.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Submission could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contactSubmissionData: ContactSubmissionData | null) {

    if (contactSubmissionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactSubmissionForm.reset({
        name: '',
        email: '',
        subject: '',
        message: '',
        submittedDate: '',
        isRead: false,
        isArchived: false,
        adminNotes: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactSubmissionForm.reset({
        name: contactSubmissionData.name ?? '',
        email: contactSubmissionData.email ?? '',
        subject: contactSubmissionData.subject ?? '',
        message: contactSubmissionData.message ?? '',
        submittedDate: isoUtcStringToDateTimeLocal(contactSubmissionData.submittedDate) ?? '',
        isRead: contactSubmissionData.isRead ?? false,
        isArchived: contactSubmissionData.isArchived ?? false,
        adminNotes: contactSubmissionData.adminNotes ?? '',
        active: contactSubmissionData.active ?? true,
        deleted: contactSubmissionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactSubmissionForm.markAsPristine();
    this.contactSubmissionForm.markAsUntouched();
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


  public userIsCommunityContactSubmissionReader(): boolean {
    return this.contactSubmissionService.userIsCommunityContactSubmissionReader();
  }

  public userIsCommunityContactSubmissionWriter(): boolean {
    return this.contactSubmissionService.userIsCommunityContactSubmissionWriter();
  }
}

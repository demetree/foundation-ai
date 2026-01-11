/*
   GENERATED FORM FOR THE CONTACTTAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactTag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-tag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactTagService, ContactTagData, ContactTagSubmitData } from '../../../scheduler-data-services/contact-tag.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { TagService } from '../../../scheduler-data-services/tag.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ContactTagFormValues {
  contactId: number | bigint,       // For FK link number
  tagId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-contact-tag-add-edit',
  templateUrl: './contact-tag-add-edit.component.html',
  styleUrls: ['./contact-tag-add-edit.component.scss']
})
export class ContactTagAddEditComponent {
  @ViewChild('contactTagModal') contactTagModal!: TemplateRef<any>;
  @Output() contactTagChanged = new Subject<ContactTagData[]>();
  @Input() contactTagSubmitData: ContactTagSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactTagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactTagForm: FormGroup = this.fb.group({
        contactId: [null, Validators.required],
        tagId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  contactTags$ = this.contactTagService.GetContactTagList();
  contacts$ = this.contactService.GetContactList();
  tags$ = this.tagService.GetTagList();

  constructor(
    private modalService: NgbModal,
    private contactTagService: ContactTagService,
    private contactService: ContactService,
    private tagService: TagService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contactTagData?: ContactTagData) {

    if (contactTagData != null) {

      if (!this.contactTagService.userIsSchedulerContactTagReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Contact Tags`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contactTagSubmitData = this.contactTagService.ConvertToContactTagSubmitData(contactTagData);
      this.isEditMode = true;
      this.objectGuid = contactTagData.objectGuid;

      this.buildFormValues(contactTagData);

    } else {

      if (!this.contactTagService.userIsSchedulerContactTagWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Contact Tags`,
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
        this.contactTagForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactTagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contactTagModal, {
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

    if (this.contactTagService.userIsSchedulerContactTagWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Contact Tags`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contactTagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactTagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactTagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactTagSubmitData: ContactTagSubmitData = {
        id: this.contactTagSubmitData?.id || 0,
        contactId: Number(formValue.contactId),
        tagId: Number(formValue.tagId),
        versionNumber: this.contactTagSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContactTag(contactTagSubmitData);
      } else {
        this.addContactTag(contactTagSubmitData);
      }
  }

  private addContactTag(contactTagData: ContactTagSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contactTagData.versionNumber = 0;
    contactTagData.active = true;
    contactTagData.deleted = false;
    this.contactTagService.PostContactTag(contactTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContactTag) => {

        this.contactTagService.ClearAllCaches();

        this.contactTagChanged.next([newContactTag]);

        this.alertService.showMessage("Contact Tag added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contacttag', newContactTag.id]);
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
                                   'You do not have permission to save this Contact Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContactTag(contactTagData: ContactTagSubmitData) {
    this.contactTagService.PutContactTag(contactTagData.id, contactTagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContactTag) => {

        this.contactTagService.ClearAllCaches();

        this.contactTagChanged.next([updatedContactTag]);

        this.alertService.showMessage("Contact Tag updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Contact Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contactTagData: ContactTagData | null) {

    if (contactTagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactTagForm.reset({
        contactId: null,
        tagId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactTagForm.reset({
        contactId: contactTagData.contactId,
        tagId: contactTagData.tagId,
        versionNumber: contactTagData.versionNumber?.toString() ?? '',
        active: contactTagData.active ?? true,
        deleted: contactTagData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactTagForm.markAsPristine();
    this.contactTagForm.markAsUntouched();
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


  public userIsSchedulerContactTagReader(): boolean {
    return this.contactTagService.userIsSchedulerContactTagReader();
  }

  public userIsSchedulerContactTagWriter(): boolean {
    return this.contactTagService.userIsSchedulerContactTagWriter();
  }
}

/*
   GENERATED FORM FOR THE OFFICECONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from OfficeContact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to office-contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeContactService, OfficeContactData, OfficeContactSubmitData } from '../../../scheduler-data-services/office-contact.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { RelationshipTypeService } from '../../../scheduler-data-services/relationship-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface OfficeContactFormValues {
  officeId: number | bigint,       // For FK link number
  contactId: number | bigint,       // For FK link number
  isPrimary: boolean,
  relationshipTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-office-contact-add-edit',
  templateUrl: './office-contact-add-edit.component.html',
  styleUrls: ['./office-contact-add-edit.component.scss']
})
export class OfficeContactAddEditComponent {
  @ViewChild('officeContactModal') officeContactModal!: TemplateRef<any>;
  @Output() officeContactChanged = new Subject<OfficeContactData[]>();
  @Input() officeContactSubmitData: OfficeContactSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<OfficeContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public officeContactForm: FormGroup = this.fb.group({
        officeId: [null, Validators.required],
        contactId: [null, Validators.required],
        isPrimary: [false],
        relationshipTypeId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  officeContacts$ = this.officeContactService.GetOfficeContactList();
  offices$ = this.officeService.GetOfficeList();
  contacts$ = this.contactService.GetContactList();
  relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();

  constructor(
    private modalService: NgbModal,
    private officeContactService: OfficeContactService,
    private officeService: OfficeService,
    private contactService: ContactService,
    private relationshipTypeService: RelationshipTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(officeContactData?: OfficeContactData) {

    if (officeContactData != null) {

      if (!this.officeContactService.userIsSchedulerOfficeContactReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Office Contacts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.officeContactSubmitData = this.officeContactService.ConvertToOfficeContactSubmitData(officeContactData);
      this.isEditMode = true;
      this.objectGuid = officeContactData.objectGuid;

      this.buildFormValues(officeContactData);

    } else {

      if (!this.officeContactService.userIsSchedulerOfficeContactWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Office Contacts`,
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
        this.officeContactForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.officeContactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.officeContactModal, {
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

    if (this.officeContactService.userIsSchedulerOfficeContactWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Office Contacts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.officeContactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.officeContactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.officeContactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const officeContactSubmitData: OfficeContactSubmitData = {
        id: this.officeContactSubmitData?.id || 0,
        officeId: Number(formValue.officeId),
        contactId: Number(formValue.contactId),
        isPrimary: !!formValue.isPrimary,
        relationshipTypeId: Number(formValue.relationshipTypeId),
        versionNumber: this.officeContactSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateOfficeContact(officeContactSubmitData);
      } else {
        this.addOfficeContact(officeContactSubmitData);
      }
  }

  private addOfficeContact(officeContactData: OfficeContactSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    officeContactData.versionNumber = 0;
    officeContactData.active = true;
    officeContactData.deleted = false;
    this.officeContactService.PostOfficeContact(officeContactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newOfficeContact) => {

        this.officeContactService.ClearAllCaches();

        this.officeContactChanged.next([newOfficeContact]);

        this.alertService.showMessage("Office Contact added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/officecontact', newOfficeContact.id]);
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
                                   'You do not have permission to save this Office Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Office Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Office Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateOfficeContact(officeContactData: OfficeContactSubmitData) {
    this.officeContactService.PutOfficeContact(officeContactData.id, officeContactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedOfficeContact) => {

        this.officeContactService.ClearAllCaches();

        this.officeContactChanged.next([updatedOfficeContact]);

        this.alertService.showMessage("Office Contact updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Office Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Office Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Office Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(officeContactData: OfficeContactData | null) {

    if (officeContactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.officeContactForm.reset({
        officeId: null,
        contactId: null,
        isPrimary: false,
        relationshipTypeId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.officeContactForm.reset({
        officeId: officeContactData.officeId,
        contactId: officeContactData.contactId,
        isPrimary: officeContactData.isPrimary ?? false,
        relationshipTypeId: officeContactData.relationshipTypeId,
        versionNumber: officeContactData.versionNumber?.toString() ?? '',
        active: officeContactData.active ?? true,
        deleted: officeContactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.officeContactForm.markAsPristine();
    this.officeContactForm.markAsUntouched();
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


  public userIsSchedulerOfficeContactReader(): boolean {
    return this.officeContactService.userIsSchedulerOfficeContactReader();
  }

  public userIsSchedulerOfficeContactWriter(): boolean {
    return this.officeContactService.userIsSchedulerOfficeContactWriter();
  }
}

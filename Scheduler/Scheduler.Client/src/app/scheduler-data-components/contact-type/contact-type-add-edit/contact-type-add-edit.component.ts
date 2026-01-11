/*
   GENERATED FORM FOR THE CONTACTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactTypeService, ContactTypeData, ContactTypeSubmitData } from '../../../scheduler-data-services/contact-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ContactTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-contact-type-add-edit',
  templateUrl: './contact-type-add-edit.component.html',
  styleUrls: ['./contact-type-add-edit.component.scss']
})
export class ContactTypeAddEditComponent {
  @ViewChild('contactTypeModal') contactTypeModal!: TemplateRef<any>;
  @Output() contactTypeChanged = new Subject<ContactTypeData[]>();
  @Input() contactTypeSubmitData: ContactTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  contactTypes$ = this.contactTypeService.GetContactTypeList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private contactTypeService: ContactTypeService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contactTypeData?: ContactTypeData) {

    if (contactTypeData != null) {

      if (!this.contactTypeService.userIsSchedulerContactTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Contact Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contactTypeSubmitData = this.contactTypeService.ConvertToContactTypeSubmitData(contactTypeData);
      this.isEditMode = true;
      this.objectGuid = contactTypeData.objectGuid;

      this.buildFormValues(contactTypeData);

    } else {

      if (!this.contactTypeService.userIsSchedulerContactTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Contact Types`,
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
        this.contactTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contactTypeModal, {
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

    if (this.contactTypeService.userIsSchedulerContactTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Contact Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contactTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactTypeSubmitData: ContactTypeSubmitData = {
        id: this.contactTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContactType(contactTypeSubmitData);
      } else {
        this.addContactType(contactTypeSubmitData);
      }
  }

  private addContactType(contactTypeData: ContactTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contactTypeData.active = true;
    contactTypeData.deleted = false;
    this.contactTypeService.PostContactType(contactTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContactType) => {

        this.contactTypeService.ClearAllCaches();

        this.contactTypeChanged.next([newContactType]);

        this.alertService.showMessage("Contact Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contacttype', newContactType.id]);
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
                                   'You do not have permission to save this Contact Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContactType(contactTypeData: ContactTypeSubmitData) {
    this.contactTypeService.PutContactType(contactTypeData.id, contactTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContactType) => {

        this.contactTypeService.ClearAllCaches();

        this.contactTypeChanged.next([updatedContactType]);

        this.alertService.showMessage("Contact Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Contact Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contactTypeData: ContactTypeData | null) {

    if (contactTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        iconId: null,
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactTypeForm.reset({
        name: contactTypeData.name ?? '',
        description: contactTypeData.description ?? '',
        sequence: contactTypeData.sequence?.toString() ?? '',
        iconId: contactTypeData.iconId,
        color: contactTypeData.color ?? '',
        active: contactTypeData.active ?? true,
        deleted: contactTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactTypeForm.markAsPristine();
    this.contactTypeForm.markAsUntouched();
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


  public userIsSchedulerContactTypeReader(): boolean {
    return this.contactTypeService.userIsSchedulerContactTypeReader();
  }

  public userIsSchedulerContactTypeWriter(): boolean {
    return this.contactTypeService.userIsSchedulerContactTypeWriter();
  }
}

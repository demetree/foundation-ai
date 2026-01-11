/*
   GENERATED FORM FOR THE CONTACTMETHOD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactMethod table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-method-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactMethodService, ContactMethodData, ContactMethodSubmitData } from '../../../scheduler-data-services/contact-method.service';
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
interface ContactMethodFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-contact-method-add-edit',
  templateUrl: './contact-method-add-edit.component.html',
  styleUrls: ['./contact-method-add-edit.component.scss']
})
export class ContactMethodAddEditComponent {
  @ViewChild('contactMethodModal') contactMethodModal!: TemplateRef<any>;
  @Output() contactMethodChanged = new Subject<ContactMethodData[]>();
  @Input() contactMethodSubmitData: ContactMethodSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactMethodFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactMethodForm: FormGroup = this.fb.group({
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

  contactMethods$ = this.contactMethodService.GetContactMethodList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private contactMethodService: ContactMethodService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contactMethodData?: ContactMethodData) {

    if (contactMethodData != null) {

      if (!this.contactMethodService.userIsSchedulerContactMethodReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Contact Methods`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contactMethodSubmitData = this.contactMethodService.ConvertToContactMethodSubmitData(contactMethodData);
      this.isEditMode = true;
      this.objectGuid = contactMethodData.objectGuid;

      this.buildFormValues(contactMethodData);

    } else {

      if (!this.contactMethodService.userIsSchedulerContactMethodWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Contact Methods`,
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
        this.contactMethodForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactMethodForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contactMethodModal, {
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

    if (this.contactMethodService.userIsSchedulerContactMethodWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Contact Methods`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contactMethodForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactMethodForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactMethodForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactMethodSubmitData: ContactMethodSubmitData = {
        id: this.contactMethodSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContactMethod(contactMethodSubmitData);
      } else {
        this.addContactMethod(contactMethodSubmitData);
      }
  }

  private addContactMethod(contactMethodData: ContactMethodSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contactMethodData.active = true;
    contactMethodData.deleted = false;
    this.contactMethodService.PostContactMethod(contactMethodData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContactMethod) => {

        this.contactMethodService.ClearAllCaches();

        this.contactMethodChanged.next([newContactMethod]);

        this.alertService.showMessage("Contact Method added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contactmethod', newContactMethod.id]);
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
                                   'You do not have permission to save this Contact Method.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Method.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Method could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContactMethod(contactMethodData: ContactMethodSubmitData) {
    this.contactMethodService.PutContactMethod(contactMethodData.id, contactMethodData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContactMethod) => {

        this.contactMethodService.ClearAllCaches();

        this.contactMethodChanged.next([updatedContactMethod]);

        this.alertService.showMessage("Contact Method updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Contact Method.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Method.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Method could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contactMethodData: ContactMethodData | null) {

    if (contactMethodData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactMethodForm.reset({
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
        this.contactMethodForm.reset({
        name: contactMethodData.name ?? '',
        description: contactMethodData.description ?? '',
        sequence: contactMethodData.sequence?.toString() ?? '',
        iconId: contactMethodData.iconId,
        color: contactMethodData.color ?? '',
        active: contactMethodData.active ?? true,
        deleted: contactMethodData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactMethodForm.markAsPristine();
    this.contactMethodForm.markAsUntouched();
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


  public userIsSchedulerContactMethodReader(): boolean {
    return this.contactMethodService.userIsSchedulerContactMethodReader();
  }

  public userIsSchedulerContactMethodWriter(): boolean {
    return this.contactMethodService.userIsSchedulerContactMethodWriter();
  }
}

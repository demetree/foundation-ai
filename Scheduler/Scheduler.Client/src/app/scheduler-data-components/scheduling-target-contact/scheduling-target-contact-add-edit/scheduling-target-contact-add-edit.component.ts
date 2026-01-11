/*
   GENERATED FORM FOR THE SCHEDULINGTARGETCONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetContact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetContactService, SchedulingTargetContactData, SchedulingTargetContactSubmitData } from '../../../scheduler-data-services/scheduling-target-contact.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
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
interface SchedulingTargetContactFormValues {
  schedulingTargetId: number | bigint,       // For FK link number
  contactId: number | bigint,       // For FK link number
  isPrimary: boolean,
  relationshipTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-scheduling-target-contact-add-edit',
  templateUrl: './scheduling-target-contact-add-edit.component.html',
  styleUrls: ['./scheduling-target-contact-add-edit.component.scss']
})
export class SchedulingTargetContactAddEditComponent {
  @ViewChild('schedulingTargetContactModal') schedulingTargetContactModal!: TemplateRef<any>;
  @Output() schedulingTargetContactChanged = new Subject<SchedulingTargetContactData[]>();
  @Input() schedulingTargetContactSubmitData: SchedulingTargetContactSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetContactForm: FormGroup = this.fb.group({
        schedulingTargetId: [null, Validators.required],
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

  schedulingTargetContacts$ = this.schedulingTargetContactService.GetSchedulingTargetContactList();
  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  contacts$ = this.contactService.GetContactList();
  relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();

  constructor(
    private modalService: NgbModal,
    private schedulingTargetContactService: SchedulingTargetContactService,
    private schedulingTargetService: SchedulingTargetService,
    private contactService: ContactService,
    private relationshipTypeService: RelationshipTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(schedulingTargetContactData?: SchedulingTargetContactData) {

    if (schedulingTargetContactData != null) {

      if (!this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduling Target Contacts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.schedulingTargetContactSubmitData = this.schedulingTargetContactService.ConvertToSchedulingTargetContactSubmitData(schedulingTargetContactData);
      this.isEditMode = true;
      this.objectGuid = schedulingTargetContactData.objectGuid;

      this.buildFormValues(schedulingTargetContactData);

    } else {

      if (!this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Contacts`,
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
        this.schedulingTargetContactForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetContactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.schedulingTargetContactModal, {
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

    if (this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Contacts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.schedulingTargetContactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetContactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetContactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetContactSubmitData: SchedulingTargetContactSubmitData = {
        id: this.schedulingTargetContactSubmitData?.id || 0,
        schedulingTargetId: Number(formValue.schedulingTargetId),
        contactId: Number(formValue.contactId),
        isPrimary: !!formValue.isPrimary,
        relationshipTypeId: Number(formValue.relationshipTypeId),
        versionNumber: this.schedulingTargetContactSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSchedulingTargetContact(schedulingTargetContactSubmitData);
      } else {
        this.addSchedulingTargetContact(schedulingTargetContactSubmitData);
      }
  }

  private addSchedulingTargetContact(schedulingTargetContactData: SchedulingTargetContactSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    schedulingTargetContactData.versionNumber = 0;
    schedulingTargetContactData.active = true;
    schedulingTargetContactData.deleted = false;
    this.schedulingTargetContactService.PostSchedulingTargetContact(schedulingTargetContactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSchedulingTargetContact) => {

        this.schedulingTargetContactService.ClearAllCaches();

        this.schedulingTargetContactChanged.next([newSchedulingTargetContact]);

        this.alertService.showMessage("Scheduling Target Contact added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulingtargetcontact', newSchedulingTargetContact.id]);
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
                                   'You do not have permission to save this Scheduling Target Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSchedulingTargetContact(schedulingTargetContactData: SchedulingTargetContactSubmitData) {
    this.schedulingTargetContactService.PutSchedulingTargetContact(schedulingTargetContactData.id, schedulingTargetContactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSchedulingTargetContact) => {

        this.schedulingTargetContactService.ClearAllCaches();

        this.schedulingTargetContactChanged.next([updatedSchedulingTargetContact]);

        this.alertService.showMessage("Scheduling Target Contact updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduling Target Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(schedulingTargetContactData: SchedulingTargetContactData | null) {

    if (schedulingTargetContactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetContactForm.reset({
        schedulingTargetId: null,
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
        this.schedulingTargetContactForm.reset({
        schedulingTargetId: schedulingTargetContactData.schedulingTargetId,
        contactId: schedulingTargetContactData.contactId,
        isPrimary: schedulingTargetContactData.isPrimary ?? false,
        relationshipTypeId: schedulingTargetContactData.relationshipTypeId,
        versionNumber: schedulingTargetContactData.versionNumber?.toString() ?? '',
        active: schedulingTargetContactData.active ?? true,
        deleted: schedulingTargetContactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetContactForm.markAsPristine();
    this.schedulingTargetContactForm.markAsUntouched();
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


  public userIsSchedulerSchedulingTargetContactReader(): boolean {
    return this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactReader();
  }

  public userIsSchedulerSchedulingTargetContactWriter(): boolean {
    return this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactWriter();
  }
}

/*
   GENERATED FORM FOR THE CONTACTINTERACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactInteraction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-interaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactInteractionService, ContactInteractionData, ContactInteractionSubmitData } from '../../../scheduler-data-services/contact-interaction.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { InteractionTypeService } from '../../../scheduler-data-services/interaction-type.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { PriorityService } from '../../../scheduler-data-services/priority.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ContactInteractionFormValues {
  contactId: number | bigint,       // For FK link number
  initiatingContactId: number | bigint | null,       // For FK link number
  interactionTypeId: number | bigint,       // For FK link number
  scheduledEventId: number | bigint | null,       // For FK link number
  startTime: string,
  endTime: string | null,
  notes: string | null,
  location: string | null,
  priorityId: number | bigint | null,       // For FK link number
  externalId: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-contact-interaction-add-edit',
  templateUrl: './contact-interaction-add-edit.component.html',
  styleUrls: ['./contact-interaction-add-edit.component.scss']
})
export class ContactInteractionAddEditComponent {
  @ViewChild('contactInteractionModal') contactInteractionModal!: TemplateRef<any>;
  @Output() contactInteractionChanged = new Subject<ContactInteractionData[]>();
  @Input() contactInteractionSubmitData: ContactInteractionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactInteractionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactInteractionForm: FormGroup = this.fb.group({
        contactId: [null, Validators.required],
        initiatingContactId: [null],
        interactionTypeId: [null, Validators.required],
        scheduledEventId: [null],
        startTime: ['', Validators.required],
        endTime: [''],
        notes: [''],
        location: [''],
        priorityId: [null],
        externalId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  contactInteractions$ = this.contactInteractionService.GetContactInteractionList();
  contacts$ = this.contactService.GetContactList();
  interactionTypes$ = this.interactionTypeService.GetInteractionTypeList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  priorities$ = this.priorityService.GetPriorityList();

  constructor(
    private modalService: NgbModal,
    private contactInteractionService: ContactInteractionService,
    private contactService: ContactService,
    private interactionTypeService: InteractionTypeService,
    private scheduledEventService: ScheduledEventService,
    private priorityService: PriorityService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contactInteractionData?: ContactInteractionData) {

    if (contactInteractionData != null) {

      if (!this.contactInteractionService.userIsSchedulerContactInteractionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Contact Interactions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contactInteractionSubmitData = this.contactInteractionService.ConvertToContactInteractionSubmitData(contactInteractionData);
      this.isEditMode = true;
      this.objectGuid = contactInteractionData.objectGuid;

      this.buildFormValues(contactInteractionData);

    } else {

      if (!this.contactInteractionService.userIsSchedulerContactInteractionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Contact Interactions`,
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
        this.contactInteractionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactInteractionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contactInteractionModal, {
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

    if (this.contactInteractionService.userIsSchedulerContactInteractionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Contact Interactions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contactInteractionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactInteractionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactInteractionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactInteractionSubmitData: ContactInteractionSubmitData = {
        id: this.contactInteractionSubmitData?.id || 0,
        contactId: Number(formValue.contactId),
        initiatingContactId: formValue.initiatingContactId ? Number(formValue.initiatingContactId) : null,
        interactionTypeId: Number(formValue.interactionTypeId),
        scheduledEventId: formValue.scheduledEventId ? Number(formValue.scheduledEventId) : null,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        endTime: formValue.endTime ? dateTimeLocalToIsoUtc(formValue.endTime.trim()) : null,
        notes: formValue.notes?.trim() || null,
        location: formValue.location?.trim() || null,
        priorityId: formValue.priorityId ? Number(formValue.priorityId) : null,
        externalId: formValue.externalId?.trim() || null,
        versionNumber: this.contactInteractionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContactInteraction(contactInteractionSubmitData);
      } else {
        this.addContactInteraction(contactInteractionSubmitData);
      }
  }

  private addContactInteraction(contactInteractionData: ContactInteractionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contactInteractionData.versionNumber = 0;
    contactInteractionData.active = true;
    contactInteractionData.deleted = false;
    this.contactInteractionService.PostContactInteraction(contactInteractionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContactInteraction) => {

        this.contactInteractionService.ClearAllCaches();

        this.contactInteractionChanged.next([newContactInteraction]);

        this.alertService.showMessage("Contact Interaction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contactinteraction', newContactInteraction.id]);
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
                                   'You do not have permission to save this Contact Interaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Interaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Interaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContactInteraction(contactInteractionData: ContactInteractionSubmitData) {
    this.contactInteractionService.PutContactInteraction(contactInteractionData.id, contactInteractionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContactInteraction) => {

        this.contactInteractionService.ClearAllCaches();

        this.contactInteractionChanged.next([updatedContactInteraction]);

        this.alertService.showMessage("Contact Interaction updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Contact Interaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Interaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Interaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contactInteractionData: ContactInteractionData | null) {

    if (contactInteractionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactInteractionForm.reset({
        contactId: null,
        initiatingContactId: null,
        interactionTypeId: null,
        scheduledEventId: null,
        startTime: '',
        endTime: '',
        notes: '',
        location: '',
        priorityId: null,
        externalId: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactInteractionForm.reset({
        contactId: contactInteractionData.contactId,
        initiatingContactId: contactInteractionData.initiatingContactId,
        interactionTypeId: contactInteractionData.interactionTypeId,
        scheduledEventId: contactInteractionData.scheduledEventId,
        startTime: isoUtcStringToDateTimeLocal(contactInteractionData.startTime) ?? '',
        endTime: isoUtcStringToDateTimeLocal(contactInteractionData.endTime) ?? '',
        notes: contactInteractionData.notes ?? '',
        location: contactInteractionData.location ?? '',
        priorityId: contactInteractionData.priorityId,
        externalId: contactInteractionData.externalId ?? '',
        versionNumber: contactInteractionData.versionNumber?.toString() ?? '',
        active: contactInteractionData.active ?? true,
        deleted: contactInteractionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactInteractionForm.markAsPristine();
    this.contactInteractionForm.markAsUntouched();
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


  public userIsSchedulerContactInteractionReader(): boolean {
    return this.contactInteractionService.userIsSchedulerContactInteractionReader();
  }

  public userIsSchedulerContactInteractionWriter(): boolean {
    return this.contactInteractionService.userIsSchedulerContactInteractionWriter();
  }
}

/*
   GENERATED FORM FOR THE CLIENTCONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ClientContact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to client-contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ClientContactService, ClientContactData, ClientContactSubmitData } from '../../../scheduler-data-services/client-contact.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ClientService } from '../../../scheduler-data-services/client.service';
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
interface ClientContactFormValues {
  clientId: number | bigint,       // For FK link number
  contactId: number | bigint,       // For FK link number
  isPrimary: boolean,
  relationshipTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-client-contact-add-edit',
  templateUrl: './client-contact-add-edit.component.html',
  styleUrls: ['./client-contact-add-edit.component.scss']
})
export class ClientContactAddEditComponent {
  @ViewChild('clientContactModal') clientContactModal!: TemplateRef<any>;
  @Output() clientContactChanged = new Subject<ClientContactData[]>();
  @Input() clientContactSubmitData: ClientContactSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ClientContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public clientContactForm: FormGroup = this.fb.group({
        clientId: [null, Validators.required],
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

  clientContacts$ = this.clientContactService.GetClientContactList();
  clients$ = this.clientService.GetClientList();
  contacts$ = this.contactService.GetContactList();
  relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();

  constructor(
    private modalService: NgbModal,
    private clientContactService: ClientContactService,
    private clientService: ClientService,
    private contactService: ContactService,
    private relationshipTypeService: RelationshipTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(clientContactData?: ClientContactData) {

    if (clientContactData != null) {

      if (!this.clientContactService.userIsSchedulerClientContactReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Client Contacts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.clientContactSubmitData = this.clientContactService.ConvertToClientContactSubmitData(clientContactData);
      this.isEditMode = true;
      this.objectGuid = clientContactData.objectGuid;

      this.buildFormValues(clientContactData);

    } else {

      if (!this.clientContactService.userIsSchedulerClientContactWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Client Contacts`,
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
        this.clientContactForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.clientContactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.clientContactModal, {
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

    if (this.clientContactService.userIsSchedulerClientContactWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Client Contacts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.clientContactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.clientContactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.clientContactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const clientContactSubmitData: ClientContactSubmitData = {
        id: this.clientContactSubmitData?.id || 0,
        clientId: Number(formValue.clientId),
        contactId: Number(formValue.contactId),
        isPrimary: !!formValue.isPrimary,
        relationshipTypeId: Number(formValue.relationshipTypeId),
        versionNumber: this.clientContactSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateClientContact(clientContactSubmitData);
      } else {
        this.addClientContact(clientContactSubmitData);
      }
  }

  private addClientContact(clientContactData: ClientContactSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    clientContactData.versionNumber = 0;
    clientContactData.active = true;
    clientContactData.deleted = false;
    this.clientContactService.PostClientContact(clientContactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newClientContact) => {

        this.clientContactService.ClearAllCaches();

        this.clientContactChanged.next([newClientContact]);

        this.alertService.showMessage("Client Contact added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/clientcontact', newClientContact.id]);
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
                                   'You do not have permission to save this Client Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Client Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Client Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateClientContact(clientContactData: ClientContactSubmitData) {
    this.clientContactService.PutClientContact(clientContactData.id, clientContactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedClientContact) => {

        this.clientContactService.ClearAllCaches();

        this.clientContactChanged.next([updatedClientContact]);

        this.alertService.showMessage("Client Contact updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Client Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Client Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Client Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(clientContactData: ClientContactData | null) {

    if (clientContactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.clientContactForm.reset({
        clientId: null,
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
        this.clientContactForm.reset({
        clientId: clientContactData.clientId,
        contactId: clientContactData.contactId,
        isPrimary: clientContactData.isPrimary ?? false,
        relationshipTypeId: clientContactData.relationshipTypeId,
        versionNumber: clientContactData.versionNumber?.toString() ?? '',
        active: clientContactData.active ?? true,
        deleted: clientContactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.clientContactForm.markAsPristine();
    this.clientContactForm.markAsUntouched();
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


  public userIsSchedulerClientContactReader(): boolean {
    return this.clientContactService.userIsSchedulerClientContactReader();
  }

  public userIsSchedulerClientContactWriter(): boolean {
    return this.clientContactService.userIsSchedulerClientContactWriter();
  }
}

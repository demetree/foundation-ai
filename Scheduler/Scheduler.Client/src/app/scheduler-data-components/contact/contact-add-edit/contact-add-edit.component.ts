/*
   GENERATED FORM FOR THE CONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Contact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactService, ContactData, ContactSubmitData } from '../../../scheduler-data-services/contact.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ContactTypeService } from '../../../scheduler-data-services/contact-type.service';
import { SalutationService } from '../../../scheduler-data-services/salutation.service';
import { ContactMethodService } from '../../../scheduler-data-services/contact-method.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ContactFormValues {
  contactTypeId: number | bigint,       // For FK link number
  firstName: string,
  middleName: string | null,
  lastName: string,
  salutationId: number | bigint | null,       // For FK link number
  title: string | null,
  birthDate: string | null,
  company: string | null,
  email: string | null,
  phone: string | null,
  mobile: string | null,
  position: string | null,
  webSite: string | null,
  contactMethodId: number | bigint | null,       // For FK link number
  notes: string | null,
  timeZoneId: number | bigint | null,       // For FK link number
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  externalId: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-contact-add-edit',
  templateUrl: './contact-add-edit.component.html',
  styleUrls: ['./contact-add-edit.component.scss']
})
export class ContactAddEditComponent {
  @ViewChild('contactModal') contactModal!: TemplateRef<any>;
  @Output() contactChanged = new Subject<ContactData[]>();
  @Input() contactSubmitData: ContactSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactForm: FormGroup = this.fb.group({
        contactTypeId: [null, Validators.required],
        firstName: ['', Validators.required],
        middleName: [''],
        lastName: ['', Validators.required],
        salutationId: [null],
        title: [''],
        birthDate: [''],
        company: [''],
        email: [''],
        phone: [''],
        mobile: [''],
        position: [''],
        webSite: [''],
        contactMethodId: [null],
        notes: [''],
        timeZoneId: [null],
        iconId: [null],
        color: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
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

  contacts$ = this.contactService.GetContactList();
  contactTypes$ = this.contactTypeService.GetContactTypeList();
  salutations$ = this.salutationService.GetSalutationList();
  contactMethods$ = this.contactMethodService.GetContactMethodList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private contactService: ContactService,
    private contactTypeService: ContactTypeService,
    private salutationService: SalutationService,
    private contactMethodService: ContactMethodService,
    private timeZoneService: TimeZoneService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contactData?: ContactData) {

    if (contactData != null) {

      if (!this.contactService.userIsSchedulerContactReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Contacts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contactSubmitData = this.contactService.ConvertToContactSubmitData(contactData);
      this.isEditMode = true;
      this.objectGuid = contactData.objectGuid;

      this.buildFormValues(contactData);

    } else {

      if (!this.contactService.userIsSchedulerContactWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Contacts`,
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
        this.contactForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contactModal, {
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

    if (this.contactService.userIsSchedulerContactWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Contacts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactSubmitData: ContactSubmitData = {
        id: this.contactSubmitData?.id || 0,
        contactTypeId: Number(formValue.contactTypeId),
        firstName: formValue.firstName!.trim(),
        middleName: formValue.middleName?.trim() || null,
        lastName: formValue.lastName!.trim(),
        salutationId: formValue.salutationId ? Number(formValue.salutationId) : null,
        title: formValue.title?.trim() || null,
        birthDate: formValue.birthDate?.trim() || null,
        company: formValue.company?.trim() || null,
        email: formValue.email?.trim() || null,
        phone: formValue.phone?.trim() || null,
        mobile: formValue.mobile?.trim() || null,
        position: formValue.position?.trim() || null,
        webSite: formValue.webSite?.trim() || null,
        contactMethodId: formValue.contactMethodId ? Number(formValue.contactMethodId) : null,
        notes: formValue.notes?.trim() || null,
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        versionNumber: this.contactSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContact(contactSubmitData);
      } else {
        this.addContact(contactSubmitData);
      }
  }

  private addContact(contactData: ContactSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contactData.versionNumber = 0;
    contactData.active = true;
    contactData.deleted = false;
    this.contactService.PostContact(contactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContact) => {

        this.contactService.ClearAllCaches();

        this.contactChanged.next([newContact]);

        this.alertService.showMessage("Contact added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contact', newContact.id]);
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
                                   'You do not have permission to save this Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContact(contactData: ContactSubmitData) {
    this.contactService.PutContact(contactData.id, contactData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContact) => {

        this.contactService.ClearAllCaches();

        this.contactChanged.next([updatedContact]);

        this.alertService.showMessage("Contact updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contactData: ContactData | null) {

    if (contactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactForm.reset({
        contactTypeId: null,
        firstName: '',
        middleName: '',
        lastName: '',
        salutationId: null,
        title: '',
        birthDate: '',
        company: '',
        email: '',
        phone: '',
        mobile: '',
        position: '',
        webSite: '',
        contactMethodId: null,
        notes: '',
        timeZoneId: null,
        iconId: null,
        color: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
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
        this.contactForm.reset({
        contactTypeId: contactData.contactTypeId,
        firstName: contactData.firstName ?? '',
        middleName: contactData.middleName ?? '',
        lastName: contactData.lastName ?? '',
        salutationId: contactData.salutationId,
        title: contactData.title ?? '',
        birthDate: contactData.birthDate ?? '',
        company: contactData.company ?? '',
        email: contactData.email ?? '',
        phone: contactData.phone ?? '',
        mobile: contactData.mobile ?? '',
        position: contactData.position ?? '',
        webSite: contactData.webSite ?? '',
        contactMethodId: contactData.contactMethodId,
        notes: contactData.notes ?? '',
        timeZoneId: contactData.timeZoneId,
        iconId: contactData.iconId,
        color: contactData.color ?? '',
        avatarFileName: contactData.avatarFileName ?? '',
        avatarSize: contactData.avatarSize?.toString() ?? '',
        avatarData: contactData.avatarData ?? '',
        avatarMimeType: contactData.avatarMimeType ?? '',
        externalId: contactData.externalId ?? '',
        versionNumber: contactData.versionNumber?.toString() ?? '',
        active: contactData.active ?? true,
        deleted: contactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactForm.markAsPristine();
    this.contactForm.markAsUntouched();
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


  public userIsSchedulerContactReader(): boolean {
    return this.contactService.userIsSchedulerContactReader();
  }

  public userIsSchedulerContactWriter(): boolean {
    return this.contactService.userIsSchedulerContactWriter();
  }
}

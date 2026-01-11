import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
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
import { CurrentUserService } from '../../../services/current-user.service';

@Component({
  selector: 'app-contact-custom-add-edit',
  templateUrl: './contact-custom-add-edit.component.html',
  styleUrls: ['./contact-custom-add-edit.component.scss'],
  animations: [
    trigger('collapse', [
      state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
      state('true', style({ height: '*', opacity: 1 })),
      transition('false <=> true', animate('300ms ease-in-out'))
    ])
  ]
})
export class ContactCustomAddEditComponent {
  @ViewChild('contactModal') contactModal!: TemplateRef<any>;
  @Output() contactChanged = new Subject<ContactData[]>();
  @Input() contactSubmitData: ContactSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  public currentAvatarUrl: string | null = null;
  public isAvatarPanelOpen = false;
  public isDragOver = false;

  contactForm: FormGroup = this.fb.group({
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
        timeZoneId: [this.currentUserService.defaultTimeZoneId],
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
    private iconService: IconService,
    private timeZoneService: TimeZoneService,
    private currentUserService: CurrentUserService,
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


  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    if (!file.type.startsWith('image/')) {
      this.alertService.showMessage('Invalid file type', 'Please drop an image file', MessageSeverity.warn);
      return;
    }

    // Reuse existing logic — simulate file input change
    const fakeEvent = { target: { files: [file] } } as any;
    this.onAvatarSelected(fakeEvent);
  }


  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    // Enforce 2MB limit
    if (file.size > 2 * 1024 * 1024) {
      this.alertService.showMessage(
        'Image too large',
        'Please select an image under 2MB',
        MessageSeverity.warn
      );
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const result = e.target?.result as string;

      if (!result) return;

      // Extract only the base64 part (remove data:image/png;base64, prefix)
      const base64Data = result.split(',')[1];

      if (!base64Data) {
        this.alertService.showMessage('Invalid image data', '', MessageSeverity.error);
        return;
      }

      this.currentAvatarUrl = result; // Full data URL for preview (includes prefix)

      // Populate form fields
      this.contactForm.patchValue({
        avatarFileName: file.name,
        avatarSize: file.size,
        avatarData: base64Data,         // ← Only the raw base64 string
        avatarMimeType: file.type
      });

      this.contactForm.markAsDirty();
    };

    reader.onerror = () => {
      this.alertService.showMessage('Failed to read file', '', MessageSeverity.error);
    };

    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.currentAvatarUrl = null;
    this.contactForm.patchValue({
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    });
    this.contactForm.markAsDirty();
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.isAvatarPanelOpen = false;
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
        timeZoneId: this.currentUserService.defaultTimeZoneId,
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
        timeZoneId: contactData.timeZoneId ?? this.currentUserService.defaultTimeZoneId,
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

  public userIsSchedulerContactReader(): boolean {
    return this.contactService.userIsSchedulerContactReader();
  }

  public userIsSchedulerContactWriter(): boolean {
    return this.contactService.userIsSchedulerContactWriter();
  }


  public userIsSchedulerAdministrator(): boolean {
    return this.authService.isSchedulerAdministrator;
  }

  public userIsFoundationAdministrator(): boolean {
    return this.authService.isFoundationAdmin;
  }
}

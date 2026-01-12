import { Component, Input, Output, EventEmitter, ViewChild, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { finalize } from 'rxjs';

import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';

import { ClientData } from '../../../scheduler-data-services/client.service';
import { ContactService, ContactSubmitData } from '../../../scheduler-data-services/contact.service';
import { ContactTypeService } from '../../../scheduler-data-services/contact-type.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ClientContactService, ClientContactSubmitData } from '../../../scheduler-data-services/client-contact.service';
import { RelationshipTypeService } from '../../../scheduler-data-services/relationship-type.service';

/**
 * Modal component for adding or editing a ClientContact link.
 *
 * Used from the Client Contacts tab to:
 * - Create a new contact and link it to the client
 * - Link an existing contact to the client
 * - Edit relationship type or primary flag on existing link
 *
 * Supports both modes:
 * - Add: create new Contact + ClientContact
 * - Edit: modify existing ClientContact (relationship, primary)
 */
@Component({
  selector: 'app-client-contact-custom-add-edit-modal',
  templateUrl: './client-contact-custom-add-edit-modal.component.html',
  styleUrls: ['./client-contact-custom-add-edit-modal.component.scss']
})
export class ClientContactCustomAddEditModalComponent {
  @ViewChild('clientContactModal') clientContactModal!: TemplateRef<any>;

  // Optional pre-configured client (when opened from client detail)
  @Input() client: ClientData | null = null;

  // Output when a change occurs (used by parent to refresh)
  @Output() contactChanged = new EventEmitter<void>();

  public contactForm: FormGroup;
  public isEditMode = false;
  public isSaving = false;

  // Dropdown data
  public contacts$ = this.contactService.GetContactList();
  public relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();
  public contactTypes$ = this.contactTypeService.GetContactTypeList();
  public icons$ = this.iconService.GetIconList();

  constructor(
    private fb: FormBuilder,
    public activeModal: NgbActiveModal,
    private modalService: NgbModal,
    private contactService: ContactService,
    private clientContactService: ClientContactService,
    private relationshipTypeService: RelationshipTypeService,
    private contactTypeService: ContactTypeService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router
  ) {
    // Form definition — all fields required for new contact; minimal for link-only edit
    this.contactForm = this.fb.group({
      contactId: [null], // For linking existing contact
      createNewContact: [false], // Toggle: create new vs link existing   - Default to pick existing
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      middleName: [''],
      email: [''],
      mobile: [''],
      phone: [''],
      company: [''],
      title: [''],
      position: [''],
      notes: [''],
      iconId: [null],
      color: [''],
      contactTypeId: [null, Validators.required],
      relationshipTypeId: [null, Validators.required],
      isPrimary: [false]
    });

    // Dynamic validation
    this.contactForm.get('createNewContact')?.valueChanges.subscribe(createNew => {
      const firstNameCtrl = this.contactForm.get('firstName');
      const lastNameCtrl = this.contactForm.get('lastName');
      const contactTypeId = this.contactForm.get('contactTypeId');
      const contactIdCtrl = this.contactForm.get('contactId')

      if (createNew) {
        firstNameCtrl?.setValidators(Validators.required);
        lastNameCtrl?.setValidators(Validators.required);
        contactTypeId?.setValidators(Validators.required);
        contactIdCtrl?.clearValidators();   // Not needed when creating new
      } else {
        firstNameCtrl?.clearValidators();
        lastNameCtrl?.clearValidators();
        contactTypeId?.clearValidators();
        contactIdCtrl?.setValidators(Validators.required);
      }

      firstNameCtrl?.updateValueAndValidity();
      lastNameCtrl?.updateValueAndValidity();
      contactTypeId?.updateValueAndValidity();
      contactIdCtrl?.updateValueAndValidity();
    });

    this.contactForm.get('createNewContact')?.updateValueAndValidity();
  }
 
  public submitForm(): void {
    if (this.isSaving) return;

    if (!this.client) {
      this.alertService.showMessage('No client selected', '', MessageSeverity.error);
      return;
    }

    if (!this.contactForm.valid) {
      this.contactForm.markAllAsTouched();
      this.alertService.showMessage('Please fix form errors', '', MessageSeverity.warn);
      return;
    }

    this.isSaving = true;

    const formValue = this.contactForm.getRawValue();

    if (formValue.createNewContact) {
      // Step 1: Create new contact
      const newContact: ContactSubmitData = {
        id: 0,
        firstName: formValue.firstName.trim(),
        middleName: formValue.middleName?.trim() || null,
        lastName: formValue.lastName.trim(),
        email: formValue.email?.trim() || null,
        mobile: formValue.mobile?.trim() || null,
        phone: formValue.phone?.trim() || null,
        company: formValue.company?.trim() || null,
        notes: formValue.notes?.trim() || null,
        title: formValue.title?.trim() || null,
        color: formValue.color?.trim() || null,
        timeZoneId: this.client.timeZoneId,     // reasonable enough to assume that the client contact is in the same time zone as the client..
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        position: formValue.position?.trim() || null,
        webSite: null,
        externalId: null,
        attributes: null,
        salutationId: null,
        birthDate: null,
        contactMethodId: null,
        avatarFileName: null,   // fix this
        avatarMimeType: null,
        avatarData: null,
        avatarSize: null,
        contactTypeId: Number(formValue.contactTypeId),
        versionNumber: 0,
        active: true,
        deleted: false
      };

      this.contactService.PostContact(newContact).pipe(
        finalize(() => this.isSaving = false)
      ).subscribe({
        next: (createdContact) => {
          this.linkContactToClient(createdContact.id as number);
        },
        error: (err) => {
          this.handleSaveError(err);
        }
      });
    } else {
      // Just link existing contact
      this.linkContactToClient(formValue.contactId);
    }
  }

  private linkContactToClient(contactId: number): void {
    const formValue = this.contactForm.getRawValue();

    const linkData: ClientContactSubmitData = {
      id: 0, // Always 0 for new link
      clientId: this.client!.id,
      contactId: contactId,
      relationshipTypeId: Number(formValue.relationshipTypeId),
      isPrimary: formValue.isPrimary,
      versionNumber: 0,
      active: true,
      deleted: false
    };

    this.clientContactService.PostClientContact(linkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: () => {
        this.alertService.showMessage(
          this.isEditMode ? 'Contact link updated' : 'Contact added successfully',
          '',
          MessageSeverity.success
        );
        this.contactChanged.emit();
        this.activeModal.close();
      },
      error: (err) => {
        this.handleSaveError(err);
      }
    });
  }

  public closeModal(): void {
    this.activeModal.dismiss('cancel');
  }

  private handleSaveSuccess(): void {
    this.alertService.showMessage(
      this.isEditMode ? 'Contact link updated' : 'Contact added successfully',
      '',
      MessageSeverity.success
    );
    this.contactChanged.emit();
    this.activeModal.close();
  }

  private handleSaveError(err: any): void {
    let message = 'An error occurred while saving the contact link.';
    if (err?.error?.message) {
      message = err.error.message;
    }
    this.alertService.showMessage('Save failed', message, MessageSeverity.error);
  }

  public userIsSchedulerClientContactWriter(): boolean {
    return this.clientContactService.userIsSchedulerClientContactWriter();
  }
}

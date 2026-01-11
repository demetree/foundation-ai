import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { OfficeData } from '../../../scheduler-data-services/office.service';
import { OfficeContactData } from '../../../scheduler-data-services/office-contact.service';
import { ContactData, ContactService } from '../../../scheduler-data-services/contact.service';
import { ContactCustomAddEditComponent } from '../../contact-custom/contact-custom-add-edit/contact-custom-add-edit.component';
import { OfficeContactAddEditComponent } from '../../../scheduler-data-components/office-contact/office-contact-add-edit/office-contact-add-edit.component'
import { OfficeContactCustomAddEditModalComponent } from '../office-contact-custom-add-edit-modal/office-contact-custom-add-edit-modal.component'

/**
 * Contacts tab for the Office detail page.
 *
 * Displays all contacts linked to this office (e.g., emergency contacts, next-of-kin, personal contacts).
 * Shows relationship type, phone/email, and primary flag.
 *
 * Supports:
 * - Lazy loading via office.OfficeContacts promise
 * - Add new contact modal
 * - Navigation to full contact detail (optional)
 * - Safe empty/loading/error states
 * - Badge count on tab header
 */
@Component({
  selector: 'app-office-contacts-tab',
  templateUrl: './office-contacts-tab.component.html',
  styleUrls: ['./office-contacts-tab.component.scss']
})
export class OfficeContactsTabComponent implements OnChanges {

  @ViewChild(ContactCustomAddEditComponent) addEditContactComponent!: ContactCustomAddEditComponent;
  @ViewChild(OfficeContactAddEditComponent) addEditOfficeContactComponent!: OfficeContactAddEditComponent;

  @Input() office!: OfficeData | null;

  @Output() officeContactChanged = new Subject<OfficeContactData>();
  @Output() contactChanged = new Subject<ContactData>();


  public contacts: OfficeContactData[] | null = null;
  public isLoading = true;
  public error: string | null = null;

  constructor(
    private router: Router,
    private modalService: NgbModal,
    private contactService: ContactService
  ) { }

  /**
   * React to office input changes and load contacts when available.
   */
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['office'] && this.office) {

      this.office.ClearOfficeContactsCache();

      this.loadContacts();
    }
  }

  ngAfterViewInit(): void {

    //
    // Subscribe to the observables on the add/edit components and emit when they emit
    //
    if (this.addEditContactComponent) {
      this.addEditContactComponent.contactChanged.subscribe({
        next: (data: ContactData[] | null) => {

          this.office?.ClearOfficeContactsCache();

          if (data != null && data.length > 0) {
            this.contactChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }

    if (this.addEditOfficeContactComponent) {
      this.addEditOfficeContactComponent.officeContactChanged.subscribe({
        next: (data: OfficeContactData[] | null) => {

          this.office?.ClearOfficeContactsCache();

          if (data != null && data.length > 0) {
            this.officeContactChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }
  }

  /**
   * Loads contacts using the lazy promise on the office object.
   * Ensures child contact and relationshipType are revived for template access.
   */
  public loadContacts(): void {
    if (!this.office) {
      this.contacts = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.office.OfficeContacts
      .then(officeContacts => {
        this.contacts = officeContacts ?? [];


        //
        // Revive contacts 
        //
        this.contacts.forEach(c => {

          //
          // Reydrate the contact because this level of nav property isn't fully constructed yet.  We just have a basic data object for nav properties of nav properties.
          //
          // We need to convert it to a full data object by reviving it, and then calling reload on it, so we then get it's nav property values like icon and contactType
          //
          c.contact = ContactService.Instance.ReviveContact(c.contact);
          c.contact.Reload();
        });

        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load office contacts', err);
        this.error = 'Unable to load contacts';
        this.contacts = [];
        this.isLoading = false;
      });
  }

  /**
   * Opens modal to add a new contact linked to this office.
   */
  public openAddContactModal(): void {
    if (!this.office) return;

    const modalRef = this.modalService.open(OfficeContactCustomAddEditModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    // Pre-configure modal for linking to this office
    modalRef.componentInstance.office = this.office;

    modalRef.result.then(
      (data) => {
        // Success: refresh contacts
        this.office?.ClearOfficeContactsCache();
        this.officeContactChanged.next(data);
        this.loadContacts();
      },
      () => {
        // Dismissed: do nothing
      }
    );
  }

  /**
   * Navigate to full contact detail page (optional feature)
   */
  public navigateToContact(contactId: number | bigint | null | undefined): void {
    if (contactId) {
      this.router.navigate(['/contact', contactId]);
    }
  }

  openContactModal(contact: ContactData | null | undefined): void {

    if (contact == null || contact == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditContactComponent.openModal(contact); // Default edit behavior
  }


  openOfficeContactModal(officeContact: OfficeContactData | null | undefined): void {

    if (officeContact == null || officeContact == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditOfficeContactComponent.openModal(officeContact); // Default edit behavior
  }

  //
  // Force a reload if a office contact changes
  // 
  public reloadOfficeContact(officeContact: OfficeContactData[]): void {

    this.office?.ClearOfficeContactsCache();

    this.loadContacts();
  }


  //
  // Force a reload if a contact changes
  // 
  public reloadContact(contact: ContactData[]): void {

    this.office?.ClearOfficeContactsCache();

    this.loadContacts();
  }


  /**
   * Helper: get best available phone number
   */
  public getPhone(contact: OfficeContactData): string {
    return contact.contact?.mobile || contact.contact?.phone || '—';
  }

  /**
   * Helper: get full name safely
   */
  public getFullName(contact: OfficeContactData): string {
    if (!contact.contact) return 'Unknown';
    const parts = [
      contact.contact.firstName,
      contact.contact.middleName,
      contact.contact.lastName
    ].filter(p => p);
    return parts.join(' ') || 'Unknown';
  }
}

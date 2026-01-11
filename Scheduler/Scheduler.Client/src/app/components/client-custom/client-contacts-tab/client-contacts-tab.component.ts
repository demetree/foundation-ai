import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ClientData } from '../../../scheduler-data-services/client.service';
import { ClientContactData } from '../../../scheduler-data-services/client-contact.service';
import { ContactData, ContactService } from '../../../scheduler-data-services/contact.service';
import { ContactCustomAddEditComponent } from '../../contact-custom/contact-custom-add-edit/contact-custom-add-edit.component';
import { ClientContactAddEditComponent } from '../../../scheduler-data-components/client-contact/client-contact-add-edit/client-contact-add-edit.component'
import { ClientContactCustomAddEditModalComponent } from '../client-contact-custom-add-edit-modal/client-contact-custom-add-edit-modal.component'

/**
 * Contacts tab for the Client detail page.
 *
 * Displays all contacts linked to this client (e.g., emergency contacts, next-of-kin, personal contacts).
 * Shows relationship type, phone/email, and primary flag.
 *
 * Supports:
 * - Lazy loading via client.ClientContacts promise
 * - Add new contact modal
 * - Navigation to full contact detail (optional)
 * - Safe empty/loading/error states
 * - Badge count on tab header
 */
@Component({
  selector: 'app-client-contacts-tab',
  templateUrl: './client-contacts-tab.component.html',
  styleUrls: ['./client-contacts-tab.component.scss']
})
export class ClientContactsTabComponent implements OnChanges {

  @ViewChild(ContactCustomAddEditComponent) addEditContactComponent!: ContactCustomAddEditComponent;
  @ViewChild(ClientContactAddEditComponent) addEditClientContactComponent!: ClientContactAddEditComponent;

  @Input() client!: ClientData | null;

  @Output() clientContactChanged = new Subject<ClientContactData>();
  @Output() contactChanged = new Subject<ContactData>();


  public contacts: ClientContactData[] | null = null;
  public isLoading = true;
  public error: string | null = null;

  constructor(
    private router: Router,
    private modalService: NgbModal,
    private contactService: ContactService
  ) { }

  /**
   * React to client input changes and load contacts when available.
   */
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['client'] && this.client) {

      this.client.ClearClientContactsCache();

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

          this.client?.ClearClientContactsCache();

          if (data != null && data.length > 0) {
            this.contactChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }

    if (this.addEditClientContactComponent) {
      this.addEditClientContactComponent.clientContactChanged.subscribe({
        next: (data: ClientContactData[] | null) => {

          this.client?.ClearClientContactsCache();

          if (data != null && data.length > 0) {
            this.clientContactChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }
  }

  /**
   * Loads contacts using the lazy promise on the client object.
   * Ensures child contact and relationshipType are revived for template access.
   */
  public loadContacts(): void {
    if (!this.client) {
      this.contacts = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.client.ClientContacts
      .then(clientContacts => {
        this.contacts = clientContacts ?? [];


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
        console.error('Failed to load client contacts', err);
        this.error = 'Unable to load contacts';
        this.contacts = [];
        this.isLoading = false;
      });
  }

  /**
   * Opens modal to add a new contact linked to this client.
   */
  public openAddContactModal(): void {
    if (!this.client) return;

    const modalRef = this.modalService.open(ClientContactCustomAddEditModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    // Pre-configure modal for linking to this client
    modalRef.componentInstance.client = this.client;

    modalRef.result.then(
      (data) => {
        // Success: refresh contacts
        this.client?.ClearClientContactsCache();
        this.clientContactChanged.next(data);
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


  openClientContactModal(clientContact: ClientContactData | null | undefined): void {

    if (clientContact == null || clientContact == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditClientContactComponent.openModal(clientContact); // Default edit behavior
  }

  //
  // Force a reload if a client contact changes
  // 
  public reloadClientContact(clientContact: ClientContactData[]): void {

    this.client?.ClearClientContactsCache();

    this.loadContacts();
  }


  //
  // Force a reload if a contact changes
  // 
  public reloadContact(contact: ContactData[]): void {

    this.client?.ClearClientContactsCache();

    this.loadContacts();
  }


  /**
   * Helper: get best available phone number
   */
  public getPhone(contact: ClientContactData): string {
    return contact.contact?.mobile || contact.contact?.phone || '—';
  }

  /**
   * Helper: get full name safely
   */
  public getFullName(contact: ClientContactData): string {
    if (!contact.contact) return 'Unknown';
    const parts = [
      contact.contact.firstName,
      contact.contact.middleName,
      contact.contact.lastName
    ].filter(p => p);
    return parts.join(' ') || 'Unknown';
  }
}

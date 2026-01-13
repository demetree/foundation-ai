import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { ResourceContactData } from '../../../scheduler-data-services/resource-contact.service';
import { ContactData, ContactService } from '../../../scheduler-data-services/contact.service';
import { ContactCustomAddEditComponent } from '../../contact-custom/contact-custom-add-edit/contact-custom-add-edit.component';
import { ResourceContactAddEditComponent } from '../../../scheduler-data-components/resource-contact/resource-contact-add-edit/resource-contact-add-edit.component'
import { ResourceContactCustomAddEditModalComponent } from '../resource-contact-custom-add-edit-modal/resource-contact-custom-add-edit-modal.component'

/**
 * Contacts tab for the Resource detail page.
 *
 * Displays all contacts linked to this resource (e.g., emergency contacts, next-of-kin, personal contacts).
 * Shows relationship type, phone/email, and primary flag.
 *
 * Supports:
 * - Lazy loading via resource.ResourceContacts promise
 * - Add new contact modal
 * - Navigation to full contact detail (optional)
 * - Safe empty/loading/error states
 * - Badge count on tab header
 */
@Component({
  selector: 'app-resource-contacts-tab',
  templateUrl: './resource-contacts-tab.component.html',
  styleUrls: ['./resource-contacts-tab.component.scss']
})
export class ResourceContactsTabComponent implements OnChanges {

  @ViewChild(ContactCustomAddEditComponent) addEditContactComponent!: ContactCustomAddEditComponent;
  @ViewChild(ResourceContactAddEditComponent) addEditResourceContactComponent!: ResourceContactAddEditComponent;

  @Input() resource!: ResourceData | null;

  // Triggers when a resource contact is changed.  To be implemented by users of this component.
  @Output() resourceContactChanged = new Subject<ResourceContactData>();
  @Output() contactChanged = new Subject<ContactData>();

  public contacts: ResourceContactData[] | null = null;
  public isLoading = true;
  public error: string | null = null;

  constructor(
    private router: Router,
    private modalService: NgbModal,
    private contactService: ContactService,
    private cdr: ChangeDetectorRef
  ) { }

  /**
   * React to resource input changes and load contacts when available.
   */
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resource'] && this.resource) {

      this.resource?.ClearResourceContactsCache();

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

          this.resource?.ClearResourceContactsCache();

          if (data != null && data.length > 0) {
            this.contactChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }

    if (this.addEditResourceContactComponent) {
      this.addEditResourceContactComponent.resourceContactChanged.subscribe({
        next: (data: ResourceContactData[] | null) => {

          this.resource?.ClearResourceContactsCache();

          if (data != null && data.length > 0) {
            this.resourceContactChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }
  }


  /**
   * Loads contacts using the lazy promise on the resource object.
   * Ensures child contact and relationshipType are revived for template access.
   */
  public loadContacts(): void {
    if (!this.resource) {
      this.contacts = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.resource.ResourceContacts
      .then(resourceContacts => {
        this.contacts = resourceContacts ?? [];
        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load resource contacts', err);
        this.error = 'Unable to load contacts';
        this.contacts = [];
        this.isLoading = false;
      });
  }

  /**
   * Opens modal to add a new contact linked to this resource.
   */
  public openAddContactModal(): void {
    if (!this.resource) return;

    const modalRef = this.modalService.open(ResourceContactCustomAddEditModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    // Pre-configure modal for linking to this resource
    modalRef.componentInstance.resource = this.resource;

    modalRef.result.then(
      (data) => {
        // Success: refresh contacts
        this.resource?.ClearResourceContactsCache();
        this.resourceContactChanged.next(data);
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


  openResourceContactModal(resourceContact: ResourceContactData | null | undefined): void {

    if (resourceContact == null || resourceContact == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditResourceContactComponent.openModal(resourceContact); // Default edit behavior
  }

  //
  // Force a reload if a resource contact changes
  // 
  public reloadResourceContact(resourceContact: ResourceContactData[]): void {

    this.resource?.ClearResourceContactsCache();

    this.loadContacts();
  }


  //
  // Force a reload if a contact changes
  // 
  public reloadContact(contact: ContactData[]): void {

    this.resource?.ClearResourceContactsCache();

    this.loadContacts();
  }

  /**
   * Helper: get best available phone number
   */
  public getPhone(contact: ResourceContactData): string {
    return contact.contact?.mobile || contact.contact?.phone || '—';
  }
}

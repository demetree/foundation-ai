import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { ContactService, ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactContactData } from '../../../scheduler-data-services/contact-contact.service';
import { AuthService } from '../../../services/auth.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ContactFullNamePipe } from '../../../pipes/contact-full-name.pipe';
// import { ContactRelationshipEditModalComponent } from '../contact-relationship-edit-modal/contact-relationship-edit-modal.component';

@Component({
  selector: 'app-contact-relationships-tab',
  templateUrl: './contact-relationships-tab.component.html',
  styleUrls: ['./contact-relationships-tab.component.scss']
})
export class ContactRelationshipsTabComponent implements OnInit, OnDestroy {
  /**
   * The parent contact whose relationships we are displaying.
   */
  @Input() contact!: ContactData;

  /**
   * Observable stream of relationships for this contact.
   * Uses the lazy-loaded ContactContacts$ from ContactData.
   */
  public relationships$: Observable<ContactContactData[] | null>;

  /**
   * Loading state derived from the stream (null = loading).
   */
  public isLoading$: Observable<boolean>;

  constructor(
    private authService: AuthService,
    private contactService: ContactService,
    private contactFullNamePipe: ContactFullNamePipe,
    private modalService: NgbModal) {
    this.relationships$ = of(null);
    this.isLoading$ = of(true);
  }

  ngOnInit(): void {
    if (!this.contact) {
      console.error('ContactRelationshipsTabComponent: No contact input provided.');
      this.relationships$ = of([]);
      this.isLoading$ = of(false);
      return;
    }

    // Subscribe to the lazy-loaded relationships observable
    this.relationships$ = this.contact.ContactContacts$;

    // Derive loading state: true while data is null
    this.isLoading$ = this.relationships$.pipe(
      map(rels => rels === null)
    );
  }

  ngOnDestroy(): void {
    // No subscriptions to clean up currently, but kept for consistency
  }

  /**
   * Permission check — can the user add/edit/delete relationships?
   */
  public userCanManageRelationships(): boolean {
    // Reuse contact writer permission or create specific one
    return this.contactService.userIsSchedulerContactWriter?.() ?? false;
  }

  /**
   * Opens modal to add a new relationship.
   */
  public openAddRelationshipModal(): void {
    if (!this.userCanManageRelationships()) return;

    // const modalRef = this.modalService.open(ContactRelationshipEditModalComponent, {
    //   size: 'lg',
    //   backdrop: 'static'
    // });
    // modalRef.componentInstance.contact = this.contact;
    // modalRef.componentInstance.relationship = null; // add mode

    // modalRef.result.then(
    //   () => this.contact.ClearContactContactsCache(), // force refresh
    //   () => {}
    // );

    console.log('Opening add relationship modal for contact:', this.contact.id);
    // TODO: Implement modal
  }

  /**
   * Opens modal to edit an existing relationship.
   */
  public openEditRelationshipModal(relationship: ContactContactData): void {
    if (!this.userCanManageRelationships()) return;

    // Similar to add, but pass the relationship object
    console.log('Editing relationship:', relationship);
    // TODO: Implement modal
  }

  /**
   * Deletes a relationship (with confirmation).
   */
  public deleteRelationship(relationship: ContactContactData): void {
    if (!this.userCanManageRelationships()) return;

    if (!confirm(`Delete relationship with ${this.contactFullNamePipe.transform(relationship.relatedContact!)}?`)) {
      return;
    }

    // this.contactContactService.DeleteContactContact(relationship.id).subscribe({
    //   next: () => {
    //     this.contact.ClearContactContactsCache();
    //   }
    // });

    console.log('Deleting relationship:', relationship.id);
    // TODO: Implement delete
  }
}

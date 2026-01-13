import { Component, Input, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, of, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';

import { ContactService, ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactContactData, ContactContactService } from '../../../scheduler-data-services/contact-contact.service';
import { OfficeContactData } from '../../../scheduler-data-services/office-contact.service';
import { ClientContactData } from '../../../scheduler-data-services/client-contact.service';
import { SchedulingTargetContactData } from '../../../scheduler-data-services/scheduling-target-contact.service';
import { ResourceContactData } from '../../../scheduler-data-services/resource-contact.service';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ContactFullNamePipe } from '../../../pipes/contact-full-name.pipe';
import { ContactRelationshipEditModalComponent } from '../contact-relationship-edit-modal/contact-relationship-edit-modal.component';

@Component({
  selector: 'app-contact-relationships-tab',
  templateUrl: './contact-relationships-tab.component.html',
  styleUrls: ['./contact-relationships-tab.component.scss']
})
export class ContactRelationshipsTabComponent implements OnInit, OnDestroy, AfterViewInit {

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

  /**
   * Additional relationship observables
   */
  public officeContacts$: Observable<OfficeContactData[] | null>;
  public clientContacts$: Observable<ClientContactData[] | null>;
  public schedulingTargetContacts$: Observable<SchedulingTargetContactData[] | null>;
  public resourceContacts$: Observable<ResourceContactData[] | null>;

  /**
   * Flag to track if we should auto-open the add modal after view init  
   */
  private shouldAutoOpenAddModal = false;

  /**
   * Cleanup subject
   */
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private contactService: ContactService,
    private contactContactService: ContactContactService,
    private contactFullNamePipe: ContactFullNamePipe,
    private alertService: AlertService,
    private modalService: NgbModal,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.relationships$ = of(null);
    this.officeContacts$ = of(null);
    this.clientContacts$ = of(null);
    this.schedulingTargetContacts$ = of(null);
    this.resourceContacts$ = of(null);
    this.isLoading$ = of(true);
  }


  ngOnInit(): void {

    if (!this.contact) {
      console.error('ContactRelationshipsTabComponent: No contact input provided.');
      this.relationships$ = of([]);
      this.officeContacts$ = of([]);
      this.clientContacts$ = of([]);
      this.schedulingTargetContacts$ = of([]);
      this.resourceContacts$ = of([]);
      this.isLoading$ = of(false);
      return;
    }

    //
    // Check if we should auto-open the add relationship modal (navigated from Overview tab)
    //
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['action'] === 'add' && params['tab'] === 'relationships') {
        this.shouldAutoOpenAddModal = true;
      }
    });

    //
    // Subscribe to the lazy-loaded relationships observable
    //
    this.relationships$ = this.contact.ContactContacts$;
    this.officeContacts$ = this.contact.OfficeContacts$;
    this.clientContacts$ = this.contact.ClientContacts$;
    this.schedulingTargetContacts$ = this.contact.SchedulingTargetContacts$;
    this.resourceContacts$ = this.contact.ResourceContacts$;

    //
    // Derive loading state: true while data is null
    //
    this.isLoading$ = this.relationships$.pipe(
      map(rels => rels === null)
    );
  }


  /**
   * Auto-open modal if navigated from Overview tab with action=add param.
   */
  ngAfterViewInit(): void {

    if (this.shouldAutoOpenAddModal && this.contact) {
      setTimeout(() => {
        this.openAddRelationshipModal();

        //
        // Clear the query param so modal doesn't reopen on tab switch
        //
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { action: null },
          queryParamsHandling: 'merge',
          replaceUrl: true
        });

        this.shouldAutoOpenAddModal = false;
      }, 0);
    }
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  /**
   * Permission check — can the user add/edit/delete relationships?
   */
  public userCanManageRelationships(): boolean {
    return this.contactContactService.userIsSchedulerContactContactWriter?.() ?? false;
  }


  /**
   * Opens modal to add a new relationship.
   */
  public openAddRelationshipModal(): void {

    if (!this.userCanManageRelationships()) {
      this.alertService.showMessage(
        'You do not have permission to add relationships.',
        'Access Denied',
        MessageSeverity.warn
      );
      return;
    }

    const modalRef = this.modalService.open(ContactRelationshipEditModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    modalRef.componentInstance.contact = this.contact;
    modalRef.componentInstance.relationship = null; // add mode

    modalRef.result.then(
      () => {
        //
        // Force refresh of relationships list after save
        //
        this.contact.ClearContactContactsCache();
      },
      () => {
        // Dismissed — no action needed
      }
    );
  }


  /**
   * Opens modal to edit an existing relationship.
   */
  public openEditRelationshipModal(relationship: ContactContactData): void {

    if (!this.userCanManageRelationships()) {
      this.alertService.showMessage(
        'You do not have permission to edit relationships.',
        'Access Denied',
        MessageSeverity.warn
      );
      return;
    }

    const modalRef = this.modalService.open(ContactRelationshipEditModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    modalRef.componentInstance.contact = this.contact;
    modalRef.componentInstance.relationship = relationship; // edit mode

    modalRef.result.then(
      () => {
        //
        // Force refresh of relationships list after save
        //
        this.contact.ClearContactContactsCache();
      },
      () => {
        // Dismissed — no action needed
      }
    );
  }


  /**
   * Deletes a relationship (with confirmation).
   */
  public deleteRelationship(relationship: ContactContactData): void {

    if (!this.userCanManageRelationships()) {
      this.alertService.showMessage(
        'You do not have permission to delete relationships.',
        'Access Denied',
        MessageSeverity.warn
      );
      return;
    }

    const relatedName = this.contactFullNamePipe.transform(relationship.relatedContact!);

    if (!confirm(`Are you sure you want to delete the relationship with ${relatedName}?`)) {
      return;
    }

    //
    // Soft delete by setting deleted=true
    //
    const submitData = relationship.ConvertToSubmitData();
    submitData.deleted = true;

    this.contactContactService.PutContactContact(relationship.id, submitData).subscribe({
      next: () => {
        this.alertService.showMessage(
          `Relationship with ${relatedName} deleted.`,
          '',
          MessageSeverity.success
        );
        //
        // Force refresh of relationships list
        //
        this.contact.ClearContactContactsCache();
      },
      error: (err) => {
        this.alertService.showMessage(
          'Failed to delete relationship.',
          err.message || 'Unknown error',
          MessageSeverity.error
        );
      }
    });
  }
}


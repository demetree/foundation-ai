/*
 * Contact Relationship Edit Modal Component
 * 
 * AI-developed: Modal for adding/editing personal relationships (ContactContact) between contacts.
 * Follows the same pattern as contact-interaction-edit-modal.
 */
import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable, Subject, debounceTime, distinctUntilChanged, switchMap, of, catchError } from 'rxjs';

import { ContactData, ContactService } from '../../../scheduler-data-services/contact.service';
import { ContactContactData, ContactContactService, ContactContactSubmitData } from '../../../scheduler-data-services/contact-contact.service';
import { RelationshipTypeService, RelationshipTypeData } from '../../../scheduler-data-services/relationship-type.service';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-contact-relationship-edit-modal',
    templateUrl: './contact-relationship-edit-modal.component.html',
    styleUrls: ['./contact-relationship-edit-modal.component.scss']
})
export class ContactRelationshipEditModalComponent implements OnInit, OnDestroy {

    /**
     * The contact this relationship belongs to (the "source" contact).
     * Passed from the parent when opening the modal.
     */
    @Input() contact!: ContactData;

    /**
     * Optional existing relationship to edit.
     * If null → Add mode
     * If provided → Edit mode
     */
    @Input() relationship: ContactContactData | null = null;

    /**
     * Form group for the relationship data.
     */
    public relationshipForm: FormGroup;

    /**
     * List of available relationship types (loaded once).
     */
    public relationshipTypes$: Observable<RelationshipTypeData[]>;

    /**
     * Search results for the related contact typeahead.
     */
    public contactSearchResults$: Observable<ContactData[]>;

    /**
     * Selected related contact for display.
     */
    public selectedRelatedContact: ContactData | null = null;

    /**
     * Controls visibility of the search results dropdown.
     * Separate from results to prevent "No contacts found" showing inappropriately.
     */
    public showDropdown: boolean = false;

    /**
     * Search term input subject for typeahead debouncing.
     */
    private searchTerms$ = new Subject<string>();

    /**
     * Tracks whether we're currently saving to prevent double-submit.
     */
    public isSaving: boolean = false;

    /**
     * Determines if this is add mode (true) or edit mode (false).
     */
    public get isAddMode(): boolean {
        return this.relationship === null;
    }

    /**
     * Cleanup subject for subscriptions.
     */
    private destroy$ = new Subject<void>();

    constructor(
        private fb: FormBuilder,
        private activeModal: NgbActiveModal,
        private contactContactService: ContactContactService,
        private contactService: ContactService,
        private relationshipTypeService: RelationshipTypeService,
        private alertService: AlertService,
        private authService: AuthService
    ) {
        //
        // Initialize form with default controls
        //
        this.relationshipForm = this.fb.group({
            relatedContactId: [null, Validators.required],
            relationshipTypeId: [null, Validators.required],
            isPrimary: [false]
        });

        //
        // Load relationship types
        //
        this.relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList({ active: true, deleted: false });

        //
        // Setup contact search with debounce
        //
        this.contactSearchResults$ = this.searchTerms$.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            switchMap(term => {
                if (term.length < 2) {
                    return of([]);
                }
                return this.contactService.GetContactList({ anyStringContains: term, active: true, deleted: false, pageSize: 10 }).pipe(
                    catchError(() => of([]))
                );
            })
        );
    }


    /**
     * Lifecycle hook — called after inputs are set.
     */
    ngOnInit(): void {

        if (!this.contact) {
            this.alertService.showMessage(
                'No contact provided to add relationship.',
                'Invalid State',
                MessageSeverity.error
            );
            this.activeModal.dismiss('no contact');
            return;
        }

        //
        // If editing existing relationship, patch form values
        //
        if (this.relationship) {
            this.relationshipForm.patchValue({
                relatedContactId: this.relationship.relatedContactId,
                relationshipTypeId: this.relationship.relationshipTypeId,
                isPrimary: this.relationship.isPrimary
            });

            //
            // Set the selected contact for display
            //
            if (this.relationship.relatedContact) {
                this.selectedRelatedContact = this.relationship.relatedContact as ContactData;
            }
        }
    }


    /**
     * Lifecycle cleanup.
     */
    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    /**
     * Handles the search input for related contact typeahead.
     */
    public onSearchInput(event: Event): void {
        const input = event.target as HTMLInputElement;
        const term = input.value;

        //
        // Show dropdown only when there's enough text to search
        //
        this.showDropdown = term.length >= 2;

        this.searchTerms$.next(term);
    }


    /**
     * Selects a contact from the search results.
     */
    public selectContact(contact: ContactData): void {
        this.selectedRelatedContact = contact;
        this.relationshipForm.patchValue({ relatedContactId: contact.id });
        this.showDropdown = false; // Hide dropdown immediately
    }


    /**
     * Clears the selected contact.
     */
    public clearSelectedContact(): void {
        this.selectedRelatedContact = null;
        this.relationshipForm.patchValue({ relatedContactId: null });
    }


    /**
     * Gets the display name for a contact.
     */
    public getContactDisplayName(contact: ContactData): string {
        const name = `${contact.firstName || ''} ${contact.lastName || ''}`.trim();
        if (contact.company) {
            return `${name} (${contact.company})`;
        }
        return name;
    }


    /**
     * Permission check — can the current user create/edit relationships?
     */
    public userCanSaveRelationship(): boolean {
        return this.contactContactService.userIsSchedulerContactContactWriter?.() ?? false;
    }


    /**
     * Close modal without saving.
     */
    public cancel(): void {
        this.activeModal.dismiss('cancel');
    }


    /**
     * Submit the form — creates or updates the relationship.
     */
    public submit(): void {

        if (this.isSaving) {
            return; // Prevent double-submit
        }

        if (!this.userCanSaveRelationship()) {
            this.alertService.showMessage(
                'You do not have permission to save contact relationships.',
                'Access Denied',
                MessageSeverity.warn
            );
            return;
        }

        if (!this.relationshipForm.valid) {
            this.relationshipForm.markAllAsTouched();
            this.alertService.showMessage(
                'Please correct the errors in the form.',
                'Validation Error',
                MessageSeverity.warn
            );
            return;
        }

        //
        // Prevent self-relationships
        //
        const formValue = this.relationshipForm.getRawValue();
        if (Number(formValue.relatedContactId) === Number(this.contact.id)) {
            this.alertService.showMessage(
                'A contact cannot have a relationship with themselves.',
                'Validation Error',
                MessageSeverity.warn
            );
            return;
        }

        this.isSaving = true;

        const submitData: ContactContactSubmitData = {
            id: this.relationship?.id || 0,
            contactId: this.contact.id,
            relatedContactId: Number(formValue.relatedContactId),
            relationshipTypeId: Number(formValue.relationshipTypeId),
            isPrimary: formValue.isPrimary ?? false,
            versionNumber: this.relationship?.versionNumber ?? 0,
            active: true,
            deleted: false
        };

        const saveOperation = this.isAddMode
            ? this.contactContactService.PostContactContact(submitData)
            : this.contactContactService.PutContactContact(submitData.id, submitData);

        saveOperation.subscribe({
            next: (savedRelationship) => {
                this.alertService.showMessage(
                    `Relationship ${this.isAddMode ? 'added' : 'updated'} successfully.`,
                    '',
                    MessageSeverity.success
                );
                //
                // Close modal and return the saved object for parent to refresh
                //
                this.activeModal.close(savedRelationship);
            },
            error: (err) => {
                this.alertService.showMessage(
                    'Failed to save relationship.',
                    err.message || 'Unknown error',
                    MessageSeverity.error
                );
                this.isSaving = false;
            }
        });
    }
}

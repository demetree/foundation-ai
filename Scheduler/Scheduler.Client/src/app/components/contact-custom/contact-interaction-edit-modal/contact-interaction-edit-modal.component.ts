import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable, Subject } from 'rxjs';

import { ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactInteractionData, ContactInteractionService, ContactInteractionSubmitData } from '../../../scheduler-data-services/contact-interaction.service';
import { InteractionTypeService, InteractionTypeData } from '../../../scheduler-data-services/interaction-type.service'; // Adjust path if needed
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-contact-interaction-edit-modal',
  templateUrl: './contact-interaction-edit-modal.component.html',
  styleUrls: ['./contact-interaction-edit-modal.component.scss']
})
export class ContactInteractionEditModalComponent implements OnInit, OnDestroy {
  /**
   * The contact this interaction belongs to.
   * Passed from the parent (contact detail page) when opening the modal.
   */
  @Input() contact!: ContactData;

  /**
   * Optional existing interaction to edit.
   * If null → Add mode
   * If provided → Edit mode
   */
  @Input() interaction: ContactInteractionData | null = null;

  /**
   * Form group for the interaction data.
   */
  public interactionForm: FormGroup;

  /**
   * List of available interaction types (loaded once).
   */
  public interactionTypes$: Observable<InteractionTypeData[]>;

  /**
   * Tracks whether we're currently saving to prevent double-submit.
   */
  public isSaving: boolean = false;

  /**
   * Determines if this is add mode (true) or edit mode (false).
   */
  public get isAddMode(): boolean {
    return this.interaction === null;
  }

  /**
   * Cleanup subject for subscriptions.
   */
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private activeModal: NgbActiveModal,
    private contactInteractionService: ContactInteractionService,
    private interactionTypeService: InteractionTypeService,
    private alertService: AlertService,
    private authService: AuthService
  ) {
    // Initialize form with default controls
    this.interactionForm = this.fb.group({
      interactionTypeId: [null, Validators.required],
      startTime: [new Date().toISOString().slice(0, 16), Validators.required], // datetime-local format
      endTime: [null],
      notes: [''],
      location: ['']
    });

    // Load interaction types (assuming you have a service method)
    this.interactionTypes$ = this.interactionTypeService.GetInteractionTypeList();
  }

  /**
   * Lifecycle hook — called after inputs are set.
   */
  ngOnInit(): void {
    if (!this.contact) {
      this.alertService.showMessage(
        'No contact provided to log interaction.',
        'Invalid State',
        MessageSeverity.error
      );
      this.activeModal.dismiss('no contact');
      return;
    }

    // If editing existing interaction, patch form values
    if (this.interaction) {
      this.interactionForm.patchValue({
        interactionTypeId: this.interaction.interactionTypeId,
        startTime: this.interaction.startTime ? this.interaction.startTime.slice(0, 16) : null,
        endTime: this.interaction.endTime ? this.interaction.endTime.slice(0, 16) : null,
        notes: this.interaction.notes || '',
        location: this.interaction.location || ''
      });
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
   * Permission check — can the current user create/edit interactions?
   */
  public userCanSaveInteraction(): boolean {

    return this.contactInteractionService.userIsSchedulerContactInteractionWriter?.() ?? false;
  }

  /**
   * Close modal without saving.
   */
  public cancel(): void {
    this.activeModal.dismiss('cancel');
  }

  /**
   * Submit the form — creates or updates the interaction.
   */
  public submit(): void {
    if (this.isSaving) {
      return; // Prevent double-submit
    }

    if (!this.userCanSaveInteraction()) {
      this.alertService.showMessage(
        'You do not have permission to save contact interactions.',
        'Access Denied',
        MessageSeverity.warn
      );
      return;
    }

    if (!this.interactionForm.valid) {
      this.interactionForm.markAllAsTouched();
      this.alertService.showMessage(
        'Please correct the errors in the form.',
        'Validation Error',
        MessageSeverity.warn
      );
      return;
    }

    this.isSaving = true;

    const formValue = this.interactionForm.getRawValue();

    const submitData: ContactInteractionSubmitData = {
      id: this.interaction?.id || 0,
      contactId: this.contact.id,
      interactionTypeId: Number(formValue.interactionTypeId),
      startTime: formValue.startTime ? new Date(formValue.startTime).toISOString() : "",
      endTime: formValue.endTime ? new Date(formValue.endTime).toISOString() : null,
      notes: formValue.notes?.trim() || null,
      location: formValue.location?.trim() || null,

      // review and fix these - add UI elements if need be.
      priorityId: null,
      initiatingContactId: null,    // probably need to add this
      scheduledEventId: null,
      externalId: null,

      versionNumber: this.interaction?.versionNumber ?? 0,
      active: true,
      deleted: false
    };

    const saveOperation = this.isAddMode
      ? this.contactInteractionService.PostContactInteraction(submitData)
      : this.contactInteractionService.PutContactInteraction(submitData.id, submitData);

    saveOperation.subscribe({
      next: (savedInteraction) => {
        this.alertService.showMessage(
          `Interaction ${this.isAddMode ? 'logged' : 'updated'} successfully.`,
          '',
          MessageSeverity.success
        );
        // Close modal and return the saved object for parent to refresh
        this.activeModal.close(savedInteraction);
      },
      error: (err) => {
        this.alertService.showMessage(
          'Failed to save interaction.',
          err.message || 'Unknown error',
          MessageSeverity.error
        );
        this.isSaving = false;
      }
    });
  }
}

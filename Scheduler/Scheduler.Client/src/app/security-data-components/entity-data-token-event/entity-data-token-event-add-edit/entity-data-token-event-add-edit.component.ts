/*
   GENERATED FORM FOR THE ENTITYDATATOKENEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EntityDataTokenEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to entity-data-token-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EntityDataTokenEventService, EntityDataTokenEventData, EntityDataTokenEventSubmitData } from '../../../security-data-services/entity-data-token-event.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { EntityDataTokenService } from '../../../security-data-services/entity-data-token.service';
import { EntityDataTokenEventTypeService } from '../../../security-data-services/entity-data-token-event-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface EntityDataTokenEventFormValues {
  entityDataTokenId: number | bigint,       // For FK link number
  entityDataTokenEventTypeId: number | bigint,       // For FK link number
  timeStamp: string,
  comments: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-entity-data-token-event-add-edit',
  templateUrl: './entity-data-token-event-add-edit.component.html',
  styleUrls: ['./entity-data-token-event-add-edit.component.scss']
})
export class EntityDataTokenEventAddEditComponent {
  @ViewChild('entityDataTokenEventModal') entityDataTokenEventModal!: TemplateRef<any>;
  @Output() entityDataTokenEventChanged = new Subject<EntityDataTokenEventData[]>();
  @Input() entityDataTokenEventSubmitData: EntityDataTokenEventSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EntityDataTokenEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public entityDataTokenEventForm: FormGroup = this.fb.group({
        entityDataTokenId: [null, Validators.required],
        entityDataTokenEventTypeId: [null, Validators.required],
        timeStamp: ['', Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  entityDataTokenEvents$ = this.entityDataTokenEventService.GetEntityDataTokenEventList();
  entityDataTokens$ = this.entityDataTokenService.GetEntityDataTokenList();
  entityDataTokenEventTypes$ = this.entityDataTokenEventTypeService.GetEntityDataTokenEventTypeList();

  constructor(
    private modalService: NgbModal,
    private entityDataTokenEventService: EntityDataTokenEventService,
    private entityDataTokenService: EntityDataTokenService,
    private entityDataTokenEventTypeService: EntityDataTokenEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(entityDataTokenEventData?: EntityDataTokenEventData) {

    if (entityDataTokenEventData != null) {

      if (!this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Entity Data Token Events`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.entityDataTokenEventSubmitData = this.entityDataTokenEventService.ConvertToEntityDataTokenEventSubmitData(entityDataTokenEventData);
      this.isEditMode = true;

      this.buildFormValues(entityDataTokenEventData);

    } else {

      if (!this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Entity Data Token Events`,
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
        this.entityDataTokenEventForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.entityDataTokenEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.entityDataTokenEventModal, {
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

    if (this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Entity Data Token Events`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.entityDataTokenEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.entityDataTokenEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.entityDataTokenEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const entityDataTokenEventSubmitData: EntityDataTokenEventSubmitData = {
        id: this.entityDataTokenEventSubmitData?.id || 0,
        entityDataTokenId: Number(formValue.entityDataTokenId),
        entityDataTokenEventTypeId: Number(formValue.entityDataTokenEventTypeId),
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEntityDataTokenEvent(entityDataTokenEventSubmitData);
      } else {
        this.addEntityDataTokenEvent(entityDataTokenEventSubmitData);
      }
  }

  private addEntityDataTokenEvent(entityDataTokenEventData: EntityDataTokenEventSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    entityDataTokenEventData.active = true;
    entityDataTokenEventData.deleted = false;
    this.entityDataTokenEventService.PostEntityDataTokenEvent(entityDataTokenEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEntityDataTokenEvent) => {

        this.entityDataTokenEventService.ClearAllCaches();

        this.entityDataTokenEventChanged.next([newEntityDataTokenEvent]);

        this.alertService.showMessage("Entity Data Token Event added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/entitydatatokenevent', newEntityDataTokenEvent.id]);
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
                                   'You do not have permission to save this Entity Data Token Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEntityDataTokenEvent(entityDataTokenEventData: EntityDataTokenEventSubmitData) {
    this.entityDataTokenEventService.PutEntityDataTokenEvent(entityDataTokenEventData.id, entityDataTokenEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEntityDataTokenEvent) => {

        this.entityDataTokenEventService.ClearAllCaches();

        this.entityDataTokenEventChanged.next([updatedEntityDataTokenEvent]);

        this.alertService.showMessage("Entity Data Token Event updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Entity Data Token Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(entityDataTokenEventData: EntityDataTokenEventData | null) {

    if (entityDataTokenEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.entityDataTokenEventForm.reset({
        entityDataTokenId: null,
        entityDataTokenEventTypeId: null,
        timeStamp: '',
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.entityDataTokenEventForm.reset({
        entityDataTokenId: entityDataTokenEventData.entityDataTokenId,
        entityDataTokenEventTypeId: entityDataTokenEventData.entityDataTokenEventTypeId,
        timeStamp: isoUtcStringToDateTimeLocal(entityDataTokenEventData.timeStamp) ?? '',
        comments: entityDataTokenEventData.comments ?? '',
        active: entityDataTokenEventData.active ?? true,
        deleted: entityDataTokenEventData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.entityDataTokenEventForm.markAsPristine();
    this.entityDataTokenEventForm.markAsUntouched();
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


  public userIsSecurityEntityDataTokenEventReader(): boolean {
    return this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventReader();
  }

  public userIsSecurityEntityDataTokenEventWriter(): boolean {
    return this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventWriter();
  }
}

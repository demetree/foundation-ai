/*
   GENERATED FORM FOR THE ENTITYDATATOKENEVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EntityDataTokenEventType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to entity-data-token-event-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EntityDataTokenEventTypeService, EntityDataTokenEventTypeData, EntityDataTokenEventTypeSubmitData } from '../../../security-data-services/entity-data-token-event-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface EntityDataTokenEventTypeFormValues {
  name: string,
  description: string | null,
};

@Component({
  selector: 'app-entity-data-token-event-type-add-edit',
  templateUrl: './entity-data-token-event-type-add-edit.component.html',
  styleUrls: ['./entity-data-token-event-type-add-edit.component.scss']
})
export class EntityDataTokenEventTypeAddEditComponent {
  @ViewChild('entityDataTokenEventTypeModal') entityDataTokenEventTypeModal!: TemplateRef<any>;
  @Output() entityDataTokenEventTypeChanged = new Subject<EntityDataTokenEventTypeData[]>();
  @Input() entityDataTokenEventTypeSubmitData: EntityDataTokenEventTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EntityDataTokenEventTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public entityDataTokenEventTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  entityDataTokenEventTypes$ = this.entityDataTokenEventTypeService.GetEntityDataTokenEventTypeList();

  constructor(
    private modalService: NgbModal,
    private entityDataTokenEventTypeService: EntityDataTokenEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(entityDataTokenEventTypeData?: EntityDataTokenEventTypeData) {

    if (entityDataTokenEventTypeData != null) {

      if (!this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Entity Data Token Event Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.entityDataTokenEventTypeSubmitData = this.entityDataTokenEventTypeService.ConvertToEntityDataTokenEventTypeSubmitData(entityDataTokenEventTypeData);
      this.isEditMode = true;

      this.buildFormValues(entityDataTokenEventTypeData);

    } else {

      if (!this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Entity Data Token Event Types`,
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
        this.entityDataTokenEventTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.entityDataTokenEventTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.entityDataTokenEventTypeModal, {
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

    if (this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Entity Data Token Event Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.entityDataTokenEventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.entityDataTokenEventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.entityDataTokenEventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const entityDataTokenEventTypeSubmitData: EntityDataTokenEventTypeSubmitData = {
        id: this.entityDataTokenEventTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateEntityDataTokenEventType(entityDataTokenEventTypeSubmitData);
      } else {
        this.addEntityDataTokenEventType(entityDataTokenEventTypeSubmitData);
      }
  }

  private addEntityDataTokenEventType(entityDataTokenEventTypeData: EntityDataTokenEventTypeSubmitData) {
    this.entityDataTokenEventTypeService.PostEntityDataTokenEventType(entityDataTokenEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEntityDataTokenEventType) => {

        this.entityDataTokenEventTypeService.ClearAllCaches();

        this.entityDataTokenEventTypeChanged.next([newEntityDataTokenEventType]);

        this.alertService.showMessage("Entity Data Token Event Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/entitydatatokeneventtype', newEntityDataTokenEventType.id]);
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
                                   'You do not have permission to save this Entity Data Token Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEntityDataTokenEventType(entityDataTokenEventTypeData: EntityDataTokenEventTypeSubmitData) {
    this.entityDataTokenEventTypeService.PutEntityDataTokenEventType(entityDataTokenEventTypeData.id, entityDataTokenEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEntityDataTokenEventType) => {

        this.entityDataTokenEventTypeService.ClearAllCaches();

        this.entityDataTokenEventTypeChanged.next([updatedEntityDataTokenEventType]);

        this.alertService.showMessage("Entity Data Token Event Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Entity Data Token Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(entityDataTokenEventTypeData: EntityDataTokenEventTypeData | null) {

    if (entityDataTokenEventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.entityDataTokenEventTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.entityDataTokenEventTypeForm.reset({
        name: entityDataTokenEventTypeData.name ?? '',
        description: entityDataTokenEventTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.entityDataTokenEventTypeForm.markAsPristine();
    this.entityDataTokenEventTypeForm.markAsUntouched();
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


  public userIsSecurityEntityDataTokenEventTypeReader(): boolean {
    return this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeReader();
  }

  public userIsSecurityEntityDataTokenEventTypeWriter(): boolean {
    return this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeWriter();
  }
}

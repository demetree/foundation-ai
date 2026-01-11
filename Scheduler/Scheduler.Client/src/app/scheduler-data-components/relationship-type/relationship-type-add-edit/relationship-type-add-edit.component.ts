/*
   GENERATED FORM FOR THE RELATIONSHIPTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RelationshipType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to relationship-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RelationshipTypeService, RelationshipTypeData, RelationshipTypeSubmitData } from '../../../scheduler-data-services/relationship-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RelationshipTypeFormValues {
  name: string,
  description: string,
  isEmergencyEligible: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-relationship-type-add-edit',
  templateUrl: './relationship-type-add-edit.component.html',
  styleUrls: ['./relationship-type-add-edit.component.scss']
})
export class RelationshipTypeAddEditComponent {
  @ViewChild('relationshipTypeModal') relationshipTypeModal!: TemplateRef<any>;
  @Output() relationshipTypeChanged = new Subject<RelationshipTypeData[]>();
  @Input() relationshipTypeSubmitData: RelationshipTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RelationshipTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public relationshipTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isEmergencyEligible: [false],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private relationshipTypeService: RelationshipTypeService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(relationshipTypeData?: RelationshipTypeData) {

    if (relationshipTypeData != null) {

      if (!this.relationshipTypeService.userIsSchedulerRelationshipTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Relationship Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.relationshipTypeSubmitData = this.relationshipTypeService.ConvertToRelationshipTypeSubmitData(relationshipTypeData);
      this.isEditMode = true;
      this.objectGuid = relationshipTypeData.objectGuid;

      this.buildFormValues(relationshipTypeData);

    } else {

      if (!this.relationshipTypeService.userIsSchedulerRelationshipTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Relationship Types`,
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
        this.relationshipTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.relationshipTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.relationshipTypeModal, {
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

    if (this.relationshipTypeService.userIsSchedulerRelationshipTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Relationship Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.relationshipTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.relationshipTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.relationshipTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const relationshipTypeSubmitData: RelationshipTypeSubmitData = {
        id: this.relationshipTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isEmergencyEligible: !!formValue.isEmergencyEligible,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRelationshipType(relationshipTypeSubmitData);
      } else {
        this.addRelationshipType(relationshipTypeSubmitData);
      }
  }

  private addRelationshipType(relationshipTypeData: RelationshipTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    relationshipTypeData.active = true;
    relationshipTypeData.deleted = false;
    this.relationshipTypeService.PostRelationshipType(relationshipTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRelationshipType) => {

        this.relationshipTypeService.ClearAllCaches();

        this.relationshipTypeChanged.next([newRelationshipType]);

        this.alertService.showMessage("Relationship Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/relationshiptype', newRelationshipType.id]);
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
                                   'You do not have permission to save this Relationship Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Relationship Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Relationship Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRelationshipType(relationshipTypeData: RelationshipTypeSubmitData) {
    this.relationshipTypeService.PutRelationshipType(relationshipTypeData.id, relationshipTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRelationshipType) => {

        this.relationshipTypeService.ClearAllCaches();

        this.relationshipTypeChanged.next([updatedRelationshipType]);

        this.alertService.showMessage("Relationship Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Relationship Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Relationship Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Relationship Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(relationshipTypeData: RelationshipTypeData | null) {

    if (relationshipTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.relationshipTypeForm.reset({
        name: '',
        description: '',
        isEmergencyEligible: false,
        sequence: '',
        iconId: null,
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.relationshipTypeForm.reset({
        name: relationshipTypeData.name ?? '',
        description: relationshipTypeData.description ?? '',
        isEmergencyEligible: relationshipTypeData.isEmergencyEligible ?? false,
        sequence: relationshipTypeData.sequence?.toString() ?? '',
        iconId: relationshipTypeData.iconId,
        color: relationshipTypeData.color ?? '',
        active: relationshipTypeData.active ?? true,
        deleted: relationshipTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.relationshipTypeForm.markAsPristine();
    this.relationshipTypeForm.markAsUntouched();
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


  public userIsSchedulerRelationshipTypeReader(): boolean {
    return this.relationshipTypeService.userIsSchedulerRelationshipTypeReader();
  }

  public userIsSchedulerRelationshipTypeWriter(): boolean {
    return this.relationshipTypeService.userIsSchedulerRelationshipTypeWriter();
  }
}

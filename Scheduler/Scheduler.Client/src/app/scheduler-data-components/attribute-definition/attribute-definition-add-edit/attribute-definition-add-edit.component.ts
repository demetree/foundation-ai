/*
   GENERATED FORM FOR THE ATTRIBUTEDEFINITION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AttributeDefinition table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to attribute-definition-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AttributeDefinitionService, AttributeDefinitionData, AttributeDefinitionSubmitData } from '../../../scheduler-data-services/attribute-definition.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AttributeDefinitionEntityService } from '../../../scheduler-data-services/attribute-definition-entity.service';
import { AttributeDefinitionTypeService } from '../../../scheduler-data-services/attribute-definition-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AttributeDefinitionFormValues {
  attributeDefinitionEntityId: number | bigint | null,       // For FK link number
  key: string | null,
  label: string | null,
  attributeDefinitionTypeId: number | bigint | null,       // For FK link number
  options: string | null,
  isRequired: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-attribute-definition-add-edit',
  templateUrl: './attribute-definition-add-edit.component.html',
  styleUrls: ['./attribute-definition-add-edit.component.scss']
})
export class AttributeDefinitionAddEditComponent {
  @ViewChild('attributeDefinitionModal') attributeDefinitionModal!: TemplateRef<any>;
  @Output() attributeDefinitionChanged = new Subject<AttributeDefinitionData[]>();
  @Input() attributeDefinitionSubmitData: AttributeDefinitionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AttributeDefinitionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public attributeDefinitionForm: FormGroup = this.fb.group({
        attributeDefinitionEntityId: [null],
        key: [''],
        label: [''],
        attributeDefinitionTypeId: [null],
        options: [''],
        isRequired: [false],
        sequence: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  attributeDefinitions$ = this.attributeDefinitionService.GetAttributeDefinitionList();
  attributeDefinitionEntities$ = this.attributeDefinitionEntityService.GetAttributeDefinitionEntityList();
  attributeDefinitionTypes$ = this.attributeDefinitionTypeService.GetAttributeDefinitionTypeList();

  constructor(
    private modalService: NgbModal,
    private attributeDefinitionService: AttributeDefinitionService,
    private attributeDefinitionEntityService: AttributeDefinitionEntityService,
    private attributeDefinitionTypeService: AttributeDefinitionTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(attributeDefinitionData?: AttributeDefinitionData) {

    if (attributeDefinitionData != null) {

      if (!this.attributeDefinitionService.userIsSchedulerAttributeDefinitionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Attribute Definitions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.attributeDefinitionSubmitData = this.attributeDefinitionService.ConvertToAttributeDefinitionSubmitData(attributeDefinitionData);
      this.isEditMode = true;
      this.objectGuid = attributeDefinitionData.objectGuid;

      this.buildFormValues(attributeDefinitionData);

    } else {

      if (!this.attributeDefinitionService.userIsSchedulerAttributeDefinitionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Attribute Definitions`,
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
        this.attributeDefinitionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.attributeDefinitionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.attributeDefinitionModal, {
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

    if (this.attributeDefinitionService.userIsSchedulerAttributeDefinitionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Attribute Definitions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.attributeDefinitionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.attributeDefinitionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.attributeDefinitionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const attributeDefinitionSubmitData: AttributeDefinitionSubmitData = {
        id: this.attributeDefinitionSubmitData?.id || 0,
        attributeDefinitionEntityId: formValue.attributeDefinitionEntityId ? Number(formValue.attributeDefinitionEntityId) : null,
        key: formValue.key?.trim() || null,
        label: formValue.label?.trim() || null,
        attributeDefinitionTypeId: formValue.attributeDefinitionTypeId ? Number(formValue.attributeDefinitionTypeId) : null,
        options: formValue.options?.trim() || null,
        isRequired: !!formValue.isRequired,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        versionNumber: this.attributeDefinitionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateAttributeDefinition(attributeDefinitionSubmitData);
      } else {
        this.addAttributeDefinition(attributeDefinitionSubmitData);
      }
  }

  private addAttributeDefinition(attributeDefinitionData: AttributeDefinitionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    attributeDefinitionData.versionNumber = 0;
    attributeDefinitionData.active = true;
    attributeDefinitionData.deleted = false;
    this.attributeDefinitionService.PostAttributeDefinition(attributeDefinitionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAttributeDefinition) => {

        this.attributeDefinitionService.ClearAllCaches();

        this.attributeDefinitionChanged.next([newAttributeDefinition]);

        this.alertService.showMessage("Attribute Definition added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/attributedefinition', newAttributeDefinition.id]);
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
                                   'You do not have permission to save this Attribute Definition.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Attribute Definition.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Attribute Definition could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAttributeDefinition(attributeDefinitionData: AttributeDefinitionSubmitData) {
    this.attributeDefinitionService.PutAttributeDefinition(attributeDefinitionData.id, attributeDefinitionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAttributeDefinition) => {

        this.attributeDefinitionService.ClearAllCaches();

        this.attributeDefinitionChanged.next([updatedAttributeDefinition]);

        this.alertService.showMessage("Attribute Definition updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Attribute Definition.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Attribute Definition.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Attribute Definition could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(attributeDefinitionData: AttributeDefinitionData | null) {

    if (attributeDefinitionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.attributeDefinitionForm.reset({
        attributeDefinitionEntityId: null,
        key: '',
        label: '',
        attributeDefinitionTypeId: null,
        options: '',
        isRequired: false,
        sequence: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.attributeDefinitionForm.reset({
        attributeDefinitionEntityId: attributeDefinitionData.attributeDefinitionEntityId,
        key: attributeDefinitionData.key ?? '',
        label: attributeDefinitionData.label ?? '',
        attributeDefinitionTypeId: attributeDefinitionData.attributeDefinitionTypeId,
        options: attributeDefinitionData.options ?? '',
        isRequired: attributeDefinitionData.isRequired ?? false,
        sequence: attributeDefinitionData.sequence?.toString() ?? '',
        versionNumber: attributeDefinitionData.versionNumber?.toString() ?? '',
        active: attributeDefinitionData.active ?? true,
        deleted: attributeDefinitionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.attributeDefinitionForm.markAsPristine();
    this.attributeDefinitionForm.markAsUntouched();
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


  public userIsSchedulerAttributeDefinitionReader(): boolean {
    return this.attributeDefinitionService.userIsSchedulerAttributeDefinitionReader();
  }

  public userIsSchedulerAttributeDefinitionWriter(): boolean {
    return this.attributeDefinitionService.userIsSchedulerAttributeDefinitionWriter();
  }
}

/*
   GENERATED FORM FOR THE CONNECTORTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConnectorType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to connector-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConnectorTypeService, ConnectorTypeData, ConnectorTypeSubmitData } from '../../../bmc-data-services/connector-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConnectorTypeFormValues {
  name: string,
  description: string,
  degreesOfFreedom: string | null,     // Stored as string for form input, converted to number on submit.
  allowsRotation: boolean,
  allowsSlide: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-connector-type-add-edit',
  templateUrl: './connector-type-add-edit.component.html',
  styleUrls: ['./connector-type-add-edit.component.scss']
})
export class ConnectorTypeAddEditComponent {
  @ViewChild('connectorTypeModal') connectorTypeModal!: TemplateRef<any>;
  @Output() connectorTypeChanged = new Subject<ConnectorTypeData[]>();
  @Input() connectorTypeSubmitData: ConnectorTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConnectorTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public connectorTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        degreesOfFreedom: [''],
        allowsRotation: [false],
        allowsSlide: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  connectorTypes$ = this.connectorTypeService.GetConnectorTypeList();

  constructor(
    private modalService: NgbModal,
    private connectorTypeService: ConnectorTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(connectorTypeData?: ConnectorTypeData) {

    if (connectorTypeData != null) {

      if (!this.connectorTypeService.userIsBMCConnectorTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Connector Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.connectorTypeSubmitData = this.connectorTypeService.ConvertToConnectorTypeSubmitData(connectorTypeData);
      this.isEditMode = true;
      this.objectGuid = connectorTypeData.objectGuid;

      this.buildFormValues(connectorTypeData);

    } else {

      if (!this.connectorTypeService.userIsBMCConnectorTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Connector Types`,
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
        this.connectorTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.connectorTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.connectorTypeModal, {
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

    if (this.connectorTypeService.userIsBMCConnectorTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Connector Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.connectorTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.connectorTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.connectorTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const connectorTypeSubmitData: ConnectorTypeSubmitData = {
        id: this.connectorTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        degreesOfFreedom: formValue.degreesOfFreedom ? Number(formValue.degreesOfFreedom) : null,
        allowsRotation: !!formValue.allowsRotation,
        allowsSlide: !!formValue.allowsSlide,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConnectorType(connectorTypeSubmitData);
      } else {
        this.addConnectorType(connectorTypeSubmitData);
      }
  }

  private addConnectorType(connectorTypeData: ConnectorTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    connectorTypeData.active = true;
    connectorTypeData.deleted = false;
    this.connectorTypeService.PostConnectorType(connectorTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConnectorType) => {

        this.connectorTypeService.ClearAllCaches();

        this.connectorTypeChanged.next([newConnectorType]);

        this.alertService.showMessage("Connector Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/connectortype', newConnectorType.id]);
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
                                   'You do not have permission to save this Connector Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Connector Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Connector Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConnectorType(connectorTypeData: ConnectorTypeSubmitData) {
    this.connectorTypeService.PutConnectorType(connectorTypeData.id, connectorTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConnectorType) => {

        this.connectorTypeService.ClearAllCaches();

        this.connectorTypeChanged.next([updatedConnectorType]);

        this.alertService.showMessage("Connector Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Connector Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Connector Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Connector Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(connectorTypeData: ConnectorTypeData | null) {

    if (connectorTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.connectorTypeForm.reset({
        name: '',
        description: '',
        degreesOfFreedom: '',
        allowsRotation: false,
        allowsSlide: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.connectorTypeForm.reset({
        name: connectorTypeData.name ?? '',
        description: connectorTypeData.description ?? '',
        degreesOfFreedom: connectorTypeData.degreesOfFreedom?.toString() ?? '',
        allowsRotation: connectorTypeData.allowsRotation ?? false,
        allowsSlide: connectorTypeData.allowsSlide ?? false,
        sequence: connectorTypeData.sequence?.toString() ?? '',
        active: connectorTypeData.active ?? true,
        deleted: connectorTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.connectorTypeForm.markAsPristine();
    this.connectorTypeForm.markAsUntouched();
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


  public userIsBMCConnectorTypeReader(): boolean {
    return this.connectorTypeService.userIsBMCConnectorTypeReader();
  }

  public userIsBMCConnectorTypeWriter(): boolean {
    return this.connectorTypeService.userIsBMCConnectorTypeWriter();
  }
}

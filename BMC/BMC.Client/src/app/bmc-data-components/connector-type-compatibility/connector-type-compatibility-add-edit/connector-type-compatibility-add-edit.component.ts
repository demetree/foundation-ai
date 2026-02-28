/*
   GENERATED FORM FOR THE CONNECTORTYPECOMPATIBILITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConnectorTypeCompatibility table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to connector-type-compatibility-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConnectorTypeCompatibilityService, ConnectorTypeCompatibilityData, ConnectorTypeCompatibilitySubmitData } from '../../../bmc-data-services/connector-type-compatibility.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ConnectorTypeService } from '../../../bmc-data-services/connector-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConnectorTypeCompatibilityFormValues {
  maleConnectorTypeId: number | bigint,       // For FK link number
  femaleConnectorTypeId: number | bigint,       // For FK link number
  connectionStrength: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-connector-type-compatibility-add-edit',
  templateUrl: './connector-type-compatibility-add-edit.component.html',
  styleUrls: ['./connector-type-compatibility-add-edit.component.scss']
})
export class ConnectorTypeCompatibilityAddEditComponent {
  @ViewChild('connectorTypeCompatibilityModal') connectorTypeCompatibilityModal!: TemplateRef<any>;
  @Output() connectorTypeCompatibilityChanged = new Subject<ConnectorTypeCompatibilityData[]>();
  @Input() connectorTypeCompatibilitySubmitData: ConnectorTypeCompatibilitySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConnectorTypeCompatibilityFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public connectorTypeCompatibilityForm: FormGroup = this.fb.group({
        maleConnectorTypeId: [null, Validators.required],
        femaleConnectorTypeId: [null, Validators.required],
        connectionStrength: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  connectorTypeCompatibilities$ = this.connectorTypeCompatibilityService.GetConnectorTypeCompatibilityList();
  connectorTypes$ = this.connectorTypeService.GetConnectorTypeList();

  constructor(
    private modalService: NgbModal,
    private connectorTypeCompatibilityService: ConnectorTypeCompatibilityService,
    private connectorTypeService: ConnectorTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(connectorTypeCompatibilityData?: ConnectorTypeCompatibilityData) {

    if (connectorTypeCompatibilityData != null) {

      if (!this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Connector Type Compatibilities`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.connectorTypeCompatibilitySubmitData = this.connectorTypeCompatibilityService.ConvertToConnectorTypeCompatibilitySubmitData(connectorTypeCompatibilityData);
      this.isEditMode = true;
      this.objectGuid = connectorTypeCompatibilityData.objectGuid;

      this.buildFormValues(connectorTypeCompatibilityData);

    } else {

      if (!this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Connector Type Compatibilities`,
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
        this.connectorTypeCompatibilityForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.connectorTypeCompatibilityForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.connectorTypeCompatibilityModal, {
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

    if (this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Connector Type Compatibilities`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.connectorTypeCompatibilityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.connectorTypeCompatibilityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.connectorTypeCompatibilityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const connectorTypeCompatibilitySubmitData: ConnectorTypeCompatibilitySubmitData = {
        id: this.connectorTypeCompatibilitySubmitData?.id || 0,
        maleConnectorTypeId: Number(formValue.maleConnectorTypeId),
        femaleConnectorTypeId: Number(formValue.femaleConnectorTypeId),
        connectionStrength: formValue.connectionStrength!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConnectorTypeCompatibility(connectorTypeCompatibilitySubmitData);
      } else {
        this.addConnectorTypeCompatibility(connectorTypeCompatibilitySubmitData);
      }
  }

  private addConnectorTypeCompatibility(connectorTypeCompatibilityData: ConnectorTypeCompatibilitySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    connectorTypeCompatibilityData.active = true;
    connectorTypeCompatibilityData.deleted = false;
    this.connectorTypeCompatibilityService.PostConnectorTypeCompatibility(connectorTypeCompatibilityData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConnectorTypeCompatibility) => {

        this.connectorTypeCompatibilityService.ClearAllCaches();

        this.connectorTypeCompatibilityChanged.next([newConnectorTypeCompatibility]);

        this.alertService.showMessage("Connector Type Compatibility added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/connectortypecompatibility', newConnectorTypeCompatibility.id]);
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
                                   'You do not have permission to save this Connector Type Compatibility.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Connector Type Compatibility.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Connector Type Compatibility could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConnectorTypeCompatibility(connectorTypeCompatibilityData: ConnectorTypeCompatibilitySubmitData) {
    this.connectorTypeCompatibilityService.PutConnectorTypeCompatibility(connectorTypeCompatibilityData.id, connectorTypeCompatibilityData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConnectorTypeCompatibility) => {

        this.connectorTypeCompatibilityService.ClearAllCaches();

        this.connectorTypeCompatibilityChanged.next([updatedConnectorTypeCompatibility]);

        this.alertService.showMessage("Connector Type Compatibility updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Connector Type Compatibility.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Connector Type Compatibility.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Connector Type Compatibility could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(connectorTypeCompatibilityData: ConnectorTypeCompatibilityData | null) {

    if (connectorTypeCompatibilityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.connectorTypeCompatibilityForm.reset({
        maleConnectorTypeId: null,
        femaleConnectorTypeId: null,
        connectionStrength: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.connectorTypeCompatibilityForm.reset({
        maleConnectorTypeId: connectorTypeCompatibilityData.maleConnectorTypeId,
        femaleConnectorTypeId: connectorTypeCompatibilityData.femaleConnectorTypeId,
        connectionStrength: connectorTypeCompatibilityData.connectionStrength ?? '',
        active: connectorTypeCompatibilityData.active ?? true,
        deleted: connectorTypeCompatibilityData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.connectorTypeCompatibilityForm.markAsPristine();
    this.connectorTypeCompatibilityForm.markAsUntouched();
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


  public userIsBMCConnectorTypeCompatibilityReader(): boolean {
    return this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityReader();
  }

  public userIsBMCConnectorTypeCompatibilityWriter(): boolean {
    return this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityWriter();
  }
}

/*
   GENERATED FORM FOR THE BRICKPARTCONNECTOR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPartConnector table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-connector-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickPartConnectorService, BrickPartConnectorData, BrickPartConnectorSubmitData } from '../../../bmc-data-services/brick-part-connector.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { ConnectorTypeService } from '../../../bmc-data-services/connector-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickPartConnectorFormValues {
  brickPartId: number | bigint,       // For FK link number
  connectorTypeId: number | bigint,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  orientationX: string | null,     // Stored as string for form input, converted to number on submit.
  orientationY: string | null,     // Stored as string for form input, converted to number on submit.
  orientationZ: string | null,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-part-connector-add-edit',
  templateUrl: './brick-part-connector-add-edit.component.html',
  styleUrls: ['./brick-part-connector-add-edit.component.scss']
})
export class BrickPartConnectorAddEditComponent {
  @ViewChild('brickPartConnectorModal') brickPartConnectorModal!: TemplateRef<any>;
  @Output() brickPartConnectorChanged = new Subject<BrickPartConnectorData[]>();
  @Input() brickPartConnectorSubmitData: BrickPartConnectorSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickPartConnectorFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickPartConnectorForm: FormGroup = this.fb.group({
        brickPartId: [null, Validators.required],
        connectorTypeId: [null, Validators.required],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        orientationX: [''],
        orientationY: [''],
        orientationZ: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickPartConnectors$ = this.brickPartConnectorService.GetBrickPartConnectorList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  connectorTypes$ = this.connectorTypeService.GetConnectorTypeList();

  constructor(
    private modalService: NgbModal,
    private brickPartConnectorService: BrickPartConnectorService,
    private brickPartService: BrickPartService,
    private connectorTypeService: ConnectorTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickPartConnectorData?: BrickPartConnectorData) {

    if (brickPartConnectorData != null) {

      if (!this.brickPartConnectorService.userIsBMCBrickPartConnectorReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Part Connectors`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickPartConnectorSubmitData = this.brickPartConnectorService.ConvertToBrickPartConnectorSubmitData(brickPartConnectorData);
      this.isEditMode = true;
      this.objectGuid = brickPartConnectorData.objectGuid;

      this.buildFormValues(brickPartConnectorData);

    } else {

      if (!this.brickPartConnectorService.userIsBMCBrickPartConnectorWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Part Connectors`,
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
        this.brickPartConnectorForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickPartConnectorForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickPartConnectorModal, {
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

    if (this.brickPartConnectorService.userIsBMCBrickPartConnectorWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Part Connectors`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickPartConnectorForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickPartConnectorForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickPartConnectorForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickPartConnectorSubmitData: BrickPartConnectorSubmitData = {
        id: this.brickPartConnectorSubmitData?.id || 0,
        brickPartId: Number(formValue.brickPartId),
        connectorTypeId: Number(formValue.connectorTypeId),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        orientationX: formValue.orientationX ? Number(formValue.orientationX) : null,
        orientationY: formValue.orientationY ? Number(formValue.orientationY) : null,
        orientationZ: formValue.orientationZ ? Number(formValue.orientationZ) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickPartConnector(brickPartConnectorSubmitData);
      } else {
        this.addBrickPartConnector(brickPartConnectorSubmitData);
      }
  }

  private addBrickPartConnector(brickPartConnectorData: BrickPartConnectorSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickPartConnectorData.active = true;
    brickPartConnectorData.deleted = false;
    this.brickPartConnectorService.PostBrickPartConnector(brickPartConnectorData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickPartConnector) => {

        this.brickPartConnectorService.ClearAllCaches();

        this.brickPartConnectorChanged.next([newBrickPartConnector]);

        this.alertService.showMessage("Brick Part Connector added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickpartconnector', newBrickPartConnector.id]);
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
                                   'You do not have permission to save this Brick Part Connector.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part Connector.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part Connector could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickPartConnector(brickPartConnectorData: BrickPartConnectorSubmitData) {
    this.brickPartConnectorService.PutBrickPartConnector(brickPartConnectorData.id, brickPartConnectorData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickPartConnector) => {

        this.brickPartConnectorService.ClearAllCaches();

        this.brickPartConnectorChanged.next([updatedBrickPartConnector]);

        this.alertService.showMessage("Brick Part Connector updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Part Connector.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part Connector.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part Connector could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickPartConnectorData: BrickPartConnectorData | null) {

    if (brickPartConnectorData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickPartConnectorForm.reset({
        brickPartId: null,
        connectorTypeId: null,
        positionX: '',
        positionY: '',
        positionZ: '',
        orientationX: '',
        orientationY: '',
        orientationZ: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickPartConnectorForm.reset({
        brickPartId: brickPartConnectorData.brickPartId,
        connectorTypeId: brickPartConnectorData.connectorTypeId,
        positionX: brickPartConnectorData.positionX?.toString() ?? '',
        positionY: brickPartConnectorData.positionY?.toString() ?? '',
        positionZ: brickPartConnectorData.positionZ?.toString() ?? '',
        orientationX: brickPartConnectorData.orientationX?.toString() ?? '',
        orientationY: brickPartConnectorData.orientationY?.toString() ?? '',
        orientationZ: brickPartConnectorData.orientationZ?.toString() ?? '',
        sequence: brickPartConnectorData.sequence?.toString() ?? '',
        active: brickPartConnectorData.active ?? true,
        deleted: brickPartConnectorData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickPartConnectorForm.markAsPristine();
    this.brickPartConnectorForm.markAsUntouched();
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


  public userIsBMCBrickPartConnectorReader(): boolean {
    return this.brickPartConnectorService.userIsBMCBrickPartConnectorReader();
  }

  public userIsBMCBrickPartConnectorWriter(): boolean {
    return this.brickPartConnectorService.userIsBMCBrickPartConnectorWriter();
  }
}

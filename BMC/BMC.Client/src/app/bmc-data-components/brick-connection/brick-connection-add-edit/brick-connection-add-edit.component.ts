/*
   GENERATED FORM FOR THE BRICKCONNECTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickConnection table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-connection-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickConnectionService, BrickConnectionData, BrickConnectionSubmitData } from '../../../bmc-data-services/brick-connection.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
import { BrickPartConnectorService } from '../../../bmc-data-services/brick-part-connector.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickConnectionFormValues {
  projectId: number | bigint,       // For FK link number
  sourcePlacedBrickId: number | bigint,       // For FK link number
  sourceConnectorId: number | bigint,       // For FK link number
  targetPlacedBrickId: number | bigint,       // For FK link number
  targetConnectorId: number | bigint,       // For FK link number
  connectionStrength: string | null,
  isLocked: boolean,
  angleDegrees: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-connection-add-edit',
  templateUrl: './brick-connection-add-edit.component.html',
  styleUrls: ['./brick-connection-add-edit.component.scss']
})
export class BrickConnectionAddEditComponent {
  @ViewChild('brickConnectionModal') brickConnectionModal!: TemplateRef<any>;
  @Output() brickConnectionChanged = new Subject<BrickConnectionData[]>();
  @Input() brickConnectionSubmitData: BrickConnectionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickConnectionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickConnectionForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        sourcePlacedBrickId: [null, Validators.required],
        sourceConnectorId: [null, Validators.required],
        targetPlacedBrickId: [null, Validators.required],
        targetConnectorId: [null, Validators.required],
        connectionStrength: [''],
        isLocked: [false],
        angleDegrees: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickConnections$ = this.brickConnectionService.GetBrickConnectionList();
  projects$ = this.projectService.GetProjectList();
  placedBricks$ = this.placedBrickService.GetPlacedBrickList();
  brickPartConnectors$ = this.brickPartConnectorService.GetBrickPartConnectorList();

  constructor(
    private modalService: NgbModal,
    private brickConnectionService: BrickConnectionService,
    private projectService: ProjectService,
    private placedBrickService: PlacedBrickService,
    private brickPartConnectorService: BrickPartConnectorService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickConnectionData?: BrickConnectionData) {

    if (brickConnectionData != null) {

      if (!this.brickConnectionService.userIsBMCBrickConnectionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Connections`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickConnectionSubmitData = this.brickConnectionService.ConvertToBrickConnectionSubmitData(brickConnectionData);
      this.isEditMode = true;
      this.objectGuid = brickConnectionData.objectGuid;

      this.buildFormValues(brickConnectionData);

    } else {

      if (!this.brickConnectionService.userIsBMCBrickConnectionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Connections`,
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
        this.brickConnectionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickConnectionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickConnectionModal, {
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

    if (this.brickConnectionService.userIsBMCBrickConnectionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Connections`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickConnectionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickConnectionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickConnectionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickConnectionSubmitData: BrickConnectionSubmitData = {
        id: this.brickConnectionSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        sourcePlacedBrickId: Number(formValue.sourcePlacedBrickId),
        sourceConnectorId: Number(formValue.sourceConnectorId),
        targetPlacedBrickId: Number(formValue.targetPlacedBrickId),
        targetConnectorId: Number(formValue.targetConnectorId),
        connectionStrength: formValue.connectionStrength?.trim() || null,
        isLocked: !!formValue.isLocked,
        angleDegrees: formValue.angleDegrees ? Number(formValue.angleDegrees) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickConnection(brickConnectionSubmitData);
      } else {
        this.addBrickConnection(brickConnectionSubmitData);
      }
  }

  private addBrickConnection(brickConnectionData: BrickConnectionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickConnectionData.active = true;
    brickConnectionData.deleted = false;
    this.brickConnectionService.PostBrickConnection(brickConnectionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickConnection) => {

        this.brickConnectionService.ClearAllCaches();

        this.brickConnectionChanged.next([newBrickConnection]);

        this.alertService.showMessage("Brick Connection added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickconnection', newBrickConnection.id]);
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
                                   'You do not have permission to save this Brick Connection.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Connection.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Connection could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickConnection(brickConnectionData: BrickConnectionSubmitData) {
    this.brickConnectionService.PutBrickConnection(brickConnectionData.id, brickConnectionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickConnection) => {

        this.brickConnectionService.ClearAllCaches();

        this.brickConnectionChanged.next([updatedBrickConnection]);

        this.alertService.showMessage("Brick Connection updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Connection.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Connection.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Connection could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickConnectionData: BrickConnectionData | null) {

    if (brickConnectionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickConnectionForm.reset({
        projectId: null,
        sourcePlacedBrickId: null,
        sourceConnectorId: null,
        targetPlacedBrickId: null,
        targetConnectorId: null,
        connectionStrength: '',
        isLocked: false,
        angleDegrees: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickConnectionForm.reset({
        projectId: brickConnectionData.projectId,
        sourcePlacedBrickId: brickConnectionData.sourcePlacedBrickId,
        sourceConnectorId: brickConnectionData.sourceConnectorId,
        targetPlacedBrickId: brickConnectionData.targetPlacedBrickId,
        targetConnectorId: brickConnectionData.targetConnectorId,
        connectionStrength: brickConnectionData.connectionStrength ?? '',
        isLocked: brickConnectionData.isLocked ?? false,
        angleDegrees: brickConnectionData.angleDegrees?.toString() ?? '',
        active: brickConnectionData.active ?? true,
        deleted: brickConnectionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickConnectionForm.markAsPristine();
    this.brickConnectionForm.markAsUntouched();
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


  public userIsBMCBrickConnectionReader(): boolean {
    return this.brickConnectionService.userIsBMCBrickConnectionReader();
  }

  public userIsBMCBrickConnectionWriter(): boolean {
    return this.brickConnectionService.userIsBMCBrickConnectionWriter();
  }
}

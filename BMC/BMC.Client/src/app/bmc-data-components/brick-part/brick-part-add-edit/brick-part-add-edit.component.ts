/*
   GENERATED FORM FOR THE BRICKPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickPartService, BrickPartData, BrickPartSubmitData } from '../../../bmc-data-services/brick-part.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PartTypeService } from '../../../bmc-data-services/part-type.service';
import { BrickCategoryService } from '../../../bmc-data-services/brick-category.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickPartFormValues {
  name: string,
  ldrawPartId: string,
  ldrawTitle: string | null,
  ldrawCategory: string | null,
  partTypeId: number | bigint,       // For FK link number
  keywords: string | null,
  author: string | null,
  brickCategoryId: number | bigint,       // For FK link number
  widthLdu: string | null,     // Stored as string for form input, converted to number on submit.
  heightLdu: string | null,     // Stored as string for form input, converted to number on submit.
  depthLdu: string | null,     // Stored as string for form input, converted to number on submit.
  massGrams: string | null,     // Stored as string for form input, converted to number on submit.
  geometryFilePath: string | null,
  toothCount: string | null,     // Stored as string for form input, converted to number on submit.
  gearRatio: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-part-add-edit',
  templateUrl: './brick-part-add-edit.component.html',
  styleUrls: ['./brick-part-add-edit.component.scss']
})
export class BrickPartAddEditComponent {
  @ViewChild('brickPartModal') brickPartModal!: TemplateRef<any>;
  @Output() brickPartChanged = new Subject<BrickPartData[]>();
  @Input() brickPartSubmitData: BrickPartSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickPartForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        ldrawPartId: ['', Validators.required],
        ldrawTitle: [''],
        ldrawCategory: [''],
        partTypeId: [null, Validators.required],
        keywords: [''],
        author: [''],
        brickCategoryId: [null, Validators.required],
        widthLdu: [''],
        heightLdu: [''],
        depthLdu: [''],
        massGrams: [''],
        geometryFilePath: [''],
        toothCount: [''],
        gearRatio: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickParts$ = this.brickPartService.GetBrickPartList();
  partTypes$ = this.partTypeService.GetPartTypeList();
  brickCategories$ = this.brickCategoryService.GetBrickCategoryList();

  constructor(
    private modalService: NgbModal,
    private brickPartService: BrickPartService,
    private partTypeService: PartTypeService,
    private brickCategoryService: BrickCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickPartData?: BrickPartData) {

    if (brickPartData != null) {

      if (!this.brickPartService.userIsBMCBrickPartReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Parts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickPartSubmitData = this.brickPartService.ConvertToBrickPartSubmitData(brickPartData);
      this.isEditMode = true;
      this.objectGuid = brickPartData.objectGuid;

      this.buildFormValues(brickPartData);

    } else {

      if (!this.brickPartService.userIsBMCBrickPartWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Parts`,
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
        this.brickPartForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickPartModal, {
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

    if (this.brickPartService.userIsBMCBrickPartWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Parts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickPartSubmitData: BrickPartSubmitData = {
        id: this.brickPartSubmitData?.id || 0,
        name: formValue.name!.trim(),
        ldrawPartId: formValue.ldrawPartId!.trim(),
        ldrawTitle: formValue.ldrawTitle?.trim() || null,
        ldrawCategory: formValue.ldrawCategory?.trim() || null,
        partTypeId: Number(formValue.partTypeId),
        keywords: formValue.keywords?.trim() || null,
        author: formValue.author?.trim() || null,
        brickCategoryId: Number(formValue.brickCategoryId),
        widthLdu: formValue.widthLdu ? Number(formValue.widthLdu) : null,
        heightLdu: formValue.heightLdu ? Number(formValue.heightLdu) : null,
        depthLdu: formValue.depthLdu ? Number(formValue.depthLdu) : null,
        massGrams: formValue.massGrams ? Number(formValue.massGrams) : null,
        geometryFilePath: formValue.geometryFilePath?.trim() || null,
        toothCount: formValue.toothCount ? Number(formValue.toothCount) : null,
        gearRatio: formValue.gearRatio ? Number(formValue.gearRatio) : null,
        versionNumber: this.brickPartSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickPart(brickPartSubmitData);
      } else {
        this.addBrickPart(brickPartSubmitData);
      }
  }

  private addBrickPart(brickPartData: BrickPartSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickPartData.versionNumber = 0;
    brickPartData.active = true;
    brickPartData.deleted = false;
    this.brickPartService.PostBrickPart(brickPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickPart) => {

        this.brickPartService.ClearAllCaches();

        this.brickPartChanged.next([newBrickPart]);

        this.alertService.showMessage("Brick Part added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickpart', newBrickPart.id]);
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
                                   'You do not have permission to save this Brick Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickPart(brickPartData: BrickPartSubmitData) {
    this.brickPartService.PutBrickPart(brickPartData.id, brickPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickPart) => {

        this.brickPartService.ClearAllCaches();

        this.brickPartChanged.next([updatedBrickPart]);

        this.alertService.showMessage("Brick Part updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickPartData: BrickPartData | null) {

    if (brickPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickPartForm.reset({
        name: '',
        ldrawPartId: '',
        ldrawTitle: '',
        ldrawCategory: '',
        partTypeId: null,
        keywords: '',
        author: '',
        brickCategoryId: null,
        widthLdu: '',
        heightLdu: '',
        depthLdu: '',
        massGrams: '',
        geometryFilePath: '',
        toothCount: '',
        gearRatio: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickPartForm.reset({
        name: brickPartData.name ?? '',
        ldrawPartId: brickPartData.ldrawPartId ?? '',
        ldrawTitle: brickPartData.ldrawTitle ?? '',
        ldrawCategory: brickPartData.ldrawCategory ?? '',
        partTypeId: brickPartData.partTypeId,
        keywords: brickPartData.keywords ?? '',
        author: brickPartData.author ?? '',
        brickCategoryId: brickPartData.brickCategoryId,
        widthLdu: brickPartData.widthLdu?.toString() ?? '',
        heightLdu: brickPartData.heightLdu?.toString() ?? '',
        depthLdu: brickPartData.depthLdu?.toString() ?? '',
        massGrams: brickPartData.massGrams?.toString() ?? '',
        geometryFilePath: brickPartData.geometryFilePath ?? '',
        toothCount: brickPartData.toothCount?.toString() ?? '',
        gearRatio: brickPartData.gearRatio?.toString() ?? '',
        versionNumber: brickPartData.versionNumber?.toString() ?? '',
        active: brickPartData.active ?? true,
        deleted: brickPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickPartForm.markAsPristine();
    this.brickPartForm.markAsUntouched();
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


  public userIsBMCBrickPartReader(): boolean {
    return this.brickPartService.userIsBMCBrickPartReader();
  }

  public userIsBMCBrickPartWriter(): boolean {
    return this.brickPartService.userIsBMCBrickPartWriter();
  }
}

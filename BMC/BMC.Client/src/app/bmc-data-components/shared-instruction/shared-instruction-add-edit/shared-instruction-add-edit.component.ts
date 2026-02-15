/*
   GENERATED FORM FOR THE SHAREDINSTRUCTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SharedInstruction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to shared-instruction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SharedInstructionService, SharedInstructionData, SharedInstructionSubmitData } from '../../../bmc-data-services/shared-instruction.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BuildManualService } from '../../../bmc-data-services/build-manual.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SharedInstructionFormValues {
  buildManualId: number | bigint | null,       // For FK link number
  publishedMocId: number | bigint | null,       // For FK link number
  name: string,
  description: string | null,
  formatType: string,
  filePath: string | null,
  isPublished: boolean,
  publishedDate: string | null,
  downloadCount: string,     // Stored as string for form input, converted to number on submit.
  pageCount: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-shared-instruction-add-edit',
  templateUrl: './shared-instruction-add-edit.component.html',
  styleUrls: ['./shared-instruction-add-edit.component.scss']
})
export class SharedInstructionAddEditComponent {
  @ViewChild('sharedInstructionModal') sharedInstructionModal!: TemplateRef<any>;
  @Output() sharedInstructionChanged = new Subject<SharedInstructionData[]>();
  @Input() sharedInstructionSubmitData: SharedInstructionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SharedInstructionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public sharedInstructionForm: FormGroup = this.fb.group({
        buildManualId: [null],
        publishedMocId: [null],
        name: ['', Validators.required],
        description: [''],
        formatType: ['', Validators.required],
        filePath: [''],
        isPublished: [false],
        publishedDate: [''],
        downloadCount: ['', Validators.required],
        pageCount: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  sharedInstructions$ = this.sharedInstructionService.GetSharedInstructionList();
  buildManuals$ = this.buildManualService.GetBuildManualList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  constructor(
    private modalService: NgbModal,
    private sharedInstructionService: SharedInstructionService,
    private buildManualService: BuildManualService,
    private publishedMocService: PublishedMocService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(sharedInstructionData?: SharedInstructionData) {

    if (sharedInstructionData != null) {

      if (!this.sharedInstructionService.userIsBMCSharedInstructionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Shared Instructions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.sharedInstructionSubmitData = this.sharedInstructionService.ConvertToSharedInstructionSubmitData(sharedInstructionData);
      this.isEditMode = true;
      this.objectGuid = sharedInstructionData.objectGuid;

      this.buildFormValues(sharedInstructionData);

    } else {

      if (!this.sharedInstructionService.userIsBMCSharedInstructionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Shared Instructions`,
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
        this.sharedInstructionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.sharedInstructionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.sharedInstructionModal, {
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

    if (this.sharedInstructionService.userIsBMCSharedInstructionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Shared Instructions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.sharedInstructionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.sharedInstructionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.sharedInstructionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const sharedInstructionSubmitData: SharedInstructionSubmitData = {
        id: this.sharedInstructionSubmitData?.id || 0,
        buildManualId: formValue.buildManualId ? Number(formValue.buildManualId) : null,
        publishedMocId: formValue.publishedMocId ? Number(formValue.publishedMocId) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        formatType: formValue.formatType!.trim(),
        filePath: formValue.filePath?.trim() || null,
        isPublished: !!formValue.isPublished,
        publishedDate: formValue.publishedDate ? dateTimeLocalToIsoUtc(formValue.publishedDate.trim()) : null,
        downloadCount: Number(formValue.downloadCount),
        pageCount: formValue.pageCount ? Number(formValue.pageCount) : null,
        versionNumber: this.sharedInstructionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSharedInstruction(sharedInstructionSubmitData);
      } else {
        this.addSharedInstruction(sharedInstructionSubmitData);
      }
  }

  private addSharedInstruction(sharedInstructionData: SharedInstructionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    sharedInstructionData.versionNumber = 0;
    sharedInstructionData.active = true;
    sharedInstructionData.deleted = false;
    this.sharedInstructionService.PostSharedInstruction(sharedInstructionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSharedInstruction) => {

        this.sharedInstructionService.ClearAllCaches();

        this.sharedInstructionChanged.next([newSharedInstruction]);

        this.alertService.showMessage("Shared Instruction added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/sharedinstruction', newSharedInstruction.id]);
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
                                   'You do not have permission to save this Shared Instruction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shared Instruction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shared Instruction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSharedInstruction(sharedInstructionData: SharedInstructionSubmitData) {
    this.sharedInstructionService.PutSharedInstruction(sharedInstructionData.id, sharedInstructionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSharedInstruction) => {

        this.sharedInstructionService.ClearAllCaches();

        this.sharedInstructionChanged.next([updatedSharedInstruction]);

        this.alertService.showMessage("Shared Instruction updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Shared Instruction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shared Instruction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shared Instruction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(sharedInstructionData: SharedInstructionData | null) {

    if (sharedInstructionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.sharedInstructionForm.reset({
        buildManualId: null,
        publishedMocId: null,
        name: '',
        description: '',
        formatType: '',
        filePath: '',
        isPublished: false,
        publishedDate: '',
        downloadCount: '',
        pageCount: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.sharedInstructionForm.reset({
        buildManualId: sharedInstructionData.buildManualId,
        publishedMocId: sharedInstructionData.publishedMocId,
        name: sharedInstructionData.name ?? '',
        description: sharedInstructionData.description ?? '',
        formatType: sharedInstructionData.formatType ?? '',
        filePath: sharedInstructionData.filePath ?? '',
        isPublished: sharedInstructionData.isPublished ?? false,
        publishedDate: isoUtcStringToDateTimeLocal(sharedInstructionData.publishedDate) ?? '',
        downloadCount: sharedInstructionData.downloadCount?.toString() ?? '',
        pageCount: sharedInstructionData.pageCount?.toString() ?? '',
        versionNumber: sharedInstructionData.versionNumber?.toString() ?? '',
        active: sharedInstructionData.active ?? true,
        deleted: sharedInstructionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.sharedInstructionForm.markAsPristine();
    this.sharedInstructionForm.markAsUntouched();
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


  public userIsBMCSharedInstructionReader(): boolean {
    return this.sharedInstructionService.userIsBMCSharedInstructionReader();
  }

  public userIsBMCSharedInstructionWriter(): boolean {
    return this.sharedInstructionService.userIsBMCSharedInstructionWriter();
  }
}

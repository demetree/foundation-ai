/*
   GENERATED FORM FOR THE EXPORTFORMAT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ExportFormat table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to export-format-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ExportFormatService, ExportFormatData, ExportFormatSubmitData } from '../../../bmc-data-services/export-format.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ExportFormatFormValues {
  name: string,
  description: string,
  fileExtension: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-export-format-add-edit',
  templateUrl: './export-format-add-edit.component.html',
  styleUrls: ['./export-format-add-edit.component.scss']
})
export class ExportFormatAddEditComponent {
  @ViewChild('exportFormatModal') exportFormatModal!: TemplateRef<any>;
  @Output() exportFormatChanged = new Subject<ExportFormatData[]>();
  @Input() exportFormatSubmitData: ExportFormatSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ExportFormatFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public exportFormatForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        fileExtension: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  exportFormats$ = this.exportFormatService.GetExportFormatList();

  constructor(
    private modalService: NgbModal,
    private exportFormatService: ExportFormatService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(exportFormatData?: ExportFormatData) {

    if (exportFormatData != null) {

      if (!this.exportFormatService.userIsBMCExportFormatReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Export Formats`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.exportFormatSubmitData = this.exportFormatService.ConvertToExportFormatSubmitData(exportFormatData);
      this.isEditMode = true;
      this.objectGuid = exportFormatData.objectGuid;

      this.buildFormValues(exportFormatData);

    } else {

      if (!this.exportFormatService.userIsBMCExportFormatWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Export Formats`,
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
        this.exportFormatForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.exportFormatForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.exportFormatModal, {
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

    if (this.exportFormatService.userIsBMCExportFormatWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Export Formats`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.exportFormatForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.exportFormatForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.exportFormatForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const exportFormatSubmitData: ExportFormatSubmitData = {
        id: this.exportFormatSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        fileExtension: formValue.fileExtension?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateExportFormat(exportFormatSubmitData);
      } else {
        this.addExportFormat(exportFormatSubmitData);
      }
  }

  private addExportFormat(exportFormatData: ExportFormatSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    exportFormatData.active = true;
    exportFormatData.deleted = false;
    this.exportFormatService.PostExportFormat(exportFormatData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newExportFormat) => {

        this.exportFormatService.ClearAllCaches();

        this.exportFormatChanged.next([newExportFormat]);

        this.alertService.showMessage("Export Format added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/exportformat', newExportFormat.id]);
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
                                   'You do not have permission to save this Export Format.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Export Format.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Export Format could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateExportFormat(exportFormatData: ExportFormatSubmitData) {
    this.exportFormatService.PutExportFormat(exportFormatData.id, exportFormatData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedExportFormat) => {

        this.exportFormatService.ClearAllCaches();

        this.exportFormatChanged.next([updatedExportFormat]);

        this.alertService.showMessage("Export Format updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Export Format.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Export Format.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Export Format could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(exportFormatData: ExportFormatData | null) {

    if (exportFormatData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.exportFormatForm.reset({
        name: '',
        description: '',
        fileExtension: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.exportFormatForm.reset({
        name: exportFormatData.name ?? '',
        description: exportFormatData.description ?? '',
        fileExtension: exportFormatData.fileExtension ?? '',
        sequence: exportFormatData.sequence?.toString() ?? '',
        active: exportFormatData.active ?? true,
        deleted: exportFormatData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.exportFormatForm.markAsPristine();
    this.exportFormatForm.markAsUntouched();
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


  public userIsBMCExportFormatReader(): boolean {
    return this.exportFormatService.userIsBMCExportFormatReader();
  }

  public userIsBMCExportFormatWriter(): boolean {
    return this.exportFormatService.userIsBMCExportFormatWriter();
  }
}

/*
   GENERATED FORM FOR THE BUILDMANUALPAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildManualPage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-manual-page-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildManualPageService, BuildManualPageData, BuildManualPageSubmitData } from '../../../bmc-data-services/build-manual-page.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BuildManualService } from '../../../bmc-data-services/build-manual.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BuildManualPageFormValues {
  buildManualId: number | bigint,       // For FK link number
  pageNum: string | null,     // Stored as string for form input, converted to number on submit.
  title: string | null,
  notes: string | null,
  backgroundTheme: string | null,
  layoutPreset: string | null,
  backgroundColorHex: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-build-manual-page-add-edit',
  templateUrl: './build-manual-page-add-edit.component.html',
  styleUrls: ['./build-manual-page-add-edit.component.scss']
})
export class BuildManualPageAddEditComponent {
  @ViewChild('buildManualPageModal') buildManualPageModal!: TemplateRef<any>;
  @Output() buildManualPageChanged = new Subject<BuildManualPageData[]>();
  @Input() buildManualPageSubmitData: BuildManualPageSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildManualPageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildManualPageForm: FormGroup = this.fb.group({
        buildManualId: [null, Validators.required],
        pageNum: [''],
        title: [''],
        notes: [''],
        backgroundTheme: [''],
        layoutPreset: [''],
        backgroundColorHex: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildManualPages$ = this.buildManualPageService.GetBuildManualPageList();
  buildManuals$ = this.buildManualService.GetBuildManualList();

  constructor(
    private modalService: NgbModal,
    private buildManualPageService: BuildManualPageService,
    private buildManualService: BuildManualService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildManualPageData?: BuildManualPageData) {

    if (buildManualPageData != null) {

      if (!this.buildManualPageService.userIsBMCBuildManualPageReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Manual Pages`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildManualPageSubmitData = this.buildManualPageService.ConvertToBuildManualPageSubmitData(buildManualPageData);
      this.isEditMode = true;
      this.objectGuid = buildManualPageData.objectGuid;

      this.buildFormValues(buildManualPageData);

    } else {

      if (!this.buildManualPageService.userIsBMCBuildManualPageWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Manual Pages`,
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
        this.buildManualPageForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildManualPageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildManualPageModal, {
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

    if (this.buildManualPageService.userIsBMCBuildManualPageWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Manual Pages`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildManualPageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildManualPageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildManualPageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildManualPageSubmitData: BuildManualPageSubmitData = {
        id: this.buildManualPageSubmitData?.id || 0,
        buildManualId: Number(formValue.buildManualId),
        pageNum: formValue.pageNum ? Number(formValue.pageNum) : null,
        title: formValue.title?.trim() || null,
        notes: formValue.notes?.trim() || null,
        backgroundTheme: formValue.backgroundTheme?.trim() || null,
        layoutPreset: formValue.layoutPreset?.trim() || null,
        backgroundColorHex: formValue.backgroundColorHex?.trim() || null,
        versionNumber: this.buildManualPageSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBuildManualPage(buildManualPageSubmitData);
      } else {
        this.addBuildManualPage(buildManualPageSubmitData);
      }
  }

  private addBuildManualPage(buildManualPageData: BuildManualPageSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildManualPageData.versionNumber = 0;
    buildManualPageData.active = true;
    buildManualPageData.deleted = false;
    this.buildManualPageService.PostBuildManualPage(buildManualPageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildManualPage) => {

        this.buildManualPageService.ClearAllCaches();

        this.buildManualPageChanged.next([newBuildManualPage]);

        this.alertService.showMessage("Build Manual Page added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildmanualpage', newBuildManualPage.id]);
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
                                   'You do not have permission to save this Build Manual Page.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Page.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Page could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildManualPage(buildManualPageData: BuildManualPageSubmitData) {
    this.buildManualPageService.PutBuildManualPage(buildManualPageData.id, buildManualPageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildManualPage) => {

        this.buildManualPageService.ClearAllCaches();

        this.buildManualPageChanged.next([updatedBuildManualPage]);

        this.alertService.showMessage("Build Manual Page updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Manual Page.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Manual Page.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Manual Page could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildManualPageData: BuildManualPageData | null) {

    if (buildManualPageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildManualPageForm.reset({
        buildManualId: null,
        pageNum: '',
        title: '',
        notes: '',
        backgroundTheme: '',
        layoutPreset: '',
        backgroundColorHex: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildManualPageForm.reset({
        buildManualId: buildManualPageData.buildManualId,
        pageNum: buildManualPageData.pageNum?.toString() ?? '',
        title: buildManualPageData.title ?? '',
        notes: buildManualPageData.notes ?? '',
        backgroundTheme: buildManualPageData.backgroundTheme ?? '',
        layoutPreset: buildManualPageData.layoutPreset ?? '',
        backgroundColorHex: buildManualPageData.backgroundColorHex ?? '',
        versionNumber: buildManualPageData.versionNumber?.toString() ?? '',
        active: buildManualPageData.active ?? true,
        deleted: buildManualPageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildManualPageForm.markAsPristine();
    this.buildManualPageForm.markAsUntouched();
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


  public userIsBMCBuildManualPageReader(): boolean {
    return this.buildManualPageService.userIsBMCBuildManualPageReader();
  }

  public userIsBMCBuildManualPageWriter(): boolean {
    return this.buildManualPageService.userIsBMCBuildManualPageWriter();
  }
}

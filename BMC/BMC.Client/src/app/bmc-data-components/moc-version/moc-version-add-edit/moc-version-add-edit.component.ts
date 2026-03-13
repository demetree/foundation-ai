/*
   GENERATED FORM FOR THE MOCVERSION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocVersion table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-version-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocVersionService, MocVersionData, MocVersionSubmitData } from '../../../bmc-data-services/moc-version.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MocVersionFormValues {
  publishedMocId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  commitMessage: string,
  mpdSnapshot: string,
  partCount: string | null,     // Stored as string for form input, converted to number on submit.
  addedPartCount: string | null,     // Stored as string for form input, converted to number on submit.
  removedPartCount: string | null,     // Stored as string for form input, converted to number on submit.
  modifiedPartCount: string | null,     // Stored as string for form input, converted to number on submit.
  snapshotDate: string,
  authorTenantGuid: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-moc-version-add-edit',
  templateUrl: './moc-version-add-edit.component.html',
  styleUrls: ['./moc-version-add-edit.component.scss']
})
export class MocVersionAddEditComponent {
  @ViewChild('mocVersionModal') mocVersionModal!: TemplateRef<any>;
  @Output() mocVersionChanged = new Subject<MocVersionData[]>();
  @Input() mocVersionSubmitData: MocVersionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocVersionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocVersionForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        versionNumber: [''],
        commitMessage: ['', Validators.required],
        mpdSnapshot: ['', Validators.required],
        partCount: [''],
        addedPartCount: [''],
        removedPartCount: [''],
        modifiedPartCount: [''],
        snapshotDate: ['', Validators.required],
        authorTenantGuid: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  mocVersions$ = this.mocVersionService.GetMocVersionList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  constructor(
    private modalService: NgbModal,
    private mocVersionService: MocVersionService,
    private publishedMocService: PublishedMocService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(mocVersionData?: MocVersionData) {

    if (mocVersionData != null) {

      if (!this.mocVersionService.userIsBMCMocVersionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Moc Versions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.mocVersionSubmitData = this.mocVersionService.ConvertToMocVersionSubmitData(mocVersionData);
      this.isEditMode = true;
      this.objectGuid = mocVersionData.objectGuid;

      this.buildFormValues(mocVersionData);

    } else {

      if (!this.mocVersionService.userIsBMCMocVersionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Moc Versions`,
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
        this.mocVersionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocVersionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.mocVersionModal, {
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

    if (this.mocVersionService.userIsBMCMocVersionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Moc Versions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.mocVersionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocVersionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocVersionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocVersionSubmitData: MocVersionSubmitData = {
        id: this.mocVersionSubmitData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        versionNumber: this.mocVersionSubmitData?.versionNumber ?? 0,
        commitMessage: formValue.commitMessage!.trim(),
        mpdSnapshot: formValue.mpdSnapshot!.trim(),
        partCount: formValue.partCount ? Number(formValue.partCount) : null,
        addedPartCount: formValue.addedPartCount ? Number(formValue.addedPartCount) : null,
        removedPartCount: formValue.removedPartCount ? Number(formValue.removedPartCount) : null,
        modifiedPartCount: formValue.modifiedPartCount ? Number(formValue.modifiedPartCount) : null,
        snapshotDate: dateTimeLocalToIsoUtc(formValue.snapshotDate!.trim())!,
        authorTenantGuid: formValue.authorTenantGuid!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMocVersion(mocVersionSubmitData);
      } else {
        this.addMocVersion(mocVersionSubmitData);
      }
  }

  private addMocVersion(mocVersionData: MocVersionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    mocVersionData.versionNumber = 0;
    mocVersionData.active = true;
    mocVersionData.deleted = false;
    this.mocVersionService.PostMocVersion(mocVersionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMocVersion) => {

        this.mocVersionService.ClearAllCaches();

        this.mocVersionChanged.next([newMocVersion]);

        this.alertService.showMessage("Moc Version added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/mocversion', newMocVersion.id]);
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
                                   'You do not have permission to save this Moc Version.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Version.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Version could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMocVersion(mocVersionData: MocVersionSubmitData) {
    this.mocVersionService.PutMocVersion(mocVersionData.id, mocVersionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMocVersion) => {

        this.mocVersionService.ClearAllCaches();

        this.mocVersionChanged.next([updatedMocVersion]);

        this.alertService.showMessage("Moc Version updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Moc Version.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Version.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Version could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(mocVersionData: MocVersionData | null) {

    if (mocVersionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocVersionForm.reset({
        publishedMocId: null,
        versionNumber: '',
        commitMessage: '',
        mpdSnapshot: '',
        partCount: '',
        addedPartCount: '',
        removedPartCount: '',
        modifiedPartCount: '',
        snapshotDate: '',
        authorTenantGuid: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocVersionForm.reset({
        publishedMocId: mocVersionData.publishedMocId,
        versionNumber: mocVersionData.versionNumber?.toString() ?? '',
        commitMessage: mocVersionData.commitMessage ?? '',
        mpdSnapshot: mocVersionData.mpdSnapshot ?? '',
        partCount: mocVersionData.partCount?.toString() ?? '',
        addedPartCount: mocVersionData.addedPartCount?.toString() ?? '',
        removedPartCount: mocVersionData.removedPartCount?.toString() ?? '',
        modifiedPartCount: mocVersionData.modifiedPartCount?.toString() ?? '',
        snapshotDate: isoUtcStringToDateTimeLocal(mocVersionData.snapshotDate) ?? '',
        authorTenantGuid: mocVersionData.authorTenantGuid ?? '',
        active: mocVersionData.active ?? true,
        deleted: mocVersionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocVersionForm.markAsPristine();
    this.mocVersionForm.markAsUntouched();
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


  public userIsBMCMocVersionReader(): boolean {
    return this.mocVersionService.userIsBMCMocVersionReader();
  }

  public userIsBMCMocVersionWriter(): boolean {
    return this.mocVersionService.userIsBMCMocVersionWriter();
  }
}

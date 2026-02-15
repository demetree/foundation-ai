/*
   GENERATED FORM FOR THE CONTENTREPORTREASON TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContentReportReason table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to content-report-reason-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContentReportReasonService, ContentReportReasonData, ContentReportReasonSubmitData } from '../../../bmc-data-services/content-report-reason.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ContentReportReasonFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-content-report-reason-add-edit',
  templateUrl: './content-report-reason-add-edit.component.html',
  styleUrls: ['./content-report-reason-add-edit.component.scss']
})
export class ContentReportReasonAddEditComponent {
  @ViewChild('contentReportReasonModal') contentReportReasonModal!: TemplateRef<any>;
  @Output() contentReportReasonChanged = new Subject<ContentReportReasonData[]>();
  @Input() contentReportReasonSubmitData: ContentReportReasonSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContentReportReasonFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contentReportReasonForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  contentReportReasons$ = this.contentReportReasonService.GetContentReportReasonList();

  constructor(
    private modalService: NgbModal,
    private contentReportReasonService: ContentReportReasonService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contentReportReasonData?: ContentReportReasonData) {

    if (contentReportReasonData != null) {

      if (!this.contentReportReasonService.userIsBMCContentReportReasonReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Content Report Reasons`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contentReportReasonSubmitData = this.contentReportReasonService.ConvertToContentReportReasonSubmitData(contentReportReasonData);
      this.isEditMode = true;
      this.objectGuid = contentReportReasonData.objectGuid;

      this.buildFormValues(contentReportReasonData);

    } else {

      if (!this.contentReportReasonService.userIsBMCContentReportReasonWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Content Report Reasons`,
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
        this.contentReportReasonForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contentReportReasonForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contentReportReasonModal, {
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

    if (this.contentReportReasonService.userIsBMCContentReportReasonWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Content Report Reasons`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contentReportReasonForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contentReportReasonForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contentReportReasonForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contentReportReasonSubmitData: ContentReportReasonSubmitData = {
        id: this.contentReportReasonSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContentReportReason(contentReportReasonSubmitData);
      } else {
        this.addContentReportReason(contentReportReasonSubmitData);
      }
  }

  private addContentReportReason(contentReportReasonData: ContentReportReasonSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contentReportReasonData.active = true;
    contentReportReasonData.deleted = false;
    this.contentReportReasonService.PostContentReportReason(contentReportReasonData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContentReportReason) => {

        this.contentReportReasonService.ClearAllCaches();

        this.contentReportReasonChanged.next([newContentReportReason]);

        this.alertService.showMessage("Content Report Reason added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contentreportreason', newContentReportReason.id]);
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
                                   'You do not have permission to save this Content Report Reason.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Content Report Reason.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Content Report Reason could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContentReportReason(contentReportReasonData: ContentReportReasonSubmitData) {
    this.contentReportReasonService.PutContentReportReason(contentReportReasonData.id, contentReportReasonData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContentReportReason) => {

        this.contentReportReasonService.ClearAllCaches();

        this.contentReportReasonChanged.next([updatedContentReportReason]);

        this.alertService.showMessage("Content Report Reason updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Content Report Reason.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Content Report Reason.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Content Report Reason could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contentReportReasonData: ContentReportReasonData | null) {

    if (contentReportReasonData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contentReportReasonForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contentReportReasonForm.reset({
        name: contentReportReasonData.name ?? '',
        description: contentReportReasonData.description ?? '',
        sequence: contentReportReasonData.sequence?.toString() ?? '',
        active: contentReportReasonData.active ?? true,
        deleted: contentReportReasonData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contentReportReasonForm.markAsPristine();
    this.contentReportReasonForm.markAsUntouched();
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


  public userIsBMCContentReportReasonReader(): boolean {
    return this.contentReportReasonService.userIsBMCContentReportReasonReader();
  }

  public userIsBMCContentReportReasonWriter(): boolean {
    return this.contentReportReasonService.userIsBMCContentReportReasonWriter();
  }
}

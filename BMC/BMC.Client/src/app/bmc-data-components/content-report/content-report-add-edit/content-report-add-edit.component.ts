/*
   GENERATED FORM FOR THE CONTENTREPORT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContentReport table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to content-report-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContentReportService, ContentReportData, ContentReportSubmitData } from '../../../bmc-data-services/content-report.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ContentReportReasonService } from '../../../bmc-data-services/content-report-reason.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ContentReportFormValues {
  contentReportReasonId: number | bigint,       // For FK link number
  reporterTenantGuid: string,
  reportedEntityType: string,
  reportedEntityId: string,     // Stored as string for form input, converted to number on submit.
  description: string | null,
  status: string,
  reportedDate: string,
  reviewedDate: string | null,
  reviewerTenantGuid: string | null,
  reviewNotes: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-content-report-add-edit',
  templateUrl: './content-report-add-edit.component.html',
  styleUrls: ['./content-report-add-edit.component.scss']
})
export class ContentReportAddEditComponent {
  @ViewChild('contentReportModal') contentReportModal!: TemplateRef<any>;
  @Output() contentReportChanged = new Subject<ContentReportData[]>();
  @Input() contentReportSubmitData: ContentReportSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContentReportFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contentReportForm: FormGroup = this.fb.group({
        contentReportReasonId: [null, Validators.required],
        reporterTenantGuid: ['', Validators.required],
        reportedEntityType: ['', Validators.required],
        reportedEntityId: ['', Validators.required],
        description: [''],
        status: ['', Validators.required],
        reportedDate: ['', Validators.required],
        reviewedDate: [''],
        reviewerTenantGuid: [''],
        reviewNotes: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  contentReports$ = this.contentReportService.GetContentReportList();
  contentReportReasons$ = this.contentReportReasonService.GetContentReportReasonList();

  constructor(
    private modalService: NgbModal,
    private contentReportService: ContentReportService,
    private contentReportReasonService: ContentReportReasonService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(contentReportData?: ContentReportData) {

    if (contentReportData != null) {

      if (!this.contentReportService.userIsBMCContentReportReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Content Reports`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.contentReportSubmitData = this.contentReportService.ConvertToContentReportSubmitData(contentReportData);
      this.isEditMode = true;
      this.objectGuid = contentReportData.objectGuid;

      this.buildFormValues(contentReportData);

    } else {

      if (!this.contentReportService.userIsBMCContentReportWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Content Reports`,
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
        this.contentReportForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contentReportForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.contentReportModal, {
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

    if (this.contentReportService.userIsBMCContentReportWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Content Reports`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.contentReportForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contentReportForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contentReportForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contentReportSubmitData: ContentReportSubmitData = {
        id: this.contentReportSubmitData?.id || 0,
        contentReportReasonId: Number(formValue.contentReportReasonId),
        reporterTenantGuid: formValue.reporterTenantGuid!.trim(),
        reportedEntityType: formValue.reportedEntityType!.trim(),
        reportedEntityId: Number(formValue.reportedEntityId),
        description: formValue.description?.trim() || null,
        status: formValue.status!.trim(),
        reportedDate: dateTimeLocalToIsoUtc(formValue.reportedDate!.trim())!,
        reviewedDate: formValue.reviewedDate ? dateTimeLocalToIsoUtc(formValue.reviewedDate.trim()) : null,
        reviewerTenantGuid: formValue.reviewerTenantGuid?.trim() || null,
        reviewNotes: formValue.reviewNotes?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateContentReport(contentReportSubmitData);
      } else {
        this.addContentReport(contentReportSubmitData);
      }
  }

  private addContentReport(contentReportData: ContentReportSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    contentReportData.active = true;
    contentReportData.deleted = false;
    this.contentReportService.PostContentReport(contentReportData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newContentReport) => {

        this.contentReportService.ClearAllCaches();

        this.contentReportChanged.next([newContentReport]);

        this.alertService.showMessage("Content Report added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/contentreport', newContentReport.id]);
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
                                   'You do not have permission to save this Content Report.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Content Report.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Content Report could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateContentReport(contentReportData: ContentReportSubmitData) {
    this.contentReportService.PutContentReport(contentReportData.id, contentReportData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedContentReport) => {

        this.contentReportService.ClearAllCaches();

        this.contentReportChanged.next([updatedContentReport]);

        this.alertService.showMessage("Content Report updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Content Report.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Content Report.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Content Report could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(contentReportData: ContentReportData | null) {

    if (contentReportData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contentReportForm.reset({
        contentReportReasonId: null,
        reporterTenantGuid: '',
        reportedEntityType: '',
        reportedEntityId: '',
        description: '',
        status: '',
        reportedDate: '',
        reviewedDate: '',
        reviewerTenantGuid: '',
        reviewNotes: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contentReportForm.reset({
        contentReportReasonId: contentReportData.contentReportReasonId,
        reporterTenantGuid: contentReportData.reporterTenantGuid ?? '',
        reportedEntityType: contentReportData.reportedEntityType ?? '',
        reportedEntityId: contentReportData.reportedEntityId?.toString() ?? '',
        description: contentReportData.description ?? '',
        status: contentReportData.status ?? '',
        reportedDate: isoUtcStringToDateTimeLocal(contentReportData.reportedDate) ?? '',
        reviewedDate: isoUtcStringToDateTimeLocal(contentReportData.reviewedDate) ?? '',
        reviewerTenantGuid: contentReportData.reviewerTenantGuid ?? '',
        reviewNotes: contentReportData.reviewNotes ?? '',
        active: contentReportData.active ?? true,
        deleted: contentReportData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contentReportForm.markAsPristine();
    this.contentReportForm.markAsUntouched();
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


  public userIsBMCContentReportReader(): boolean {
    return this.contentReportService.userIsBMCContentReportReader();
  }

  public userIsBMCContentReportWriter(): boolean {
    return this.contentReportService.userIsBMCContentReportWriter();
  }
}

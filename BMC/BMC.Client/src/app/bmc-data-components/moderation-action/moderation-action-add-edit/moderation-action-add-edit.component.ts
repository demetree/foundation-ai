/*
   GENERATED FORM FOR THE MODERATIONACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ModerationAction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moderation-action-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModerationActionService, ModerationActionData, ModerationActionSubmitData } from '../../../bmc-data-services/moderation-action.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ContentReportService } from '../../../bmc-data-services/content-report.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ModerationActionFormValues {
  moderatorTenantGuid: string,
  actionType: string,
  targetTenantGuid: string | null,
  targetEntityType: string | null,
  targetEntityId: string | null,     // Stored as string for form input, converted to number on submit.
  reason: string | null,
  actionDate: string,
  contentReportId: number | bigint | null,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-moderation-action-add-edit',
  templateUrl: './moderation-action-add-edit.component.html',
  styleUrls: ['./moderation-action-add-edit.component.scss']
})
export class ModerationActionAddEditComponent {
  @ViewChild('moderationActionModal') moderationActionModal!: TemplateRef<any>;
  @Output() moderationActionChanged = new Subject<ModerationActionData[]>();
  @Input() moderationActionSubmitData: ModerationActionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ModerationActionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public moderationActionForm: FormGroup = this.fb.group({
        moderatorTenantGuid: ['', Validators.required],
        actionType: ['', Validators.required],
        targetTenantGuid: [''],
        targetEntityType: [''],
        targetEntityId: [''],
        reason: [''],
        actionDate: ['', Validators.required],
        contentReportId: [null],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  moderationActions$ = this.moderationActionService.GetModerationActionList();
  contentReports$ = this.contentReportService.GetContentReportList();

  constructor(
    private modalService: NgbModal,
    private moderationActionService: ModerationActionService,
    private contentReportService: ContentReportService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(moderationActionData?: ModerationActionData) {

    if (moderationActionData != null) {

      if (!this.moderationActionService.userIsBMCModerationActionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Moderation Actions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.moderationActionSubmitData = this.moderationActionService.ConvertToModerationActionSubmitData(moderationActionData);
      this.isEditMode = true;
      this.objectGuid = moderationActionData.objectGuid;

      this.buildFormValues(moderationActionData);

    } else {

      if (!this.moderationActionService.userIsBMCModerationActionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Moderation Actions`,
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
        this.moderationActionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.moderationActionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.moderationActionModal, {
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

    if (this.moderationActionService.userIsBMCModerationActionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Moderation Actions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.moderationActionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.moderationActionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.moderationActionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const moderationActionSubmitData: ModerationActionSubmitData = {
        id: this.moderationActionSubmitData?.id || 0,
        moderatorTenantGuid: formValue.moderatorTenantGuid!.trim(),
        actionType: formValue.actionType!.trim(),
        targetTenantGuid: formValue.targetTenantGuid?.trim() || null,
        targetEntityType: formValue.targetEntityType?.trim() || null,
        targetEntityId: formValue.targetEntityId ? Number(formValue.targetEntityId) : null,
        reason: formValue.reason?.trim() || null,
        actionDate: dateTimeLocalToIsoUtc(formValue.actionDate!.trim())!,
        contentReportId: formValue.contentReportId ? Number(formValue.contentReportId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateModerationAction(moderationActionSubmitData);
      } else {
        this.addModerationAction(moderationActionSubmitData);
      }
  }

  private addModerationAction(moderationActionData: ModerationActionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    moderationActionData.active = true;
    moderationActionData.deleted = false;
    this.moderationActionService.PostModerationAction(moderationActionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newModerationAction) => {

        this.moderationActionService.ClearAllCaches();

        this.moderationActionChanged.next([newModerationAction]);

        this.alertService.showMessage("Moderation Action added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/moderationaction', newModerationAction.id]);
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
                                   'You do not have permission to save this Moderation Action.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moderation Action.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moderation Action could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateModerationAction(moderationActionData: ModerationActionSubmitData) {
    this.moderationActionService.PutModerationAction(moderationActionData.id, moderationActionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedModerationAction) => {

        this.moderationActionService.ClearAllCaches();

        this.moderationActionChanged.next([updatedModerationAction]);

        this.alertService.showMessage("Moderation Action updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Moderation Action.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moderation Action.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moderation Action could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(moderationActionData: ModerationActionData | null) {

    if (moderationActionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.moderationActionForm.reset({
        moderatorTenantGuid: '',
        actionType: '',
        targetTenantGuid: '',
        targetEntityType: '',
        targetEntityId: '',
        reason: '',
        actionDate: '',
        contentReportId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.moderationActionForm.reset({
        moderatorTenantGuid: moderationActionData.moderatorTenantGuid ?? '',
        actionType: moderationActionData.actionType ?? '',
        targetTenantGuid: moderationActionData.targetTenantGuid ?? '',
        targetEntityType: moderationActionData.targetEntityType ?? '',
        targetEntityId: moderationActionData.targetEntityId?.toString() ?? '',
        reason: moderationActionData.reason ?? '',
        actionDate: isoUtcStringToDateTimeLocal(moderationActionData.actionDate) ?? '',
        contentReportId: moderationActionData.contentReportId,
        active: moderationActionData.active ?? true,
        deleted: moderationActionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.moderationActionForm.markAsPristine();
    this.moderationActionForm.markAsUntouched();
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


  public userIsBMCModerationActionReader(): boolean {
    return this.moderationActionService.userIsBMCModerationActionReader();
  }

  public userIsBMCModerationActionWriter(): boolean {
    return this.moderationActionService.userIsBMCModerationActionWriter();
  }
}

/*
   GENERATED FORM FOR THE SCHEDULINGTARGETQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetQualificationRequirement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-qualification-requirement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetQualificationRequirementService, SchedulingTargetQualificationRequirementData, SchedulingTargetQualificationRequirementSubmitData } from '../../../scheduler-data-services/scheduling-target-qualification-requirement.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SchedulingTargetQualificationRequirementFormValues {
  schedulingTargetId: number | bigint,       // For FK link number
  qualificationId: number | bigint,       // For FK link number
  isRequired: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-scheduling-target-qualification-requirement-add-edit',
  templateUrl: './scheduling-target-qualification-requirement-add-edit.component.html',
  styleUrls: ['./scheduling-target-qualification-requirement-add-edit.component.scss']
})
export class SchedulingTargetQualificationRequirementAddEditComponent {
  @ViewChild('schedulingTargetQualificationRequirementModal') schedulingTargetQualificationRequirementModal!: TemplateRef<any>;
  @Output() schedulingTargetQualificationRequirementChanged = new Subject<SchedulingTargetQualificationRequirementData[]>();
  @Input() schedulingTargetQualificationRequirementSubmitData: SchedulingTargetQualificationRequirementSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetQualificationRequirementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetQualificationRequirementForm: FormGroup = this.fb.group({
        schedulingTargetId: [null, Validators.required],
        qualificationId: [null, Validators.required],
        isRequired: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  schedulingTargetQualificationRequirements$ = this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirementList();
  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  qualifications$ = this.qualificationService.GetQualificationList();

  constructor(
    private modalService: NgbModal,
    private schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService,
    private schedulingTargetService: SchedulingTargetService,
    private qualificationService: QualificationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(schedulingTargetQualificationRequirementData?: SchedulingTargetQualificationRequirementData) {

    if (schedulingTargetQualificationRequirementData != null) {

      if (!this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduling Target Qualification Requirements`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.schedulingTargetQualificationRequirementSubmitData = this.schedulingTargetQualificationRequirementService.ConvertToSchedulingTargetQualificationRequirementSubmitData(schedulingTargetQualificationRequirementData);
      this.isEditMode = true;
      this.objectGuid = schedulingTargetQualificationRequirementData.objectGuid;

      this.buildFormValues(schedulingTargetQualificationRequirementData);

    } else {

      if (!this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Qualification Requirements`,
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
        this.schedulingTargetQualificationRequirementForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetQualificationRequirementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.schedulingTargetQualificationRequirementModal, {
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

    if (this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Qualification Requirements`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.schedulingTargetQualificationRequirementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetQualificationRequirementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetQualificationRequirementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetQualificationRequirementSubmitData: SchedulingTargetQualificationRequirementSubmitData = {
        id: this.schedulingTargetQualificationRequirementSubmitData?.id || 0,
        schedulingTargetId: Number(formValue.schedulingTargetId),
        qualificationId: Number(formValue.qualificationId),
        isRequired: !!formValue.isRequired,
        versionNumber: this.schedulingTargetQualificationRequirementSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementSubmitData);
      } else {
        this.addSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementSubmitData);
      }
  }

  private addSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementData: SchedulingTargetQualificationRequirementSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    schedulingTargetQualificationRequirementData.versionNumber = 0;
    schedulingTargetQualificationRequirementData.active = true;
    schedulingTargetQualificationRequirementData.deleted = false;
    this.schedulingTargetQualificationRequirementService.PostSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSchedulingTargetQualificationRequirement) => {

        this.schedulingTargetQualificationRequirementService.ClearAllCaches();

        this.schedulingTargetQualificationRequirementChanged.next([newSchedulingTargetQualificationRequirement]);

        this.alertService.showMessage("Scheduling Target Qualification Requirement added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulingtargetqualificationrequirement', newSchedulingTargetQualificationRequirement.id]);
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
                                   'You do not have permission to save this Scheduling Target Qualification Requirement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Qualification Requirement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Qualification Requirement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementData: SchedulingTargetQualificationRequirementSubmitData) {
    this.schedulingTargetQualificationRequirementService.PutSchedulingTargetQualificationRequirement(schedulingTargetQualificationRequirementData.id, schedulingTargetQualificationRequirementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSchedulingTargetQualificationRequirement) => {

        this.schedulingTargetQualificationRequirementService.ClearAllCaches();

        this.schedulingTargetQualificationRequirementChanged.next([updatedSchedulingTargetQualificationRequirement]);

        this.alertService.showMessage("Scheduling Target Qualification Requirement updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduling Target Qualification Requirement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Qualification Requirement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Qualification Requirement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(schedulingTargetQualificationRequirementData: SchedulingTargetQualificationRequirementData | null) {

    if (schedulingTargetQualificationRequirementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetQualificationRequirementForm.reset({
        schedulingTargetId: null,
        qualificationId: null,
        isRequired: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.schedulingTargetQualificationRequirementForm.reset({
        schedulingTargetId: schedulingTargetQualificationRequirementData.schedulingTargetId,
        qualificationId: schedulingTargetQualificationRequirementData.qualificationId,
        isRequired: schedulingTargetQualificationRequirementData.isRequired ?? false,
        versionNumber: schedulingTargetQualificationRequirementData.versionNumber?.toString() ?? '',
        active: schedulingTargetQualificationRequirementData.active ?? true,
        deleted: schedulingTargetQualificationRequirementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetQualificationRequirementForm.markAsPristine();
    this.schedulingTargetQualificationRequirementForm.markAsUntouched();
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


  public userIsSchedulerSchedulingTargetQualificationRequirementReader(): boolean {
    return this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementReader();
  }

  public userIsSchedulerSchedulingTargetQualificationRequirementWriter(): boolean {
    return this.schedulingTargetQualificationRequirementService.userIsSchedulerSchedulingTargetQualificationRequirementWriter();
  }
}

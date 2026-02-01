/*
   GENERATED FORM FOR THE SCHEDULELAYERMEMBER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleLayerMember table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-layer-member-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduleLayerMemberService, ScheduleLayerMemberData, ScheduleLayerMemberSubmitData } from '../../../alerting-data-services/schedule-layer-member.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduleLayerService } from '../../../alerting-data-services/schedule-layer.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduleLayerMemberFormValues {
  scheduleLayerId: number | bigint,       // For FK link number
  position: string,     // Stored as string for form input, converted to number on submit.
  securityUserObjectGuid: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-schedule-layer-member-add-edit',
  templateUrl: './schedule-layer-member-add-edit.component.html',
  styleUrls: ['./schedule-layer-member-add-edit.component.scss']
})
export class ScheduleLayerMemberAddEditComponent {
  @ViewChild('scheduleLayerMemberModal') scheduleLayerMemberModal!: TemplateRef<any>;
  @Output() scheduleLayerMemberChanged = new Subject<ScheduleLayerMemberData[]>();
  @Input() scheduleLayerMemberSubmitData: ScheduleLayerMemberSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduleLayerMemberFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduleLayerMemberForm: FormGroup = this.fb.group({
        scheduleLayerId: [null, Validators.required],
        position: ['', Validators.required],
        securityUserObjectGuid: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  scheduleLayerMembers$ = this.scheduleLayerMemberService.GetScheduleLayerMemberList();
  scheduleLayers$ = this.scheduleLayerService.GetScheduleLayerList();

  constructor(
    private modalService: NgbModal,
    private scheduleLayerMemberService: ScheduleLayerMemberService,
    private scheduleLayerService: ScheduleLayerService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduleLayerMemberData?: ScheduleLayerMemberData) {

    if (scheduleLayerMemberData != null) {

      if (!this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Schedule Layer Members`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduleLayerMemberSubmitData = this.scheduleLayerMemberService.ConvertToScheduleLayerMemberSubmitData(scheduleLayerMemberData);
      this.isEditMode = true;
      this.objectGuid = scheduleLayerMemberData.objectGuid;

      this.buildFormValues(scheduleLayerMemberData);

    } else {

      if (!this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Schedule Layer Members`,
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
        this.scheduleLayerMemberForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduleLayerMemberForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduleLayerMemberModal, {
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

    if (this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Schedule Layer Members`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduleLayerMemberForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduleLayerMemberForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduleLayerMemberForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduleLayerMemberSubmitData: ScheduleLayerMemberSubmitData = {
        id: this.scheduleLayerMemberSubmitData?.id || 0,
        scheduleLayerId: Number(formValue.scheduleLayerId),
        position: Number(formValue.position),
        securityUserObjectGuid: formValue.securityUserObjectGuid!.trim(),
        versionNumber: this.scheduleLayerMemberSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateScheduleLayerMember(scheduleLayerMemberSubmitData);
      } else {
        this.addScheduleLayerMember(scheduleLayerMemberSubmitData);
      }
  }

  private addScheduleLayerMember(scheduleLayerMemberData: ScheduleLayerMemberSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduleLayerMemberData.versionNumber = 0;
    scheduleLayerMemberData.active = true;
    scheduleLayerMemberData.deleted = false;
    this.scheduleLayerMemberService.PostScheduleLayerMember(scheduleLayerMemberData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduleLayerMember) => {

        this.scheduleLayerMemberService.ClearAllCaches();

        this.scheduleLayerMemberChanged.next([newScheduleLayerMember]);

        this.alertService.showMessage("Schedule Layer Member added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulelayermember', newScheduleLayerMember.id]);
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
                                   'You do not have permission to save this Schedule Layer Member.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Layer Member.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Layer Member could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduleLayerMember(scheduleLayerMemberData: ScheduleLayerMemberSubmitData) {
    this.scheduleLayerMemberService.PutScheduleLayerMember(scheduleLayerMemberData.id, scheduleLayerMemberData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduleLayerMember) => {

        this.scheduleLayerMemberService.ClearAllCaches();

        this.scheduleLayerMemberChanged.next([updatedScheduleLayerMember]);

        this.alertService.showMessage("Schedule Layer Member updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Schedule Layer Member.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Layer Member.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Layer Member could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduleLayerMemberData: ScheduleLayerMemberData | null) {

    if (scheduleLayerMemberData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduleLayerMemberForm.reset({
        scheduleLayerId: null,
        position: '',
        securityUserObjectGuid: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduleLayerMemberForm.reset({
        scheduleLayerId: scheduleLayerMemberData.scheduleLayerId,
        position: scheduleLayerMemberData.position?.toString() ?? '',
        securityUserObjectGuid: scheduleLayerMemberData.securityUserObjectGuid ?? '',
        versionNumber: scheduleLayerMemberData.versionNumber?.toString() ?? '',
        active: scheduleLayerMemberData.active ?? true,
        deleted: scheduleLayerMemberData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduleLayerMemberForm.markAsPristine();
    this.scheduleLayerMemberForm.markAsUntouched();
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


  public userIsAlertingScheduleLayerMemberReader(): boolean {
    return this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberReader();
  }

  public userIsAlertingScheduleLayerMemberWriter(): boolean {
    return this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberWriter();
  }
}

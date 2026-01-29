/*
   GENERATED FORM FOR THE VOLUNTEERSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from VolunteerStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to volunteer-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerStatusService, VolunteerStatusData, VolunteerStatusSubmitData } from '../../../scheduler-data-services/volunteer-status.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface VolunteerStatusFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  iconId: number | bigint | null,       // For FK link number
  isActive: boolean | null,
  preventsScheduling: boolean,
  requiresApproval: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-volunteer-status-add-edit',
  templateUrl: './volunteer-status-add-edit.component.html',
  styleUrls: ['./volunteer-status-add-edit.component.scss']
})
export class VolunteerStatusAddEditComponent {
  @ViewChild('volunteerStatusModal') volunteerStatusModal!: TemplateRef<any>;
  @Output() volunteerStatusChanged = new Subject<VolunteerStatusData[]>();
  @Input() volunteerStatusSubmitData: VolunteerStatusSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<VolunteerStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public volunteerStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        iconId: [null],
        isActive: [false],
        preventsScheduling: [false],
        requiresApproval: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private volunteerStatusService: VolunteerStatusService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(volunteerStatusData?: VolunteerStatusData) {

    if (volunteerStatusData != null) {

      if (!this.volunteerStatusService.userIsSchedulerVolunteerStatusReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Volunteer Statuses`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.volunteerStatusSubmitData = this.volunteerStatusService.ConvertToVolunteerStatusSubmitData(volunteerStatusData);
      this.isEditMode = true;
      this.objectGuid = volunteerStatusData.objectGuid;

      this.buildFormValues(volunteerStatusData);

    } else {

      if (!this.volunteerStatusService.userIsSchedulerVolunteerStatusWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Volunteer Statuses`,
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
        this.volunteerStatusForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.volunteerStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.volunteerStatusModal, {
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

    if (this.volunteerStatusService.userIsSchedulerVolunteerStatusWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Volunteer Statuses`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.volunteerStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.volunteerStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.volunteerStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const volunteerStatusSubmitData: VolunteerStatusSubmitData = {
        id: this.volunteerStatusSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        isActive: formValue.isActive == true ? true : formValue.isActive == false ? false : null,
        preventsScheduling: !!formValue.preventsScheduling,
        requiresApproval: !!formValue.requiresApproval,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateVolunteerStatus(volunteerStatusSubmitData);
      } else {
        this.addVolunteerStatus(volunteerStatusSubmitData);
      }
  }

  private addVolunteerStatus(volunteerStatusData: VolunteerStatusSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    volunteerStatusData.active = true;
    volunteerStatusData.deleted = false;
    this.volunteerStatusService.PostVolunteerStatus(volunteerStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newVolunteerStatus) => {

        this.volunteerStatusService.ClearAllCaches();

        this.volunteerStatusChanged.next([newVolunteerStatus]);

        this.alertService.showMessage("Volunteer Status added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/volunteerstatus', newVolunteerStatus.id]);
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
                                   'You do not have permission to save this Volunteer Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Volunteer Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Volunteer Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateVolunteerStatus(volunteerStatusData: VolunteerStatusSubmitData) {
    this.volunteerStatusService.PutVolunteerStatus(volunteerStatusData.id, volunteerStatusData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedVolunteerStatus) => {

        this.volunteerStatusService.ClearAllCaches();

        this.volunteerStatusChanged.next([updatedVolunteerStatus]);

        this.alertService.showMessage("Volunteer Status updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Volunteer Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Volunteer Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Volunteer Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(volunteerStatusData: VolunteerStatusData | null) {

    if (volunteerStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.volunteerStatusForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        iconId: null,
        isActive: false,
        preventsScheduling: false,
        requiresApproval: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.volunteerStatusForm.reset({
        name: volunteerStatusData.name ?? '',
        description: volunteerStatusData.description ?? '',
        sequence: volunteerStatusData.sequence?.toString() ?? '',
        color: volunteerStatusData.color ?? '',
        iconId: volunteerStatusData.iconId,
        isActive: volunteerStatusData.isActive ?? false,
        preventsScheduling: volunteerStatusData.preventsScheduling ?? false,
        requiresApproval: volunteerStatusData.requiresApproval ?? false,
        active: volunteerStatusData.active ?? true,
        deleted: volunteerStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.volunteerStatusForm.markAsPristine();
    this.volunteerStatusForm.markAsUntouched();
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


  public userIsSchedulerVolunteerStatusReader(): boolean {
    return this.volunteerStatusService.userIsSchedulerVolunteerStatusReader();
  }

  public userIsSchedulerVolunteerStatusWriter(): boolean {
    return this.volunteerStatusService.userIsSchedulerVolunteerStatusWriter();
  }
}

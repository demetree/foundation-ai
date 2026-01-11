/*
   GENERATED FORM FOR THE RESOURCESHIFT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceShift table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-shift-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceShiftService, ResourceShiftData, ResourceShiftSubmitData } from '../../../scheduler-data-services/resource-shift.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ResourceShiftFormValues {
  resourceId: number | bigint,       // For FK link number
  dayOfWeek: string,     // Stored as string for form input, converted to number on submit.
  timeZoneId: number | bigint | null,       // For FK link number
  startTime: string,
  hours: string,     // Stored as string for form input, converted to number on submit.
  label: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-resource-shift-add-edit',
  templateUrl: './resource-shift-add-edit.component.html',
  styleUrls: ['./resource-shift-add-edit.component.scss']
})
export class ResourceShiftAddEditComponent {
  @ViewChild('resourceShiftModal') resourceShiftModal!: TemplateRef<any>;
  @Output() resourceShiftChanged = new Subject<ResourceShiftData[]>();
  @Input() resourceShiftSubmitData: ResourceShiftSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceShiftFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceShiftForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        dayOfWeek: ['', Validators.required],
        timeZoneId: [null],
        startTime: ['', Validators.required],
        hours: ['', Validators.required],
        label: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  resourceShifts$ = this.resourceShiftService.GetResourceShiftList();
  resources$ = this.resourceService.GetResourceList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();

  constructor(
    private modalService: NgbModal,
    private resourceShiftService: ResourceShiftService,
    private resourceService: ResourceService,
    private timeZoneService: TimeZoneService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(resourceShiftData?: ResourceShiftData) {

    if (resourceShiftData != null) {

      if (!this.resourceShiftService.userIsSchedulerResourceShiftReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Resource Shifts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.resourceShiftSubmitData = this.resourceShiftService.ConvertToResourceShiftSubmitData(resourceShiftData);
      this.isEditMode = true;
      this.objectGuid = resourceShiftData.objectGuid;

      this.buildFormValues(resourceShiftData);

    } else {

      if (!this.resourceShiftService.userIsSchedulerResourceShiftWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Resource Shifts`,
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
        this.resourceShiftForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceShiftForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.resourceShiftModal, {
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

    if (this.resourceShiftService.userIsSchedulerResourceShiftWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Resource Shifts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.resourceShiftForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceShiftForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceShiftForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceShiftSubmitData: ResourceShiftSubmitData = {
        id: this.resourceShiftSubmitData?.id || 0,
        resourceId: Number(formValue.resourceId),
        dayOfWeek: Number(formValue.dayOfWeek),
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        startTime: formValue.startTime!.trim(),
        hours: Number(formValue.hours),
        label: formValue.label?.trim() || null,
        versionNumber: this.resourceShiftSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateResourceShift(resourceShiftSubmitData);
      } else {
        this.addResourceShift(resourceShiftSubmitData);
      }
  }

  private addResourceShift(resourceShiftData: ResourceShiftSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    resourceShiftData.versionNumber = 0;
    resourceShiftData.active = true;
    resourceShiftData.deleted = false;
    this.resourceShiftService.PostResourceShift(resourceShiftData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newResourceShift) => {

        this.resourceShiftService.ClearAllCaches();

        this.resourceShiftChanged.next([newResourceShift]);

        this.alertService.showMessage("Resource Shift added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/resourceshift', newResourceShift.id]);
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
                                   'You do not have permission to save this Resource Shift.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Shift.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Shift could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateResourceShift(resourceShiftData: ResourceShiftSubmitData) {
    this.resourceShiftService.PutResourceShift(resourceShiftData.id, resourceShiftData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedResourceShift) => {

        this.resourceShiftService.ClearAllCaches();

        this.resourceShiftChanged.next([updatedResourceShift]);

        this.alertService.showMessage("Resource Shift updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Resource Shift.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Shift.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Shift could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(resourceShiftData: ResourceShiftData | null) {

    if (resourceShiftData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceShiftForm.reset({
        resourceId: null,
        dayOfWeek: '',
        timeZoneId: null,
        startTime: '',
        hours: '',
        label: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceShiftForm.reset({
        resourceId: resourceShiftData.resourceId,
        dayOfWeek: resourceShiftData.dayOfWeek?.toString() ?? '',
        timeZoneId: resourceShiftData.timeZoneId,
        startTime: resourceShiftData.startTime ?? '',
        hours: resourceShiftData.hours?.toString() ?? '',
        label: resourceShiftData.label ?? '',
        versionNumber: resourceShiftData.versionNumber?.toString() ?? '',
        active: resourceShiftData.active ?? true,
        deleted: resourceShiftData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceShiftForm.markAsPristine();
    this.resourceShiftForm.markAsUntouched();
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


  public userIsSchedulerResourceShiftReader(): boolean {
    return this.resourceShiftService.userIsSchedulerResourceShiftReader();
  }

  public userIsSchedulerResourceShiftWriter(): boolean {
    return this.resourceShiftService.userIsSchedulerResourceShiftWriter();
  }
}

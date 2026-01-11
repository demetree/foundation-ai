/*
   GENERATED FORM FOR THE RESOURCEAVAILABILITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceAvailability table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-availability-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceAvailabilityService, ResourceAvailabilityData, ResourceAvailabilitySubmitData } from '../../../scheduler-data-services/resource-availability.service';
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
interface ResourceAvailabilityFormValues {
  resourceId: number | bigint,       // For FK link number
  timeZoneId: number | bigint | null,       // For FK link number
  startDateTime: string,
  endDateTime: string | null,
  reason: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-resource-availability-add-edit',
  templateUrl: './resource-availability-add-edit.component.html',
  styleUrls: ['./resource-availability-add-edit.component.scss']
})
export class ResourceAvailabilityAddEditComponent {
  @ViewChild('resourceAvailabilityModal') resourceAvailabilityModal!: TemplateRef<any>;
  @Output() resourceAvailabilityChanged = new Subject<ResourceAvailabilityData[]>();
  @Input() resourceAvailabilitySubmitData: ResourceAvailabilitySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceAvailabilityFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceAvailabilityForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        timeZoneId: [null],
        startDateTime: ['', Validators.required],
        endDateTime: [''],
        reason: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  resourceAvailabilities$ = this.resourceAvailabilityService.GetResourceAvailabilityList();
  resources$ = this.resourceService.GetResourceList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();

  constructor(
    private modalService: NgbModal,
    private resourceAvailabilityService: ResourceAvailabilityService,
    private resourceService: ResourceService,
    private timeZoneService: TimeZoneService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(resourceAvailabilityData?: ResourceAvailabilityData) {

    if (resourceAvailabilityData != null) {

      if (!this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Resource Availabilities`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.resourceAvailabilitySubmitData = this.resourceAvailabilityService.ConvertToResourceAvailabilitySubmitData(resourceAvailabilityData);
      this.isEditMode = true;
      this.objectGuid = resourceAvailabilityData.objectGuid;

      this.buildFormValues(resourceAvailabilityData);

    } else {

      if (!this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Resource Availabilities`,
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
        this.resourceAvailabilityForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceAvailabilityForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.resourceAvailabilityModal, {
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

    if (this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Resource Availabilities`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.resourceAvailabilityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceAvailabilityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceAvailabilityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceAvailabilitySubmitData: ResourceAvailabilitySubmitData = {
        id: this.resourceAvailabilitySubmitData?.id || 0,
        resourceId: Number(formValue.resourceId),
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        startDateTime: dateTimeLocalToIsoUtc(formValue.startDateTime!.trim())!,
        endDateTime: formValue.endDateTime ? dateTimeLocalToIsoUtc(formValue.endDateTime.trim()) : null,
        reason: formValue.reason?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.resourceAvailabilitySubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateResourceAvailability(resourceAvailabilitySubmitData);
      } else {
        this.addResourceAvailability(resourceAvailabilitySubmitData);
      }
  }

  private addResourceAvailability(resourceAvailabilityData: ResourceAvailabilitySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    resourceAvailabilityData.versionNumber = 0;
    resourceAvailabilityData.active = true;
    resourceAvailabilityData.deleted = false;
    this.resourceAvailabilityService.PostResourceAvailability(resourceAvailabilityData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newResourceAvailability) => {

        this.resourceAvailabilityService.ClearAllCaches();

        this.resourceAvailabilityChanged.next([newResourceAvailability]);

        this.alertService.showMessage("Resource Availability added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/resourceavailability', newResourceAvailability.id]);
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
                                   'You do not have permission to save this Resource Availability.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Availability.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Availability could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateResourceAvailability(resourceAvailabilityData: ResourceAvailabilitySubmitData) {
    this.resourceAvailabilityService.PutResourceAvailability(resourceAvailabilityData.id, resourceAvailabilityData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedResourceAvailability) => {

        this.resourceAvailabilityService.ClearAllCaches();

        this.resourceAvailabilityChanged.next([updatedResourceAvailability]);

        this.alertService.showMessage("Resource Availability updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Resource Availability.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Availability.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Availability could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(resourceAvailabilityData: ResourceAvailabilityData | null) {

    if (resourceAvailabilityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceAvailabilityForm.reset({
        resourceId: null,
        timeZoneId: null,
        startDateTime: '',
        endDateTime: '',
        reason: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceAvailabilityForm.reset({
        resourceId: resourceAvailabilityData.resourceId,
        timeZoneId: resourceAvailabilityData.timeZoneId,
        startDateTime: isoUtcStringToDateTimeLocal(resourceAvailabilityData.startDateTime) ?? '',
        endDateTime: isoUtcStringToDateTimeLocal(resourceAvailabilityData.endDateTime) ?? '',
        reason: resourceAvailabilityData.reason ?? '',
        notes: resourceAvailabilityData.notes ?? '',
        versionNumber: resourceAvailabilityData.versionNumber?.toString() ?? '',
        active: resourceAvailabilityData.active ?? true,
        deleted: resourceAvailabilityData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceAvailabilityForm.markAsPristine();
    this.resourceAvailabilityForm.markAsUntouched();
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


  public userIsSchedulerResourceAvailabilityReader(): boolean {
    return this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityReader();
  }

  public userIsSchedulerResourceAvailabilityWriter(): boolean {
    return this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityWriter();
  }
}

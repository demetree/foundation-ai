/*
   GENERATED FORM FOR THE ACTIVITYEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ActivityEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to activity-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ActivityEventService, ActivityEventData, ActivityEventSubmitData } from '../../../bmc-data-services/activity-event.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ActivityEventTypeService } from '../../../bmc-data-services/activity-event-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ActivityEventFormValues {
  activityEventTypeId: number | bigint,       // For FK link number
  title: string,
  description: string | null,
  relatedEntityType: string | null,
  relatedEntityId: string | null,     // Stored as string for form input, converted to number on submit.
  eventDate: string,
  isPublic: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-activity-event-add-edit',
  templateUrl: './activity-event-add-edit.component.html',
  styleUrls: ['./activity-event-add-edit.component.scss']
})
export class ActivityEventAddEditComponent {
  @ViewChild('activityEventModal') activityEventModal!: TemplateRef<any>;
  @Output() activityEventChanged = new Subject<ActivityEventData[]>();
  @Input() activityEventSubmitData: ActivityEventSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ActivityEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public activityEventForm: FormGroup = this.fb.group({
        activityEventTypeId: [null, Validators.required],
        title: ['', Validators.required],
        description: [''],
        relatedEntityType: [''],
        relatedEntityId: [''],
        eventDate: ['', Validators.required],
        isPublic: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  activityEvents$ = this.activityEventService.GetActivityEventList();
  activityEventTypes$ = this.activityEventTypeService.GetActivityEventTypeList();

  constructor(
    private modalService: NgbModal,
    private activityEventService: ActivityEventService,
    private activityEventTypeService: ActivityEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(activityEventData?: ActivityEventData) {

    if (activityEventData != null) {

      if (!this.activityEventService.userIsBMCActivityEventReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Activity Events`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.activityEventSubmitData = this.activityEventService.ConvertToActivityEventSubmitData(activityEventData);
      this.isEditMode = true;
      this.objectGuid = activityEventData.objectGuid;

      this.buildFormValues(activityEventData);

    } else {

      if (!this.activityEventService.userIsBMCActivityEventWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Activity Events`,
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
        this.activityEventForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.activityEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.activityEventModal, {
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

    if (this.activityEventService.userIsBMCActivityEventWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Activity Events`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.activityEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.activityEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.activityEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const activityEventSubmitData: ActivityEventSubmitData = {
        id: this.activityEventSubmitData?.id || 0,
        activityEventTypeId: Number(formValue.activityEventTypeId),
        title: formValue.title!.trim(),
        description: formValue.description?.trim() || null,
        relatedEntityType: formValue.relatedEntityType?.trim() || null,
        relatedEntityId: formValue.relatedEntityId ? Number(formValue.relatedEntityId) : null,
        eventDate: dateTimeLocalToIsoUtc(formValue.eventDate!.trim())!,
        isPublic: !!formValue.isPublic,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateActivityEvent(activityEventSubmitData);
      } else {
        this.addActivityEvent(activityEventSubmitData);
      }
  }

  private addActivityEvent(activityEventData: ActivityEventSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    activityEventData.active = true;
    activityEventData.deleted = false;
    this.activityEventService.PostActivityEvent(activityEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newActivityEvent) => {

        this.activityEventService.ClearAllCaches();

        this.activityEventChanged.next([newActivityEvent]);

        this.alertService.showMessage("Activity Event added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/activityevent', newActivityEvent.id]);
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
                                   'You do not have permission to save this Activity Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Activity Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Activity Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateActivityEvent(activityEventData: ActivityEventSubmitData) {
    this.activityEventService.PutActivityEvent(activityEventData.id, activityEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedActivityEvent) => {

        this.activityEventService.ClearAllCaches();

        this.activityEventChanged.next([updatedActivityEvent]);

        this.alertService.showMessage("Activity Event updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Activity Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Activity Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Activity Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(activityEventData: ActivityEventData | null) {

    if (activityEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.activityEventForm.reset({
        activityEventTypeId: null,
        title: '',
        description: '',
        relatedEntityType: '',
        relatedEntityId: '',
        eventDate: '',
        isPublic: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.activityEventForm.reset({
        activityEventTypeId: activityEventData.activityEventTypeId,
        title: activityEventData.title ?? '',
        description: activityEventData.description ?? '',
        relatedEntityType: activityEventData.relatedEntityType ?? '',
        relatedEntityId: activityEventData.relatedEntityId?.toString() ?? '',
        eventDate: isoUtcStringToDateTimeLocal(activityEventData.eventDate) ?? '',
        isPublic: activityEventData.isPublic ?? false,
        active: activityEventData.active ?? true,
        deleted: activityEventData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.activityEventForm.markAsPristine();
    this.activityEventForm.markAsUntouched();
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


  public userIsBMCActivityEventReader(): boolean {
    return this.activityEventService.userIsBMCActivityEventReader();
  }

  public userIsBMCActivityEventWriter(): boolean {
    return this.activityEventService.userIsBMCActivityEventWriter();
  }
}

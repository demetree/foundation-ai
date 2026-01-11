/*
   GENERATED FORM FOR THE SCHEDULEDEVENTDEPENDENCY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduledEventDependency table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduled-event-dependency-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduledEventDependencyService, ScheduledEventDependencyData, ScheduledEventDependencySubmitData } from '../../../scheduler-data-services/scheduled-event-dependency.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { DependencyTypeService } from '../../../scheduler-data-services/dependency-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ScheduledEventDependencyFormValues {
  predecessorEventId: number | bigint,       // For FK link number
  successorEventId: number | bigint,       // For FK link number
  dependencyTypeId: number | bigint,       // For FK link number
  lagMinutes: string,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-scheduled-event-dependency-add-edit',
  templateUrl: './scheduled-event-dependency-add-edit.component.html',
  styleUrls: ['./scheduled-event-dependency-add-edit.component.scss']
})
export class ScheduledEventDependencyAddEditComponent {
  @ViewChild('scheduledEventDependencyModal') scheduledEventDependencyModal!: TemplateRef<any>;
  @Output() scheduledEventDependencyChanged = new Subject<ScheduledEventDependencyData[]>();
  @Input() scheduledEventDependencySubmitData: ScheduledEventDependencySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduledEventDependencyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduledEventDependencyForm: FormGroup = this.fb.group({
        predecessorEventId: [null, Validators.required],
        successorEventId: [null, Validators.required],
        dependencyTypeId: [null, Validators.required],
        lagMinutes: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  scheduledEventDependencies$ = this.scheduledEventDependencyService.GetScheduledEventDependencyList();
  scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  dependencyTypes$ = this.dependencyTypeService.GetDependencyTypeList();

  constructor(
    private modalService: NgbModal,
    private scheduledEventDependencyService: ScheduledEventDependencyService,
    private scheduledEventService: ScheduledEventService,
    private dependencyTypeService: DependencyTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(scheduledEventDependencyData?: ScheduledEventDependencyData) {

    if (scheduledEventDependencyData != null) {

      if (!this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduled Event Dependencies`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.scheduledEventDependencySubmitData = this.scheduledEventDependencyService.ConvertToScheduledEventDependencySubmitData(scheduledEventDependencyData);
      this.isEditMode = true;
      this.objectGuid = scheduledEventDependencyData.objectGuid;

      this.buildFormValues(scheduledEventDependencyData);

    } else {

      if (!this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Dependencies`,
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
        this.scheduledEventDependencyForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduledEventDependencyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.scheduledEventDependencyModal, {
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

    if (this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduled Event Dependencies`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.scheduledEventDependencyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduledEventDependencyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduledEventDependencyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduledEventDependencySubmitData: ScheduledEventDependencySubmitData = {
        id: this.scheduledEventDependencySubmitData?.id || 0,
        predecessorEventId: Number(formValue.predecessorEventId),
        successorEventId: Number(formValue.successorEventId),
        dependencyTypeId: Number(formValue.dependencyTypeId),
        lagMinutes: Number(formValue.lagMinutes),
        versionNumber: this.scheduledEventDependencySubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateScheduledEventDependency(scheduledEventDependencySubmitData);
      } else {
        this.addScheduledEventDependency(scheduledEventDependencySubmitData);
      }
  }

  private addScheduledEventDependency(scheduledEventDependencyData: ScheduledEventDependencySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    scheduledEventDependencyData.versionNumber = 0;
    scheduledEventDependencyData.active = true;
    scheduledEventDependencyData.deleted = false;
    this.scheduledEventDependencyService.PostScheduledEventDependency(scheduledEventDependencyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newScheduledEventDependency) => {

        this.scheduledEventDependencyService.ClearAllCaches();

        this.scheduledEventDependencyChanged.next([newScheduledEventDependency]);

        this.alertService.showMessage("Scheduled Event Dependency added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/scheduledeventdependency', newScheduledEventDependency.id]);
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
                                   'You do not have permission to save this Scheduled Event Dependency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Dependency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Dependency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateScheduledEventDependency(scheduledEventDependencyData: ScheduledEventDependencySubmitData) {
    this.scheduledEventDependencyService.PutScheduledEventDependency(scheduledEventDependencyData.id, scheduledEventDependencyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedScheduledEventDependency) => {

        this.scheduledEventDependencyService.ClearAllCaches();

        this.scheduledEventDependencyChanged.next([updatedScheduledEventDependency]);

        this.alertService.showMessage("Scheduled Event Dependency updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduled Event Dependency.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduled Event Dependency.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduled Event Dependency could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(scheduledEventDependencyData: ScheduledEventDependencyData | null) {

    if (scheduledEventDependencyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduledEventDependencyForm.reset({
        predecessorEventId: null,
        successorEventId: null,
        dependencyTypeId: null,
        lagMinutes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduledEventDependencyForm.reset({
        predecessorEventId: scheduledEventDependencyData.predecessorEventId,
        successorEventId: scheduledEventDependencyData.successorEventId,
        dependencyTypeId: scheduledEventDependencyData.dependencyTypeId,
        lagMinutes: scheduledEventDependencyData.lagMinutes?.toString() ?? '',
        versionNumber: scheduledEventDependencyData.versionNumber?.toString() ?? '',
        active: scheduledEventDependencyData.active ?? true,
        deleted: scheduledEventDependencyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduledEventDependencyForm.markAsPristine();
    this.scheduledEventDependencyForm.markAsUntouched();
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


  public userIsSchedulerScheduledEventDependencyReader(): boolean {
    return this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyReader();
  }

  public userIsSchedulerScheduledEventDependencyWriter(): boolean {
    return this.scheduledEventDependencyService.userIsSchedulerScheduledEventDependencyWriter();
  }
}

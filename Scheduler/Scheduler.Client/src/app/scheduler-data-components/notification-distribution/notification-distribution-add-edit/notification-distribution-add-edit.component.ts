/*
   GENERATED FORM FOR THE NOTIFICATIONDISTRIBUTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationDistribution table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-distribution-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationDistributionService, NotificationDistributionData, NotificationDistributionSubmitData } from '../../../scheduler-data-services/notification-distribution.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { NotificationService } from '../../../scheduler-data-services/notification.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface NotificationDistributionFormValues {
  notificationId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  acknowledged: boolean,
  dateTimeAcknowledged: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-notification-distribution-add-edit',
  templateUrl: './notification-distribution-add-edit.component.html',
  styleUrls: ['./notification-distribution-add-edit.component.scss']
})
export class NotificationDistributionAddEditComponent {
  @ViewChild('notificationDistributionModal') notificationDistributionModal!: TemplateRef<any>;
  @Output() notificationDistributionChanged = new Subject<NotificationDistributionData[]>();
  @Input() notificationDistributionSubmitData: NotificationDistributionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationDistributionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationDistributionForm: FormGroup = this.fb.group({
        notificationId: [null, Validators.required],
        userId: ['', Validators.required],
        acknowledged: [false],
        dateTimeAcknowledged: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  notificationDistributions$ = this.notificationDistributionService.GetNotificationDistributionList();
  notifications$ = this.notificationService.GetNotificationList();

  constructor(
    private modalService: NgbModal,
    private notificationDistributionService: NotificationDistributionService,
    private notificationService: NotificationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(notificationDistributionData?: NotificationDistributionData) {

    if (notificationDistributionData != null) {

      if (!this.notificationDistributionService.userIsSchedulerNotificationDistributionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Notification Distributions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.notificationDistributionSubmitData = this.notificationDistributionService.ConvertToNotificationDistributionSubmitData(notificationDistributionData);
      this.isEditMode = true;
      this.objectGuid = notificationDistributionData.objectGuid;

      this.buildFormValues(notificationDistributionData);

    } else {

      if (!this.notificationDistributionService.userIsSchedulerNotificationDistributionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Notification Distributions`,
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
        this.notificationDistributionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationDistributionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.notificationDistributionModal, {
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

    if (this.notificationDistributionService.userIsSchedulerNotificationDistributionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Notification Distributions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.notificationDistributionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationDistributionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationDistributionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationDistributionSubmitData: NotificationDistributionSubmitData = {
        id: this.notificationDistributionSubmitData?.id || 0,
        notificationId: Number(formValue.notificationId),
        userId: Number(formValue.userId),
        acknowledged: !!formValue.acknowledged,
        dateTimeAcknowledged: formValue.dateTimeAcknowledged ? dateTimeLocalToIsoUtc(formValue.dateTimeAcknowledged.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateNotificationDistribution(notificationDistributionSubmitData);
      } else {
        this.addNotificationDistribution(notificationDistributionSubmitData);
      }
  }

  private addNotificationDistribution(notificationDistributionData: NotificationDistributionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    notificationDistributionData.active = true;
    notificationDistributionData.deleted = false;
    this.notificationDistributionService.PostNotificationDistribution(notificationDistributionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newNotificationDistribution) => {

        this.notificationDistributionService.ClearAllCaches();

        this.notificationDistributionChanged.next([newNotificationDistribution]);

        this.alertService.showMessage("Notification Distribution added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/notificationdistribution', newNotificationDistribution.id]);
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
                                   'You do not have permission to save this Notification Distribution.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Distribution.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Distribution could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateNotificationDistribution(notificationDistributionData: NotificationDistributionSubmitData) {
    this.notificationDistributionService.PutNotificationDistribution(notificationDistributionData.id, notificationDistributionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedNotificationDistribution) => {

        this.notificationDistributionService.ClearAllCaches();

        this.notificationDistributionChanged.next([updatedNotificationDistribution]);

        this.alertService.showMessage("Notification Distribution updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Notification Distribution.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Distribution.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Distribution could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(notificationDistributionData: NotificationDistributionData | null) {

    if (notificationDistributionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationDistributionForm.reset({
        notificationId: null,
        userId: '',
        acknowledged: false,
        dateTimeAcknowledged: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.notificationDistributionForm.reset({
        notificationId: notificationDistributionData.notificationId,
        userId: notificationDistributionData.userId?.toString() ?? '',
        acknowledged: notificationDistributionData.acknowledged ?? false,
        dateTimeAcknowledged: isoUtcStringToDateTimeLocal(notificationDistributionData.dateTimeAcknowledged) ?? '',
        active: notificationDistributionData.active ?? true,
        deleted: notificationDistributionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.notificationDistributionForm.markAsPristine();
    this.notificationDistributionForm.markAsUntouched();
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


  public userIsSchedulerNotificationDistributionReader(): boolean {
    return this.notificationDistributionService.userIsSchedulerNotificationDistributionReader();
  }

  public userIsSchedulerNotificationDistributionWriter(): boolean {
    return this.notificationDistributionService.userIsSchedulerNotificationDistributionWriter();
  }
}

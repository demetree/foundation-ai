/*
   GENERATED FORM FOR THE USERNOTIFICATIONCHANNELPREFERENCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserNotificationChannelPreference table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-notification-channel-preference-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserNotificationChannelPreferenceService, UserNotificationChannelPreferenceData, UserNotificationChannelPreferenceSubmitData } from '../../../alerting-data-services/user-notification-channel-preference.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserNotificationPreferenceService } from '../../../alerting-data-services/user-notification-preference.service';
import { NotificationChannelTypeService } from '../../../alerting-data-services/notification-channel-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserNotificationChannelPreferenceFormValues {
  userNotificationPreferenceId: number | bigint,       // For FK link number
  notificationChannelTypeId: number | bigint,       // For FK link number
  isEnabled: boolean,
  priorityOverride: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-notification-channel-preference-add-edit',
  templateUrl: './user-notification-channel-preference-add-edit.component.html',
  styleUrls: ['./user-notification-channel-preference-add-edit.component.scss']
})
export class UserNotificationChannelPreferenceAddEditComponent {
  @ViewChild('userNotificationChannelPreferenceModal') userNotificationChannelPreferenceModal!: TemplateRef<any>;
  @Output() userNotificationChannelPreferenceChanged = new Subject<UserNotificationChannelPreferenceData[]>();
  @Input() userNotificationChannelPreferenceSubmitData: UserNotificationChannelPreferenceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserNotificationChannelPreferenceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userNotificationChannelPreferenceForm: FormGroup = this.fb.group({
        userNotificationPreferenceId: [null, Validators.required],
        notificationChannelTypeId: [null, Validators.required],
        isEnabled: [false],
        priorityOverride: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userNotificationChannelPreferences$ = this.userNotificationChannelPreferenceService.GetUserNotificationChannelPreferenceList();
  userNotificationPreferences$ = this.userNotificationPreferenceService.GetUserNotificationPreferenceList();
  notificationChannelTypes$ = this.notificationChannelTypeService.GetNotificationChannelTypeList();

  constructor(
    private modalService: NgbModal,
    private userNotificationChannelPreferenceService: UserNotificationChannelPreferenceService,
    private userNotificationPreferenceService: UserNotificationPreferenceService,
    private notificationChannelTypeService: NotificationChannelTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userNotificationChannelPreferenceData?: UserNotificationChannelPreferenceData) {

    if (userNotificationChannelPreferenceData != null) {

      if (!this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Notification Channel Preferences`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userNotificationChannelPreferenceSubmitData = this.userNotificationChannelPreferenceService.ConvertToUserNotificationChannelPreferenceSubmitData(userNotificationChannelPreferenceData);
      this.isEditMode = true;
      this.objectGuid = userNotificationChannelPreferenceData.objectGuid;

      this.buildFormValues(userNotificationChannelPreferenceData);

    } else {

      if (!this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Notification Channel Preferences`,
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
        this.userNotificationChannelPreferenceForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userNotificationChannelPreferenceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userNotificationChannelPreferenceModal, {
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

    if (this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Notification Channel Preferences`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userNotificationChannelPreferenceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userNotificationChannelPreferenceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userNotificationChannelPreferenceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userNotificationChannelPreferenceSubmitData: UserNotificationChannelPreferenceSubmitData = {
        id: this.userNotificationChannelPreferenceSubmitData?.id || 0,
        userNotificationPreferenceId: Number(formValue.userNotificationPreferenceId),
        notificationChannelTypeId: Number(formValue.notificationChannelTypeId),
        isEnabled: !!formValue.isEnabled,
        priorityOverride: formValue.priorityOverride ? Number(formValue.priorityOverride) : null,
        versionNumber: this.userNotificationChannelPreferenceSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserNotificationChannelPreference(userNotificationChannelPreferenceSubmitData);
      } else {
        this.addUserNotificationChannelPreference(userNotificationChannelPreferenceSubmitData);
      }
  }

  private addUserNotificationChannelPreference(userNotificationChannelPreferenceData: UserNotificationChannelPreferenceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userNotificationChannelPreferenceData.versionNumber = 0;
    userNotificationChannelPreferenceData.active = true;
    userNotificationChannelPreferenceData.deleted = false;
    this.userNotificationChannelPreferenceService.PostUserNotificationChannelPreference(userNotificationChannelPreferenceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserNotificationChannelPreference) => {

        this.userNotificationChannelPreferenceService.ClearAllCaches();

        this.userNotificationChannelPreferenceChanged.next([newUserNotificationChannelPreference]);

        this.alertService.showMessage("User Notification Channel Preference added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usernotificationchannelpreference', newUserNotificationChannelPreference.id]);
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
                                   'You do not have permission to save this User Notification Channel Preference.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Notification Channel Preference.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Notification Channel Preference could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserNotificationChannelPreference(userNotificationChannelPreferenceData: UserNotificationChannelPreferenceSubmitData) {
    this.userNotificationChannelPreferenceService.PutUserNotificationChannelPreference(userNotificationChannelPreferenceData.id, userNotificationChannelPreferenceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserNotificationChannelPreference) => {

        this.userNotificationChannelPreferenceService.ClearAllCaches();

        this.userNotificationChannelPreferenceChanged.next([updatedUserNotificationChannelPreference]);

        this.alertService.showMessage("User Notification Channel Preference updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Notification Channel Preference.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Notification Channel Preference.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Notification Channel Preference could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userNotificationChannelPreferenceData: UserNotificationChannelPreferenceData | null) {

    if (userNotificationChannelPreferenceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userNotificationChannelPreferenceForm.reset({
        userNotificationPreferenceId: null,
        notificationChannelTypeId: null,
        isEnabled: false,
        priorityOverride: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userNotificationChannelPreferenceForm.reset({
        userNotificationPreferenceId: userNotificationChannelPreferenceData.userNotificationPreferenceId,
        notificationChannelTypeId: userNotificationChannelPreferenceData.notificationChannelTypeId,
        isEnabled: userNotificationChannelPreferenceData.isEnabled ?? false,
        priorityOverride: userNotificationChannelPreferenceData.priorityOverride?.toString() ?? '',
        versionNumber: userNotificationChannelPreferenceData.versionNumber?.toString() ?? '',
        active: userNotificationChannelPreferenceData.active ?? true,
        deleted: userNotificationChannelPreferenceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userNotificationChannelPreferenceForm.markAsPristine();
    this.userNotificationChannelPreferenceForm.markAsUntouched();
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


  public userIsAlertingUserNotificationChannelPreferenceReader(): boolean {
    return this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceReader();
  }

  public userIsAlertingUserNotificationChannelPreferenceWriter(): boolean {
    return this.userNotificationChannelPreferenceService.userIsAlertingUserNotificationChannelPreferenceWriter();
  }
}

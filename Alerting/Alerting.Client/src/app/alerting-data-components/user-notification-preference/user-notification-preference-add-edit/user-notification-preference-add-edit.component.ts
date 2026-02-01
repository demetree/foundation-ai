/*
   GENERATED FORM FOR THE USERNOTIFICATIONPREFERENCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserNotificationPreference table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-notification-preference-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserNotificationPreferenceService, UserNotificationPreferenceData, UserNotificationPreferenceSubmitData } from '../../../alerting-data-services/user-notification-preference.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserNotificationPreferenceFormValues {
  securityUserObjectGuid: string,
  timeZoneId: string | null,
  quietHoursStart: string | null,
  quietHoursEnd: string | null,
  isDoNotDisturb: boolean,
  isDoNotDisturbPermanent: boolean,
  doNotDisturbUntil: string | null,
  customSettingsJson: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-notification-preference-add-edit',
  templateUrl: './user-notification-preference-add-edit.component.html',
  styleUrls: ['./user-notification-preference-add-edit.component.scss']
})
export class UserNotificationPreferenceAddEditComponent {
  @ViewChild('userNotificationPreferenceModal') userNotificationPreferenceModal!: TemplateRef<any>;
  @Output() userNotificationPreferenceChanged = new Subject<UserNotificationPreferenceData[]>();
  @Input() userNotificationPreferenceSubmitData: UserNotificationPreferenceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserNotificationPreferenceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userNotificationPreferenceForm: FormGroup = this.fb.group({
        securityUserObjectGuid: ['', Validators.required],
        timeZoneId: [''],
        quietHoursStart: [''],
        quietHoursEnd: [''],
        isDoNotDisturb: [false],
        isDoNotDisturbPermanent: [false],
        doNotDisturbUntil: [''],
        customSettingsJson: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userNotificationPreferences$ = this.userNotificationPreferenceService.GetUserNotificationPreferenceList();

  constructor(
    private modalService: NgbModal,
    private userNotificationPreferenceService: UserNotificationPreferenceService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userNotificationPreferenceData?: UserNotificationPreferenceData) {

    if (userNotificationPreferenceData != null) {

      if (!this.userNotificationPreferenceService.userIsAlertingUserNotificationPreferenceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Notification Preferences`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userNotificationPreferenceSubmitData = this.userNotificationPreferenceService.ConvertToUserNotificationPreferenceSubmitData(userNotificationPreferenceData);
      this.isEditMode = true;
      this.objectGuid = userNotificationPreferenceData.objectGuid;

      this.buildFormValues(userNotificationPreferenceData);

    } else {

      if (!this.userNotificationPreferenceService.userIsAlertingUserNotificationPreferenceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Notification Preferences`,
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
        this.userNotificationPreferenceForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userNotificationPreferenceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userNotificationPreferenceModal, {
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

    if (this.userNotificationPreferenceService.userIsAlertingUserNotificationPreferenceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Notification Preferences`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userNotificationPreferenceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userNotificationPreferenceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userNotificationPreferenceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userNotificationPreferenceSubmitData: UserNotificationPreferenceSubmitData = {
        id: this.userNotificationPreferenceSubmitData?.id || 0,
        securityUserObjectGuid: formValue.securityUserObjectGuid!.trim(),
        timeZoneId: formValue.timeZoneId?.trim() || null,
        quietHoursStart: formValue.quietHoursStart?.trim() || null,
        quietHoursEnd: formValue.quietHoursEnd?.trim() || null,
        isDoNotDisturb: !!formValue.isDoNotDisturb,
        isDoNotDisturbPermanent: !!formValue.isDoNotDisturbPermanent,
        doNotDisturbUntil: formValue.doNotDisturbUntil ? dateTimeLocalToIsoUtc(formValue.doNotDisturbUntil.trim()) : null,
        customSettingsJson: formValue.customSettingsJson?.trim() || null,
        versionNumber: this.userNotificationPreferenceSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserNotificationPreference(userNotificationPreferenceSubmitData);
      } else {
        this.addUserNotificationPreference(userNotificationPreferenceSubmitData);
      }
  }

  private addUserNotificationPreference(userNotificationPreferenceData: UserNotificationPreferenceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userNotificationPreferenceData.versionNumber = 0;
    userNotificationPreferenceData.active = true;
    userNotificationPreferenceData.deleted = false;
    this.userNotificationPreferenceService.PostUserNotificationPreference(userNotificationPreferenceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserNotificationPreference) => {

        this.userNotificationPreferenceService.ClearAllCaches();

        this.userNotificationPreferenceChanged.next([newUserNotificationPreference]);

        this.alertService.showMessage("User Notification Preference added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usernotificationpreference', newUserNotificationPreference.id]);
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
                                   'You do not have permission to save this User Notification Preference.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Notification Preference.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Notification Preference could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserNotificationPreference(userNotificationPreferenceData: UserNotificationPreferenceSubmitData) {
    this.userNotificationPreferenceService.PutUserNotificationPreference(userNotificationPreferenceData.id, userNotificationPreferenceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserNotificationPreference) => {

        this.userNotificationPreferenceService.ClearAllCaches();

        this.userNotificationPreferenceChanged.next([updatedUserNotificationPreference]);

        this.alertService.showMessage("User Notification Preference updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Notification Preference.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Notification Preference.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Notification Preference could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userNotificationPreferenceData: UserNotificationPreferenceData | null) {

    if (userNotificationPreferenceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userNotificationPreferenceForm.reset({
        securityUserObjectGuid: '',
        timeZoneId: '',
        quietHoursStart: '',
        quietHoursEnd: '',
        isDoNotDisturb: false,
        isDoNotDisturbPermanent: false,
        doNotDisturbUntil: '',
        customSettingsJson: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userNotificationPreferenceForm.reset({
        securityUserObjectGuid: userNotificationPreferenceData.securityUserObjectGuid ?? '',
        timeZoneId: userNotificationPreferenceData.timeZoneId ?? '',
        quietHoursStart: userNotificationPreferenceData.quietHoursStart ?? '',
        quietHoursEnd: userNotificationPreferenceData.quietHoursEnd ?? '',
        isDoNotDisturb: userNotificationPreferenceData.isDoNotDisturb ?? false,
        isDoNotDisturbPermanent: userNotificationPreferenceData.isDoNotDisturbPermanent ?? false,
        doNotDisturbUntil: isoUtcStringToDateTimeLocal(userNotificationPreferenceData.doNotDisturbUntil) ?? '',
        customSettingsJson: userNotificationPreferenceData.customSettingsJson ?? '',
        versionNumber: userNotificationPreferenceData.versionNumber?.toString() ?? '',
        active: userNotificationPreferenceData.active ?? true,
        deleted: userNotificationPreferenceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userNotificationPreferenceForm.markAsPristine();
    this.userNotificationPreferenceForm.markAsUntouched();
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


  public userIsAlertingUserNotificationPreferenceReader(): boolean {
    return this.userNotificationPreferenceService.userIsAlertingUserNotificationPreferenceReader();
  }

  public userIsAlertingUserNotificationPreferenceWriter(): boolean {
    return this.userNotificationPreferenceService.userIsAlertingUserNotificationPreferenceWriter();
  }
}

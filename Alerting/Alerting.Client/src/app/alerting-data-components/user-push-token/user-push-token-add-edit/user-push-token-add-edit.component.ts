/*
   GENERATED FORM FOR THE USERPUSHTOKEN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserPushToken table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-push-token-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserPushTokenService, UserPushTokenData, UserPushTokenSubmitData } from '../../../alerting-data-services/user-push-token.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserPushTokenFormValues {
  userObjectGuid: string,
  fcmToken: string,
  deviceFingerprint: string,
  platform: string,
  userAgent: string | null,
  registeredAt: string,
  lastUpdatedAt: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-push-token-add-edit',
  templateUrl: './user-push-token-add-edit.component.html',
  styleUrls: ['./user-push-token-add-edit.component.scss']
})
export class UserPushTokenAddEditComponent {
  @ViewChild('userPushTokenModal') userPushTokenModal!: TemplateRef<any>;
  @Output() userPushTokenChanged = new Subject<UserPushTokenData[]>();
  @Input() userPushTokenSubmitData: UserPushTokenSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserPushTokenFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userPushTokenForm: FormGroup = this.fb.group({
        userObjectGuid: ['', Validators.required],
        fcmToken: ['', Validators.required],
        deviceFingerprint: ['', Validators.required],
        platform: ['', Validators.required],
        userAgent: [''],
        registeredAt: ['', Validators.required],
        lastUpdatedAt: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userPushTokens$ = this.userPushTokenService.GetUserPushTokenList();

  constructor(
    private modalService: NgbModal,
    private userPushTokenService: UserPushTokenService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userPushTokenData?: UserPushTokenData) {

    if (userPushTokenData != null) {

      if (!this.userPushTokenService.userIsAlertingUserPushTokenReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Push Tokens`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userPushTokenSubmitData = this.userPushTokenService.ConvertToUserPushTokenSubmitData(userPushTokenData);
      this.isEditMode = true;
      this.objectGuid = userPushTokenData.objectGuid;

      this.buildFormValues(userPushTokenData);

    } else {

      if (!this.userPushTokenService.userIsAlertingUserPushTokenWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Push Tokens`,
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
        this.userPushTokenForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userPushTokenForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userPushTokenModal, {
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

    if (this.userPushTokenService.userIsAlertingUserPushTokenWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Push Tokens`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userPushTokenForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userPushTokenForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userPushTokenForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userPushTokenSubmitData: UserPushTokenSubmitData = {
        id: this.userPushTokenSubmitData?.id || 0,
        userObjectGuid: formValue.userObjectGuid!.trim(),
        fcmToken: formValue.fcmToken!.trim(),
        deviceFingerprint: formValue.deviceFingerprint!.trim(),
        platform: formValue.platform!.trim(),
        userAgent: formValue.userAgent?.trim() || null,
        registeredAt: dateTimeLocalToIsoUtc(formValue.registeredAt!.trim())!,
        lastUpdatedAt: dateTimeLocalToIsoUtc(formValue.lastUpdatedAt!.trim())!,
        versionNumber: this.userPushTokenSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserPushToken(userPushTokenSubmitData);
      } else {
        this.addUserPushToken(userPushTokenSubmitData);
      }
  }

  private addUserPushToken(userPushTokenData: UserPushTokenSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userPushTokenData.versionNumber = 0;
    userPushTokenData.active = true;
    userPushTokenData.deleted = false;
    this.userPushTokenService.PostUserPushToken(userPushTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserPushToken) => {

        this.userPushTokenService.ClearAllCaches();

        this.userPushTokenChanged.next([newUserPushToken]);

        this.alertService.showMessage("User Push Token added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userpushtoken', newUserPushToken.id]);
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
                                   'You do not have permission to save this User Push Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Push Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Push Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserPushToken(userPushTokenData: UserPushTokenSubmitData) {
    this.userPushTokenService.PutUserPushToken(userPushTokenData.id, userPushTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserPushToken) => {

        this.userPushTokenService.ClearAllCaches();

        this.userPushTokenChanged.next([updatedUserPushToken]);

        this.alertService.showMessage("User Push Token updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Push Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Push Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Push Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userPushTokenData: UserPushTokenData | null) {

    if (userPushTokenData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userPushTokenForm.reset({
        userObjectGuid: '',
        fcmToken: '',
        deviceFingerprint: '',
        platform: '',
        userAgent: '',
        registeredAt: '',
        lastUpdatedAt: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userPushTokenForm.reset({
        userObjectGuid: userPushTokenData.userObjectGuid ?? '',
        fcmToken: userPushTokenData.fcmToken ?? '',
        deviceFingerprint: userPushTokenData.deviceFingerprint ?? '',
        platform: userPushTokenData.platform ?? '',
        userAgent: userPushTokenData.userAgent ?? '',
        registeredAt: isoUtcStringToDateTimeLocal(userPushTokenData.registeredAt) ?? '',
        lastUpdatedAt: isoUtcStringToDateTimeLocal(userPushTokenData.lastUpdatedAt) ?? '',
        versionNumber: userPushTokenData.versionNumber?.toString() ?? '',
        active: userPushTokenData.active ?? true,
        deleted: userPushTokenData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userPushTokenForm.markAsPristine();
    this.userPushTokenForm.markAsUntouched();
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


  public userIsAlertingUserPushTokenReader(): boolean {
    return this.userPushTokenService.userIsAlertingUserPushTokenReader();
  }

  public userIsAlertingUserPushTokenWriter(): boolean {
    return this.userPushTokenService.userIsAlertingUserPushTokenWriter();
  }
}

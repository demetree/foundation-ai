/*
   GENERATED FORM FOR THE USERPROFILECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileChangeHistoryService, UserProfileChangeHistoryData, UserProfileChangeHistorySubmitData } from '../../../bmc-data-services/user-profile-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserProfileService } from '../../../bmc-data-services/user-profile.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserProfileChangeHistoryFormValues {
  userProfileId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-user-profile-change-history-add-edit',
  templateUrl: './user-profile-change-history-add-edit.component.html',
  styleUrls: ['./user-profile-change-history-add-edit.component.scss']
})
export class UserProfileChangeHistoryAddEditComponent {
  @ViewChild('userProfileChangeHistoryModal') userProfileChangeHistoryModal!: TemplateRef<any>;
  @Output() userProfileChangeHistoryChanged = new Subject<UserProfileChangeHistoryData[]>();
  @Input() userProfileChangeHistorySubmitData: UserProfileChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileChangeHistoryForm: FormGroup = this.fb.group({
        userProfileId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userProfileChangeHistories$ = this.userProfileChangeHistoryService.GetUserProfileChangeHistoryList();
  userProfiles$ = this.userProfileService.GetUserProfileList();

  constructor(
    private modalService: NgbModal,
    private userProfileChangeHistoryService: UserProfileChangeHistoryService,
    private userProfileService: UserProfileService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userProfileChangeHistoryData?: UserProfileChangeHistoryData) {

    if (userProfileChangeHistoryData != null) {

      if (!this.userProfileChangeHistoryService.userIsBMCUserProfileChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Profile Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userProfileChangeHistorySubmitData = this.userProfileChangeHistoryService.ConvertToUserProfileChangeHistorySubmitData(userProfileChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(userProfileChangeHistoryData);

    } else {

      if (!this.userProfileChangeHistoryService.userIsBMCUserProfileChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Profile Change Histories`,
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
        this.userProfileChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userProfileChangeHistoryModal, {
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

    if (this.userProfileChangeHistoryService.userIsBMCUserProfileChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Profile Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userProfileChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileChangeHistorySubmitData: UserProfileChangeHistorySubmitData = {
        id: this.userProfileChangeHistorySubmitData?.id || 0,
        userProfileId: Number(formValue.userProfileId),
        versionNumber: this.userProfileChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateUserProfileChangeHistory(userProfileChangeHistorySubmitData);
      } else {
        this.addUserProfileChangeHistory(userProfileChangeHistorySubmitData);
      }
  }

  private addUserProfileChangeHistory(userProfileChangeHistoryData: UserProfileChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userProfileChangeHistoryData.versionNumber = 0;
    this.userProfileChangeHistoryService.PostUserProfileChangeHistory(userProfileChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserProfileChangeHistory) => {

        this.userProfileChangeHistoryService.ClearAllCaches();

        this.userProfileChangeHistoryChanged.next([newUserProfileChangeHistory]);

        this.alertService.showMessage("User Profile Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userprofilechangehistory', newUserProfileChangeHistory.id]);
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
                                   'You do not have permission to save this User Profile Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserProfileChangeHistory(userProfileChangeHistoryData: UserProfileChangeHistorySubmitData) {
    this.userProfileChangeHistoryService.PutUserProfileChangeHistory(userProfileChangeHistoryData.id, userProfileChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserProfileChangeHistory) => {

        this.userProfileChangeHistoryService.ClearAllCaches();

        this.userProfileChangeHistoryChanged.next([updatedUserProfileChangeHistory]);

        this.alertService.showMessage("User Profile Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Profile Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userProfileChangeHistoryData: UserProfileChangeHistoryData | null) {

    if (userProfileChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileChangeHistoryForm.reset({
        userProfileId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileChangeHistoryForm.reset({
        userProfileId: userProfileChangeHistoryData.userProfileId,
        versionNumber: userProfileChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(userProfileChangeHistoryData.timeStamp) ?? '',
        userId: userProfileChangeHistoryData.userId?.toString() ?? '',
        data: userProfileChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.userProfileChangeHistoryForm.markAsPristine();
    this.userProfileChangeHistoryForm.markAsUntouched();
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


  public userIsBMCUserProfileChangeHistoryReader(): boolean {
    return this.userProfileChangeHistoryService.userIsBMCUserProfileChangeHistoryReader();
  }

  public userIsBMCUserProfileChangeHistoryWriter(): boolean {
    return this.userProfileChangeHistoryService.userIsBMCUserProfileChangeHistoryWriter();
  }
}

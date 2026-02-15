/*
   GENERATED FORM FOR THE USERPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileService, UserProfileData, UserProfileSubmitData } from '../../../bmc-data-services/user-profile.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserProfileFormValues {
  displayName: string,
  bio: string | null,
  location: string | null,
  avatarImagePath: string | null,
  profileBannerImagePath: string | null,
  websiteUrl: string | null,
  isPublic: boolean,
  memberSinceDate: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-profile-add-edit',
  templateUrl: './user-profile-add-edit.component.html',
  styleUrls: ['./user-profile-add-edit.component.scss']
})
export class UserProfileAddEditComponent {
  @ViewChild('userProfileModal') userProfileModal!: TemplateRef<any>;
  @Output() userProfileChanged = new Subject<UserProfileData[]>();
  @Input() userProfileSubmitData: UserProfileSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileForm: FormGroup = this.fb.group({
        displayName: ['', Validators.required],
        bio: [''],
        location: [''],
        avatarImagePath: [''],
        profileBannerImagePath: [''],
        websiteUrl: [''],
        isPublic: [false],
        memberSinceDate: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userProfiles$ = this.userProfileService.GetUserProfileList();

  constructor(
    private modalService: NgbModal,
    private userProfileService: UserProfileService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userProfileData?: UserProfileData) {

    if (userProfileData != null) {

      if (!this.userProfileService.userIsBMCUserProfileReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Profiles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userProfileSubmitData = this.userProfileService.ConvertToUserProfileSubmitData(userProfileData);
      this.isEditMode = true;
      this.objectGuid = userProfileData.objectGuid;

      this.buildFormValues(userProfileData);

    } else {

      if (!this.userProfileService.userIsBMCUserProfileWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Profiles`,
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
        this.userProfileForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userProfileModal, {
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

    if (this.userProfileService.userIsBMCUserProfileWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Profiles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userProfileForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileSubmitData: UserProfileSubmitData = {
        id: this.userProfileSubmitData?.id || 0,
        displayName: formValue.displayName!.trim(),
        bio: formValue.bio?.trim() || null,
        location: formValue.location?.trim() || null,
        avatarImagePath: formValue.avatarImagePath?.trim() || null,
        profileBannerImagePath: formValue.profileBannerImagePath?.trim() || null,
        websiteUrl: formValue.websiteUrl?.trim() || null,
        isPublic: !!formValue.isPublic,
        memberSinceDate: formValue.memberSinceDate ? dateTimeLocalToIsoUtc(formValue.memberSinceDate.trim()) : null,
        versionNumber: this.userProfileSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserProfile(userProfileSubmitData);
      } else {
        this.addUserProfile(userProfileSubmitData);
      }
  }

  private addUserProfile(userProfileData: UserProfileSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userProfileData.versionNumber = 0;
    userProfileData.active = true;
    userProfileData.deleted = false;
    this.userProfileService.PostUserProfile(userProfileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserProfile) => {

        this.userProfileService.ClearAllCaches();

        this.userProfileChanged.next([newUserProfile]);

        this.alertService.showMessage("User Profile added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userprofile', newUserProfile.id]);
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
                                   'You do not have permission to save this User Profile.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserProfile(userProfileData: UserProfileSubmitData) {
    this.userProfileService.PutUserProfile(userProfileData.id, userProfileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserProfile) => {

        this.userProfileService.ClearAllCaches();

        this.userProfileChanged.next([updatedUserProfile]);

        this.alertService.showMessage("User Profile updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Profile.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userProfileData: UserProfileData | null) {

    if (userProfileData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileForm.reset({
        displayName: '',
        bio: '',
        location: '',
        avatarImagePath: '',
        profileBannerImagePath: '',
        websiteUrl: '',
        isPublic: false,
        memberSinceDate: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileForm.reset({
        displayName: userProfileData.displayName ?? '',
        bio: userProfileData.bio ?? '',
        location: userProfileData.location ?? '',
        avatarImagePath: userProfileData.avatarImagePath ?? '',
        profileBannerImagePath: userProfileData.profileBannerImagePath ?? '',
        websiteUrl: userProfileData.websiteUrl ?? '',
        isPublic: userProfileData.isPublic ?? false,
        memberSinceDate: isoUtcStringToDateTimeLocal(userProfileData.memberSinceDate) ?? '',
        versionNumber: userProfileData.versionNumber?.toString() ?? '',
        active: userProfileData.active ?? true,
        deleted: userProfileData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileForm.markAsPristine();
    this.userProfileForm.markAsUntouched();
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


  public userIsBMCUserProfileReader(): boolean {
    return this.userProfileService.userIsBMCUserProfileReader();
  }

  public userIsBMCUserProfileWriter(): boolean {
    return this.userProfileService.userIsBMCUserProfileWriter();
  }
}

/*
   GENERATED FORM FOR THE USERPROFILEPREFERREDTHEME TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfilePreferredTheme table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-preferred-theme-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfilePreferredThemeService, UserProfilePreferredThemeData, UserProfilePreferredThemeSubmitData } from '../../../bmc-data-services/user-profile-preferred-theme.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserProfileService } from '../../../bmc-data-services/user-profile.service';
import { LegoThemeService } from '../../../bmc-data-services/lego-theme.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserProfilePreferredThemeFormValues {
  userProfileId: number | bigint,       // For FK link number
  legoThemeId: number | bigint,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-profile-preferred-theme-add-edit',
  templateUrl: './user-profile-preferred-theme-add-edit.component.html',
  styleUrls: ['./user-profile-preferred-theme-add-edit.component.scss']
})
export class UserProfilePreferredThemeAddEditComponent {
  @ViewChild('userProfilePreferredThemeModal') userProfilePreferredThemeModal!: TemplateRef<any>;
  @Output() userProfilePreferredThemeChanged = new Subject<UserProfilePreferredThemeData[]>();
  @Input() userProfilePreferredThemeSubmitData: UserProfilePreferredThemeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfilePreferredThemeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfilePreferredThemeForm: FormGroup = this.fb.group({
        userProfileId: [null, Validators.required],
        legoThemeId: [null, Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userProfilePreferredThemes$ = this.userProfilePreferredThemeService.GetUserProfilePreferredThemeList();
  userProfiles$ = this.userProfileService.GetUserProfileList();
  legoThemes$ = this.legoThemeService.GetLegoThemeList();

  constructor(
    private modalService: NgbModal,
    private userProfilePreferredThemeService: UserProfilePreferredThemeService,
    private userProfileService: UserProfileService,
    private legoThemeService: LegoThemeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userProfilePreferredThemeData?: UserProfilePreferredThemeData) {

    if (userProfilePreferredThemeData != null) {

      if (!this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Profile Preferred Themes`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userProfilePreferredThemeSubmitData = this.userProfilePreferredThemeService.ConvertToUserProfilePreferredThemeSubmitData(userProfilePreferredThemeData);
      this.isEditMode = true;
      this.objectGuid = userProfilePreferredThemeData.objectGuid;

      this.buildFormValues(userProfilePreferredThemeData);

    } else {

      if (!this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Profile Preferred Themes`,
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
        this.userProfilePreferredThemeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfilePreferredThemeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userProfilePreferredThemeModal, {
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

    if (this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Profile Preferred Themes`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userProfilePreferredThemeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfilePreferredThemeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfilePreferredThemeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfilePreferredThemeSubmitData: UserProfilePreferredThemeSubmitData = {
        id: this.userProfilePreferredThemeSubmitData?.id || 0,
        userProfileId: Number(formValue.userProfileId),
        legoThemeId: Number(formValue.legoThemeId),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserProfilePreferredTheme(userProfilePreferredThemeSubmitData);
      } else {
        this.addUserProfilePreferredTheme(userProfilePreferredThemeSubmitData);
      }
  }

  private addUserProfilePreferredTheme(userProfilePreferredThemeData: UserProfilePreferredThemeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userProfilePreferredThemeData.active = true;
    userProfilePreferredThemeData.deleted = false;
    this.userProfilePreferredThemeService.PostUserProfilePreferredTheme(userProfilePreferredThemeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserProfilePreferredTheme) => {

        this.userProfilePreferredThemeService.ClearAllCaches();

        this.userProfilePreferredThemeChanged.next([newUserProfilePreferredTheme]);

        this.alertService.showMessage("User Profile Preferred Theme added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userprofilepreferredtheme', newUserProfilePreferredTheme.id]);
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
                                   'You do not have permission to save this User Profile Preferred Theme.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Preferred Theme.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Preferred Theme could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserProfilePreferredTheme(userProfilePreferredThemeData: UserProfilePreferredThemeSubmitData) {
    this.userProfilePreferredThemeService.PutUserProfilePreferredTheme(userProfilePreferredThemeData.id, userProfilePreferredThemeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserProfilePreferredTheme) => {

        this.userProfilePreferredThemeService.ClearAllCaches();

        this.userProfilePreferredThemeChanged.next([updatedUserProfilePreferredTheme]);

        this.alertService.showMessage("User Profile Preferred Theme updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Profile Preferred Theme.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Preferred Theme.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Preferred Theme could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userProfilePreferredThemeData: UserProfilePreferredThemeData | null) {

    if (userProfilePreferredThemeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfilePreferredThemeForm.reset({
        userProfileId: null,
        legoThemeId: null,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfilePreferredThemeForm.reset({
        userProfileId: userProfilePreferredThemeData.userProfileId,
        legoThemeId: userProfilePreferredThemeData.legoThemeId,
        sequence: userProfilePreferredThemeData.sequence?.toString() ?? '',
        active: userProfilePreferredThemeData.active ?? true,
        deleted: userProfilePreferredThemeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfilePreferredThemeForm.markAsPristine();
    this.userProfilePreferredThemeForm.markAsUntouched();
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


  public userIsBMCUserProfilePreferredThemeReader(): boolean {
    return this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeReader();
  }

  public userIsBMCUserProfilePreferredThemeWriter(): boolean {
    return this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeWriter();
  }
}

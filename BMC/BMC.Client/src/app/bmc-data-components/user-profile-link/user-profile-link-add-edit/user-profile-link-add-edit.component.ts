/*
   GENERATED FORM FOR THE USERPROFILELINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileLinkService, UserProfileLinkData, UserProfileLinkSubmitData } from '../../../bmc-data-services/user-profile-link.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserProfileService } from '../../../bmc-data-services/user-profile.service';
import { UserProfileLinkTypeService } from '../../../bmc-data-services/user-profile-link-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserProfileLinkFormValues {
  userProfileId: number | bigint,       // For FK link number
  userProfileLinkTypeId: number | bigint,       // For FK link number
  url: string,
  displayLabel: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-profile-link-add-edit',
  templateUrl: './user-profile-link-add-edit.component.html',
  styleUrls: ['./user-profile-link-add-edit.component.scss']
})
export class UserProfileLinkAddEditComponent {
  @ViewChild('userProfileLinkModal') userProfileLinkModal!: TemplateRef<any>;
  @Output() userProfileLinkChanged = new Subject<UserProfileLinkData[]>();
  @Input() userProfileLinkSubmitData: UserProfileLinkSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileLinkForm: FormGroup = this.fb.group({
        userProfileId: [null, Validators.required],
        userProfileLinkTypeId: [null, Validators.required],
        url: ['', Validators.required],
        displayLabel: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userProfileLinks$ = this.userProfileLinkService.GetUserProfileLinkList();
  userProfiles$ = this.userProfileService.GetUserProfileList();
  userProfileLinkTypes$ = this.userProfileLinkTypeService.GetUserProfileLinkTypeList();

  constructor(
    private modalService: NgbModal,
    private userProfileLinkService: UserProfileLinkService,
    private userProfileService: UserProfileService,
    private userProfileLinkTypeService: UserProfileLinkTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userProfileLinkData?: UserProfileLinkData) {

    if (userProfileLinkData != null) {

      if (!this.userProfileLinkService.userIsBMCUserProfileLinkReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Profile Links`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userProfileLinkSubmitData = this.userProfileLinkService.ConvertToUserProfileLinkSubmitData(userProfileLinkData);
      this.isEditMode = true;
      this.objectGuid = userProfileLinkData.objectGuid;

      this.buildFormValues(userProfileLinkData);

    } else {

      if (!this.userProfileLinkService.userIsBMCUserProfileLinkWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Profile Links`,
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
        this.userProfileLinkForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userProfileLinkModal, {
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

    if (this.userProfileLinkService.userIsBMCUserProfileLinkWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Profile Links`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userProfileLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileLinkSubmitData: UserProfileLinkSubmitData = {
        id: this.userProfileLinkSubmitData?.id || 0,
        userProfileId: Number(formValue.userProfileId),
        userProfileLinkTypeId: Number(formValue.userProfileLinkTypeId),
        url: formValue.url!.trim(),
        displayLabel: formValue.displayLabel?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserProfileLink(userProfileLinkSubmitData);
      } else {
        this.addUserProfileLink(userProfileLinkSubmitData);
      }
  }

  private addUserProfileLink(userProfileLinkData: UserProfileLinkSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userProfileLinkData.active = true;
    userProfileLinkData.deleted = false;
    this.userProfileLinkService.PostUserProfileLink(userProfileLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserProfileLink) => {

        this.userProfileLinkService.ClearAllCaches();

        this.userProfileLinkChanged.next([newUserProfileLink]);

        this.alertService.showMessage("User Profile Link added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userprofilelink', newUserProfileLink.id]);
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
                                   'You do not have permission to save this User Profile Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserProfileLink(userProfileLinkData: UserProfileLinkSubmitData) {
    this.userProfileLinkService.PutUserProfileLink(userProfileLinkData.id, userProfileLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserProfileLink) => {

        this.userProfileLinkService.ClearAllCaches();

        this.userProfileLinkChanged.next([updatedUserProfileLink]);

        this.alertService.showMessage("User Profile Link updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Profile Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userProfileLinkData: UserProfileLinkData | null) {

    if (userProfileLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileLinkForm.reset({
        userProfileId: null,
        userProfileLinkTypeId: null,
        url: '',
        displayLabel: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileLinkForm.reset({
        userProfileId: userProfileLinkData.userProfileId,
        userProfileLinkTypeId: userProfileLinkData.userProfileLinkTypeId,
        url: userProfileLinkData.url ?? '',
        displayLabel: userProfileLinkData.displayLabel ?? '',
        sequence: userProfileLinkData.sequence?.toString() ?? '',
        active: userProfileLinkData.active ?? true,
        deleted: userProfileLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileLinkForm.markAsPristine();
    this.userProfileLinkForm.markAsUntouched();
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


  public userIsBMCUserProfileLinkReader(): boolean {
    return this.userProfileLinkService.userIsBMCUserProfileLinkReader();
  }

  public userIsBMCUserProfileLinkWriter(): boolean {
    return this.userProfileLinkService.userIsBMCUserProfileLinkWriter();
  }
}

/*
   GENERATED FORM FOR THE USERPROFILESTAT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileStat table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-stat-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileStatService, UserProfileStatData, UserProfileStatSubmitData } from '../../../bmc-data-services/user-profile-stat.service';
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
interface UserProfileStatFormValues {
  userProfileId: number | bigint,       // For FK link number
  totalPartsOwned: string,     // Stored as string for form input, converted to number on submit.
  totalUniquePartsOwned: string,     // Stored as string for form input, converted to number on submit.
  totalSetsOwned: string,     // Stored as string for form input, converted to number on submit.
  totalMocsPublished: string,     // Stored as string for form input, converted to number on submit.
  totalFollowers: string,     // Stored as string for form input, converted to number on submit.
  totalFollowing: string,     // Stored as string for form input, converted to number on submit.
  totalLikesReceived: string,     // Stored as string for form input, converted to number on submit.
  totalAchievementPoints: string,     // Stored as string for form input, converted to number on submit.
  lastCalculatedDate: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-profile-stat-add-edit',
  templateUrl: './user-profile-stat-add-edit.component.html',
  styleUrls: ['./user-profile-stat-add-edit.component.scss']
})
export class UserProfileStatAddEditComponent {
  @ViewChild('userProfileStatModal') userProfileStatModal!: TemplateRef<any>;
  @Output() userProfileStatChanged = new Subject<UserProfileStatData[]>();
  @Input() userProfileStatSubmitData: UserProfileStatSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileStatFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileStatForm: FormGroup = this.fb.group({
        userProfileId: [null, Validators.required],
        totalPartsOwned: ['', Validators.required],
        totalUniquePartsOwned: ['', Validators.required],
        totalSetsOwned: ['', Validators.required],
        totalMocsPublished: ['', Validators.required],
        totalFollowers: ['', Validators.required],
        totalFollowing: ['', Validators.required],
        totalLikesReceived: ['', Validators.required],
        totalAchievementPoints: ['', Validators.required],
        lastCalculatedDate: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userProfileStats$ = this.userProfileStatService.GetUserProfileStatList();
  userProfiles$ = this.userProfileService.GetUserProfileList();

  constructor(
    private modalService: NgbModal,
    private userProfileStatService: UserProfileStatService,
    private userProfileService: UserProfileService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userProfileStatData?: UserProfileStatData) {

    if (userProfileStatData != null) {

      if (!this.userProfileStatService.userIsBMCUserProfileStatReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Profile Stats`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userProfileStatSubmitData = this.userProfileStatService.ConvertToUserProfileStatSubmitData(userProfileStatData);
      this.isEditMode = true;
      this.objectGuid = userProfileStatData.objectGuid;

      this.buildFormValues(userProfileStatData);

    } else {

      if (!this.userProfileStatService.userIsBMCUserProfileStatWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Profile Stats`,
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
        this.userProfileStatForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileStatForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userProfileStatModal, {
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

    if (this.userProfileStatService.userIsBMCUserProfileStatWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Profile Stats`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userProfileStatForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileStatForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileStatForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileStatSubmitData: UserProfileStatSubmitData = {
        id: this.userProfileStatSubmitData?.id || 0,
        userProfileId: Number(formValue.userProfileId),
        totalPartsOwned: Number(formValue.totalPartsOwned),
        totalUniquePartsOwned: Number(formValue.totalUniquePartsOwned),
        totalSetsOwned: Number(formValue.totalSetsOwned),
        totalMocsPublished: Number(formValue.totalMocsPublished),
        totalFollowers: Number(formValue.totalFollowers),
        totalFollowing: Number(formValue.totalFollowing),
        totalLikesReceived: Number(formValue.totalLikesReceived),
        totalAchievementPoints: Number(formValue.totalAchievementPoints),
        lastCalculatedDate: formValue.lastCalculatedDate ? dateTimeLocalToIsoUtc(formValue.lastCalculatedDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserProfileStat(userProfileStatSubmitData);
      } else {
        this.addUserProfileStat(userProfileStatSubmitData);
      }
  }

  private addUserProfileStat(userProfileStatData: UserProfileStatSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userProfileStatData.active = true;
    userProfileStatData.deleted = false;
    this.userProfileStatService.PostUserProfileStat(userProfileStatData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserProfileStat) => {

        this.userProfileStatService.ClearAllCaches();

        this.userProfileStatChanged.next([newUserProfileStat]);

        this.alertService.showMessage("User Profile Stat added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userprofilestat', newUserProfileStat.id]);
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
                                   'You do not have permission to save this User Profile Stat.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Stat.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Stat could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserProfileStat(userProfileStatData: UserProfileStatSubmitData) {
    this.userProfileStatService.PutUserProfileStat(userProfileStatData.id, userProfileStatData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserProfileStat) => {

        this.userProfileStatService.ClearAllCaches();

        this.userProfileStatChanged.next([updatedUserProfileStat]);

        this.alertService.showMessage("User Profile Stat updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Profile Stat.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Stat.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Stat could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userProfileStatData: UserProfileStatData | null) {

    if (userProfileStatData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileStatForm.reset({
        userProfileId: null,
        totalPartsOwned: '',
        totalUniquePartsOwned: '',
        totalSetsOwned: '',
        totalMocsPublished: '',
        totalFollowers: '',
        totalFollowing: '',
        totalLikesReceived: '',
        totalAchievementPoints: '',
        lastCalculatedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileStatForm.reset({
        userProfileId: userProfileStatData.userProfileId,
        totalPartsOwned: userProfileStatData.totalPartsOwned?.toString() ?? '',
        totalUniquePartsOwned: userProfileStatData.totalUniquePartsOwned?.toString() ?? '',
        totalSetsOwned: userProfileStatData.totalSetsOwned?.toString() ?? '',
        totalMocsPublished: userProfileStatData.totalMocsPublished?.toString() ?? '',
        totalFollowers: userProfileStatData.totalFollowers?.toString() ?? '',
        totalFollowing: userProfileStatData.totalFollowing?.toString() ?? '',
        totalLikesReceived: userProfileStatData.totalLikesReceived?.toString() ?? '',
        totalAchievementPoints: userProfileStatData.totalAchievementPoints?.toString() ?? '',
        lastCalculatedDate: isoUtcStringToDateTimeLocal(userProfileStatData.lastCalculatedDate) ?? '',
        active: userProfileStatData.active ?? true,
        deleted: userProfileStatData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileStatForm.markAsPristine();
    this.userProfileStatForm.markAsUntouched();
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


  public userIsBMCUserProfileStatReader(): boolean {
    return this.userProfileStatService.userIsBMCUserProfileStatReader();
  }

  public userIsBMCUserProfileStatWriter(): boolean {
    return this.userProfileStatService.userIsBMCUserProfileStatWriter();
  }
}

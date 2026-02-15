/*
   GENERATED FORM FOR THE USERACHIEVEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserAchievement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-achievement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserAchievementService, UserAchievementData, UserAchievementSubmitData } from '../../../bmc-data-services/user-achievement.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AchievementService } from '../../../bmc-data-services/achievement.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserAchievementFormValues {
  achievementId: number | bigint,       // For FK link number
  earnedDate: string,
  isDisplayed: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-achievement-add-edit',
  templateUrl: './user-achievement-add-edit.component.html',
  styleUrls: ['./user-achievement-add-edit.component.scss']
})
export class UserAchievementAddEditComponent {
  @ViewChild('userAchievementModal') userAchievementModal!: TemplateRef<any>;
  @Output() userAchievementChanged = new Subject<UserAchievementData[]>();
  @Input() userAchievementSubmitData: UserAchievementSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserAchievementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userAchievementForm: FormGroup = this.fb.group({
        achievementId: [null, Validators.required],
        earnedDate: ['', Validators.required],
        isDisplayed: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userAchievements$ = this.userAchievementService.GetUserAchievementList();
  achievements$ = this.achievementService.GetAchievementList();

  constructor(
    private modalService: NgbModal,
    private userAchievementService: UserAchievementService,
    private achievementService: AchievementService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userAchievementData?: UserAchievementData) {

    if (userAchievementData != null) {

      if (!this.userAchievementService.userIsBMCUserAchievementReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Achievements`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userAchievementSubmitData = this.userAchievementService.ConvertToUserAchievementSubmitData(userAchievementData);
      this.isEditMode = true;
      this.objectGuid = userAchievementData.objectGuid;

      this.buildFormValues(userAchievementData);

    } else {

      if (!this.userAchievementService.userIsBMCUserAchievementWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Achievements`,
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
        this.userAchievementForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userAchievementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userAchievementModal, {
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

    if (this.userAchievementService.userIsBMCUserAchievementWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Achievements`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userAchievementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userAchievementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userAchievementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userAchievementSubmitData: UserAchievementSubmitData = {
        id: this.userAchievementSubmitData?.id || 0,
        achievementId: Number(formValue.achievementId),
        earnedDate: dateTimeLocalToIsoUtc(formValue.earnedDate!.trim())!,
        isDisplayed: !!formValue.isDisplayed,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserAchievement(userAchievementSubmitData);
      } else {
        this.addUserAchievement(userAchievementSubmitData);
      }
  }

  private addUserAchievement(userAchievementData: UserAchievementSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userAchievementData.active = true;
    userAchievementData.deleted = false;
    this.userAchievementService.PostUserAchievement(userAchievementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserAchievement) => {

        this.userAchievementService.ClearAllCaches();

        this.userAchievementChanged.next([newUserAchievement]);

        this.alertService.showMessage("User Achievement added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userachievement', newUserAchievement.id]);
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
                                   'You do not have permission to save this User Achievement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Achievement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Achievement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserAchievement(userAchievementData: UserAchievementSubmitData) {
    this.userAchievementService.PutUserAchievement(userAchievementData.id, userAchievementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserAchievement) => {

        this.userAchievementService.ClearAllCaches();

        this.userAchievementChanged.next([updatedUserAchievement]);

        this.alertService.showMessage("User Achievement updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Achievement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Achievement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Achievement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userAchievementData: UserAchievementData | null) {

    if (userAchievementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userAchievementForm.reset({
        achievementId: null,
        earnedDate: '',
        isDisplayed: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userAchievementForm.reset({
        achievementId: userAchievementData.achievementId,
        earnedDate: isoUtcStringToDateTimeLocal(userAchievementData.earnedDate) ?? '',
        isDisplayed: userAchievementData.isDisplayed ?? false,
        active: userAchievementData.active ?? true,
        deleted: userAchievementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userAchievementForm.markAsPristine();
    this.userAchievementForm.markAsUntouched();
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


  public userIsBMCUserAchievementReader(): boolean {
    return this.userAchievementService.userIsBMCUserAchievementReader();
  }

  public userIsBMCUserAchievementWriter(): boolean {
    return this.userAchievementService.userIsBMCUserAchievementWriter();
  }
}

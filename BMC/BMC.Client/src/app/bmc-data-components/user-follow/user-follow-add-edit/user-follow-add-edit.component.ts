/*
   GENERATED FORM FOR THE USERFOLLOW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserFollow table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-follow-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserFollowService, UserFollowData, UserFollowSubmitData } from '../../../bmc-data-services/user-follow.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserFollowFormValues {
  followerTenantGuid: string,
  followedTenantGuid: string,
  followedDate: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-follow-add-edit',
  templateUrl: './user-follow-add-edit.component.html',
  styleUrls: ['./user-follow-add-edit.component.scss']
})
export class UserFollowAddEditComponent {
  @ViewChild('userFollowModal') userFollowModal!: TemplateRef<any>;
  @Output() userFollowChanged = new Subject<UserFollowData[]>();
  @Input() userFollowSubmitData: UserFollowSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserFollowFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userFollowForm: FormGroup = this.fb.group({
        followerTenantGuid: ['', Validators.required],
        followedTenantGuid: ['', Validators.required],
        followedDate: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userFollows$ = this.userFollowService.GetUserFollowList();

  constructor(
    private modalService: NgbModal,
    private userFollowService: UserFollowService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userFollowData?: UserFollowData) {

    if (userFollowData != null) {

      if (!this.userFollowService.userIsBMCUserFollowReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Follows`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userFollowSubmitData = this.userFollowService.ConvertToUserFollowSubmitData(userFollowData);
      this.isEditMode = true;
      this.objectGuid = userFollowData.objectGuid;

      this.buildFormValues(userFollowData);

    } else {

      if (!this.userFollowService.userIsBMCUserFollowWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Follows`,
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
        this.userFollowForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userFollowForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userFollowModal, {
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

    if (this.userFollowService.userIsBMCUserFollowWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Follows`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userFollowForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userFollowForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userFollowForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userFollowSubmitData: UserFollowSubmitData = {
        id: this.userFollowSubmitData?.id || 0,
        followerTenantGuid: formValue.followerTenantGuid!.trim(),
        followedTenantGuid: formValue.followedTenantGuid!.trim(),
        followedDate: dateTimeLocalToIsoUtc(formValue.followedDate!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserFollow(userFollowSubmitData);
      } else {
        this.addUserFollow(userFollowSubmitData);
      }
  }

  private addUserFollow(userFollowData: UserFollowSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userFollowData.active = true;
    userFollowData.deleted = false;
    this.userFollowService.PostUserFollow(userFollowData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserFollow) => {

        this.userFollowService.ClearAllCaches();

        this.userFollowChanged.next([newUserFollow]);

        this.alertService.showMessage("User Follow added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userfollow', newUserFollow.id]);
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
                                   'You do not have permission to save this User Follow.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Follow.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Follow could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserFollow(userFollowData: UserFollowSubmitData) {
    this.userFollowService.PutUserFollow(userFollowData.id, userFollowData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserFollow) => {

        this.userFollowService.ClearAllCaches();

        this.userFollowChanged.next([updatedUserFollow]);

        this.alertService.showMessage("User Follow updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Follow.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Follow.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Follow could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userFollowData: UserFollowData | null) {

    if (userFollowData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userFollowForm.reset({
        followerTenantGuid: '',
        followedTenantGuid: '',
        followedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userFollowForm.reset({
        followerTenantGuid: userFollowData.followerTenantGuid ?? '',
        followedTenantGuid: userFollowData.followedTenantGuid ?? '',
        followedDate: isoUtcStringToDateTimeLocal(userFollowData.followedDate) ?? '',
        active: userFollowData.active ?? true,
        deleted: userFollowData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userFollowForm.markAsPristine();
    this.userFollowForm.markAsUntouched();
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


  public userIsBMCUserFollowReader(): boolean {
    return this.userFollowService.userIsBMCUserFollowReader();
  }

  public userIsBMCUserFollowWriter(): boolean {
    return this.userFollowService.userIsBMCUserFollowWriter();
  }
}

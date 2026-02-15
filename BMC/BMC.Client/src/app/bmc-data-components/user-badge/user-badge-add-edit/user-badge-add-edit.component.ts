/*
   GENERATED FORM FOR THE USERBADGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserBadge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-badge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserBadgeService, UserBadgeData, UserBadgeSubmitData } from '../../../bmc-data-services/user-badge.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserBadgeFormValues {
  name: string,
  description: string,
  iconCssClass: string | null,
  iconImagePath: string | null,
  badgeColor: string | null,
  isAutomatic: boolean,
  automaticCriteriaCode: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-badge-add-edit',
  templateUrl: './user-badge-add-edit.component.html',
  styleUrls: ['./user-badge-add-edit.component.scss']
})
export class UserBadgeAddEditComponent {
  @ViewChild('userBadgeModal') userBadgeModal!: TemplateRef<any>;
  @Output() userBadgeChanged = new Subject<UserBadgeData[]>();
  @Input() userBadgeSubmitData: UserBadgeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserBadgeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userBadgeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        iconCssClass: [''],
        iconImagePath: [''],
        badgeColor: [''],
        isAutomatic: [false],
        automaticCriteriaCode: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userBadges$ = this.userBadgeService.GetUserBadgeList();

  constructor(
    private modalService: NgbModal,
    private userBadgeService: UserBadgeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userBadgeData?: UserBadgeData) {

    if (userBadgeData != null) {

      if (!this.userBadgeService.userIsBMCUserBadgeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Badges`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userBadgeSubmitData = this.userBadgeService.ConvertToUserBadgeSubmitData(userBadgeData);
      this.isEditMode = true;
      this.objectGuid = userBadgeData.objectGuid;

      this.buildFormValues(userBadgeData);

    } else {

      if (!this.userBadgeService.userIsBMCUserBadgeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Badges`,
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
        this.userBadgeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userBadgeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userBadgeModal, {
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

    if (this.userBadgeService.userIsBMCUserBadgeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Badges`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userBadgeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userBadgeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userBadgeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userBadgeSubmitData: UserBadgeSubmitData = {
        id: this.userBadgeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        iconCssClass: formValue.iconCssClass?.trim() || null,
        iconImagePath: formValue.iconImagePath?.trim() || null,
        badgeColor: formValue.badgeColor?.trim() || null,
        isAutomatic: !!formValue.isAutomatic,
        automaticCriteriaCode: formValue.automaticCriteriaCode?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserBadge(userBadgeSubmitData);
      } else {
        this.addUserBadge(userBadgeSubmitData);
      }
  }

  private addUserBadge(userBadgeData: UserBadgeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userBadgeData.active = true;
    userBadgeData.deleted = false;
    this.userBadgeService.PostUserBadge(userBadgeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserBadge) => {

        this.userBadgeService.ClearAllCaches();

        this.userBadgeChanged.next([newUserBadge]);

        this.alertService.showMessage("User Badge added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userbadge', newUserBadge.id]);
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
                                   'You do not have permission to save this User Badge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Badge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Badge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserBadge(userBadgeData: UserBadgeSubmitData) {
    this.userBadgeService.PutUserBadge(userBadgeData.id, userBadgeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserBadge) => {

        this.userBadgeService.ClearAllCaches();

        this.userBadgeChanged.next([updatedUserBadge]);

        this.alertService.showMessage("User Badge updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Badge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Badge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Badge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userBadgeData: UserBadgeData | null) {

    if (userBadgeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userBadgeForm.reset({
        name: '',
        description: '',
        iconCssClass: '',
        iconImagePath: '',
        badgeColor: '',
        isAutomatic: false,
        automaticCriteriaCode: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userBadgeForm.reset({
        name: userBadgeData.name ?? '',
        description: userBadgeData.description ?? '',
        iconCssClass: userBadgeData.iconCssClass ?? '',
        iconImagePath: userBadgeData.iconImagePath ?? '',
        badgeColor: userBadgeData.badgeColor ?? '',
        isAutomatic: userBadgeData.isAutomatic ?? false,
        automaticCriteriaCode: userBadgeData.automaticCriteriaCode ?? '',
        sequence: userBadgeData.sequence?.toString() ?? '',
        active: userBadgeData.active ?? true,
        deleted: userBadgeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userBadgeForm.markAsPristine();
    this.userBadgeForm.markAsUntouched();
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


  public userIsBMCUserBadgeReader(): boolean {
    return this.userBadgeService.userIsBMCUserBadgeReader();
  }

  public userIsBMCUserBadgeWriter(): boolean {
    return this.userBadgeService.userIsBMCUserBadgeWriter();
  }
}

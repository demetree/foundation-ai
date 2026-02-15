/*
   GENERATED FORM FOR THE USERPROFILELINKTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileLinkType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-link-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileLinkTypeService, UserProfileLinkTypeData, UserProfileLinkTypeSubmitData } from '../../../bmc-data-services/user-profile-link-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserProfileLinkTypeFormValues {
  name: string,
  description: string,
  iconCssClass: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-profile-link-type-add-edit',
  templateUrl: './user-profile-link-type-add-edit.component.html',
  styleUrls: ['./user-profile-link-type-add-edit.component.scss']
})
export class UserProfileLinkTypeAddEditComponent {
  @ViewChild('userProfileLinkTypeModal') userProfileLinkTypeModal!: TemplateRef<any>;
  @Output() userProfileLinkTypeChanged = new Subject<UserProfileLinkTypeData[]>();
  @Input() userProfileLinkTypeSubmitData: UserProfileLinkTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileLinkTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileLinkTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        iconCssClass: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userProfileLinkTypes$ = this.userProfileLinkTypeService.GetUserProfileLinkTypeList();

  constructor(
    private modalService: NgbModal,
    private userProfileLinkTypeService: UserProfileLinkTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userProfileLinkTypeData?: UserProfileLinkTypeData) {

    if (userProfileLinkTypeData != null) {

      if (!this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Profile Link Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userProfileLinkTypeSubmitData = this.userProfileLinkTypeService.ConvertToUserProfileLinkTypeSubmitData(userProfileLinkTypeData);
      this.isEditMode = true;
      this.objectGuid = userProfileLinkTypeData.objectGuid;

      this.buildFormValues(userProfileLinkTypeData);

    } else {

      if (!this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Profile Link Types`,
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
        this.userProfileLinkTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileLinkTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userProfileLinkTypeModal, {
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

    if (this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Profile Link Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userProfileLinkTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileLinkTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileLinkTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileLinkTypeSubmitData: UserProfileLinkTypeSubmitData = {
        id: this.userProfileLinkTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        iconCssClass: formValue.iconCssClass?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserProfileLinkType(userProfileLinkTypeSubmitData);
      } else {
        this.addUserProfileLinkType(userProfileLinkTypeSubmitData);
      }
  }

  private addUserProfileLinkType(userProfileLinkTypeData: UserProfileLinkTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userProfileLinkTypeData.active = true;
    userProfileLinkTypeData.deleted = false;
    this.userProfileLinkTypeService.PostUserProfileLinkType(userProfileLinkTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserProfileLinkType) => {

        this.userProfileLinkTypeService.ClearAllCaches();

        this.userProfileLinkTypeChanged.next([newUserProfileLinkType]);

        this.alertService.showMessage("User Profile Link Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userprofilelinktype', newUserProfileLinkType.id]);
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
                                   'You do not have permission to save this User Profile Link Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Link Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Link Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserProfileLinkType(userProfileLinkTypeData: UserProfileLinkTypeSubmitData) {
    this.userProfileLinkTypeService.PutUserProfileLinkType(userProfileLinkTypeData.id, userProfileLinkTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserProfileLinkType) => {

        this.userProfileLinkTypeService.ClearAllCaches();

        this.userProfileLinkTypeChanged.next([updatedUserProfileLinkType]);

        this.alertService.showMessage("User Profile Link Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Profile Link Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Link Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Link Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userProfileLinkTypeData: UserProfileLinkTypeData | null) {

    if (userProfileLinkTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileLinkTypeForm.reset({
        name: '',
        description: '',
        iconCssClass: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileLinkTypeForm.reset({
        name: userProfileLinkTypeData.name ?? '',
        description: userProfileLinkTypeData.description ?? '',
        iconCssClass: userProfileLinkTypeData.iconCssClass ?? '',
        sequence: userProfileLinkTypeData.sequence?.toString() ?? '',
        active: userProfileLinkTypeData.active ?? true,
        deleted: userProfileLinkTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileLinkTypeForm.markAsPristine();
    this.userProfileLinkTypeForm.markAsUntouched();
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


  public userIsBMCUserProfileLinkTypeReader(): boolean {
    return this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeReader();
  }

  public userIsBMCUserProfileLinkTypeWriter(): boolean {
    return this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeWriter();
  }
}

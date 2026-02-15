/*
   GENERATED FORM FOR THE USERBADGEASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserBadgeAssignment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-badge-assignment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserBadgeAssignmentService, UserBadgeAssignmentData, UserBadgeAssignmentSubmitData } from '../../../bmc-data-services/user-badge-assignment.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserBadgeService } from '../../../bmc-data-services/user-badge.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserBadgeAssignmentFormValues {
  userBadgeId: number | bigint,       // For FK link number
  awardedDate: string,
  awardedByTenantGuid: string | null,
  reason: string | null,
  isDisplayed: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-badge-assignment-add-edit',
  templateUrl: './user-badge-assignment-add-edit.component.html',
  styleUrls: ['./user-badge-assignment-add-edit.component.scss']
})
export class UserBadgeAssignmentAddEditComponent {
  @ViewChild('userBadgeAssignmentModal') userBadgeAssignmentModal!: TemplateRef<any>;
  @Output() userBadgeAssignmentChanged = new Subject<UserBadgeAssignmentData[]>();
  @Input() userBadgeAssignmentSubmitData: UserBadgeAssignmentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserBadgeAssignmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userBadgeAssignmentForm: FormGroup = this.fb.group({
        userBadgeId: [null, Validators.required],
        awardedDate: ['', Validators.required],
        awardedByTenantGuid: [''],
        reason: [''],
        isDisplayed: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userBadgeAssignments$ = this.userBadgeAssignmentService.GetUserBadgeAssignmentList();
  userBadges$ = this.userBadgeService.GetUserBadgeList();

  constructor(
    private modalService: NgbModal,
    private userBadgeAssignmentService: UserBadgeAssignmentService,
    private userBadgeService: UserBadgeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userBadgeAssignmentData?: UserBadgeAssignmentData) {

    if (userBadgeAssignmentData != null) {

      if (!this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Badge Assignments`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userBadgeAssignmentSubmitData = this.userBadgeAssignmentService.ConvertToUserBadgeAssignmentSubmitData(userBadgeAssignmentData);
      this.isEditMode = true;
      this.objectGuid = userBadgeAssignmentData.objectGuid;

      this.buildFormValues(userBadgeAssignmentData);

    } else {

      if (!this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Badge Assignments`,
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
        this.userBadgeAssignmentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userBadgeAssignmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userBadgeAssignmentModal, {
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

    if (this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Badge Assignments`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userBadgeAssignmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userBadgeAssignmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userBadgeAssignmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userBadgeAssignmentSubmitData: UserBadgeAssignmentSubmitData = {
        id: this.userBadgeAssignmentSubmitData?.id || 0,
        userBadgeId: Number(formValue.userBadgeId),
        awardedDate: dateTimeLocalToIsoUtc(formValue.awardedDate!.trim())!,
        awardedByTenantGuid: formValue.awardedByTenantGuid?.trim() || null,
        reason: formValue.reason?.trim() || null,
        isDisplayed: !!formValue.isDisplayed,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserBadgeAssignment(userBadgeAssignmentSubmitData);
      } else {
        this.addUserBadgeAssignment(userBadgeAssignmentSubmitData);
      }
  }

  private addUserBadgeAssignment(userBadgeAssignmentData: UserBadgeAssignmentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userBadgeAssignmentData.active = true;
    userBadgeAssignmentData.deleted = false;
    this.userBadgeAssignmentService.PostUserBadgeAssignment(userBadgeAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserBadgeAssignment) => {

        this.userBadgeAssignmentService.ClearAllCaches();

        this.userBadgeAssignmentChanged.next([newUserBadgeAssignment]);

        this.alertService.showMessage("User Badge Assignment added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userbadgeassignment', newUserBadgeAssignment.id]);
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
                                   'You do not have permission to save this User Badge Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Badge Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Badge Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserBadgeAssignment(userBadgeAssignmentData: UserBadgeAssignmentSubmitData) {
    this.userBadgeAssignmentService.PutUserBadgeAssignment(userBadgeAssignmentData.id, userBadgeAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserBadgeAssignment) => {

        this.userBadgeAssignmentService.ClearAllCaches();

        this.userBadgeAssignmentChanged.next([updatedUserBadgeAssignment]);

        this.alertService.showMessage("User Badge Assignment updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Badge Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Badge Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Badge Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userBadgeAssignmentData: UserBadgeAssignmentData | null) {

    if (userBadgeAssignmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userBadgeAssignmentForm.reset({
        userBadgeId: null,
        awardedDate: '',
        awardedByTenantGuid: '',
        reason: '',
        isDisplayed: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userBadgeAssignmentForm.reset({
        userBadgeId: userBadgeAssignmentData.userBadgeId,
        awardedDate: isoUtcStringToDateTimeLocal(userBadgeAssignmentData.awardedDate) ?? '',
        awardedByTenantGuid: userBadgeAssignmentData.awardedByTenantGuid ?? '',
        reason: userBadgeAssignmentData.reason ?? '',
        isDisplayed: userBadgeAssignmentData.isDisplayed ?? false,
        active: userBadgeAssignmentData.active ?? true,
        deleted: userBadgeAssignmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userBadgeAssignmentForm.markAsPristine();
    this.userBadgeAssignmentForm.markAsUntouched();
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


  public userIsBMCUserBadgeAssignmentReader(): boolean {
    return this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentReader();
  }

  public userIsBMCUserBadgeAssignmentWriter(): boolean {
    return this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentWriter();
  }
}

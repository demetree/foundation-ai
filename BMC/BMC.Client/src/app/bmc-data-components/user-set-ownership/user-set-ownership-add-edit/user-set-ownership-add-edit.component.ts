/*
   GENERATED FORM FOR THE USERSETOWNERSHIP TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSetOwnership table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-set-ownership-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserSetOwnershipService, UserSetOwnershipData, UserSetOwnershipSubmitData } from '../../../bmc-data-services/user-set-ownership.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserSetOwnershipFormValues {
  legoSetId: number | bigint,       // For FK link number
  status: string,
  acquiredDate: string | null,
  personalRating: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  quantity: string,     // Stored as string for form input, converted to number on submit.
  isPublic: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-set-ownership-add-edit',
  templateUrl: './user-set-ownership-add-edit.component.html',
  styleUrls: ['./user-set-ownership-add-edit.component.scss']
})
export class UserSetOwnershipAddEditComponent {
  @ViewChild('userSetOwnershipModal') userSetOwnershipModal!: TemplateRef<any>;
  @Output() userSetOwnershipChanged = new Subject<UserSetOwnershipData[]>();
  @Input() userSetOwnershipSubmitData: UserSetOwnershipSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserSetOwnershipFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userSetOwnershipForm: FormGroup = this.fb.group({
        legoSetId: [null, Validators.required],
        status: ['', Validators.required],
        acquiredDate: [''],
        personalRating: [''],
        notes: [''],
        quantity: ['', Validators.required],
        isPublic: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userSetOwnerships$ = this.userSetOwnershipService.GetUserSetOwnershipList();
  legoSets$ = this.legoSetService.GetLegoSetList();

  constructor(
    private modalService: NgbModal,
    private userSetOwnershipService: UserSetOwnershipService,
    private legoSetService: LegoSetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userSetOwnershipData?: UserSetOwnershipData) {

    if (userSetOwnershipData != null) {

      if (!this.userSetOwnershipService.userIsBMCUserSetOwnershipReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Set Ownerships`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userSetOwnershipSubmitData = this.userSetOwnershipService.ConvertToUserSetOwnershipSubmitData(userSetOwnershipData);
      this.isEditMode = true;
      this.objectGuid = userSetOwnershipData.objectGuid;

      this.buildFormValues(userSetOwnershipData);

    } else {

      if (!this.userSetOwnershipService.userIsBMCUserSetOwnershipWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Set Ownerships`,
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
        this.userSetOwnershipForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userSetOwnershipForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userSetOwnershipModal, {
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

    if (this.userSetOwnershipService.userIsBMCUserSetOwnershipWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Set Ownerships`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userSetOwnershipForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userSetOwnershipForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userSetOwnershipForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userSetOwnershipSubmitData: UserSetOwnershipSubmitData = {
        id: this.userSetOwnershipSubmitData?.id || 0,
        legoSetId: Number(formValue.legoSetId),
        status: formValue.status!.trim(),
        acquiredDate: formValue.acquiredDate ? dateTimeLocalToIsoUtc(formValue.acquiredDate.trim()) : null,
        personalRating: formValue.personalRating ? Number(formValue.personalRating) : null,
        notes: formValue.notes?.trim() || null,
        quantity: Number(formValue.quantity),
        isPublic: !!formValue.isPublic,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserSetOwnership(userSetOwnershipSubmitData);
      } else {
        this.addUserSetOwnership(userSetOwnershipSubmitData);
      }
  }

  private addUserSetOwnership(userSetOwnershipData: UserSetOwnershipSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userSetOwnershipData.active = true;
    userSetOwnershipData.deleted = false;
    this.userSetOwnershipService.PostUserSetOwnership(userSetOwnershipData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserSetOwnership) => {

        this.userSetOwnershipService.ClearAllCaches();

        this.userSetOwnershipChanged.next([newUserSetOwnership]);

        this.alertService.showMessage("User Set Ownership added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usersetownership', newUserSetOwnership.id]);
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
                                   'You do not have permission to save this User Set Ownership.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Set Ownership.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Set Ownership could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserSetOwnership(userSetOwnershipData: UserSetOwnershipSubmitData) {
    this.userSetOwnershipService.PutUserSetOwnership(userSetOwnershipData.id, userSetOwnershipData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserSetOwnership) => {

        this.userSetOwnershipService.ClearAllCaches();

        this.userSetOwnershipChanged.next([updatedUserSetOwnership]);

        this.alertService.showMessage("User Set Ownership updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Set Ownership.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Set Ownership.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Set Ownership could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userSetOwnershipData: UserSetOwnershipData | null) {

    if (userSetOwnershipData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userSetOwnershipForm.reset({
        legoSetId: null,
        status: '',
        acquiredDate: '',
        personalRating: '',
        notes: '',
        quantity: '',
        isPublic: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userSetOwnershipForm.reset({
        legoSetId: userSetOwnershipData.legoSetId,
        status: userSetOwnershipData.status ?? '',
        acquiredDate: isoUtcStringToDateTimeLocal(userSetOwnershipData.acquiredDate) ?? '',
        personalRating: userSetOwnershipData.personalRating?.toString() ?? '',
        notes: userSetOwnershipData.notes ?? '',
        quantity: userSetOwnershipData.quantity?.toString() ?? '',
        isPublic: userSetOwnershipData.isPublic ?? false,
        active: userSetOwnershipData.active ?? true,
        deleted: userSetOwnershipData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userSetOwnershipForm.markAsPristine();
    this.userSetOwnershipForm.markAsUntouched();
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


  public userIsBMCUserSetOwnershipReader(): boolean {
    return this.userSetOwnershipService.userIsBMCUserSetOwnershipReader();
  }

  public userIsBMCUserSetOwnershipWriter(): boolean {
    return this.userSetOwnershipService.userIsBMCUserSetOwnershipWriter();
  }
}

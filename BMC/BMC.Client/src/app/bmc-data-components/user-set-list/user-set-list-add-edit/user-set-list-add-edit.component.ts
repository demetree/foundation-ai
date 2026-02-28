/*
   GENERATED FORM FOR THE USERSETLIST TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSetList table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-set-list-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserSetListService, UserSetListData, UserSetListSubmitData } from '../../../bmc-data-services/user-set-list.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserSetListFormValues {
  name: string,
  isBuildable: boolean,
  rebrickableListId: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-set-list-add-edit',
  templateUrl: './user-set-list-add-edit.component.html',
  styleUrls: ['./user-set-list-add-edit.component.scss']
})
export class UserSetListAddEditComponent {
  @ViewChild('userSetListModal') userSetListModal!: TemplateRef<any>;
  @Output() userSetListChanged = new Subject<UserSetListData[]>();
  @Input() userSetListSubmitData: UserSetListSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserSetListFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userSetListForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        isBuildable: [false],
        rebrickableListId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userSetLists$ = this.userSetListService.GetUserSetListList();

  constructor(
    private modalService: NgbModal,
    private userSetListService: UserSetListService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userSetListData?: UserSetListData) {

    if (userSetListData != null) {

      if (!this.userSetListService.userIsBMCUserSetListReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Set Lists`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userSetListSubmitData = this.userSetListService.ConvertToUserSetListSubmitData(userSetListData);
      this.isEditMode = true;
      this.objectGuid = userSetListData.objectGuid;

      this.buildFormValues(userSetListData);

    } else {

      if (!this.userSetListService.userIsBMCUserSetListWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Set Lists`,
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
        this.userSetListForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userSetListForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userSetListModal, {
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

    if (this.userSetListService.userIsBMCUserSetListWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Set Lists`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userSetListForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userSetListForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userSetListForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userSetListSubmitData: UserSetListSubmitData = {
        id: this.userSetListSubmitData?.id || 0,
        name: formValue.name!.trim(),
        isBuildable: !!formValue.isBuildable,
        rebrickableListId: formValue.rebrickableListId ? Number(formValue.rebrickableListId) : null,
        versionNumber: this.userSetListSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserSetList(userSetListSubmitData);
      } else {
        this.addUserSetList(userSetListSubmitData);
      }
  }

  private addUserSetList(userSetListData: UserSetListSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userSetListData.versionNumber = 0;
    userSetListData.active = true;
    userSetListData.deleted = false;
    this.userSetListService.PostUserSetList(userSetListData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserSetList) => {

        this.userSetListService.ClearAllCaches();

        this.userSetListChanged.next([newUserSetList]);

        this.alertService.showMessage("User Set List added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usersetlist', newUserSetList.id]);
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
                                   'You do not have permission to save this User Set List.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Set List.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Set List could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserSetList(userSetListData: UserSetListSubmitData) {
    this.userSetListService.PutUserSetList(userSetListData.id, userSetListData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserSetList) => {

        this.userSetListService.ClearAllCaches();

        this.userSetListChanged.next([updatedUserSetList]);

        this.alertService.showMessage("User Set List updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Set List.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Set List.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Set List could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userSetListData: UserSetListData | null) {

    if (userSetListData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userSetListForm.reset({
        name: '',
        isBuildable: false,
        rebrickableListId: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userSetListForm.reset({
        name: userSetListData.name ?? '',
        isBuildable: userSetListData.isBuildable ?? false,
        rebrickableListId: userSetListData.rebrickableListId?.toString() ?? '',
        versionNumber: userSetListData.versionNumber?.toString() ?? '',
        active: userSetListData.active ?? true,
        deleted: userSetListData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userSetListForm.markAsPristine();
    this.userSetListForm.markAsUntouched();
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


  public userIsBMCUserSetListReader(): boolean {
    return this.userSetListService.userIsBMCUserSetListReader();
  }

  public userIsBMCUserSetListWriter(): boolean {
    return this.userSetListService.userIsBMCUserSetListWriter();
  }
}

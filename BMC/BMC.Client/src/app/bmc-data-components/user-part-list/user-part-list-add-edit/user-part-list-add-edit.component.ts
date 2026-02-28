/*
   GENERATED FORM FOR THE USERPARTLIST TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserPartList table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-part-list-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserPartListService, UserPartListData, UserPartListSubmitData } from '../../../bmc-data-services/user-part-list.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserPartListFormValues {
  name: string,
  isBuildable: boolean,
  rebrickableListId: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-part-list-add-edit',
  templateUrl: './user-part-list-add-edit.component.html',
  styleUrls: ['./user-part-list-add-edit.component.scss']
})
export class UserPartListAddEditComponent {
  @ViewChild('userPartListModal') userPartListModal!: TemplateRef<any>;
  @Output() userPartListChanged = new Subject<UserPartListData[]>();
  @Input() userPartListSubmitData: UserPartListSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserPartListFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userPartListForm: FormGroup = this.fb.group({
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

  userPartLists$ = this.userPartListService.GetUserPartListList();

  constructor(
    private modalService: NgbModal,
    private userPartListService: UserPartListService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userPartListData?: UserPartListData) {

    if (userPartListData != null) {

      if (!this.userPartListService.userIsBMCUserPartListReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Part Lists`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userPartListSubmitData = this.userPartListService.ConvertToUserPartListSubmitData(userPartListData);
      this.isEditMode = true;
      this.objectGuid = userPartListData.objectGuid;

      this.buildFormValues(userPartListData);

    } else {

      if (!this.userPartListService.userIsBMCUserPartListWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Part Lists`,
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
        this.userPartListForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userPartListForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userPartListModal, {
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

    if (this.userPartListService.userIsBMCUserPartListWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Part Lists`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userPartListForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userPartListForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userPartListForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userPartListSubmitData: UserPartListSubmitData = {
        id: this.userPartListSubmitData?.id || 0,
        name: formValue.name!.trim(),
        isBuildable: !!formValue.isBuildable,
        rebrickableListId: formValue.rebrickableListId ? Number(formValue.rebrickableListId) : null,
        versionNumber: this.userPartListSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserPartList(userPartListSubmitData);
      } else {
        this.addUserPartList(userPartListSubmitData);
      }
  }

  private addUserPartList(userPartListData: UserPartListSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userPartListData.versionNumber = 0;
    userPartListData.active = true;
    userPartListData.deleted = false;
    this.userPartListService.PostUserPartList(userPartListData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserPartList) => {

        this.userPartListService.ClearAllCaches();

        this.userPartListChanged.next([newUserPartList]);

        this.alertService.showMessage("User Part List added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userpartlist', newUserPartList.id]);
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
                                   'You do not have permission to save this User Part List.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Part List.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Part List could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserPartList(userPartListData: UserPartListSubmitData) {
    this.userPartListService.PutUserPartList(userPartListData.id, userPartListData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserPartList) => {

        this.userPartListService.ClearAllCaches();

        this.userPartListChanged.next([updatedUserPartList]);

        this.alertService.showMessage("User Part List updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Part List.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Part List.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Part List could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userPartListData: UserPartListData | null) {

    if (userPartListData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userPartListForm.reset({
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
        this.userPartListForm.reset({
        name: userPartListData.name ?? '',
        isBuildable: userPartListData.isBuildable ?? false,
        rebrickableListId: userPartListData.rebrickableListId?.toString() ?? '',
        versionNumber: userPartListData.versionNumber?.toString() ?? '',
        active: userPartListData.active ?? true,
        deleted: userPartListData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userPartListForm.markAsPristine();
    this.userPartListForm.markAsUntouched();
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


  public userIsBMCUserPartListReader(): boolean {
    return this.userPartListService.userIsBMCUserPartListReader();
  }

  public userIsBMCUserPartListWriter(): boolean {
    return this.userPartListService.userIsBMCUserPartListWriter();
  }
}

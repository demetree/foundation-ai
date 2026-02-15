/*
   GENERATED FORM FOR THE USERCOLLECTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserCollection table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-collection-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserCollectionService, UserCollectionData, UserCollectionSubmitData } from '../../../bmc-data-services/user-collection.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserCollectionFormValues {
  name: string,
  description: string,
  isDefault: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-collection-add-edit',
  templateUrl: './user-collection-add-edit.component.html',
  styleUrls: ['./user-collection-add-edit.component.scss']
})
export class UserCollectionAddEditComponent {
  @ViewChild('userCollectionModal') userCollectionModal!: TemplateRef<any>;
  @Output() userCollectionChanged = new Subject<UserCollectionData[]>();
  @Input() userCollectionSubmitData: UserCollectionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserCollectionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userCollectionForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isDefault: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userCollections$ = this.userCollectionService.GetUserCollectionList();

  constructor(
    private modalService: NgbModal,
    private userCollectionService: UserCollectionService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userCollectionData?: UserCollectionData) {

    if (userCollectionData != null) {

      if (!this.userCollectionService.userIsBMCUserCollectionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Collections`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userCollectionSubmitData = this.userCollectionService.ConvertToUserCollectionSubmitData(userCollectionData);
      this.isEditMode = true;
      this.objectGuid = userCollectionData.objectGuid;

      this.buildFormValues(userCollectionData);

    } else {

      if (!this.userCollectionService.userIsBMCUserCollectionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Collections`,
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
        this.userCollectionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userCollectionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userCollectionModal, {
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

    if (this.userCollectionService.userIsBMCUserCollectionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Collections`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userCollectionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userCollectionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userCollectionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userCollectionSubmitData: UserCollectionSubmitData = {
        id: this.userCollectionSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isDefault: !!formValue.isDefault,
        versionNumber: this.userCollectionSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserCollection(userCollectionSubmitData);
      } else {
        this.addUserCollection(userCollectionSubmitData);
      }
  }

  private addUserCollection(userCollectionData: UserCollectionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userCollectionData.versionNumber = 0;
    userCollectionData.active = true;
    userCollectionData.deleted = false;
    this.userCollectionService.PostUserCollection(userCollectionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserCollection) => {

        this.userCollectionService.ClearAllCaches();

        this.userCollectionChanged.next([newUserCollection]);

        this.alertService.showMessage("User Collection added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usercollection', newUserCollection.id]);
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
                                   'You do not have permission to save this User Collection.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserCollection(userCollectionData: UserCollectionSubmitData) {
    this.userCollectionService.PutUserCollection(userCollectionData.id, userCollectionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserCollection) => {

        this.userCollectionService.ClearAllCaches();

        this.userCollectionChanged.next([updatedUserCollection]);

        this.alertService.showMessage("User Collection updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Collection.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userCollectionData: UserCollectionData | null) {

    if (userCollectionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userCollectionForm.reset({
        name: '',
        description: '',
        isDefault: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userCollectionForm.reset({
        name: userCollectionData.name ?? '',
        description: userCollectionData.description ?? '',
        isDefault: userCollectionData.isDefault ?? false,
        versionNumber: userCollectionData.versionNumber?.toString() ?? '',
        active: userCollectionData.active ?? true,
        deleted: userCollectionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userCollectionForm.markAsPristine();
    this.userCollectionForm.markAsUntouched();
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


  public userIsBMCUserCollectionReader(): boolean {
    return this.userCollectionService.userIsBMCUserCollectionReader();
  }

  public userIsBMCUserCollectionWriter(): boolean {
    return this.userCollectionService.userIsBMCUserCollectionWriter();
  }
}

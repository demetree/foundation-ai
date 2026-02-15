/*
   GENERATED FORM FOR THE USERCOLLECTIONPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserCollectionPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-collection-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserCollectionPartService, UserCollectionPartData, UserCollectionPartSubmitData } from '../../../bmc-data-services/user-collection-part.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserCollectionService } from '../../../bmc-data-services/user-collection.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserCollectionPartFormValues {
  userCollectionId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  quantityOwned: string | null,     // Stored as string for form input, converted to number on submit.
  quantityUsed: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-collection-part-add-edit',
  templateUrl: './user-collection-part-add-edit.component.html',
  styleUrls: ['./user-collection-part-add-edit.component.scss']
})
export class UserCollectionPartAddEditComponent {
  @ViewChild('userCollectionPartModal') userCollectionPartModal!: TemplateRef<any>;
  @Output() userCollectionPartChanged = new Subject<UserCollectionPartData[]>();
  @Input() userCollectionPartSubmitData: UserCollectionPartSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserCollectionPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userCollectionPartForm: FormGroup = this.fb.group({
        userCollectionId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        quantityOwned: [''],
        quantityUsed: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userCollectionParts$ = this.userCollectionPartService.GetUserCollectionPartList();
  userCollections$ = this.userCollectionService.GetUserCollectionList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private userCollectionPartService: UserCollectionPartService,
    private userCollectionService: UserCollectionService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userCollectionPartData?: UserCollectionPartData) {

    if (userCollectionPartData != null) {

      if (!this.userCollectionPartService.userIsBMCUserCollectionPartReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Collection Parts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userCollectionPartSubmitData = this.userCollectionPartService.ConvertToUserCollectionPartSubmitData(userCollectionPartData);
      this.isEditMode = true;
      this.objectGuid = userCollectionPartData.objectGuid;

      this.buildFormValues(userCollectionPartData);

    } else {

      if (!this.userCollectionPartService.userIsBMCUserCollectionPartWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Collection Parts`,
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
        this.userCollectionPartForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userCollectionPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userCollectionPartModal, {
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

    if (this.userCollectionPartService.userIsBMCUserCollectionPartWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Collection Parts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userCollectionPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userCollectionPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userCollectionPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userCollectionPartSubmitData: UserCollectionPartSubmitData = {
        id: this.userCollectionPartSubmitData?.id || 0,
        userCollectionId: Number(formValue.userCollectionId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        quantityOwned: formValue.quantityOwned ? Number(formValue.quantityOwned) : null,
        quantityUsed: formValue.quantityUsed ? Number(formValue.quantityUsed) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserCollectionPart(userCollectionPartSubmitData);
      } else {
        this.addUserCollectionPart(userCollectionPartSubmitData);
      }
  }

  private addUserCollectionPart(userCollectionPartData: UserCollectionPartSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userCollectionPartData.active = true;
    userCollectionPartData.deleted = false;
    this.userCollectionPartService.PostUserCollectionPart(userCollectionPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserCollectionPart) => {

        this.userCollectionPartService.ClearAllCaches();

        this.userCollectionPartChanged.next([newUserCollectionPart]);

        this.alertService.showMessage("User Collection Part added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usercollectionpart', newUserCollectionPart.id]);
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
                                   'You do not have permission to save this User Collection Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserCollectionPart(userCollectionPartData: UserCollectionPartSubmitData) {
    this.userCollectionPartService.PutUserCollectionPart(userCollectionPartData.id, userCollectionPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserCollectionPart) => {

        this.userCollectionPartService.ClearAllCaches();

        this.userCollectionPartChanged.next([updatedUserCollectionPart]);

        this.alertService.showMessage("User Collection Part updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Collection Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userCollectionPartData: UserCollectionPartData | null) {

    if (userCollectionPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userCollectionPartForm.reset({
        userCollectionId: null,
        brickPartId: null,
        brickColourId: null,
        quantityOwned: '',
        quantityUsed: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userCollectionPartForm.reset({
        userCollectionId: userCollectionPartData.userCollectionId,
        brickPartId: userCollectionPartData.brickPartId,
        brickColourId: userCollectionPartData.brickColourId,
        quantityOwned: userCollectionPartData.quantityOwned?.toString() ?? '',
        quantityUsed: userCollectionPartData.quantityUsed?.toString() ?? '',
        active: userCollectionPartData.active ?? true,
        deleted: userCollectionPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userCollectionPartForm.markAsPristine();
    this.userCollectionPartForm.markAsUntouched();
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


  public userIsBMCUserCollectionPartReader(): boolean {
    return this.userCollectionPartService.userIsBMCUserCollectionPartReader();
  }

  public userIsBMCUserCollectionPartWriter(): boolean {
    return this.userCollectionPartService.userIsBMCUserCollectionPartWriter();
  }
}

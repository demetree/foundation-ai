/*
   GENERATED FORM FOR THE USERWISHLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserWishlistItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-wishlist-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserWishlistItemService, UserWishlistItemData, UserWishlistItemSubmitData } from '../../../bmc-data-services/user-wishlist-item.service';
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
interface UserWishlistItemFormValues {
  userCollectionId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint | null,       // For FK link number
  quantityDesired: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-wishlist-item-add-edit',
  templateUrl: './user-wishlist-item-add-edit.component.html',
  styleUrls: ['./user-wishlist-item-add-edit.component.scss']
})
export class UserWishlistItemAddEditComponent {
  @ViewChild('userWishlistItemModal') userWishlistItemModal!: TemplateRef<any>;
  @Output() userWishlistItemChanged = new Subject<UserWishlistItemData[]>();
  @Input() userWishlistItemSubmitData: UserWishlistItemSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserWishlistItemFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userWishlistItemForm: FormGroup = this.fb.group({
        userCollectionId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null],
        quantityDesired: [''],
        notes: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userWishlistItems$ = this.userWishlistItemService.GetUserWishlistItemList();
  userCollections$ = this.userCollectionService.GetUserCollectionList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private userWishlistItemService: UserWishlistItemService,
    private userCollectionService: UserCollectionService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userWishlistItemData?: UserWishlistItemData) {

    if (userWishlistItemData != null) {

      if (!this.userWishlistItemService.userIsBMCUserWishlistItemReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Wishlist Items`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userWishlistItemSubmitData = this.userWishlistItemService.ConvertToUserWishlistItemSubmitData(userWishlistItemData);
      this.isEditMode = true;
      this.objectGuid = userWishlistItemData.objectGuid;

      this.buildFormValues(userWishlistItemData);

    } else {

      if (!this.userWishlistItemService.userIsBMCUserWishlistItemWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Wishlist Items`,
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
        this.userWishlistItemForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userWishlistItemForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userWishlistItemModal, {
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

    if (this.userWishlistItemService.userIsBMCUserWishlistItemWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Wishlist Items`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userWishlistItemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userWishlistItemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userWishlistItemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userWishlistItemSubmitData: UserWishlistItemSubmitData = {
        id: this.userWishlistItemSubmitData?.id || 0,
        userCollectionId: Number(formValue.userCollectionId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: formValue.brickColourId ? Number(formValue.brickColourId) : null,
        quantityDesired: formValue.quantityDesired ? Number(formValue.quantityDesired) : null,
        notes: formValue.notes?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserWishlistItem(userWishlistItemSubmitData);
      } else {
        this.addUserWishlistItem(userWishlistItemSubmitData);
      }
  }

  private addUserWishlistItem(userWishlistItemData: UserWishlistItemSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userWishlistItemData.active = true;
    userWishlistItemData.deleted = false;
    this.userWishlistItemService.PostUserWishlistItem(userWishlistItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserWishlistItem) => {

        this.userWishlistItemService.ClearAllCaches();

        this.userWishlistItemChanged.next([newUserWishlistItem]);

        this.alertService.showMessage("User Wishlist Item added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userwishlistitem', newUserWishlistItem.id]);
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
                                   'You do not have permission to save this User Wishlist Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Wishlist Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Wishlist Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserWishlistItem(userWishlistItemData: UserWishlistItemSubmitData) {
    this.userWishlistItemService.PutUserWishlistItem(userWishlistItemData.id, userWishlistItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserWishlistItem) => {

        this.userWishlistItemService.ClearAllCaches();

        this.userWishlistItemChanged.next([updatedUserWishlistItem]);

        this.alertService.showMessage("User Wishlist Item updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Wishlist Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Wishlist Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Wishlist Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userWishlistItemData: UserWishlistItemData | null) {

    if (userWishlistItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userWishlistItemForm.reset({
        userCollectionId: null,
        brickPartId: null,
        brickColourId: null,
        quantityDesired: '',
        notes: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userWishlistItemForm.reset({
        userCollectionId: userWishlistItemData.userCollectionId,
        brickPartId: userWishlistItemData.brickPartId,
        brickColourId: userWishlistItemData.brickColourId,
        quantityDesired: userWishlistItemData.quantityDesired?.toString() ?? '',
        notes: userWishlistItemData.notes ?? '',
        active: userWishlistItemData.active ?? true,
        deleted: userWishlistItemData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userWishlistItemForm.markAsPristine();
    this.userWishlistItemForm.markAsUntouched();
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


  public userIsBMCUserWishlistItemReader(): boolean {
    return this.userWishlistItemService.userIsBMCUserWishlistItemReader();
  }

  public userIsBMCUserWishlistItemWriter(): boolean {
    return this.userWishlistItemService.userIsBMCUserWishlistItemWriter();
  }
}

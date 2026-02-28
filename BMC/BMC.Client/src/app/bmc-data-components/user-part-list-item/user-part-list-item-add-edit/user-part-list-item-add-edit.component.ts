/*
   GENERATED FORM FOR THE USERPARTLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserPartListItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-part-list-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserPartListItemService, UserPartListItemData, UserPartListItemSubmitData } from '../../../bmc-data-services/user-part-list-item.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserPartListService } from '../../../bmc-data-services/user-part-list.service';
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
interface UserPartListItemFormValues {
  userPartListId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  quantity: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-part-list-item-add-edit',
  templateUrl: './user-part-list-item-add-edit.component.html',
  styleUrls: ['./user-part-list-item-add-edit.component.scss']
})
export class UserPartListItemAddEditComponent {
  @ViewChild('userPartListItemModal') userPartListItemModal!: TemplateRef<any>;
  @Output() userPartListItemChanged = new Subject<UserPartListItemData[]>();
  @Input() userPartListItemSubmitData: UserPartListItemSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserPartListItemFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userPartListItemForm: FormGroup = this.fb.group({
        userPartListId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        quantity: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userPartListItems$ = this.userPartListItemService.GetUserPartListItemList();
  userPartLists$ = this.userPartListService.GetUserPartListList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();

  constructor(
    private modalService: NgbModal,
    private userPartListItemService: UserPartListItemService,
    private userPartListService: UserPartListService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userPartListItemData?: UserPartListItemData) {

    if (userPartListItemData != null) {

      if (!this.userPartListItemService.userIsBMCUserPartListItemReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Part List Items`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userPartListItemSubmitData = this.userPartListItemService.ConvertToUserPartListItemSubmitData(userPartListItemData);
      this.isEditMode = true;
      this.objectGuid = userPartListItemData.objectGuid;

      this.buildFormValues(userPartListItemData);

    } else {

      if (!this.userPartListItemService.userIsBMCUserPartListItemWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Part List Items`,
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
        this.userPartListItemForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userPartListItemForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userPartListItemModal, {
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

    if (this.userPartListItemService.userIsBMCUserPartListItemWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Part List Items`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userPartListItemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userPartListItemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userPartListItemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userPartListItemSubmitData: UserPartListItemSubmitData = {
        id: this.userPartListItemSubmitData?.id || 0,
        userPartListId: Number(formValue.userPartListId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        quantity: Number(formValue.quantity),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserPartListItem(userPartListItemSubmitData);
      } else {
        this.addUserPartListItem(userPartListItemSubmitData);
      }
  }

  private addUserPartListItem(userPartListItemData: UserPartListItemSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userPartListItemData.active = true;
    userPartListItemData.deleted = false;
    this.userPartListItemService.PostUserPartListItem(userPartListItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserPartListItem) => {

        this.userPartListItemService.ClearAllCaches();

        this.userPartListItemChanged.next([newUserPartListItem]);

        this.alertService.showMessage("User Part List Item added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userpartlistitem', newUserPartListItem.id]);
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
                                   'You do not have permission to save this User Part List Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Part List Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Part List Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserPartListItem(userPartListItemData: UserPartListItemSubmitData) {
    this.userPartListItemService.PutUserPartListItem(userPartListItemData.id, userPartListItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserPartListItem) => {

        this.userPartListItemService.ClearAllCaches();

        this.userPartListItemChanged.next([updatedUserPartListItem]);

        this.alertService.showMessage("User Part List Item updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Part List Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Part List Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Part List Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userPartListItemData: UserPartListItemData | null) {

    if (userPartListItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userPartListItemForm.reset({
        userPartListId: null,
        brickPartId: null,
        brickColourId: null,
        quantity: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userPartListItemForm.reset({
        userPartListId: userPartListItemData.userPartListId,
        brickPartId: userPartListItemData.brickPartId,
        brickColourId: userPartListItemData.brickColourId,
        quantity: userPartListItemData.quantity?.toString() ?? '',
        active: userPartListItemData.active ?? true,
        deleted: userPartListItemData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userPartListItemForm.markAsPristine();
    this.userPartListItemForm.markAsUntouched();
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


  public userIsBMCUserPartListItemReader(): boolean {
    return this.userPartListItemService.userIsBMCUserPartListItemReader();
  }

  public userIsBMCUserPartListItemWriter(): boolean {
    return this.userPartListItemService.userIsBMCUserPartListItemWriter();
  }
}

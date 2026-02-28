/*
   GENERATED FORM FOR THE USERSETLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSetListItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-set-list-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserSetListItemService, UserSetListItemData, UserSetListItemSubmitData } from '../../../bmc-data-services/user-set-list-item.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserSetListService } from '../../../bmc-data-services/user-set-list.service';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserSetListItemFormValues {
  userSetListId: number | bigint,       // For FK link number
  legoSetId: number | bigint,       // For FK link number
  quantity: string,     // Stored as string for form input, converted to number on submit.
  includeSpares: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-set-list-item-add-edit',
  templateUrl: './user-set-list-item-add-edit.component.html',
  styleUrls: ['./user-set-list-item-add-edit.component.scss']
})
export class UserSetListItemAddEditComponent {
  @ViewChild('userSetListItemModal') userSetListItemModal!: TemplateRef<any>;
  @Output() userSetListItemChanged = new Subject<UserSetListItemData[]>();
  @Input() userSetListItemSubmitData: UserSetListItemSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserSetListItemFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userSetListItemForm: FormGroup = this.fb.group({
        userSetListId: [null, Validators.required],
        legoSetId: [null, Validators.required],
        quantity: ['', Validators.required],
        includeSpares: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userSetListItems$ = this.userSetListItemService.GetUserSetListItemList();
  userSetLists$ = this.userSetListService.GetUserSetListList();
  legoSets$ = this.legoSetService.GetLegoSetList();

  constructor(
    private modalService: NgbModal,
    private userSetListItemService: UserSetListItemService,
    private userSetListService: UserSetListService,
    private legoSetService: LegoSetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userSetListItemData?: UserSetListItemData) {

    if (userSetListItemData != null) {

      if (!this.userSetListItemService.userIsBMCUserSetListItemReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Set List Items`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userSetListItemSubmitData = this.userSetListItemService.ConvertToUserSetListItemSubmitData(userSetListItemData);
      this.isEditMode = true;
      this.objectGuid = userSetListItemData.objectGuid;

      this.buildFormValues(userSetListItemData);

    } else {

      if (!this.userSetListItemService.userIsBMCUserSetListItemWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Set List Items`,
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
        this.userSetListItemForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userSetListItemForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userSetListItemModal, {
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

    if (this.userSetListItemService.userIsBMCUserSetListItemWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Set List Items`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userSetListItemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userSetListItemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userSetListItemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userSetListItemSubmitData: UserSetListItemSubmitData = {
        id: this.userSetListItemSubmitData?.id || 0,
        userSetListId: Number(formValue.userSetListId),
        legoSetId: Number(formValue.legoSetId),
        quantity: Number(formValue.quantity),
        includeSpares: !!formValue.includeSpares,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserSetListItem(userSetListItemSubmitData);
      } else {
        this.addUserSetListItem(userSetListItemSubmitData);
      }
  }

  private addUserSetListItem(userSetListItemData: UserSetListItemSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userSetListItemData.active = true;
    userSetListItemData.deleted = false;
    this.userSetListItemService.PostUserSetListItem(userSetListItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserSetListItem) => {

        this.userSetListItemService.ClearAllCaches();

        this.userSetListItemChanged.next([newUserSetListItem]);

        this.alertService.showMessage("User Set List Item added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usersetlistitem', newUserSetListItem.id]);
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
                                   'You do not have permission to save this User Set List Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Set List Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Set List Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserSetListItem(userSetListItemData: UserSetListItemSubmitData) {
    this.userSetListItemService.PutUserSetListItem(userSetListItemData.id, userSetListItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserSetListItem) => {

        this.userSetListItemService.ClearAllCaches();

        this.userSetListItemChanged.next([updatedUserSetListItem]);

        this.alertService.showMessage("User Set List Item updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Set List Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Set List Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Set List Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userSetListItemData: UserSetListItemData | null) {

    if (userSetListItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userSetListItemForm.reset({
        userSetListId: null,
        legoSetId: null,
        quantity: '',
        includeSpares: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userSetListItemForm.reset({
        userSetListId: userSetListItemData.userSetListId,
        legoSetId: userSetListItemData.legoSetId,
        quantity: userSetListItemData.quantity?.toString() ?? '',
        includeSpares: userSetListItemData.includeSpares ?? false,
        active: userSetListItemData.active ?? true,
        deleted: userSetListItemData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userSetListItemForm.markAsPristine();
    this.userSetListItemForm.markAsUntouched();
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


  public userIsBMCUserSetListItemReader(): boolean {
    return this.userSetListItemService.userIsBMCUserSetListItemReader();
  }

  public userIsBMCUserSetListItemWriter(): boolean {
    return this.userSetListItemService.userIsBMCUserSetListItemWriter();
  }
}

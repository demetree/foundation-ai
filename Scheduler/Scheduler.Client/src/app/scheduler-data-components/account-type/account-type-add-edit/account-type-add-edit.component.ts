/*
   GENERATED FORM FOR THE ACCOUNTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AccountType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to account-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AccountTypeService, AccountTypeData, AccountTypeSubmitData } from '../../../scheduler-data-services/account-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AccountTypeFormValues {
  name: string,
  description: string,
  isRevenue: boolean,
  externalMapping: string | null,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-account-type-add-edit',
  templateUrl: './account-type-add-edit.component.html',
  styleUrls: ['./account-type-add-edit.component.scss']
})
export class AccountTypeAddEditComponent {
  @ViewChild('accountTypeModal') accountTypeModal!: TemplateRef<any>;
  @Output() accountTypeChanged = new Subject<AccountTypeData[]>();
  @Input() accountTypeSubmitData: AccountTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AccountTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public accountTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isRevenue: [false],
        externalMapping: [''],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  accountTypes$ = this.accountTypeService.GetAccountTypeList();

  constructor(
    private modalService: NgbModal,
    private accountTypeService: AccountTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(accountTypeData?: AccountTypeData) {

    if (accountTypeData != null) {

      if (!this.accountTypeService.userIsSchedulerAccountTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Account Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.accountTypeSubmitData = this.accountTypeService.ConvertToAccountTypeSubmitData(accountTypeData);
      this.isEditMode = true;
      this.objectGuid = accountTypeData.objectGuid;

      this.buildFormValues(accountTypeData);

    } else {

      if (!this.accountTypeService.userIsSchedulerAccountTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Account Types`,
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
        this.accountTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.accountTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.accountTypeModal, {
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

    if (this.accountTypeService.userIsSchedulerAccountTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Account Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.accountTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.accountTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.accountTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const accountTypeSubmitData: AccountTypeSubmitData = {
        id: this.accountTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isRevenue: !!formValue.isRevenue,
        externalMapping: formValue.externalMapping?.trim() || null,
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateAccountType(accountTypeSubmitData);
      } else {
        this.addAccountType(accountTypeSubmitData);
      }
  }

  private addAccountType(accountTypeData: AccountTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    accountTypeData.active = true;
    accountTypeData.deleted = false;
    this.accountTypeService.PostAccountType(accountTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAccountType) => {

        this.accountTypeService.ClearAllCaches();

        this.accountTypeChanged.next([newAccountType]);

        this.alertService.showMessage("Account Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/accounttype', newAccountType.id]);
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
                                   'You do not have permission to save this Account Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Account Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Account Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAccountType(accountTypeData: AccountTypeSubmitData) {
    this.accountTypeService.PutAccountType(accountTypeData.id, accountTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAccountType) => {

        this.accountTypeService.ClearAllCaches();

        this.accountTypeChanged.next([updatedAccountType]);

        this.alertService.showMessage("Account Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Account Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Account Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Account Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(accountTypeData: AccountTypeData | null) {

    if (accountTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.accountTypeForm.reset({
        name: '',
        description: '',
        isRevenue: false,
        externalMapping: '',
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.accountTypeForm.reset({
        name: accountTypeData.name ?? '',
        description: accountTypeData.description ?? '',
        isRevenue: accountTypeData.isRevenue ?? false,
        externalMapping: accountTypeData.externalMapping ?? '',
        color: accountTypeData.color ?? '',
        sequence: accountTypeData.sequence?.toString() ?? '',
        active: accountTypeData.active ?? true,
        deleted: accountTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.accountTypeForm.markAsPristine();
    this.accountTypeForm.markAsUntouched();
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


  public userIsSchedulerAccountTypeReader(): boolean {
    return this.accountTypeService.userIsSchedulerAccountTypeReader();
  }

  public userIsSchedulerAccountTypeWriter(): boolean {
    return this.accountTypeService.userIsSchedulerAccountTypeWriter();
  }
}

/*
   GENERATED FORM FOR THE USERCOLLECTIONSETIMPORT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserCollectionSetImport table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-collection-set-import-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserCollectionSetImportService, UserCollectionSetImportData, UserCollectionSetImportSubmitData } from '../../../bmc-data-services/user-collection-set-import.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { UserCollectionService } from '../../../bmc-data-services/user-collection.service';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserCollectionSetImportFormValues {
  userCollectionId: number | bigint,       // For FK link number
  legoSetId: number | bigint,       // For FK link number
  quantity: string | null,     // Stored as string for form input, converted to number on submit.
  importedDate: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-collection-set-import-add-edit',
  templateUrl: './user-collection-set-import-add-edit.component.html',
  styleUrls: ['./user-collection-set-import-add-edit.component.scss']
})
export class UserCollectionSetImportAddEditComponent {
  @ViewChild('userCollectionSetImportModal') userCollectionSetImportModal!: TemplateRef<any>;
  @Output() userCollectionSetImportChanged = new Subject<UserCollectionSetImportData[]>();
  @Input() userCollectionSetImportSubmitData: UserCollectionSetImportSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserCollectionSetImportFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userCollectionSetImportForm: FormGroup = this.fb.group({
        userCollectionId: [null, Validators.required],
        legoSetId: [null, Validators.required],
        quantity: [''],
        importedDate: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userCollectionSetImports$ = this.userCollectionSetImportService.GetUserCollectionSetImportList();
  userCollections$ = this.userCollectionService.GetUserCollectionList();
  legoSets$ = this.legoSetService.GetLegoSetList();

  constructor(
    private modalService: NgbModal,
    private userCollectionSetImportService: UserCollectionSetImportService,
    private userCollectionService: UserCollectionService,
    private legoSetService: LegoSetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userCollectionSetImportData?: UserCollectionSetImportData) {

    if (userCollectionSetImportData != null) {

      if (!this.userCollectionSetImportService.userIsBMCUserCollectionSetImportReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Collection Set Imports`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userCollectionSetImportSubmitData = this.userCollectionSetImportService.ConvertToUserCollectionSetImportSubmitData(userCollectionSetImportData);
      this.isEditMode = true;
      this.objectGuid = userCollectionSetImportData.objectGuid;

      this.buildFormValues(userCollectionSetImportData);

    } else {

      if (!this.userCollectionSetImportService.userIsBMCUserCollectionSetImportWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Collection Set Imports`,
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
        this.userCollectionSetImportForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userCollectionSetImportForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userCollectionSetImportModal, {
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

    if (this.userCollectionSetImportService.userIsBMCUserCollectionSetImportWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Collection Set Imports`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userCollectionSetImportForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userCollectionSetImportForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userCollectionSetImportForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userCollectionSetImportSubmitData: UserCollectionSetImportSubmitData = {
        id: this.userCollectionSetImportSubmitData?.id || 0,
        userCollectionId: Number(formValue.userCollectionId),
        legoSetId: Number(formValue.legoSetId),
        quantity: formValue.quantity ? Number(formValue.quantity) : null,
        importedDate: formValue.importedDate ? dateTimeLocalToIsoUtc(formValue.importedDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserCollectionSetImport(userCollectionSetImportSubmitData);
      } else {
        this.addUserCollectionSetImport(userCollectionSetImportSubmitData);
      }
  }

  private addUserCollectionSetImport(userCollectionSetImportData: UserCollectionSetImportSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userCollectionSetImportData.active = true;
    userCollectionSetImportData.deleted = false;
    this.userCollectionSetImportService.PostUserCollectionSetImport(userCollectionSetImportData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserCollectionSetImport) => {

        this.userCollectionSetImportService.ClearAllCaches();

        this.userCollectionSetImportChanged.next([newUserCollectionSetImport]);

        this.alertService.showMessage("User Collection Set Import added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usercollectionsetimport', newUserCollectionSetImport.id]);
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
                                   'You do not have permission to save this User Collection Set Import.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection Set Import.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection Set Import could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserCollectionSetImport(userCollectionSetImportData: UserCollectionSetImportSubmitData) {
    this.userCollectionSetImportService.PutUserCollectionSetImport(userCollectionSetImportData.id, userCollectionSetImportData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserCollectionSetImport) => {

        this.userCollectionSetImportService.ClearAllCaches();

        this.userCollectionSetImportChanged.next([updatedUserCollectionSetImport]);

        this.alertService.showMessage("User Collection Set Import updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Collection Set Import.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection Set Import.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection Set Import could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userCollectionSetImportData: UserCollectionSetImportData | null) {

    if (userCollectionSetImportData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userCollectionSetImportForm.reset({
        userCollectionId: null,
        legoSetId: null,
        quantity: '',
        importedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userCollectionSetImportForm.reset({
        userCollectionId: userCollectionSetImportData.userCollectionId,
        legoSetId: userCollectionSetImportData.legoSetId,
        quantity: userCollectionSetImportData.quantity?.toString() ?? '',
        importedDate: isoUtcStringToDateTimeLocal(userCollectionSetImportData.importedDate) ?? '',
        active: userCollectionSetImportData.active ?? true,
        deleted: userCollectionSetImportData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userCollectionSetImportForm.markAsPristine();
    this.userCollectionSetImportForm.markAsUntouched();
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


  public userIsBMCUserCollectionSetImportReader(): boolean {
    return this.userCollectionSetImportService.userIsBMCUserCollectionSetImportReader();
  }

  public userIsBMCUserCollectionSetImportWriter(): boolean {
    return this.userCollectionSetImportService.userIsBMCUserCollectionSetImportWriter();
  }
}

/*
   GENERATED FORM FOR THE USERLOSTPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserLostPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-lost-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserLostPartService, UserLostPartData, UserLostPartSubmitData } from '../../../bmc-data-services/user-lost-part.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserLostPartFormValues {
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  legoSetId: number | bigint | null,       // For FK link number
  lostQuantity: string,     // Stored as string for form input, converted to number on submit.
  rebrickableInvPartId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-lost-part-add-edit',
  templateUrl: './user-lost-part-add-edit.component.html',
  styleUrls: ['./user-lost-part-add-edit.component.scss']
})
export class UserLostPartAddEditComponent {
  @ViewChild('userLostPartModal') userLostPartModal!: TemplateRef<any>;
  @Output() userLostPartChanged = new Subject<UserLostPartData[]>();
  @Input() userLostPartSubmitData: UserLostPartSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserLostPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userLostPartForm: FormGroup = this.fb.group({
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        legoSetId: [null],
        lostQuantity: ['', Validators.required],
        rebrickableInvPartId: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userLostParts$ = this.userLostPartService.GetUserLostPartList();
  brickParts$ = this.brickPartService.GetBrickPartList();
  brickColours$ = this.brickColourService.GetBrickColourList();
  legoSets$ = this.legoSetService.GetLegoSetList();

  constructor(
    private modalService: NgbModal,
    private userLostPartService: UserLostPartService,
    private brickPartService: BrickPartService,
    private brickColourService: BrickColourService,
    private legoSetService: LegoSetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userLostPartData?: UserLostPartData) {

    if (userLostPartData != null) {

      if (!this.userLostPartService.userIsBMCUserLostPartReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Lost Parts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userLostPartSubmitData = this.userLostPartService.ConvertToUserLostPartSubmitData(userLostPartData);
      this.isEditMode = true;
      this.objectGuid = userLostPartData.objectGuid;

      this.buildFormValues(userLostPartData);

    } else {

      if (!this.userLostPartService.userIsBMCUserLostPartWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Lost Parts`,
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
        this.userLostPartForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userLostPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userLostPartModal, {
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

    if (this.userLostPartService.userIsBMCUserLostPartWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Lost Parts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userLostPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userLostPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userLostPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userLostPartSubmitData: UserLostPartSubmitData = {
        id: this.userLostPartSubmitData?.id || 0,
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        legoSetId: formValue.legoSetId ? Number(formValue.legoSetId) : null,
        lostQuantity: Number(formValue.lostQuantity),
        rebrickableInvPartId: formValue.rebrickableInvPartId ? Number(formValue.rebrickableInvPartId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserLostPart(userLostPartSubmitData);
      } else {
        this.addUserLostPart(userLostPartSubmitData);
      }
  }

  private addUserLostPart(userLostPartData: UserLostPartSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userLostPartData.active = true;
    userLostPartData.deleted = false;
    this.userLostPartService.PostUserLostPart(userLostPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserLostPart) => {

        this.userLostPartService.ClearAllCaches();

        this.userLostPartChanged.next([newUserLostPart]);

        this.alertService.showMessage("User Lost Part added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userlostpart', newUserLostPart.id]);
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
                                   'You do not have permission to save this User Lost Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Lost Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Lost Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserLostPart(userLostPartData: UserLostPartSubmitData) {
    this.userLostPartService.PutUserLostPart(userLostPartData.id, userLostPartData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserLostPart) => {

        this.userLostPartService.ClearAllCaches();

        this.userLostPartChanged.next([updatedUserLostPart]);

        this.alertService.showMessage("User Lost Part updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Lost Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Lost Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Lost Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userLostPartData: UserLostPartData | null) {

    if (userLostPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userLostPartForm.reset({
        brickPartId: null,
        brickColourId: null,
        legoSetId: null,
        lostQuantity: '',
        rebrickableInvPartId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userLostPartForm.reset({
        brickPartId: userLostPartData.brickPartId,
        brickColourId: userLostPartData.brickColourId,
        legoSetId: userLostPartData.legoSetId,
        lostQuantity: userLostPartData.lostQuantity?.toString() ?? '',
        rebrickableInvPartId: userLostPartData.rebrickableInvPartId?.toString() ?? '',
        active: userLostPartData.active ?? true,
        deleted: userLostPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userLostPartForm.markAsPristine();
    this.userLostPartForm.markAsUntouched();
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


  public userIsBMCUserLostPartReader(): boolean {
    return this.userLostPartService.userIsBMCUserLostPartReader();
  }

  public userIsBMCUserLostPartWriter(): boolean {
    return this.userLostPartService.userIsBMCUserLostPartWriter();
  }
}

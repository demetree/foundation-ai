/*
   GENERATED FORM FOR THE LEGOSET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoSet table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-set-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoSetService, LegoSetData, LegoSetSubmitData } from '../../../bmc-data-services/lego-set.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { LegoThemeService } from '../../../bmc-data-services/lego-theme.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface LegoSetFormValues {
  name: string,
  setNumber: string,
  year: string,     // Stored as string for form input, converted to number on submit.
  partCount: string,     // Stored as string for form input, converted to number on submit.
  legoThemeId: number | bigint | null,       // For FK link number
  imageUrl: string | null,
  brickLinkUrl: string | null,
  rebrickableUrl: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-lego-set-add-edit',
  templateUrl: './lego-set-add-edit.component.html',
  styleUrls: ['./lego-set-add-edit.component.scss']
})
export class LegoSetAddEditComponent {
  @ViewChild('legoSetModal') legoSetModal!: TemplateRef<any>;
  @Output() legoSetChanged = new Subject<LegoSetData[]>();
  @Input() legoSetSubmitData: LegoSetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoSetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoSetForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        setNumber: ['', Validators.required],
        year: ['', Validators.required],
        partCount: ['', Validators.required],
        legoThemeId: [null],
        imageUrl: [''],
        brickLinkUrl: [''],
        rebrickableUrl: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  legoSets$ = this.legoSetService.GetLegoSetList();
  legoThemes$ = this.legoThemeService.GetLegoThemeList();

  constructor(
    private modalService: NgbModal,
    private legoSetService: LegoSetService,
    private legoThemeService: LegoThemeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(legoSetData?: LegoSetData) {

    if (legoSetData != null) {

      if (!this.legoSetService.userIsBMCLegoSetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Lego Sets`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.legoSetSubmitData = this.legoSetService.ConvertToLegoSetSubmitData(legoSetData);
      this.isEditMode = true;
      this.objectGuid = legoSetData.objectGuid;

      this.buildFormValues(legoSetData);

    } else {

      if (!this.legoSetService.userIsBMCLegoSetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Lego Sets`,
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
        this.legoSetForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoSetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.legoSetModal, {
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

    if (this.legoSetService.userIsBMCLegoSetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Lego Sets`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.legoSetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoSetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoSetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoSetSubmitData: LegoSetSubmitData = {
        id: this.legoSetSubmitData?.id || 0,
        name: formValue.name!.trim(),
        setNumber: formValue.setNumber!.trim(),
        year: Number(formValue.year),
        partCount: Number(formValue.partCount),
        legoThemeId: formValue.legoThemeId ? Number(formValue.legoThemeId) : null,
        imageUrl: formValue.imageUrl?.trim() || null,
        brickLinkUrl: formValue.brickLinkUrl?.trim() || null,
        rebrickableUrl: formValue.rebrickableUrl?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateLegoSet(legoSetSubmitData);
      } else {
        this.addLegoSet(legoSetSubmitData);
      }
  }

  private addLegoSet(legoSetData: LegoSetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    legoSetData.active = true;
    legoSetData.deleted = false;
    this.legoSetService.PostLegoSet(legoSetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newLegoSet) => {

        this.legoSetService.ClearAllCaches();

        this.legoSetChanged.next([newLegoSet]);

        this.alertService.showMessage("Lego Set added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/legoset', newLegoSet.id]);
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
                                   'You do not have permission to save this Lego Set.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateLegoSet(legoSetData: LegoSetSubmitData) {
    this.legoSetService.PutLegoSet(legoSetData.id, legoSetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedLegoSet) => {

        this.legoSetService.ClearAllCaches();

        this.legoSetChanged.next([updatedLegoSet]);

        this.alertService.showMessage("Lego Set updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Lego Set.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(legoSetData: LegoSetData | null) {

    if (legoSetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoSetForm.reset({
        name: '',
        setNumber: '',
        year: '',
        partCount: '',
        legoThemeId: null,
        imageUrl: '',
        brickLinkUrl: '',
        rebrickableUrl: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoSetForm.reset({
        name: legoSetData.name ?? '',
        setNumber: legoSetData.setNumber ?? '',
        year: legoSetData.year?.toString() ?? '',
        partCount: legoSetData.partCount?.toString() ?? '',
        legoThemeId: legoSetData.legoThemeId,
        imageUrl: legoSetData.imageUrl ?? '',
        brickLinkUrl: legoSetData.brickLinkUrl ?? '',
        rebrickableUrl: legoSetData.rebrickableUrl ?? '',
        active: legoSetData.active ?? true,
        deleted: legoSetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoSetForm.markAsPristine();
    this.legoSetForm.markAsUntouched();
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


  public userIsBMCLegoSetReader(): boolean {
    return this.legoSetService.userIsBMCLegoSetReader();
  }

  public userIsBMCLegoSetWriter(): boolean {
    return this.legoSetService.userIsBMCLegoSetWriter();
  }
}

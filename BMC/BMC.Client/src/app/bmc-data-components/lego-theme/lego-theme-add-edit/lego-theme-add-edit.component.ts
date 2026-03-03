/*
   GENERATED FORM FOR THE LEGOTHEME TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoTheme table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-theme-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoThemeService, LegoThemeData, LegoThemeSubmitData } from '../../../bmc-data-services/lego-theme.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface LegoThemeFormValues {
  name: string,
  description: string,
  legoThemeId: number | bigint | null,       // For FK link number
  rebrickableThemeId: string,     // Stored as string for form input, converted to number on submit.
  brickSetThemeName: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-lego-theme-add-edit',
  templateUrl: './lego-theme-add-edit.component.html',
  styleUrls: ['./lego-theme-add-edit.component.scss']
})
export class LegoThemeAddEditComponent {
  @ViewChild('legoThemeModal') legoThemeModal!: TemplateRef<any>;
  @Output() legoThemeChanged = new Subject<LegoThemeData[]>();
  @Input() legoThemeSubmitData: LegoThemeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoThemeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoThemeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        legoThemeId: [null],
        rebrickableThemeId: ['', Validators.required],
        brickSetThemeName: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  legoThemes$ = this.legoThemeService.GetLegoThemeList();

  constructor(
    private modalService: NgbModal,
    private legoThemeService: LegoThemeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(legoThemeData?: LegoThemeData) {

    if (legoThemeData != null) {

      if (!this.legoThemeService.userIsBMCLegoThemeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Lego Themes`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.legoThemeSubmitData = this.legoThemeService.ConvertToLegoThemeSubmitData(legoThemeData);
      this.isEditMode = true;
      this.objectGuid = legoThemeData.objectGuid;

      this.buildFormValues(legoThemeData);

    } else {

      if (!this.legoThemeService.userIsBMCLegoThemeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Lego Themes`,
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
        this.legoThemeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoThemeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.legoThemeModal, {
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

    if (this.legoThemeService.userIsBMCLegoThemeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Lego Themes`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.legoThemeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoThemeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoThemeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoThemeSubmitData: LegoThemeSubmitData = {
        id: this.legoThemeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        legoThemeId: formValue.legoThemeId ? Number(formValue.legoThemeId) : null,
        rebrickableThemeId: Number(formValue.rebrickableThemeId),
        brickSetThemeName: formValue.brickSetThemeName?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateLegoTheme(legoThemeSubmitData);
      } else {
        this.addLegoTheme(legoThemeSubmitData);
      }
  }

  private addLegoTheme(legoThemeData: LegoThemeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    legoThemeData.active = true;
    legoThemeData.deleted = false;
    this.legoThemeService.PostLegoTheme(legoThemeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newLegoTheme) => {

        this.legoThemeService.ClearAllCaches();

        this.legoThemeChanged.next([newLegoTheme]);

        this.alertService.showMessage("Lego Theme added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/legotheme', newLegoTheme.id]);
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
                                   'You do not have permission to save this Lego Theme.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Theme.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Theme could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateLegoTheme(legoThemeData: LegoThemeSubmitData) {
    this.legoThemeService.PutLegoTheme(legoThemeData.id, legoThemeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedLegoTheme) => {

        this.legoThemeService.ClearAllCaches();

        this.legoThemeChanged.next([updatedLegoTheme]);

        this.alertService.showMessage("Lego Theme updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Lego Theme.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Theme.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Theme could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(legoThemeData: LegoThemeData | null) {

    if (legoThemeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoThemeForm.reset({
        name: '',
        description: '',
        legoThemeId: null,
        rebrickableThemeId: '',
        brickSetThemeName: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoThemeForm.reset({
        name: legoThemeData.name ?? '',
        description: legoThemeData.description ?? '',
        legoThemeId: legoThemeData.legoThemeId,
        rebrickableThemeId: legoThemeData.rebrickableThemeId?.toString() ?? '',
        brickSetThemeName: legoThemeData.brickSetThemeName ?? '',
        sequence: legoThemeData.sequence?.toString() ?? '',
        active: legoThemeData.active ?? true,
        deleted: legoThemeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoThemeForm.markAsPristine();
    this.legoThemeForm.markAsUntouched();
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


  public userIsBMCLegoThemeReader(): boolean {
    return this.legoThemeService.userIsBMCLegoThemeReader();
  }

  public userIsBMCLegoThemeWriter(): boolean {
    return this.legoThemeService.userIsBMCLegoThemeWriter();
  }
}

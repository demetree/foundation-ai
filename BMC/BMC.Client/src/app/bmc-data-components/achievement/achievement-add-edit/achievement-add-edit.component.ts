/*
   GENERATED FORM FOR THE ACHIEVEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Achievement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to achievement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AchievementService, AchievementData, AchievementSubmitData } from '../../../bmc-data-services/achievement.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AchievementCategoryService } from '../../../bmc-data-services/achievement-category.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AchievementFormValues {
  achievementCategoryId: number | bigint,       // For FK link number
  name: string,
  description: string,
  iconCssClass: string | null,
  iconImagePath: string | null,
  criteria: string | null,
  criteriaCode: string | null,
  pointValue: string,     // Stored as string for form input, converted to number on submit.
  rarity: string,
  isActive: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-achievement-add-edit',
  templateUrl: './achievement-add-edit.component.html',
  styleUrls: ['./achievement-add-edit.component.scss']
})
export class AchievementAddEditComponent {
  @ViewChild('achievementModal') achievementModal!: TemplateRef<any>;
  @Output() achievementChanged = new Subject<AchievementData[]>();
  @Input() achievementSubmitData: AchievementSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AchievementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public achievementForm: FormGroup = this.fb.group({
        achievementCategoryId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        iconCssClass: [''],
        iconImagePath: [''],
        criteria: [''],
        criteriaCode: [''],
        pointValue: ['', Validators.required],
        rarity: ['', Validators.required],
        isActive: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  achievements$ = this.achievementService.GetAchievementList();
  achievementCategories$ = this.achievementCategoryService.GetAchievementCategoryList();

  constructor(
    private modalService: NgbModal,
    private achievementService: AchievementService,
    private achievementCategoryService: AchievementCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(achievementData?: AchievementData) {

    if (achievementData != null) {

      if (!this.achievementService.userIsBMCAchievementReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Achievements`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.achievementSubmitData = this.achievementService.ConvertToAchievementSubmitData(achievementData);
      this.isEditMode = true;
      this.objectGuid = achievementData.objectGuid;

      this.buildFormValues(achievementData);

    } else {

      if (!this.achievementService.userIsBMCAchievementWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Achievements`,
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
        this.achievementForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.achievementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.achievementModal, {
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

    if (this.achievementService.userIsBMCAchievementWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Achievements`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.achievementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.achievementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.achievementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const achievementSubmitData: AchievementSubmitData = {
        id: this.achievementSubmitData?.id || 0,
        achievementCategoryId: Number(formValue.achievementCategoryId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        iconCssClass: formValue.iconCssClass?.trim() || null,
        iconImagePath: formValue.iconImagePath?.trim() || null,
        criteria: formValue.criteria?.trim() || null,
        criteriaCode: formValue.criteriaCode?.trim() || null,
        pointValue: Number(formValue.pointValue),
        rarity: formValue.rarity!.trim(),
        isActive: !!formValue.isActive,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateAchievement(achievementSubmitData);
      } else {
        this.addAchievement(achievementSubmitData);
      }
  }

  private addAchievement(achievementData: AchievementSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    achievementData.active = true;
    achievementData.deleted = false;
    this.achievementService.PostAchievement(achievementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAchievement) => {

        this.achievementService.ClearAllCaches();

        this.achievementChanged.next([newAchievement]);

        this.alertService.showMessage("Achievement added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/achievement', newAchievement.id]);
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
                                   'You do not have permission to save this Achievement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Achievement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Achievement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAchievement(achievementData: AchievementSubmitData) {
    this.achievementService.PutAchievement(achievementData.id, achievementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAchievement) => {

        this.achievementService.ClearAllCaches();

        this.achievementChanged.next([updatedAchievement]);

        this.alertService.showMessage("Achievement updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Achievement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Achievement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Achievement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(achievementData: AchievementData | null) {

    if (achievementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.achievementForm.reset({
        achievementCategoryId: null,
        name: '',
        description: '',
        iconCssClass: '',
        iconImagePath: '',
        criteria: '',
        criteriaCode: '',
        pointValue: '',
        rarity: '',
        isActive: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.achievementForm.reset({
        achievementCategoryId: achievementData.achievementCategoryId,
        name: achievementData.name ?? '',
        description: achievementData.description ?? '',
        iconCssClass: achievementData.iconCssClass ?? '',
        iconImagePath: achievementData.iconImagePath ?? '',
        criteria: achievementData.criteria ?? '',
        criteriaCode: achievementData.criteriaCode ?? '',
        pointValue: achievementData.pointValue?.toString() ?? '',
        rarity: achievementData.rarity ?? '',
        isActive: achievementData.isActive ?? false,
        sequence: achievementData.sequence?.toString() ?? '',
        active: achievementData.active ?? true,
        deleted: achievementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.achievementForm.markAsPristine();
    this.achievementForm.markAsUntouched();
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


  public userIsBMCAchievementReader(): boolean {
    return this.achievementService.userIsBMCAchievementReader();
  }

  public userIsBMCAchievementWriter(): boolean {
    return this.achievementService.userIsBMCAchievementWriter();
  }
}

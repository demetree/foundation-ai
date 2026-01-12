/*
   GENERATED FORM FOR THE CONSTITUENTJOURNEYSTAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConstituentJourneyStage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to constituent-journey-stage-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConstituentJourneyStageService, ConstituentJourneyStageData, ConstituentJourneyStageSubmitData } from '../../../scheduler-data-services/constituent-journey-stage.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConstituentJourneyStageFormValues {
  name: string,
  description: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  minLifetimeGiving: string | null,     // Stored as string for form input, converted to number on submit.
  maxLifetimeGiving: string | null,     // Stored as string for form input, converted to number on submit.
  minSingleGiftAmount: string | null,     // Stored as string for form input, converted to number on submit.
  isDefault: boolean,
  minAnnualGiving: string | null,     // Stored as string for form input, converted to number on submit.
  maxDaysSinceLastGift: string | null,     // Stored as string for form input, converted to number on submit.
  minGiftCount: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-constituent-journey-stage-add-edit',
  templateUrl: './constituent-journey-stage-add-edit.component.html',
  styleUrls: ['./constituent-journey-stage-add-edit.component.scss']
})
export class ConstituentJourneyStageAddEditComponent {
  @ViewChild('constituentJourneyStageModal') constituentJourneyStageModal!: TemplateRef<any>;
  @Output() constituentJourneyStageChanged = new Subject<ConstituentJourneyStageData[]>();
  @Input() constituentJourneyStageSubmitData: ConstituentJourneyStageSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConstituentJourneyStageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public constituentJourneyStageForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    sequence: [''],
    iconId: [null],
    color: [''],
    minLifetimeGiving: [''],
    maxLifetimeGiving: [''],
    minSingleGiftAmount: [''],
    isDefault: [false],
    minAnnualGiving: [''],
    maxDaysSinceLastGift: [''],
    minGiftCount: [''],
    versionNumber: [''],
    active: [true],
    deleted: [false],
  });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;
  public ruleExplanation: string = '';

  constituentJourneyStages$ = this.constituentJourneyStageService.GetConstituentJourneyStageList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private constituentJourneyStageService: ConstituentJourneyStageService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
    this.constituentJourneyStageForm.valueChanges.subscribe(() => {
      this.updateRuleExplanation();
    });
  }

  private updateRuleExplanation() {
    const val = this.constituentJourneyStageForm.getRawValue();
    const parts: string[] = [];

    if (val.minAnnualGiving) {
      parts.push(`gave at least $${val.minAnnualGiving} in the last 365 days`);
    }

    if (val.maxDaysSinceLastGift) {
      parts.push(`gave a gift within the last ${val.maxDaysSinceLastGift} days`);
    }

    if (val.minGiftCount) {
      parts.push(`have given at least ${val.minGiftCount} total gifts`);
    }

    if (val.minLifetimeGiving) {
      parts.push(`have a total lifetime giving of at least $${val.minLifetimeGiving}`);
    }

    // Note: minSingleGiftAmount is often a specific criteria or part of a calculated metric not directly standard, 
    // but if it's there, let's include it.
    if (val.minSingleGiftAmount) {
      parts.push(`have given at least one gift of $${val.minSingleGiftAmount} or more`);
    }

    if (parts.length > 0) {
      this.ruleExplanation = "Donors will enter this stage if they " + parts.join(' AND ') + ".";
    } else {
      this.ruleExplanation = "No specific automatic entry rules set.";
    }
  }

  openModal(constituentJourneyStageData?: ConstituentJourneyStageData) {

    if (constituentJourneyStageData != null) {

      if (!this.constituentJourneyStageService.userIsSchedulerConstituentJourneyStageReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Constituent Journey Stages`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.constituentJourneyStageSubmitData = this.constituentJourneyStageService.ConvertToConstituentJourneyStageSubmitData(constituentJourneyStageData);
      this.isEditMode = true;
      this.objectGuid = constituentJourneyStageData.objectGuid;

      this.buildFormValues(constituentJourneyStageData);

    } else {

      if (!this.constituentJourneyStageService.userIsSchedulerConstituentJourneyStageWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Constituent Journey Stages`,
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
        this.constituentJourneyStageForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.constituentJourneyStageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.constituentJourneyStageModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;

    // Trigger initial explanation update
    this.updateRuleExplanation();
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

    if (this.constituentJourneyStageService.userIsSchedulerConstituentJourneyStageWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Constituent Journey Stages`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.constituentJourneyStageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.constituentJourneyStageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.constituentJourneyStageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const constituentJourneyStageSubmitData: ConstituentJourneyStageSubmitData = {
      id: this.constituentJourneyStageSubmitData?.id || 0,
      name: formValue.name!.trim(),
      description: formValue.description?.trim() || null,
      sequence: formValue.sequence ? Number(formValue.sequence) : null,
      iconId: formValue.iconId ? Number(formValue.iconId) : null,
      color: formValue.color?.trim() || null,
      minLifetimeGiving: formValue.minLifetimeGiving ? Number(formValue.minLifetimeGiving) : null,
      maxLifetimeGiving: formValue.maxLifetimeGiving ? Number(formValue.maxLifetimeGiving) : null,
      minSingleGiftAmount: formValue.minSingleGiftAmount ? Number(formValue.minSingleGiftAmount) : null,
      isDefault: !!formValue.isDefault,
      minAnnualGiving: formValue.minAnnualGiving ? Number(formValue.minAnnualGiving) : null,
      maxDaysSinceLastGift: formValue.maxDaysSinceLastGift ? Number(formValue.maxDaysSinceLastGift) : null,
      minGiftCount: formValue.minGiftCount ? Number(formValue.minGiftCount) : null,
      versionNumber: this.constituentJourneyStageSubmitData?.versionNumber ?? 0,
      active: !!formValue.active,
      deleted: !!formValue.deleted,
    };

    if (this.isEditMode) {
      this.updateConstituentJourneyStage(constituentJourneyStageSubmitData);
    } else {
      this.addConstituentJourneyStage(constituentJourneyStageSubmitData);
    }
  }

  private addConstituentJourneyStage(constituentJourneyStageData: ConstituentJourneyStageSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    constituentJourneyStageData.versionNumber = 0;
    constituentJourneyStageData.active = true;
    constituentJourneyStageData.deleted = false;
    this.constituentJourneyStageService.PostConstituentJourneyStage(constituentJourneyStageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConstituentJourneyStage) => {

        this.constituentJourneyStageService.ClearAllCaches();

        this.constituentJourneyStageChanged.next([newConstituentJourneyStage]);

        this.alertService.showMessage("Constituent Journey Stage added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/constituentjourneystage', newConstituentJourneyStage.id]);
        }
      },
      error: (err) => {
        let errorMessage: string;

        // Check if err is an Error object (e.g., new Error('message'))
        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        // Check if err is a ServerError object with status and error properties
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Constituent Journey Stage.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Constituent Journey Stage.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Constituent Journey Stage could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }


  private updateConstituentJourneyStage(constituentJourneyStageData: ConstituentJourneyStageSubmitData) {
    this.constituentJourneyStageService.PutConstituentJourneyStage(constituentJourneyStageData.id, constituentJourneyStageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConstituentJourneyStage) => {

        this.constituentJourneyStageService.ClearAllCaches();

        this.constituentJourneyStageChanged.next([updatedConstituentJourneyStage]);

        this.alertService.showMessage("Constituent Journey Stage updated successfully", '', MessageSeverity.success);

        this.closeModal();
      },
      error: (err) => {
        let errorMessage: string;

        // Check if err is an Error object (e.g., new Error('message'))
        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        // Check if err is a ServerError object with status and error properties
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Constituent Journey Stage.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Constituent Journey Stage.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Constituent Journey Stage could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(constituentJourneyStageData: ConstituentJourneyStageData | null) {

    if (constituentJourneyStageData == null) {

      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.constituentJourneyStageForm.reset({
        name: '',
        description: '',
        sequence: '',
        iconId: null,
        color: '',
        minLifetimeGiving: '',
        maxLifetimeGiving: '',
        minSingleGiftAmount: '',
        isDefault: false,
        minAnnualGiving: '',
        maxDaysSinceLastGift: '',
        minGiftCount: '',
        versionNumber: '',
        active: true,
        deleted: false,
      }, { emitEvent: false });

    }
    else {

      //
      // Reset the form with properly formatted values that support dates in datetime-local inputs
      //
      this.constituentJourneyStageForm.reset({
        name: constituentJourneyStageData.name ?? '',
        description: constituentJourneyStageData.description ?? '',
        sequence: constituentJourneyStageData.sequence?.toString() ?? '',
        iconId: constituentJourneyStageData.iconId,
        color: constituentJourneyStageData.color ?? '',
        minLifetimeGiving: constituentJourneyStageData.minLifetimeGiving?.toString() ?? '',
        maxLifetimeGiving: constituentJourneyStageData.maxLifetimeGiving?.toString() ?? '',
        minSingleGiftAmount: constituentJourneyStageData.minSingleGiftAmount?.toString() ?? '',
        isDefault: constituentJourneyStageData.isDefault ?? false,
        minAnnualGiving: constituentJourneyStageData.minAnnualGiving?.toString() ?? '',
        maxDaysSinceLastGift: constituentJourneyStageData.maxDaysSinceLastGift?.toString() ?? '',
        minGiftCount: constituentJourneyStageData.minGiftCount?.toString() ?? '',
        versionNumber: constituentJourneyStageData.versionNumber?.toString() ?? '',
        active: constituentJourneyStageData.active ?? true,
        deleted: constituentJourneyStageData.deleted ?? false,
      }, { emitEvent: false });
    }

    this.constituentJourneyStageForm.markAsPristine();
    this.constituentJourneyStageForm.markAsUntouched();
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


  public userIsSchedulerConstituentJourneyStageReader(): boolean {
    return this.constituentJourneyStageService.userIsSchedulerConstituentJourneyStageReader();
  }

  public userIsSchedulerConstituentJourneyStageWriter(): boolean {
    return this.constituentJourneyStageService.userIsSchedulerConstituentJourneyStageWriter();
  }
}

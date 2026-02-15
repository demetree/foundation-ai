/*
   GENERATED FORM FOR THE HOUSEHOLD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Household table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to household-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { HouseholdService, HouseholdData, HouseholdSubmitData } from '../../../scheduler-data-services/household.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface HouseholdFormValues {
  name: string,
  description: string | null,
  schedulingTargetId: number | bigint | null,       // For FK link number
  formalSalutation: string | null,
  informalSalutation: string | null,
  addressee: string | null,
  totalHouseholdGiving: string,     // Stored as string for form input, converted to number on submit.
  lastHouseholdGiftDate: string | null,
  notes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-household-add-edit',
  templateUrl: './household-add-edit.component.html',
  styleUrls: ['./household-add-edit.component.scss']
})
export class HouseholdAddEditComponent {
  @ViewChild('householdModal') householdModal!: TemplateRef<any>;
  @Output() householdChanged = new Subject<HouseholdData[]>();
  @Input() householdSubmitData: HouseholdSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<HouseholdFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public householdForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        schedulingTargetId: [null],
        formalSalutation: [''],
        informalSalutation: [''],
        addressee: [''],
        totalHouseholdGiving: ['', Validators.required],
        lastHouseholdGiftDate: [''],
        notes: [''],
        iconId: [null],
        color: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  households$ = this.householdService.GetHouseholdList();
  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private householdService: HouseholdService,
    private schedulingTargetService: SchedulingTargetService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(householdData?: HouseholdData) {

    if (householdData != null) {

      if (!this.householdService.userIsSchedulerHouseholdReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Households`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.householdSubmitData = this.householdService.ConvertToHouseholdSubmitData(householdData);
      this.isEditMode = true;
      this.objectGuid = householdData.objectGuid;

      this.buildFormValues(householdData);

    } else {

      if (!this.householdService.userIsSchedulerHouseholdWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Households`,
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
        this.householdForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.householdForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.householdModal, {
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

    if (this.householdService.userIsSchedulerHouseholdWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Households`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.householdForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.householdForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.householdForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const householdSubmitData: HouseholdSubmitData = {
        id: this.householdSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        schedulingTargetId: formValue.schedulingTargetId ? Number(formValue.schedulingTargetId) : null,
        formalSalutation: formValue.formalSalutation?.trim() || null,
        informalSalutation: formValue.informalSalutation?.trim() || null,
        addressee: formValue.addressee?.trim() || null,
        totalHouseholdGiving: Number(formValue.totalHouseholdGiving),
        lastHouseholdGiftDate: formValue.lastHouseholdGiftDate ? formValue.lastHouseholdGiftDate.trim() : null,
        notes: formValue.notes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.householdSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateHousehold(householdSubmitData);
      } else {
        this.addHousehold(householdSubmitData);
      }
  }

  private addHousehold(householdData: HouseholdSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    householdData.versionNumber = 0;
    householdData.active = true;
    householdData.deleted = false;
    this.householdService.PostHousehold(householdData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newHousehold) => {

        this.householdService.ClearAllCaches();

        this.householdChanged.next([newHousehold]);

        this.alertService.showMessage("Household added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/household', newHousehold.id]);
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
                                   'You do not have permission to save this Household.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Household.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Household could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateHousehold(householdData: HouseholdSubmitData) {
    this.householdService.PutHousehold(householdData.id, householdData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedHousehold) => {

        this.householdService.ClearAllCaches();

        this.householdChanged.next([updatedHousehold]);

        this.alertService.showMessage("Household updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Household.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Household.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Household could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(householdData: HouseholdData | null) {

    if (householdData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.householdForm.reset({
        name: '',
        description: '',
        schedulingTargetId: null,
        formalSalutation: '',
        informalSalutation: '',
        addressee: '',
        totalHouseholdGiving: '',
        lastHouseholdGiftDate: '',
        notes: '',
        iconId: null,
        color: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.householdForm.reset({
        name: householdData.name ?? '',
        description: householdData.description ?? '',
        schedulingTargetId: householdData.schedulingTargetId,
        formalSalutation: householdData.formalSalutation ?? '',
        informalSalutation: householdData.informalSalutation ?? '',
        addressee: householdData.addressee ?? '',
        totalHouseholdGiving: householdData.totalHouseholdGiving?.toString() ?? '',
        lastHouseholdGiftDate: householdData.lastHouseholdGiftDate ?? '',
        notes: householdData.notes ?? '',
        iconId: householdData.iconId,
        color: householdData.color ?? '',
        avatarFileName: householdData.avatarFileName ?? '',
        avatarSize: householdData.avatarSize?.toString() ?? '',
        avatarData: householdData.avatarData ?? '',
        avatarMimeType: householdData.avatarMimeType ?? '',
        versionNumber: householdData.versionNumber?.toString() ?? '',
        active: householdData.active ?? true,
        deleted: householdData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.householdForm.markAsPristine();
    this.householdForm.markAsUntouched();
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


  public userIsSchedulerHouseholdReader(): boolean {
    return this.householdService.userIsSchedulerHouseholdReader();
  }

  public userIsSchedulerHouseholdWriter(): boolean {
    return this.householdService.userIsSchedulerHouseholdWriter();
  }
}

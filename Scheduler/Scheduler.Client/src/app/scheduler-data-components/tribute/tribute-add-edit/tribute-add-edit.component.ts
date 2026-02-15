/*
   GENERATED FORM FOR THE TRIBUTE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Tribute table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tribute-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TributeService, TributeData, TributeSubmitData } from '../../../scheduler-data-services/tribute.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { TributeTypeService } from '../../../scheduler-data-services/tribute-type.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TributeFormValues {
  name: string,
  description: string | null,
  tributeTypeId: number | bigint | null,       // For FK link number
  defaultAcknowledgeeId: number | bigint | null,       // For FK link number
  startDate: string | null,
  endDate: string | null,
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
  selector: 'app-tribute-add-edit',
  templateUrl: './tribute-add-edit.component.html',
  styleUrls: ['./tribute-add-edit.component.scss']
})
export class TributeAddEditComponent {
  @ViewChild('tributeModal') tributeModal!: TemplateRef<any>;
  @Output() tributeChanged = new Subject<TributeData[]>();
  @Input() tributeSubmitData: TributeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TributeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public tributeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        tributeTypeId: [null],
        defaultAcknowledgeeId: [null],
        startDate: [''],
        endDate: [''],
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

  tributes$ = this.tributeService.GetTributeList();
  tributeTypes$ = this.tributeTypeService.GetTributeTypeList();
  constituents$ = this.constituentService.GetConstituentList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private tributeService: TributeService,
    private tributeTypeService: TributeTypeService,
    private constituentService: ConstituentService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(tributeData?: TributeData) {

    if (tributeData != null) {

      if (!this.tributeService.userIsSchedulerTributeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Tributes`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.tributeSubmitData = this.tributeService.ConvertToTributeSubmitData(tributeData);
      this.isEditMode = true;
      this.objectGuid = tributeData.objectGuid;

      this.buildFormValues(tributeData);

    } else {

      if (!this.tributeService.userIsSchedulerTributeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Tributes`,
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
        this.tributeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.tributeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.tributeModal, {
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

    if (this.tributeService.userIsSchedulerTributeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Tributes`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.tributeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.tributeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.tributeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const tributeSubmitData: TributeSubmitData = {
        id: this.tributeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        tributeTypeId: formValue.tributeTypeId ? Number(formValue.tributeTypeId) : null,
        defaultAcknowledgeeId: formValue.defaultAcknowledgeeId ? Number(formValue.defaultAcknowledgeeId) : null,
        startDate: formValue.startDate ? formValue.startDate.trim() : null,
        endDate: formValue.endDate ? formValue.endDate.trim() : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.tributeSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateTribute(tributeSubmitData);
      } else {
        this.addTribute(tributeSubmitData);
      }
  }

  private addTribute(tributeData: TributeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    tributeData.versionNumber = 0;
    tributeData.active = true;
    tributeData.deleted = false;
    this.tributeService.PostTribute(tributeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTribute) => {

        this.tributeService.ClearAllCaches();

        this.tributeChanged.next([newTribute]);

        this.alertService.showMessage("Tribute added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/tribute', newTribute.id]);
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
                                   'You do not have permission to save this Tribute.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tribute.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tribute could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTribute(tributeData: TributeSubmitData) {
    this.tributeService.PutTribute(tributeData.id, tributeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTribute) => {

        this.tributeService.ClearAllCaches();

        this.tributeChanged.next([updatedTribute]);

        this.alertService.showMessage("Tribute updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Tribute.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tribute.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tribute could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(tributeData: TributeData | null) {

    if (tributeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.tributeForm.reset({
        name: '',
        description: '',
        tributeTypeId: null,
        defaultAcknowledgeeId: null,
        startDate: '',
        endDate: '',
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
        this.tributeForm.reset({
        name: tributeData.name ?? '',
        description: tributeData.description ?? '',
        tributeTypeId: tributeData.tributeTypeId,
        defaultAcknowledgeeId: tributeData.defaultAcknowledgeeId,
        startDate: tributeData.startDate ?? '',
        endDate: tributeData.endDate ?? '',
        iconId: tributeData.iconId,
        color: tributeData.color ?? '',
        avatarFileName: tributeData.avatarFileName ?? '',
        avatarSize: tributeData.avatarSize?.toString() ?? '',
        avatarData: tributeData.avatarData ?? '',
        avatarMimeType: tributeData.avatarMimeType ?? '',
        versionNumber: tributeData.versionNumber?.toString() ?? '',
        active: tributeData.active ?? true,
        deleted: tributeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.tributeForm.markAsPristine();
    this.tributeForm.markAsUntouched();
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


  public userIsSchedulerTributeReader(): boolean {
    return this.tributeService.userIsSchedulerTributeReader();
  }

  public userIsSchedulerTributeWriter(): boolean {
    return this.tributeService.userIsSchedulerTributeWriter();
  }
}

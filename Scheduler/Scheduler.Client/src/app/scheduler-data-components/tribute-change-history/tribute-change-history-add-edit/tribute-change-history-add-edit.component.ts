/*
   GENERATED FORM FOR THE TRIBUTECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TributeChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tribute-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TributeChangeHistoryService, TributeChangeHistoryData, TributeChangeHistorySubmitData } from '../../../scheduler-data-services/tribute-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { TributeService } from '../../../scheduler-data-services/tribute.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TributeChangeHistoryFormValues {
  tributeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-tribute-change-history-add-edit',
  templateUrl: './tribute-change-history-add-edit.component.html',
  styleUrls: ['./tribute-change-history-add-edit.component.scss']
})
export class TributeChangeHistoryAddEditComponent {
  @ViewChild('tributeChangeHistoryModal') tributeChangeHistoryModal!: TemplateRef<any>;
  @Output() tributeChangeHistoryChanged = new Subject<TributeChangeHistoryData[]>();
  @Input() tributeChangeHistorySubmitData: TributeChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TributeChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public tributeChangeHistoryForm: FormGroup = this.fb.group({
        tributeId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  tributeChangeHistories$ = this.tributeChangeHistoryService.GetTributeChangeHistoryList();
  tributes$ = this.tributeService.GetTributeList();

  constructor(
    private modalService: NgbModal,
    private tributeChangeHistoryService: TributeChangeHistoryService,
    private tributeService: TributeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(tributeChangeHistoryData?: TributeChangeHistoryData) {

    if (tributeChangeHistoryData != null) {

      if (!this.tributeChangeHistoryService.userIsSchedulerTributeChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Tribute Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.tributeChangeHistorySubmitData = this.tributeChangeHistoryService.ConvertToTributeChangeHistorySubmitData(tributeChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(tributeChangeHistoryData);

    } else {

      if (!this.tributeChangeHistoryService.userIsSchedulerTributeChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Tribute Change Histories`,
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
        this.tributeChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.tributeChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.tributeChangeHistoryModal, {
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

    if (this.tributeChangeHistoryService.userIsSchedulerTributeChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Tribute Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.tributeChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.tributeChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.tributeChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const tributeChangeHistorySubmitData: TributeChangeHistorySubmitData = {
        id: this.tributeChangeHistorySubmitData?.id || 0,
        tributeId: Number(formValue.tributeId),
        versionNumber: this.tributeChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateTributeChangeHistory(tributeChangeHistorySubmitData);
      } else {
        this.addTributeChangeHistory(tributeChangeHistorySubmitData);
      }
  }

  private addTributeChangeHistory(tributeChangeHistoryData: TributeChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    tributeChangeHistoryData.versionNumber = 0;
    this.tributeChangeHistoryService.PostTributeChangeHistory(tributeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTributeChangeHistory) => {

        this.tributeChangeHistoryService.ClearAllCaches();

        this.tributeChangeHistoryChanged.next([newTributeChangeHistory]);

        this.alertService.showMessage("Tribute Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/tributechangehistory', newTributeChangeHistory.id]);
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
                                   'You do not have permission to save this Tribute Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tribute Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tribute Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTributeChangeHistory(tributeChangeHistoryData: TributeChangeHistorySubmitData) {
    this.tributeChangeHistoryService.PutTributeChangeHistory(tributeChangeHistoryData.id, tributeChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTributeChangeHistory) => {

        this.tributeChangeHistoryService.ClearAllCaches();

        this.tributeChangeHistoryChanged.next([updatedTributeChangeHistory]);

        this.alertService.showMessage("Tribute Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Tribute Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tribute Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tribute Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(tributeChangeHistoryData: TributeChangeHistoryData | null) {

    if (tributeChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.tributeChangeHistoryForm.reset({
        tributeId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.tributeChangeHistoryForm.reset({
        tributeId: tributeChangeHistoryData.tributeId,
        versionNumber: tributeChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(tributeChangeHistoryData.timeStamp) ?? '',
        userId: tributeChangeHistoryData.userId?.toString() ?? '',
        data: tributeChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.tributeChangeHistoryForm.markAsPristine();
    this.tributeChangeHistoryForm.markAsUntouched();
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


  public userIsSchedulerTributeChangeHistoryReader(): boolean {
    return this.tributeChangeHistoryService.userIsSchedulerTributeChangeHistoryReader();
  }

  public userIsSchedulerTributeChangeHistoryWriter(): boolean {
    return this.tributeChangeHistoryService.userIsSchedulerTributeChangeHistoryWriter();
  }
}

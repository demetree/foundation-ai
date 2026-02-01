/*
   GENERATED FORM FOR THE ESCALATIONPOLICYCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EscalationPolicyChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to escalation-policy-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EscalationPolicyChangeHistoryService, EscalationPolicyChangeHistoryData, EscalationPolicyChangeHistorySubmitData } from '../../../alerting-data-services/escalation-policy-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { EscalationPolicyService } from '../../../alerting-data-services/escalation-policy.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface EscalationPolicyChangeHistoryFormValues {
  escalationPolicyId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-escalation-policy-change-history-add-edit',
  templateUrl: './escalation-policy-change-history-add-edit.component.html',
  styleUrls: ['./escalation-policy-change-history-add-edit.component.scss']
})
export class EscalationPolicyChangeHistoryAddEditComponent {
  @ViewChild('escalationPolicyChangeHistoryModal') escalationPolicyChangeHistoryModal!: TemplateRef<any>;
  @Output() escalationPolicyChangeHistoryChanged = new Subject<EscalationPolicyChangeHistoryData[]>();
  @Input() escalationPolicyChangeHistorySubmitData: EscalationPolicyChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EscalationPolicyChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public escalationPolicyChangeHistoryForm: FormGroup = this.fb.group({
        escalationPolicyId: [null, Validators.required],
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

  escalationPolicyChangeHistories$ = this.escalationPolicyChangeHistoryService.GetEscalationPolicyChangeHistoryList();
  escalationPolicies$ = this.escalationPolicyService.GetEscalationPolicyList();

  constructor(
    private modalService: NgbModal,
    private escalationPolicyChangeHistoryService: EscalationPolicyChangeHistoryService,
    private escalationPolicyService: EscalationPolicyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(escalationPolicyChangeHistoryData?: EscalationPolicyChangeHistoryData) {

    if (escalationPolicyChangeHistoryData != null) {

      if (!this.escalationPolicyChangeHistoryService.userIsAlertingEscalationPolicyChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Escalation Policy Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.escalationPolicyChangeHistorySubmitData = this.escalationPolicyChangeHistoryService.ConvertToEscalationPolicyChangeHistorySubmitData(escalationPolicyChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(escalationPolicyChangeHistoryData);

    } else {

      if (!this.escalationPolicyChangeHistoryService.userIsAlertingEscalationPolicyChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Escalation Policy Change Histories`,
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
        this.escalationPolicyChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.escalationPolicyChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.escalationPolicyChangeHistoryModal, {
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

    if (this.escalationPolicyChangeHistoryService.userIsAlertingEscalationPolicyChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Escalation Policy Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.escalationPolicyChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.escalationPolicyChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.escalationPolicyChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const escalationPolicyChangeHistorySubmitData: EscalationPolicyChangeHistorySubmitData = {
        id: this.escalationPolicyChangeHistorySubmitData?.id || 0,
        escalationPolicyId: Number(formValue.escalationPolicyId),
        versionNumber: this.escalationPolicyChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updateEscalationPolicyChangeHistory(escalationPolicyChangeHistorySubmitData);
      } else {
        this.addEscalationPolicyChangeHistory(escalationPolicyChangeHistorySubmitData);
      }
  }

  private addEscalationPolicyChangeHistory(escalationPolicyChangeHistoryData: EscalationPolicyChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    escalationPolicyChangeHistoryData.versionNumber = 0;
    this.escalationPolicyChangeHistoryService.PostEscalationPolicyChangeHistory(escalationPolicyChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEscalationPolicyChangeHistory) => {

        this.escalationPolicyChangeHistoryService.ClearAllCaches();

        this.escalationPolicyChangeHistoryChanged.next([newEscalationPolicyChangeHistory]);

        this.alertService.showMessage("Escalation Policy Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/escalationpolicychangehistory', newEscalationPolicyChangeHistory.id]);
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
                                   'You do not have permission to save this Escalation Policy Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Escalation Policy Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Escalation Policy Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEscalationPolicyChangeHistory(escalationPolicyChangeHistoryData: EscalationPolicyChangeHistorySubmitData) {
    this.escalationPolicyChangeHistoryService.PutEscalationPolicyChangeHistory(escalationPolicyChangeHistoryData.id, escalationPolicyChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEscalationPolicyChangeHistory) => {

        this.escalationPolicyChangeHistoryService.ClearAllCaches();

        this.escalationPolicyChangeHistoryChanged.next([updatedEscalationPolicyChangeHistory]);

        this.alertService.showMessage("Escalation Policy Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Escalation Policy Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Escalation Policy Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Escalation Policy Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(escalationPolicyChangeHistoryData: EscalationPolicyChangeHistoryData | null) {

    if (escalationPolicyChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.escalationPolicyChangeHistoryForm.reset({
        escalationPolicyId: null,
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
        this.escalationPolicyChangeHistoryForm.reset({
        escalationPolicyId: escalationPolicyChangeHistoryData.escalationPolicyId,
        versionNumber: escalationPolicyChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(escalationPolicyChangeHistoryData.timeStamp) ?? '',
        userId: escalationPolicyChangeHistoryData.userId?.toString() ?? '',
        data: escalationPolicyChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.escalationPolicyChangeHistoryForm.markAsPristine();
    this.escalationPolicyChangeHistoryForm.markAsUntouched();
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


  public userIsAlertingEscalationPolicyChangeHistoryReader(): boolean {
    return this.escalationPolicyChangeHistoryService.userIsAlertingEscalationPolicyChangeHistoryReader();
  }

  public userIsAlertingEscalationPolicyChangeHistoryWriter(): boolean {
    return this.escalationPolicyChangeHistoryService.userIsAlertingEscalationPolicyChangeHistoryWriter();
  }
}

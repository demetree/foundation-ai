/*
   GENERATED FORM FOR THE ESCALATIONRULE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EscalationRule table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to escalation-rule-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EscalationRuleService, EscalationRuleData, EscalationRuleSubmitData } from '../../../alerting-data-services/escalation-rule.service';
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
interface EscalationRuleFormValues {
  escalationPolicyId: number | bigint,       // For FK link number
  ruleOrder: string,     // Stored as string for form input, converted to number on submit.
  delayMinutes: string,     // Stored as string for form input, converted to number on submit.
  repeatCount: string,     // Stored as string for form input, converted to number on submit.
  repeatDelayMinutes: string | null,     // Stored as string for form input, converted to number on submit.
  targetType: string,
  targetObjectGuid: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-escalation-rule-add-edit',
  templateUrl: './escalation-rule-add-edit.component.html',
  styleUrls: ['./escalation-rule-add-edit.component.scss']
})
export class EscalationRuleAddEditComponent {
  @ViewChild('escalationRuleModal') escalationRuleModal!: TemplateRef<any>;
  @Output() escalationRuleChanged = new Subject<EscalationRuleData[]>();
  @Input() escalationRuleSubmitData: EscalationRuleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EscalationRuleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public escalationRuleForm: FormGroup = this.fb.group({
        escalationPolicyId: [null, Validators.required],
        ruleOrder: ['', Validators.required],
        delayMinutes: ['', Validators.required],
        repeatCount: ['', Validators.required],
        repeatDelayMinutes: [''],
        targetType: ['', Validators.required],
        targetObjectGuid: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  escalationRules$ = this.escalationRuleService.GetEscalationRuleList();
  escalationPolicies$ = this.escalationPolicyService.GetEscalationPolicyList();

  constructor(
    private modalService: NgbModal,
    private escalationRuleService: EscalationRuleService,
    private escalationPolicyService: EscalationPolicyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(escalationRuleData?: EscalationRuleData) {

    if (escalationRuleData != null) {

      if (!this.escalationRuleService.userIsAlertingEscalationRuleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Escalation Rules`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.escalationRuleSubmitData = this.escalationRuleService.ConvertToEscalationRuleSubmitData(escalationRuleData);
      this.isEditMode = true;
      this.objectGuid = escalationRuleData.objectGuid;

      this.buildFormValues(escalationRuleData);

    } else {

      if (!this.escalationRuleService.userIsAlertingEscalationRuleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Escalation Rules`,
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
        this.escalationRuleForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.escalationRuleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.escalationRuleModal, {
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

    if (this.escalationRuleService.userIsAlertingEscalationRuleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Escalation Rules`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.escalationRuleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.escalationRuleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.escalationRuleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const escalationRuleSubmitData: EscalationRuleSubmitData = {
        id: this.escalationRuleSubmitData?.id || 0,
        escalationPolicyId: Number(formValue.escalationPolicyId),
        ruleOrder: Number(formValue.ruleOrder),
        delayMinutes: Number(formValue.delayMinutes),
        repeatCount: Number(formValue.repeatCount),
        repeatDelayMinutes: formValue.repeatDelayMinutes ? Number(formValue.repeatDelayMinutes) : null,
        targetType: formValue.targetType!.trim(),
        targetObjectGuid: formValue.targetObjectGuid?.trim() || null,
        versionNumber: this.escalationRuleSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEscalationRule(escalationRuleSubmitData);
      } else {
        this.addEscalationRule(escalationRuleSubmitData);
      }
  }

  private addEscalationRule(escalationRuleData: EscalationRuleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    escalationRuleData.versionNumber = 0;
    escalationRuleData.active = true;
    escalationRuleData.deleted = false;
    this.escalationRuleService.PostEscalationRule(escalationRuleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEscalationRule) => {

        this.escalationRuleService.ClearAllCaches();

        this.escalationRuleChanged.next([newEscalationRule]);

        this.alertService.showMessage("Escalation Rule added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/escalationrule', newEscalationRule.id]);
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
                                   'You do not have permission to save this Escalation Rule.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Escalation Rule.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Escalation Rule could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEscalationRule(escalationRuleData: EscalationRuleSubmitData) {
    this.escalationRuleService.PutEscalationRule(escalationRuleData.id, escalationRuleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEscalationRule) => {

        this.escalationRuleService.ClearAllCaches();

        this.escalationRuleChanged.next([updatedEscalationRule]);

        this.alertService.showMessage("Escalation Rule updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Escalation Rule.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Escalation Rule.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Escalation Rule could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(escalationRuleData: EscalationRuleData | null) {

    if (escalationRuleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.escalationRuleForm.reset({
        escalationPolicyId: null,
        ruleOrder: '',
        delayMinutes: '',
        repeatCount: '',
        repeatDelayMinutes: '',
        targetType: '',
        targetObjectGuid: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.escalationRuleForm.reset({
        escalationPolicyId: escalationRuleData.escalationPolicyId,
        ruleOrder: escalationRuleData.ruleOrder?.toString() ?? '',
        delayMinutes: escalationRuleData.delayMinutes?.toString() ?? '',
        repeatCount: escalationRuleData.repeatCount?.toString() ?? '',
        repeatDelayMinutes: escalationRuleData.repeatDelayMinutes?.toString() ?? '',
        targetType: escalationRuleData.targetType ?? '',
        targetObjectGuid: escalationRuleData.targetObjectGuid ?? '',
        versionNumber: escalationRuleData.versionNumber?.toString() ?? '',
        active: escalationRuleData.active ?? true,
        deleted: escalationRuleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.escalationRuleForm.markAsPristine();
    this.escalationRuleForm.markAsUntouched();
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


  public userIsAlertingEscalationRuleReader(): boolean {
    return this.escalationRuleService.userIsAlertingEscalationRuleReader();
  }

  public userIsAlertingEscalationRuleWriter(): boolean {
    return this.escalationRuleService.userIsAlertingEscalationRuleWriter();
  }
}

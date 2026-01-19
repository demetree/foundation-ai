/*
   GENERATED FORM FOR THE AUDITPLANB TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditPlanB table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-plan-b-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditPlanBService, AuditPlanBData, AuditPlanBSubmitData } from '../../../auditor-data-services/audit-plan-b.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AuditPlanBFormValues {
  startTime: string,
  stopTime: string,
  completedSuccessfully: boolean,
  user: string | null,
  session: string | null,
  type: string | null,
  accessType: string | null,
  source: string | null,
  userAgent: string | null,
  module: string | null,
  moduleEntity: string | null,
  resource: string | null,
  hostSystem: string | null,
  primaryKey: string | null,
  threadId: string | null,     // Stored as string for form input, converted to number on submit.
  message: string | null,
  beforeState: string | null,
  afterState: string | null,
  errorMessage: string | null,
  exceptionText: string | null,
};

@Component({
  selector: 'app-audit-plan-b-add-edit',
  templateUrl: './audit-plan-b-add-edit.component.html',
  styleUrls: ['./audit-plan-b-add-edit.component.scss']
})
export class AuditPlanBAddEditComponent {
  @ViewChild('auditPlanBModal') auditPlanBModal!: TemplateRef<any>;
  @Output() auditPlanBChanged = new Subject<AuditPlanBData[]>();
  @Input() auditPlanBSubmitData: AuditPlanBSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditPlanBFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditPlanBForm: FormGroup = this.fb.group({
        startTime: ['', Validators.required],
        stopTime: ['', Validators.required],
        completedSuccessfully: [false],
        user: [''],
        session: [''],
        type: [''],
        accessType: [''],
        source: [''],
        userAgent: [''],
        module: [''],
        moduleEntity: [''],
        resource: [''],
        hostSystem: [''],
        primaryKey: [''],
        threadId: [''],
        message: [''],
        beforeState: [''],
        afterState: [''],
        errorMessage: [''],
        exceptionText: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditPlanBs$ = this.auditPlanBService.GetAuditPlanBList();

  constructor(
    private modalService: NgbModal,
    private auditPlanBService: AuditPlanBService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditPlanBData?: AuditPlanBData) {

    if (auditPlanBData != null) {

      if (!this.auditPlanBService.userIsAuditorAuditPlanBReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Plan Bs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditPlanBSubmitData = this.auditPlanBService.ConvertToAuditPlanBSubmitData(auditPlanBData);
      this.isEditMode = true;

      this.buildFormValues(auditPlanBData);

    } else {

      if (!this.auditPlanBService.userIsAuditorAuditPlanBWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Plan Bs`,
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
        this.auditPlanBForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditPlanBForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.auditPlanBModal, {
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

    if (this.auditPlanBService.userIsAuditorAuditPlanBWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Plan Bs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditPlanBForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditPlanBForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditPlanBForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditPlanBSubmitData: AuditPlanBSubmitData = {
        id: this.auditPlanBSubmitData?.id || 0,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        stopTime: dateTimeLocalToIsoUtc(formValue.stopTime!.trim())!,
        completedSuccessfully: !!formValue.completedSuccessfully,
        user: formValue.user?.trim() || null,
        session: formValue.session?.trim() || null,
        type: formValue.type?.trim() || null,
        accessType: formValue.accessType?.trim() || null,
        source: formValue.source?.trim() || null,
        userAgent: formValue.userAgent?.trim() || null,
        module: formValue.module?.trim() || null,
        moduleEntity: formValue.moduleEntity?.trim() || null,
        resource: formValue.resource?.trim() || null,
        hostSystem: formValue.hostSystem?.trim() || null,
        primaryKey: formValue.primaryKey?.trim() || null,
        threadId: formValue.threadId ? Number(formValue.threadId) : null,
        message: formValue.message?.trim() || null,
        beforeState: formValue.beforeState?.trim() || null,
        afterState: formValue.afterState?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
        exceptionText: formValue.exceptionText?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateAuditPlanB(auditPlanBSubmitData);
      } else {
        this.addAuditPlanB(auditPlanBSubmitData);
      }
  }

  private addAuditPlanB(auditPlanBData: AuditPlanBSubmitData) {
    this.auditPlanBService.PostAuditPlanB(auditPlanBData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditPlanB) => {

        this.auditPlanBService.ClearAllCaches();

        this.auditPlanBChanged.next([newAuditPlanB]);

        this.alertService.showMessage("Audit Plan B added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditplanb', newAuditPlanB.id]);
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
                                   'You do not have permission to save this Audit Plan B.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Plan B.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Plan B could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditPlanB(auditPlanBData: AuditPlanBSubmitData) {
    this.auditPlanBService.PutAuditPlanB(auditPlanBData.id, auditPlanBData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditPlanB) => {

        this.auditPlanBService.ClearAllCaches();

        this.auditPlanBChanged.next([updatedAuditPlanB]);

        this.alertService.showMessage("Audit Plan B updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Plan B.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Plan B.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Plan B could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditPlanBData: AuditPlanBData | null) {

    if (auditPlanBData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditPlanBForm.reset({
        startTime: '',
        stopTime: '',
        completedSuccessfully: false,
        user: '',
        session: '',
        type: '',
        accessType: '',
        source: '',
        userAgent: '',
        module: '',
        moduleEntity: '',
        resource: '',
        hostSystem: '',
        primaryKey: '',
        threadId: '',
        message: '',
        beforeState: '',
        afterState: '',
        errorMessage: '',
        exceptionText: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditPlanBForm.reset({
        startTime: isoUtcStringToDateTimeLocal(auditPlanBData.startTime) ?? '',
        stopTime: isoUtcStringToDateTimeLocal(auditPlanBData.stopTime) ?? '',
        completedSuccessfully: auditPlanBData.completedSuccessfully ?? false,
        user: auditPlanBData.user ?? '',
        session: auditPlanBData.session ?? '',
        type: auditPlanBData.type ?? '',
        accessType: auditPlanBData.accessType ?? '',
        source: auditPlanBData.source ?? '',
        userAgent: auditPlanBData.userAgent ?? '',
        module: auditPlanBData.module ?? '',
        moduleEntity: auditPlanBData.moduleEntity ?? '',
        resource: auditPlanBData.resource ?? '',
        hostSystem: auditPlanBData.hostSystem ?? '',
        primaryKey: auditPlanBData.primaryKey ?? '',
        threadId: auditPlanBData.threadId?.toString() ?? '',
        message: auditPlanBData.message ?? '',
        beforeState: auditPlanBData.beforeState ?? '',
        afterState: auditPlanBData.afterState ?? '',
        errorMessage: auditPlanBData.errorMessage ?? '',
        exceptionText: auditPlanBData.exceptionText ?? '',
      }, { emitEvent: false});
    }

    this.auditPlanBForm.markAsPristine();
    this.auditPlanBForm.markAsUntouched();
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


  public userIsAuditorAuditPlanBReader(): boolean {
    return this.auditPlanBService.userIsAuditorAuditPlanBReader();
  }

  public userIsAuditorAuditPlanBWriter(): boolean {
    return this.auditPlanBService.userIsAuditorAuditPlanBWriter();
  }
}

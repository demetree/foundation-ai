/*
   GENERATED FORM FOR THE AUDITUSERAGENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditUserAgent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-user-agent-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditUserAgentService, AuditUserAgentData, AuditUserAgentSubmitData } from '../../../auditor-data-services/audit-user-agent.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AuditUserAgentFormValues {
  name: string,
  comments: string | null,
  firstAccess: string | null,
};

@Component({
  selector: 'app-audit-user-agent-add-edit',
  templateUrl: './audit-user-agent-add-edit.component.html',
  styleUrls: ['./audit-user-agent-add-edit.component.scss']
})
export class AuditUserAgentAddEditComponent {
  @ViewChild('auditUserAgentModal') auditUserAgentModal!: TemplateRef<any>;
  @Output() auditUserAgentChanged = new Subject<AuditUserAgentData[]>();
  @Input() auditUserAgentSubmitData: AuditUserAgentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditUserAgentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditUserAgentForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditUserAgents$ = this.auditUserAgentService.GetAuditUserAgentList();

  constructor(
    private modalService: NgbModal,
    private auditUserAgentService: AuditUserAgentService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditUserAgentData?: AuditUserAgentData) {

    if (auditUserAgentData != null) {

      if (!this.auditUserAgentService.userIsAuditorAuditUserAgentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit User Agents`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditUserAgentSubmitData = this.auditUserAgentService.ConvertToAuditUserAgentSubmitData(auditUserAgentData);
      this.isEditMode = true;

      this.buildFormValues(auditUserAgentData);

    } else {

      if (!this.auditUserAgentService.userIsAuditorAuditUserAgentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit User Agents`,
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
        this.auditUserAgentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditUserAgentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.auditUserAgentModal, {
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

    if (this.auditUserAgentService.userIsAuditorAuditUserAgentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit User Agents`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditUserAgentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditUserAgentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditUserAgentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditUserAgentSubmitData: AuditUserAgentSubmitData = {
        id: this.auditUserAgentSubmitData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };

      if (this.isEditMode) {
        this.updateAuditUserAgent(auditUserAgentSubmitData);
      } else {
        this.addAuditUserAgent(auditUserAgentSubmitData);
      }
  }

  private addAuditUserAgent(auditUserAgentData: AuditUserAgentSubmitData) {
    this.auditUserAgentService.PostAuditUserAgent(auditUserAgentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditUserAgent) => {

        this.auditUserAgentService.ClearAllCaches();

        this.auditUserAgentChanged.next([newAuditUserAgent]);

        this.alertService.showMessage("Audit User Agent added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/audituseragent', newAuditUserAgent.id]);
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
                                   'You do not have permission to save this Audit User Agent.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit User Agent.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit User Agent could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditUserAgent(auditUserAgentData: AuditUserAgentSubmitData) {
    this.auditUserAgentService.PutAuditUserAgent(auditUserAgentData.id, auditUserAgentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditUserAgent) => {

        this.auditUserAgentService.ClearAllCaches();

        this.auditUserAgentChanged.next([updatedAuditUserAgent]);

        this.alertService.showMessage("Audit User Agent updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit User Agent.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit User Agent.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit User Agent could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditUserAgentData: AuditUserAgentData | null) {

    if (auditUserAgentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditUserAgentForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditUserAgentForm.reset({
        name: auditUserAgentData.name ?? '',
        comments: auditUserAgentData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditUserAgentData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditUserAgentForm.markAsPristine();
    this.auditUserAgentForm.markAsUntouched();
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


  public userIsAuditorAuditUserAgentReader(): boolean {
    return this.auditUserAgentService.userIsAuditorAuditUserAgentReader();
  }

  public userIsAuditorAuditUserAgentWriter(): boolean {
    return this.auditUserAgentService.userIsAuditorAuditUserAgentWriter();
  }
}

/*
   GENERATED FORM FOR THE AUDITEVENTENTITYSTATE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditEventEntityState table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-event-entity-state-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditEventEntityStateService, AuditEventEntityStateData, AuditEventEntityStateSubmitData } from '../../../auditor-data-services/audit-event-entity-state.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AuditEventEntityStateFormValues {
  auditEventId: number | bigint,       // For FK link number
  beforeState: string | null,
  afterState: string | null,
};

@Component({
  selector: 'app-audit-event-entity-state-add-edit',
  templateUrl: './audit-event-entity-state-add-edit.component.html',
  styleUrls: ['./audit-event-entity-state-add-edit.component.scss']
})
export class AuditEventEntityStateAddEditComponent {
  @ViewChild('auditEventEntityStateModal') auditEventEntityStateModal!: TemplateRef<any>;
  @Output() auditEventEntityStateChanged = new Subject<AuditEventEntityStateData[]>();
  @Input() auditEventEntityStateSubmitData: AuditEventEntityStateSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditEventEntityStateFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditEventEntityStateForm: FormGroup = this.fb.group({
        auditEventId: [null, Validators.required],
        beforeState: [''],
        afterState: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditEventEntityStates$ = this.auditEventEntityStateService.GetAuditEventEntityStateList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  constructor(
    private modalService: NgbModal,
    private auditEventEntityStateService: AuditEventEntityStateService,
    private auditEventService: AuditEventService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditEventEntityStateData?: AuditEventEntityStateData) {

    if (auditEventEntityStateData != null) {

      if (!this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Event Entity States`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditEventEntityStateSubmitData = this.auditEventEntityStateService.ConvertToAuditEventEntityStateSubmitData(auditEventEntityStateData);
      this.isEditMode = true;

      this.buildFormValues(auditEventEntityStateData);

    } else {

      if (!this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Event Entity States`,
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
        this.auditEventEntityStateForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditEventEntityStateForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.auditEventEntityStateModal, {
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

    if (this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Event Entity States`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditEventEntityStateForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditEventEntityStateForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditEventEntityStateForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditEventEntityStateSubmitData: AuditEventEntityStateSubmitData = {
        id: this.auditEventEntityStateSubmitData?.id || 0,
        auditEventId: Number(formValue.auditEventId),
        beforeState: formValue.beforeState?.trim() || null,
        afterState: formValue.afterState?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateAuditEventEntityState(auditEventEntityStateSubmitData);
      } else {
        this.addAuditEventEntityState(auditEventEntityStateSubmitData);
      }
  }

  private addAuditEventEntityState(auditEventEntityStateData: AuditEventEntityStateSubmitData) {
    this.auditEventEntityStateService.PostAuditEventEntityState(auditEventEntityStateData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditEventEntityState) => {

        this.auditEventEntityStateService.ClearAllCaches();

        this.auditEventEntityStateChanged.next([newAuditEventEntityState]);

        this.alertService.showMessage("Audit Event Entity State added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditevententitystate', newAuditEventEntityState.id]);
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
                                   'You do not have permission to save this Audit Event Entity State.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event Entity State.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event Entity State could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditEventEntityState(auditEventEntityStateData: AuditEventEntityStateSubmitData) {
    this.auditEventEntityStateService.PutAuditEventEntityState(auditEventEntityStateData.id, auditEventEntityStateData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditEventEntityState) => {

        this.auditEventEntityStateService.ClearAllCaches();

        this.auditEventEntityStateChanged.next([updatedAuditEventEntityState]);

        this.alertService.showMessage("Audit Event Entity State updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Event Entity State.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event Entity State.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event Entity State could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditEventEntityStateData: AuditEventEntityStateData | null) {

    if (auditEventEntityStateData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditEventEntityStateForm.reset({
        auditEventId: null,
        beforeState: '',
        afterState: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditEventEntityStateForm.reset({
        auditEventId: auditEventEntityStateData.auditEventId,
        beforeState: auditEventEntityStateData.beforeState ?? '',
        afterState: auditEventEntityStateData.afterState ?? '',
      }, { emitEvent: false});
    }

    this.auditEventEntityStateForm.markAsPristine();
    this.auditEventEntityStateForm.markAsUntouched();
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


  public userIsAuditorAuditEventEntityStateReader(): boolean {
    return this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateReader();
  }

  public userIsAuditorAuditEventEntityStateWriter(): boolean {
    return this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateWriter();
  }
}

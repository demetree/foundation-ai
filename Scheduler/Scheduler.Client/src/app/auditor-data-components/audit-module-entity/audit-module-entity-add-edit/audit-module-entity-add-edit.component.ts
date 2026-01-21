/*
   GENERATED FORM FOR THE AUDITMODULEENTITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditModuleEntity table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-module-entity-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditModuleEntityService, AuditModuleEntityData, AuditModuleEntitySubmitData } from '../../../auditor-data-services/audit-module-entity.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuditModuleService } from '../../../auditor-data-services/audit-module.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AuditModuleEntityFormValues {
  auditModuleId: number | bigint,       // For FK link number
  name: string,
  comments: string | null,
  firstAccess: string | null,
};

@Component({
  selector: 'app-audit-module-entity-add-edit',
  templateUrl: './audit-module-entity-add-edit.component.html',
  styleUrls: ['./audit-module-entity-add-edit.component.scss']
})
export class AuditModuleEntityAddEditComponent {
  @ViewChild('auditModuleEntityModal') auditModuleEntityModal!: TemplateRef<any>;
  @Output() auditModuleEntityChanged = new Subject<AuditModuleEntityData[]>();
  @Input() auditModuleEntitySubmitData: AuditModuleEntitySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditModuleEntityFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditModuleEntityForm: FormGroup = this.fb.group({
        auditModuleId: [null, Validators.required],
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditModuleEntities$ = this.auditModuleEntityService.GetAuditModuleEntityList();
  auditModules$ = this.auditModuleService.GetAuditModuleList();

  constructor(
    private modalService: NgbModal,
    private auditModuleEntityService: AuditModuleEntityService,
    private auditModuleService: AuditModuleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditModuleEntityData?: AuditModuleEntityData) {

    if (auditModuleEntityData != null) {

      if (!this.auditModuleEntityService.userIsAuditorAuditModuleEntityReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Module Entities`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditModuleEntitySubmitData = this.auditModuleEntityService.ConvertToAuditModuleEntitySubmitData(auditModuleEntityData);
      this.isEditMode = true;

      this.buildFormValues(auditModuleEntityData);

    } else {

      if (!this.auditModuleEntityService.userIsAuditorAuditModuleEntityWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Module Entities`,
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
        this.auditModuleEntityForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditModuleEntityForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.auditModuleEntityModal, {
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

    if (this.auditModuleEntityService.userIsAuditorAuditModuleEntityWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Module Entities`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditModuleEntityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditModuleEntityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditModuleEntityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditModuleEntitySubmitData: AuditModuleEntitySubmitData = {
        id: this.auditModuleEntitySubmitData?.id || 0,
        auditModuleId: Number(formValue.auditModuleId),
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };

      if (this.isEditMode) {
        this.updateAuditModuleEntity(auditModuleEntitySubmitData);
      } else {
        this.addAuditModuleEntity(auditModuleEntitySubmitData);
      }
  }

  private addAuditModuleEntity(auditModuleEntityData: AuditModuleEntitySubmitData) {
    this.auditModuleEntityService.PostAuditModuleEntity(auditModuleEntityData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditModuleEntity) => {

        this.auditModuleEntityService.ClearAllCaches();

        this.auditModuleEntityChanged.next([newAuditModuleEntity]);

        this.alertService.showMessage("Audit Module Entity added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditmoduleentity', newAuditModuleEntity.id]);
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
                                   'You do not have permission to save this Audit Module Entity.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Module Entity.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Module Entity could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditModuleEntity(auditModuleEntityData: AuditModuleEntitySubmitData) {
    this.auditModuleEntityService.PutAuditModuleEntity(auditModuleEntityData.id, auditModuleEntityData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditModuleEntity) => {

        this.auditModuleEntityService.ClearAllCaches();

        this.auditModuleEntityChanged.next([updatedAuditModuleEntity]);

        this.alertService.showMessage("Audit Module Entity updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Module Entity.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Module Entity.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Module Entity could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditModuleEntityData: AuditModuleEntityData | null) {

    if (auditModuleEntityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditModuleEntityForm.reset({
        auditModuleId: null,
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditModuleEntityForm.reset({
        auditModuleId: auditModuleEntityData.auditModuleId,
        name: auditModuleEntityData.name ?? '',
        comments: auditModuleEntityData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditModuleEntityData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditModuleEntityForm.markAsPristine();
    this.auditModuleEntityForm.markAsUntouched();
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


  public userIsAuditorAuditModuleEntityReader(): boolean {
    return this.auditModuleEntityService.userIsAuditorAuditModuleEntityReader();
  }

  public userIsAuditorAuditModuleEntityWriter(): boolean {
    return this.auditModuleEntityService.userIsAuditorAuditModuleEntityWriter();
  }
}

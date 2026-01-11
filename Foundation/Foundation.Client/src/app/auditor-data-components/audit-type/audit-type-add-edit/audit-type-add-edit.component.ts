import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditTypeService, AuditTypeData, AuditTypeSubmitData } from '../../../auditor-data-services/audit-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-audit-type-add-edit',
  templateUrl: './audit-type-add-edit.component.html',
  styleUrls: ['./audit-type-add-edit.component.scss']
})
export class AuditTypeAddEditComponent {
  @ViewChild('auditTypeModal') auditTypeModal!: TemplateRef<any>;
  @Output() auditTypeChanged = new Subject<AuditTypeData[]>();
  @Input() auditTypeSubmitData: AuditTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  auditTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditTypes$ = this.auditTypeService.GetAuditTypeList();

  constructor(
    private modalService: NgbModal,
    private auditTypeService: AuditTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditTypeData?: AuditTypeData) {

    if (auditTypeData != null) {

      if (!this.auditTypeService.userIsAuditorAuditTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditTypeSubmitData = this.auditTypeService.ConvertToAuditTypeSubmitData(auditTypeData);
      this.isEditMode = true;

      this.buildFormValues(auditTypeData);

    } else {

      if (!this.auditTypeService.userIsAuditorAuditTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Types`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.auditTypeModal, {
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

    if (this.auditTypeService.userIsAuditorAuditTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditTypeSubmitData: AuditTypeSubmitData = {
        id: this.auditTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateAuditType(auditTypeSubmitData);
      } else {
        this.addAuditType(auditTypeSubmitData);
      }
  }

  private addAuditType(auditTypeData: AuditTypeSubmitData) {
    this.auditTypeService.PostAuditType(auditTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditType) => {

        this.auditTypeService.ClearAllCaches();

        this.auditTypeChanged.next([newAuditType]);

        this.alertService.showMessage("Audit Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/audittype', newAuditType.id]);
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
                                   'You do not have permission to save this Audit Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditType(auditTypeData: AuditTypeSubmitData) {
    this.auditTypeService.PutAuditType(auditTypeData.id, auditTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditType) => {

        this.auditTypeService.ClearAllCaches();

        this.auditTypeChanged.next([updatedAuditType]);

        this.alertService.showMessage("Audit Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditTypeData: AuditTypeData | null) {

    if (auditTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditTypeForm.reset({
        name: auditTypeData.name ?? '',
        description: auditTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.auditTypeForm.markAsPristine();
    this.auditTypeForm.markAsUntouched();
  }

  public userIsAuditorAuditTypeReader(): boolean {
    return this.auditTypeService.userIsAuditorAuditTypeReader();
  }

  public userIsAuditorAuditTypeWriter(): boolean {
    return this.auditTypeService.userIsAuditorAuditTypeWriter();
  }
}

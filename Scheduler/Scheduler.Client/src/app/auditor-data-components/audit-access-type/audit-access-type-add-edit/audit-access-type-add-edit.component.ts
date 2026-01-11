import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditAccessTypeService, AuditAccessTypeData, AuditAccessTypeSubmitData } from '../../../auditor-data-services/audit-access-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-audit-access-type-add-edit',
  templateUrl: './audit-access-type-add-edit.component.html',
  styleUrls: ['./audit-access-type-add-edit.component.scss']
})
export class AuditAccessTypeAddEditComponent {
  @ViewChild('auditAccessTypeModal') auditAccessTypeModal!: TemplateRef<any>;
  @Output() auditAccessTypeChanged = new Subject<AuditAccessTypeData[]>();
  @Input() auditAccessTypeSubmitData: AuditAccessTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  auditAccessTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditAccessTypes$ = this.auditAccessTypeService.GetAuditAccessTypeList();

  constructor(
    private modalService: NgbModal,
    private auditAccessTypeService: AuditAccessTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditAccessTypeData?: AuditAccessTypeData) {

    if (auditAccessTypeData != null) {

      if (!this.auditAccessTypeService.userIsAuditorAuditAccessTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Access Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditAccessTypeSubmitData = this.auditAccessTypeService.ConvertToAuditAccessTypeSubmitData(auditAccessTypeData);
      this.isEditMode = true;

      this.buildFormValues(auditAccessTypeData);

    } else {

      if (!this.auditAccessTypeService.userIsAuditorAuditAccessTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Access Types`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.auditAccessTypeModal, {
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

    if (this.auditAccessTypeService.userIsAuditorAuditAccessTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Access Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditAccessTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditAccessTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditAccessTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditAccessTypeSubmitData: AuditAccessTypeSubmitData = {
        id: this.auditAccessTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateAuditAccessType(auditAccessTypeSubmitData);
      } else {
        this.addAuditAccessType(auditAccessTypeSubmitData);
      }
  }

  private addAuditAccessType(auditAccessTypeData: AuditAccessTypeSubmitData) {
    this.auditAccessTypeService.PostAuditAccessType(auditAccessTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditAccessType) => {

        this.auditAccessTypeService.ClearAllCaches();

        this.auditAccessTypeChanged.next([newAuditAccessType]);

        this.alertService.showMessage("Audit Access Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditaccesstype', newAuditAccessType.id]);
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
                                   'You do not have permission to save this Audit Access Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Access Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Access Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditAccessType(auditAccessTypeData: AuditAccessTypeSubmitData) {
    this.auditAccessTypeService.PutAuditAccessType(auditAccessTypeData.id, auditAccessTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditAccessType) => {

        this.auditAccessTypeService.ClearAllCaches();

        this.auditAccessTypeChanged.next([updatedAuditAccessType]);

        this.alertService.showMessage("Audit Access Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Access Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Access Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Access Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditAccessTypeData: AuditAccessTypeData | null) {

    if (auditAccessTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditAccessTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditAccessTypeForm.reset({
        name: auditAccessTypeData.name ?? '',
        description: auditAccessTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.auditAccessTypeForm.markAsPristine();
    this.auditAccessTypeForm.markAsUntouched();
  }

  public userIsAuditorAuditAccessTypeReader(): boolean {
    return this.auditAccessTypeService.userIsAuditorAuditAccessTypeReader();
  }

  public userIsAuditorAuditAccessTypeWriter(): boolean {
    return this.auditAccessTypeService.userIsAuditorAuditAccessTypeWriter();
  }
}

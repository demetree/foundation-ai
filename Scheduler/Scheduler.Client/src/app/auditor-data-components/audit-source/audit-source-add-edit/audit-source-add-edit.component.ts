import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditSourceService, AuditSourceData, AuditSourceSubmitData } from '../../../auditor-data-services/audit-source.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-audit-source-add-edit',
  templateUrl: './audit-source-add-edit.component.html',
  styleUrls: ['./audit-source-add-edit.component.scss']
})
export class AuditSourceAddEditComponent {
  @ViewChild('auditSourceModal') auditSourceModal!: TemplateRef<any>;
  @Output() auditSourceChanged = new Subject<AuditSourceData[]>();
  @Input() auditSourceSubmitData: AuditSourceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  auditSourceForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditSources$ = this.auditSourceService.GetAuditSourceList();

  constructor(
    private modalService: NgbModal,
    private auditSourceService: AuditSourceService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditSourceData?: AuditSourceData) {

    if (auditSourceData != null) {

      if (!this.auditSourceService.userIsAuditorAuditSourceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Sources`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditSourceSubmitData = this.auditSourceService.ConvertToAuditSourceSubmitData(auditSourceData);
      this.isEditMode = true;

      this.buildFormValues(auditSourceData);

    } else {

      if (!this.auditSourceService.userIsAuditorAuditSourceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Sources`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.auditSourceModal, {
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

    if (this.auditSourceService.userIsAuditorAuditSourceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Sources`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditSourceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditSourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditSourceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditSourceSubmitData: AuditSourceSubmitData = {
        id: this.auditSourceSubmitData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };

      if (this.isEditMode) {
        this.updateAuditSource(auditSourceSubmitData);
      } else {
        this.addAuditSource(auditSourceSubmitData);
      }
  }

  private addAuditSource(auditSourceData: AuditSourceSubmitData) {
    this.auditSourceService.PostAuditSource(auditSourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditSource) => {

        this.auditSourceService.ClearAllCaches();

        this.auditSourceChanged.next([newAuditSource]);

        this.alertService.showMessage("Audit Source added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditsource', newAuditSource.id]);
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
                                   'You do not have permission to save this Audit Source.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Source.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Source could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditSource(auditSourceData: AuditSourceSubmitData) {
    this.auditSourceService.PutAuditSource(auditSourceData.id, auditSourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditSource) => {

        this.auditSourceService.ClearAllCaches();

        this.auditSourceChanged.next([updatedAuditSource]);

        this.alertService.showMessage("Audit Source updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Source.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Source.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Source could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditSourceData: AuditSourceData | null) {

    if (auditSourceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditSourceForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditSourceForm.reset({
        name: auditSourceData.name ?? '',
        comments: auditSourceData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditSourceData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditSourceForm.markAsPristine();
    this.auditSourceForm.markAsUntouched();
  }

  public userIsAuditorAuditSourceReader(): boolean {
    return this.auditSourceService.userIsAuditorAuditSourceReader();
  }

  public userIsAuditorAuditSourceWriter(): boolean {
    return this.auditSourceService.userIsAuditorAuditSourceWriter();
  }
}

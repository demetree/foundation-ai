import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditSessionService, AuditSessionData, AuditSessionSubmitData } from '../../../auditor-data-services/audit-session.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-audit-session-add-edit',
  templateUrl: './audit-session-add-edit.component.html',
  styleUrls: ['./audit-session-add-edit.component.scss']
})
export class AuditSessionAddEditComponent {
  @ViewChild('auditSessionModal') auditSessionModal!: TemplateRef<any>;
  @Output() auditSessionChanged = new Subject<AuditSessionData[]>();
  @Input() auditSessionSubmitData: AuditSessionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  auditSessionForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditSessions$ = this.auditSessionService.GetAuditSessionList();

  constructor(
    private modalService: NgbModal,
    private auditSessionService: AuditSessionService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditSessionData?: AuditSessionData) {

    if (auditSessionData != null) {

      if (!this.auditSessionService.userIsAuditorAuditSessionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Sessions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditSessionSubmitData = this.auditSessionService.ConvertToAuditSessionSubmitData(auditSessionData);
      this.isEditMode = true;

      this.buildFormValues(auditSessionData);

    } else {

      if (!this.auditSessionService.userIsAuditorAuditSessionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Sessions`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.auditSessionModal, {
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

    if (this.auditSessionService.userIsAuditorAuditSessionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Sessions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditSessionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditSessionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditSessionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditSessionSubmitData: AuditSessionSubmitData = {
        id: this.auditSessionSubmitData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };

      if (this.isEditMode) {
        this.updateAuditSession(auditSessionSubmitData);
      } else {
        this.addAuditSession(auditSessionSubmitData);
      }
  }

  private addAuditSession(auditSessionData: AuditSessionSubmitData) {
    this.auditSessionService.PostAuditSession(auditSessionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditSession) => {

        this.auditSessionService.ClearAllCaches();

        this.auditSessionChanged.next([newAuditSession]);

        this.alertService.showMessage("Audit Session added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditsession', newAuditSession.id]);
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
                                   'You do not have permission to save this Audit Session.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Session.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Session could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditSession(auditSessionData: AuditSessionSubmitData) {
    this.auditSessionService.PutAuditSession(auditSessionData.id, auditSessionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditSession) => {

        this.auditSessionService.ClearAllCaches();

        this.auditSessionChanged.next([updatedAuditSession]);

        this.alertService.showMessage("Audit Session updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Session.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Session.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Session could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditSessionData: AuditSessionData | null) {

    if (auditSessionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditSessionForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditSessionForm.reset({
        name: auditSessionData.name ?? '',
        comments: auditSessionData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditSessionData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditSessionForm.markAsPristine();
    this.auditSessionForm.markAsUntouched();
  }

  public userIsAuditorAuditSessionReader(): boolean {
    return this.auditSessionService.userIsAuditorAuditSessionReader();
  }

  public userIsAuditorAuditSessionWriter(): boolean {
    return this.auditSessionService.userIsAuditorAuditSessionWriter();
  }
}

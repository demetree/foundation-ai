import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditEventErrorMessageService, AuditEventErrorMessageData, AuditEventErrorMessageSubmitData } from '../../../auditor-data-services/audit-event-error-message.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-audit-event-error-message-add-edit',
  templateUrl: './audit-event-error-message-add-edit.component.html',
  styleUrls: ['./audit-event-error-message-add-edit.component.scss']
})
export class AuditEventErrorMessageAddEditComponent {
  @ViewChild('auditEventErrorMessageModal') auditEventErrorMessageModal!: TemplateRef<any>;
  @Output() auditEventErrorMessageChanged = new Subject<AuditEventErrorMessageData[]>();
  @Input() auditEventErrorMessageSubmitData: AuditEventErrorMessageSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  auditEventErrorMessageForm: FormGroup = this.fb.group({
        auditEventId: [null, Validators.required],
        errorMessage: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditEventErrorMessages$ = this.auditEventErrorMessageService.GetAuditEventErrorMessageList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  constructor(
    private modalService: NgbModal,
    private auditEventErrorMessageService: AuditEventErrorMessageService,
    private auditEventService: AuditEventService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditEventErrorMessageData?: AuditEventErrorMessageData) {

    if (auditEventErrorMessageData != null) {

      if (!this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Event Error Messages`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditEventErrorMessageSubmitData = this.auditEventErrorMessageService.ConvertToAuditEventErrorMessageSubmitData(auditEventErrorMessageData);
      this.isEditMode = true;

      this.buildFormValues(auditEventErrorMessageData);

    } else {

      if (!this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Event Error Messages`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.auditEventErrorMessageModal, {
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

    if (this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Event Error Messages`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditEventErrorMessageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditEventErrorMessageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditEventErrorMessageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditEventErrorMessageSubmitData: AuditEventErrorMessageSubmitData = {
        id: this.auditEventErrorMessageSubmitData?.id || 0,
        auditEventId: Number(formValue.auditEventId),
        errorMessage: formValue.errorMessage!.trim(),
   };

      if (this.isEditMode) {
        this.updateAuditEventErrorMessage(auditEventErrorMessageSubmitData);
      } else {
        this.addAuditEventErrorMessage(auditEventErrorMessageSubmitData);
      }
  }

  private addAuditEventErrorMessage(auditEventErrorMessageData: AuditEventErrorMessageSubmitData) {
    this.auditEventErrorMessageService.PostAuditEventErrorMessage(auditEventErrorMessageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditEventErrorMessage) => {

        this.auditEventErrorMessageService.ClearAllCaches();

        this.auditEventErrorMessageChanged.next([newAuditEventErrorMessage]);

        this.alertService.showMessage("Audit Event Error Message added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditeventerrormessage', newAuditEventErrorMessage.id]);
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
                                   'You do not have permission to save this Audit Event Error Message.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event Error Message.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event Error Message could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditEventErrorMessage(auditEventErrorMessageData: AuditEventErrorMessageSubmitData) {
    this.auditEventErrorMessageService.PutAuditEventErrorMessage(auditEventErrorMessageData.id, auditEventErrorMessageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditEventErrorMessage) => {

        this.auditEventErrorMessageService.ClearAllCaches();

        this.auditEventErrorMessageChanged.next([updatedAuditEventErrorMessage]);

        this.alertService.showMessage("Audit Event Error Message updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Event Error Message.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event Error Message.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event Error Message could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditEventErrorMessageData: AuditEventErrorMessageData | null) {

    if (auditEventErrorMessageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditEventErrorMessageForm.reset({
        auditEventId: null,
        errorMessage: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditEventErrorMessageForm.reset({
        auditEventId: auditEventErrorMessageData.auditEventId,
        errorMessage: auditEventErrorMessageData.errorMessage ?? '',
      }, { emitEvent: false});
    }

    this.auditEventErrorMessageForm.markAsPristine();
    this.auditEventErrorMessageForm.markAsUntouched();
  }

  public userIsAuditorAuditEventErrorMessageReader(): boolean {
    return this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageReader();
  }

  public userIsAuditorAuditEventErrorMessageWriter(): boolean {
    return this.auditEventErrorMessageService.userIsAuditorAuditEventErrorMessageWriter();
  }
}

import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ExternalCommunicationService, ExternalCommunicationData, ExternalCommunicationSubmitData } from '../../../auditor-data-services/external-communication.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuditUserService } from '../../../auditor-data-services/audit-user.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-external-communication-add-edit',
  templateUrl: './external-communication-add-edit.component.html',
  styleUrls: ['./external-communication-add-edit.component.scss']
})
export class ExternalCommunicationAddEditComponent {
  @ViewChild('externalCommunicationModal') externalCommunicationModal!: TemplateRef<any>;
  @Output() externalCommunicationChanged = new Subject<ExternalCommunicationData[]>();
  @Input() externalCommunicationSubmitData: ExternalCommunicationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  externalCommunicationForm: FormGroup = this.fb.group({
        timeStamp: [''],
        auditUserId: [null],
        communicationType: [''],
        subject: [''],
        message: [''],
        completedSuccessfully: [false],
        responseMessage: [''],
        exceptionText: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  externalCommunications$ = this.externalCommunicationService.GetExternalCommunicationList();
  auditUsers$ = this.auditUserService.GetAuditUserList();

  constructor(
    private modalService: NgbModal,
    private externalCommunicationService: ExternalCommunicationService,
    private auditUserService: AuditUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(externalCommunicationData?: ExternalCommunicationData) {

    if (externalCommunicationData != null) {

      if (!this.externalCommunicationService.userIsAuditorExternalCommunicationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read External Communications`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.externalCommunicationSubmitData = this.externalCommunicationService.ConvertToExternalCommunicationSubmitData(externalCommunicationData);
      this.isEditMode = true;

      this.buildFormValues(externalCommunicationData);

    } else {

      if (!this.externalCommunicationService.userIsAuditorExternalCommunicationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write External Communications`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.externalCommunicationModal, {
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

    if (this.externalCommunicationService.userIsAuditorExternalCommunicationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write External Communications`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.externalCommunicationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.externalCommunicationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.externalCommunicationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const externalCommunicationSubmitData: ExternalCommunicationSubmitData = {
        id: this.externalCommunicationSubmitData?.id || 0,
        timeStamp: formValue.timeStamp ? dateTimeLocalToIsoUtc(formValue.timeStamp.trim()) : null,
        auditUserId: formValue.auditUserId ? Number(formValue.auditUserId) : null,
        communicationType: formValue.communicationType?.trim() || null,
        subject: formValue.subject?.trim() || null,
        message: formValue.message?.trim() || null,
        completedSuccessfully: !!formValue.completedSuccessfully,
        responseMessage: formValue.responseMessage?.trim() || null,
        exceptionText: formValue.exceptionText?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateExternalCommunication(externalCommunicationSubmitData);
      } else {
        this.addExternalCommunication(externalCommunicationSubmitData);
      }
  }

  private addExternalCommunication(externalCommunicationData: ExternalCommunicationSubmitData) {
    this.externalCommunicationService.PostExternalCommunication(externalCommunicationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newExternalCommunication) => {

        this.externalCommunicationService.ClearAllCaches();

        this.externalCommunicationChanged.next([newExternalCommunication]);

        this.alertService.showMessage("External Communication added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/externalcommunication', newExternalCommunication.id]);
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
                                   'You do not have permission to save this External Communication.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateExternalCommunication(externalCommunicationData: ExternalCommunicationSubmitData) {
    this.externalCommunicationService.PutExternalCommunication(externalCommunicationData.id, externalCommunicationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedExternalCommunication) => {

        this.externalCommunicationService.ClearAllCaches();

        this.externalCommunicationChanged.next([updatedExternalCommunication]);

        this.alertService.showMessage("External Communication updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this External Communication.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(externalCommunicationData: ExternalCommunicationData | null) {

    if (externalCommunicationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.externalCommunicationForm.reset({
        timeStamp: '',
        auditUserId: null,
        communicationType: '',
        subject: '',
        message: '',
        completedSuccessfully: false,
        responseMessage: '',
        exceptionText: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.externalCommunicationForm.reset({
        timeStamp: isoUtcStringToDateTimeLocal(externalCommunicationData.timeStamp) ?? '',
        auditUserId: externalCommunicationData.auditUserId,
        communicationType: externalCommunicationData.communicationType ?? '',
        subject: externalCommunicationData.subject ?? '',
        message: externalCommunicationData.message ?? '',
        completedSuccessfully: externalCommunicationData.completedSuccessfully ?? false,
        responseMessage: externalCommunicationData.responseMessage ?? '',
        exceptionText: externalCommunicationData.exceptionText ?? '',
      }, { emitEvent: false});
    }

    this.externalCommunicationForm.markAsPristine();
    this.externalCommunicationForm.markAsUntouched();
  }

  public userIsAuditorExternalCommunicationReader(): boolean {
    return this.externalCommunicationService.userIsAuditorExternalCommunicationReader();
  }

  public userIsAuditorExternalCommunicationWriter(): boolean {
    return this.externalCommunicationService.userIsAuditorExternalCommunicationWriter();
  }
}

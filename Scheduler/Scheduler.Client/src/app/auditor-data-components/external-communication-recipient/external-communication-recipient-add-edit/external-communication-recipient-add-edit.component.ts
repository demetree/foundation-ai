import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ExternalCommunicationRecipientService, ExternalCommunicationRecipientData, ExternalCommunicationRecipientSubmitData } from '../../../auditor-data-services/external-communication-recipient.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ExternalCommunicationService } from '../../../auditor-data-services/external-communication.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-external-communication-recipient-add-edit',
  templateUrl: './external-communication-recipient-add-edit.component.html',
  styleUrls: ['./external-communication-recipient-add-edit.component.scss']
})
export class ExternalCommunicationRecipientAddEditComponent {
  @ViewChild('externalCommunicationRecipientModal') externalCommunicationRecipientModal!: TemplateRef<any>;
  @Output() externalCommunicationRecipientChanged = new Subject<ExternalCommunicationRecipientData[]>();
  @Input() externalCommunicationRecipientSubmitData: ExternalCommunicationRecipientSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  externalCommunicationRecipientForm: FormGroup = this.fb.group({
        externalCommunicationId: [null],
        recipient: [''],
        type: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  externalCommunicationRecipients$ = this.externalCommunicationRecipientService.GetExternalCommunicationRecipientList();
  externalCommunications$ = this.externalCommunicationService.GetExternalCommunicationList();

  constructor(
    private modalService: NgbModal,
    private externalCommunicationRecipientService: ExternalCommunicationRecipientService,
    private externalCommunicationService: ExternalCommunicationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(externalCommunicationRecipientData?: ExternalCommunicationRecipientData) {

    if (externalCommunicationRecipientData != null) {

      if (!this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read External Communication Recipients`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.externalCommunicationRecipientSubmitData = this.externalCommunicationRecipientService.ConvertToExternalCommunicationRecipientSubmitData(externalCommunicationRecipientData);
      this.isEditMode = true;

      this.buildFormValues(externalCommunicationRecipientData);

    } else {

      if (!this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write External Communication Recipients`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.externalCommunicationRecipientModal, {
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

    if (this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write External Communication Recipients`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.externalCommunicationRecipientForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.externalCommunicationRecipientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.externalCommunicationRecipientForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const externalCommunicationRecipientSubmitData: ExternalCommunicationRecipientSubmitData = {
        id: this.externalCommunicationRecipientSubmitData?.id || 0,
        externalCommunicationId: formValue.externalCommunicationId ? Number(formValue.externalCommunicationId) : null,
        recipient: formValue.recipient?.trim() || null,
        type: formValue.type?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateExternalCommunicationRecipient(externalCommunicationRecipientSubmitData);
      } else {
        this.addExternalCommunicationRecipient(externalCommunicationRecipientSubmitData);
      }
  }

  private addExternalCommunicationRecipient(externalCommunicationRecipientData: ExternalCommunicationRecipientSubmitData) {
    this.externalCommunicationRecipientService.PostExternalCommunicationRecipient(externalCommunicationRecipientData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newExternalCommunicationRecipient) => {

        this.externalCommunicationRecipientService.ClearAllCaches();

        this.externalCommunicationRecipientChanged.next([newExternalCommunicationRecipient]);

        this.alertService.showMessage("External Communication Recipient added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/externalcommunicationrecipient', newExternalCommunicationRecipient.id]);
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
                                   'You do not have permission to save this External Communication Recipient.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication Recipient.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication Recipient could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateExternalCommunicationRecipient(externalCommunicationRecipientData: ExternalCommunicationRecipientSubmitData) {
    this.externalCommunicationRecipientService.PutExternalCommunicationRecipient(externalCommunicationRecipientData.id, externalCommunicationRecipientData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedExternalCommunicationRecipient) => {

        this.externalCommunicationRecipientService.ClearAllCaches();

        this.externalCommunicationRecipientChanged.next([updatedExternalCommunicationRecipient]);

        this.alertService.showMessage("External Communication Recipient updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this External Communication Recipient.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the External Communication Recipient.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('External Communication Recipient could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(externalCommunicationRecipientData: ExternalCommunicationRecipientData | null) {

    if (externalCommunicationRecipientData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.externalCommunicationRecipientForm.reset({
        externalCommunicationId: null,
        recipient: '',
        type: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.externalCommunicationRecipientForm.reset({
        externalCommunicationId: externalCommunicationRecipientData.externalCommunicationId,
        recipient: externalCommunicationRecipientData.recipient ?? '',
        type: externalCommunicationRecipientData.type ?? '',
      }, { emitEvent: false});
    }

    this.externalCommunicationRecipientForm.markAsPristine();
    this.externalCommunicationRecipientForm.markAsUntouched();
  }

  public userIsAuditorExternalCommunicationRecipientReader(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientReader();
  }

  public userIsAuditorExternalCommunicationRecipientWriter(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter();
  }
}

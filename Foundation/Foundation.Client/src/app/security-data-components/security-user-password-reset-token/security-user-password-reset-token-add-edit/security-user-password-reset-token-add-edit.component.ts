import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserPasswordResetTokenService, SecurityUserPasswordResetTokenData, SecurityUserPasswordResetTokenSubmitData } from '../../../security-data-services/security-user-password-reset-token.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-user-password-reset-token-add-edit',
  templateUrl: './security-user-password-reset-token-add-edit.component.html',
  styleUrls: ['./security-user-password-reset-token-add-edit.component.scss']
})
export class SecurityUserPasswordResetTokenAddEditComponent {
  @ViewChild('securityUserPasswordResetTokenModal') securityUserPasswordResetTokenModal!: TemplateRef<any>;
  @Output() securityUserPasswordResetTokenChanged = new Subject<SecurityUserPasswordResetTokenData[]>();
  @Input() securityUserPasswordResetTokenSubmitData: SecurityUserPasswordResetTokenSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityUserPasswordResetTokenForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        token: ['', Validators.required],
        timeStamp: ['', Validators.required],
        expiry: ['', Validators.required],
        systemInitiated: [false],
        completed: [false],
        comments: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityUserPasswordResetTokens$ = this.securityUserPasswordResetTokenService.GetSecurityUserPasswordResetTokenList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  constructor(
    private modalService: NgbModal,
    private securityUserPasswordResetTokenService: SecurityUserPasswordResetTokenService,
    private securityUserService: SecurityUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityUserPasswordResetTokenData?: SecurityUserPasswordResetTokenData) {

    if (securityUserPasswordResetTokenData != null) {

      if (!this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security User Password Reset Tokens`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityUserPasswordResetTokenSubmitData = this.securityUserPasswordResetTokenService.ConvertToSecurityUserPasswordResetTokenSubmitData(securityUserPasswordResetTokenData);
      this.isEditMode = true;

      this.buildFormValues(securityUserPasswordResetTokenData);

    } else {

      if (!this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security User Password Reset Tokens`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityUserPasswordResetTokenModal, {
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

    if (this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security User Password Reset Tokens`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityUserPasswordResetTokenForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserPasswordResetTokenForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserPasswordResetTokenForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserPasswordResetTokenSubmitData: SecurityUserPasswordResetTokenSubmitData = {
        id: this.securityUserPasswordResetTokenSubmitData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        token: formValue.token!.trim(),
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        expiry: dateTimeLocalToIsoUtc(formValue.expiry!.trim())!,
        systemInitiated: !!formValue.systemInitiated,
        completed: !!formValue.completed,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityUserPasswordResetToken(securityUserPasswordResetTokenSubmitData);
      } else {
        this.addSecurityUserPasswordResetToken(securityUserPasswordResetTokenSubmitData);
      }
  }

  private addSecurityUserPasswordResetToken(securityUserPasswordResetTokenData: SecurityUserPasswordResetTokenSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityUserPasswordResetTokenData.active = true;
    securityUserPasswordResetTokenData.deleted = false;
    this.securityUserPasswordResetTokenService.PostSecurityUserPasswordResetToken(securityUserPasswordResetTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityUserPasswordResetToken) => {

        this.securityUserPasswordResetTokenService.ClearAllCaches();

        this.securityUserPasswordResetTokenChanged.next([newSecurityUserPasswordResetToken]);

        this.alertService.showMessage("Security User Password Reset Token added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityuserpasswordresettoken', newSecurityUserPasswordResetToken.id]);
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
                                   'You do not have permission to save this Security User Password Reset Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Password Reset Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Password Reset Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityUserPasswordResetToken(securityUserPasswordResetTokenData: SecurityUserPasswordResetTokenSubmitData) {
    this.securityUserPasswordResetTokenService.PutSecurityUserPasswordResetToken(securityUserPasswordResetTokenData.id, securityUserPasswordResetTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityUserPasswordResetToken) => {

        this.securityUserPasswordResetTokenService.ClearAllCaches();

        this.securityUserPasswordResetTokenChanged.next([updatedSecurityUserPasswordResetToken]);

        this.alertService.showMessage("Security User Password Reset Token updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security User Password Reset Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Password Reset Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Password Reset Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityUserPasswordResetTokenData: SecurityUserPasswordResetTokenData | null) {

    if (securityUserPasswordResetTokenData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserPasswordResetTokenForm.reset({
        securityUserId: null,
        token: '',
        timeStamp: '',
        expiry: '',
        systemInitiated: false,
        completed: false,
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserPasswordResetTokenForm.reset({
        securityUserId: securityUserPasswordResetTokenData.securityUserId,
        token: securityUserPasswordResetTokenData.token ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(securityUserPasswordResetTokenData.timeStamp) ?? '',
        expiry: isoUtcStringToDateTimeLocal(securityUserPasswordResetTokenData.expiry) ?? '',
        systemInitiated: securityUserPasswordResetTokenData.systemInitiated ?? false,
        completed: securityUserPasswordResetTokenData.completed ?? false,
        comments: securityUserPasswordResetTokenData.comments ?? '',
        active: securityUserPasswordResetTokenData.active ?? true,
        deleted: securityUserPasswordResetTokenData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserPasswordResetTokenForm.markAsPristine();
    this.securityUserPasswordResetTokenForm.markAsUntouched();
  }

  public userIsSecuritySecurityUserPasswordResetTokenReader(): boolean {
    return this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenReader();
  }

  public userIsSecuritySecurityUserPasswordResetTokenWriter(): boolean {
    return this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenWriter();
  }
}

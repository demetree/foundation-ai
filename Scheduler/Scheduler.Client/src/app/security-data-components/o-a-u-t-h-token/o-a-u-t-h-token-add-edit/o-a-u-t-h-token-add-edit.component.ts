import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OAUTHTokenService, OAUTHTokenData, OAUTHTokenSubmitData } from '../../../security-data-services/o-a-u-t-h-token.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-o-a-u-t-h-token-add-edit',
  templateUrl: './o-a-u-t-h-token-add-edit.component.html',
  styleUrls: ['./o-a-u-t-h-token-add-edit.component.scss']
})
export class OAUTHTokenAddEditComponent {
  @ViewChild('oAUTHTokenModal') oAUTHTokenModal!: TemplateRef<any>;
  @Output() oAUTHTokenChanged = new Subject<OAUTHTokenData[]>();
  @Input() oAUTHTokenSubmitData: OAUTHTokenSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  oAUTHTokenForm: FormGroup = this.fb.group({
        token: ['', Validators.required],
        expiryDateTime: ['', Validators.required],
        userData: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  oAUTHTokens$ = this.oAUTHTokenService.GetOAUTHTokenList();

  constructor(
    private modalService: NgbModal,
    private oAUTHTokenService: OAUTHTokenService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(oAUTHTokenData?: OAUTHTokenData) {

    if (oAUTHTokenData != null) {

      if (!this.oAUTHTokenService.userIsSecurityOAUTHTokenReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read O A U T H Tokens`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.oAUTHTokenSubmitData = this.oAUTHTokenService.ConvertToOAUTHTokenSubmitData(oAUTHTokenData);
      this.isEditMode = true;

      this.buildFormValues(oAUTHTokenData);

    } else {

      if (!this.oAUTHTokenService.userIsSecurityOAUTHTokenWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write O A U T H Tokens`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.oAUTHTokenModal, {
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

    if (this.oAUTHTokenService.userIsSecurityOAUTHTokenWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write O A U T H Tokens`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.oAUTHTokenForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.oAUTHTokenForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.oAUTHTokenForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const oAUTHTokenSubmitData: OAUTHTokenSubmitData = {
        id: this.oAUTHTokenSubmitData?.id || 0,
        token: formValue.token!.trim(),
        expiryDateTime: dateTimeLocalToIsoUtc(formValue.expiryDateTime!.trim())!,
        userData: formValue.userData?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateOAUTHToken(oAUTHTokenSubmitData);
      } else {
        this.addOAUTHToken(oAUTHTokenSubmitData);
      }
  }

  private addOAUTHToken(oAUTHTokenData: OAUTHTokenSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    oAUTHTokenData.active = true;
    oAUTHTokenData.deleted = false;
    this.oAUTHTokenService.PostOAUTHToken(oAUTHTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newOAUTHToken) => {

        this.oAUTHTokenService.ClearAllCaches();

        this.oAUTHTokenChanged.next([newOAUTHToken]);

        this.alertService.showMessage("O A U T H Token added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/oauthtoken', newOAUTHToken.id]);
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
                                   'You do not have permission to save this O A U T H Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the O A U T H Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('O A U T H Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateOAUTHToken(oAUTHTokenData: OAUTHTokenSubmitData) {
    this.oAUTHTokenService.PutOAUTHToken(oAUTHTokenData.id, oAUTHTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedOAUTHToken) => {

        this.oAUTHTokenService.ClearAllCaches();

        this.oAUTHTokenChanged.next([updatedOAUTHToken]);

        this.alertService.showMessage("O A U T H Token updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this O A U T H Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the O A U T H Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('O A U T H Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(oAUTHTokenData: OAUTHTokenData | null) {

    if (oAUTHTokenData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.oAUTHTokenForm.reset({
        token: '',
        expiryDateTime: '',
        userData: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.oAUTHTokenForm.reset({
        token: oAUTHTokenData.token ?? '',
        expiryDateTime: isoUtcStringToDateTimeLocal(oAUTHTokenData.expiryDateTime) ?? '',
        userData: oAUTHTokenData.userData ?? '',
        active: oAUTHTokenData.active ?? true,
        deleted: oAUTHTokenData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.oAUTHTokenForm.markAsPristine();
    this.oAUTHTokenForm.markAsUntouched();
  }

  public userIsSecurityOAUTHTokenReader(): boolean {
    return this.oAUTHTokenService.userIsSecurityOAUTHTokenReader();
  }

  public userIsSecurityOAUTHTokenWriter(): boolean {
    return this.oAUTHTokenService.userIsSecurityOAUTHTokenWriter();
  }
}

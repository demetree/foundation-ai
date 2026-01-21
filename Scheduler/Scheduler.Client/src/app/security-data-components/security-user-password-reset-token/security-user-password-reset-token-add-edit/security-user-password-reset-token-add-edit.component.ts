/*
   GENERATED FORM FOR THE SECURITYUSERPASSWORDRESETTOKEN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUserPasswordResetToken table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-password-reset-token-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
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

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityUserPasswordResetTokenFormValues {
  securityUserId: number | bigint,       // For FK link number
  token: string,
  timeStamp: string,
  expiry: string,
  systemInitiated: boolean,
  completed: boolean,
  comments: string | null,
  active: boolean,
  deleted: boolean,
};

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


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityUserPasswordResetTokenFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityUserPasswordResetTokenForm: FormGroup = this.fb.group({
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

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityUserPasswordResetTokenForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityUserPasswordResetTokenForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
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


  public userIsSecuritySecurityUserPasswordResetTokenReader(): boolean {
    return this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenReader();
  }

  public userIsSecuritySecurityUserPasswordResetTokenWriter(): boolean {
    return this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenWriter();
  }
}

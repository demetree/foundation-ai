/*
   GENERATED FORM FOR THE LOGINATTEMPT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LoginAttempt table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to login-attempt-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LoginAttemptService, LoginAttemptData, LoginAttemptSubmitData } from '../../../security-data-services/login-attempt.service';
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
interface LoginAttemptFormValues {
  timeStamp: string,
  userName: string | null,
  passwordHash: string | null,     // Stored as string for form input, converted to number on submit.
  resource: string | null,
  sessionId: string | null,
  ipAddress: string | null,
  userAgent: string | null,
  value: string | null,
  success: boolean | null,
  securityUserId: number | bigint | null,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-login-attempt-add-edit',
  templateUrl: './login-attempt-add-edit.component.html',
  styleUrls: ['./login-attempt-add-edit.component.scss']
})
export class LoginAttemptAddEditComponent {
  @ViewChild('loginAttemptModal') loginAttemptModal!: TemplateRef<any>;
  @Output() loginAttemptChanged = new Subject<LoginAttemptData[]>();
  @Input() loginAttemptSubmitData: LoginAttemptSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LoginAttemptFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public loginAttemptForm: FormGroup = this.fb.group({
        timeStamp: ['', Validators.required],
        userName: [''],
        passwordHash: [''],
        resource: [''],
        sessionId: [''],
        ipAddress: [''],
        userAgent: [''],
        value: [''],
        success: [false],
        securityUserId: [null],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  loginAttempts$ = this.loginAttemptService.GetLoginAttemptList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  constructor(
    private modalService: NgbModal,
    private loginAttemptService: LoginAttemptService,
    private securityUserService: SecurityUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(loginAttemptData?: LoginAttemptData) {

    if (loginAttemptData != null) {

      if (!this.loginAttemptService.userIsSecurityLoginAttemptReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Login Attempts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.loginAttemptSubmitData = this.loginAttemptService.ConvertToLoginAttemptSubmitData(loginAttemptData);
      this.isEditMode = true;

      this.buildFormValues(loginAttemptData);

    } else {

      if (!this.loginAttemptService.userIsSecurityLoginAttemptWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Login Attempts`,
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
        this.loginAttemptForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.loginAttemptForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.loginAttemptModal, {
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

    if (this.loginAttemptService.userIsSecurityLoginAttemptWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Login Attempts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.loginAttemptForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.loginAttemptForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.loginAttemptForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const loginAttemptSubmitData: LoginAttemptSubmitData = {
        id: this.loginAttemptSubmitData?.id || 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userName: formValue.userName?.trim() || null,
        passwordHash: formValue.passwordHash ? Number(formValue.passwordHash) : null,
        resource: formValue.resource?.trim() || null,
        sessionId: formValue.sessionId?.trim() || null,
        ipAddress: formValue.ipAddress?.trim() || null,
        userAgent: formValue.userAgent?.trim() || null,
        value: formValue.value?.trim() || null,
        success: formValue.success == true ? true : formValue.success == false ? false : null,
        securityUserId: formValue.securityUserId ? Number(formValue.securityUserId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateLoginAttempt(loginAttemptSubmitData);
      } else {
        this.addLoginAttempt(loginAttemptSubmitData);
      }
  }

  private addLoginAttempt(loginAttemptData: LoginAttemptSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    loginAttemptData.active = true;
    loginAttemptData.deleted = false;
    this.loginAttemptService.PostLoginAttempt(loginAttemptData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newLoginAttempt) => {

        this.loginAttemptService.ClearAllCaches();

        this.loginAttemptChanged.next([newLoginAttempt]);

        this.alertService.showMessage("Login Attempt added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/loginattempt', newLoginAttempt.id]);
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
                                   'You do not have permission to save this Login Attempt.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Login Attempt.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Login Attempt could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateLoginAttempt(loginAttemptData: LoginAttemptSubmitData) {
    this.loginAttemptService.PutLoginAttempt(loginAttemptData.id, loginAttemptData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedLoginAttempt) => {

        this.loginAttemptService.ClearAllCaches();

        this.loginAttemptChanged.next([updatedLoginAttempt]);

        this.alertService.showMessage("Login Attempt updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Login Attempt.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Login Attempt.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Login Attempt could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(loginAttemptData: LoginAttemptData | null) {

    if (loginAttemptData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.loginAttemptForm.reset({
        timeStamp: '',
        userName: '',
        passwordHash: '',
        resource: '',
        sessionId: '',
        ipAddress: '',
        userAgent: '',
        value: '',
        success: false,
        securityUserId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.loginAttemptForm.reset({
        timeStamp: isoUtcStringToDateTimeLocal(loginAttemptData.timeStamp) ?? '',
        userName: loginAttemptData.userName ?? '',
        passwordHash: loginAttemptData.passwordHash?.toString() ?? '',
        resource: loginAttemptData.resource ?? '',
        sessionId: loginAttemptData.sessionId ?? '',
        ipAddress: loginAttemptData.ipAddress ?? '',
        userAgent: loginAttemptData.userAgent ?? '',
        value: loginAttemptData.value ?? '',
        success: loginAttemptData.success ?? false,
        securityUserId: loginAttemptData.securityUserId,
        active: loginAttemptData.active ?? true,
        deleted: loginAttemptData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.loginAttemptForm.markAsPristine();
    this.loginAttemptForm.markAsUntouched();
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


  public userIsSecurityLoginAttemptReader(): boolean {
    return this.loginAttemptService.userIsSecurityLoginAttemptReader();
  }

  public userIsSecurityLoginAttemptWriter(): boolean {
    return this.loginAttemptService.userIsSecurityLoginAttemptWriter();
  }
}

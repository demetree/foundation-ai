/*
   GENERATED FORM FOR THE USERSESSION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSession table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-session-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserSessionService, UserSessionData, UserSessionSubmitData } from '../../../security-data-services/user-session.service';
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
interface UserSessionFormValues {
  securityUserId: number | bigint,       // For FK link number
  tokenId: string | null,
  sessionStart: string,
  expiresAt: string,
  ipAddress: string | null,
  userAgent: string | null,
  loginMethod: string | null,
  clientApplication: string | null,
  isRevoked: boolean,
  revokedAt: string | null,
  revokedBy: string | null,
  revokedReason: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-session-add-edit',
  templateUrl: './user-session-add-edit.component.html',
  styleUrls: ['./user-session-add-edit.component.scss']
})
export class UserSessionAddEditComponent {
  @ViewChild('userSessionModal') userSessionModal!: TemplateRef<any>;
  @Output() userSessionChanged = new Subject<UserSessionData[]>();
  @Input() userSessionSubmitData: UserSessionSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserSessionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userSessionForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        tokenId: [''],
        sessionStart: ['', Validators.required],
        expiresAt: ['', Validators.required],
        ipAddress: [''],
        userAgent: [''],
        loginMethod: [''],
        clientApplication: [''],
        isRevoked: [false],
        revokedAt: [''],
        revokedBy: [''],
        revokedReason: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userSessions$ = this.userSessionService.GetUserSessionList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  constructor(
    private modalService: NgbModal,
    private userSessionService: UserSessionService,
    private securityUserService: SecurityUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userSessionData?: UserSessionData) {

    if (userSessionData != null) {

      if (!this.userSessionService.userIsSecurityUserSessionReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Sessions`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userSessionSubmitData = this.userSessionService.ConvertToUserSessionSubmitData(userSessionData);
      this.isEditMode = true;
      this.objectGuid = userSessionData.objectGuid;

      this.buildFormValues(userSessionData);

    } else {

      if (!this.userSessionService.userIsSecurityUserSessionWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Sessions`,
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
        this.userSessionForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userSessionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userSessionModal, {
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

    if (this.userSessionService.userIsSecurityUserSessionWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Sessions`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userSessionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userSessionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userSessionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userSessionSubmitData: UserSessionSubmitData = {
        id: this.userSessionSubmitData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        tokenId: formValue.tokenId?.trim() || null,
        sessionStart: dateTimeLocalToIsoUtc(formValue.sessionStart!.trim())!,
        expiresAt: dateTimeLocalToIsoUtc(formValue.expiresAt!.trim())!,
        ipAddress: formValue.ipAddress?.trim() || null,
        userAgent: formValue.userAgent?.trim() || null,
        loginMethod: formValue.loginMethod?.trim() || null,
        clientApplication: formValue.clientApplication?.trim() || null,
        isRevoked: !!formValue.isRevoked,
        revokedAt: formValue.revokedAt ? dateTimeLocalToIsoUtc(formValue.revokedAt.trim()) : null,
        revokedBy: formValue.revokedBy?.trim() || null,
        revokedReason: formValue.revokedReason?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserSession(userSessionSubmitData);
      } else {
        this.addUserSession(userSessionSubmitData);
      }
  }

  private addUserSession(userSessionData: UserSessionSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userSessionData.active = true;
    userSessionData.deleted = false;
    this.userSessionService.PostUserSession(userSessionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserSession) => {

        this.userSessionService.ClearAllCaches();

        this.userSessionChanged.next([newUserSession]);

        this.alertService.showMessage("User Session added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/usersession', newUserSession.id]);
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
                                   'You do not have permission to save this User Session.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Session.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Session could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserSession(userSessionData: UserSessionSubmitData) {
    this.userSessionService.PutUserSession(userSessionData.id, userSessionData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserSession) => {

        this.userSessionService.ClearAllCaches();

        this.userSessionChanged.next([updatedUserSession]);

        this.alertService.showMessage("User Session updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Session.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Session.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Session could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userSessionData: UserSessionData | null) {

    if (userSessionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userSessionForm.reset({
        securityUserId: null,
        tokenId: '',
        sessionStart: '',
        expiresAt: '',
        ipAddress: '',
        userAgent: '',
        loginMethod: '',
        clientApplication: '',
        isRevoked: false,
        revokedAt: '',
        revokedBy: '',
        revokedReason: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userSessionForm.reset({
        securityUserId: userSessionData.securityUserId,
        tokenId: userSessionData.tokenId ?? '',
        sessionStart: isoUtcStringToDateTimeLocal(userSessionData.sessionStart) ?? '',
        expiresAt: isoUtcStringToDateTimeLocal(userSessionData.expiresAt) ?? '',
        ipAddress: userSessionData.ipAddress ?? '',
        userAgent: userSessionData.userAgent ?? '',
        loginMethod: userSessionData.loginMethod ?? '',
        clientApplication: userSessionData.clientApplication ?? '',
        isRevoked: userSessionData.isRevoked ?? false,
        revokedAt: isoUtcStringToDateTimeLocal(userSessionData.revokedAt) ?? '',
        revokedBy: userSessionData.revokedBy ?? '',
        revokedReason: userSessionData.revokedReason ?? '',
        active: userSessionData.active ?? true,
        deleted: userSessionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userSessionForm.markAsPristine();
    this.userSessionForm.markAsUntouched();
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


  public userIsSecurityUserSessionReader(): boolean {
    return this.userSessionService.userIsSecurityUserSessionReader();
  }

  public userIsSecurityUserSessionWriter(): boolean {
    return this.userSessionService.userIsSecurityUserSessionWriter();
  }
}

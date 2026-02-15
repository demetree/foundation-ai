/*
   GENERATED FORM FOR THE PENDINGREGISTRATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PendingRegistration table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to pending-registration-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PendingRegistrationService, PendingRegistrationData, PendingRegistrationSubmitData } from '../../../bmc-data-services/pending-registration.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PendingRegistrationFormValues {
  accountName: string,
  emailAddress: string,
  displayName: string | null,
  passwordHash: string,
  verificationCode: string,
  codeExpiresAt: string,
  verificationAttempts: string,     // Stored as string for form input, converted to number on submit.
  status: string,
  createdAt: string,
  verifiedAt: string | null,
  provisionedAt: string | null,
  ipAddress: string | null,
  userAgent: string | null,
  verificationChannel: string | null,
  failureReason: string | null,
  provisionedSecurityUserId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-pending-registration-add-edit',
  templateUrl: './pending-registration-add-edit.component.html',
  styleUrls: ['./pending-registration-add-edit.component.scss']
})
export class PendingRegistrationAddEditComponent {
  @ViewChild('pendingRegistrationModal') pendingRegistrationModal!: TemplateRef<any>;
  @Output() pendingRegistrationChanged = new Subject<PendingRegistrationData[]>();
  @Input() pendingRegistrationSubmitData: PendingRegistrationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PendingRegistrationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pendingRegistrationForm: FormGroup = this.fb.group({
        accountName: ['', Validators.required],
        emailAddress: ['', Validators.required],
        displayName: [''],
        passwordHash: ['', Validators.required],
        verificationCode: ['', Validators.required],
        codeExpiresAt: ['', Validators.required],
        verificationAttempts: ['', Validators.required],
        status: ['', Validators.required],
        createdAt: ['', Validators.required],
        verifiedAt: [''],
        provisionedAt: [''],
        ipAddress: [''],
        userAgent: [''],
        verificationChannel: [''],
        failureReason: [''],
        provisionedSecurityUserId: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  pendingRegistrations$ = this.pendingRegistrationService.GetPendingRegistrationList();

  constructor(
    private modalService: NgbModal,
    private pendingRegistrationService: PendingRegistrationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(pendingRegistrationData?: PendingRegistrationData) {

    if (pendingRegistrationData != null) {

      if (!this.pendingRegistrationService.userIsBMCPendingRegistrationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Pending Registrations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.pendingRegistrationSubmitData = this.pendingRegistrationService.ConvertToPendingRegistrationSubmitData(pendingRegistrationData);
      this.isEditMode = true;
      this.objectGuid = pendingRegistrationData.objectGuid;

      this.buildFormValues(pendingRegistrationData);

    } else {

      if (!this.pendingRegistrationService.userIsBMCPendingRegistrationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Pending Registrations`,
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
        this.pendingRegistrationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pendingRegistrationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.pendingRegistrationModal, {
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

    if (this.pendingRegistrationService.userIsBMCPendingRegistrationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Pending Registrations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.pendingRegistrationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pendingRegistrationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pendingRegistrationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pendingRegistrationSubmitData: PendingRegistrationSubmitData = {
        id: this.pendingRegistrationSubmitData?.id || 0,
        accountName: formValue.accountName!.trim(),
        emailAddress: formValue.emailAddress!.trim(),
        displayName: formValue.displayName?.trim() || null,
        passwordHash: formValue.passwordHash!.trim(),
        verificationCode: formValue.verificationCode!.trim(),
        codeExpiresAt: dateTimeLocalToIsoUtc(formValue.codeExpiresAt!.trim())!,
        verificationAttempts: Number(formValue.verificationAttempts),
        status: formValue.status!.trim(),
        createdAt: dateTimeLocalToIsoUtc(formValue.createdAt!.trim())!,
        verifiedAt: formValue.verifiedAt ? dateTimeLocalToIsoUtc(formValue.verifiedAt.trim()) : null,
        provisionedAt: formValue.provisionedAt ? dateTimeLocalToIsoUtc(formValue.provisionedAt.trim()) : null,
        ipAddress: formValue.ipAddress?.trim() || null,
        userAgent: formValue.userAgent?.trim() || null,
        verificationChannel: formValue.verificationChannel?.trim() || null,
        failureReason: formValue.failureReason?.trim() || null,
        provisionedSecurityUserId: formValue.provisionedSecurityUserId ? Number(formValue.provisionedSecurityUserId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePendingRegistration(pendingRegistrationSubmitData);
      } else {
        this.addPendingRegistration(pendingRegistrationSubmitData);
      }
  }

  private addPendingRegistration(pendingRegistrationData: PendingRegistrationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    pendingRegistrationData.active = true;
    pendingRegistrationData.deleted = false;
    this.pendingRegistrationService.PostPendingRegistration(pendingRegistrationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPendingRegistration) => {

        this.pendingRegistrationService.ClearAllCaches();

        this.pendingRegistrationChanged.next([newPendingRegistration]);

        this.alertService.showMessage("Pending Registration added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/pendingregistration', newPendingRegistration.id]);
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
                                   'You do not have permission to save this Pending Registration.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Pending Registration.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Pending Registration could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePendingRegistration(pendingRegistrationData: PendingRegistrationSubmitData) {
    this.pendingRegistrationService.PutPendingRegistration(pendingRegistrationData.id, pendingRegistrationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPendingRegistration) => {

        this.pendingRegistrationService.ClearAllCaches();

        this.pendingRegistrationChanged.next([updatedPendingRegistration]);

        this.alertService.showMessage("Pending Registration updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Pending Registration.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Pending Registration.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Pending Registration could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(pendingRegistrationData: PendingRegistrationData | null) {

    if (pendingRegistrationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pendingRegistrationForm.reset({
        accountName: '',
        emailAddress: '',
        displayName: '',
        passwordHash: '',
        verificationCode: '',
        codeExpiresAt: '',
        verificationAttempts: '',
        status: '',
        createdAt: '',
        verifiedAt: '',
        provisionedAt: '',
        ipAddress: '',
        userAgent: '',
        verificationChannel: '',
        failureReason: '',
        provisionedSecurityUserId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pendingRegistrationForm.reset({
        accountName: pendingRegistrationData.accountName ?? '',
        emailAddress: pendingRegistrationData.emailAddress ?? '',
        displayName: pendingRegistrationData.displayName ?? '',
        passwordHash: pendingRegistrationData.passwordHash ?? '',
        verificationCode: pendingRegistrationData.verificationCode ?? '',
        codeExpiresAt: isoUtcStringToDateTimeLocal(pendingRegistrationData.codeExpiresAt) ?? '',
        verificationAttempts: pendingRegistrationData.verificationAttempts?.toString() ?? '',
        status: pendingRegistrationData.status ?? '',
        createdAt: isoUtcStringToDateTimeLocal(pendingRegistrationData.createdAt) ?? '',
        verifiedAt: isoUtcStringToDateTimeLocal(pendingRegistrationData.verifiedAt) ?? '',
        provisionedAt: isoUtcStringToDateTimeLocal(pendingRegistrationData.provisionedAt) ?? '',
        ipAddress: pendingRegistrationData.ipAddress ?? '',
        userAgent: pendingRegistrationData.userAgent ?? '',
        verificationChannel: pendingRegistrationData.verificationChannel ?? '',
        failureReason: pendingRegistrationData.failureReason ?? '',
        provisionedSecurityUserId: pendingRegistrationData.provisionedSecurityUserId?.toString() ?? '',
        active: pendingRegistrationData.active ?? true,
        deleted: pendingRegistrationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.pendingRegistrationForm.markAsPristine();
    this.pendingRegistrationForm.markAsUntouched();
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


  public userIsBMCPendingRegistrationReader(): boolean {
    return this.pendingRegistrationService.userIsBMCPendingRegistrationReader();
  }

  public userIsBMCPendingRegistrationWriter(): boolean {
    return this.pendingRegistrationService.userIsBMCPendingRegistrationWriter();
  }
}

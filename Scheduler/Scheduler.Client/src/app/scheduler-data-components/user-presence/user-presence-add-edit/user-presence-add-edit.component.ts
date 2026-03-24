/*
   GENERATED FORM FOR THE USERPRESENCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserPresence table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-presence-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserPresenceService, UserPresenceData, UserPresenceSubmitData } from '../../../scheduler-data-services/user-presence.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface UserPresenceFormValues {
  userId: string,     // Stored as string for form input, converted to number on submit.
  status: string,
  customStatusMessage: string | null,
  lastSeenDateTime: string,
  lastActivityDateTime: string,
  connectionCount: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-user-presence-add-edit',
  templateUrl: './user-presence-add-edit.component.html',
  styleUrls: ['./user-presence-add-edit.component.scss']
})
export class UserPresenceAddEditComponent {
  @ViewChild('userPresenceModal') userPresenceModal!: TemplateRef<any>;
  @Output() userPresenceChanged = new Subject<UserPresenceData[]>();
  @Input() userPresenceSubmitData: UserPresenceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserPresenceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userPresenceForm: FormGroup = this.fb.group({
        userId: ['', Validators.required],
        status: ['', Validators.required],
        customStatusMessage: [''],
        lastSeenDateTime: ['', Validators.required],
        lastActivityDateTime: ['', Validators.required],
        connectionCount: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  userPresences$ = this.userPresenceService.GetUserPresenceList();

  constructor(
    private modalService: NgbModal,
    private userPresenceService: UserPresenceService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(userPresenceData?: UserPresenceData) {

    if (userPresenceData != null) {

      if (!this.userPresenceService.userIsSchedulerUserPresenceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read User Presences`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.userPresenceSubmitData = this.userPresenceService.ConvertToUserPresenceSubmitData(userPresenceData);
      this.isEditMode = true;
      this.objectGuid = userPresenceData.objectGuid;

      this.buildFormValues(userPresenceData);

    } else {

      if (!this.userPresenceService.userIsSchedulerUserPresenceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write User Presences`,
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
        this.userPresenceForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userPresenceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.userPresenceModal, {
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

    if (this.userPresenceService.userIsSchedulerUserPresenceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write User Presences`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.userPresenceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userPresenceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userPresenceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userPresenceSubmitData: UserPresenceSubmitData = {
        id: this.userPresenceSubmitData?.id || 0,
        userId: Number(formValue.userId),
        status: formValue.status!.trim(),
        customStatusMessage: formValue.customStatusMessage?.trim() || null,
        lastSeenDateTime: dateTimeLocalToIsoUtc(formValue.lastSeenDateTime!.trim())!,
        lastActivityDateTime: dateTimeLocalToIsoUtc(formValue.lastActivityDateTime!.trim())!,
        connectionCount: Number(formValue.connectionCount),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateUserPresence(userPresenceSubmitData);
      } else {
        this.addUserPresence(userPresenceSubmitData);
      }
  }

  private addUserPresence(userPresenceData: UserPresenceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    userPresenceData.active = true;
    userPresenceData.deleted = false;
    this.userPresenceService.PostUserPresence(userPresenceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newUserPresence) => {

        this.userPresenceService.ClearAllCaches();

        this.userPresenceChanged.next([newUserPresence]);

        this.alertService.showMessage("User Presence added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/userpresence', newUserPresence.id]);
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
                                   'You do not have permission to save this User Presence.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Presence.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Presence could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateUserPresence(userPresenceData: UserPresenceSubmitData) {
    this.userPresenceService.PutUserPresence(userPresenceData.id, userPresenceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedUserPresence) => {

        this.userPresenceService.ClearAllCaches();

        this.userPresenceChanged.next([updatedUserPresence]);

        this.alertService.showMessage("User Presence updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this User Presence.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Presence.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Presence could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(userPresenceData: UserPresenceData | null) {

    if (userPresenceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userPresenceForm.reset({
        userId: '',
        status: '',
        customStatusMessage: '',
        lastSeenDateTime: '',
        lastActivityDateTime: '',
        connectionCount: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userPresenceForm.reset({
        userId: userPresenceData.userId?.toString() ?? '',
        status: userPresenceData.status ?? '',
        customStatusMessage: userPresenceData.customStatusMessage ?? '',
        lastSeenDateTime: isoUtcStringToDateTimeLocal(userPresenceData.lastSeenDateTime) ?? '',
        lastActivityDateTime: isoUtcStringToDateTimeLocal(userPresenceData.lastActivityDateTime) ?? '',
        connectionCount: userPresenceData.connectionCount?.toString() ?? '',
        active: userPresenceData.active ?? true,
        deleted: userPresenceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userPresenceForm.markAsPristine();
    this.userPresenceForm.markAsUntouched();
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


  public userIsSchedulerUserPresenceReader(): boolean {
    return this.userPresenceService.userIsSchedulerUserPresenceReader();
  }

  public userIsSchedulerUserPresenceWriter(): boolean {
    return this.userPresenceService.userIsSchedulerUserPresenceWriter();
  }
}

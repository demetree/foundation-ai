/*
   GENERATED FORM FOR THE SECURITYUSEREVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUserEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserEventService, SecurityUserEventData, SecurityUserEventSubmitData } from '../../../security-data-services/security-user-event.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityUserEventTypeService } from '../../../security-data-services/security-user-event-type.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SecurityUserEventFormValues {
  securityUserId: number | bigint,       // For FK link number
  securityUserEventTypeId: number | bigint,       // For FK link number
  timeStamp: string,
  comments: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-security-user-event-add-edit',
  templateUrl: './security-user-event-add-edit.component.html',
  styleUrls: ['./security-user-event-add-edit.component.scss']
})
export class SecurityUserEventAddEditComponent {
  @ViewChild('securityUserEventModal') securityUserEventModal!: TemplateRef<any>;
  @Output() securityUserEventChanged = new Subject<SecurityUserEventData[]>();
  @Input() securityUserEventSubmitData: SecurityUserEventSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityUserEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityUserEventForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        securityUserEventTypeId: [null, Validators.required],
        timeStamp: ['', Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityUserEvents$ = this.securityUserEventService.GetSecurityUserEventList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();
  securityUserEventTypes$ = this.securityUserEventTypeService.GetSecurityUserEventTypeList();

  constructor(
    private modalService: NgbModal,
    private securityUserEventService: SecurityUserEventService,
    private securityUserService: SecurityUserService,
    private securityUserEventTypeService: SecurityUserEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityUserEventData?: SecurityUserEventData) {

    if (securityUserEventData != null) {

      if (!this.securityUserEventService.userIsSecuritySecurityUserEventReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security User Events`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityUserEventSubmitData = this.securityUserEventService.ConvertToSecurityUserEventSubmitData(securityUserEventData);
      this.isEditMode = true;

      this.buildFormValues(securityUserEventData);

    } else {

      if (!this.securityUserEventService.userIsSecuritySecurityUserEventWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security User Events`,
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
        this.securityUserEventForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityUserEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.securityUserEventModal, {
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

    if (this.securityUserEventService.userIsSecuritySecurityUserEventWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security User Events`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityUserEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserEventSubmitData: SecurityUserEventSubmitData = {
        id: this.securityUserEventSubmitData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        securityUserEventTypeId: Number(formValue.securityUserEventTypeId),
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityUserEvent(securityUserEventSubmitData);
      } else {
        this.addSecurityUserEvent(securityUserEventSubmitData);
      }
  }

  private addSecurityUserEvent(securityUserEventData: SecurityUserEventSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityUserEventData.active = true;
    securityUserEventData.deleted = false;
    this.securityUserEventService.PostSecurityUserEvent(securityUserEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityUserEvent) => {

        this.securityUserEventService.ClearAllCaches();

        this.securityUserEventChanged.next([newSecurityUserEvent]);

        this.alertService.showMessage("Security User Event added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityuserevent', newSecurityUserEvent.id]);
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
                                   'You do not have permission to save this Security User Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityUserEvent(securityUserEventData: SecurityUserEventSubmitData) {
    this.securityUserEventService.PutSecurityUserEvent(securityUserEventData.id, securityUserEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityUserEvent) => {

        this.securityUserEventService.ClearAllCaches();

        this.securityUserEventChanged.next([updatedSecurityUserEvent]);

        this.alertService.showMessage("Security User Event updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security User Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityUserEventData: SecurityUserEventData | null) {

    if (securityUserEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserEventForm.reset({
        securityUserId: null,
        securityUserEventTypeId: null,
        timeStamp: '',
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserEventForm.reset({
        securityUserId: securityUserEventData.securityUserId,
        securityUserEventTypeId: securityUserEventData.securityUserEventTypeId,
        timeStamp: isoUtcStringToDateTimeLocal(securityUserEventData.timeStamp) ?? '',
        comments: securityUserEventData.comments ?? '',
        active: securityUserEventData.active ?? true,
        deleted: securityUserEventData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserEventForm.markAsPristine();
    this.securityUserEventForm.markAsUntouched();
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


  public userIsSecuritySecurityUserEventReader(): boolean {
    return this.securityUserEventService.userIsSecuritySecurityUserEventReader();
  }

  public userIsSecuritySecurityUserEventWriter(): boolean {
    return this.securityUserEventService.userIsSecuritySecurityUserEventWriter();
  }
}

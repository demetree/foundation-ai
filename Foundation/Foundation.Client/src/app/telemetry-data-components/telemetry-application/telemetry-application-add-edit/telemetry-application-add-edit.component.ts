/*
   GENERATED FORM FOR THE TELEMETRYAPPLICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryApplication table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-application-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryApplicationService, TelemetryApplicationData, TelemetryApplicationSubmitData } from '../../../telemetry-data-services/telemetry-application.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TelemetryApplicationFormValues {
  name: string,
  url: string | null,
  isSelf: boolean,
  firstSeen: string,
  lastSeen: string | null,
};

@Component({
  selector: 'app-telemetry-application-add-edit',
  templateUrl: './telemetry-application-add-edit.component.html',
  styleUrls: ['./telemetry-application-add-edit.component.scss']
})
export class TelemetryApplicationAddEditComponent {
  @ViewChild('telemetryApplicationModal') telemetryApplicationModal!: TemplateRef<any>;
  @Output() telemetryApplicationChanged = new Subject<TelemetryApplicationData[]>();
  @Input() telemetryApplicationSubmitData: TelemetryApplicationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryApplicationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryApplicationForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        url: [''],
        isSelf: [false],
        firstSeen: ['', Validators.required],
        lastSeen: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  telemetryApplications$ = this.telemetryApplicationService.GetTelemetryApplicationList();

  constructor(
    private modalService: NgbModal,
    private telemetryApplicationService: TelemetryApplicationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(telemetryApplicationData?: TelemetryApplicationData) {

    if (telemetryApplicationData != null) {

      if (!this.telemetryApplicationService.userIsTelemetryTelemetryApplicationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Telemetry Applications`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.telemetryApplicationSubmitData = this.telemetryApplicationService.ConvertToTelemetryApplicationSubmitData(telemetryApplicationData);
      this.isEditMode = true;

      this.buildFormValues(telemetryApplicationData);

    } else {

      if (!this.telemetryApplicationService.userIsTelemetryTelemetryApplicationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Telemetry Applications`,
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
        this.telemetryApplicationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryApplicationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.telemetryApplicationModal, {
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

    if (this.telemetryApplicationService.userIsTelemetryTelemetryApplicationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Telemetry Applications`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.telemetryApplicationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryApplicationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryApplicationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryApplicationSubmitData: TelemetryApplicationSubmitData = {
        id: this.telemetryApplicationSubmitData?.id || 0,
        name: formValue.name!.trim(),
        url: formValue.url?.trim() || null,
        isSelf: !!formValue.isSelf,
        firstSeen: dateTimeLocalToIsoUtc(formValue.firstSeen!.trim())!,
        lastSeen: formValue.lastSeen ? dateTimeLocalToIsoUtc(formValue.lastSeen.trim()) : null,
   };

      if (this.isEditMode) {
        this.updateTelemetryApplication(telemetryApplicationSubmitData);
      } else {
        this.addTelemetryApplication(telemetryApplicationSubmitData);
      }
  }

  private addTelemetryApplication(telemetryApplicationData: TelemetryApplicationSubmitData) {
    this.telemetryApplicationService.PostTelemetryApplication(telemetryApplicationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTelemetryApplication) => {

        this.telemetryApplicationService.ClearAllCaches();

        this.telemetryApplicationChanged.next([newTelemetryApplication]);

        this.alertService.showMessage("Telemetry Application added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/telemetryapplication', newTelemetryApplication.id]);
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
                                   'You do not have permission to save this Telemetry Application.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Application.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Application could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTelemetryApplication(telemetryApplicationData: TelemetryApplicationSubmitData) {
    this.telemetryApplicationService.PutTelemetryApplication(telemetryApplicationData.id, telemetryApplicationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTelemetryApplication) => {

        this.telemetryApplicationService.ClearAllCaches();

        this.telemetryApplicationChanged.next([updatedTelemetryApplication]);

        this.alertService.showMessage("Telemetry Application updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Telemetry Application.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Application.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Application could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(telemetryApplicationData: TelemetryApplicationData | null) {

    if (telemetryApplicationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryApplicationForm.reset({
        name: '',
        url: '',
        isSelf: false,
        firstSeen: '',
        lastSeen: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryApplicationForm.reset({
        name: telemetryApplicationData.name ?? '',
        url: telemetryApplicationData.url ?? '',
        isSelf: telemetryApplicationData.isSelf ?? false,
        firstSeen: isoUtcStringToDateTimeLocal(telemetryApplicationData.firstSeen) ?? '',
        lastSeen: isoUtcStringToDateTimeLocal(telemetryApplicationData.lastSeen) ?? '',
      }, { emitEvent: false});
    }

    this.telemetryApplicationForm.markAsPristine();
    this.telemetryApplicationForm.markAsUntouched();
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


  public userIsTelemetryTelemetryApplicationReader(): boolean {
    return this.telemetryApplicationService.userIsTelemetryTelemetryApplicationReader();
  }

  public userIsTelemetryTelemetryApplicationWriter(): boolean {
    return this.telemetryApplicationService.userIsTelemetryTelemetryApplicationWriter();
  }
}

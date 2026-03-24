/*
   GENERATED FORM FOR THE PUSHPROVIDERCONFIGURATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PushProviderConfiguration table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to push-provider-configuration-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PushProviderConfigurationService, PushProviderConfigurationData, PushProviderConfigurationSubmitData } from '../../../scheduler-data-services/push-provider-configuration.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PushProviderConfigurationFormValues {
  providerId: string,
  enabled: boolean,
  configurationJson: string | null,
  dateTimeModified: string,
  modifiedByUserId: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-push-provider-configuration-add-edit',
  templateUrl: './push-provider-configuration-add-edit.component.html',
  styleUrls: ['./push-provider-configuration-add-edit.component.scss']
})
export class PushProviderConfigurationAddEditComponent {
  @ViewChild('pushProviderConfigurationModal') pushProviderConfigurationModal!: TemplateRef<any>;
  @Output() pushProviderConfigurationChanged = new Subject<PushProviderConfigurationData[]>();
  @Input() pushProviderConfigurationSubmitData: PushProviderConfigurationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PushProviderConfigurationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pushProviderConfigurationForm: FormGroup = this.fb.group({
        providerId: ['', Validators.required],
        enabled: [false],
        configurationJson: [''],
        dateTimeModified: ['', Validators.required],
        modifiedByUserId: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  pushProviderConfigurations$ = this.pushProviderConfigurationService.GetPushProviderConfigurationList();

  constructor(
    private modalService: NgbModal,
    private pushProviderConfigurationService: PushProviderConfigurationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(pushProviderConfigurationData?: PushProviderConfigurationData) {

    if (pushProviderConfigurationData != null) {

      if (!this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Push Provider Configurations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.pushProviderConfigurationSubmitData = this.pushProviderConfigurationService.ConvertToPushProviderConfigurationSubmitData(pushProviderConfigurationData);
      this.isEditMode = true;
      this.objectGuid = pushProviderConfigurationData.objectGuid;

      this.buildFormValues(pushProviderConfigurationData);

    } else {

      if (!this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Push Provider Configurations`,
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
        this.pushProviderConfigurationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pushProviderConfigurationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.pushProviderConfigurationModal, {
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

    if (this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Push Provider Configurations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.pushProviderConfigurationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pushProviderConfigurationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pushProviderConfigurationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pushProviderConfigurationSubmitData: PushProviderConfigurationSubmitData = {
        id: this.pushProviderConfigurationSubmitData?.id || 0,
        providerId: formValue.providerId!.trim(),
        enabled: !!formValue.enabled,
        configurationJson: formValue.configurationJson?.trim() || null,
        dateTimeModified: dateTimeLocalToIsoUtc(formValue.dateTimeModified!.trim())!,
        modifiedByUserId: Number(formValue.modifiedByUserId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePushProviderConfiguration(pushProviderConfigurationSubmitData);
      } else {
        this.addPushProviderConfiguration(pushProviderConfigurationSubmitData);
      }
  }

  private addPushProviderConfiguration(pushProviderConfigurationData: PushProviderConfigurationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    pushProviderConfigurationData.active = true;
    pushProviderConfigurationData.deleted = false;
    this.pushProviderConfigurationService.PostPushProviderConfiguration(pushProviderConfigurationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPushProviderConfiguration) => {

        this.pushProviderConfigurationService.ClearAllCaches();

        this.pushProviderConfigurationChanged.next([newPushProviderConfiguration]);

        this.alertService.showMessage("Push Provider Configuration added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/pushproviderconfiguration', newPushProviderConfiguration.id]);
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
                                   'You do not have permission to save this Push Provider Configuration.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Push Provider Configuration.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Push Provider Configuration could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePushProviderConfiguration(pushProviderConfigurationData: PushProviderConfigurationSubmitData) {
    this.pushProviderConfigurationService.PutPushProviderConfiguration(pushProviderConfigurationData.id, pushProviderConfigurationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPushProviderConfiguration) => {

        this.pushProviderConfigurationService.ClearAllCaches();

        this.pushProviderConfigurationChanged.next([updatedPushProviderConfiguration]);

        this.alertService.showMessage("Push Provider Configuration updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Push Provider Configuration.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Push Provider Configuration.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Push Provider Configuration could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(pushProviderConfigurationData: PushProviderConfigurationData | null) {

    if (pushProviderConfigurationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pushProviderConfigurationForm.reset({
        providerId: '',
        enabled: false,
        configurationJson: '',
        dateTimeModified: '',
        modifiedByUserId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pushProviderConfigurationForm.reset({
        providerId: pushProviderConfigurationData.providerId ?? '',
        enabled: pushProviderConfigurationData.enabled ?? false,
        configurationJson: pushProviderConfigurationData.configurationJson ?? '',
        dateTimeModified: isoUtcStringToDateTimeLocal(pushProviderConfigurationData.dateTimeModified) ?? '',
        modifiedByUserId: pushProviderConfigurationData.modifiedByUserId?.toString() ?? '',
        active: pushProviderConfigurationData.active ?? true,
        deleted: pushProviderConfigurationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.pushProviderConfigurationForm.markAsPristine();
    this.pushProviderConfigurationForm.markAsUntouched();
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


  public userIsSchedulerPushProviderConfigurationReader(): boolean {
    return this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationReader();
  }

  public userIsSchedulerPushProviderConfigurationWriter(): boolean {
    return this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationWriter();
  }
}

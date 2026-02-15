/*
   GENERATED FORM FOR THE APIREQUESTLOG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ApiRequestLog table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to api-request-log-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ApiRequestLogService, ApiRequestLogData, ApiRequestLogSubmitData } from '../../../bmc-data-services/api-request-log.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ApiKeyService } from '../../../bmc-data-services/api-key.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ApiRequestLogFormValues {
  apiKeyId: number | bigint,       // For FK link number
  endpoint: string,
  httpMethod: string,
  responseStatus: string,     // Stored as string for form input, converted to number on submit.
  requestDate: string,
  durationMs: string | null,     // Stored as string for form input, converted to number on submit.
  clientIpAddress: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-api-request-log-add-edit',
  templateUrl: './api-request-log-add-edit.component.html',
  styleUrls: ['./api-request-log-add-edit.component.scss']
})
export class ApiRequestLogAddEditComponent {
  @ViewChild('apiRequestLogModal') apiRequestLogModal!: TemplateRef<any>;
  @Output() apiRequestLogChanged = new Subject<ApiRequestLogData[]>();
  @Input() apiRequestLogSubmitData: ApiRequestLogSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ApiRequestLogFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public apiRequestLogForm: FormGroup = this.fb.group({
        apiKeyId: [null, Validators.required],
        endpoint: ['', Validators.required],
        httpMethod: ['', Validators.required],
        responseStatus: ['', Validators.required],
        requestDate: ['', Validators.required],
        durationMs: [''],
        clientIpAddress: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  apiRequestLogs$ = this.apiRequestLogService.GetApiRequestLogList();
  apiKeies$ = this.apiKeyService.GetApiKeyList();

  constructor(
    private modalService: NgbModal,
    private apiRequestLogService: ApiRequestLogService,
    private apiKeyService: ApiKeyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(apiRequestLogData?: ApiRequestLogData) {

    if (apiRequestLogData != null) {

      if (!this.apiRequestLogService.userIsBMCApiRequestLogReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Api Request Logs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.apiRequestLogSubmitData = this.apiRequestLogService.ConvertToApiRequestLogSubmitData(apiRequestLogData);
      this.isEditMode = true;
      this.objectGuid = apiRequestLogData.objectGuid;

      this.buildFormValues(apiRequestLogData);

    } else {

      if (!this.apiRequestLogService.userIsBMCApiRequestLogWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Api Request Logs`,
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
        this.apiRequestLogForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.apiRequestLogForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.apiRequestLogModal, {
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

    if (this.apiRequestLogService.userIsBMCApiRequestLogWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Api Request Logs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.apiRequestLogForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.apiRequestLogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.apiRequestLogForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const apiRequestLogSubmitData: ApiRequestLogSubmitData = {
        id: this.apiRequestLogSubmitData?.id || 0,
        apiKeyId: Number(formValue.apiKeyId),
        endpoint: formValue.endpoint!.trim(),
        httpMethod: formValue.httpMethod!.trim(),
        responseStatus: Number(formValue.responseStatus),
        requestDate: dateTimeLocalToIsoUtc(formValue.requestDate!.trim())!,
        durationMs: formValue.durationMs ? Number(formValue.durationMs) : null,
        clientIpAddress: formValue.clientIpAddress?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateApiRequestLog(apiRequestLogSubmitData);
      } else {
        this.addApiRequestLog(apiRequestLogSubmitData);
      }
  }

  private addApiRequestLog(apiRequestLogData: ApiRequestLogSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    apiRequestLogData.active = true;
    apiRequestLogData.deleted = false;
    this.apiRequestLogService.PostApiRequestLog(apiRequestLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newApiRequestLog) => {

        this.apiRequestLogService.ClearAllCaches();

        this.apiRequestLogChanged.next([newApiRequestLog]);

        this.alertService.showMessage("Api Request Log added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/apirequestlog', newApiRequestLog.id]);
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
                                   'You do not have permission to save this Api Request Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Api Request Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Api Request Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateApiRequestLog(apiRequestLogData: ApiRequestLogSubmitData) {
    this.apiRequestLogService.PutApiRequestLog(apiRequestLogData.id, apiRequestLogData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedApiRequestLog) => {

        this.apiRequestLogService.ClearAllCaches();

        this.apiRequestLogChanged.next([updatedApiRequestLog]);

        this.alertService.showMessage("Api Request Log updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Api Request Log.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Api Request Log.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Api Request Log could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(apiRequestLogData: ApiRequestLogData | null) {

    if (apiRequestLogData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.apiRequestLogForm.reset({
        apiKeyId: null,
        endpoint: '',
        httpMethod: '',
        responseStatus: '',
        requestDate: '',
        durationMs: '',
        clientIpAddress: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.apiRequestLogForm.reset({
        apiKeyId: apiRequestLogData.apiKeyId,
        endpoint: apiRequestLogData.endpoint ?? '',
        httpMethod: apiRequestLogData.httpMethod ?? '',
        responseStatus: apiRequestLogData.responseStatus?.toString() ?? '',
        requestDate: isoUtcStringToDateTimeLocal(apiRequestLogData.requestDate) ?? '',
        durationMs: apiRequestLogData.durationMs?.toString() ?? '',
        clientIpAddress: apiRequestLogData.clientIpAddress ?? '',
        active: apiRequestLogData.active ?? true,
        deleted: apiRequestLogData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.apiRequestLogForm.markAsPristine();
    this.apiRequestLogForm.markAsUntouched();
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


  public userIsBMCApiRequestLogReader(): boolean {
    return this.apiRequestLogService.userIsBMCApiRequestLogReader();
  }

  public userIsBMCApiRequestLogWriter(): boolean {
    return this.apiRequestLogService.userIsBMCApiRequestLogWriter();
  }
}

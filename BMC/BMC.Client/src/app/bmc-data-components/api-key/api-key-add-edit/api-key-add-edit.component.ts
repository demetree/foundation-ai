/*
   GENERATED FORM FOR THE APIKEY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ApiKey table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to api-key-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ApiKeyService, ApiKeyData, ApiKeySubmitData } from '../../../bmc-data-services/api-key.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ApiKeyFormValues {
  keyHash: string,
  keyPrefix: string,
  name: string,
  description: string | null,
  isActive: boolean,
  createdDate: string,
  lastUsedDate: string | null,
  expiresDate: string | null,
  rateLimitPerHour: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-api-key-add-edit',
  templateUrl: './api-key-add-edit.component.html',
  styleUrls: ['./api-key-add-edit.component.scss']
})
export class ApiKeyAddEditComponent {
  @ViewChild('apiKeyModal') apiKeyModal!: TemplateRef<any>;
  @Output() apiKeyChanged = new Subject<ApiKeyData[]>();
  @Input() apiKeySubmitData: ApiKeySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ApiKeyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public apiKeyForm: FormGroup = this.fb.group({
        keyHash: ['', Validators.required],
        keyPrefix: ['', Validators.required],
        name: ['', Validators.required],
        description: [''],
        isActive: [false],
        createdDate: ['', Validators.required],
        lastUsedDate: [''],
        expiresDate: [''],
        rateLimitPerHour: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  apiKeies$ = this.apiKeyService.GetApiKeyList();

  constructor(
    private modalService: NgbModal,
    private apiKeyService: ApiKeyService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(apiKeyData?: ApiKeyData) {

    if (apiKeyData != null) {

      if (!this.apiKeyService.userIsBMCApiKeyReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Api Keies`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.apiKeySubmitData = this.apiKeyService.ConvertToApiKeySubmitData(apiKeyData);
      this.isEditMode = true;
      this.objectGuid = apiKeyData.objectGuid;

      this.buildFormValues(apiKeyData);

    } else {

      if (!this.apiKeyService.userIsBMCApiKeyWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Api Keies`,
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
        this.apiKeyForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.apiKeyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.apiKeyModal, {
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

    if (this.apiKeyService.userIsBMCApiKeyWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Api Keies`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.apiKeyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.apiKeyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.apiKeyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const apiKeySubmitData: ApiKeySubmitData = {
        id: this.apiKeySubmitData?.id || 0,
        keyHash: formValue.keyHash!.trim(),
        keyPrefix: formValue.keyPrefix!.trim(),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        isActive: !!formValue.isActive,
        createdDate: dateTimeLocalToIsoUtc(formValue.createdDate!.trim())!,
        lastUsedDate: formValue.lastUsedDate ? dateTimeLocalToIsoUtc(formValue.lastUsedDate.trim()) : null,
        expiresDate: formValue.expiresDate ? dateTimeLocalToIsoUtc(formValue.expiresDate.trim()) : null,
        rateLimitPerHour: Number(formValue.rateLimitPerHour),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateApiKey(apiKeySubmitData);
      } else {
        this.addApiKey(apiKeySubmitData);
      }
  }

  private addApiKey(apiKeyData: ApiKeySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    apiKeyData.active = true;
    apiKeyData.deleted = false;
    this.apiKeyService.PostApiKey(apiKeyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newApiKey) => {

        this.apiKeyService.ClearAllCaches();

        this.apiKeyChanged.next([newApiKey]);

        this.alertService.showMessage("Api Key added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/apikey', newApiKey.id]);
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
                                   'You do not have permission to save this Api Key.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Api Key.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Api Key could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateApiKey(apiKeyData: ApiKeySubmitData) {
    this.apiKeyService.PutApiKey(apiKeyData.id, apiKeyData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedApiKey) => {

        this.apiKeyService.ClearAllCaches();

        this.apiKeyChanged.next([updatedApiKey]);

        this.alertService.showMessage("Api Key updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Api Key.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Api Key.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Api Key could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(apiKeyData: ApiKeyData | null) {

    if (apiKeyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.apiKeyForm.reset({
        keyHash: '',
        keyPrefix: '',
        name: '',
        description: '',
        isActive: false,
        createdDate: '',
        lastUsedDate: '',
        expiresDate: '',
        rateLimitPerHour: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.apiKeyForm.reset({
        keyHash: apiKeyData.keyHash ?? '',
        keyPrefix: apiKeyData.keyPrefix ?? '',
        name: apiKeyData.name ?? '',
        description: apiKeyData.description ?? '',
        isActive: apiKeyData.isActive ?? false,
        createdDate: isoUtcStringToDateTimeLocal(apiKeyData.createdDate) ?? '',
        lastUsedDate: isoUtcStringToDateTimeLocal(apiKeyData.lastUsedDate) ?? '',
        expiresDate: isoUtcStringToDateTimeLocal(apiKeyData.expiresDate) ?? '',
        rateLimitPerHour: apiKeyData.rateLimitPerHour?.toString() ?? '',
        active: apiKeyData.active ?? true,
        deleted: apiKeyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.apiKeyForm.markAsPristine();
    this.apiKeyForm.markAsUntouched();
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


  public userIsBMCApiKeyReader(): boolean {
    return this.apiKeyService.userIsBMCApiKeyReader();
  }

  public userIsBMCApiKeyWriter(): boolean {
    return this.apiKeyService.userIsBMCApiKeyWriter();
  }
}

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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ApiKeyService, ApiKeyData, ApiKeySubmitData } from '../../../bmc-data-services/api-key.service';
import { ApiRequestLogService } from '../../../bmc-data-services/api-request-log.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
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
  selector: 'app-api-key-detail',
  templateUrl: './api-key-detail.component.html',
  styleUrls: ['./api-key-detail.component.scss']
})

export class ApiKeyDetailComponent implements OnInit, CanComponentDeactivate {


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


  public apiKeyId: string | null = null;
  public apiKeyData: ApiKeyData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  apiKeys$ = this.apiKeyService.GetApiKeyList();
  public apiRequestLogs$ = this.apiRequestLogService.GetApiRequestLogList();

  private destroy$ = new Subject<void>();

  constructor(
    public apiKeyService: ApiKeyService,
    public apiRequestLogService: ApiRequestLogService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the apiKeyId from the route parameters
    this.apiKeyId = this.route.snapshot.paramMap.get('apiKeyId');

    if (this.apiKeyId === 'new' ||
        this.apiKeyId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.apiKeyData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.apiKeyForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Api Key';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Api Key';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.apiKeyForm.dirty) {
      return confirm('You have unsaved Api Key changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.apiKeyId != null && this.apiKeyId !== 'new') {

      const id = parseInt(this.apiKeyId, 10);

      if (!isNaN(id)) {
        return { apiKeyId: id };
      }
    }

    return null;
  }


/*
  * Loads the ApiKey data for the current apiKeyId.
  *
  * Fully respects the ApiKeyService caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.apiKeyService.userIsBMCApiKeyReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ApiKeys.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate apiKeyId
    //
    if (!this.apiKeyId) {

      this.alertService.showMessage('No ApiKey ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const apiKeyId = Number(this.apiKeyId);

    if (isNaN(apiKeyId) || apiKeyId <= 0) {

      this.alertService.showMessage(`Invalid Api Key ID: "${this.apiKeyId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this ApiKey + relations

      this.apiKeyService.ClearRecordCache(apiKeyId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.apiKeyService.GetApiKey(apiKeyId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (apiKeyData) => {

        //
        // Success path — apiKeyData can legitimately be null if 404'd but request succeeded
        //
        if (!apiKeyData) {

          this.handleApiKeyNotFound(apiKeyId);

        } else {

          this.apiKeyData = apiKeyData;
          this.buildFormValues(this.apiKeyData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ApiKey loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleApiKeyLoadError(error, apiKeyId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleApiKeyNotFound(apiKeyId: number): void {

    this.apiKeyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ApiKey #${apiKeyId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleApiKeyLoadError(error: any, apiKeyId: number): void {

    let message = 'Failed to load Api Key.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Api Key.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Api Key #${apiKeyId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Api Key load failed (ID: ${apiKeyId})`, error);

    //
    // Reset UI to safe state
    //
    this.apiKeyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
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


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.apiKeyService.userIsBMCApiKeyWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Api Keys", 'Access Denied', MessageSeverity.info);
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
        id: this.apiKeyData?.id || 0,
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


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.apiKeyService.PutApiKey(apiKeySubmitData.id, apiKeySubmitData)
      : this.apiKeyService.PostApiKey(apiKeySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedApiKeyData) => {

        this.apiKeyService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Api Key's detail page
          //
          this.apiKeyForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.apiKeyForm.markAsUntouched();

          this.router.navigate(['/apikeys', savedApiKeyData.id]);
          this.alertService.showMessage('Api Key added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.apiKeyData = savedApiKeyData;
          this.buildFormValues(this.apiKeyData);

          this.alertService.showMessage("Api Key saved successfully", '', MessageSeverity.success);
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

  public userIsBMCApiKeyReader(): boolean {
    return this.apiKeyService.userIsBMCApiKeyReader();
  }

  public userIsBMCApiKeyWriter(): boolean {
    return this.apiKeyService.userIsBMCApiKeyWriter();
  }
}

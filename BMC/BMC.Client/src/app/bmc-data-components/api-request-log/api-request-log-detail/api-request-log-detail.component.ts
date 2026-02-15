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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ApiRequestLogService, ApiRequestLogData, ApiRequestLogSubmitData } from '../../../bmc-data-services/api-request-log.service';
import { ApiKeyService } from '../../../bmc-data-services/api-key.service';
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
  selector: 'app-api-request-log-detail',
  templateUrl: './api-request-log-detail.component.html',
  styleUrls: ['./api-request-log-detail.component.scss']
})

export class ApiRequestLogDetailComponent implements OnInit, CanComponentDeactivate {


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


  public apiRequestLogId: string | null = null;
  public apiRequestLogData: ApiRequestLogData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  apiRequestLogs$ = this.apiRequestLogService.GetApiRequestLogList();
  public apiKeys$ = this.apiKeyService.GetApiKeyList();

  private destroy$ = new Subject<void>();

  constructor(
    public apiRequestLogService: ApiRequestLogService,
    public apiKeyService: ApiKeyService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the apiRequestLogId from the route parameters
    this.apiRequestLogId = this.route.snapshot.paramMap.get('apiRequestLogId');

    if (this.apiRequestLogId === 'new' ||
        this.apiRequestLogId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.apiRequestLogData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.apiRequestLogForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Api Request Log';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Api Request Log';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.apiRequestLogForm.dirty) {
      return confirm('You have unsaved Api Request Log changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.apiRequestLogId != null && this.apiRequestLogId !== 'new') {

      const id = parseInt(this.apiRequestLogId, 10);

      if (!isNaN(id)) {
        return { apiRequestLogId: id };
      }
    }

    return null;
  }


/*
  * Loads the ApiRequestLog data for the current apiRequestLogId.
  *
  * Fully respects the ApiRequestLogService caching strategy and error handling strategy.
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
    if (!this.apiRequestLogService.userIsBMCApiRequestLogReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ApiRequestLogs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate apiRequestLogId
    //
    if (!this.apiRequestLogId) {

      this.alertService.showMessage('No ApiRequestLog ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const apiRequestLogId = Number(this.apiRequestLogId);

    if (isNaN(apiRequestLogId) || apiRequestLogId <= 0) {

      this.alertService.showMessage(`Invalid Api Request Log ID: "${this.apiRequestLogId}"`,
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
      // This is the most targeted way: clear only this ApiRequestLog + relations

      this.apiRequestLogService.ClearRecordCache(apiRequestLogId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.apiRequestLogService.GetApiRequestLog(apiRequestLogId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (apiRequestLogData) => {

        //
        // Success path — apiRequestLogData can legitimately be null if 404'd but request succeeded
        //
        if (!apiRequestLogData) {

          this.handleApiRequestLogNotFound(apiRequestLogId);

        } else {

          this.apiRequestLogData = apiRequestLogData;
          this.buildFormValues(this.apiRequestLogData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ApiRequestLog loaded successfully',
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
        this.handleApiRequestLogLoadError(error, apiRequestLogId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleApiRequestLogNotFound(apiRequestLogId: number): void {

    this.apiRequestLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ApiRequestLog #${apiRequestLogId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleApiRequestLogLoadError(error: any, apiRequestLogId: number): void {

    let message = 'Failed to load Api Request Log.';
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
          message = 'You do not have permission to view this Api Request Log.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Api Request Log #${apiRequestLogId} was not found.`;
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

    console.error(`Api Request Log load failed (ID: ${apiRequestLogId})`, error);

    //
    // Reset UI to safe state
    //
    this.apiRequestLogData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

    if (this.apiRequestLogService.userIsBMCApiRequestLogWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Api Request Logs", 'Access Denied', MessageSeverity.info);
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
        id: this.apiRequestLogData?.id || 0,
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


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.apiRequestLogService.PutApiRequestLog(apiRequestLogSubmitData.id, apiRequestLogSubmitData)
      : this.apiRequestLogService.PostApiRequestLog(apiRequestLogSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedApiRequestLogData) => {

        this.apiRequestLogService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Api Request Log's detail page
          //
          this.apiRequestLogForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.apiRequestLogForm.markAsUntouched();

          this.router.navigate(['/apirequestlogs', savedApiRequestLogData.id]);
          this.alertService.showMessage('Api Request Log added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.apiRequestLogData = savedApiRequestLogData;
          this.buildFormValues(this.apiRequestLogData);

          this.alertService.showMessage("Api Request Log saved successfully", '', MessageSeverity.success);
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

  public userIsBMCApiRequestLogReader(): boolean {
    return this.apiRequestLogService.userIsBMCApiRequestLogReader();
  }

  public userIsBMCApiRequestLogWriter(): boolean {
    return this.apiRequestLogService.userIsBMCApiRequestLogWriter();
  }
}

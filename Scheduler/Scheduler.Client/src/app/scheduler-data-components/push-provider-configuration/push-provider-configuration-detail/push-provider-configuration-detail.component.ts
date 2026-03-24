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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PushProviderConfigurationService, PushProviderConfigurationData, PushProviderConfigurationSubmitData } from '../../../scheduler-data-services/push-provider-configuration.service';
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
  selector: 'app-push-provider-configuration-detail',
  templateUrl: './push-provider-configuration-detail.component.html',
  styleUrls: ['./push-provider-configuration-detail.component.scss']
})

export class PushProviderConfigurationDetailComponent implements OnInit, CanComponentDeactivate {


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


  public pushProviderConfigurationId: string | null = null;
  public pushProviderConfigurationData: PushProviderConfigurationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  pushProviderConfigurations$ = this.pushProviderConfigurationService.GetPushProviderConfigurationList();

  private destroy$ = new Subject<void>();

  constructor(
    public pushProviderConfigurationService: PushProviderConfigurationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the pushProviderConfigurationId from the route parameters
    this.pushProviderConfigurationId = this.route.snapshot.paramMap.get('pushProviderConfigurationId');

    if (this.pushProviderConfigurationId === 'new' ||
        this.pushProviderConfigurationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.pushProviderConfigurationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.pushProviderConfigurationForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Push Provider Configuration';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Push Provider Configuration';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.pushProviderConfigurationForm.dirty) {
      return confirm('You have unsaved Push Provider Configuration changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.pushProviderConfigurationId != null && this.pushProviderConfigurationId !== 'new') {

      const id = parseInt(this.pushProviderConfigurationId, 10);

      if (!isNaN(id)) {
        return { pushProviderConfigurationId: id };
      }
    }

    return null;
  }


/*
  * Loads the PushProviderConfiguration data for the current pushProviderConfigurationId.
  *
  * Fully respects the PushProviderConfigurationService caching strategy and error handling strategy.
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
    if (!this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PushProviderConfigurations.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate pushProviderConfigurationId
    //
    if (!this.pushProviderConfigurationId) {

      this.alertService.showMessage('No PushProviderConfiguration ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const pushProviderConfigurationId = Number(this.pushProviderConfigurationId);

    if (isNaN(pushProviderConfigurationId) || pushProviderConfigurationId <= 0) {

      this.alertService.showMessage(`Invalid Push Provider Configuration ID: "${this.pushProviderConfigurationId}"`,
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
      // This is the most targeted way: clear only this PushProviderConfiguration + relations

      this.pushProviderConfigurationService.ClearRecordCache(pushProviderConfigurationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.pushProviderConfigurationService.GetPushProviderConfiguration(pushProviderConfigurationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (pushProviderConfigurationData) => {

        //
        // Success path — pushProviderConfigurationData can legitimately be null if 404'd but request succeeded
        //
        if (!pushProviderConfigurationData) {

          this.handlePushProviderConfigurationNotFound(pushProviderConfigurationId);

        } else {

          this.pushProviderConfigurationData = pushProviderConfigurationData;
          this.buildFormValues(this.pushProviderConfigurationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PushProviderConfiguration loaded successfully',
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
        this.handlePushProviderConfigurationLoadError(error, pushProviderConfigurationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePushProviderConfigurationNotFound(pushProviderConfigurationId: number): void {

    this.pushProviderConfigurationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PushProviderConfiguration #${pushProviderConfigurationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePushProviderConfigurationLoadError(error: any, pushProviderConfigurationId: number): void {

    let message = 'Failed to load Push Provider Configuration.';
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
          message = 'You do not have permission to view this Push Provider Configuration.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Push Provider Configuration #${pushProviderConfigurationId} was not found.`;
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

    console.error(`Push Provider Configuration load failed (ID: ${pushProviderConfigurationId})`, error);

    //
    // Reset UI to safe state
    //
    this.pushProviderConfigurationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

    if (this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Push Provider Configurations", 'Access Denied', MessageSeverity.info);
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
        id: this.pushProviderConfigurationData?.id || 0,
        providerId: formValue.providerId!.trim(),
        enabled: !!formValue.enabled,
        configurationJson: formValue.configurationJson?.trim() || null,
        dateTimeModified: dateTimeLocalToIsoUtc(formValue.dateTimeModified!.trim())!,
        modifiedByUserId: Number(formValue.modifiedByUserId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.pushProviderConfigurationService.PutPushProviderConfiguration(pushProviderConfigurationSubmitData.id, pushProviderConfigurationSubmitData)
      : this.pushProviderConfigurationService.PostPushProviderConfiguration(pushProviderConfigurationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPushProviderConfigurationData) => {

        this.pushProviderConfigurationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Push Provider Configuration's detail page
          //
          this.pushProviderConfigurationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.pushProviderConfigurationForm.markAsUntouched();

          this.router.navigate(['/pushproviderconfigurations', savedPushProviderConfigurationData.id]);
          this.alertService.showMessage('Push Provider Configuration added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.pushProviderConfigurationData = savedPushProviderConfigurationData;
          this.buildFormValues(this.pushProviderConfigurationData);

          this.alertService.showMessage("Push Provider Configuration saved successfully", '', MessageSeverity.success);
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

  public userIsSchedulerPushProviderConfigurationReader(): boolean {
    return this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationReader();
  }

  public userIsSchedulerPushProviderConfigurationWriter(): boolean {
    return this.pushProviderConfigurationService.userIsSchedulerPushProviderConfigurationWriter();
  }
}

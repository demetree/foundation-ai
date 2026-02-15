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
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PendingRegistrationService, PendingRegistrationData, PendingRegistrationSubmitData } from '../../../bmc-data-services/pending-registration.service';
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
  selector: 'app-pending-registration-detail',
  templateUrl: './pending-registration-detail.component.html',
  styleUrls: ['./pending-registration-detail.component.scss']
})

export class PendingRegistrationDetailComponent implements OnInit, CanComponentDeactivate {


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


  public pendingRegistrationId: string | null = null;
  public pendingRegistrationData: PendingRegistrationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  pendingRegistrations$ = this.pendingRegistrationService.GetPendingRegistrationList();

  private destroy$ = new Subject<void>();

  constructor(
    public pendingRegistrationService: PendingRegistrationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the pendingRegistrationId from the route parameters
    this.pendingRegistrationId = this.route.snapshot.paramMap.get('pendingRegistrationId');

    if (this.pendingRegistrationId === 'new' ||
        this.pendingRegistrationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.pendingRegistrationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.pendingRegistrationForm.patchValue(this.preSeededData);
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


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Pending Registration';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Pending Registration';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.pendingRegistrationForm.dirty) {
      return confirm('You have unsaved Pending Registration changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.pendingRegistrationId != null && this.pendingRegistrationId !== 'new') {

      const id = parseInt(this.pendingRegistrationId, 10);

      if (!isNaN(id)) {
        return { pendingRegistrationId: id };
      }
    }

    return null;
  }


/*
  * Loads the PendingRegistration data for the current pendingRegistrationId.
  *
  * Fully respects the PendingRegistrationService caching strategy and error handling strategy.
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
    if (!this.pendingRegistrationService.userIsBMCPendingRegistrationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PendingRegistrations.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate pendingRegistrationId
    //
    if (!this.pendingRegistrationId) {

      this.alertService.showMessage('No PendingRegistration ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const pendingRegistrationId = Number(this.pendingRegistrationId);

    if (isNaN(pendingRegistrationId) || pendingRegistrationId <= 0) {

      this.alertService.showMessage(`Invalid Pending Registration ID: "${this.pendingRegistrationId}"`,
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
      // This is the most targeted way: clear only this PendingRegistration + relations

      this.pendingRegistrationService.ClearRecordCache(pendingRegistrationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.pendingRegistrationService.GetPendingRegistration(pendingRegistrationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (pendingRegistrationData) => {

        //
        // Success path — pendingRegistrationData can legitimately be null if 404'd but request succeeded
        //
        if (!pendingRegistrationData) {

          this.handlePendingRegistrationNotFound(pendingRegistrationId);

        } else {

          this.pendingRegistrationData = pendingRegistrationData;
          this.buildFormValues(this.pendingRegistrationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PendingRegistration loaded successfully',
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
        this.handlePendingRegistrationLoadError(error, pendingRegistrationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePendingRegistrationNotFound(pendingRegistrationId: number): void {

    this.pendingRegistrationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PendingRegistration #${pendingRegistrationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePendingRegistrationLoadError(error: any, pendingRegistrationId: number): void {

    let message = 'Failed to load Pending Registration.';
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
          message = 'You do not have permission to view this Pending Registration.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Pending Registration #${pendingRegistrationId} was not found.`;
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

    console.error(`Pending Registration load failed (ID: ${pendingRegistrationId})`, error);

    //
    // Reset UI to safe state
    //
    this.pendingRegistrationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
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

    if (this.pendingRegistrationService.userIsBMCPendingRegistrationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Pending Registrations", 'Access Denied', MessageSeverity.info);
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
        id: this.pendingRegistrationData?.id || 0,
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


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.pendingRegistrationService.PutPendingRegistration(pendingRegistrationSubmitData.id, pendingRegistrationSubmitData)
      : this.pendingRegistrationService.PostPendingRegistration(pendingRegistrationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPendingRegistrationData) => {

        this.pendingRegistrationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Pending Registration's detail page
          //
          this.pendingRegistrationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.pendingRegistrationForm.markAsUntouched();

          this.router.navigate(['/pendingregistrations', savedPendingRegistrationData.id]);
          this.alertService.showMessage('Pending Registration added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.pendingRegistrationData = savedPendingRegistrationData;
          this.buildFormValues(this.pendingRegistrationData);

          this.alertService.showMessage("Pending Registration saved successfully", '', MessageSeverity.success);
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

  public userIsBMCPendingRegistrationReader(): boolean {
    return this.pendingRegistrationService.userIsBMCPendingRegistrationReader();
  }

  public userIsBMCPendingRegistrationWriter(): boolean {
    return this.pendingRegistrationService.userIsBMCPendingRegistrationWriter();
  }
}

/*
   GENERATED FORM FOR THE SECURITYUSERPASSWORDRESETTOKEN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUserPasswordResetToken table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-password-reset-token-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserPasswordResetTokenService, SecurityUserPasswordResetTokenData, SecurityUserPasswordResetTokenSubmitData } from '../../../security-data-services/security-user-password-reset-token.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
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
interface SecurityUserPasswordResetTokenFormValues {
  securityUserId: number | bigint,       // For FK link number
  token: string,
  timeStamp: string,
  expiry: string,
  systemInitiated: boolean,
  completed: boolean,
  comments: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-security-user-password-reset-token-detail',
  templateUrl: './security-user-password-reset-token-detail.component.html',
  styleUrls: ['./security-user-password-reset-token-detail.component.scss']
})

export class SecurityUserPasswordResetTokenDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityUserPasswordResetTokenFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityUserPasswordResetTokenForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        token: ['', Validators.required],
        timeStamp: ['', Validators.required],
        expiry: ['', Validators.required],
        systemInitiated: [false],
        completed: [false],
        comments: [''],
        active: [true],
        deleted: [false],
      });


  public securityUserPasswordResetTokenId: string | null = null;
  public securityUserPasswordResetTokenData: SecurityUserPasswordResetTokenData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityUserPasswordResetTokens$ = this.securityUserPasswordResetTokenService.GetSecurityUserPasswordResetTokenList();
  public securityUsers$ = this.securityUserService.GetSecurityUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityUserPasswordResetTokenService: SecurityUserPasswordResetTokenService,
    public securityUserService: SecurityUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityUserPasswordResetTokenId from the route parameters
    this.securityUserPasswordResetTokenId = this.route.snapshot.paramMap.get('securityUserPasswordResetTokenId');

    if (this.securityUserPasswordResetTokenId === 'new' ||
        this.securityUserPasswordResetTokenId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityUserPasswordResetTokenData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityUserPasswordResetTokenForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityUserPasswordResetTokenForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security User Password Reset Token';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security User Password Reset Token';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityUserPasswordResetTokenForm.dirty) {
      return confirm('You have unsaved Security User Password Reset Token changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityUserPasswordResetTokenId != null && this.securityUserPasswordResetTokenId !== 'new') {

      const id = parseInt(this.securityUserPasswordResetTokenId, 10);

      if (!isNaN(id)) {
        return { securityUserPasswordResetTokenId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityUserPasswordResetToken data for the current securityUserPasswordResetTokenId.
  *
  * Fully respects the SecurityUserPasswordResetTokenService caching strategy and error handling strategy.
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
    if (!this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityUserPasswordResetTokens.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityUserPasswordResetTokenId
    //
    if (!this.securityUserPasswordResetTokenId) {

      this.alertService.showMessage('No SecurityUserPasswordResetToken ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityUserPasswordResetTokenId = Number(this.securityUserPasswordResetTokenId);

    if (isNaN(securityUserPasswordResetTokenId) || securityUserPasswordResetTokenId <= 0) {

      this.alertService.showMessage(`Invalid Security User Password Reset Token ID: "${this.securityUserPasswordResetTokenId}"`,
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
      // This is the most targeted way: clear only this SecurityUserPasswordResetToken + relations

      this.securityUserPasswordResetTokenService.ClearRecordCache(securityUserPasswordResetTokenId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityUserPasswordResetTokenService.GetSecurityUserPasswordResetToken(securityUserPasswordResetTokenId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityUserPasswordResetTokenData) => {

        //
        // Success path — securityUserPasswordResetTokenData can legitimately be null if 404'd but request succeeded
        //
        if (!securityUserPasswordResetTokenData) {

          this.handleSecurityUserPasswordResetTokenNotFound(securityUserPasswordResetTokenId);

        } else {

          this.securityUserPasswordResetTokenData = securityUserPasswordResetTokenData;
          this.buildFormValues(this.securityUserPasswordResetTokenData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityUserPasswordResetToken loaded successfully',
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
        this.handleSecurityUserPasswordResetTokenLoadError(error, securityUserPasswordResetTokenId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityUserPasswordResetTokenNotFound(securityUserPasswordResetTokenId: number): void {

    this.securityUserPasswordResetTokenData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityUserPasswordResetToken #${securityUserPasswordResetTokenId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityUserPasswordResetTokenLoadError(error: any, securityUserPasswordResetTokenId: number): void {

    let message = 'Failed to load Security User Password Reset Token.';
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
          message = 'You do not have permission to view this Security User Password Reset Token.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security User Password Reset Token #${securityUserPasswordResetTokenId} was not found.`;
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

    console.error(`Security User Password Reset Token load failed (ID: ${securityUserPasswordResetTokenId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityUserPasswordResetTokenData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityUserPasswordResetTokenData: SecurityUserPasswordResetTokenData | null) {

    if (securityUserPasswordResetTokenData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserPasswordResetTokenForm.reset({
        securityUserId: null,
        token: '',
        timeStamp: '',
        expiry: '',
        systemInitiated: false,
        completed: false,
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserPasswordResetTokenForm.reset({
        securityUserId: securityUserPasswordResetTokenData.securityUserId,
        token: securityUserPasswordResetTokenData.token ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(securityUserPasswordResetTokenData.timeStamp) ?? '',
        expiry: isoUtcStringToDateTimeLocal(securityUserPasswordResetTokenData.expiry) ?? '',
        systemInitiated: securityUserPasswordResetTokenData.systemInitiated ?? false,
        completed: securityUserPasswordResetTokenData.completed ?? false,
        comments: securityUserPasswordResetTokenData.comments ?? '',
        active: securityUserPasswordResetTokenData.active ?? true,
        deleted: securityUserPasswordResetTokenData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserPasswordResetTokenForm.markAsPristine();
    this.securityUserPasswordResetTokenForm.markAsUntouched();
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

    if (this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security User Password Reset Tokens", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityUserPasswordResetTokenForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserPasswordResetTokenForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserPasswordResetTokenForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserPasswordResetTokenSubmitData: SecurityUserPasswordResetTokenSubmitData = {
        id: this.securityUserPasswordResetTokenData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        token: formValue.token!.trim(),
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        expiry: dateTimeLocalToIsoUtc(formValue.expiry!.trim())!,
        systemInitiated: !!formValue.systemInitiated,
        completed: !!formValue.completed,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityUserPasswordResetTokenService.PutSecurityUserPasswordResetToken(securityUserPasswordResetTokenSubmitData.id, securityUserPasswordResetTokenSubmitData)
      : this.securityUserPasswordResetTokenService.PostSecurityUserPasswordResetToken(securityUserPasswordResetTokenSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityUserPasswordResetTokenData) => {

        this.securityUserPasswordResetTokenService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security User Password Reset Token's detail page
          //
          this.securityUserPasswordResetTokenForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityUserPasswordResetTokenForm.markAsUntouched();

          this.router.navigate(['/securityuserpasswordresettokens', savedSecurityUserPasswordResetTokenData.id]);
          this.alertService.showMessage('Security User Password Reset Token added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityUserPasswordResetTokenData = savedSecurityUserPasswordResetTokenData;
          this.buildFormValues(this.securityUserPasswordResetTokenData);

          this.alertService.showMessage("Security User Password Reset Token saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security User Password Reset Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Password Reset Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Password Reset Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityUserPasswordResetTokenReader(): boolean {
    return this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenReader();
  }

  public userIsSecuritySecurityUserPasswordResetTokenWriter(): boolean {
    return this.securityUserPasswordResetTokenService.userIsSecuritySecurityUserPasswordResetTokenWriter();
  }
}

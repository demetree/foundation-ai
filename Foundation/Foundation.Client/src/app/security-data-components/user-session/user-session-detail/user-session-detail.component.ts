/*
   GENERATED FORM FOR THE USERSESSION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSession table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-session-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserSessionService, UserSessionData, UserSessionSubmitData } from '../../../security-data-services/user-session.service';
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
interface UserSessionFormValues {
  securityUserId: number | bigint,       // For FK link number
  tokenId: string | null,
  sessionStart: string,
  expiresAt: string,
  ipAddress: string | null,
  userAgent: string | null,
  loginMethod: string | null,
  clientApplication: string | null,
  isRevoked: boolean,
  revokedAt: string | null,
  revokedBy: string | null,
  revokedReason: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-session-detail',
  templateUrl: './user-session-detail.component.html',
  styleUrls: ['./user-session-detail.component.scss']
})

export class UserSessionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserSessionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userSessionForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        tokenId: [''],
        sessionStart: ['', Validators.required],
        expiresAt: ['', Validators.required],
        ipAddress: [''],
        userAgent: [''],
        loginMethod: [''],
        clientApplication: [''],
        isRevoked: [false],
        revokedAt: [''],
        revokedBy: [''],
        revokedReason: [''],
        active: [true],
        deleted: [false],
      });


  public userSessionId: string | null = null;
  public userSessionData: UserSessionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userSessions$ = this.userSessionService.GetUserSessionList();
  public securityUsers$ = this.securityUserService.GetSecurityUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public userSessionService: UserSessionService,
    public securityUserService: SecurityUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userSessionId from the route parameters
    this.userSessionId = this.route.snapshot.paramMap.get('userSessionId');

    if (this.userSessionId === 'new' ||
        this.userSessionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userSessionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userSessionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userSessionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Session';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Session';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userSessionForm.dirty) {
      return confirm('You have unsaved User Session changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userSessionId != null && this.userSessionId !== 'new') {

      const id = parseInt(this.userSessionId, 10);

      if (!isNaN(id)) {
        return { userSessionId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserSession data for the current userSessionId.
  *
  * Fully respects the UserSessionService caching strategy and error handling strategy.
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
    if (!this.userSessionService.userIsSecurityUserSessionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserSessions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userSessionId
    //
    if (!this.userSessionId) {

      this.alertService.showMessage('No UserSession ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userSessionId = Number(this.userSessionId);

    if (isNaN(userSessionId) || userSessionId <= 0) {

      this.alertService.showMessage(`Invalid User Session ID: "${this.userSessionId}"`,
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
      // This is the most targeted way: clear only this UserSession + relations

      this.userSessionService.ClearRecordCache(userSessionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userSessionService.GetUserSession(userSessionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userSessionData) => {

        //
        // Success path — userSessionData can legitimately be null if 404'd but request succeeded
        //
        if (!userSessionData) {

          this.handleUserSessionNotFound(userSessionId);

        } else {

          this.userSessionData = userSessionData;
          this.buildFormValues(this.userSessionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserSession loaded successfully',
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
        this.handleUserSessionLoadError(error, userSessionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserSessionNotFound(userSessionId: number): void {

    this.userSessionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserSession #${userSessionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserSessionLoadError(error: any, userSessionId: number): void {

    let message = 'Failed to load User Session.';
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
          message = 'You do not have permission to view this User Session.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Session #${userSessionId} was not found.`;
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

    console.error(`User Session load failed (ID: ${userSessionId})`, error);

    //
    // Reset UI to safe state
    //
    this.userSessionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userSessionData: UserSessionData | null) {

    if (userSessionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userSessionForm.reset({
        securityUserId: null,
        tokenId: '',
        sessionStart: '',
        expiresAt: '',
        ipAddress: '',
        userAgent: '',
        loginMethod: '',
        clientApplication: '',
        isRevoked: false,
        revokedAt: '',
        revokedBy: '',
        revokedReason: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userSessionForm.reset({
        securityUserId: userSessionData.securityUserId,
        tokenId: userSessionData.tokenId ?? '',
        sessionStart: isoUtcStringToDateTimeLocal(userSessionData.sessionStart) ?? '',
        expiresAt: isoUtcStringToDateTimeLocal(userSessionData.expiresAt) ?? '',
        ipAddress: userSessionData.ipAddress ?? '',
        userAgent: userSessionData.userAgent ?? '',
        loginMethod: userSessionData.loginMethod ?? '',
        clientApplication: userSessionData.clientApplication ?? '',
        isRevoked: userSessionData.isRevoked ?? false,
        revokedAt: isoUtcStringToDateTimeLocal(userSessionData.revokedAt) ?? '',
        revokedBy: userSessionData.revokedBy ?? '',
        revokedReason: userSessionData.revokedReason ?? '',
        active: userSessionData.active ?? true,
        deleted: userSessionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userSessionForm.markAsPristine();
    this.userSessionForm.markAsUntouched();
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

    if (this.userSessionService.userIsSecurityUserSessionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Sessions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userSessionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userSessionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userSessionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userSessionSubmitData: UserSessionSubmitData = {
        id: this.userSessionData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        tokenId: formValue.tokenId?.trim() || null,
        sessionStart: dateTimeLocalToIsoUtc(formValue.sessionStart!.trim())!,
        expiresAt: dateTimeLocalToIsoUtc(formValue.expiresAt!.trim())!,
        ipAddress: formValue.ipAddress?.trim() || null,
        userAgent: formValue.userAgent?.trim() || null,
        loginMethod: formValue.loginMethod?.trim() || null,
        clientApplication: formValue.clientApplication?.trim() || null,
        isRevoked: !!formValue.isRevoked,
        revokedAt: formValue.revokedAt ? dateTimeLocalToIsoUtc(formValue.revokedAt.trim()) : null,
        revokedBy: formValue.revokedBy?.trim() || null,
        revokedReason: formValue.revokedReason?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userSessionService.PutUserSession(userSessionSubmitData.id, userSessionSubmitData)
      : this.userSessionService.PostUserSession(userSessionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserSessionData) => {

        this.userSessionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Session's detail page
          //
          this.userSessionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userSessionForm.markAsUntouched();

          this.router.navigate(['/usersessions', savedUserSessionData.id]);
          this.alertService.showMessage('User Session added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userSessionData = savedUserSessionData;
          this.buildFormValues(this.userSessionData);

          this.alertService.showMessage("User Session saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Session.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Session.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Session could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityUserSessionReader(): boolean {
    return this.userSessionService.userIsSecurityUserSessionReader();
  }

  public userIsSecurityUserSessionWriter(): boolean {
    return this.userSessionService.userIsSecurityUserSessionWriter();
  }
}

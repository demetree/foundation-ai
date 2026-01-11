import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LoginAttemptService, LoginAttemptData, LoginAttemptSubmitData } from '../../../security-data-services/login-attempt.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-login-attempt-detail',
  templateUrl: './login-attempt-detail.component.html',
  styleUrls: ['./login-attempt-detail.component.scss']
})

export class LoginAttemptDetailComponent implements OnInit, CanComponentDeactivate {

  loginAttemptForm: FormGroup = this.fb.group({
        timeStamp: ['', Validators.required],
        userName: [''],
        passwordHash: [''],
        resource: [''],
        sessionId: [''],
        ipAddress: [''],
        userAgent: [''],
        value: [''],
        active: [true],
        deleted: [false],
      });


  public loginAttemptId: string | null = null;
  public loginAttemptData: LoginAttemptData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  loginAttempts$ = this.loginAttemptService.GetLoginAttemptList();

  private destroy$ = new Subject<void>();

  constructor(
    public loginAttemptService: LoginAttemptService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the loginAttemptId from the route parameters
    this.loginAttemptId = this.route.snapshot.paramMap.get('loginAttemptId');

    if (this.loginAttemptId === 'new' ||
        this.loginAttemptId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.loginAttemptData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Login Attempt';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Login Attempt';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.loginAttemptForm.dirty) {
      return confirm('You have unsaved Login Attempt changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.loginAttemptId != null && this.loginAttemptId !== 'new') {

      const id = parseInt(this.loginAttemptId, 10);

      if (!isNaN(id)) {
        return { loginAttemptId: id };
      }
    }

    return null;
  }


/*
  * Loads the LoginAttempt data for the current loginAttemptId.
  *
  * Fully respects the LoginAttemptService caching strategy and error handling strategy.
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
    if (!this.loginAttemptService.userIsSecurityLoginAttemptReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read LoginAttempts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate loginAttemptId
    //
    if (!this.loginAttemptId) {

      this.alertService.showMessage('No LoginAttempt ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const loginAttemptId = Number(this.loginAttemptId);

    if (isNaN(loginAttemptId) || loginAttemptId <= 0) {

      this.alertService.showMessage(`Invalid Login Attempt ID: "${this.loginAttemptId}"`,
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
      // This is the most targeted way: clear only this LoginAttempt + relations

      this.loginAttemptService.ClearRecordCache(loginAttemptId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.loginAttemptService.GetLoginAttempt(loginAttemptId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (loginAttemptData) => {

        //
        // Success path — loginAttemptData can legitimately be null if 404'd but request succeeded
        //
        if (!loginAttemptData) {

          this.handleLoginAttemptNotFound(loginAttemptId);

        } else {

          this.loginAttemptData = loginAttemptData;
          this.buildFormValues(this.loginAttemptData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'LoginAttempt loaded successfully',
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
        this.handleLoginAttemptLoadError(error, loginAttemptId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleLoginAttemptNotFound(loginAttemptId: number): void {

    this.loginAttemptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `LoginAttempt #${loginAttemptId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleLoginAttemptLoadError(error: any, loginAttemptId: number): void {

    let message = 'Failed to load Login Attempt.';
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
          message = 'You do not have permission to view this Login Attempt.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Login Attempt #${loginAttemptId} was not found.`;
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

    console.error(`Login Attempt load failed (ID: ${loginAttemptId})`, error);

    //
    // Reset UI to safe state
    //
    this.loginAttemptData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(loginAttemptData: LoginAttemptData | null) {

    if (loginAttemptData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.loginAttemptForm.reset({
        timeStamp: '',
        userName: '',
        passwordHash: '',
        resource: '',
        sessionId: '',
        ipAddress: '',
        userAgent: '',
        value: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.loginAttemptForm.reset({
        timeStamp: isoUtcStringToDateTimeLocal(loginAttemptData.timeStamp) ?? '',
        userName: loginAttemptData.userName ?? '',
        passwordHash: loginAttemptData.passwordHash?.toString() ?? '',
        resource: loginAttemptData.resource ?? '',
        sessionId: loginAttemptData.sessionId ?? '',
        ipAddress: loginAttemptData.ipAddress ?? '',
        userAgent: loginAttemptData.userAgent ?? '',
        value: loginAttemptData.value ?? '',
        active: loginAttemptData.active ?? true,
        deleted: loginAttemptData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.loginAttemptForm.markAsPristine();
    this.loginAttemptForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.loginAttemptService.userIsSecurityLoginAttemptWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Login Attempts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.loginAttemptForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.loginAttemptForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.loginAttemptForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const loginAttemptSubmitData: LoginAttemptSubmitData = {
        id: this.loginAttemptData?.id || 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userName: formValue.userName?.trim() || null,
        passwordHash: formValue.passwordHash ? Number(formValue.passwordHash) : null,
        resource: formValue.resource?.trim() || null,
        sessionId: formValue.sessionId?.trim() || null,
        ipAddress: formValue.ipAddress?.trim() || null,
        userAgent: formValue.userAgent?.trim() || null,
        value: formValue.value?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.loginAttemptService.PutLoginAttempt(loginAttemptSubmitData.id, loginAttemptSubmitData)
      : this.loginAttemptService.PostLoginAttempt(loginAttemptSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedLoginAttemptData) => {

        this.loginAttemptService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Login Attempt's detail page
          //
          this.loginAttemptForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.loginAttemptForm.markAsUntouched();

          this.router.navigate(['/loginattempts', savedLoginAttemptData.id]);
          this.alertService.showMessage('Login Attempt added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.loginAttemptData = savedLoginAttemptData;
          this.buildFormValues(this.loginAttemptData);

          this.alertService.showMessage("Login Attempt saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Login Attempt.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Login Attempt.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Login Attempt could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityLoginAttemptReader(): boolean {
    return this.loginAttemptService.userIsSecurityLoginAttemptReader();
  }

  public userIsSecurityLoginAttemptWriter(): boolean {
    return this.loginAttemptService.userIsSecurityLoginAttemptWriter();
  }
}

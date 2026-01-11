import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityTenantUserService, SecurityTenantUserData, SecurityTenantUserSubmitData } from '../../../security-data-services/security-tenant-user.service';
import { SecurityTenantService } from '../../../security-data-services/security-tenant.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-tenant-user-detail',
  templateUrl: './security-tenant-user-detail.component.html',
  styleUrls: ['./security-tenant-user-detail.component.scss']
})

export class SecurityTenantUserDetailComponent implements OnInit, CanComponentDeactivate {

  securityTenantUserForm: FormGroup = this.fb.group({
        securityTenantId: [null, Validators.required],
        securityUserId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });


  public securityTenantUserId: string | null = null;
  public securityTenantUserData: SecurityTenantUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityTenantUsers$ = this.securityTenantUserService.GetSecurityTenantUserList();
  securityTenants$ = this.securityTenantService.GetSecurityTenantList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityTenantUserService: SecurityTenantUserService,
    public securityTenantService: SecurityTenantService,
    public securityUserService: SecurityUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityTenantUserId from the route parameters
    this.securityTenantUserId = this.route.snapshot.paramMap.get('securityTenantUserId');

    if (this.securityTenantUserId === 'new' ||
        this.securityTenantUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityTenantUserData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Tenant User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Tenant User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityTenantUserForm.dirty) {
      return confirm('You have unsaved Security Tenant User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityTenantUserId != null && this.securityTenantUserId !== 'new') {

      const id = parseInt(this.securityTenantUserId, 10);

      if (!isNaN(id)) {
        return { securityTenantUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityTenantUser data for the current securityTenantUserId.
  *
  * Fully respects the SecurityTenantUserService caching strategy and error handling strategy.
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
    if (!this.securityTenantUserService.userIsSecuritySecurityTenantUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityTenantUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityTenantUserId
    //
    if (!this.securityTenantUserId) {

      this.alertService.showMessage('No SecurityTenantUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityTenantUserId = Number(this.securityTenantUserId);

    if (isNaN(securityTenantUserId) || securityTenantUserId <= 0) {

      this.alertService.showMessage(`Invalid Security Tenant User ID: "${this.securityTenantUserId}"`,
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
      // This is the most targeted way: clear only this SecurityTenantUser + relations

      this.securityTenantUserService.ClearRecordCache(securityTenantUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityTenantUserService.GetSecurityTenantUser(securityTenantUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityTenantUserData) => {

        //
        // Success path — securityTenantUserData can legitimately be null if 404'd but request succeeded
        //
        if (!securityTenantUserData) {

          this.handleSecurityTenantUserNotFound(securityTenantUserId);

        } else {

          this.securityTenantUserData = securityTenantUserData;
          this.buildFormValues(this.securityTenantUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityTenantUser loaded successfully',
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
        this.handleSecurityTenantUserLoadError(error, securityTenantUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityTenantUserNotFound(securityTenantUserId: number): void {

    this.securityTenantUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityTenantUser #${securityTenantUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityTenantUserLoadError(error: any, securityTenantUserId: number): void {

    let message = 'Failed to load Security Tenant User.';
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
          message = 'You do not have permission to view this Security Tenant User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Tenant User #${securityTenantUserId} was not found.`;
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

    console.error(`Security Tenant User load failed (ID: ${securityTenantUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityTenantUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityTenantUserData: SecurityTenantUserData | null) {

    if (securityTenantUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityTenantUserForm.reset({
        securityTenantId: null,
        securityUserId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityTenantUserForm.reset({
        securityTenantId: securityTenantUserData.securityTenantId,
        securityUserId: securityTenantUserData.securityUserId,
        active: securityTenantUserData.active ?? true,
        deleted: securityTenantUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityTenantUserForm.markAsPristine();
    this.securityTenantUserForm.markAsUntouched();
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

    if (this.securityTenantUserService.userIsSecuritySecurityTenantUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Tenant Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityTenantUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityTenantUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityTenantUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityTenantUserSubmitData: SecurityTenantUserSubmitData = {
        id: this.securityTenantUserData?.id || 0,
        securityTenantId: Number(formValue.securityTenantId),
        securityUserId: Number(formValue.securityUserId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityTenantUserService.PutSecurityTenantUser(securityTenantUserSubmitData.id, securityTenantUserSubmitData)
      : this.securityTenantUserService.PostSecurityTenantUser(securityTenantUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityTenantUserData) => {

        this.securityTenantUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Tenant User's detail page
          //
          this.securityTenantUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityTenantUserForm.markAsUntouched();

          this.router.navigate(['/securitytenantusers', savedSecurityTenantUserData.id]);
          this.alertService.showMessage('Security Tenant User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityTenantUserData = savedSecurityTenantUserData;
          this.buildFormValues(this.securityTenantUserData);

          this.alertService.showMessage("Security Tenant User saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Tenant User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Tenant User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Tenant User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityTenantUserReader(): boolean {
    return this.securityTenantUserService.userIsSecuritySecurityTenantUserReader();
  }

  public userIsSecuritySecurityTenantUserWriter(): boolean {
    return this.securityTenantUserService.userIsSecuritySecurityTenantUserWriter();
  }
}

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityOrganizationUserService, SecurityOrganizationUserData, SecurityOrganizationUserSubmitData } from '../../../security-data-services/security-organization-user.service';
import { SecurityOrganizationService } from '../../../security-data-services/security-organization.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-organization-user-detail',
  templateUrl: './security-organization-user-detail.component.html',
  styleUrls: ['./security-organization-user-detail.component.scss']
})

export class SecurityOrganizationUserDetailComponent implements OnInit, CanComponentDeactivate {

  securityOrganizationUserForm: FormGroup = this.fb.group({
        securityOrganizationId: [null, Validators.required],
        securityUserId: [null, Validators.required],
        canRead: [false],
        canWrite: [false],
        canChangeHierarchy: [false],
        canChangeOwner: [false],
        active: [true],
        deleted: [false],
      });


  public securityOrganizationUserId: string | null = null;
  public securityOrganizationUserData: SecurityOrganizationUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityOrganizationUsers$ = this.securityOrganizationUserService.GetSecurityOrganizationUserList();
  securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityOrganizationUserService: SecurityOrganizationUserService,
    public securityOrganizationService: SecurityOrganizationService,
    public securityUserService: SecurityUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityOrganizationUserId from the route parameters
    this.securityOrganizationUserId = this.route.snapshot.paramMap.get('securityOrganizationUserId');

    if (this.securityOrganizationUserId === 'new' ||
        this.securityOrganizationUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityOrganizationUserData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Organization User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Organization User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityOrganizationUserForm.dirty) {
      return confirm('You have unsaved Security Organization User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityOrganizationUserId != null && this.securityOrganizationUserId !== 'new') {

      const id = parseInt(this.securityOrganizationUserId, 10);

      if (!isNaN(id)) {
        return { securityOrganizationUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityOrganizationUser data for the current securityOrganizationUserId.
  *
  * Fully respects the SecurityOrganizationUserService caching strategy and error handling strategy.
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
    if (!this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityOrganizationUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityOrganizationUserId
    //
    if (!this.securityOrganizationUserId) {

      this.alertService.showMessage('No SecurityOrganizationUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityOrganizationUserId = Number(this.securityOrganizationUserId);

    if (isNaN(securityOrganizationUserId) || securityOrganizationUserId <= 0) {

      this.alertService.showMessage(`Invalid Security Organization User ID: "${this.securityOrganizationUserId}"`,
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
      // This is the most targeted way: clear only this SecurityOrganizationUser + relations

      this.securityOrganizationUserService.ClearRecordCache(securityOrganizationUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityOrganizationUserService.GetSecurityOrganizationUser(securityOrganizationUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityOrganizationUserData) => {

        //
        // Success path — securityOrganizationUserData can legitimately be null if 404'd but request succeeded
        //
        if (!securityOrganizationUserData) {

          this.handleSecurityOrganizationUserNotFound(securityOrganizationUserId);

        } else {

          this.securityOrganizationUserData = securityOrganizationUserData;
          this.buildFormValues(this.securityOrganizationUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityOrganizationUser loaded successfully',
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
        this.handleSecurityOrganizationUserLoadError(error, securityOrganizationUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityOrganizationUserNotFound(securityOrganizationUserId: number): void {

    this.securityOrganizationUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityOrganizationUser #${securityOrganizationUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityOrganizationUserLoadError(error: any, securityOrganizationUserId: number): void {

    let message = 'Failed to load Security Organization User.';
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
          message = 'You do not have permission to view this Security Organization User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Organization User #${securityOrganizationUserId} was not found.`;
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

    console.error(`Security Organization User load failed (ID: ${securityOrganizationUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityOrganizationUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityOrganizationUserData: SecurityOrganizationUserData | null) {

    if (securityOrganizationUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityOrganizationUserForm.reset({
        securityOrganizationId: null,
        securityUserId: null,
        canRead: false,
        canWrite: false,
        canChangeHierarchy: false,
        canChangeOwner: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityOrganizationUserForm.reset({
        securityOrganizationId: securityOrganizationUserData.securityOrganizationId,
        securityUserId: securityOrganizationUserData.securityUserId,
        canRead: securityOrganizationUserData.canRead ?? false,
        canWrite: securityOrganizationUserData.canWrite ?? false,
        canChangeHierarchy: securityOrganizationUserData.canChangeHierarchy ?? false,
        canChangeOwner: securityOrganizationUserData.canChangeOwner ?? false,
        active: securityOrganizationUserData.active ?? true,
        deleted: securityOrganizationUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityOrganizationUserForm.markAsPristine();
    this.securityOrganizationUserForm.markAsUntouched();
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

    if (this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Organization Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityOrganizationUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityOrganizationUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityOrganizationUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityOrganizationUserSubmitData: SecurityOrganizationUserSubmitData = {
        id: this.securityOrganizationUserData?.id || 0,
        securityOrganizationId: Number(formValue.securityOrganizationId),
        securityUserId: Number(formValue.securityUserId),
        canRead: !!formValue.canRead,
        canWrite: !!formValue.canWrite,
        canChangeHierarchy: !!formValue.canChangeHierarchy,
        canChangeOwner: !!formValue.canChangeOwner,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityOrganizationUserService.PutSecurityOrganizationUser(securityOrganizationUserSubmitData.id, securityOrganizationUserSubmitData)
      : this.securityOrganizationUserService.PostSecurityOrganizationUser(securityOrganizationUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityOrganizationUserData) => {

        this.securityOrganizationUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Organization User's detail page
          //
          this.securityOrganizationUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityOrganizationUserForm.markAsUntouched();

          this.router.navigate(['/securityorganizationusers', savedSecurityOrganizationUserData.id]);
          this.alertService.showMessage('Security Organization User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityOrganizationUserData = savedSecurityOrganizationUserData;
          this.buildFormValues(this.securityOrganizationUserData);

          this.alertService.showMessage("Security Organization User saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Organization User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Organization User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Organization User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityOrganizationUserReader(): boolean {
    return this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserReader();
  }

  public userIsSecuritySecurityOrganizationUserWriter(): boolean {
    return this.securityOrganizationUserService.userIsSecuritySecurityOrganizationUserWriter();
  }
}

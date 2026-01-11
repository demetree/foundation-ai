import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityDepartmentUserService, SecurityDepartmentUserData, SecurityDepartmentUserSubmitData } from '../../../security-data-services/security-department-user.service';
import { SecurityDepartmentService } from '../../../security-data-services/security-department.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-department-user-detail',
  templateUrl: './security-department-user-detail.component.html',
  styleUrls: ['./security-department-user-detail.component.scss']
})

export class SecurityDepartmentUserDetailComponent implements OnInit, CanComponentDeactivate {

  securityDepartmentUserForm: FormGroup = this.fb.group({
        securityDepartmentId: [null, Validators.required],
        securityUserId: [null, Validators.required],
        canRead: [false],
        canWrite: [false],
        canChangeHierarchy: [false],
        canChangeOwner: [false],
        active: [true],
        deleted: [false],
      });


  public securityDepartmentUserId: string | null = null;
  public securityDepartmentUserData: SecurityDepartmentUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityDepartmentUsers$ = this.securityDepartmentUserService.GetSecurityDepartmentUserList();
  securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityDepartmentUserService: SecurityDepartmentUserService,
    public securityDepartmentService: SecurityDepartmentService,
    public securityUserService: SecurityUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityDepartmentUserId from the route parameters
    this.securityDepartmentUserId = this.route.snapshot.paramMap.get('securityDepartmentUserId');

    if (this.securityDepartmentUserId === 'new' ||
        this.securityDepartmentUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityDepartmentUserData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Department User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Department User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityDepartmentUserForm.dirty) {
      return confirm('You have unsaved Security Department User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityDepartmentUserId != null && this.securityDepartmentUserId !== 'new') {

      const id = parseInt(this.securityDepartmentUserId, 10);

      if (!isNaN(id)) {
        return { securityDepartmentUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityDepartmentUser data for the current securityDepartmentUserId.
  *
  * Fully respects the SecurityDepartmentUserService caching strategy and error handling strategy.
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
    if (!this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityDepartmentUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityDepartmentUserId
    //
    if (!this.securityDepartmentUserId) {

      this.alertService.showMessage('No SecurityDepartmentUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityDepartmentUserId = Number(this.securityDepartmentUserId);

    if (isNaN(securityDepartmentUserId) || securityDepartmentUserId <= 0) {

      this.alertService.showMessage(`Invalid Security Department User ID: "${this.securityDepartmentUserId}"`,
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
      // This is the most targeted way: clear only this SecurityDepartmentUser + relations

      this.securityDepartmentUserService.ClearRecordCache(securityDepartmentUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityDepartmentUserService.GetSecurityDepartmentUser(securityDepartmentUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityDepartmentUserData) => {

        //
        // Success path — securityDepartmentUserData can legitimately be null if 404'd but request succeeded
        //
        if (!securityDepartmentUserData) {

          this.handleSecurityDepartmentUserNotFound(securityDepartmentUserId);

        } else {

          this.securityDepartmentUserData = securityDepartmentUserData;
          this.buildFormValues(this.securityDepartmentUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityDepartmentUser loaded successfully',
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
        this.handleSecurityDepartmentUserLoadError(error, securityDepartmentUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityDepartmentUserNotFound(securityDepartmentUserId: number): void {

    this.securityDepartmentUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityDepartmentUser #${securityDepartmentUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityDepartmentUserLoadError(error: any, securityDepartmentUserId: number): void {

    let message = 'Failed to load Security Department User.';
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
          message = 'You do not have permission to view this Security Department User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Department User #${securityDepartmentUserId} was not found.`;
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

    console.error(`Security Department User load failed (ID: ${securityDepartmentUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityDepartmentUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityDepartmentUserData: SecurityDepartmentUserData | null) {

    if (securityDepartmentUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityDepartmentUserForm.reset({
        securityDepartmentId: null,
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
        this.securityDepartmentUserForm.reset({
        securityDepartmentId: securityDepartmentUserData.securityDepartmentId,
        securityUserId: securityDepartmentUserData.securityUserId,
        canRead: securityDepartmentUserData.canRead ?? false,
        canWrite: securityDepartmentUserData.canWrite ?? false,
        canChangeHierarchy: securityDepartmentUserData.canChangeHierarchy ?? false,
        canChangeOwner: securityDepartmentUserData.canChangeOwner ?? false,
        active: securityDepartmentUserData.active ?? true,
        deleted: securityDepartmentUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityDepartmentUserForm.markAsPristine();
    this.securityDepartmentUserForm.markAsUntouched();
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

    if (this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Department Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityDepartmentUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityDepartmentUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityDepartmentUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityDepartmentUserSubmitData: SecurityDepartmentUserSubmitData = {
        id: this.securityDepartmentUserData?.id || 0,
        securityDepartmentId: Number(formValue.securityDepartmentId),
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
      ? this.securityDepartmentUserService.PutSecurityDepartmentUser(securityDepartmentUserSubmitData.id, securityDepartmentUserSubmitData)
      : this.securityDepartmentUserService.PostSecurityDepartmentUser(securityDepartmentUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityDepartmentUserData) => {

        this.securityDepartmentUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Department User's detail page
          //
          this.securityDepartmentUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityDepartmentUserForm.markAsUntouched();

          this.router.navigate(['/securitydepartmentusers', savedSecurityDepartmentUserData.id]);
          this.alertService.showMessage('Security Department User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityDepartmentUserData = savedSecurityDepartmentUserData;
          this.buildFormValues(this.securityDepartmentUserData);

          this.alertService.showMessage("Security Department User saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Department User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Department User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Department User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityDepartmentUserReader(): boolean {
    return this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserReader();
  }

  public userIsSecuritySecurityDepartmentUserWriter(): boolean {
    return this.securityDepartmentUserService.userIsSecuritySecurityDepartmentUserWriter();
  }
}

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityDepartmentService, SecurityDepartmentData, SecurityDepartmentSubmitData } from '../../../security-data-services/security-department.service';
import { SecurityOrganizationService } from '../../../security-data-services/security-organization.service';
import { SecurityTeamService } from '../../../security-data-services/security-team.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityDepartmentUserService } from '../../../security-data-services/security-department-user.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-department-detail',
  templateUrl: './security-department-detail.component.html',
  styleUrls: ['./security-department-detail.component.scss']
})

export class SecurityDepartmentDetailComponent implements OnInit, CanComponentDeactivate {

  securityDepartmentForm: FormGroup = this.fb.group({
        securityOrganizationId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public securityDepartmentId: string | null = null;
  public securityDepartmentData: SecurityDepartmentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();
  securityTeams$ = this.securityTeamService.GetSecurityTeamList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();
  securityDepartmentUsers$ = this.securityDepartmentUserService.GetSecurityDepartmentUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityDepartmentService: SecurityDepartmentService,
    public securityOrganizationService: SecurityOrganizationService,
    public securityTeamService: SecurityTeamService,
    public securityUserService: SecurityUserService,
    public securityDepartmentUserService: SecurityDepartmentUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityDepartmentId from the route parameters
    this.securityDepartmentId = this.route.snapshot.paramMap.get('securityDepartmentId');

    if (this.securityDepartmentId === 'new' ||
        this.securityDepartmentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityDepartmentData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Department';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Department';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityDepartmentForm.dirty) {
      return confirm('You have unsaved Security Department changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityDepartmentId != null && this.securityDepartmentId !== 'new') {

      const id = parseInt(this.securityDepartmentId, 10);

      if (!isNaN(id)) {
        return { securityDepartmentId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityDepartment data for the current securityDepartmentId.
  *
  * Fully respects the SecurityDepartmentService caching strategy and error handling strategy.
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
    if (!this.securityDepartmentService.userIsSecuritySecurityDepartmentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityDepartments.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityDepartmentId
    //
    if (!this.securityDepartmentId) {

      this.alertService.showMessage('No SecurityDepartment ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityDepartmentId = Number(this.securityDepartmentId);

    if (isNaN(securityDepartmentId) || securityDepartmentId <= 0) {

      this.alertService.showMessage(`Invalid Security Department ID: "${this.securityDepartmentId}"`,
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
      // This is the most targeted way: clear only this SecurityDepartment + relations

      this.securityDepartmentService.ClearRecordCache(securityDepartmentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityDepartmentService.GetSecurityDepartment(securityDepartmentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityDepartmentData) => {

        //
        // Success path — securityDepartmentData can legitimately be null if 404'd but request succeeded
        //
        if (!securityDepartmentData) {

          this.handleSecurityDepartmentNotFound(securityDepartmentId);

        } else {

          this.securityDepartmentData = securityDepartmentData;
          this.buildFormValues(this.securityDepartmentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityDepartment loaded successfully',
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
        this.handleSecurityDepartmentLoadError(error, securityDepartmentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityDepartmentNotFound(securityDepartmentId: number): void {

    this.securityDepartmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityDepartment #${securityDepartmentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityDepartmentLoadError(error: any, securityDepartmentId: number): void {

    let message = 'Failed to load Security Department.';
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
          message = 'You do not have permission to view this Security Department.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Department #${securityDepartmentId} was not found.`;
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

    console.error(`Security Department load failed (ID: ${securityDepartmentId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityDepartmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityDepartmentData: SecurityDepartmentData | null) {

    if (securityDepartmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityDepartmentForm.reset({
        securityOrganizationId: null,
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityDepartmentForm.reset({
        securityOrganizationId: securityDepartmentData.securityOrganizationId,
        name: securityDepartmentData.name ?? '',
        description: securityDepartmentData.description ?? '',
        active: securityDepartmentData.active ?? true,
        deleted: securityDepartmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityDepartmentForm.markAsPristine();
    this.securityDepartmentForm.markAsUntouched();
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

    if (this.securityDepartmentService.userIsSecuritySecurityDepartmentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Departments", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityDepartmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityDepartmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityDepartmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityDepartmentSubmitData: SecurityDepartmentSubmitData = {
        id: this.securityDepartmentData?.id || 0,
        securityOrganizationId: Number(formValue.securityOrganizationId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityDepartmentService.PutSecurityDepartment(securityDepartmentSubmitData.id, securityDepartmentSubmitData)
      : this.securityDepartmentService.PostSecurityDepartment(securityDepartmentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityDepartmentData) => {

        this.securityDepartmentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Department's detail page
          //
          this.securityDepartmentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityDepartmentForm.markAsUntouched();

          this.router.navigate(['/securitydepartments', savedSecurityDepartmentData.id]);
          this.alertService.showMessage('Security Department added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityDepartmentData = savedSecurityDepartmentData;
          this.buildFormValues(this.securityDepartmentData);

          this.alertService.showMessage("Security Department saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Department.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Department.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Department could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityDepartmentReader(): boolean {
    return this.securityDepartmentService.userIsSecuritySecurityDepartmentReader();
  }

  public userIsSecuritySecurityDepartmentWriter(): boolean {
    return this.securityDepartmentService.userIsSecuritySecurityDepartmentWriter();
  }
}

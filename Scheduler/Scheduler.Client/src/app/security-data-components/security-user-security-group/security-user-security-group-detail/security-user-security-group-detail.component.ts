import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserSecurityGroupService, SecurityUserSecurityGroupData, SecurityUserSecurityGroupSubmitData } from '../../../security-data-services/security-user-security-group.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityGroupService } from '../../../security-data-services/security-group.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-user-security-group-detail',
  templateUrl: './security-user-security-group-detail.component.html',
  styleUrls: ['./security-user-security-group-detail.component.scss']
})

export class SecurityUserSecurityGroupDetailComponent implements OnInit, CanComponentDeactivate {

  securityUserSecurityGroupForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        securityGroupId: [null, Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });


  public securityUserSecurityGroupId: string | null = null;
  public securityUserSecurityGroupData: SecurityUserSecurityGroupData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityUserSecurityGroups$ = this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();
  securityGroups$ = this.securityGroupService.GetSecurityGroupList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityUserSecurityGroupService: SecurityUserSecurityGroupService,
    public securityUserService: SecurityUserService,
    public securityGroupService: SecurityGroupService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityUserSecurityGroupId from the route parameters
    this.securityUserSecurityGroupId = this.route.snapshot.paramMap.get('securityUserSecurityGroupId');

    if (this.securityUserSecurityGroupId === 'new' ||
        this.securityUserSecurityGroupId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityUserSecurityGroupData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security User Security Group';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security User Security Group';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityUserSecurityGroupForm.dirty) {
      return confirm('You have unsaved Security User Security Group changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityUserSecurityGroupId != null && this.securityUserSecurityGroupId !== 'new') {

      const id = parseInt(this.securityUserSecurityGroupId, 10);

      if (!isNaN(id)) {
        return { securityUserSecurityGroupId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityUserSecurityGroup data for the current securityUserSecurityGroupId.
  *
  * Fully respects the SecurityUserSecurityGroupService caching strategy and error handling strategy.
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
    if (!this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityUserSecurityGroups.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityUserSecurityGroupId
    //
    if (!this.securityUserSecurityGroupId) {

      this.alertService.showMessage('No SecurityUserSecurityGroup ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityUserSecurityGroupId = Number(this.securityUserSecurityGroupId);

    if (isNaN(securityUserSecurityGroupId) || securityUserSecurityGroupId <= 0) {

      this.alertService.showMessage(`Invalid Security User Security Group ID: "${this.securityUserSecurityGroupId}"`,
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
      // This is the most targeted way: clear only this SecurityUserSecurityGroup + relations

      this.securityUserSecurityGroupService.ClearRecordCache(securityUserSecurityGroupId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityUserSecurityGroupService.GetSecurityUserSecurityGroup(securityUserSecurityGroupId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityUserSecurityGroupData) => {

        //
        // Success path — securityUserSecurityGroupData can legitimately be null if 404'd but request succeeded
        //
        if (!securityUserSecurityGroupData) {

          this.handleSecurityUserSecurityGroupNotFound(securityUserSecurityGroupId);

        } else {

          this.securityUserSecurityGroupData = securityUserSecurityGroupData;
          this.buildFormValues(this.securityUserSecurityGroupData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityUserSecurityGroup loaded successfully',
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
        this.handleSecurityUserSecurityGroupLoadError(error, securityUserSecurityGroupId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityUserSecurityGroupNotFound(securityUserSecurityGroupId: number): void {

    this.securityUserSecurityGroupData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityUserSecurityGroup #${securityUserSecurityGroupId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityUserSecurityGroupLoadError(error: any, securityUserSecurityGroupId: number): void {

    let message = 'Failed to load Security User Security Group.';
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
          message = 'You do not have permission to view this Security User Security Group.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security User Security Group #${securityUserSecurityGroupId} was not found.`;
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

    console.error(`Security User Security Group load failed (ID: ${securityUserSecurityGroupId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityUserSecurityGroupData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityUserSecurityGroupData: SecurityUserSecurityGroupData | null) {

    if (securityUserSecurityGroupData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserSecurityGroupForm.reset({
        securityUserId: null,
        securityGroupId: null,
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserSecurityGroupForm.reset({
        securityUserId: securityUserSecurityGroupData.securityUserId,
        securityGroupId: securityUserSecurityGroupData.securityGroupId,
        comments: securityUserSecurityGroupData.comments ?? '',
        active: securityUserSecurityGroupData.active ?? true,
        deleted: securityUserSecurityGroupData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserSecurityGroupForm.markAsPristine();
    this.securityUserSecurityGroupForm.markAsUntouched();
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

    if (this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security User Security Groups", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityUserSecurityGroupForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserSecurityGroupForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserSecurityGroupForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserSecurityGroupSubmitData: SecurityUserSecurityGroupSubmitData = {
        id: this.securityUserSecurityGroupData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        securityGroupId: Number(formValue.securityGroupId),
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityUserSecurityGroupService.PutSecurityUserSecurityGroup(securityUserSecurityGroupSubmitData.id, securityUserSecurityGroupSubmitData)
      : this.securityUserSecurityGroupService.PostSecurityUserSecurityGroup(securityUserSecurityGroupSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityUserSecurityGroupData) => {

        this.securityUserSecurityGroupService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security User Security Group's detail page
          //
          this.securityUserSecurityGroupForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityUserSecurityGroupForm.markAsUntouched();

          this.router.navigate(['/securityusersecuritygroups', savedSecurityUserSecurityGroupData.id]);
          this.alertService.showMessage('Security User Security Group added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityUserSecurityGroupData = savedSecurityUserSecurityGroupData;
          this.buildFormValues(this.securityUserSecurityGroupData);

          this.alertService.showMessage("Security User Security Group saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security User Security Group.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Security Group.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Security Group could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityUserSecurityGroupReader(): boolean {
    return this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupReader();
  }

  public userIsSecuritySecurityUserSecurityGroupWriter(): boolean {
    return this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupWriter();
  }
}

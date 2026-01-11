import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityGroupService, SecurityGroupData, SecurityGroupSubmitData } from '../../../security-data-services/security-group.service';
import { SecurityUserSecurityGroupService } from '../../../security-data-services/security-user-security-group.service';
import { SecurityGroupSecurityRoleService } from '../../../security-data-services/security-group-security-role.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-group-detail',
  templateUrl: './security-group-detail.component.html',
  styleUrls: ['./security-group-detail.component.scss']
})

export class SecurityGroupDetailComponent implements OnInit, CanComponentDeactivate {

  securityGroupForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public securityGroupId: string | null = null;
  public securityGroupData: SecurityGroupData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityGroups$ = this.securityGroupService.GetSecurityGroupList();
  securityUserSecurityGroups$ = this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList();
  securityGroupSecurityRoles$ = this.securityGroupSecurityRoleService.GetSecurityGroupSecurityRoleList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityGroupService: SecurityGroupService,
    public securityUserSecurityGroupService: SecurityUserSecurityGroupService,
    public securityGroupSecurityRoleService: SecurityGroupSecurityRoleService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityGroupId from the route parameters
    this.securityGroupId = this.route.snapshot.paramMap.get('securityGroupId');

    if (this.securityGroupId === 'new' ||
        this.securityGroupId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityGroupData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Group';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Group';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityGroupForm.dirty) {
      return confirm('You have unsaved Security Group changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityGroupId != null && this.securityGroupId !== 'new') {

      const id = parseInt(this.securityGroupId, 10);

      if (!isNaN(id)) {
        return { securityGroupId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityGroup data for the current securityGroupId.
  *
  * Fully respects the SecurityGroupService caching strategy and error handling strategy.
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
    if (!this.securityGroupService.userIsSecuritySecurityGroupReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityGroups.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityGroupId
    //
    if (!this.securityGroupId) {

      this.alertService.showMessage('No SecurityGroup ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityGroupId = Number(this.securityGroupId);

    if (isNaN(securityGroupId) || securityGroupId <= 0) {

      this.alertService.showMessage(`Invalid Security Group ID: "${this.securityGroupId}"`,
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
      // This is the most targeted way: clear only this SecurityGroup + relations

      this.securityGroupService.ClearRecordCache(securityGroupId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityGroupService.GetSecurityGroup(securityGroupId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityGroupData) => {

        //
        // Success path — securityGroupData can legitimately be null if 404'd but request succeeded
        //
        if (!securityGroupData) {

          this.handleSecurityGroupNotFound(securityGroupId);

        } else {

          this.securityGroupData = securityGroupData;
          this.buildFormValues(this.securityGroupData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityGroup loaded successfully',
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
        this.handleSecurityGroupLoadError(error, securityGroupId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityGroupNotFound(securityGroupId: number): void {

    this.securityGroupData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityGroup #${securityGroupId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityGroupLoadError(error: any, securityGroupId: number): void {

    let message = 'Failed to load Security Group.';
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
          message = 'You do not have permission to view this Security Group.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Group #${securityGroupId} was not found.`;
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

    console.error(`Security Group load failed (ID: ${securityGroupId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityGroupData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityGroupData: SecurityGroupData | null) {

    if (securityGroupData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityGroupForm.reset({
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
        this.securityGroupForm.reset({
        name: securityGroupData.name ?? '',
        description: securityGroupData.description ?? '',
        active: securityGroupData.active ?? true,
        deleted: securityGroupData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityGroupForm.markAsPristine();
    this.securityGroupForm.markAsUntouched();
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

    if (this.securityGroupService.userIsSecuritySecurityGroupWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Groups", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityGroupForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityGroupForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityGroupForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityGroupSubmitData: SecurityGroupSubmitData = {
        id: this.securityGroupData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityGroupService.PutSecurityGroup(securityGroupSubmitData.id, securityGroupSubmitData)
      : this.securityGroupService.PostSecurityGroup(securityGroupSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityGroupData) => {

        this.securityGroupService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Group's detail page
          //
          this.securityGroupForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityGroupForm.markAsUntouched();

          this.router.navigate(['/securitygroups', savedSecurityGroupData.id]);
          this.alertService.showMessage('Security Group added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityGroupData = savedSecurityGroupData;
          this.buildFormValues(this.securityGroupData);

          this.alertService.showMessage("Security Group saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Group.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Group.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Group could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityGroupReader(): boolean {
    return this.securityGroupService.userIsSecuritySecurityGroupReader();
  }

  public userIsSecuritySecurityGroupWriter(): boolean {
    return this.securityGroupService.userIsSecuritySecurityGroupWriter();
  }
}

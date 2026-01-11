import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModuleSecurityRoleService, ModuleSecurityRoleData, ModuleSecurityRoleSubmitData } from '../../../security-data-services/module-security-role.service';
import { ModuleService } from '../../../security-data-services/module.service';
import { SecurityRoleService } from '../../../security-data-services/security-role.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-module-security-role-detail',
  templateUrl: './module-security-role-detail.component.html',
  styleUrls: ['./module-security-role-detail.component.scss']
})

export class ModuleSecurityRoleDetailComponent implements OnInit, CanComponentDeactivate {

  moduleSecurityRoleForm: FormGroup = this.fb.group({
        moduleId: [null, Validators.required],
        securityRoleId: [null, Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });


  public moduleSecurityRoleId: string | null = null;
  public moduleSecurityRoleData: ModuleSecurityRoleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  moduleSecurityRoles$ = this.moduleSecurityRoleService.GetModuleSecurityRoleList();
  modules$ = this.moduleService.GetModuleList();
  securityRoles$ = this.securityRoleService.GetSecurityRoleList();

  private destroy$ = new Subject<void>();

  constructor(
    public moduleSecurityRoleService: ModuleSecurityRoleService,
    public moduleService: ModuleService,
    public securityRoleService: SecurityRoleService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the moduleSecurityRoleId from the route parameters
    this.moduleSecurityRoleId = this.route.snapshot.paramMap.get('moduleSecurityRoleId');

    if (this.moduleSecurityRoleId === 'new' ||
        this.moduleSecurityRoleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.moduleSecurityRoleData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Module Security Role';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Module Security Role';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.moduleSecurityRoleForm.dirty) {
      return confirm('You have unsaved Module Security Role changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.moduleSecurityRoleId != null && this.moduleSecurityRoleId !== 'new') {

      const id = parseInt(this.moduleSecurityRoleId, 10);

      if (!isNaN(id)) {
        return { moduleSecurityRoleId: id };
      }
    }

    return null;
  }


/*
  * Loads the ModuleSecurityRole data for the current moduleSecurityRoleId.
  *
  * Fully respects the ModuleSecurityRoleService caching strategy and error handling strategy.
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
    if (!this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ModuleSecurityRoles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate moduleSecurityRoleId
    //
    if (!this.moduleSecurityRoleId) {

      this.alertService.showMessage('No ModuleSecurityRole ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const moduleSecurityRoleId = Number(this.moduleSecurityRoleId);

    if (isNaN(moduleSecurityRoleId) || moduleSecurityRoleId <= 0) {

      this.alertService.showMessage(`Invalid Module Security Role ID: "${this.moduleSecurityRoleId}"`,
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
      // This is the most targeted way: clear only this ModuleSecurityRole + relations

      this.moduleSecurityRoleService.ClearRecordCache(moduleSecurityRoleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.moduleSecurityRoleService.GetModuleSecurityRole(moduleSecurityRoleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (moduleSecurityRoleData) => {

        //
        // Success path — moduleSecurityRoleData can legitimately be null if 404'd but request succeeded
        //
        if (!moduleSecurityRoleData) {

          this.handleModuleSecurityRoleNotFound(moduleSecurityRoleId);

        } else {

          this.moduleSecurityRoleData = moduleSecurityRoleData;
          this.buildFormValues(this.moduleSecurityRoleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ModuleSecurityRole loaded successfully',
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
        this.handleModuleSecurityRoleLoadError(error, moduleSecurityRoleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleModuleSecurityRoleNotFound(moduleSecurityRoleId: number): void {

    this.moduleSecurityRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ModuleSecurityRole #${moduleSecurityRoleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleModuleSecurityRoleLoadError(error: any, moduleSecurityRoleId: number): void {

    let message = 'Failed to load Module Security Role.';
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
          message = 'You do not have permission to view this Module Security Role.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Module Security Role #${moduleSecurityRoleId} was not found.`;
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

    console.error(`Module Security Role load failed (ID: ${moduleSecurityRoleId})`, error);

    //
    // Reset UI to safe state
    //
    this.moduleSecurityRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(moduleSecurityRoleData: ModuleSecurityRoleData | null) {

    if (moduleSecurityRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.moduleSecurityRoleForm.reset({
        moduleId: null,
        securityRoleId: null,
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.moduleSecurityRoleForm.reset({
        moduleId: moduleSecurityRoleData.moduleId,
        securityRoleId: moduleSecurityRoleData.securityRoleId,
        comments: moduleSecurityRoleData.comments ?? '',
        active: moduleSecurityRoleData.active ?? true,
        deleted: moduleSecurityRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.moduleSecurityRoleForm.markAsPristine();
    this.moduleSecurityRoleForm.markAsUntouched();
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

    if (this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Module Security Roles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.moduleSecurityRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.moduleSecurityRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.moduleSecurityRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const moduleSecurityRoleSubmitData: ModuleSecurityRoleSubmitData = {
        id: this.moduleSecurityRoleData?.id || 0,
        moduleId: Number(formValue.moduleId),
        securityRoleId: Number(formValue.securityRoleId),
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.moduleSecurityRoleService.PutModuleSecurityRole(moduleSecurityRoleSubmitData.id, moduleSecurityRoleSubmitData)
      : this.moduleSecurityRoleService.PostModuleSecurityRole(moduleSecurityRoleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedModuleSecurityRoleData) => {

        this.moduleSecurityRoleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Module Security Role's detail page
          //
          this.moduleSecurityRoleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.moduleSecurityRoleForm.markAsUntouched();

          this.router.navigate(['/modulesecurityroles', savedModuleSecurityRoleData.id]);
          this.alertService.showMessage('Module Security Role added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.moduleSecurityRoleData = savedModuleSecurityRoleData;
          this.buildFormValues(this.moduleSecurityRoleData);

          this.alertService.showMessage("Module Security Role saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Module Security Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Module Security Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Module Security Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityModuleSecurityRoleReader(): boolean {
    return this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleReader();
  }

  public userIsSecurityModuleSecurityRoleWriter(): boolean {
    return this.moduleSecurityRoleService.userIsSecurityModuleSecurityRoleWriter();
  }
}

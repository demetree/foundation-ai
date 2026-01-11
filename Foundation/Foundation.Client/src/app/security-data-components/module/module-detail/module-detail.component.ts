import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModuleService, ModuleData, ModuleSubmitData } from '../../../security-data-services/module.service';
import { ModuleSecurityRoleService } from '../../../security-data-services/module-security-role.service';
import { EntityDataTokenService } from '../../../security-data-services/entity-data-token.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-module-detail',
  templateUrl: './module-detail.component.html',
  styleUrls: ['./module-detail.component.scss']
})

export class ModuleDetailComponent implements OnInit, CanComponentDeactivate {

  moduleForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public moduleId: string | null = null;
  public moduleData: ModuleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  modules$ = this.moduleService.GetModuleList();
  moduleSecurityRoles$ = this.moduleSecurityRoleService.GetModuleSecurityRoleList();
  entityDataTokens$ = this.entityDataTokenService.GetEntityDataTokenList();

  private destroy$ = new Subject<void>();

  constructor(
    public moduleService: ModuleService,
    public moduleSecurityRoleService: ModuleSecurityRoleService,
    public entityDataTokenService: EntityDataTokenService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the moduleId from the route parameters
    this.moduleId = this.route.snapshot.paramMap.get('moduleId');

    if (this.moduleId === 'new' ||
        this.moduleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.moduleData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Module';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Module';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.moduleForm.dirty) {
      return confirm('You have unsaved Module changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.moduleId != null && this.moduleId !== 'new') {

      const id = parseInt(this.moduleId, 10);

      if (!isNaN(id)) {
        return { moduleId: id };
      }
    }

    return null;
  }


/*
  * Loads the Module data for the current moduleId.
  *
  * Fully respects the ModuleService caching strategy and error handling strategy.
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
    if (!this.moduleService.userIsSecurityModuleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Modules.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate moduleId
    //
    if (!this.moduleId) {

      this.alertService.showMessage('No Module ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const moduleId = Number(this.moduleId);

    if (isNaN(moduleId) || moduleId <= 0) {

      this.alertService.showMessage(`Invalid Module ID: "${this.moduleId}"`,
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
      // This is the most targeted way: clear only this Module + relations

      this.moduleService.ClearRecordCache(moduleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.moduleService.GetModule(moduleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (moduleData) => {

        //
        // Success path — moduleData can legitimately be null if 404'd but request succeeded
        //
        if (!moduleData) {

          this.handleModuleNotFound(moduleId);

        } else {

          this.moduleData = moduleData;
          this.buildFormValues(this.moduleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Module loaded successfully',
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
        this.handleModuleLoadError(error, moduleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleModuleNotFound(moduleId: number): void {

    this.moduleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Module #${moduleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleModuleLoadError(error: any, moduleId: number): void {

    let message = 'Failed to load Module.';
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
          message = 'You do not have permission to view this Module.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Module #${moduleId} was not found.`;
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

    console.error(`Module load failed (ID: ${moduleId})`, error);

    //
    // Reset UI to safe state
    //
    this.moduleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(moduleData: ModuleData | null) {

    if (moduleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.moduleForm.reset({
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
        this.moduleForm.reset({
        name: moduleData.name ?? '',
        description: moduleData.description ?? '',
        active: moduleData.active ?? true,
        deleted: moduleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.moduleForm.markAsPristine();
    this.moduleForm.markAsUntouched();
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

    if (this.moduleService.userIsSecurityModuleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Modules", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.moduleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.moduleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.moduleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const moduleSubmitData: ModuleSubmitData = {
        id: this.moduleData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.moduleService.PutModule(moduleSubmitData.id, moduleSubmitData)
      : this.moduleService.PostModule(moduleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedModuleData) => {

        this.moduleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Module's detail page
          //
          this.moduleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.moduleForm.markAsUntouched();

          this.router.navigate(['/modules', savedModuleData.id]);
          this.alertService.showMessage('Module added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.moduleData = savedModuleData;
          this.buildFormValues(this.moduleData);

          this.alertService.showMessage("Module saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Module.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Module.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Module could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityModuleReader(): boolean {
    return this.moduleService.userIsSecurityModuleReader();
  }

  public userIsSecurityModuleWriter(): boolean {
    return this.moduleService.userIsSecurityModuleWriter();
  }
}

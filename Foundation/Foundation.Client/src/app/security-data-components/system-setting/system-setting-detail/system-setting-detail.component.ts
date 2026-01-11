import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SystemSettingService, SystemSettingData, SystemSettingSubmitData } from '../../../security-data-services/system-setting.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-system-setting-detail',
  templateUrl: './system-setting-detail.component.html',
  styleUrls: ['./system-setting-detail.component.scss']
})

export class SystemSettingDetailComponent implements OnInit, CanComponentDeactivate {

  systemSettingForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        value: [''],
        active: [true],
        deleted: [false],
      });


  public systemSettingId: string | null = null;
  public systemSettingData: SystemSettingData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  systemSettings$ = this.systemSettingService.GetSystemSettingList();

  private destroy$ = new Subject<void>();

  constructor(
    public systemSettingService: SystemSettingService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the systemSettingId from the route parameters
    this.systemSettingId = this.route.snapshot.paramMap.get('systemSettingId');

    if (this.systemSettingId === 'new' ||
        this.systemSettingId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.systemSettingData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New System Setting';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit System Setting';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.systemSettingForm.dirty) {
      return confirm('You have unsaved System Setting changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.systemSettingId != null && this.systemSettingId !== 'new') {

      const id = parseInt(this.systemSettingId, 10);

      if (!isNaN(id)) {
        return { systemSettingId: id };
      }
    }

    return null;
  }


/*
  * Loads the SystemSetting data for the current systemSettingId.
  *
  * Fully respects the SystemSettingService caching strategy and error handling strategy.
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
    if (!this.systemSettingService.userIsSecuritySystemSettingReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SystemSettings.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate systemSettingId
    //
    if (!this.systemSettingId) {

      this.alertService.showMessage('No SystemSetting ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const systemSettingId = Number(this.systemSettingId);

    if (isNaN(systemSettingId) || systemSettingId <= 0) {

      this.alertService.showMessage(`Invalid System Setting ID: "${this.systemSettingId}"`,
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
      // This is the most targeted way: clear only this SystemSetting + relations

      this.systemSettingService.ClearRecordCache(systemSettingId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.systemSettingService.GetSystemSetting(systemSettingId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (systemSettingData) => {

        //
        // Success path — systemSettingData can legitimately be null if 404'd but request succeeded
        //
        if (!systemSettingData) {

          this.handleSystemSettingNotFound(systemSettingId);

        } else {

          this.systemSettingData = systemSettingData;
          this.buildFormValues(this.systemSettingData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SystemSetting loaded successfully',
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
        this.handleSystemSettingLoadError(error, systemSettingId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSystemSettingNotFound(systemSettingId: number): void {

    this.systemSettingData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SystemSetting #${systemSettingId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSystemSettingLoadError(error: any, systemSettingId: number): void {

    let message = 'Failed to load System Setting.';
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
          message = 'You do not have permission to view this System Setting.';
          title = 'Forbidden';
          break;
        case 404:
          message = `System Setting #${systemSettingId} was not found.`;
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

    console.error(`System Setting load failed (ID: ${systemSettingId})`, error);

    //
    // Reset UI to safe state
    //
    this.systemSettingData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(systemSettingData: SystemSettingData | null) {

    if (systemSettingData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.systemSettingForm.reset({
        name: '',
        description: '',
        value: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.systemSettingForm.reset({
        name: systemSettingData.name ?? '',
        description: systemSettingData.description ?? '',
        value: systemSettingData.value ?? '',
        active: systemSettingData.active ?? true,
        deleted: systemSettingData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.systemSettingForm.markAsPristine();
    this.systemSettingForm.markAsUntouched();
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

    if (this.systemSettingService.userIsSecuritySystemSettingWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to System Settings", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.systemSettingForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.systemSettingForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.systemSettingForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const systemSettingSubmitData: SystemSettingSubmitData = {
        id: this.systemSettingData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        value: formValue.value?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.systemSettingService.PutSystemSetting(systemSettingSubmitData.id, systemSettingSubmitData)
      : this.systemSettingService.PostSystemSetting(systemSettingSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSystemSettingData) => {

        this.systemSettingService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created System Setting's detail page
          //
          this.systemSettingForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.systemSettingForm.markAsUntouched();

          this.router.navigate(['/systemsettings', savedSystemSettingData.id]);
          this.alertService.showMessage('System Setting added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.systemSettingData = savedSystemSettingData;
          this.buildFormValues(this.systemSettingData);

          this.alertService.showMessage("System Setting saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this System Setting.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the System Setting.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('System Setting could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySystemSettingReader(): boolean {
    return this.systemSettingService.userIsSecuritySystemSettingReader();
  }

  public userIsSecuritySystemSettingWriter(): boolean {
    return this.systemSettingService.userIsSecuritySystemSettingWriter();
  }
}

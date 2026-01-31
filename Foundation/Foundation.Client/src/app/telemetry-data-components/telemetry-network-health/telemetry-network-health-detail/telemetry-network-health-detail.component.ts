/*
   GENERATED FORM FOR THE TELEMETRYNETWORKHEALTH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from TelemetryNetworkHealth table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to telemetry-network-health-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TelemetryNetworkHealthService, TelemetryNetworkHealthData, TelemetryNetworkHealthSubmitData } from '../../../telemetry-data-services/telemetry-network-health.service';
import { TelemetrySnapshotService } from '../../../telemetry-data-services/telemetry-snapshot.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TelemetryNetworkHealthFormValues {
  telemetrySnapshotId: number | bigint,       // For FK link number
  interfaceName: string,
  interfaceDescription: string | null,
  linkSpeedMbps: string | null,     // Stored as string for form input, converted to number on submit.
  bytesSentTotal: string | null,     // Stored as string for form input, converted to number on submit.
  bytesReceivedTotal: string | null,     // Stored as string for form input, converted to number on submit.
  bytesSentPerSecond: string | null,     // Stored as string for form input, converted to number on submit.
  bytesReceivedPerSecond: string | null,     // Stored as string for form input, converted to number on submit.
  utilizationPercent: string | null,     // Stored as string for form input, converted to number on submit.
  status: string | null,
  isActive: boolean,
};


@Component({
  selector: 'app-telemetry-network-health-detail',
  templateUrl: './telemetry-network-health-detail.component.html',
  styleUrls: ['./telemetry-network-health-detail.component.scss']
})

export class TelemetryNetworkHealthDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TelemetryNetworkHealthFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public telemetryNetworkHealthForm: FormGroup = this.fb.group({
        telemetrySnapshotId: [null, Validators.required],
        interfaceName: ['', Validators.required],
        interfaceDescription: [''],
        linkSpeedMbps: [''],
        bytesSentTotal: [''],
        bytesReceivedTotal: [''],
        bytesSentPerSecond: [''],
        bytesReceivedPerSecond: [''],
        utilizationPercent: [''],
        status: [''],
        isActive: [false],
      });


  public telemetryNetworkHealthId: string | null = null;
  public telemetryNetworkHealthData: TelemetryNetworkHealthData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  telemetryNetworkHealths$ = this.telemetryNetworkHealthService.GetTelemetryNetworkHealthList();
  public telemetrySnapshots$ = this.telemetrySnapshotService.GetTelemetrySnapshotList();

  private destroy$ = new Subject<void>();

  constructor(
    public telemetryNetworkHealthService: TelemetryNetworkHealthService,
    public telemetrySnapshotService: TelemetrySnapshotService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the telemetryNetworkHealthId from the route parameters
    this.telemetryNetworkHealthId = this.route.snapshot.paramMap.get('telemetryNetworkHealthId');

    if (this.telemetryNetworkHealthId === 'new' ||
        this.telemetryNetworkHealthId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.telemetryNetworkHealthData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.telemetryNetworkHealthForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.telemetryNetworkHealthForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Telemetry Network Health';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Telemetry Network Health';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.telemetryNetworkHealthForm.dirty) {
      return confirm('You have unsaved Telemetry Network Health changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.telemetryNetworkHealthId != null && this.telemetryNetworkHealthId !== 'new') {

      const id = parseInt(this.telemetryNetworkHealthId, 10);

      if (!isNaN(id)) {
        return { telemetryNetworkHealthId: id };
      }
    }

    return null;
  }


/*
  * Loads the TelemetryNetworkHealth data for the current telemetryNetworkHealthId.
  *
  * Fully respects the TelemetryNetworkHealthService caching strategy and error handling strategy.
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
    if (!this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read TelemetryNetworkHealths.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate telemetryNetworkHealthId
    //
    if (!this.telemetryNetworkHealthId) {

      this.alertService.showMessage('No TelemetryNetworkHealth ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const telemetryNetworkHealthId = Number(this.telemetryNetworkHealthId);

    if (isNaN(telemetryNetworkHealthId) || telemetryNetworkHealthId <= 0) {

      this.alertService.showMessage(`Invalid Telemetry Network Health ID: "${this.telemetryNetworkHealthId}"`,
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
      // This is the most targeted way: clear only this TelemetryNetworkHealth + relations

      this.telemetryNetworkHealthService.ClearRecordCache(telemetryNetworkHealthId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.telemetryNetworkHealthService.GetTelemetryNetworkHealth(telemetryNetworkHealthId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (telemetryNetworkHealthData) => {

        //
        // Success path — telemetryNetworkHealthData can legitimately be null if 404'd but request succeeded
        //
        if (!telemetryNetworkHealthData) {

          this.handleTelemetryNetworkHealthNotFound(telemetryNetworkHealthId);

        } else {

          this.telemetryNetworkHealthData = telemetryNetworkHealthData;
          this.buildFormValues(this.telemetryNetworkHealthData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'TelemetryNetworkHealth loaded successfully',
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
        this.handleTelemetryNetworkHealthLoadError(error, telemetryNetworkHealthId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTelemetryNetworkHealthNotFound(telemetryNetworkHealthId: number): void {

    this.telemetryNetworkHealthData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `TelemetryNetworkHealth #${telemetryNetworkHealthId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTelemetryNetworkHealthLoadError(error: any, telemetryNetworkHealthId: number): void {

    let message = 'Failed to load Telemetry Network Health.';
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
          message = 'You do not have permission to view this Telemetry Network Health.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Telemetry Network Health #${telemetryNetworkHealthId} was not found.`;
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

    console.error(`Telemetry Network Health load failed (ID: ${telemetryNetworkHealthId})`, error);

    //
    // Reset UI to safe state
    //
    this.telemetryNetworkHealthData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(telemetryNetworkHealthData: TelemetryNetworkHealthData | null) {

    if (telemetryNetworkHealthData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.telemetryNetworkHealthForm.reset({
        telemetrySnapshotId: null,
        interfaceName: '',
        interfaceDescription: '',
        linkSpeedMbps: '',
        bytesSentTotal: '',
        bytesReceivedTotal: '',
        bytesSentPerSecond: '',
        bytesReceivedPerSecond: '',
        utilizationPercent: '',
        status: '',
        isActive: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.telemetryNetworkHealthForm.reset({
        telemetrySnapshotId: telemetryNetworkHealthData.telemetrySnapshotId,
        interfaceName: telemetryNetworkHealthData.interfaceName ?? '',
        interfaceDescription: telemetryNetworkHealthData.interfaceDescription ?? '',
        linkSpeedMbps: telemetryNetworkHealthData.linkSpeedMbps?.toString() ?? '',
        bytesSentTotal: telemetryNetworkHealthData.bytesSentTotal?.toString() ?? '',
        bytesReceivedTotal: telemetryNetworkHealthData.bytesReceivedTotal?.toString() ?? '',
        bytesSentPerSecond: telemetryNetworkHealthData.bytesSentPerSecond?.toString() ?? '',
        bytesReceivedPerSecond: telemetryNetworkHealthData.bytesReceivedPerSecond?.toString() ?? '',
        utilizationPercent: telemetryNetworkHealthData.utilizationPercent?.toString() ?? '',
        status: telemetryNetworkHealthData.status ?? '',
        isActive: telemetryNetworkHealthData.isActive ?? false,
      }, { emitEvent: false});
    }

    this.telemetryNetworkHealthForm.markAsPristine();
    this.telemetryNetworkHealthForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Telemetry Network Healths", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.telemetryNetworkHealthForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.telemetryNetworkHealthForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.telemetryNetworkHealthForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const telemetryNetworkHealthSubmitData: TelemetryNetworkHealthSubmitData = {
        id: this.telemetryNetworkHealthData?.id || 0,
        telemetrySnapshotId: Number(formValue.telemetrySnapshotId),
        interfaceName: formValue.interfaceName!.trim(),
        interfaceDescription: formValue.interfaceDescription?.trim() || null,
        linkSpeedMbps: formValue.linkSpeedMbps ? Number(formValue.linkSpeedMbps) : null,
        bytesSentTotal: formValue.bytesSentTotal ? Number(formValue.bytesSentTotal) : null,
        bytesReceivedTotal: formValue.bytesReceivedTotal ? Number(formValue.bytesReceivedTotal) : null,
        bytesSentPerSecond: formValue.bytesSentPerSecond ? Number(formValue.bytesSentPerSecond) : null,
        bytesReceivedPerSecond: formValue.bytesReceivedPerSecond ? Number(formValue.bytesReceivedPerSecond) : null,
        utilizationPercent: formValue.utilizationPercent ? Number(formValue.utilizationPercent) : null,
        status: formValue.status?.trim() || null,
        isActive: !!formValue.isActive,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.telemetryNetworkHealthService.PutTelemetryNetworkHealth(telemetryNetworkHealthSubmitData.id, telemetryNetworkHealthSubmitData)
      : this.telemetryNetworkHealthService.PostTelemetryNetworkHealth(telemetryNetworkHealthSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTelemetryNetworkHealthData) => {

        this.telemetryNetworkHealthService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Telemetry Network Health's detail page
          //
          this.telemetryNetworkHealthForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.telemetryNetworkHealthForm.markAsUntouched();

          this.router.navigate(['/telemetrynetworkhealths', savedTelemetryNetworkHealthData.id]);
          this.alertService.showMessage('Telemetry Network Health added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.telemetryNetworkHealthData = savedTelemetryNetworkHealthData;
          this.buildFormValues(this.telemetryNetworkHealthData);

          this.alertService.showMessage("Telemetry Network Health saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Telemetry Network Health.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Telemetry Network Health.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Telemetry Network Health could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsTelemetryTelemetryNetworkHealthReader(): boolean {
    return this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthReader();
  }

  public userIsTelemetryTelemetryNetworkHealthWriter(): boolean {
    return this.telemetryNetworkHealthService.userIsTelemetryTelemetryNetworkHealthWriter();
  }
}

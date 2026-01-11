/*
   GENERATED FORM FOR THE RESOURCEAVAILABILITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceAvailability table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-availability-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceAvailabilityService, ResourceAvailabilityData, ResourceAvailabilitySubmitData } from '../../../scheduler-data-services/resource-availability.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { ResourceAvailabilityChangeHistoryService } from '../../../scheduler-data-services/resource-availability-change-history.service';
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
interface ResourceAvailabilityFormValues {
  resourceId: number | bigint,       // For FK link number
  timeZoneId: number | bigint | null,       // For FK link number
  startDateTime: string,
  endDateTime: string | null,
  reason: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-resource-availability-detail',
  templateUrl: './resource-availability-detail.component.html',
  styleUrls: ['./resource-availability-detail.component.scss']
})

export class ResourceAvailabilityDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceAvailabilityFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceAvailabilityForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        timeZoneId: [null],
        startDateTime: ['', Validators.required],
        endDateTime: [''],
        reason: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public resourceAvailabilityId: string | null = null;
  public resourceAvailabilityData: ResourceAvailabilityData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  resourceAvailabilities$ = this.resourceAvailabilityService.GetResourceAvailabilityList();
  public resources$ = this.resourceService.GetResourceList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public resourceAvailabilityChangeHistories$ = this.resourceAvailabilityChangeHistoryService.GetResourceAvailabilityChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public resourceAvailabilityService: ResourceAvailabilityService,
    public resourceService: ResourceService,
    public timeZoneService: TimeZoneService,
    public resourceAvailabilityChangeHistoryService: ResourceAvailabilityChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the resourceAvailabilityId from the route parameters
    this.resourceAvailabilityId = this.route.snapshot.paramMap.get('resourceAvailabilityId');

    if (this.resourceAvailabilityId === 'new' ||
        this.resourceAvailabilityId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.resourceAvailabilityData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.resourceAvailabilityForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceAvailabilityForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Resource Availability';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Resource Availability';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.resourceAvailabilityForm.dirty) {
      return confirm('You have unsaved Resource Availability changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.resourceAvailabilityId != null && this.resourceAvailabilityId !== 'new') {

      const id = parseInt(this.resourceAvailabilityId, 10);

      if (!isNaN(id)) {
        return { resourceAvailabilityId: id };
      }
    }

    return null;
  }


/*
  * Loads the ResourceAvailability data for the current resourceAvailabilityId.
  *
  * Fully respects the ResourceAvailabilityService caching strategy and error handling strategy.
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
    if (!this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ResourceAvailabilities.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate resourceAvailabilityId
    //
    if (!this.resourceAvailabilityId) {

      this.alertService.showMessage('No ResourceAvailability ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const resourceAvailabilityId = Number(this.resourceAvailabilityId);

    if (isNaN(resourceAvailabilityId) || resourceAvailabilityId <= 0) {

      this.alertService.showMessage(`Invalid Resource Availability ID: "${this.resourceAvailabilityId}"`,
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
      // This is the most targeted way: clear only this ResourceAvailability + relations

      this.resourceAvailabilityService.ClearRecordCache(resourceAvailabilityId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.resourceAvailabilityService.GetResourceAvailability(resourceAvailabilityId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (resourceAvailabilityData) => {

        //
        // Success path — resourceAvailabilityData can legitimately be null if 404'd but request succeeded
        //
        if (!resourceAvailabilityData) {

          this.handleResourceAvailabilityNotFound(resourceAvailabilityId);

        } else {

          this.resourceAvailabilityData = resourceAvailabilityData;
          this.buildFormValues(this.resourceAvailabilityData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ResourceAvailability loaded successfully',
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
        this.handleResourceAvailabilityLoadError(error, resourceAvailabilityId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleResourceAvailabilityNotFound(resourceAvailabilityId: number): void {

    this.resourceAvailabilityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ResourceAvailability #${resourceAvailabilityId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleResourceAvailabilityLoadError(error: any, resourceAvailabilityId: number): void {

    let message = 'Failed to load Resource Availability.';
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
          message = 'You do not have permission to view this Resource Availability.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Resource Availability #${resourceAvailabilityId} was not found.`;
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

    console.error(`Resource Availability load failed (ID: ${resourceAvailabilityId})`, error);

    //
    // Reset UI to safe state
    //
    this.resourceAvailabilityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(resourceAvailabilityData: ResourceAvailabilityData | null) {

    if (resourceAvailabilityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceAvailabilityForm.reset({
        resourceId: null,
        timeZoneId: null,
        startDateTime: '',
        endDateTime: '',
        reason: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceAvailabilityForm.reset({
        resourceId: resourceAvailabilityData.resourceId,
        timeZoneId: resourceAvailabilityData.timeZoneId,
        startDateTime: isoUtcStringToDateTimeLocal(resourceAvailabilityData.startDateTime) ?? '',
        endDateTime: isoUtcStringToDateTimeLocal(resourceAvailabilityData.endDateTime) ?? '',
        reason: resourceAvailabilityData.reason ?? '',
        notes: resourceAvailabilityData.notes ?? '',
        versionNumber: resourceAvailabilityData.versionNumber?.toString() ?? '',
        active: resourceAvailabilityData.active ?? true,
        deleted: resourceAvailabilityData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceAvailabilityForm.markAsPristine();
    this.resourceAvailabilityForm.markAsUntouched();
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

    if (this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Resource Availabilities", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.resourceAvailabilityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceAvailabilityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceAvailabilityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceAvailabilitySubmitData: ResourceAvailabilitySubmitData = {
        id: this.resourceAvailabilityData?.id || 0,
        resourceId: Number(formValue.resourceId),
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        startDateTime: dateTimeLocalToIsoUtc(formValue.startDateTime!.trim())!,
        endDateTime: formValue.endDateTime ? dateTimeLocalToIsoUtc(formValue.endDateTime.trim()) : null,
        reason: formValue.reason?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.resourceAvailabilityData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.resourceAvailabilityService.PutResourceAvailability(resourceAvailabilitySubmitData.id, resourceAvailabilitySubmitData)
      : this.resourceAvailabilityService.PostResourceAvailability(resourceAvailabilitySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedResourceAvailabilityData) => {

        this.resourceAvailabilityService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Resource Availability's detail page
          //
          this.resourceAvailabilityForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.resourceAvailabilityForm.markAsUntouched();

          this.router.navigate(['/resourceavailabilities', savedResourceAvailabilityData.id]);
          this.alertService.showMessage('Resource Availability added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.resourceAvailabilityData = savedResourceAvailabilityData;
          this.buildFormValues(this.resourceAvailabilityData);

          this.alertService.showMessage("Resource Availability saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Resource Availability.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Availability.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Availability could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerResourceAvailabilityReader(): boolean {
    return this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityReader();
  }

  public userIsSchedulerResourceAvailabilityWriter(): boolean {
    return this.resourceAvailabilityService.userIsSchedulerResourceAvailabilityWriter();
  }
}

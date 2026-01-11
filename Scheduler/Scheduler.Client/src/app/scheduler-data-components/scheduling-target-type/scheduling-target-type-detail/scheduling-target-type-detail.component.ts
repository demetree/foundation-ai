/*
   GENERATED FORM FOR THE SCHEDULINGTARGETTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetTypeService, SchedulingTargetTypeData, SchedulingTargetTypeSubmitData } from '../../../scheduler-data-services/scheduling-target-type.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { ScheduledEventTemplateService } from '../../../scheduler-data-services/scheduled-event-template.service';
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
interface SchedulingTargetTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduling-target-type-detail',
  templateUrl: './scheduling-target-type-detail.component.html',
  styleUrls: ['./scheduling-target-type-detail.component.scss']
})

export class SchedulingTargetTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public schedulingTargetTypeId: string | null = null;
  public schedulingTargetTypeData: SchedulingTargetTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  schedulingTargetTypes$ = this.schedulingTargetTypeService.GetSchedulingTargetTypeList();
  public icons$ = this.iconService.GetIconList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();

  private destroy$ = new Subject<void>();

  constructor(
    public schedulingTargetTypeService: SchedulingTargetTypeService,
    public iconService: IconService,
    public schedulingTargetService: SchedulingTargetService,
    public scheduledEventTemplateService: ScheduledEventTemplateService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the schedulingTargetTypeId from the route parameters
    this.schedulingTargetTypeId = this.route.snapshot.paramMap.get('schedulingTargetTypeId');

    if (this.schedulingTargetTypeId === 'new' ||
        this.schedulingTargetTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.schedulingTargetTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.schedulingTargetTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduling Target Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduling Target Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.schedulingTargetTypeForm.dirty) {
      return confirm('You have unsaved Scheduling Target Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.schedulingTargetTypeId != null && this.schedulingTargetTypeId !== 'new') {

      const id = parseInt(this.schedulingTargetTypeId, 10);

      if (!isNaN(id)) {
        return { schedulingTargetTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the SchedulingTargetType data for the current schedulingTargetTypeId.
  *
  * Fully respects the SchedulingTargetTypeService caching strategy and error handling strategy.
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
    if (!this.schedulingTargetTypeService.userIsSchedulerSchedulingTargetTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SchedulingTargetTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate schedulingTargetTypeId
    //
    if (!this.schedulingTargetTypeId) {

      this.alertService.showMessage('No SchedulingTargetType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const schedulingTargetTypeId = Number(this.schedulingTargetTypeId);

    if (isNaN(schedulingTargetTypeId) || schedulingTargetTypeId <= 0) {

      this.alertService.showMessage(`Invalid Scheduling Target Type ID: "${this.schedulingTargetTypeId}"`,
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
      // This is the most targeted way: clear only this SchedulingTargetType + relations

      this.schedulingTargetTypeService.ClearRecordCache(schedulingTargetTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.schedulingTargetTypeService.GetSchedulingTargetType(schedulingTargetTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (schedulingTargetTypeData) => {

        //
        // Success path — schedulingTargetTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!schedulingTargetTypeData) {

          this.handleSchedulingTargetTypeNotFound(schedulingTargetTypeId);

        } else {

          this.schedulingTargetTypeData = schedulingTargetTypeData;
          this.buildFormValues(this.schedulingTargetTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SchedulingTargetType loaded successfully',
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
        this.handleSchedulingTargetTypeLoadError(error, schedulingTargetTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSchedulingTargetTypeNotFound(schedulingTargetTypeId: number): void {

    this.schedulingTargetTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SchedulingTargetType #${schedulingTargetTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSchedulingTargetTypeLoadError(error: any, schedulingTargetTypeId: number): void {

    let message = 'Failed to load Scheduling Target Type.';
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
          message = 'You do not have permission to view this Scheduling Target Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduling Target Type #${schedulingTargetTypeId} was not found.`;
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

    console.error(`Scheduling Target Type load failed (ID: ${schedulingTargetTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.schedulingTargetTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(schedulingTargetTypeData: SchedulingTargetTypeData | null) {

    if (schedulingTargetTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        iconId: null,
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.schedulingTargetTypeForm.reset({
        name: schedulingTargetTypeData.name ?? '',
        description: schedulingTargetTypeData.description ?? '',
        sequence: schedulingTargetTypeData.sequence?.toString() ?? '',
        iconId: schedulingTargetTypeData.iconId,
        color: schedulingTargetTypeData.color ?? '',
        active: schedulingTargetTypeData.active ?? true,
        deleted: schedulingTargetTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetTypeForm.markAsPristine();
    this.schedulingTargetTypeForm.markAsUntouched();
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

    if (this.schedulingTargetTypeService.userIsSchedulerSchedulingTargetTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduling Target Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.schedulingTargetTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetTypeSubmitData: SchedulingTargetTypeSubmitData = {
        id: this.schedulingTargetTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.schedulingTargetTypeService.PutSchedulingTargetType(schedulingTargetTypeSubmitData.id, schedulingTargetTypeSubmitData)
      : this.schedulingTargetTypeService.PostSchedulingTargetType(schedulingTargetTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSchedulingTargetTypeData) => {

        this.schedulingTargetTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduling Target Type's detail page
          //
          this.schedulingTargetTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.schedulingTargetTypeForm.markAsUntouched();

          this.router.navigate(['/schedulingtargettypes', savedSchedulingTargetTypeData.id]);
          this.alertService.showMessage('Scheduling Target Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.schedulingTargetTypeData = savedSchedulingTargetTypeData;
          this.buildFormValues(this.schedulingTargetTypeData);

          this.alertService.showMessage("Scheduling Target Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduling Target Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSchedulingTargetTypeReader(): boolean {
    return this.schedulingTargetTypeService.userIsSchedulerSchedulingTargetTypeReader();
  }

  public userIsSchedulerSchedulingTargetTypeWriter(): boolean {
    return this.schedulingTargetTypeService.userIsSchedulerSchedulingTargetTypeWriter();
  }
}

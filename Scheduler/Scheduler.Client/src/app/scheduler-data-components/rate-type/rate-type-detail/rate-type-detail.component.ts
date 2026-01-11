/*
   GENERATED FORM FOR THE RATETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RateType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rate-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RateTypeService, RateTypeData, RateTypeSubmitData } from '../../../scheduler-data-services/rate-type.service';
import { ChargeTypeService } from '../../../scheduler-data-services/charge-type.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
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
interface RateTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-rate-type-detail',
  templateUrl: './rate-type-detail.component.html',
  styleUrls: ['./rate-type-detail.component.scss']
})

export class RateTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RateTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rateTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public rateTypeId: string | null = null;
  public rateTypeData: RateTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  rateTypes$ = this.rateTypeService.GetRateTypeList();
  public chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();

  private destroy$ = new Subject<void>();

  constructor(
    public rateTypeService: RateTypeService,
    public chargeTypeService: ChargeTypeService,
    public rateSheetService: RateSheetService,
    public eventChargeService: EventChargeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the rateTypeId from the route parameters
    this.rateTypeId = this.route.snapshot.paramMap.get('rateTypeId');

    if (this.rateTypeId === 'new' ||
        this.rateTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.rateTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.rateTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rateTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Rate Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Rate Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.rateTypeForm.dirty) {
      return confirm('You have unsaved Rate Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.rateTypeId != null && this.rateTypeId !== 'new') {

      const id = parseInt(this.rateTypeId, 10);

      if (!isNaN(id)) {
        return { rateTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the RateType data for the current rateTypeId.
  *
  * Fully respects the RateTypeService caching strategy and error handling strategy.
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
    if (!this.rateTypeService.userIsSchedulerRateTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RateTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate rateTypeId
    //
    if (!this.rateTypeId) {

      this.alertService.showMessage('No RateType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const rateTypeId = Number(this.rateTypeId);

    if (isNaN(rateTypeId) || rateTypeId <= 0) {

      this.alertService.showMessage(`Invalid Rate Type ID: "${this.rateTypeId}"`,
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
      // This is the most targeted way: clear only this RateType + relations

      this.rateTypeService.ClearRecordCache(rateTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.rateTypeService.GetRateType(rateTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (rateTypeData) => {

        //
        // Success path — rateTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!rateTypeData) {

          this.handleRateTypeNotFound(rateTypeId);

        } else {

          this.rateTypeData = rateTypeData;
          this.buildFormValues(this.rateTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RateType loaded successfully',
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
        this.handleRateTypeLoadError(error, rateTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRateTypeNotFound(rateTypeId: number): void {

    this.rateTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RateType #${rateTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRateTypeLoadError(error: any, rateTypeId: number): void {

    let message = 'Failed to load Rate Type.';
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
          message = 'You do not have permission to view this Rate Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Rate Type #${rateTypeId} was not found.`;
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

    console.error(`Rate Type load failed (ID: ${rateTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.rateTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(rateTypeData: RateTypeData | null) {

    if (rateTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rateTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.rateTypeForm.reset({
        name: rateTypeData.name ?? '',
        description: rateTypeData.description ?? '',
        sequence: rateTypeData.sequence?.toString() ?? '',
        color: rateTypeData.color ?? '',
        active: rateTypeData.active ?? true,
        deleted: rateTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rateTypeForm.markAsPristine();
    this.rateTypeForm.markAsUntouched();
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

    if (this.rateTypeService.userIsSchedulerRateTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Rate Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.rateTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rateTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rateTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rateTypeSubmitData: RateTypeSubmitData = {
        id: this.rateTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.rateTypeService.PutRateType(rateTypeSubmitData.id, rateTypeSubmitData)
      : this.rateTypeService.PostRateType(rateTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRateTypeData) => {

        this.rateTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Rate Type's detail page
          //
          this.rateTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.rateTypeForm.markAsUntouched();

          this.router.navigate(['/ratetypes', savedRateTypeData.id]);
          this.alertService.showMessage('Rate Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.rateTypeData = savedRateTypeData;
          this.buildFormValues(this.rateTypeData);

          this.alertService.showMessage("Rate Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Rate Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rate Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rate Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerRateTypeReader(): boolean {
    return this.rateTypeService.userIsSchedulerRateTypeReader();
  }

  public userIsSchedulerRateTypeWriter(): boolean {
    return this.rateTypeService.userIsSchedulerRateTypeWriter();
  }
}

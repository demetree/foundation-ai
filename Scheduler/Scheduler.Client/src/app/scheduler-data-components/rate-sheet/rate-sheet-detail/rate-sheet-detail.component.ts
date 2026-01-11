/*
   GENERATED FORM FOR THE RATESHEET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RateSheet table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rate-sheet-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RateSheetService, RateSheetData, RateSheetSubmitData } from '../../../scheduler-data-services/rate-sheet.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { RateSheetChangeHistoryService } from '../../../scheduler-data-services/rate-sheet-change-history.service';
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
interface RateSheetFormValues {
  officeId: number | bigint | null,       // For FK link number
  assignmentRoleId: number | bigint | null,       // For FK link number
  resourceId: number | bigint | null,       // For FK link number
  schedulingTargetId: number | bigint | null,       // For FK link number
  rateTypeId: number | bigint,       // For FK link number
  effectiveDate: string,
  currencyId: number | bigint,       // For FK link number
  costRate: string,     // Stored as string for form input, converted to number on submit.
  billingRate: string,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-rate-sheet-detail',
  templateUrl: './rate-sheet-detail.component.html',
  styleUrls: ['./rate-sheet-detail.component.scss']
})

export class RateSheetDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RateSheetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rateSheetForm: FormGroup = this.fb.group({
        officeId: [null],
        assignmentRoleId: [null],
        resourceId: [null],
        schedulingTargetId: [null],
        rateTypeId: [null, Validators.required],
        effectiveDate: ['', Validators.required],
        currencyId: [null, Validators.required],
        costRate: ['', Validators.required],
        billingRate: ['', Validators.required],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public rateSheetId: string | null = null;
  public rateSheetData: RateSheetData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  rateSheets$ = this.rateSheetService.GetRateSheetList();
  public offices$ = this.officeService.GetOfficeList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public resources$ = this.resourceService.GetResourceList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public rateTypes$ = this.rateTypeService.GetRateTypeList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public rateSheetChangeHistories$ = this.rateSheetChangeHistoryService.GetRateSheetChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public rateSheetService: RateSheetService,
    public officeService: OfficeService,
    public assignmentRoleService: AssignmentRoleService,
    public resourceService: ResourceService,
    public schedulingTargetService: SchedulingTargetService,
    public rateTypeService: RateTypeService,
    public currencyService: CurrencyService,
    public rateSheetChangeHistoryService: RateSheetChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the rateSheetId from the route parameters
    this.rateSheetId = this.route.snapshot.paramMap.get('rateSheetId');

    if (this.rateSheetId === 'new' ||
        this.rateSheetId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.rateSheetData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.rateSheetForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rateSheetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Rate Sheet';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Rate Sheet';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.rateSheetForm.dirty) {
      return confirm('You have unsaved Rate Sheet changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.rateSheetId != null && this.rateSheetId !== 'new') {

      const id = parseInt(this.rateSheetId, 10);

      if (!isNaN(id)) {
        return { rateSheetId: id };
      }
    }

    return null;
  }


/*
  * Loads the RateSheet data for the current rateSheetId.
  *
  * Fully respects the RateSheetService caching strategy and error handling strategy.
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
    if (!this.rateSheetService.userIsSchedulerRateSheetReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RateSheets.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate rateSheetId
    //
    if (!this.rateSheetId) {

      this.alertService.showMessage('No RateSheet ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const rateSheetId = Number(this.rateSheetId);

    if (isNaN(rateSheetId) || rateSheetId <= 0) {

      this.alertService.showMessage(`Invalid Rate Sheet ID: "${this.rateSheetId}"`,
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
      // This is the most targeted way: clear only this RateSheet + relations

      this.rateSheetService.ClearRecordCache(rateSheetId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.rateSheetService.GetRateSheet(rateSheetId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (rateSheetData) => {

        //
        // Success path — rateSheetData can legitimately be null if 404'd but request succeeded
        //
        if (!rateSheetData) {

          this.handleRateSheetNotFound(rateSheetId);

        } else {

          this.rateSheetData = rateSheetData;
          this.buildFormValues(this.rateSheetData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RateSheet loaded successfully',
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
        this.handleRateSheetLoadError(error, rateSheetId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRateSheetNotFound(rateSheetId: number): void {

    this.rateSheetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RateSheet #${rateSheetId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRateSheetLoadError(error: any, rateSheetId: number): void {

    let message = 'Failed to load Rate Sheet.';
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
          message = 'You do not have permission to view this Rate Sheet.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Rate Sheet #${rateSheetId} was not found.`;
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

    console.error(`Rate Sheet load failed (ID: ${rateSheetId})`, error);

    //
    // Reset UI to safe state
    //
    this.rateSheetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(rateSheetData: RateSheetData | null) {

    if (rateSheetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rateSheetForm.reset({
        officeId: null,
        assignmentRoleId: null,
        resourceId: null,
        schedulingTargetId: null,
        rateTypeId: null,
        effectiveDate: '',
        currencyId: null,
        costRate: '',
        billingRate: '',
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
        this.rateSheetForm.reset({
        officeId: rateSheetData.officeId,
        assignmentRoleId: rateSheetData.assignmentRoleId,
        resourceId: rateSheetData.resourceId,
        schedulingTargetId: rateSheetData.schedulingTargetId,
        rateTypeId: rateSheetData.rateTypeId,
        effectiveDate: isoUtcStringToDateTimeLocal(rateSheetData.effectiveDate) ?? '',
        currencyId: rateSheetData.currencyId,
        costRate: rateSheetData.costRate?.toString() ?? '',
        billingRate: rateSheetData.billingRate?.toString() ?? '',
        notes: rateSheetData.notes ?? '',
        versionNumber: rateSheetData.versionNumber?.toString() ?? '',
        active: rateSheetData.active ?? true,
        deleted: rateSheetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rateSheetForm.markAsPristine();
    this.rateSheetForm.markAsUntouched();
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

    if (this.rateSheetService.userIsSchedulerRateSheetWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Rate Sheets", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.rateSheetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rateSheetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rateSheetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rateSheetSubmitData: RateSheetSubmitData = {
        id: this.rateSheetData?.id || 0,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        schedulingTargetId: formValue.schedulingTargetId ? Number(formValue.schedulingTargetId) : null,
        rateTypeId: Number(formValue.rateTypeId),
        effectiveDate: dateTimeLocalToIsoUtc(formValue.effectiveDate!.trim())!,
        currencyId: Number(formValue.currencyId),
        costRate: Number(formValue.costRate),
        billingRate: Number(formValue.billingRate),
        notes: formValue.notes?.trim() || null,
        versionNumber: this.rateSheetData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.rateSheetService.PutRateSheet(rateSheetSubmitData.id, rateSheetSubmitData)
      : this.rateSheetService.PostRateSheet(rateSheetSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRateSheetData) => {

        this.rateSheetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Rate Sheet's detail page
          //
          this.rateSheetForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.rateSheetForm.markAsUntouched();

          this.router.navigate(['/ratesheets', savedRateSheetData.id]);
          this.alertService.showMessage('Rate Sheet added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.rateSheetData = savedRateSheetData;
          this.buildFormValues(this.rateSheetData);

          this.alertService.showMessage("Rate Sheet saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Rate Sheet.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rate Sheet.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rate Sheet could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerRateSheetReader(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetReader();
  }

  public userIsSchedulerRateSheetWriter(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetWriter();
  }
}

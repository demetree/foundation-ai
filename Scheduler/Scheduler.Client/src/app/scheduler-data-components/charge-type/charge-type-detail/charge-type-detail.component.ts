/*
   GENERATED FORM FOR THE CHARGETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ChargeType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to charge-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ChargeTypeService, ChargeTypeData, ChargeTypeSubmitData } from '../../../scheduler-data-services/charge-type.service';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
import { TaxCodeService } from '../../../scheduler-data-services/tax-code.service';
import { ChargeTypeChangeHistoryService } from '../../../scheduler-data-services/charge-type-change-history.service';
import { ScheduledEventTemplateChargeService } from '../../../scheduler-data-services/scheduled-event-template-charge.service';
import { EventTypeService } from '../../../scheduler-data-services/event-type.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
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
interface ChargeTypeFormValues {
  name: string,
  description: string,
  externalId: string | null,
  isRevenue: boolean,
  isTaxable: boolean | null,
  defaultAmount: string | null,     // Stored as string for form input, converted to number on submit.
  defaultDescription: string | null,
  rateTypeId: number | bigint | null,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  financialCategoryId: number | bigint | null,       // For FK link number
  taxCodeId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-charge-type-detail',
  templateUrl: './charge-type-detail.component.html',
  styleUrls: ['./charge-type-detail.component.scss']
})

export class ChargeTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ChargeTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public chargeTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        externalId: [''],
        isRevenue: [false],
        isTaxable: [false],
        defaultAmount: [''],
        defaultDescription: [''],
        rateTypeId: [null],
        currencyId: [null, Validators.required],
        financialCategoryId: [null],
        taxCodeId: [null],
        sequence: [''],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public chargeTypeId: string | null = null;
  public chargeTypeData: ChargeTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  chargeTypes$ = this.chargeTypeService.GetChargeTypeList();
  public rateTypes$ = this.rateTypeService.GetRateTypeList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();
  public taxCodes$ = this.taxCodeService.GetTaxCodeList();
  public chargeTypeChangeHistories$ = this.chargeTypeChangeHistoryService.GetChargeTypeChangeHistoryList();
  public scheduledEventTemplateCharges$ = this.scheduledEventTemplateChargeService.GetScheduledEventTemplateChargeList();
  public eventTypes$ = this.eventTypeService.GetEventTypeList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public chargeTypeService: ChargeTypeService,
    public rateTypeService: RateTypeService,
    public currencyService: CurrencyService,
    public financialCategoryService: FinancialCategoryService,
    public taxCodeService: TaxCodeService,
    public chargeTypeChangeHistoryService: ChargeTypeChangeHistoryService,
    public scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService,
    public eventTypeService: EventTypeService,
    public eventChargeService: EventChargeService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the chargeTypeId from the route parameters
    this.chargeTypeId = this.route.snapshot.paramMap.get('chargeTypeId');

    if (this.chargeTypeId === 'new' ||
        this.chargeTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.chargeTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.chargeTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.chargeTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Charge Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Charge Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.chargeTypeForm.dirty) {
      return confirm('You have unsaved Charge Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.chargeTypeId != null && this.chargeTypeId !== 'new') {

      const id = parseInt(this.chargeTypeId, 10);

      if (!isNaN(id)) {
        return { chargeTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the ChargeType data for the current chargeTypeId.
  *
  * Fully respects the ChargeTypeService caching strategy and error handling strategy.
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
    if (!this.chargeTypeService.userIsSchedulerChargeTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ChargeTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate chargeTypeId
    //
    if (!this.chargeTypeId) {

      this.alertService.showMessage('No ChargeType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const chargeTypeId = Number(this.chargeTypeId);

    if (isNaN(chargeTypeId) || chargeTypeId <= 0) {

      this.alertService.showMessage(`Invalid Charge Type ID: "${this.chargeTypeId}"`,
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
      // This is the most targeted way: clear only this ChargeType + relations

      this.chargeTypeService.ClearRecordCache(chargeTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.chargeTypeService.GetChargeType(chargeTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (chargeTypeData) => {

        //
        // Success path — chargeTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!chargeTypeData) {

          this.handleChargeTypeNotFound(chargeTypeId);

        } else {

          this.chargeTypeData = chargeTypeData;
          this.buildFormValues(this.chargeTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ChargeType loaded successfully',
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
        this.handleChargeTypeLoadError(error, chargeTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleChargeTypeNotFound(chargeTypeId: number): void {

    this.chargeTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ChargeType #${chargeTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleChargeTypeLoadError(error: any, chargeTypeId: number): void {

    let message = 'Failed to load Charge Type.';
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
          message = 'You do not have permission to view this Charge Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Charge Type #${chargeTypeId} was not found.`;
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

    console.error(`Charge Type load failed (ID: ${chargeTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.chargeTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(chargeTypeData: ChargeTypeData | null) {

    if (chargeTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.chargeTypeForm.reset({
        name: '',
        description: '',
        externalId: '',
        isRevenue: false,
        isTaxable: false,
        defaultAmount: '',
        defaultDescription: '',
        rateTypeId: null,
        currencyId: null,
        financialCategoryId: null,
        taxCodeId: null,
        sequence: '',
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.chargeTypeForm.reset({
        name: chargeTypeData.name ?? '',
        description: chargeTypeData.description ?? '',
        externalId: chargeTypeData.externalId ?? '',
        isRevenue: chargeTypeData.isRevenue ?? false,
        isTaxable: chargeTypeData.isTaxable ?? false,
        defaultAmount: chargeTypeData.defaultAmount?.toString() ?? '',
        defaultDescription: chargeTypeData.defaultDescription ?? '',
        rateTypeId: chargeTypeData.rateTypeId,
        currencyId: chargeTypeData.currencyId,
        financialCategoryId: chargeTypeData.financialCategoryId,
        taxCodeId: chargeTypeData.taxCodeId,
        sequence: chargeTypeData.sequence?.toString() ?? '',
        color: chargeTypeData.color ?? '',
        versionNumber: chargeTypeData.versionNumber?.toString() ?? '',
        active: chargeTypeData.active ?? true,
        deleted: chargeTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.chargeTypeForm.markAsPristine();
    this.chargeTypeForm.markAsUntouched();
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

    if (this.chargeTypeService.userIsSchedulerChargeTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Charge Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.chargeTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.chargeTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.chargeTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const chargeTypeSubmitData: ChargeTypeSubmitData = {
        id: this.chargeTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        externalId: formValue.externalId?.trim() || null,
        isRevenue: !!formValue.isRevenue,
        isTaxable: formValue.isTaxable == true ? true : formValue.isTaxable == false ? false : null,
        defaultAmount: formValue.defaultAmount ? Number(formValue.defaultAmount) : null,
        defaultDescription: formValue.defaultDescription?.trim() || null,
        rateTypeId: formValue.rateTypeId ? Number(formValue.rateTypeId) : null,
        currencyId: Number(formValue.currencyId),
        financialCategoryId: formValue.financialCategoryId ? Number(formValue.financialCategoryId) : null,
        taxCodeId: formValue.taxCodeId ? Number(formValue.taxCodeId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.chargeTypeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.chargeTypeService.PutChargeType(chargeTypeSubmitData.id, chargeTypeSubmitData)
      : this.chargeTypeService.PostChargeType(chargeTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedChargeTypeData) => {

        this.chargeTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Charge Type's detail page
          //
          this.chargeTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.chargeTypeForm.markAsUntouched();

          this.router.navigate(['/chargetypes', savedChargeTypeData.id]);
          this.alertService.showMessage('Charge Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.chargeTypeData = savedChargeTypeData;
          this.buildFormValues(this.chargeTypeData);

          this.alertService.showMessage("Charge Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Charge Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Charge Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Charge Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerChargeTypeReader(): boolean {
    return this.chargeTypeService.userIsSchedulerChargeTypeReader();
  }

  public userIsSchedulerChargeTypeWriter(): boolean {
    return this.chargeTypeService.userIsSchedulerChargeTypeWriter();
  }
}

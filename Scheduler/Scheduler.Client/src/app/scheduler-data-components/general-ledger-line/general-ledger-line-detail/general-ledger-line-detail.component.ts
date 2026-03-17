/*
   GENERATED FORM FOR THE GENERALLEDGERLINE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from GeneralLedgerLine table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to general-ledger-line-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { GeneralLedgerLineService, GeneralLedgerLineData, GeneralLedgerLineSubmitData } from '../../../scheduler-data-services/general-ledger-line.service';
import { GeneralLedgerEntryService } from '../../../scheduler-data-services/general-ledger-entry.service';
import { FinancialCategoryService } from '../../../scheduler-data-services/financial-category.service';
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
interface GeneralLedgerLineFormValues {
  generalLedgerEntryId: number | bigint,       // For FK link number
  financialCategoryId: number | bigint,       // For FK link number
  debitAmount: string,     // Stored as string for form input, converted to number on submit.
  creditAmount: string,     // Stored as string for form input, converted to number on submit.
  description: string | null,
};


@Component({
  selector: 'app-general-ledger-line-detail',
  templateUrl: './general-ledger-line-detail.component.html',
  styleUrls: ['./general-ledger-line-detail.component.scss']
})

export class GeneralLedgerLineDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<GeneralLedgerLineFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public generalLedgerLineForm: FormGroup = this.fb.group({
        generalLedgerEntryId: [null, Validators.required],
        financialCategoryId: [null, Validators.required],
        debitAmount: ['', Validators.required],
        creditAmount: ['', Validators.required],
        description: [''],
      });


  public generalLedgerLineId: string | null = null;
  public generalLedgerLineData: GeneralLedgerLineData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  generalLedgerLines$ = this.generalLedgerLineService.GetGeneralLedgerLineList();
  public generalLedgerEntries$ = this.generalLedgerEntryService.GetGeneralLedgerEntryList();
  public financialCategories$ = this.financialCategoryService.GetFinancialCategoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public generalLedgerLineService: GeneralLedgerLineService,
    public generalLedgerEntryService: GeneralLedgerEntryService,
    public financialCategoryService: FinancialCategoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the generalLedgerLineId from the route parameters
    this.generalLedgerLineId = this.route.snapshot.paramMap.get('generalLedgerLineId');

    if (this.generalLedgerLineId === 'new' ||
        this.generalLedgerLineId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.generalLedgerLineData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.generalLedgerLineForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.generalLedgerLineForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New General Ledger Line';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit General Ledger Line';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.generalLedgerLineForm.dirty) {
      return confirm('You have unsaved General Ledger Line changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.generalLedgerLineId != null && this.generalLedgerLineId !== 'new') {

      const id = parseInt(this.generalLedgerLineId, 10);

      if (!isNaN(id)) {
        return { generalLedgerLineId: id };
      }
    }

    return null;
  }


/*
  * Loads the GeneralLedgerLine data for the current generalLedgerLineId.
  *
  * Fully respects the GeneralLedgerLineService caching strategy and error handling strategy.
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
    if (!this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read GeneralLedgerLines.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate generalLedgerLineId
    //
    if (!this.generalLedgerLineId) {

      this.alertService.showMessage('No GeneralLedgerLine ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const generalLedgerLineId = Number(this.generalLedgerLineId);

    if (isNaN(generalLedgerLineId) || generalLedgerLineId <= 0) {

      this.alertService.showMessage(`Invalid General Ledger Line ID: "${this.generalLedgerLineId}"`,
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
      // This is the most targeted way: clear only this GeneralLedgerLine + relations

      this.generalLedgerLineService.ClearRecordCache(generalLedgerLineId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.generalLedgerLineService.GetGeneralLedgerLine(generalLedgerLineId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (generalLedgerLineData) => {

        //
        // Success path — generalLedgerLineData can legitimately be null if 404'd but request succeeded
        //
        if (!generalLedgerLineData) {

          this.handleGeneralLedgerLineNotFound(generalLedgerLineId);

        } else {

          this.generalLedgerLineData = generalLedgerLineData;
          this.buildFormValues(this.generalLedgerLineData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'GeneralLedgerLine loaded successfully',
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
        this.handleGeneralLedgerLineLoadError(error, generalLedgerLineId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleGeneralLedgerLineNotFound(generalLedgerLineId: number): void {

    this.generalLedgerLineData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `GeneralLedgerLine #${generalLedgerLineId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleGeneralLedgerLineLoadError(error: any, generalLedgerLineId: number): void {

    let message = 'Failed to load General Ledger Line.';
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
          message = 'You do not have permission to view this General Ledger Line.';
          title = 'Forbidden';
          break;
        case 404:
          message = `General Ledger Line #${generalLedgerLineId} was not found.`;
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

    console.error(`General Ledger Line load failed (ID: ${generalLedgerLineId})`, error);

    //
    // Reset UI to safe state
    //
    this.generalLedgerLineData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(generalLedgerLineData: GeneralLedgerLineData | null) {

    if (generalLedgerLineData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.generalLedgerLineForm.reset({
        generalLedgerEntryId: null,
        financialCategoryId: null,
        debitAmount: '',
        creditAmount: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.generalLedgerLineForm.reset({
        generalLedgerEntryId: generalLedgerLineData.generalLedgerEntryId,
        financialCategoryId: generalLedgerLineData.financialCategoryId,
        debitAmount: generalLedgerLineData.debitAmount?.toString() ?? '',
        creditAmount: generalLedgerLineData.creditAmount?.toString() ?? '',
        description: generalLedgerLineData.description ?? '',
      }, { emitEvent: false});
    }

    this.generalLedgerLineForm.markAsPristine();
    this.generalLedgerLineForm.markAsUntouched();
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

    if (this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to General Ledger Lines", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.generalLedgerLineForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.generalLedgerLineForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.generalLedgerLineForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const generalLedgerLineSubmitData: GeneralLedgerLineSubmitData = {
        id: this.generalLedgerLineData?.id || 0,
        generalLedgerEntryId: Number(formValue.generalLedgerEntryId),
        financialCategoryId: Number(formValue.financialCategoryId),
        debitAmount: Number(formValue.debitAmount),
        creditAmount: Number(formValue.creditAmount),
        description: formValue.description?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.generalLedgerLineService.PutGeneralLedgerLine(generalLedgerLineSubmitData.id, generalLedgerLineSubmitData)
      : this.generalLedgerLineService.PostGeneralLedgerLine(generalLedgerLineSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedGeneralLedgerLineData) => {

        this.generalLedgerLineService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created General Ledger Line's detail page
          //
          this.generalLedgerLineForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.generalLedgerLineForm.markAsUntouched();

          this.router.navigate(['/generalledgerlines', savedGeneralLedgerLineData.id]);
          this.alertService.showMessage('General Ledger Line added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.generalLedgerLineData = savedGeneralLedgerLineData;
          this.buildFormValues(this.generalLedgerLineData);

          this.alertService.showMessage("General Ledger Line saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this General Ledger Line.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the General Ledger Line.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('General Ledger Line could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerGeneralLedgerLineReader(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineReader();
  }

  public userIsSchedulerGeneralLedgerLineWriter(): boolean {
    return this.generalLedgerLineService.userIsSchedulerGeneralLedgerLineWriter();
  }
}

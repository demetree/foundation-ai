/*
   GENERATED FORM FOR THE GENERALLEDGERENTRY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from GeneralLedgerEntry table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to general-ledger-entry-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { GeneralLedgerEntryService, GeneralLedgerEntryData, GeneralLedgerEntrySubmitData } from '../../../scheduler-data-services/general-ledger-entry.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { FiscalPeriodService } from '../../../scheduler-data-services/fiscal-period.service';
import { FinancialOfficeService } from '../../../scheduler-data-services/financial-office.service';
import { GeneralLedgerLineService } from '../../../scheduler-data-services/general-ledger-line.service';
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
interface GeneralLedgerEntryFormValues {
  journalEntryNumber: string,     // Stored as string for form input, converted to number on submit.
  transactionDate: string,
  description: string | null,
  referenceNumber: string | null,
  financialTransactionId: number | bigint | null,       // For FK link number
  fiscalPeriodId: number | bigint | null,       // For FK link number
  financialOfficeId: number | bigint | null,       // For FK link number
  postedBy: string,     // Stored as string for form input, converted to number on submit.
  postedDate: string,
  reversalOfId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-general-ledger-entry-detail',
  templateUrl: './general-ledger-entry-detail.component.html',
  styleUrls: ['./general-ledger-entry-detail.component.scss']
})

export class GeneralLedgerEntryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<GeneralLedgerEntryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public generalLedgerEntryForm: FormGroup = this.fb.group({
        journalEntryNumber: ['', Validators.required],
        transactionDate: ['', Validators.required],
        description: [''],
        referenceNumber: [''],
        financialTransactionId: [null],
        fiscalPeriodId: [null],
        financialOfficeId: [null],
        postedBy: ['', Validators.required],
        postedDate: ['', Validators.required],
        reversalOfId: [''],
        active: [true],
        deleted: [false],
      });


  public generalLedgerEntryId: string | null = null;
  public generalLedgerEntryData: GeneralLedgerEntryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  generalLedgerEntries$ = this.generalLedgerEntryService.GetGeneralLedgerEntryList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public fiscalPeriods$ = this.fiscalPeriodService.GetFiscalPeriodList();
  public financialOffices$ = this.financialOfficeService.GetFinancialOfficeList();
  public generalLedgerLines$ = this.generalLedgerLineService.GetGeneralLedgerLineList();

  private destroy$ = new Subject<void>();

  constructor(
    public generalLedgerEntryService: GeneralLedgerEntryService,
    public financialTransactionService: FinancialTransactionService,
    public fiscalPeriodService: FiscalPeriodService,
    public financialOfficeService: FinancialOfficeService,
    public generalLedgerLineService: GeneralLedgerLineService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the generalLedgerEntryId from the route parameters
    this.generalLedgerEntryId = this.route.snapshot.paramMap.get('generalLedgerEntryId');

    if (this.generalLedgerEntryId === 'new' ||
        this.generalLedgerEntryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.generalLedgerEntryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.generalLedgerEntryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.generalLedgerEntryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New General Ledger Entry';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit General Ledger Entry';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.generalLedgerEntryForm.dirty) {
      return confirm('You have unsaved General Ledger Entry changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.generalLedgerEntryId != null && this.generalLedgerEntryId !== 'new') {

      const id = parseInt(this.generalLedgerEntryId, 10);

      if (!isNaN(id)) {
        return { generalLedgerEntryId: id };
      }
    }

    return null;
  }


/*
  * Loads the GeneralLedgerEntry data for the current generalLedgerEntryId.
  *
  * Fully respects the GeneralLedgerEntryService caching strategy and error handling strategy.
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
    if (!this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read GeneralLedgerEntries.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate generalLedgerEntryId
    //
    if (!this.generalLedgerEntryId) {

      this.alertService.showMessage('No GeneralLedgerEntry ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const generalLedgerEntryId = Number(this.generalLedgerEntryId);

    if (isNaN(generalLedgerEntryId) || generalLedgerEntryId <= 0) {

      this.alertService.showMessage(`Invalid General Ledger Entry ID: "${this.generalLedgerEntryId}"`,
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
      // This is the most targeted way: clear only this GeneralLedgerEntry + relations

      this.generalLedgerEntryService.ClearRecordCache(generalLedgerEntryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.generalLedgerEntryService.GetGeneralLedgerEntry(generalLedgerEntryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (generalLedgerEntryData) => {

        //
        // Success path — generalLedgerEntryData can legitimately be null if 404'd but request succeeded
        //
        if (!generalLedgerEntryData) {

          this.handleGeneralLedgerEntryNotFound(generalLedgerEntryId);

        } else {

          this.generalLedgerEntryData = generalLedgerEntryData;
          this.buildFormValues(this.generalLedgerEntryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'GeneralLedgerEntry loaded successfully',
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
        this.handleGeneralLedgerEntryLoadError(error, generalLedgerEntryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleGeneralLedgerEntryNotFound(generalLedgerEntryId: number): void {

    this.generalLedgerEntryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `GeneralLedgerEntry #${generalLedgerEntryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleGeneralLedgerEntryLoadError(error: any, generalLedgerEntryId: number): void {

    let message = 'Failed to load General Ledger Entry.';
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
          message = 'You do not have permission to view this General Ledger Entry.';
          title = 'Forbidden';
          break;
        case 404:
          message = `General Ledger Entry #${generalLedgerEntryId} was not found.`;
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

    console.error(`General Ledger Entry load failed (ID: ${generalLedgerEntryId})`, error);

    //
    // Reset UI to safe state
    //
    this.generalLedgerEntryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(generalLedgerEntryData: GeneralLedgerEntryData | null) {

    if (generalLedgerEntryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.generalLedgerEntryForm.reset({
        journalEntryNumber: '',
        transactionDate: '',
        description: '',
        referenceNumber: '',
        financialTransactionId: null,
        fiscalPeriodId: null,
        financialOfficeId: null,
        postedBy: '',
        postedDate: '',
        reversalOfId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.generalLedgerEntryForm.reset({
        journalEntryNumber: generalLedgerEntryData.journalEntryNumber?.toString() ?? '',
        transactionDate: isoUtcStringToDateTimeLocal(generalLedgerEntryData.transactionDate) ?? '',
        description: generalLedgerEntryData.description ?? '',
        referenceNumber: generalLedgerEntryData.referenceNumber ?? '',
        financialTransactionId: generalLedgerEntryData.financialTransactionId,
        fiscalPeriodId: generalLedgerEntryData.fiscalPeriodId,
        financialOfficeId: generalLedgerEntryData.financialOfficeId,
        postedBy: generalLedgerEntryData.postedBy?.toString() ?? '',
        postedDate: isoUtcStringToDateTimeLocal(generalLedgerEntryData.postedDate) ?? '',
        reversalOfId: generalLedgerEntryData.reversalOfId?.toString() ?? '',
        active: generalLedgerEntryData.active ?? true,
        deleted: generalLedgerEntryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.generalLedgerEntryForm.markAsPristine();
    this.generalLedgerEntryForm.markAsUntouched();
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

    if (this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to General Ledger Entries", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.generalLedgerEntryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.generalLedgerEntryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.generalLedgerEntryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const generalLedgerEntrySubmitData: GeneralLedgerEntrySubmitData = {
        id: this.generalLedgerEntryData?.id || 0,
        journalEntryNumber: Number(formValue.journalEntryNumber),
        transactionDate: dateTimeLocalToIsoUtc(formValue.transactionDate!.trim())!,
        description: formValue.description?.trim() || null,
        referenceNumber: formValue.referenceNumber?.trim() || null,
        financialTransactionId: formValue.financialTransactionId ? Number(formValue.financialTransactionId) : null,
        fiscalPeriodId: formValue.fiscalPeriodId ? Number(formValue.fiscalPeriodId) : null,
        financialOfficeId: formValue.financialOfficeId ? Number(formValue.financialOfficeId) : null,
        postedBy: Number(formValue.postedBy),
        postedDate: dateTimeLocalToIsoUtc(formValue.postedDate!.trim())!,
        reversalOfId: formValue.reversalOfId ? Number(formValue.reversalOfId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.generalLedgerEntryService.PutGeneralLedgerEntry(generalLedgerEntrySubmitData.id, generalLedgerEntrySubmitData)
      : this.generalLedgerEntryService.PostGeneralLedgerEntry(generalLedgerEntrySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedGeneralLedgerEntryData) => {

        this.generalLedgerEntryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created General Ledger Entry's detail page
          //
          this.generalLedgerEntryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.generalLedgerEntryForm.markAsUntouched();

          this.router.navigate(['/generalledgerentries', savedGeneralLedgerEntryData.id]);
          this.alertService.showMessage('General Ledger Entry added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.generalLedgerEntryData = savedGeneralLedgerEntryData;
          this.buildFormValues(this.generalLedgerEntryData);

          this.alertService.showMessage("General Ledger Entry saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this General Ledger Entry.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the General Ledger Entry.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('General Ledger Entry could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerGeneralLedgerEntryReader(): boolean {
    return this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryReader();
  }

  public userIsSchedulerGeneralLedgerEntryWriter(): boolean {
    return this.generalLedgerEntryService.userIsSchedulerGeneralLedgerEntryWriter();
  }
}

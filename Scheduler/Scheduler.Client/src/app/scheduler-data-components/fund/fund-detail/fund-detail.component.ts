/*
   GENERATED FORM FOR THE FUND TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Fund table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to fund-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { FundService, FundData, FundSubmitData } from '../../../scheduler-data-services/fund.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { FundChangeHistoryService } from '../../../scheduler-data-services/fund-change-history.service';
import { PledgeService } from '../../../scheduler-data-services/pledge.service';
import { BatchService } from '../../../scheduler-data-services/batch.service';
import { GiftService } from '../../../scheduler-data-services/gift.service';
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
interface FundFormValues {
  name: string,
  description: string | null,
  glCode: string | null,
  isRestricted: boolean,
  goalAmount: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-fund-detail',
  templateUrl: './fund-detail.component.html',
  styleUrls: ['./fund-detail.component.scss']
})

export class FundDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<FundFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public fundForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        glCode: [''],
        isRestricted: [false],
        goalAmount: [''],
        notes: [''],
        sequence: [''],
        iconId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public fundId: string | null = null;
  public fundData: FundData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  funds$ = this.fundService.GetFundList();
  public icons$ = this.iconService.GetIconList();
  public fundChangeHistories$ = this.fundChangeHistoryService.GetFundChangeHistoryList();
  public pledges$ = this.pledgeService.GetPledgeList();
  public batches$ = this.batchService.GetBatchList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public fundService: FundService,
    public iconService: IconService,
    public fundChangeHistoryService: FundChangeHistoryService,
    public pledgeService: PledgeService,
    public batchService: BatchService,
    public giftService: GiftService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the fundId from the route parameters
    this.fundId = this.route.snapshot.paramMap.get('fundId');

    if (this.fundId === 'new' ||
        this.fundId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.fundData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.fundForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.fundForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Fund';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Fund';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.fundForm.dirty) {
      return confirm('You have unsaved Fund changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.fundId != null && this.fundId !== 'new') {

      const id = parseInt(this.fundId, 10);

      if (!isNaN(id)) {
        return { fundId: id };
      }
    }

    return null;
  }


/*
  * Loads the Fund data for the current fundId.
  *
  * Fully respects the FundService caching strategy and error handling strategy.
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
    if (!this.fundService.userIsSchedulerFundReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Funds.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate fundId
    //
    if (!this.fundId) {

      this.alertService.showMessage('No Fund ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const fundId = Number(this.fundId);

    if (isNaN(fundId) || fundId <= 0) {

      this.alertService.showMessage(`Invalid Fund ID: "${this.fundId}"`,
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
      // This is the most targeted way: clear only this Fund + relations

      this.fundService.ClearRecordCache(fundId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.fundService.GetFund(fundId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (fundData) => {

        //
        // Success path — fundData can legitimately be null if 404'd but request succeeded
        //
        if (!fundData) {

          this.handleFundNotFound(fundId);

        } else {

          this.fundData = fundData;
          this.buildFormValues(this.fundData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Fund loaded successfully',
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
        this.handleFundLoadError(error, fundId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleFundNotFound(fundId: number): void {

    this.fundData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Fund #${fundId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleFundLoadError(error: any, fundId: number): void {

    let message = 'Failed to load Fund.';
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
          message = 'You do not have permission to view this Fund.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Fund #${fundId} was not found.`;
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

    console.error(`Fund load failed (ID: ${fundId})`, error);

    //
    // Reset UI to safe state
    //
    this.fundData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(fundData: FundData | null) {

    if (fundData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.fundForm.reset({
        name: '',
        description: '',
        glCode: '',
        isRestricted: false,
        goalAmount: '',
        notes: '',
        sequence: '',
        iconId: null,
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
        this.fundForm.reset({
        name: fundData.name ?? '',
        description: fundData.description ?? '',
        glCode: fundData.glCode ?? '',
        isRestricted: fundData.isRestricted ?? false,
        goalAmount: fundData.goalAmount?.toString() ?? '',
        notes: fundData.notes ?? '',
        sequence: fundData.sequence?.toString() ?? '',
        iconId: fundData.iconId,
        color: fundData.color ?? '',
        versionNumber: fundData.versionNumber?.toString() ?? '',
        active: fundData.active ?? true,
        deleted: fundData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.fundForm.markAsPristine();
    this.fundForm.markAsUntouched();
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

    if (this.fundService.userIsSchedulerFundWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Funds", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.fundForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.fundForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.fundForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const fundSubmitData: FundSubmitData = {
        id: this.fundData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        glCode: formValue.glCode?.trim() || null,
        isRestricted: !!formValue.isRestricted,
        goalAmount: formValue.goalAmount ? Number(formValue.goalAmount) : null,
        notes: formValue.notes?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.fundData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.fundService.PutFund(fundSubmitData.id, fundSubmitData)
      : this.fundService.PostFund(fundSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedFundData) => {

        this.fundService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Fund's detail page
          //
          this.fundForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.fundForm.markAsUntouched();

          this.router.navigate(['/funds', savedFundData.id]);
          this.alertService.showMessage('Fund added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.fundData = savedFundData;
          this.buildFormValues(this.fundData);

          this.alertService.showMessage("Fund saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Fund.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Fund.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Fund could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerFundReader(): boolean {
    return this.fundService.userIsSchedulerFundReader();
  }

  public userIsSchedulerFundWriter(): boolean {
    return this.fundService.userIsSchedulerFundWriter();
  }
}

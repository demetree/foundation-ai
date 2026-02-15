/*
   GENERATED FORM FOR THE PLEDGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Pledge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to pledge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PledgeService, PledgeData, PledgeSubmitData } from '../../../scheduler-data-services/pledge.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { RecurrenceFrequencyService } from '../../../scheduler-data-services/recurrence-frequency.service';
import { FundService } from '../../../scheduler-data-services/fund.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
import { PledgeChangeHistoryService } from '../../../scheduler-data-services/pledge-change-history.service';
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
interface PledgeFormValues {
  constituentId: number | bigint,       // For FK link number
  totalAmount: string,     // Stored as string for form input, converted to number on submit.
  balanceAmount: string,     // Stored as string for form input, converted to number on submit.
  pledgeDate: string,
  startDate: string | null,
  endDate: string | null,
  recurrenceFrequencyId: number | bigint | null,       // For FK link number
  fundId: number | bigint,       // For FK link number
  campaignId: number | bigint | null,       // For FK link number
  appealId: number | bigint | null,       // For FK link number
  writeOffAmount: string,     // Stored as string for form input, converted to number on submit.
  isWrittenOff: boolean,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-pledge-detail',
  templateUrl: './pledge-detail.component.html',
  styleUrls: ['./pledge-detail.component.scss']
})

export class PledgeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PledgeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pledgeForm: FormGroup = this.fb.group({
        constituentId: [null, Validators.required],
        totalAmount: ['', Validators.required],
        balanceAmount: ['', Validators.required],
        pledgeDate: ['', Validators.required],
        startDate: [''],
        endDate: [''],
        recurrenceFrequencyId: [null],
        fundId: [null, Validators.required],
        campaignId: [null],
        appealId: [null],
        writeOffAmount: ['', Validators.required],
        isWrittenOff: [false],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public pledgeId: string | null = null;
  public pledgeData: PledgeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  pledges$ = this.pledgeService.GetPledgeList();
  public constituents$ = this.constituentService.GetConstituentList();
  public recurrenceFrequencies$ = this.recurrenceFrequencyService.GetRecurrenceFrequencyList();
  public funds$ = this.fundService.GetFundList();
  public campaigns$ = this.campaignService.GetCampaignList();
  public appeals$ = this.appealService.GetAppealList();
  public pledgeChangeHistories$ = this.pledgeChangeHistoryService.GetPledgeChangeHistoryList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public pledgeService: PledgeService,
    public constituentService: ConstituentService,
    public recurrenceFrequencyService: RecurrenceFrequencyService,
    public fundService: FundService,
    public campaignService: CampaignService,
    public appealService: AppealService,
    public pledgeChangeHistoryService: PledgeChangeHistoryService,
    public giftService: GiftService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the pledgeId from the route parameters
    this.pledgeId = this.route.snapshot.paramMap.get('pledgeId');

    if (this.pledgeId === 'new' ||
        this.pledgeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.pledgeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.pledgeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pledgeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Pledge';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Pledge';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.pledgeForm.dirty) {
      return confirm('You have unsaved Pledge changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.pledgeId != null && this.pledgeId !== 'new') {

      const id = parseInt(this.pledgeId, 10);

      if (!isNaN(id)) {
        return { pledgeId: id };
      }
    }

    return null;
  }


/*
  * Loads the Pledge data for the current pledgeId.
  *
  * Fully respects the PledgeService caching strategy and error handling strategy.
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
    if (!this.pledgeService.userIsSchedulerPledgeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Pledges.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate pledgeId
    //
    if (!this.pledgeId) {

      this.alertService.showMessage('No Pledge ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const pledgeId = Number(this.pledgeId);

    if (isNaN(pledgeId) || pledgeId <= 0) {

      this.alertService.showMessage(`Invalid Pledge ID: "${this.pledgeId}"`,
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
      // This is the most targeted way: clear only this Pledge + relations

      this.pledgeService.ClearRecordCache(pledgeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.pledgeService.GetPledge(pledgeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (pledgeData) => {

        //
        // Success path — pledgeData can legitimately be null if 404'd but request succeeded
        //
        if (!pledgeData) {

          this.handlePledgeNotFound(pledgeId);

        } else {

          this.pledgeData = pledgeData;
          this.buildFormValues(this.pledgeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Pledge loaded successfully',
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
        this.handlePledgeLoadError(error, pledgeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePledgeNotFound(pledgeId: number): void {

    this.pledgeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Pledge #${pledgeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePledgeLoadError(error: any, pledgeId: number): void {

    let message = 'Failed to load Pledge.';
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
          message = 'You do not have permission to view this Pledge.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Pledge #${pledgeId} was not found.`;
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

    console.error(`Pledge load failed (ID: ${pledgeId})`, error);

    //
    // Reset UI to safe state
    //
    this.pledgeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(pledgeData: PledgeData | null) {

    if (pledgeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pledgeForm.reset({
        constituentId: null,
        totalAmount: '',
        balanceAmount: '',
        pledgeDate: '',
        startDate: '',
        endDate: '',
        recurrenceFrequencyId: null,
        fundId: null,
        campaignId: null,
        appealId: null,
        writeOffAmount: '',
        isWrittenOff: false,
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
        this.pledgeForm.reset({
        constituentId: pledgeData.constituentId,
        totalAmount: pledgeData.totalAmount?.toString() ?? '',
        balanceAmount: pledgeData.balanceAmount?.toString() ?? '',
        pledgeDate: pledgeData.pledgeDate ?? '',
        startDate: pledgeData.startDate ?? '',
        endDate: pledgeData.endDate ?? '',
        recurrenceFrequencyId: pledgeData.recurrenceFrequencyId,
        fundId: pledgeData.fundId,
        campaignId: pledgeData.campaignId,
        appealId: pledgeData.appealId,
        writeOffAmount: pledgeData.writeOffAmount?.toString() ?? '',
        isWrittenOff: pledgeData.isWrittenOff ?? false,
        notes: pledgeData.notes ?? '',
        versionNumber: pledgeData.versionNumber?.toString() ?? '',
        active: pledgeData.active ?? true,
        deleted: pledgeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.pledgeForm.markAsPristine();
    this.pledgeForm.markAsUntouched();
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

    if (this.pledgeService.userIsSchedulerPledgeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Pledges", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.pledgeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pledgeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pledgeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pledgeSubmitData: PledgeSubmitData = {
        id: this.pledgeData?.id || 0,
        constituentId: Number(formValue.constituentId),
        totalAmount: Number(formValue.totalAmount),
        balanceAmount: Number(formValue.balanceAmount),
        pledgeDate: formValue.pledgeDate!.trim(),
        startDate: formValue.startDate ? formValue.startDate.trim() : null,
        endDate: formValue.endDate ? formValue.endDate.trim() : null,
        recurrenceFrequencyId: formValue.recurrenceFrequencyId ? Number(formValue.recurrenceFrequencyId) : null,
        fundId: Number(formValue.fundId),
        campaignId: formValue.campaignId ? Number(formValue.campaignId) : null,
        appealId: formValue.appealId ? Number(formValue.appealId) : null,
        writeOffAmount: Number(formValue.writeOffAmount),
        isWrittenOff: !!formValue.isWrittenOff,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.pledgeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.pledgeService.PutPledge(pledgeSubmitData.id, pledgeSubmitData)
      : this.pledgeService.PostPledge(pledgeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPledgeData) => {

        this.pledgeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Pledge's detail page
          //
          this.pledgeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.pledgeForm.markAsUntouched();

          this.router.navigate(['/pledges', savedPledgeData.id]);
          this.alertService.showMessage('Pledge added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.pledgeData = savedPledgeData;
          this.buildFormValues(this.pledgeData);

          this.alertService.showMessage("Pledge saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Pledge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Pledge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Pledge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPledgeReader(): boolean {
    return this.pledgeService.userIsSchedulerPledgeReader();
  }

  public userIsSchedulerPledgeWriter(): boolean {
    return this.pledgeService.userIsSchedulerPledgeWriter();
  }
}

/*
   GENERATED FORM FOR THE CAMPAIGN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Campaign table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to campaign-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CampaignService, CampaignData, CampaignSubmitData } from '../../../scheduler-data-services/campaign.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { CampaignChangeHistoryService } from '../../../scheduler-data-services/campaign-change-history.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
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
interface CampaignFormValues {
  name: string,
  description: string | null,
  startDate: string | null,
  endDate: string | null,
  fundRaisingGoal: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-campaign-detail',
  templateUrl: './campaign-detail.component.html',
  styleUrls: ['./campaign-detail.component.scss']
})

export class CampaignDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CampaignFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public campaignForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        startDate: [''],
        endDate: [''],
        fundRaisingGoal: [''],
        notes: [''],
        iconId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public campaignId: string | null = null;
  public campaignData: CampaignData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  campaigns$ = this.campaignService.GetCampaignList();
  public icons$ = this.iconService.GetIconList();
  public campaignChangeHistories$ = this.campaignChangeHistoryService.GetCampaignChangeHistoryList();
  public appeals$ = this.appealService.GetAppealList();
  public pledges$ = this.pledgeService.GetPledgeList();
  public batches$ = this.batchService.GetBatchList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public campaignService: CampaignService,
    public iconService: IconService,
    public campaignChangeHistoryService: CampaignChangeHistoryService,
    public appealService: AppealService,
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

    // Get the campaignId from the route parameters
    this.campaignId = this.route.snapshot.paramMap.get('campaignId');

    if (this.campaignId === 'new' ||
        this.campaignId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.campaignData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.campaignForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.campaignForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Campaign';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Campaign';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.campaignForm.dirty) {
      return confirm('You have unsaved Campaign changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.campaignId != null && this.campaignId !== 'new') {

      const id = parseInt(this.campaignId, 10);

      if (!isNaN(id)) {
        return { campaignId: id };
      }
    }

    return null;
  }


/*
  * Loads the Campaign data for the current campaignId.
  *
  * Fully respects the CampaignService caching strategy and error handling strategy.
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
    if (!this.campaignService.userIsSchedulerCampaignReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Campaigns.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate campaignId
    //
    if (!this.campaignId) {

      this.alertService.showMessage('No Campaign ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const campaignId = Number(this.campaignId);

    if (isNaN(campaignId) || campaignId <= 0) {

      this.alertService.showMessage(`Invalid Campaign ID: "${this.campaignId}"`,
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
      // This is the most targeted way: clear only this Campaign + relations

      this.campaignService.ClearRecordCache(campaignId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.campaignService.GetCampaign(campaignId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (campaignData) => {

        //
        // Success path — campaignData can legitimately be null if 404'd but request succeeded
        //
        if (!campaignData) {

          this.handleCampaignNotFound(campaignId);

        } else {

          this.campaignData = campaignData;
          this.buildFormValues(this.campaignData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Campaign loaded successfully',
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
        this.handleCampaignLoadError(error, campaignId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCampaignNotFound(campaignId: number): void {

    this.campaignData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Campaign #${campaignId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCampaignLoadError(error: any, campaignId: number): void {

    let message = 'Failed to load Campaign.';
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
          message = 'You do not have permission to view this Campaign.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Campaign #${campaignId} was not found.`;
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

    console.error(`Campaign load failed (ID: ${campaignId})`, error);

    //
    // Reset UI to safe state
    //
    this.campaignData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(campaignData: CampaignData | null) {

    if (campaignData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.campaignForm.reset({
        name: '',
        description: '',
        startDate: '',
        endDate: '',
        fundRaisingGoal: '',
        notes: '',
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
        this.campaignForm.reset({
        name: campaignData.name ?? '',
        description: campaignData.description ?? '',
        startDate: campaignData.startDate ?? '',
        endDate: campaignData.endDate ?? '',
        fundRaisingGoal: campaignData.fundRaisingGoal?.toString() ?? '',
        notes: campaignData.notes ?? '',
        iconId: campaignData.iconId,
        color: campaignData.color ?? '',
        versionNumber: campaignData.versionNumber?.toString() ?? '',
        active: campaignData.active ?? true,
        deleted: campaignData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.campaignForm.markAsPristine();
    this.campaignForm.markAsUntouched();
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

    if (this.campaignService.userIsSchedulerCampaignWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Campaigns", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.campaignForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.campaignForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.campaignForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const campaignSubmitData: CampaignSubmitData = {
        id: this.campaignData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        startDate: formValue.startDate ? formValue.startDate.trim() : null,
        endDate: formValue.endDate ? formValue.endDate.trim() : null,
        fundRaisingGoal: formValue.fundRaisingGoal ? Number(formValue.fundRaisingGoal) : null,
        notes: formValue.notes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.campaignData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.campaignService.PutCampaign(campaignSubmitData.id, campaignSubmitData)
      : this.campaignService.PostCampaign(campaignSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCampaignData) => {

        this.campaignService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Campaign's detail page
          //
          this.campaignForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.campaignForm.markAsUntouched();

          this.router.navigate(['/campaigns', savedCampaignData.id]);
          this.alertService.showMessage('Campaign added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.campaignData = savedCampaignData;
          this.buildFormValues(this.campaignData);

          this.alertService.showMessage("Campaign saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Campaign.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Campaign.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Campaign could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerCampaignReader(): boolean {
    return this.campaignService.userIsSchedulerCampaignReader();
  }

  public userIsSchedulerCampaignWriter(): boolean {
    return this.campaignService.userIsSchedulerCampaignWriter();
  }
}

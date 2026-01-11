/*
   GENERATED FORM FOR THE APPEAL TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Appeal table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to appeal-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AppealService, AppealData, AppealSubmitData } from '../../../scheduler-data-services/appeal.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AppealChangeHistoryService } from '../../../scheduler-data-services/appeal-change-history.service';
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
interface AppealFormValues {
  campaignId: number | bigint | null,       // For FK link number
  name: string,
  description: string | null,
  costPerUnit: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-appeal-detail',
  templateUrl: './appeal-detail.component.html',
  styleUrls: ['./appeal-detail.component.scss']
})

export class AppealDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AppealFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public appealForm: FormGroup = this.fb.group({
        campaignId: [null],
        name: ['', Validators.required],
        description: [''],
        costPerUnit: [''],
        notes: [''],
        iconId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public appealId: string | null = null;
  public appealData: AppealData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  appeals$ = this.appealService.GetAppealList();
  public campaigns$ = this.campaignService.GetCampaignList();
  public icons$ = this.iconService.GetIconList();
  public appealChangeHistories$ = this.appealChangeHistoryService.GetAppealChangeHistoryList();
  public pledges$ = this.pledgeService.GetPledgeList();
  public batches$ = this.batchService.GetBatchList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public appealService: AppealService,
    public campaignService: CampaignService,
    public iconService: IconService,
    public appealChangeHistoryService: AppealChangeHistoryService,
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

    // Get the appealId from the route parameters
    this.appealId = this.route.snapshot.paramMap.get('appealId');

    if (this.appealId === 'new' ||
        this.appealId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.appealData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.appealForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.appealForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Appeal';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Appeal';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.appealForm.dirty) {
      return confirm('You have unsaved Appeal changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.appealId != null && this.appealId !== 'new') {

      const id = parseInt(this.appealId, 10);

      if (!isNaN(id)) {
        return { appealId: id };
      }
    }

    return null;
  }


/*
  * Loads the Appeal data for the current appealId.
  *
  * Fully respects the AppealService caching strategy and error handling strategy.
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
    if (!this.appealService.userIsSchedulerAppealReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Appeals.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate appealId
    //
    if (!this.appealId) {

      this.alertService.showMessage('No Appeal ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const appealId = Number(this.appealId);

    if (isNaN(appealId) || appealId <= 0) {

      this.alertService.showMessage(`Invalid Appeal ID: "${this.appealId}"`,
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
      // This is the most targeted way: clear only this Appeal + relations

      this.appealService.ClearRecordCache(appealId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.appealService.GetAppeal(appealId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (appealData) => {

        //
        // Success path — appealData can legitimately be null if 404'd but request succeeded
        //
        if (!appealData) {

          this.handleAppealNotFound(appealId);

        } else {

          this.appealData = appealData;
          this.buildFormValues(this.appealData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Appeal loaded successfully',
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
        this.handleAppealLoadError(error, appealId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAppealNotFound(appealId: number): void {

    this.appealData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Appeal #${appealId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAppealLoadError(error: any, appealId: number): void {

    let message = 'Failed to load Appeal.';
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
          message = 'You do not have permission to view this Appeal.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Appeal #${appealId} was not found.`;
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

    console.error(`Appeal load failed (ID: ${appealId})`, error);

    //
    // Reset UI to safe state
    //
    this.appealData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(appealData: AppealData | null) {

    if (appealData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.appealForm.reset({
        campaignId: null,
        name: '',
        description: '',
        costPerUnit: '',
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
        this.appealForm.reset({
        campaignId: appealData.campaignId,
        name: appealData.name ?? '',
        description: appealData.description ?? '',
        costPerUnit: appealData.costPerUnit?.toString() ?? '',
        notes: appealData.notes ?? '',
        iconId: appealData.iconId,
        color: appealData.color ?? '',
        versionNumber: appealData.versionNumber?.toString() ?? '',
        active: appealData.active ?? true,
        deleted: appealData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.appealForm.markAsPristine();
    this.appealForm.markAsUntouched();
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

    if (this.appealService.userIsSchedulerAppealWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Appeals", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.appealForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.appealForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.appealForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const appealSubmitData: AppealSubmitData = {
        id: this.appealData?.id || 0,
        campaignId: formValue.campaignId ? Number(formValue.campaignId) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        costPerUnit: formValue.costPerUnit ? Number(formValue.costPerUnit) : null,
        notes: formValue.notes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.appealData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.appealService.PutAppeal(appealSubmitData.id, appealSubmitData)
      : this.appealService.PostAppeal(appealSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAppealData) => {

        this.appealService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Appeal's detail page
          //
          this.appealForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.appealForm.markAsUntouched();

          this.router.navigate(['/appeals', savedAppealData.id]);
          this.alertService.showMessage('Appeal added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.appealData = savedAppealData;
          this.buildFormValues(this.appealData);

          this.alertService.showMessage("Appeal saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Appeal.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Appeal.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Appeal could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerAppealReader(): boolean {
    return this.appealService.userIsSchedulerAppealReader();
  }

  public userIsSchedulerAppealWriter(): boolean {
    return this.appealService.userIsSchedulerAppealWriter();
  }
}

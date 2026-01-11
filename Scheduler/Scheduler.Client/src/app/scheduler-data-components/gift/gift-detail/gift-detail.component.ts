/*
   GENERATED FORM FOR THE GIFT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Gift table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to gift-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { GiftService, GiftData, GiftSubmitData } from '../../../scheduler-data-services/gift.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { PledgeService } from '../../../scheduler-data-services/pledge.service';
import { FundService } from '../../../scheduler-data-services/fund.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
import { PaymentTypeService } from '../../../scheduler-data-services/payment-type.service';
import { BatchService } from '../../../scheduler-data-services/batch.service';
import { ReceiptTypeService } from '../../../scheduler-data-services/receipt-type.service';
import { TributeService } from '../../../scheduler-data-services/tribute.service';
import { GiftChangeHistoryService } from '../../../scheduler-data-services/gift-change-history.service';
import { SoftCreditService } from '../../../scheduler-data-services/soft-credit.service';
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
interface GiftFormValues {
  officeId: number | bigint | null,       // For FK link number
  constituentId: number | bigint,       // For FK link number
  pledgeId: number | bigint | null,       // For FK link number
  amount: string,     // Stored as string for form input, converted to number on submit.
  receivedDate: string,
  postedDate: string | null,
  fundId: number | bigint,       // For FK link number
  campaignId: number | bigint | null,       // For FK link number
  appealId: number | bigint | null,       // For FK link number
  paymentTypeId: number | bigint,       // For FK link number
  referenceNumber: string | null,
  batchId: number | bigint | null,       // For FK link number
  receiptTypeId: number | bigint | null,       // For FK link number
  receiptDate: string | null,
  tributeId: number | bigint | null,       // For FK link number
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-gift-detail',
  templateUrl: './gift-detail.component.html',
  styleUrls: ['./gift-detail.component.scss']
})

export class GiftDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<GiftFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public giftForm: FormGroup = this.fb.group({
        officeId: [null],
        constituentId: [null, Validators.required],
        pledgeId: [null],
        amount: ['', Validators.required],
        receivedDate: ['', Validators.required],
        postedDate: [''],
        fundId: [null, Validators.required],
        campaignId: [null],
        appealId: [null],
        paymentTypeId: [null, Validators.required],
        referenceNumber: [''],
        batchId: [null],
        receiptTypeId: [null],
        receiptDate: [''],
        tributeId: [null],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public giftId: string | null = null;
  public giftData: GiftData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  gifts$ = this.giftService.GetGiftList();
  public offices$ = this.officeService.GetOfficeList();
  public constituents$ = this.constituentService.GetConstituentList();
  public pledges$ = this.pledgeService.GetPledgeList();
  public funds$ = this.fundService.GetFundList();
  public campaigns$ = this.campaignService.GetCampaignList();
  public appeals$ = this.appealService.GetAppealList();
  public paymentTypes$ = this.paymentTypeService.GetPaymentTypeList();
  public batches$ = this.batchService.GetBatchList();
  public receiptTypes$ = this.receiptTypeService.GetReceiptTypeList();
  public tributes$ = this.tributeService.GetTributeList();
  public giftChangeHistories$ = this.giftChangeHistoryService.GetGiftChangeHistoryList();
  public softCredits$ = this.softCreditService.GetSoftCreditList();

  private destroy$ = new Subject<void>();

  constructor(
    public giftService: GiftService,
    public officeService: OfficeService,
    public constituentService: ConstituentService,
    public pledgeService: PledgeService,
    public fundService: FundService,
    public campaignService: CampaignService,
    public appealService: AppealService,
    public paymentTypeService: PaymentTypeService,
    public batchService: BatchService,
    public receiptTypeService: ReceiptTypeService,
    public tributeService: TributeService,
    public giftChangeHistoryService: GiftChangeHistoryService,
    public softCreditService: SoftCreditService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the giftId from the route parameters
    this.giftId = this.route.snapshot.paramMap.get('giftId');

    if (this.giftId === 'new' ||
        this.giftId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.giftData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.giftForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.giftForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Gift';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Gift';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.giftForm.dirty) {
      return confirm('You have unsaved Gift changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.giftId != null && this.giftId !== 'new') {

      const id = parseInt(this.giftId, 10);

      if (!isNaN(id)) {
        return { giftId: id };
      }
    }

    return null;
  }


/*
  * Loads the Gift data for the current giftId.
  *
  * Fully respects the GiftService caching strategy and error handling strategy.
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
    if (!this.giftService.userIsSchedulerGiftReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Gifts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate giftId
    //
    if (!this.giftId) {

      this.alertService.showMessage('No Gift ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const giftId = Number(this.giftId);

    if (isNaN(giftId) || giftId <= 0) {

      this.alertService.showMessage(`Invalid Gift ID: "${this.giftId}"`,
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
      // This is the most targeted way: clear only this Gift + relations

      this.giftService.ClearRecordCache(giftId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.giftService.GetGift(giftId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (giftData) => {

        //
        // Success path — giftData can legitimately be null if 404'd but request succeeded
        //
        if (!giftData) {

          this.handleGiftNotFound(giftId);

        } else {

          this.giftData = giftData;
          this.buildFormValues(this.giftData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Gift loaded successfully',
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
        this.handleGiftLoadError(error, giftId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleGiftNotFound(giftId: number): void {

    this.giftData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Gift #${giftId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleGiftLoadError(error: any, giftId: number): void {

    let message = 'Failed to load Gift.';
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
          message = 'You do not have permission to view this Gift.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Gift #${giftId} was not found.`;
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

    console.error(`Gift load failed (ID: ${giftId})`, error);

    //
    // Reset UI to safe state
    //
    this.giftData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(giftData: GiftData | null) {

    if (giftData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.giftForm.reset({
        officeId: null,
        constituentId: null,
        pledgeId: null,
        amount: '',
        receivedDate: '',
        postedDate: '',
        fundId: null,
        campaignId: null,
        appealId: null,
        paymentTypeId: null,
        referenceNumber: '',
        batchId: null,
        receiptTypeId: null,
        receiptDate: '',
        tributeId: null,
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
        this.giftForm.reset({
        officeId: giftData.officeId,
        constituentId: giftData.constituentId,
        pledgeId: giftData.pledgeId,
        amount: giftData.amount?.toString() ?? '',
        receivedDate: isoUtcStringToDateTimeLocal(giftData.receivedDate) ?? '',
        postedDate: isoUtcStringToDateTimeLocal(giftData.postedDate) ?? '',
        fundId: giftData.fundId,
        campaignId: giftData.campaignId,
        appealId: giftData.appealId,
        paymentTypeId: giftData.paymentTypeId,
        referenceNumber: giftData.referenceNumber ?? '',
        batchId: giftData.batchId,
        receiptTypeId: giftData.receiptTypeId,
        receiptDate: isoUtcStringToDateTimeLocal(giftData.receiptDate) ?? '',
        tributeId: giftData.tributeId,
        notes: giftData.notes ?? '',
        versionNumber: giftData.versionNumber?.toString() ?? '',
        active: giftData.active ?? true,
        deleted: giftData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.giftForm.markAsPristine();
    this.giftForm.markAsUntouched();
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

    if (this.giftService.userIsSchedulerGiftWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Gifts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.giftForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.giftForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.giftForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const giftSubmitData: GiftSubmitData = {
        id: this.giftData?.id || 0,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        constituentId: Number(formValue.constituentId),
        pledgeId: formValue.pledgeId ? Number(formValue.pledgeId) : null,
        amount: Number(formValue.amount),
        receivedDate: dateTimeLocalToIsoUtc(formValue.receivedDate!.trim())!,
        postedDate: formValue.postedDate ? dateTimeLocalToIsoUtc(formValue.postedDate.trim()) : null,
        fundId: Number(formValue.fundId),
        campaignId: formValue.campaignId ? Number(formValue.campaignId) : null,
        appealId: formValue.appealId ? Number(formValue.appealId) : null,
        paymentTypeId: Number(formValue.paymentTypeId),
        referenceNumber: formValue.referenceNumber?.trim() || null,
        batchId: formValue.batchId ? Number(formValue.batchId) : null,
        receiptTypeId: formValue.receiptTypeId ? Number(formValue.receiptTypeId) : null,
        receiptDate: formValue.receiptDate ? dateTimeLocalToIsoUtc(formValue.receiptDate.trim()) : null,
        tributeId: formValue.tributeId ? Number(formValue.tributeId) : null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.giftData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.giftService.PutGift(giftSubmitData.id, giftSubmitData)
      : this.giftService.PostGift(giftSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedGiftData) => {

        this.giftService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Gift's detail page
          //
          this.giftForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.giftForm.markAsUntouched();

          this.router.navigate(['/gifts', savedGiftData.id]);
          this.alertService.showMessage('Gift added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.giftData = savedGiftData;
          this.buildFormValues(this.giftData);

          this.alertService.showMessage("Gift saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Gift.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Gift.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Gift could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerGiftReader(): boolean {
    return this.giftService.userIsSchedulerGiftReader();
  }

  public userIsSchedulerGiftWriter(): boolean {
    return this.giftService.userIsSchedulerGiftWriter();
  }
}

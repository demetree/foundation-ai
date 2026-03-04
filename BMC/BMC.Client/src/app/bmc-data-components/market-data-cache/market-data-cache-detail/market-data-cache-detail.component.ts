/*
   GENERATED FORM FOR THE MARKETDATACACHE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MarketDataCache table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to market-data-cache-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MarketDataCacheService, MarketDataCacheData, MarketDataCacheSubmitData } from '../../../bmc-data-services/market-data-cache.service';
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
interface MarketDataCacheFormValues {
  source: string,
  itemType: string,
  itemNumber: string,
  condition: string | null,
  responseJson: string | null,
  fetchedDate: string,
  expiresDate: string,
  ttlMinutes: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-market-data-cache-detail',
  templateUrl: './market-data-cache-detail.component.html',
  styleUrls: ['./market-data-cache-detail.component.scss']
})

export class MarketDataCacheDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MarketDataCacheFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public marketDataCacheForm: FormGroup = this.fb.group({
        source: ['', Validators.required],
        itemType: ['', Validators.required],
        itemNumber: ['', Validators.required],
        condition: [''],
        responseJson: [''],
        fetchedDate: ['', Validators.required],
        expiresDate: ['', Validators.required],
        ttlMinutes: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public marketDataCacheId: string | null = null;
  public marketDataCacheData: MarketDataCacheData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  marketDataCaches$ = this.marketDataCacheService.GetMarketDataCacheList();

  private destroy$ = new Subject<void>();

  constructor(
    public marketDataCacheService: MarketDataCacheService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the marketDataCacheId from the route parameters
    this.marketDataCacheId = this.route.snapshot.paramMap.get('marketDataCacheId');

    if (this.marketDataCacheId === 'new' ||
        this.marketDataCacheId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.marketDataCacheData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.marketDataCacheForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.marketDataCacheForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Market Data Cache';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Market Data Cache';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.marketDataCacheForm.dirty) {
      return confirm('You have unsaved Market Data Cache changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.marketDataCacheId != null && this.marketDataCacheId !== 'new') {

      const id = parseInt(this.marketDataCacheId, 10);

      if (!isNaN(id)) {
        return { marketDataCacheId: id };
      }
    }

    return null;
  }


/*
  * Loads the MarketDataCache data for the current marketDataCacheId.
  *
  * Fully respects the MarketDataCacheService caching strategy and error handling strategy.
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
    if (!this.marketDataCacheService.userIsBMCMarketDataCacheReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MarketDataCaches.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate marketDataCacheId
    //
    if (!this.marketDataCacheId) {

      this.alertService.showMessage('No MarketDataCache ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const marketDataCacheId = Number(this.marketDataCacheId);

    if (isNaN(marketDataCacheId) || marketDataCacheId <= 0) {

      this.alertService.showMessage(`Invalid Market Data Cache ID: "${this.marketDataCacheId}"`,
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
      // This is the most targeted way: clear only this MarketDataCache + relations

      this.marketDataCacheService.ClearRecordCache(marketDataCacheId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.marketDataCacheService.GetMarketDataCache(marketDataCacheId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (marketDataCacheData) => {

        //
        // Success path — marketDataCacheData can legitimately be null if 404'd but request succeeded
        //
        if (!marketDataCacheData) {

          this.handleMarketDataCacheNotFound(marketDataCacheId);

        } else {

          this.marketDataCacheData = marketDataCacheData;
          this.buildFormValues(this.marketDataCacheData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MarketDataCache loaded successfully',
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
        this.handleMarketDataCacheLoadError(error, marketDataCacheId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMarketDataCacheNotFound(marketDataCacheId: number): void {

    this.marketDataCacheData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MarketDataCache #${marketDataCacheId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMarketDataCacheLoadError(error: any, marketDataCacheId: number): void {

    let message = 'Failed to load Market Data Cache.';
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
          message = 'You do not have permission to view this Market Data Cache.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Market Data Cache #${marketDataCacheId} was not found.`;
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

    console.error(`Market Data Cache load failed (ID: ${marketDataCacheId})`, error);

    //
    // Reset UI to safe state
    //
    this.marketDataCacheData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(marketDataCacheData: MarketDataCacheData | null) {

    if (marketDataCacheData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.marketDataCacheForm.reset({
        source: '',
        itemType: '',
        itemNumber: '',
        condition: '',
        responseJson: '',
        fetchedDate: '',
        expiresDate: '',
        ttlMinutes: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.marketDataCacheForm.reset({
        source: marketDataCacheData.source ?? '',
        itemType: marketDataCacheData.itemType ?? '',
        itemNumber: marketDataCacheData.itemNumber ?? '',
        condition: marketDataCacheData.condition ?? '',
        responseJson: marketDataCacheData.responseJson ?? '',
        fetchedDate: isoUtcStringToDateTimeLocal(marketDataCacheData.fetchedDate) ?? '',
        expiresDate: isoUtcStringToDateTimeLocal(marketDataCacheData.expiresDate) ?? '',
        ttlMinutes: marketDataCacheData.ttlMinutes?.toString() ?? '',
        active: marketDataCacheData.active ?? true,
        deleted: marketDataCacheData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.marketDataCacheForm.markAsPristine();
    this.marketDataCacheForm.markAsUntouched();
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

    if (this.marketDataCacheService.userIsBMCMarketDataCacheWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Market Data Caches", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.marketDataCacheForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.marketDataCacheForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.marketDataCacheForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const marketDataCacheSubmitData: MarketDataCacheSubmitData = {
        id: this.marketDataCacheData?.id || 0,
        source: formValue.source!.trim(),
        itemType: formValue.itemType!.trim(),
        itemNumber: formValue.itemNumber!.trim(),
        condition: formValue.condition?.trim() || null,
        responseJson: formValue.responseJson?.trim() || null,
        fetchedDate: dateTimeLocalToIsoUtc(formValue.fetchedDate!.trim())!,
        expiresDate: dateTimeLocalToIsoUtc(formValue.expiresDate!.trim())!,
        ttlMinutes: Number(formValue.ttlMinutes),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.marketDataCacheService.PutMarketDataCache(marketDataCacheSubmitData.id, marketDataCacheSubmitData)
      : this.marketDataCacheService.PostMarketDataCache(marketDataCacheSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMarketDataCacheData) => {

        this.marketDataCacheService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Market Data Cache's detail page
          //
          this.marketDataCacheForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.marketDataCacheForm.markAsUntouched();

          this.router.navigate(['/marketdatacaches', savedMarketDataCacheData.id]);
          this.alertService.showMessage('Market Data Cache added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.marketDataCacheData = savedMarketDataCacheData;
          this.buildFormValues(this.marketDataCacheData);

          this.alertService.showMessage("Market Data Cache saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Market Data Cache.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Market Data Cache.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Market Data Cache could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCMarketDataCacheReader(): boolean {
    return this.marketDataCacheService.userIsBMCMarketDataCacheReader();
  }

  public userIsBMCMarketDataCacheWriter(): boolean {
    return this.marketDataCacheService.userIsBMCMarketDataCacheWriter();
  }
}

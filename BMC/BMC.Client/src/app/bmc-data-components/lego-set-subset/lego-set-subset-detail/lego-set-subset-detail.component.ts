/*
   GENERATED FORM FOR THE LEGOSETSUBSET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoSetSubset table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-set-subset-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoSetSubsetService, LegoSetSubsetData, LegoSetSubsetSubmitData } from '../../../bmc-data-services/lego-set-subset.service';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
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
interface LegoSetSubsetFormValues {
  parentLegoSetId: number | bigint,       // For FK link number
  childLegoSetId: number | bigint,       // For FK link number
  quantity: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-lego-set-subset-detail',
  templateUrl: './lego-set-subset-detail.component.html',
  styleUrls: ['./lego-set-subset-detail.component.scss']
})

export class LegoSetSubsetDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoSetSubsetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoSetSubsetForm: FormGroup = this.fb.group({
        parentLegoSetId: [null, Validators.required],
        childLegoSetId: [null, Validators.required],
        quantity: [''],
        active: [true],
        deleted: [false],
      });


  public legoSetSubsetId: string | null = null;
  public legoSetSubsetData: LegoSetSubsetData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  legoSetSubsets$ = this.legoSetSubsetService.GetLegoSetSubsetList();
  public legoSets$ = this.legoSetService.GetLegoSetList();

  private destroy$ = new Subject<void>();

  constructor(
    public legoSetSubsetService: LegoSetSubsetService,
    public legoSetService: LegoSetService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the legoSetSubsetId from the route parameters
    this.legoSetSubsetId = this.route.snapshot.paramMap.get('legoSetSubsetId');

    if (this.legoSetSubsetId === 'new' ||
        this.legoSetSubsetId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.legoSetSubsetData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.legoSetSubsetForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoSetSubsetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Lego Set Subset';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Lego Set Subset';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.legoSetSubsetForm.dirty) {
      return confirm('You have unsaved Lego Set Subset changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.legoSetSubsetId != null && this.legoSetSubsetId !== 'new') {

      const id = parseInt(this.legoSetSubsetId, 10);

      if (!isNaN(id)) {
        return { legoSetSubsetId: id };
      }
    }

    return null;
  }


/*
  * Loads the LegoSetSubset data for the current legoSetSubsetId.
  *
  * Fully respects the LegoSetSubsetService caching strategy and error handling strategy.
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
    if (!this.legoSetSubsetService.userIsBMCLegoSetSubsetReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read LegoSetSubsets.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate legoSetSubsetId
    //
    if (!this.legoSetSubsetId) {

      this.alertService.showMessage('No LegoSetSubset ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const legoSetSubsetId = Number(this.legoSetSubsetId);

    if (isNaN(legoSetSubsetId) || legoSetSubsetId <= 0) {

      this.alertService.showMessage(`Invalid Lego Set Subset ID: "${this.legoSetSubsetId}"`,
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
      // This is the most targeted way: clear only this LegoSetSubset + relations

      this.legoSetSubsetService.ClearRecordCache(legoSetSubsetId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.legoSetSubsetService.GetLegoSetSubset(legoSetSubsetId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (legoSetSubsetData) => {

        //
        // Success path — legoSetSubsetData can legitimately be null if 404'd but request succeeded
        //
        if (!legoSetSubsetData) {

          this.handleLegoSetSubsetNotFound(legoSetSubsetId);

        } else {

          this.legoSetSubsetData = legoSetSubsetData;
          this.buildFormValues(this.legoSetSubsetData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'LegoSetSubset loaded successfully',
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
        this.handleLegoSetSubsetLoadError(error, legoSetSubsetId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleLegoSetSubsetNotFound(legoSetSubsetId: number): void {

    this.legoSetSubsetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `LegoSetSubset #${legoSetSubsetId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleLegoSetSubsetLoadError(error: any, legoSetSubsetId: number): void {

    let message = 'Failed to load Lego Set Subset.';
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
          message = 'You do not have permission to view this Lego Set Subset.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Lego Set Subset #${legoSetSubsetId} was not found.`;
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

    console.error(`Lego Set Subset load failed (ID: ${legoSetSubsetId})`, error);

    //
    // Reset UI to safe state
    //
    this.legoSetSubsetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(legoSetSubsetData: LegoSetSubsetData | null) {

    if (legoSetSubsetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoSetSubsetForm.reset({
        parentLegoSetId: null,
        childLegoSetId: null,
        quantity: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoSetSubsetForm.reset({
        parentLegoSetId: legoSetSubsetData.parentLegoSetId,
        childLegoSetId: legoSetSubsetData.childLegoSetId,
        quantity: legoSetSubsetData.quantity?.toString() ?? '',
        active: legoSetSubsetData.active ?? true,
        deleted: legoSetSubsetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoSetSubsetForm.markAsPristine();
    this.legoSetSubsetForm.markAsUntouched();
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

    if (this.legoSetSubsetService.userIsBMCLegoSetSubsetWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Lego Set Subsets", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.legoSetSubsetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoSetSubsetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoSetSubsetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoSetSubsetSubmitData: LegoSetSubsetSubmitData = {
        id: this.legoSetSubsetData?.id || 0,
        parentLegoSetId: Number(formValue.parentLegoSetId),
        childLegoSetId: Number(formValue.childLegoSetId),
        quantity: formValue.quantity ? Number(formValue.quantity) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.legoSetSubsetService.PutLegoSetSubset(legoSetSubsetSubmitData.id, legoSetSubsetSubmitData)
      : this.legoSetSubsetService.PostLegoSetSubset(legoSetSubsetSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedLegoSetSubsetData) => {

        this.legoSetSubsetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Lego Set Subset's detail page
          //
          this.legoSetSubsetForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.legoSetSubsetForm.markAsUntouched();

          this.router.navigate(['/legosetsubsets', savedLegoSetSubsetData.id]);
          this.alertService.showMessage('Lego Set Subset added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.legoSetSubsetData = savedLegoSetSubsetData;
          this.buildFormValues(this.legoSetSubsetData);

          this.alertService.showMessage("Lego Set Subset saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Lego Set Subset.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set Subset.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set Subset could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCLegoSetSubsetReader(): boolean {
    return this.legoSetSubsetService.userIsBMCLegoSetSubsetReader();
  }

  public userIsBMCLegoSetSubsetWriter(): boolean {
    return this.legoSetSubsetService.userIsBMCLegoSetSubsetWriter();
  }
}

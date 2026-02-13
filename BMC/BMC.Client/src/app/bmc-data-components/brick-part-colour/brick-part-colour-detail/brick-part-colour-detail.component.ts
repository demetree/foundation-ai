/*
   GENERATED FORM FOR THE BRICKPARTCOLOUR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPartColour table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-colour-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickPartColourService, BrickPartColourData, BrickPartColourSubmitData } from '../../../bmc-data-services/brick-part-colour.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
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
interface BrickPartColourFormValues {
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-part-colour-detail',
  templateUrl: './brick-part-colour-detail.component.html',
  styleUrls: ['./brick-part-colour-detail.component.scss']
})

export class BrickPartColourDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickPartColourFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickPartColourForm: FormGroup = this.fb.group({
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });


  public brickPartColourId: string | null = null;
  public brickPartColourData: BrickPartColourData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickPartColours$ = this.brickPartColourService.GetBrickPartColourList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickPartColourService: BrickPartColourService,
    public brickPartService: BrickPartService,
    public brickColourService: BrickColourService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickPartColourId from the route parameters
    this.brickPartColourId = this.route.snapshot.paramMap.get('brickPartColourId');

    if (this.brickPartColourId === 'new' ||
        this.brickPartColourId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickPartColourData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickPartColourForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickPartColourForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Part Colour';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Part Colour';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickPartColourForm.dirty) {
      return confirm('You have unsaved Brick Part Colour changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickPartColourId != null && this.brickPartColourId !== 'new') {

      const id = parseInt(this.brickPartColourId, 10);

      if (!isNaN(id)) {
        return { brickPartColourId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickPartColour data for the current brickPartColourId.
  *
  * Fully respects the BrickPartColourService caching strategy and error handling strategy.
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
    if (!this.brickPartColourService.userIsBMCBrickPartColourReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickPartColours.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickPartColourId
    //
    if (!this.brickPartColourId) {

      this.alertService.showMessage('No BrickPartColour ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickPartColourId = Number(this.brickPartColourId);

    if (isNaN(brickPartColourId) || brickPartColourId <= 0) {

      this.alertService.showMessage(`Invalid Brick Part Colour ID: "${this.brickPartColourId}"`,
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
      // This is the most targeted way: clear only this BrickPartColour + relations

      this.brickPartColourService.ClearRecordCache(brickPartColourId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickPartColourService.GetBrickPartColour(brickPartColourId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickPartColourData) => {

        //
        // Success path — brickPartColourData can legitimately be null if 404'd but request succeeded
        //
        if (!brickPartColourData) {

          this.handleBrickPartColourNotFound(brickPartColourId);

        } else {

          this.brickPartColourData = brickPartColourData;
          this.buildFormValues(this.brickPartColourData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickPartColour loaded successfully',
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
        this.handleBrickPartColourLoadError(error, brickPartColourId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickPartColourNotFound(brickPartColourId: number): void {

    this.brickPartColourData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickPartColour #${brickPartColourId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickPartColourLoadError(error: any, brickPartColourId: number): void {

    let message = 'Failed to load Brick Part Colour.';
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
          message = 'You do not have permission to view this Brick Part Colour.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Part Colour #${brickPartColourId} was not found.`;
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

    console.error(`Brick Part Colour load failed (ID: ${brickPartColourId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickPartColourData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickPartColourData: BrickPartColourData | null) {

    if (brickPartColourData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickPartColourForm.reset({
        brickPartId: null,
        brickColourId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickPartColourForm.reset({
        brickPartId: brickPartColourData.brickPartId,
        brickColourId: brickPartColourData.brickColourId,
        active: brickPartColourData.active ?? true,
        deleted: brickPartColourData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickPartColourForm.markAsPristine();
    this.brickPartColourForm.markAsUntouched();
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

    if (this.brickPartColourService.userIsBMCBrickPartColourWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Part Colours", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickPartColourForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickPartColourForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickPartColourForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickPartColourSubmitData: BrickPartColourSubmitData = {
        id: this.brickPartColourData?.id || 0,
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickPartColourService.PutBrickPartColour(brickPartColourSubmitData.id, brickPartColourSubmitData)
      : this.brickPartColourService.PostBrickPartColour(brickPartColourSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickPartColourData) => {

        this.brickPartColourService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Part Colour's detail page
          //
          this.brickPartColourForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickPartColourForm.markAsUntouched();

          this.router.navigate(['/brickpartcolours', savedBrickPartColourData.id]);
          this.alertService.showMessage('Brick Part Colour added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickPartColourData = savedBrickPartColourData;
          this.buildFormValues(this.brickPartColourData);

          this.alertService.showMessage("Brick Part Colour saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Part Colour.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part Colour.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part Colour could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickPartColourReader(): boolean {
    return this.brickPartColourService.userIsBMCBrickPartColourReader();
  }

  public userIsBMCBrickPartColourWriter(): boolean {
    return this.brickPartColourService.userIsBMCBrickPartColourWriter();
  }
}

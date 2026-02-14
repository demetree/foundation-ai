/*
   GENERATED FORM FOR THE BRICKCOLOUR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickColour table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-colour-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickColourService, BrickColourData, BrickColourSubmitData } from '../../../bmc-data-services/brick-colour.service';
import { ColourFinishService } from '../../../bmc-data-services/colour-finish.service';
import { BrickPartColourService } from '../../../bmc-data-services/brick-part-colour.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
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
interface BrickColourFormValues {
  name: string,
  ldrawColourCode: string,     // Stored as string for form input, converted to number on submit.
  hexRgb: string | null,
  hexEdgeColour: string | null,
  alpha: string | null,     // Stored as string for form input, converted to number on submit.
  isTransparent: boolean,
  isMetallic: boolean,
  colourFinishId: number | bigint | null,       // For FK link number
  luminance: string | null,     // Stored as string for form input, converted to number on submit.
  legoColourId: string | null,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-colour-detail',
  templateUrl: './brick-colour-detail.component.html',
  styleUrls: ['./brick-colour-detail.component.scss']
})

export class BrickColourDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickColourFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickColourForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        ldrawColourCode: ['', Validators.required],
        hexRgb: [''],
        hexEdgeColour: [''],
        alpha: [''],
        isTransparent: [false],
        isMetallic: [false],
        colourFinishId: [null],
        luminance: [''],
        legoColourId: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public brickColourId: string | null = null;
  public brickColourData: BrickColourData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickColours$ = this.brickColourService.GetBrickColourList();
  public colourFinishs$ = this.colourFinishService.GetColourFinishList();
  public brickPartColours$ = this.brickPartColourService.GetBrickPartColourList();
  public placedBricks$ = this.placedBrickService.GetPlacedBrickList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickColourService: BrickColourService,
    public colourFinishService: ColourFinishService,
    public brickPartColourService: BrickPartColourService,
    public placedBrickService: PlacedBrickService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickColourId from the route parameters
    this.brickColourId = this.route.snapshot.paramMap.get('brickColourId');

    if (this.brickColourId === 'new' ||
        this.brickColourId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickColourData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickColourForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickColourForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Colour';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Colour';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickColourForm.dirty) {
      return confirm('You have unsaved Brick Colour changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickColourId != null && this.brickColourId !== 'new') {

      const id = parseInt(this.brickColourId, 10);

      if (!isNaN(id)) {
        return { brickColourId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickColour data for the current brickColourId.
  *
  * Fully respects the BrickColourService caching strategy and error handling strategy.
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
    if (!this.brickColourService.userIsBMCBrickColourReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickColours.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickColourId
    //
    if (!this.brickColourId) {

      this.alertService.showMessage('No BrickColour ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickColourId = Number(this.brickColourId);

    if (isNaN(brickColourId) || brickColourId <= 0) {

      this.alertService.showMessage(`Invalid Brick Colour ID: "${this.brickColourId}"`,
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
      // This is the most targeted way: clear only this BrickColour + relations

      this.brickColourService.ClearRecordCache(brickColourId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickColourService.GetBrickColour(brickColourId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickColourData) => {

        //
        // Success path — brickColourData can legitimately be null if 404'd but request succeeded
        //
        if (!brickColourData) {

          this.handleBrickColourNotFound(brickColourId);

        } else {

          this.brickColourData = brickColourData;
          this.buildFormValues(this.brickColourData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickColour loaded successfully',
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
        this.handleBrickColourLoadError(error, brickColourId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickColourNotFound(brickColourId: number): void {

    this.brickColourData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickColour #${brickColourId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickColourLoadError(error: any, brickColourId: number): void {

    let message = 'Failed to load Brick Colour.';
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
          message = 'You do not have permission to view this Brick Colour.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Colour #${brickColourId} was not found.`;
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

    console.error(`Brick Colour load failed (ID: ${brickColourId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickColourData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickColourData: BrickColourData | null) {

    if (brickColourData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickColourForm.reset({
        name: '',
        ldrawColourCode: '',
        hexRgb: '',
        hexEdgeColour: '',
        alpha: '',
        isTransparent: false,
        isMetallic: false,
        colourFinishId: null,
        luminance: '',
        legoColourId: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickColourForm.reset({
        name: brickColourData.name ?? '',
        ldrawColourCode: brickColourData.ldrawColourCode?.toString() ?? '',
        hexRgb: brickColourData.hexRgb ?? '',
        hexEdgeColour: brickColourData.hexEdgeColour ?? '',
        alpha: brickColourData.alpha?.toString() ?? '',
        isTransparent: brickColourData.isTransparent ?? false,
        isMetallic: brickColourData.isMetallic ?? false,
        colourFinishId: brickColourData.colourFinishId,
        luminance: brickColourData.luminance?.toString() ?? '',
        legoColourId: brickColourData.legoColourId?.toString() ?? '',
        sequence: brickColourData.sequence?.toString() ?? '',
        active: brickColourData.active ?? true,
        deleted: brickColourData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickColourForm.markAsPristine();
    this.brickColourForm.markAsUntouched();
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

    if (this.brickColourService.userIsBMCBrickColourWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Colours", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickColourForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickColourForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickColourForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickColourSubmitData: BrickColourSubmitData = {
        id: this.brickColourData?.id || 0,
        name: formValue.name!.trim(),
        ldrawColourCode: Number(formValue.ldrawColourCode),
        hexRgb: formValue.hexRgb?.trim() || null,
        hexEdgeColour: formValue.hexEdgeColour?.trim() || null,
        alpha: formValue.alpha ? Number(formValue.alpha) : null,
        isTransparent: !!formValue.isTransparent,
        isMetallic: !!formValue.isMetallic,
        colourFinishId: formValue.colourFinishId ? Number(formValue.colourFinishId) : null,
        luminance: formValue.luminance ? Number(formValue.luminance) : null,
        legoColourId: formValue.legoColourId ? Number(formValue.legoColourId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickColourService.PutBrickColour(brickColourSubmitData.id, brickColourSubmitData)
      : this.brickColourService.PostBrickColour(brickColourSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickColourData) => {

        this.brickColourService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Colour's detail page
          //
          this.brickColourForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickColourForm.markAsUntouched();

          this.router.navigate(['/brickcolours', savedBrickColourData.id]);
          this.alertService.showMessage('Brick Colour added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickColourData = savedBrickColourData;
          this.buildFormValues(this.brickColourData);

          this.alertService.showMessage("Brick Colour saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Colour.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Colour.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Colour could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickColourReader(): boolean {
    return this.brickColourService.userIsBMCBrickColourReader();
  }

  public userIsBMCBrickColourWriter(): boolean {
    return this.brickColourService.userIsBMCBrickColourWriter();
  }
}

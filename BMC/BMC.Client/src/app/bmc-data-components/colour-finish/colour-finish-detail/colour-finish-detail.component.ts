/*
   GENERATED FORM FOR THE COLOURFINISH TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ColourFinish table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to colour-finish-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ColourFinishService, ColourFinishData, ColourFinishSubmitData } from '../../../bmc-data-services/colour-finish.service';
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
interface ColourFinishFormValues {
  name: string,
  description: string,
  requiresEnvironmentMap: boolean,
  isMatte: boolean,
  defaultAlpha: string | null,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-colour-finish-detail',
  templateUrl: './colour-finish-detail.component.html',
  styleUrls: ['./colour-finish-detail.component.scss']
})

export class ColourFinishDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ColourFinishFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public colourFinishForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        requiresEnvironmentMap: [false],
        isMatte: [false],
        defaultAlpha: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public colourFinishId: string | null = null;
  public colourFinishData: ColourFinishData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  colourFinishes$ = this.colourFinishService.GetColourFinishList();
  public brickColours$ = this.brickColourService.GetBrickColourList();

  private destroy$ = new Subject<void>();

  constructor(
    public colourFinishService: ColourFinishService,
    public brickColourService: BrickColourService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the colourFinishId from the route parameters
    this.colourFinishId = this.route.snapshot.paramMap.get('colourFinishId');

    if (this.colourFinishId === 'new' ||
        this.colourFinishId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.colourFinishData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.colourFinishForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.colourFinishForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Colour Finish';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Colour Finish';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.colourFinishForm.dirty) {
      return confirm('You have unsaved Colour Finish changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.colourFinishId != null && this.colourFinishId !== 'new') {

      const id = parseInt(this.colourFinishId, 10);

      if (!isNaN(id)) {
        return { colourFinishId: id };
      }
    }

    return null;
  }


/*
  * Loads the ColourFinish data for the current colourFinishId.
  *
  * Fully respects the ColourFinishService caching strategy and error handling strategy.
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
    if (!this.colourFinishService.userIsBMCColourFinishReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ColourFinishes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate colourFinishId
    //
    if (!this.colourFinishId) {

      this.alertService.showMessage('No ColourFinish ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const colourFinishId = Number(this.colourFinishId);

    if (isNaN(colourFinishId) || colourFinishId <= 0) {

      this.alertService.showMessage(`Invalid Colour Finish ID: "${this.colourFinishId}"`,
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
      // This is the most targeted way: clear only this ColourFinish + relations

      this.colourFinishService.ClearRecordCache(colourFinishId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.colourFinishService.GetColourFinish(colourFinishId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (colourFinishData) => {

        //
        // Success path — colourFinishData can legitimately be null if 404'd but request succeeded
        //
        if (!colourFinishData) {

          this.handleColourFinishNotFound(colourFinishId);

        } else {

          this.colourFinishData = colourFinishData;
          this.buildFormValues(this.colourFinishData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ColourFinish loaded successfully',
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
        this.handleColourFinishLoadError(error, colourFinishId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleColourFinishNotFound(colourFinishId: number): void {

    this.colourFinishData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ColourFinish #${colourFinishId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleColourFinishLoadError(error: any, colourFinishId: number): void {

    let message = 'Failed to load Colour Finish.';
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
          message = 'You do not have permission to view this Colour Finish.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Colour Finish #${colourFinishId} was not found.`;
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

    console.error(`Colour Finish load failed (ID: ${colourFinishId})`, error);

    //
    // Reset UI to safe state
    //
    this.colourFinishData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(colourFinishData: ColourFinishData | null) {

    if (colourFinishData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.colourFinishForm.reset({
        name: '',
        description: '',
        requiresEnvironmentMap: false,
        isMatte: false,
        defaultAlpha: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.colourFinishForm.reset({
        name: colourFinishData.name ?? '',
        description: colourFinishData.description ?? '',
        requiresEnvironmentMap: colourFinishData.requiresEnvironmentMap ?? false,
        isMatte: colourFinishData.isMatte ?? false,
        defaultAlpha: colourFinishData.defaultAlpha?.toString() ?? '',
        sequence: colourFinishData.sequence?.toString() ?? '',
        active: colourFinishData.active ?? true,
        deleted: colourFinishData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.colourFinishForm.markAsPristine();
    this.colourFinishForm.markAsUntouched();
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

    if (this.colourFinishService.userIsBMCColourFinishWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Colour Finishes", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.colourFinishForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.colourFinishForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.colourFinishForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const colourFinishSubmitData: ColourFinishSubmitData = {
        id: this.colourFinishData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        requiresEnvironmentMap: !!formValue.requiresEnvironmentMap,
        isMatte: !!formValue.isMatte,
        defaultAlpha: formValue.defaultAlpha ? Number(formValue.defaultAlpha) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.colourFinishService.PutColourFinish(colourFinishSubmitData.id, colourFinishSubmitData)
      : this.colourFinishService.PostColourFinish(colourFinishSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedColourFinishData) => {

        this.colourFinishService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Colour Finish's detail page
          //
          this.colourFinishForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.colourFinishForm.markAsUntouched();

          this.router.navigate(['/colourfinishes', savedColourFinishData.id]);
          this.alertService.showMessage('Colour Finish added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.colourFinishData = savedColourFinishData;
          this.buildFormValues(this.colourFinishData);

          this.alertService.showMessage("Colour Finish saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Colour Finish.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Colour Finish.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Colour Finish could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCColourFinishReader(): boolean {
    return this.colourFinishService.userIsBMCColourFinishReader();
  }

  public userIsBMCColourFinishWriter(): boolean {
    return this.colourFinishService.userIsBMCColourFinishWriter();
  }
}

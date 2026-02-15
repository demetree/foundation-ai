/*
   GENERATED FORM FOR THE BRICKCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickCategory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-category-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickCategoryService, BrickCategoryData, BrickCategorySubmitData } from '../../../bmc-data-services/brick-category.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
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
interface BrickCategoryFormValues {
  name: string,
  description: string,
  rebrickablePartCategoryId: string | null,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-category-detail',
  templateUrl: './brick-category-detail.component.html',
  styleUrls: ['./brick-category-detail.component.scss']
})

export class BrickCategoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickCategoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickCategoryForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        rebrickablePartCategoryId: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public brickCategoryId: string | null = null;
  public brickCategoryData: BrickCategoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickCategories$ = this.brickCategoryService.GetBrickCategoryList();
  public brickParts$ = this.brickPartService.GetBrickPartList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickCategoryService: BrickCategoryService,
    public brickPartService: BrickPartService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickCategoryId from the route parameters
    this.brickCategoryId = this.route.snapshot.paramMap.get('brickCategoryId');

    if (this.brickCategoryId === 'new' ||
        this.brickCategoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickCategoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickCategoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickCategoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Category';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Category';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickCategoryForm.dirty) {
      return confirm('You have unsaved Brick Category changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickCategoryId != null && this.brickCategoryId !== 'new') {

      const id = parseInt(this.brickCategoryId, 10);

      if (!isNaN(id)) {
        return { brickCategoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickCategory data for the current brickCategoryId.
  *
  * Fully respects the BrickCategoryService caching strategy and error handling strategy.
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
    if (!this.brickCategoryService.userIsBMCBrickCategoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickCategories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickCategoryId
    //
    if (!this.brickCategoryId) {

      this.alertService.showMessage('No BrickCategory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickCategoryId = Number(this.brickCategoryId);

    if (isNaN(brickCategoryId) || brickCategoryId <= 0) {

      this.alertService.showMessage(`Invalid Brick Category ID: "${this.brickCategoryId}"`,
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
      // This is the most targeted way: clear only this BrickCategory + relations

      this.brickCategoryService.ClearRecordCache(brickCategoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickCategoryService.GetBrickCategory(brickCategoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickCategoryData) => {

        //
        // Success path — brickCategoryData can legitimately be null if 404'd but request succeeded
        //
        if (!brickCategoryData) {

          this.handleBrickCategoryNotFound(brickCategoryId);

        } else {

          this.brickCategoryData = brickCategoryData;
          this.buildFormValues(this.brickCategoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickCategory loaded successfully',
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
        this.handleBrickCategoryLoadError(error, brickCategoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickCategoryNotFound(brickCategoryId: number): void {

    this.brickCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickCategory #${brickCategoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickCategoryLoadError(error: any, brickCategoryId: number): void {

    let message = 'Failed to load Brick Category.';
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
          message = 'You do not have permission to view this Brick Category.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Category #${brickCategoryId} was not found.`;
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

    console.error(`Brick Category load failed (ID: ${brickCategoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickCategoryData: BrickCategoryData | null) {

    if (brickCategoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickCategoryForm.reset({
        name: '',
        description: '',
        rebrickablePartCategoryId: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickCategoryForm.reset({
        name: brickCategoryData.name ?? '',
        description: brickCategoryData.description ?? '',
        rebrickablePartCategoryId: brickCategoryData.rebrickablePartCategoryId?.toString() ?? '',
        sequence: brickCategoryData.sequence?.toString() ?? '',
        active: brickCategoryData.active ?? true,
        deleted: brickCategoryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickCategoryForm.markAsPristine();
    this.brickCategoryForm.markAsUntouched();
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

    if (this.brickCategoryService.userIsBMCBrickCategoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Categories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickCategoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickCategoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickCategoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickCategorySubmitData: BrickCategorySubmitData = {
        id: this.brickCategoryData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        rebrickablePartCategoryId: formValue.rebrickablePartCategoryId ? Number(formValue.rebrickablePartCategoryId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickCategoryService.PutBrickCategory(brickCategorySubmitData.id, brickCategorySubmitData)
      : this.brickCategoryService.PostBrickCategory(brickCategorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickCategoryData) => {

        this.brickCategoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Category's detail page
          //
          this.brickCategoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickCategoryForm.markAsUntouched();

          this.router.navigate(['/brickcategories', savedBrickCategoryData.id]);
          this.alertService.showMessage('Brick Category added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickCategoryData = savedBrickCategoryData;
          this.buildFormValues(this.brickCategoryData);

          this.alertService.showMessage("Brick Category saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickCategoryReader(): boolean {
    return this.brickCategoryService.userIsBMCBrickCategoryReader();
  }

  public userIsBMCBrickCategoryWriter(): boolean {
    return this.brickCategoryService.userIsBMCBrickCategoryWriter();
  }
}

/*
   GENERATED FORM FOR THE SUBMODELPLACEDBRICK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SubmodelPlacedBrick table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-placed-brick-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelPlacedBrickService, SubmodelPlacedBrickData, SubmodelPlacedBrickSubmitData } from '../../../bmc-data-services/submodel-placed-brick.service';
import { SubmodelService } from '../../../bmc-data-services/submodel.service';
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
interface SubmodelPlacedBrickFormValues {
  submodelId: number | bigint,       // For FK link number
  placedBrickId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-submodel-placed-brick-detail',
  templateUrl: './submodel-placed-brick-detail.component.html',
  styleUrls: ['./submodel-placed-brick-detail.component.scss']
})

export class SubmodelPlacedBrickDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelPlacedBrickFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelPlacedBrickForm: FormGroup = this.fb.group({
        submodelId: [null, Validators.required],
        placedBrickId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });


  public submodelPlacedBrickId: string | null = null;
  public submodelPlacedBrickData: SubmodelPlacedBrickData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  submodelPlacedBricks$ = this.submodelPlacedBrickService.GetSubmodelPlacedBrickList();
  public submodels$ = this.submodelService.GetSubmodelList();
  public placedBricks$ = this.placedBrickService.GetPlacedBrickList();

  private destroy$ = new Subject<void>();

  constructor(
    public submodelPlacedBrickService: SubmodelPlacedBrickService,
    public submodelService: SubmodelService,
    public placedBrickService: PlacedBrickService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the submodelPlacedBrickId from the route parameters
    this.submodelPlacedBrickId = this.route.snapshot.paramMap.get('submodelPlacedBrickId');

    if (this.submodelPlacedBrickId === 'new' ||
        this.submodelPlacedBrickId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.submodelPlacedBrickData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.submodelPlacedBrickForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelPlacedBrickForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Submodel Placed Brick';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Submodel Placed Brick';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.submodelPlacedBrickForm.dirty) {
      return confirm('You have unsaved Submodel Placed Brick changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.submodelPlacedBrickId != null && this.submodelPlacedBrickId !== 'new') {

      const id = parseInt(this.submodelPlacedBrickId, 10);

      if (!isNaN(id)) {
        return { submodelPlacedBrickId: id };
      }
    }

    return null;
  }


/*
  * Loads the SubmodelPlacedBrick data for the current submodelPlacedBrickId.
  *
  * Fully respects the SubmodelPlacedBrickService caching strategy and error handling strategy.
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
    if (!this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SubmodelPlacedBricks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate submodelPlacedBrickId
    //
    if (!this.submodelPlacedBrickId) {

      this.alertService.showMessage('No SubmodelPlacedBrick ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const submodelPlacedBrickId = Number(this.submodelPlacedBrickId);

    if (isNaN(submodelPlacedBrickId) || submodelPlacedBrickId <= 0) {

      this.alertService.showMessage(`Invalid Submodel Placed Brick ID: "${this.submodelPlacedBrickId}"`,
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
      // This is the most targeted way: clear only this SubmodelPlacedBrick + relations

      this.submodelPlacedBrickService.ClearRecordCache(submodelPlacedBrickId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.submodelPlacedBrickService.GetSubmodelPlacedBrick(submodelPlacedBrickId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (submodelPlacedBrickData) => {

        //
        // Success path — submodelPlacedBrickData can legitimately be null if 404'd but request succeeded
        //
        if (!submodelPlacedBrickData) {

          this.handleSubmodelPlacedBrickNotFound(submodelPlacedBrickId);

        } else {

          this.submodelPlacedBrickData = submodelPlacedBrickData;
          this.buildFormValues(this.submodelPlacedBrickData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SubmodelPlacedBrick loaded successfully',
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
        this.handleSubmodelPlacedBrickLoadError(error, submodelPlacedBrickId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSubmodelPlacedBrickNotFound(submodelPlacedBrickId: number): void {

    this.submodelPlacedBrickData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SubmodelPlacedBrick #${submodelPlacedBrickId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSubmodelPlacedBrickLoadError(error: any, submodelPlacedBrickId: number): void {

    let message = 'Failed to load Submodel Placed Brick.';
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
          message = 'You do not have permission to view this Submodel Placed Brick.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Submodel Placed Brick #${submodelPlacedBrickId} was not found.`;
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

    console.error(`Submodel Placed Brick load failed (ID: ${submodelPlacedBrickId})`, error);

    //
    // Reset UI to safe state
    //
    this.submodelPlacedBrickData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(submodelPlacedBrickData: SubmodelPlacedBrickData | null) {

    if (submodelPlacedBrickData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelPlacedBrickForm.reset({
        submodelId: null,
        placedBrickId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.submodelPlacedBrickForm.reset({
        submodelId: submodelPlacedBrickData.submodelId,
        placedBrickId: submodelPlacedBrickData.placedBrickId,
        active: submodelPlacedBrickData.active ?? true,
        deleted: submodelPlacedBrickData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.submodelPlacedBrickForm.markAsPristine();
    this.submodelPlacedBrickForm.markAsUntouched();
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

    if (this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Submodel Placed Bricks", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.submodelPlacedBrickForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelPlacedBrickForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelPlacedBrickForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelPlacedBrickSubmitData: SubmodelPlacedBrickSubmitData = {
        id: this.submodelPlacedBrickData?.id || 0,
        submodelId: Number(formValue.submodelId),
        placedBrickId: Number(formValue.placedBrickId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.submodelPlacedBrickService.PutSubmodelPlacedBrick(submodelPlacedBrickSubmitData.id, submodelPlacedBrickSubmitData)
      : this.submodelPlacedBrickService.PostSubmodelPlacedBrick(submodelPlacedBrickSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSubmodelPlacedBrickData) => {

        this.submodelPlacedBrickService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Submodel Placed Brick's detail page
          //
          this.submodelPlacedBrickForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.submodelPlacedBrickForm.markAsUntouched();

          this.router.navigate(['/submodelplacedbricks', savedSubmodelPlacedBrickData.id]);
          this.alertService.showMessage('Submodel Placed Brick added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.submodelPlacedBrickData = savedSubmodelPlacedBrickData;
          this.buildFormValues(this.submodelPlacedBrickData);

          this.alertService.showMessage("Submodel Placed Brick saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Submodel Placed Brick.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Placed Brick.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Placed Brick could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCSubmodelPlacedBrickReader(): boolean {
    return this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickReader();
  }

  public userIsBMCSubmodelPlacedBrickWriter(): boolean {
    return this.submodelPlacedBrickService.userIsBMCSubmodelPlacedBrickWriter();
  }
}

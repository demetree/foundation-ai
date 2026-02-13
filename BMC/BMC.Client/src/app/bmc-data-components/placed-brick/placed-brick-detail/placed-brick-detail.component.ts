/*
   GENERATED FORM FOR THE PLACEDBRICK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PlacedBrick table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to placed-brick-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PlacedBrickService, PlacedBrickData, PlacedBrickSubmitData } from '../../../bmc-data-services/placed-brick.service';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
import { PlacedBrickChangeHistoryService } from '../../../bmc-data-services/placed-brick-change-history.service';
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
interface PlacedBrickFormValues {
  projectId: number | bigint | null,       // For FK link number
  brickPartId: number | bigint | null,       // For FK link number
  brickColourId: number | bigint | null,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationX: string | null,     // Stored as string for form input, converted to number on submit.
  rotationY: string | null,     // Stored as string for form input, converted to number on submit.
  rotationZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationW: string | null,     // Stored as string for form input, converted to number on submit.
  buildStepNumber: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-placed-brick-detail',
  templateUrl: './placed-brick-detail.component.html',
  styleUrls: ['./placed-brick-detail.component.scss']
})

export class PlacedBrickDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PlacedBrickFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public placedBrickForm: FormGroup = this.fb.group({
        projectId: [null],
        brickPartId: [null],
        brickColourId: [null],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        rotationX: [''],
        rotationY: [''],
        rotationZ: [''],
        rotationW: [''],
        buildStepNumber: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public placedBrickId: string | null = null;
  public placedBrickData: PlacedBrickData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  placedBricks$ = this.placedBrickService.GetPlacedBrickList();
  public projects$ = this.projectService.GetProjectList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();
  public placedBrickChangeHistories$ = this.placedBrickChangeHistoryService.GetPlacedBrickChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public placedBrickService: PlacedBrickService,
    public projectService: ProjectService,
    public brickPartService: BrickPartService,
    public brickColourService: BrickColourService,
    public placedBrickChangeHistoryService: PlacedBrickChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the placedBrickId from the route parameters
    this.placedBrickId = this.route.snapshot.paramMap.get('placedBrickId');

    if (this.placedBrickId === 'new' ||
        this.placedBrickId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.placedBrickData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.placedBrickForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.placedBrickForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Placed Brick';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Placed Brick';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.placedBrickForm.dirty) {
      return confirm('You have unsaved Placed Brick changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.placedBrickId != null && this.placedBrickId !== 'new') {

      const id = parseInt(this.placedBrickId, 10);

      if (!isNaN(id)) {
        return { placedBrickId: id };
      }
    }

    return null;
  }


/*
  * Loads the PlacedBrick data for the current placedBrickId.
  *
  * Fully respects the PlacedBrickService caching strategy and error handling strategy.
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
    if (!this.placedBrickService.userIsBMCPlacedBrickReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PlacedBricks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate placedBrickId
    //
    if (!this.placedBrickId) {

      this.alertService.showMessage('No PlacedBrick ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const placedBrickId = Number(this.placedBrickId);

    if (isNaN(placedBrickId) || placedBrickId <= 0) {

      this.alertService.showMessage(`Invalid Placed Brick ID: "${this.placedBrickId}"`,
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
      // This is the most targeted way: clear only this PlacedBrick + relations

      this.placedBrickService.ClearRecordCache(placedBrickId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.placedBrickService.GetPlacedBrick(placedBrickId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (placedBrickData) => {

        //
        // Success path — placedBrickData can legitimately be null if 404'd but request succeeded
        //
        if (!placedBrickData) {

          this.handlePlacedBrickNotFound(placedBrickId);

        } else {

          this.placedBrickData = placedBrickData;
          this.buildFormValues(this.placedBrickData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PlacedBrick loaded successfully',
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
        this.handlePlacedBrickLoadError(error, placedBrickId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePlacedBrickNotFound(placedBrickId: number): void {

    this.placedBrickData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PlacedBrick #${placedBrickId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePlacedBrickLoadError(error: any, placedBrickId: number): void {

    let message = 'Failed to load Placed Brick.';
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
          message = 'You do not have permission to view this Placed Brick.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Placed Brick #${placedBrickId} was not found.`;
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

    console.error(`Placed Brick load failed (ID: ${placedBrickId})`, error);

    //
    // Reset UI to safe state
    //
    this.placedBrickData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(placedBrickData: PlacedBrickData | null) {

    if (placedBrickData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.placedBrickForm.reset({
        projectId: null,
        brickPartId: null,
        brickColourId: null,
        positionX: '',
        positionY: '',
        positionZ: '',
        rotationX: '',
        rotationY: '',
        rotationZ: '',
        rotationW: '',
        buildStepNumber: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.placedBrickForm.reset({
        projectId: placedBrickData.projectId,
        brickPartId: placedBrickData.brickPartId,
        brickColourId: placedBrickData.brickColourId,
        positionX: placedBrickData.positionX?.toString() ?? '',
        positionY: placedBrickData.positionY?.toString() ?? '',
        positionZ: placedBrickData.positionZ?.toString() ?? '',
        rotationX: placedBrickData.rotationX?.toString() ?? '',
        rotationY: placedBrickData.rotationY?.toString() ?? '',
        rotationZ: placedBrickData.rotationZ?.toString() ?? '',
        rotationW: placedBrickData.rotationW?.toString() ?? '',
        buildStepNumber: placedBrickData.buildStepNumber?.toString() ?? '',
        versionNumber: placedBrickData.versionNumber?.toString() ?? '',
        active: placedBrickData.active ?? true,
        deleted: placedBrickData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.placedBrickForm.markAsPristine();
    this.placedBrickForm.markAsUntouched();
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

    if (this.placedBrickService.userIsBMCPlacedBrickWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Placed Bricks", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.placedBrickForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.placedBrickForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.placedBrickForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const placedBrickSubmitData: PlacedBrickSubmitData = {
        id: this.placedBrickData?.id || 0,
        projectId: formValue.projectId ? Number(formValue.projectId) : null,
        brickPartId: formValue.brickPartId ? Number(formValue.brickPartId) : null,
        brickColourId: formValue.brickColourId ? Number(formValue.brickColourId) : null,
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        rotationX: formValue.rotationX ? Number(formValue.rotationX) : null,
        rotationY: formValue.rotationY ? Number(formValue.rotationY) : null,
        rotationZ: formValue.rotationZ ? Number(formValue.rotationZ) : null,
        rotationW: formValue.rotationW ? Number(formValue.rotationW) : null,
        buildStepNumber: formValue.buildStepNumber ? Number(formValue.buildStepNumber) : null,
        versionNumber: this.placedBrickData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.placedBrickService.PutPlacedBrick(placedBrickSubmitData.id, placedBrickSubmitData)
      : this.placedBrickService.PostPlacedBrick(placedBrickSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPlacedBrickData) => {

        this.placedBrickService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Placed Brick's detail page
          //
          this.placedBrickForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.placedBrickForm.markAsUntouched();

          this.router.navigate(['/placedbricks', savedPlacedBrickData.id]);
          this.alertService.showMessage('Placed Brick added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.placedBrickData = savedPlacedBrickData;
          this.buildFormValues(this.placedBrickData);

          this.alertService.showMessage("Placed Brick saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Placed Brick.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Placed Brick.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Placed Brick could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCPlacedBrickReader(): boolean {
    return this.placedBrickService.userIsBMCPlacedBrickReader();
  }

  public userIsBMCPlacedBrickWriter(): boolean {
    return this.placedBrickService.userIsBMCPlacedBrickWriter();
  }
}

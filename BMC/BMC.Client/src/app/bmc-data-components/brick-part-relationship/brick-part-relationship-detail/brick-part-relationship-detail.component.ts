/*
   GENERATED FORM FOR THE BRICKPARTRELATIONSHIP TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPartRelationship table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-relationship-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickPartRelationshipService, BrickPartRelationshipData, BrickPartRelationshipSubmitData } from '../../../bmc-data-services/brick-part-relationship.service';
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
interface BrickPartRelationshipFormValues {
  childBrickPartId: number | bigint,       // For FK link number
  parentBrickPartId: number | bigint,       // For FK link number
  relationshipType: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-part-relationship-detail',
  templateUrl: './brick-part-relationship-detail.component.html',
  styleUrls: ['./brick-part-relationship-detail.component.scss']
})

export class BrickPartRelationshipDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickPartRelationshipFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickPartRelationshipForm: FormGroup = this.fb.group({
        childBrickPartId: [null, Validators.required],
        parentBrickPartId: [null, Validators.required],
        relationshipType: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public brickPartRelationshipId: string | null = null;
  public brickPartRelationshipData: BrickPartRelationshipData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickPartRelationships$ = this.brickPartRelationshipService.GetBrickPartRelationshipList();
  public brickParts$ = this.brickPartService.GetBrickPartList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickPartRelationshipService: BrickPartRelationshipService,
    public brickPartService: BrickPartService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickPartRelationshipId from the route parameters
    this.brickPartRelationshipId = this.route.snapshot.paramMap.get('brickPartRelationshipId');

    if (this.brickPartRelationshipId === 'new' ||
        this.brickPartRelationshipId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickPartRelationshipData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickPartRelationshipForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickPartRelationshipForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Part Relationship';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Part Relationship';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickPartRelationshipForm.dirty) {
      return confirm('You have unsaved Brick Part Relationship changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickPartRelationshipId != null && this.brickPartRelationshipId !== 'new') {

      const id = parseInt(this.brickPartRelationshipId, 10);

      if (!isNaN(id)) {
        return { brickPartRelationshipId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickPartRelationship data for the current brickPartRelationshipId.
  *
  * Fully respects the BrickPartRelationshipService caching strategy and error handling strategy.
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
    if (!this.brickPartRelationshipService.userIsBMCBrickPartRelationshipReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickPartRelationships.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickPartRelationshipId
    //
    if (!this.brickPartRelationshipId) {

      this.alertService.showMessage('No BrickPartRelationship ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickPartRelationshipId = Number(this.brickPartRelationshipId);

    if (isNaN(brickPartRelationshipId) || brickPartRelationshipId <= 0) {

      this.alertService.showMessage(`Invalid Brick Part Relationship ID: "${this.brickPartRelationshipId}"`,
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
      // This is the most targeted way: clear only this BrickPartRelationship + relations

      this.brickPartRelationshipService.ClearRecordCache(brickPartRelationshipId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickPartRelationshipService.GetBrickPartRelationship(brickPartRelationshipId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickPartRelationshipData) => {

        //
        // Success path — brickPartRelationshipData can legitimately be null if 404'd but request succeeded
        //
        if (!brickPartRelationshipData) {

          this.handleBrickPartRelationshipNotFound(brickPartRelationshipId);

        } else {

          this.brickPartRelationshipData = brickPartRelationshipData;
          this.buildFormValues(this.brickPartRelationshipData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickPartRelationship loaded successfully',
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
        this.handleBrickPartRelationshipLoadError(error, brickPartRelationshipId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickPartRelationshipNotFound(brickPartRelationshipId: number): void {

    this.brickPartRelationshipData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickPartRelationship #${brickPartRelationshipId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickPartRelationshipLoadError(error: any, brickPartRelationshipId: number): void {

    let message = 'Failed to load Brick Part Relationship.';
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
          message = 'You do not have permission to view this Brick Part Relationship.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Part Relationship #${brickPartRelationshipId} was not found.`;
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

    console.error(`Brick Part Relationship load failed (ID: ${brickPartRelationshipId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickPartRelationshipData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickPartRelationshipData: BrickPartRelationshipData | null) {

    if (brickPartRelationshipData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickPartRelationshipForm.reset({
        childBrickPartId: null,
        parentBrickPartId: null,
        relationshipType: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickPartRelationshipForm.reset({
        childBrickPartId: brickPartRelationshipData.childBrickPartId,
        parentBrickPartId: brickPartRelationshipData.parentBrickPartId,
        relationshipType: brickPartRelationshipData.relationshipType ?? '',
        active: brickPartRelationshipData.active ?? true,
        deleted: brickPartRelationshipData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickPartRelationshipForm.markAsPristine();
    this.brickPartRelationshipForm.markAsUntouched();
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

    if (this.brickPartRelationshipService.userIsBMCBrickPartRelationshipWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Part Relationships", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickPartRelationshipForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickPartRelationshipForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickPartRelationshipForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickPartRelationshipSubmitData: BrickPartRelationshipSubmitData = {
        id: this.brickPartRelationshipData?.id || 0,
        childBrickPartId: Number(formValue.childBrickPartId),
        parentBrickPartId: Number(formValue.parentBrickPartId),
        relationshipType: formValue.relationshipType!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickPartRelationshipService.PutBrickPartRelationship(brickPartRelationshipSubmitData.id, brickPartRelationshipSubmitData)
      : this.brickPartRelationshipService.PostBrickPartRelationship(brickPartRelationshipSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickPartRelationshipData) => {

        this.brickPartRelationshipService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Part Relationship's detail page
          //
          this.brickPartRelationshipForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickPartRelationshipForm.markAsUntouched();

          this.router.navigate(['/brickpartrelationships', savedBrickPartRelationshipData.id]);
          this.alertService.showMessage('Brick Part Relationship added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickPartRelationshipData = savedBrickPartRelationshipData;
          this.buildFormValues(this.brickPartRelationshipData);

          this.alertService.showMessage("Brick Part Relationship saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Part Relationship.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part Relationship.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part Relationship could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickPartRelationshipReader(): boolean {
    return this.brickPartRelationshipService.userIsBMCBrickPartRelationshipReader();
  }

  public userIsBMCBrickPartRelationshipWriter(): boolean {
    return this.brickPartRelationshipService.userIsBMCBrickPartRelationshipWriter();
  }
}

/*
   GENERATED FORM FOR THE BRICKSETSETREVIEW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickSetSetReview table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-set-set-review-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickSetSetReviewService, BrickSetSetReviewData, BrickSetSetReviewSubmitData } from '../../../bmc-data-services/brick-set-set-review.service';
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
interface BrickSetSetReviewFormValues {
  legoSetId: number | bigint,       // For FK link number
  reviewAuthor: string,
  reviewDate: string | null,
  reviewTitle: string | null,
  reviewBody: string | null,
  overallRating: string | null,     // Stored as string for form input, converted to number on submit.
  buildingExperienceRating: string | null,     // Stored as string for form input, converted to number on submit.
  valueForMoneyRating: string | null,     // Stored as string for form input, converted to number on submit.
  partsRating: string | null,     // Stored as string for form input, converted to number on submit.
  playabilityRating: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-set-set-review-detail',
  templateUrl: './brick-set-set-review-detail.component.html',
  styleUrls: ['./brick-set-set-review-detail.component.scss']
})

export class BrickSetSetReviewDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickSetSetReviewFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickSetSetReviewForm: FormGroup = this.fb.group({
        legoSetId: [null, Validators.required],
        reviewAuthor: ['', Validators.required],
        reviewDate: [''],
        reviewTitle: [''],
        reviewBody: [''],
        overallRating: [''],
        buildingExperienceRating: [''],
        valueForMoneyRating: [''],
        partsRating: [''],
        playabilityRating: [''],
        active: [true],
        deleted: [false],
      });


  public brickSetSetReviewId: string | null = null;
  public brickSetSetReviewData: BrickSetSetReviewData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickSetSetReviews$ = this.brickSetSetReviewService.GetBrickSetSetReviewList();
  public legoSets$ = this.legoSetService.GetLegoSetList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickSetSetReviewService: BrickSetSetReviewService,
    public legoSetService: LegoSetService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickSetSetReviewId from the route parameters
    this.brickSetSetReviewId = this.route.snapshot.paramMap.get('brickSetSetReviewId');

    if (this.brickSetSetReviewId === 'new' ||
        this.brickSetSetReviewId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickSetSetReviewData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickSetSetReviewForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickSetSetReviewForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Set Set Review';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Set Set Review';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickSetSetReviewForm.dirty) {
      return confirm('You have unsaved Brick Set Set Review changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickSetSetReviewId != null && this.brickSetSetReviewId !== 'new') {

      const id = parseInt(this.brickSetSetReviewId, 10);

      if (!isNaN(id)) {
        return { brickSetSetReviewId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickSetSetReview data for the current brickSetSetReviewId.
  *
  * Fully respects the BrickSetSetReviewService caching strategy and error handling strategy.
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
    if (!this.brickSetSetReviewService.userIsBMCBrickSetSetReviewReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickSetSetReviews.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickSetSetReviewId
    //
    if (!this.brickSetSetReviewId) {

      this.alertService.showMessage('No BrickSetSetReview ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickSetSetReviewId = Number(this.brickSetSetReviewId);

    if (isNaN(brickSetSetReviewId) || brickSetSetReviewId <= 0) {

      this.alertService.showMessage(`Invalid Brick Set Set Review ID: "${this.brickSetSetReviewId}"`,
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
      // This is the most targeted way: clear only this BrickSetSetReview + relations

      this.brickSetSetReviewService.ClearRecordCache(brickSetSetReviewId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickSetSetReviewService.GetBrickSetSetReview(brickSetSetReviewId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickSetSetReviewData) => {

        //
        // Success path — brickSetSetReviewData can legitimately be null if 404'd but request succeeded
        //
        if (!brickSetSetReviewData) {

          this.handleBrickSetSetReviewNotFound(brickSetSetReviewId);

        } else {

          this.brickSetSetReviewData = brickSetSetReviewData;
          this.buildFormValues(this.brickSetSetReviewData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickSetSetReview loaded successfully',
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
        this.handleBrickSetSetReviewLoadError(error, brickSetSetReviewId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickSetSetReviewNotFound(brickSetSetReviewId: number): void {

    this.brickSetSetReviewData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickSetSetReview #${brickSetSetReviewId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickSetSetReviewLoadError(error: any, brickSetSetReviewId: number): void {

    let message = 'Failed to load Brick Set Set Review.';
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
          message = 'You do not have permission to view this Brick Set Set Review.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Set Set Review #${brickSetSetReviewId} was not found.`;
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

    console.error(`Brick Set Set Review load failed (ID: ${brickSetSetReviewId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickSetSetReviewData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickSetSetReviewData: BrickSetSetReviewData | null) {

    if (brickSetSetReviewData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickSetSetReviewForm.reset({
        legoSetId: null,
        reviewAuthor: '',
        reviewDate: '',
        reviewTitle: '',
        reviewBody: '',
        overallRating: '',
        buildingExperienceRating: '',
        valueForMoneyRating: '',
        partsRating: '',
        playabilityRating: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickSetSetReviewForm.reset({
        legoSetId: brickSetSetReviewData.legoSetId,
        reviewAuthor: brickSetSetReviewData.reviewAuthor ?? '',
        reviewDate: isoUtcStringToDateTimeLocal(brickSetSetReviewData.reviewDate) ?? '',
        reviewTitle: brickSetSetReviewData.reviewTitle ?? '',
        reviewBody: brickSetSetReviewData.reviewBody ?? '',
        overallRating: brickSetSetReviewData.overallRating?.toString() ?? '',
        buildingExperienceRating: brickSetSetReviewData.buildingExperienceRating?.toString() ?? '',
        valueForMoneyRating: brickSetSetReviewData.valueForMoneyRating?.toString() ?? '',
        partsRating: brickSetSetReviewData.partsRating?.toString() ?? '',
        playabilityRating: brickSetSetReviewData.playabilityRating?.toString() ?? '',
        active: brickSetSetReviewData.active ?? true,
        deleted: brickSetSetReviewData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickSetSetReviewForm.markAsPristine();
    this.brickSetSetReviewForm.markAsUntouched();
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

    if (this.brickSetSetReviewService.userIsBMCBrickSetSetReviewWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Set Set Reviews", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickSetSetReviewForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickSetSetReviewForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickSetSetReviewForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickSetSetReviewSubmitData: BrickSetSetReviewSubmitData = {
        id: this.brickSetSetReviewData?.id || 0,
        legoSetId: Number(formValue.legoSetId),
        reviewAuthor: formValue.reviewAuthor!.trim(),
        reviewDate: formValue.reviewDate ? dateTimeLocalToIsoUtc(formValue.reviewDate.trim()) : null,
        reviewTitle: formValue.reviewTitle?.trim() || null,
        reviewBody: formValue.reviewBody?.trim() || null,
        overallRating: formValue.overallRating ? Number(formValue.overallRating) : null,
        buildingExperienceRating: formValue.buildingExperienceRating ? Number(formValue.buildingExperienceRating) : null,
        valueForMoneyRating: formValue.valueForMoneyRating ? Number(formValue.valueForMoneyRating) : null,
        partsRating: formValue.partsRating ? Number(formValue.partsRating) : null,
        playabilityRating: formValue.playabilityRating ? Number(formValue.playabilityRating) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickSetSetReviewService.PutBrickSetSetReview(brickSetSetReviewSubmitData.id, brickSetSetReviewSubmitData)
      : this.brickSetSetReviewService.PostBrickSetSetReview(brickSetSetReviewSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickSetSetReviewData) => {

        this.brickSetSetReviewService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Set Set Review's detail page
          //
          this.brickSetSetReviewForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickSetSetReviewForm.markAsUntouched();

          this.router.navigate(['/bricksetsetreviews', savedBrickSetSetReviewData.id]);
          this.alertService.showMessage('Brick Set Set Review added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickSetSetReviewData = savedBrickSetSetReviewData;
          this.buildFormValues(this.brickSetSetReviewData);

          this.alertService.showMessage("Brick Set Set Review saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Set Set Review.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set Set Review.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set Set Review could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickSetSetReviewReader(): boolean {
    return this.brickSetSetReviewService.userIsBMCBrickSetSetReviewReader();
  }

  public userIsBMCBrickSetSetReviewWriter(): boolean {
    return this.brickSetSetReviewService.userIsBMCBrickSetSetReviewWriter();
  }
}

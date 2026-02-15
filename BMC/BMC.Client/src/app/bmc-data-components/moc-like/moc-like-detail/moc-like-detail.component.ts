/*
   GENERATED FORM FOR THE MOCLIKE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocLike table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-like-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocLikeService, MocLikeData, MocLikeSubmitData } from '../../../bmc-data-services/moc-like.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
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
interface MocLikeFormValues {
  publishedMocId: number | bigint,       // For FK link number
  likerTenantGuid: string,
  likedDate: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-moc-like-detail',
  templateUrl: './moc-like-detail.component.html',
  styleUrls: ['./moc-like-detail.component.scss']
})

export class MocLikeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocLikeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocLikeForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        likerTenantGuid: ['', Validators.required],
        likedDate: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public mocLikeId: string | null = null;
  public mocLikeData: MocLikeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  mocLikes$ = this.mocLikeService.GetMocLikeList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  private destroy$ = new Subject<void>();

  constructor(
    public mocLikeService: MocLikeService,
    public publishedMocService: PublishedMocService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the mocLikeId from the route parameters
    this.mocLikeId = this.route.snapshot.paramMap.get('mocLikeId');

    if (this.mocLikeId === 'new' ||
        this.mocLikeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.mocLikeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.mocLikeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocLikeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Moc Like';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Moc Like';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.mocLikeForm.dirty) {
      return confirm('You have unsaved Moc Like changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.mocLikeId != null && this.mocLikeId !== 'new') {

      const id = parseInt(this.mocLikeId, 10);

      if (!isNaN(id)) {
        return { mocLikeId: id };
      }
    }

    return null;
  }


/*
  * Loads the MocLike data for the current mocLikeId.
  *
  * Fully respects the MocLikeService caching strategy and error handling strategy.
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
    if (!this.mocLikeService.userIsBMCMocLikeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MocLikes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate mocLikeId
    //
    if (!this.mocLikeId) {

      this.alertService.showMessage('No MocLike ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const mocLikeId = Number(this.mocLikeId);

    if (isNaN(mocLikeId) || mocLikeId <= 0) {

      this.alertService.showMessage(`Invalid Moc Like ID: "${this.mocLikeId}"`,
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
      // This is the most targeted way: clear only this MocLike + relations

      this.mocLikeService.ClearRecordCache(mocLikeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.mocLikeService.GetMocLike(mocLikeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (mocLikeData) => {

        //
        // Success path — mocLikeData can legitimately be null if 404'd but request succeeded
        //
        if (!mocLikeData) {

          this.handleMocLikeNotFound(mocLikeId);

        } else {

          this.mocLikeData = mocLikeData;
          this.buildFormValues(this.mocLikeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MocLike loaded successfully',
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
        this.handleMocLikeLoadError(error, mocLikeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMocLikeNotFound(mocLikeId: number): void {

    this.mocLikeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MocLike #${mocLikeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMocLikeLoadError(error: any, mocLikeId: number): void {

    let message = 'Failed to load Moc Like.';
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
          message = 'You do not have permission to view this Moc Like.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Moc Like #${mocLikeId} was not found.`;
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

    console.error(`Moc Like load failed (ID: ${mocLikeId})`, error);

    //
    // Reset UI to safe state
    //
    this.mocLikeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(mocLikeData: MocLikeData | null) {

    if (mocLikeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocLikeForm.reset({
        publishedMocId: null,
        likerTenantGuid: '',
        likedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocLikeForm.reset({
        publishedMocId: mocLikeData.publishedMocId,
        likerTenantGuid: mocLikeData.likerTenantGuid ?? '',
        likedDate: isoUtcStringToDateTimeLocal(mocLikeData.likedDate) ?? '',
        active: mocLikeData.active ?? true,
        deleted: mocLikeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocLikeForm.markAsPristine();
    this.mocLikeForm.markAsUntouched();
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

    if (this.mocLikeService.userIsBMCMocLikeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Moc Likes", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.mocLikeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocLikeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocLikeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocLikeSubmitData: MocLikeSubmitData = {
        id: this.mocLikeData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        likerTenantGuid: formValue.likerTenantGuid!.trim(),
        likedDate: dateTimeLocalToIsoUtc(formValue.likedDate!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.mocLikeService.PutMocLike(mocLikeSubmitData.id, mocLikeSubmitData)
      : this.mocLikeService.PostMocLike(mocLikeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMocLikeData) => {

        this.mocLikeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Moc Like's detail page
          //
          this.mocLikeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.mocLikeForm.markAsUntouched();

          this.router.navigate(['/moclikes', savedMocLikeData.id]);
          this.alertService.showMessage('Moc Like added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.mocLikeData = savedMocLikeData;
          this.buildFormValues(this.mocLikeData);

          this.alertService.showMessage("Moc Like saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Moc Like.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Like.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Like could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCMocLikeReader(): boolean {
    return this.mocLikeService.userIsBMCMocLikeReader();
  }

  public userIsBMCMocLikeWriter(): boolean {
    return this.mocLikeService.userIsBMCMocLikeWriter();
  }
}

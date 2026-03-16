/*
   GENERATED FORM FOR THE MEDIAASSET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MediaAsset table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to media-asset-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MediaAssetService, MediaAssetData, MediaAssetSubmitData } from '../../../community-data-services/media-asset.service';
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
interface MediaAssetFormValues {
  fileName: string,
  filePath: string,
  mimeType: string,
  altText: string | null,
  caption: string | null,
  fileSizeBytes: string | null,     // Stored as string for form input, converted to number on submit.
  imageWidth: string | null,     // Stored as string for form input, converted to number on submit.
  imageHeight: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-media-asset-detail',
  templateUrl: './media-asset-detail.component.html',
  styleUrls: ['./media-asset-detail.component.scss']
})

export class MediaAssetDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MediaAssetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mediaAssetForm: FormGroup = this.fb.group({
        fileName: ['', Validators.required],
        filePath: ['', Validators.required],
        mimeType: ['', Validators.required],
        altText: [''],
        caption: [''],
        fileSizeBytes: [''],
        imageWidth: [''],
        imageHeight: [''],
        active: [true],
        deleted: [false],
      });


  public mediaAssetId: string | null = null;
  public mediaAssetData: MediaAssetData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  mediaAssets$ = this.mediaAssetService.GetMediaAssetList();

  private destroy$ = new Subject<void>();

  constructor(
    public mediaAssetService: MediaAssetService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the mediaAssetId from the route parameters
    this.mediaAssetId = this.route.snapshot.paramMap.get('mediaAssetId');

    if (this.mediaAssetId === 'new' ||
        this.mediaAssetId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.mediaAssetData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.mediaAssetForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mediaAssetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Media Asset';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Media Asset';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.mediaAssetForm.dirty) {
      return confirm('You have unsaved Media Asset changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.mediaAssetId != null && this.mediaAssetId !== 'new') {

      const id = parseInt(this.mediaAssetId, 10);

      if (!isNaN(id)) {
        return { mediaAssetId: id };
      }
    }

    return null;
  }


/*
  * Loads the MediaAsset data for the current mediaAssetId.
  *
  * Fully respects the MediaAssetService caching strategy and error handling strategy.
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
    if (!this.mediaAssetService.userIsCommunityMediaAssetReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MediaAssets.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate mediaAssetId
    //
    if (!this.mediaAssetId) {

      this.alertService.showMessage('No MediaAsset ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const mediaAssetId = Number(this.mediaAssetId);

    if (isNaN(mediaAssetId) || mediaAssetId <= 0) {

      this.alertService.showMessage(`Invalid Media Asset ID: "${this.mediaAssetId}"`,
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
      // This is the most targeted way: clear only this MediaAsset + relations

      this.mediaAssetService.ClearRecordCache(mediaAssetId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.mediaAssetService.GetMediaAsset(mediaAssetId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (mediaAssetData) => {

        //
        // Success path — mediaAssetData can legitimately be null if 404'd but request succeeded
        //
        if (!mediaAssetData) {

          this.handleMediaAssetNotFound(mediaAssetId);

        } else {

          this.mediaAssetData = mediaAssetData;
          this.buildFormValues(this.mediaAssetData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MediaAsset loaded successfully',
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
        this.handleMediaAssetLoadError(error, mediaAssetId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMediaAssetNotFound(mediaAssetId: number): void {

    this.mediaAssetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MediaAsset #${mediaAssetId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMediaAssetLoadError(error: any, mediaAssetId: number): void {

    let message = 'Failed to load Media Asset.';
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
          message = 'You do not have permission to view this Media Asset.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Media Asset #${mediaAssetId} was not found.`;
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

    console.error(`Media Asset load failed (ID: ${mediaAssetId})`, error);

    //
    // Reset UI to safe state
    //
    this.mediaAssetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(mediaAssetData: MediaAssetData | null) {

    if (mediaAssetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mediaAssetForm.reset({
        fileName: '',
        filePath: '',
        mimeType: '',
        altText: '',
        caption: '',
        fileSizeBytes: '',
        imageWidth: '',
        imageHeight: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mediaAssetForm.reset({
        fileName: mediaAssetData.fileName ?? '',
        filePath: mediaAssetData.filePath ?? '',
        mimeType: mediaAssetData.mimeType ?? '',
        altText: mediaAssetData.altText ?? '',
        caption: mediaAssetData.caption ?? '',
        fileSizeBytes: mediaAssetData.fileSizeBytes?.toString() ?? '',
        imageWidth: mediaAssetData.imageWidth?.toString() ?? '',
        imageHeight: mediaAssetData.imageHeight?.toString() ?? '',
        active: mediaAssetData.active ?? true,
        deleted: mediaAssetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mediaAssetForm.markAsPristine();
    this.mediaAssetForm.markAsUntouched();
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

    if (this.mediaAssetService.userIsCommunityMediaAssetWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Media Assets", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.mediaAssetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mediaAssetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mediaAssetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mediaAssetSubmitData: MediaAssetSubmitData = {
        id: this.mediaAssetData?.id || 0,
        fileName: formValue.fileName!.trim(),
        filePath: formValue.filePath!.trim(),
        mimeType: formValue.mimeType!.trim(),
        altText: formValue.altText?.trim() || null,
        caption: formValue.caption?.trim() || null,
        fileSizeBytes: formValue.fileSizeBytes ? Number(formValue.fileSizeBytes) : null,
        imageWidth: formValue.imageWidth ? Number(formValue.imageWidth) : null,
        imageHeight: formValue.imageHeight ? Number(formValue.imageHeight) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.mediaAssetService.PutMediaAsset(mediaAssetSubmitData.id, mediaAssetSubmitData)
      : this.mediaAssetService.PostMediaAsset(mediaAssetSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMediaAssetData) => {

        this.mediaAssetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Media Asset's detail page
          //
          this.mediaAssetForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.mediaAssetForm.markAsUntouched();

          this.router.navigate(['/mediaassets', savedMediaAssetData.id]);
          this.alertService.showMessage('Media Asset added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.mediaAssetData = savedMediaAssetData;
          this.buildFormValues(this.mediaAssetData);

          this.alertService.showMessage("Media Asset saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Media Asset.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Media Asset.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Media Asset could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityMediaAssetReader(): boolean {
    return this.mediaAssetService.userIsCommunityMediaAssetReader();
  }

  public userIsCommunityMediaAssetWriter(): boolean {
    return this.mediaAssetService.userIsCommunityMediaAssetWriter();
  }
}

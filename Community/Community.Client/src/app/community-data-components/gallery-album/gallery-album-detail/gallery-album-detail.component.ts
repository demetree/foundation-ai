/*
   GENERATED FORM FOR THE GALLERYALBUM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from GalleryAlbum table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to gallery-album-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { GalleryAlbumService, GalleryAlbumData, GalleryAlbumSubmitData } from '../../../community-data-services/gallery-album.service';
import { GalleryImageService } from '../../../community-data-services/gallery-image.service';
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
interface GalleryAlbumFormValues {
};


@Component({
  selector: 'app-gallery-album-detail',
  templateUrl: './gallery-album-detail.component.html',
  styleUrls: ['./gallery-album-detail.component.scss']
})

export class GalleryAlbumDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<GalleryAlbumFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public galleryAlbumForm: FormGroup = this.fb.group({
      });


  public galleryAlbumId: string | null = null;
  public galleryAlbumData: GalleryAlbumData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  galleryAlbums$ = this.galleryAlbumService.GetGalleryAlbumList();
  public galleryImages$ = this.galleryImageService.GetGalleryImageList();

  private destroy$ = new Subject<void>();

  constructor(
    public galleryAlbumService: GalleryAlbumService,
    public galleryImageService: GalleryImageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the galleryAlbumId from the route parameters
    this.galleryAlbumId = this.route.snapshot.paramMap.get('galleryAlbumId');

    if (this.galleryAlbumId === 'new' ||
        this.galleryAlbumId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.galleryAlbumData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.galleryAlbumForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.galleryAlbumForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Gallery Album';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Gallery Album';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.galleryAlbumForm.dirty) {
      return confirm('You have unsaved Gallery Album changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.galleryAlbumId != null && this.galleryAlbumId !== 'new') {

      const id = parseInt(this.galleryAlbumId, 10);

      if (!isNaN(id)) {
        return { galleryAlbumId: id };
      }
    }

    return null;
  }


/*
  * Loads the GalleryAlbum data for the current galleryAlbumId.
  *
  * Fully respects the GalleryAlbumService caching strategy and error handling strategy.
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
    if (!this.galleryAlbumService.userIsCommunityGalleryAlbumReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read GalleryAlbums.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate galleryAlbumId
    //
    if (!this.galleryAlbumId) {

      this.alertService.showMessage('No GalleryAlbum ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const galleryAlbumId = Number(this.galleryAlbumId);

    if (isNaN(galleryAlbumId) || galleryAlbumId <= 0) {

      this.alertService.showMessage(`Invalid Gallery Album ID: "${this.galleryAlbumId}"`,
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
      // This is the most targeted way: clear only this GalleryAlbum + relations

      this.galleryAlbumService.ClearRecordCache(galleryAlbumId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.galleryAlbumService.GetGalleryAlbum(galleryAlbumId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (galleryAlbumData) => {

        //
        // Success path — galleryAlbumData can legitimately be null if 404'd but request succeeded
        //
        if (!galleryAlbumData) {

          this.handleGalleryAlbumNotFound(galleryAlbumId);

        } else {

          this.galleryAlbumData = galleryAlbumData;
          this.buildFormValues(this.galleryAlbumData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'GalleryAlbum loaded successfully',
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
        this.handleGalleryAlbumLoadError(error, galleryAlbumId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleGalleryAlbumNotFound(galleryAlbumId: number): void {

    this.galleryAlbumData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `GalleryAlbum #${galleryAlbumId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleGalleryAlbumLoadError(error: any, galleryAlbumId: number): void {

    let message = 'Failed to load Gallery Album.';
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
          message = 'You do not have permission to view this Gallery Album.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Gallery Album #${galleryAlbumId} was not found.`;
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

    console.error(`Gallery Album load failed (ID: ${galleryAlbumId})`, error);

    //
    // Reset UI to safe state
    //
    this.galleryAlbumData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(galleryAlbumData: GalleryAlbumData | null) {

    if (galleryAlbumData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.galleryAlbumForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.galleryAlbumForm.reset({
      }, { emitEvent: false});
    }

    this.galleryAlbumForm.markAsPristine();
    this.galleryAlbumForm.markAsUntouched();
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

    if (this.galleryAlbumService.userIsCommunityGalleryAlbumWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Gallery Albums", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.galleryAlbumForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.galleryAlbumForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.galleryAlbumForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const galleryAlbumSubmitData: GalleryAlbumSubmitData = {
        id: this.galleryAlbumData?.id || 0,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.galleryAlbumService.PutGalleryAlbum(galleryAlbumSubmitData.id, galleryAlbumSubmitData)
      : this.galleryAlbumService.PostGalleryAlbum(galleryAlbumSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedGalleryAlbumData) => {

        this.galleryAlbumService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Gallery Album's detail page
          //
          this.galleryAlbumForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.galleryAlbumForm.markAsUntouched();

          this.router.navigate(['/galleryalbums', savedGalleryAlbumData.id]);
          this.alertService.showMessage('Gallery Album added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.galleryAlbumData = savedGalleryAlbumData;
          this.buildFormValues(this.galleryAlbumData);

          this.alertService.showMessage("Gallery Album saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Gallery Album.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Gallery Album.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Gallery Album could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityGalleryAlbumReader(): boolean {
    return this.galleryAlbumService.userIsCommunityGalleryAlbumReader();
  }

  public userIsCommunityGalleryAlbumWriter(): boolean {
    return this.galleryAlbumService.userIsCommunityGalleryAlbumWriter();
  }
}

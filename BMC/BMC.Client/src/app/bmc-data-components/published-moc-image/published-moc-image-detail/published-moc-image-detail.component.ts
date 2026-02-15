/*
   GENERATED FORM FOR THE PUBLISHEDMOCIMAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PublishedMocImage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to published-moc-image-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PublishedMocImageService, PublishedMocImageData, PublishedMocImageSubmitData } from '../../../bmc-data-services/published-moc-image.service';
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
interface PublishedMocImageFormValues {
  publishedMocId: number | bigint,       // For FK link number
  imagePath: string,
  caption: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-published-moc-image-detail',
  templateUrl: './published-moc-image-detail.component.html',
  styleUrls: ['./published-moc-image-detail.component.scss']
})

export class PublishedMocImageDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PublishedMocImageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public publishedMocImageForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        imagePath: ['', Validators.required],
        caption: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public publishedMocImageId: string | null = null;
  public publishedMocImageData: PublishedMocImageData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  publishedMocImages$ = this.publishedMocImageService.GetPublishedMocImageList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  private destroy$ = new Subject<void>();

  constructor(
    public publishedMocImageService: PublishedMocImageService,
    public publishedMocService: PublishedMocService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the publishedMocImageId from the route parameters
    this.publishedMocImageId = this.route.snapshot.paramMap.get('publishedMocImageId');

    if (this.publishedMocImageId === 'new' ||
        this.publishedMocImageId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.publishedMocImageData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.publishedMocImageForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.publishedMocImageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Published Moc Image';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Published Moc Image';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.publishedMocImageForm.dirty) {
      return confirm('You have unsaved Published Moc Image changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.publishedMocImageId != null && this.publishedMocImageId !== 'new') {

      const id = parseInt(this.publishedMocImageId, 10);

      if (!isNaN(id)) {
        return { publishedMocImageId: id };
      }
    }

    return null;
  }


/*
  * Loads the PublishedMocImage data for the current publishedMocImageId.
  *
  * Fully respects the PublishedMocImageService caching strategy and error handling strategy.
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
    if (!this.publishedMocImageService.userIsBMCPublishedMocImageReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PublishedMocImages.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate publishedMocImageId
    //
    if (!this.publishedMocImageId) {

      this.alertService.showMessage('No PublishedMocImage ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const publishedMocImageId = Number(this.publishedMocImageId);

    if (isNaN(publishedMocImageId) || publishedMocImageId <= 0) {

      this.alertService.showMessage(`Invalid Published Moc Image ID: "${this.publishedMocImageId}"`,
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
      // This is the most targeted way: clear only this PublishedMocImage + relations

      this.publishedMocImageService.ClearRecordCache(publishedMocImageId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.publishedMocImageService.GetPublishedMocImage(publishedMocImageId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (publishedMocImageData) => {

        //
        // Success path — publishedMocImageData can legitimately be null if 404'd but request succeeded
        //
        if (!publishedMocImageData) {

          this.handlePublishedMocImageNotFound(publishedMocImageId);

        } else {

          this.publishedMocImageData = publishedMocImageData;
          this.buildFormValues(this.publishedMocImageData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PublishedMocImage loaded successfully',
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
        this.handlePublishedMocImageLoadError(error, publishedMocImageId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePublishedMocImageNotFound(publishedMocImageId: number): void {

    this.publishedMocImageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PublishedMocImage #${publishedMocImageId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePublishedMocImageLoadError(error: any, publishedMocImageId: number): void {

    let message = 'Failed to load Published Moc Image.';
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
          message = 'You do not have permission to view this Published Moc Image.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Published Moc Image #${publishedMocImageId} was not found.`;
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

    console.error(`Published Moc Image load failed (ID: ${publishedMocImageId})`, error);

    //
    // Reset UI to safe state
    //
    this.publishedMocImageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(publishedMocImageData: PublishedMocImageData | null) {

    if (publishedMocImageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.publishedMocImageForm.reset({
        publishedMocId: null,
        imagePath: '',
        caption: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.publishedMocImageForm.reset({
        publishedMocId: publishedMocImageData.publishedMocId,
        imagePath: publishedMocImageData.imagePath ?? '',
        caption: publishedMocImageData.caption ?? '',
        sequence: publishedMocImageData.sequence?.toString() ?? '',
        active: publishedMocImageData.active ?? true,
        deleted: publishedMocImageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.publishedMocImageForm.markAsPristine();
    this.publishedMocImageForm.markAsUntouched();
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

    if (this.publishedMocImageService.userIsBMCPublishedMocImageWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Published Moc Images", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.publishedMocImageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.publishedMocImageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.publishedMocImageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const publishedMocImageSubmitData: PublishedMocImageSubmitData = {
        id: this.publishedMocImageData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        imagePath: formValue.imagePath!.trim(),
        caption: formValue.caption?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.publishedMocImageService.PutPublishedMocImage(publishedMocImageSubmitData.id, publishedMocImageSubmitData)
      : this.publishedMocImageService.PostPublishedMocImage(publishedMocImageSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPublishedMocImageData) => {

        this.publishedMocImageService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Published Moc Image's detail page
          //
          this.publishedMocImageForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.publishedMocImageForm.markAsUntouched();

          this.router.navigate(['/publishedmocimages', savedPublishedMocImageData.id]);
          this.alertService.showMessage('Published Moc Image added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.publishedMocImageData = savedPublishedMocImageData;
          this.buildFormValues(this.publishedMocImageData);

          this.alertService.showMessage("Published Moc Image saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Published Moc Image.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc Image.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc Image could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCPublishedMocImageReader(): boolean {
    return this.publishedMocImageService.userIsBMCPublishedMocImageReader();
  }

  public userIsBMCPublishedMocImageWriter(): boolean {
    return this.publishedMocImageService.userIsBMCPublishedMocImageWriter();
  }
}

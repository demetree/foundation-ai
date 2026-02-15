/*
   GENERATED FORM FOR THE PUBLISHEDMOC TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PublishedMoc table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to published-moc-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PublishedMocService, PublishedMocData, PublishedMocSubmitData } from '../../../bmc-data-services/published-moc.service';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { PublishedMocChangeHistoryService } from '../../../bmc-data-services/published-moc-change-history.service';
import { PublishedMocImageService } from '../../../bmc-data-services/published-moc-image.service';
import { MocLikeService } from '../../../bmc-data-services/moc-like.service';
import { MocCommentService } from '../../../bmc-data-services/moc-comment.service';
import { MocFavouriteService } from '../../../bmc-data-services/moc-favourite.service';
import { SharedInstructionService } from '../../../bmc-data-services/shared-instruction.service';
import { BuildChallengeEntryService } from '../../../bmc-data-services/build-challenge-entry.service';
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
interface PublishedMocFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  thumbnailImagePath: string | null,
  tags: string | null,
  isPublished: boolean,
  isFeatured: boolean,
  publishedDate: string | null,
  viewCount: string,     // Stored as string for form input, converted to number on submit.
  likeCount: string,     // Stored as string for form input, converted to number on submit.
  commentCount: string,     // Stored as string for form input, converted to number on submit.
  favouriteCount: string,     // Stored as string for form input, converted to number on submit.
  partCount: string | null,     // Stored as string for form input, converted to number on submit.
  allowForking: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-published-moc-detail',
  templateUrl: './published-moc-detail.component.html',
  styleUrls: ['./published-moc-detail.component.scss']
})

export class PublishedMocDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PublishedMocFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public publishedMocForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        thumbnailImagePath: [''],
        tags: [''],
        isPublished: [false],
        isFeatured: [false],
        publishedDate: [''],
        viewCount: ['', Validators.required],
        likeCount: ['', Validators.required],
        commentCount: ['', Validators.required],
        favouriteCount: ['', Validators.required],
        partCount: [''],
        allowForking: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public publishedMocId: string | null = null;
  public publishedMocData: PublishedMocData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  publishedMocs$ = this.publishedMocService.GetPublishedMocList();
  public projects$ = this.projectService.GetProjectList();
  public publishedMocChangeHistories$ = this.publishedMocChangeHistoryService.GetPublishedMocChangeHistoryList();
  public publishedMocImages$ = this.publishedMocImageService.GetPublishedMocImageList();
  public mocLikes$ = this.mocLikeService.GetMocLikeList();
  public mocComments$ = this.mocCommentService.GetMocCommentList();
  public mocFavourites$ = this.mocFavouriteService.GetMocFavouriteList();
  public sharedInstructions$ = this.sharedInstructionService.GetSharedInstructionList();
  public buildChallengeEntries$ = this.buildChallengeEntryService.GetBuildChallengeEntryList();

  private destroy$ = new Subject<void>();

  constructor(
    public publishedMocService: PublishedMocService,
    public projectService: ProjectService,
    public publishedMocChangeHistoryService: PublishedMocChangeHistoryService,
    public publishedMocImageService: PublishedMocImageService,
    public mocLikeService: MocLikeService,
    public mocCommentService: MocCommentService,
    public mocFavouriteService: MocFavouriteService,
    public sharedInstructionService: SharedInstructionService,
    public buildChallengeEntryService: BuildChallengeEntryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the publishedMocId from the route parameters
    this.publishedMocId = this.route.snapshot.paramMap.get('publishedMocId');

    if (this.publishedMocId === 'new' ||
        this.publishedMocId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.publishedMocData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.publishedMocForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.publishedMocForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Published Moc';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Published Moc';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.publishedMocForm.dirty) {
      return confirm('You have unsaved Published Moc changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.publishedMocId != null && this.publishedMocId !== 'new') {

      const id = parseInt(this.publishedMocId, 10);

      if (!isNaN(id)) {
        return { publishedMocId: id };
      }
    }

    return null;
  }


/*
  * Loads the PublishedMoc data for the current publishedMocId.
  *
  * Fully respects the PublishedMocService caching strategy and error handling strategy.
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
    if (!this.publishedMocService.userIsBMCPublishedMocReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PublishedMocs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate publishedMocId
    //
    if (!this.publishedMocId) {

      this.alertService.showMessage('No PublishedMoc ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const publishedMocId = Number(this.publishedMocId);

    if (isNaN(publishedMocId) || publishedMocId <= 0) {

      this.alertService.showMessage(`Invalid Published Moc ID: "${this.publishedMocId}"`,
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
      // This is the most targeted way: clear only this PublishedMoc + relations

      this.publishedMocService.ClearRecordCache(publishedMocId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.publishedMocService.GetPublishedMoc(publishedMocId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (publishedMocData) => {

        //
        // Success path — publishedMocData can legitimately be null if 404'd but request succeeded
        //
        if (!publishedMocData) {

          this.handlePublishedMocNotFound(publishedMocId);

        } else {

          this.publishedMocData = publishedMocData;
          this.buildFormValues(this.publishedMocData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PublishedMoc loaded successfully',
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
        this.handlePublishedMocLoadError(error, publishedMocId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePublishedMocNotFound(publishedMocId: number): void {

    this.publishedMocData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PublishedMoc #${publishedMocId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePublishedMocLoadError(error: any, publishedMocId: number): void {

    let message = 'Failed to load Published Moc.';
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
          message = 'You do not have permission to view this Published Moc.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Published Moc #${publishedMocId} was not found.`;
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

    console.error(`Published Moc load failed (ID: ${publishedMocId})`, error);

    //
    // Reset UI to safe state
    //
    this.publishedMocData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(publishedMocData: PublishedMocData | null) {

    if (publishedMocData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.publishedMocForm.reset({
        projectId: null,
        name: '',
        description: '',
        thumbnailImagePath: '',
        tags: '',
        isPublished: false,
        isFeatured: false,
        publishedDate: '',
        viewCount: '',
        likeCount: '',
        commentCount: '',
        favouriteCount: '',
        partCount: '',
        allowForking: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.publishedMocForm.reset({
        projectId: publishedMocData.projectId,
        name: publishedMocData.name ?? '',
        description: publishedMocData.description ?? '',
        thumbnailImagePath: publishedMocData.thumbnailImagePath ?? '',
        tags: publishedMocData.tags ?? '',
        isPublished: publishedMocData.isPublished ?? false,
        isFeatured: publishedMocData.isFeatured ?? false,
        publishedDate: isoUtcStringToDateTimeLocal(publishedMocData.publishedDate) ?? '',
        viewCount: publishedMocData.viewCount?.toString() ?? '',
        likeCount: publishedMocData.likeCount?.toString() ?? '',
        commentCount: publishedMocData.commentCount?.toString() ?? '',
        favouriteCount: publishedMocData.favouriteCount?.toString() ?? '',
        partCount: publishedMocData.partCount?.toString() ?? '',
        allowForking: publishedMocData.allowForking ?? false,
        versionNumber: publishedMocData.versionNumber?.toString() ?? '',
        active: publishedMocData.active ?? true,
        deleted: publishedMocData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.publishedMocForm.markAsPristine();
    this.publishedMocForm.markAsUntouched();
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

    if (this.publishedMocService.userIsBMCPublishedMocWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Published Mocs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.publishedMocForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.publishedMocForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.publishedMocForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const publishedMocSubmitData: PublishedMocSubmitData = {
        id: this.publishedMocData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        thumbnailImagePath: formValue.thumbnailImagePath?.trim() || null,
        tags: formValue.tags?.trim() || null,
        isPublished: !!formValue.isPublished,
        isFeatured: !!formValue.isFeatured,
        publishedDate: formValue.publishedDate ? dateTimeLocalToIsoUtc(formValue.publishedDate.trim()) : null,
        viewCount: Number(formValue.viewCount),
        likeCount: Number(formValue.likeCount),
        commentCount: Number(formValue.commentCount),
        favouriteCount: Number(formValue.favouriteCount),
        partCount: formValue.partCount ? Number(formValue.partCount) : null,
        allowForking: !!formValue.allowForking,
        versionNumber: this.publishedMocData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.publishedMocService.PutPublishedMoc(publishedMocSubmitData.id, publishedMocSubmitData)
      : this.publishedMocService.PostPublishedMoc(publishedMocSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPublishedMocData) => {

        this.publishedMocService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Published Moc's detail page
          //
          this.publishedMocForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.publishedMocForm.markAsUntouched();

          this.router.navigate(['/publishedmocs', savedPublishedMocData.id]);
          this.alertService.showMessage('Published Moc added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.publishedMocData = savedPublishedMocData;
          this.buildFormValues(this.publishedMocData);

          this.alertService.showMessage("Published Moc saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Published Moc.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCPublishedMocReader(): boolean {
    return this.publishedMocService.userIsBMCPublishedMocReader();
  }

  public userIsBMCPublishedMocWriter(): boolean {
    return this.publishedMocService.userIsBMCPublishedMocWriter();
  }
}

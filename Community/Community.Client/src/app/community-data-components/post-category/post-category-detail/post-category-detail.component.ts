/*
   GENERATED FORM FOR THE POSTCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PostCategory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to post-category-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PostCategoryService, PostCategoryData, PostCategorySubmitData } from '../../../community-data-services/post-category.service';
import { PostService } from '../../../community-data-services/post.service';
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
interface PostCategoryFormValues {
};


@Component({
  selector: 'app-post-category-detail',
  templateUrl: './post-category-detail.component.html',
  styleUrls: ['./post-category-detail.component.scss']
})

export class PostCategoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PostCategoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public postCategoryForm: FormGroup = this.fb.group({
      });


  public postCategoryId: string | null = null;
  public postCategoryData: PostCategoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  postCategories$ = this.postCategoryService.GetPostCategoryList();
  public posts$ = this.postService.GetPostList();

  private destroy$ = new Subject<void>();

  constructor(
    public postCategoryService: PostCategoryService,
    public postService: PostService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the postCategoryId from the route parameters
    this.postCategoryId = this.route.snapshot.paramMap.get('postCategoryId');

    if (this.postCategoryId === 'new' ||
        this.postCategoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.postCategoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.postCategoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.postCategoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Post Category';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Post Category';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.postCategoryForm.dirty) {
      return confirm('You have unsaved Post Category changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.postCategoryId != null && this.postCategoryId !== 'new') {

      const id = parseInt(this.postCategoryId, 10);

      if (!isNaN(id)) {
        return { postCategoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the PostCategory data for the current postCategoryId.
  *
  * Fully respects the PostCategoryService caching strategy and error handling strategy.
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
    if (!this.postCategoryService.userIsCommunityPostCategoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PostCategories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate postCategoryId
    //
    if (!this.postCategoryId) {

      this.alertService.showMessage('No PostCategory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const postCategoryId = Number(this.postCategoryId);

    if (isNaN(postCategoryId) || postCategoryId <= 0) {

      this.alertService.showMessage(`Invalid Post Category ID: "${this.postCategoryId}"`,
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
      // This is the most targeted way: clear only this PostCategory + relations

      this.postCategoryService.ClearRecordCache(postCategoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.postCategoryService.GetPostCategory(postCategoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (postCategoryData) => {

        //
        // Success path — postCategoryData can legitimately be null if 404'd but request succeeded
        //
        if (!postCategoryData) {

          this.handlePostCategoryNotFound(postCategoryId);

        } else {

          this.postCategoryData = postCategoryData;
          this.buildFormValues(this.postCategoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PostCategory loaded successfully',
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
        this.handlePostCategoryLoadError(error, postCategoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePostCategoryNotFound(postCategoryId: number): void {

    this.postCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PostCategory #${postCategoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePostCategoryLoadError(error: any, postCategoryId: number): void {

    let message = 'Failed to load Post Category.';
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
          message = 'You do not have permission to view this Post Category.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Post Category #${postCategoryId} was not found.`;
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

    console.error(`Post Category load failed (ID: ${postCategoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.postCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(postCategoryData: PostCategoryData | null) {

    if (postCategoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.postCategoryForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.postCategoryForm.reset({
      }, { emitEvent: false});
    }

    this.postCategoryForm.markAsPristine();
    this.postCategoryForm.markAsUntouched();
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

    if (this.postCategoryService.userIsCommunityPostCategoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Post Categories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.postCategoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.postCategoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.postCategoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const postCategorySubmitData: PostCategorySubmitData = {
        id: this.postCategoryData?.id || 0,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.postCategoryService.PutPostCategory(postCategorySubmitData.id, postCategorySubmitData)
      : this.postCategoryService.PostPostCategory(postCategorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPostCategoryData) => {

        this.postCategoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Post Category's detail page
          //
          this.postCategoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.postCategoryForm.markAsUntouched();

          this.router.navigate(['/postcategories', savedPostCategoryData.id]);
          this.alertService.showMessage('Post Category added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.postCategoryData = savedPostCategoryData;
          this.buildFormValues(this.postCategoryData);

          this.alertService.showMessage("Post Category saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Post Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityPostCategoryReader(): boolean {
    return this.postCategoryService.userIsCommunityPostCategoryReader();
  }

  public userIsCommunityPostCategoryWriter(): boolean {
    return this.postCategoryService.userIsCommunityPostCategoryWriter();
  }
}

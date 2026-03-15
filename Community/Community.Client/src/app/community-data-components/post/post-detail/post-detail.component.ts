/*
   GENERATED FORM FOR THE POST TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Post table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to post-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PostService, PostData, PostSubmitData } from '../../../community-data-services/post.service';
import { PostCategoryService } from '../../../community-data-services/post-category.service';
import { PostChangeHistoryService } from '../../../community-data-services/post-change-history.service';
import { PostTagAssignmentService } from '../../../community-data-services/post-tag-assignment.service';
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
interface PostFormValues {
};


@Component({
  selector: 'app-post-detail',
  templateUrl: './post-detail.component.html',
  styleUrls: ['./post-detail.component.scss']
})

export class PostDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PostFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public postForm: FormGroup = this.fb.group({
      });


  public postId: string | null = null;
  public postData: PostData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  posts$ = this.postService.GetPostList();
  public postCategories$ = this.postCategoryService.GetPostCategoryList();
  public postChangeHistories$ = this.postChangeHistoryService.GetPostChangeHistoryList();
  public postTagAssignments$ = this.postTagAssignmentService.GetPostTagAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public postService: PostService,
    public postCategoryService: PostCategoryService,
    public postChangeHistoryService: PostChangeHistoryService,
    public postTagAssignmentService: PostTagAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the postId from the route parameters
    this.postId = this.route.snapshot.paramMap.get('postId');

    if (this.postId === 'new' ||
        this.postId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.postData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.postForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.postForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Post';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Post';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.postForm.dirty) {
      return confirm('You have unsaved Post changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.postId != null && this.postId !== 'new') {

      const id = parseInt(this.postId, 10);

      if (!isNaN(id)) {
        return { postId: id };
      }
    }

    return null;
  }


/*
  * Loads the Post data for the current postId.
  *
  * Fully respects the PostService caching strategy and error handling strategy.
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
    if (!this.postService.userIsCommunityPostReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Posts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate postId
    //
    if (!this.postId) {

      this.alertService.showMessage('No Post ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const postId = Number(this.postId);

    if (isNaN(postId) || postId <= 0) {

      this.alertService.showMessage(`Invalid Post ID: "${this.postId}"`,
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
      // This is the most targeted way: clear only this Post + relations

      this.postService.ClearRecordCache(postId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.postService.GetPost(postId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (postData) => {

        //
        // Success path — postData can legitimately be null if 404'd but request succeeded
        //
        if (!postData) {

          this.handlePostNotFound(postId);

        } else {

          this.postData = postData;
          this.buildFormValues(this.postData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Post loaded successfully',
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
        this.handlePostLoadError(error, postId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePostNotFound(postId: number): void {

    this.postData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Post #${postId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePostLoadError(error: any, postId: number): void {

    let message = 'Failed to load Post.';
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
          message = 'You do not have permission to view this Post.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Post #${postId} was not found.`;
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

    console.error(`Post load failed (ID: ${postId})`, error);

    //
    // Reset UI to safe state
    //
    this.postData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(postData: PostData | null) {

    if (postData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.postForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.postForm.reset({
      }, { emitEvent: false});
    }

    this.postForm.markAsPristine();
    this.postForm.markAsUntouched();
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

    if (this.postService.userIsCommunityPostWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Posts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.postForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.postForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.postForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const postSubmitData: PostSubmitData = {
        id: this.postData?.id || 0,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.postService.PutPost(postSubmitData.id, postSubmitData)
      : this.postService.PostPost(postSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPostData) => {

        this.postService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Post's detail page
          //
          this.postForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.postForm.markAsUntouched();

          this.router.navigate(['/posts', savedPostData.id]);
          this.alertService.showMessage('Post added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.postData = savedPostData;
          this.buildFormValues(this.postData);

          this.alertService.showMessage("Post saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Post.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityPostReader(): boolean {
    return this.postService.userIsCommunityPostReader();
  }

  public userIsCommunityPostWriter(): boolean {
    return this.postService.userIsCommunityPostWriter();
  }
}

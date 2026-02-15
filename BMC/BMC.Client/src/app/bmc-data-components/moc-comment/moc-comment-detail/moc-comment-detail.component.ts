/*
   GENERATED FORM FOR THE MOCCOMMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocComment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-comment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocCommentService, MocCommentData, MocCommentSubmitData } from '../../../bmc-data-services/moc-comment.service';
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
interface MocCommentFormValues {
  publishedMocId: number | bigint,       // For FK link number
  commenterTenantGuid: string,
  commentText: string,
  postedDate: string,
  mocCommentId: number | bigint | null,       // For FK link number
  isEdited: boolean,
  isHidden: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-moc-comment-detail',
  templateUrl: './moc-comment-detail.component.html',
  styleUrls: ['./moc-comment-detail.component.scss']
})

export class MocCommentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocCommentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocCommentForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        commenterTenantGuid: ['', Validators.required],
        commentText: ['', Validators.required],
        postedDate: ['', Validators.required],
        mocCommentId: [null],
        isEdited: [false],
        isHidden: [false],
        active: [true],
        deleted: [false],
      });


  public mocCommentId: string | null = null;
  public mocCommentData: MocCommentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  mocComments$ = this.mocCommentService.GetMocCommentList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  private destroy$ = new Subject<void>();

  constructor(
    public mocCommentService: MocCommentService,
    public publishedMocService: PublishedMocService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the mocCommentId from the route parameters
    this.mocCommentId = this.route.snapshot.paramMap.get('mocCommentId');

    if (this.mocCommentId === 'new' ||
        this.mocCommentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.mocCommentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.mocCommentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocCommentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Moc Comment';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Moc Comment';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.mocCommentForm.dirty) {
      return confirm('You have unsaved Moc Comment changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.mocCommentId != null && this.mocCommentId !== 'new') {

      const id = parseInt(this.mocCommentId, 10);

      if (!isNaN(id)) {
        return { mocCommentId: id };
      }
    }

    return null;
  }


/*
  * Loads the MocComment data for the current mocCommentId.
  *
  * Fully respects the MocCommentService caching strategy and error handling strategy.
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
    if (!this.mocCommentService.userIsBMCMocCommentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MocComments.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate mocCommentId
    //
    if (!this.mocCommentId) {

      this.alertService.showMessage('No MocComment ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const mocCommentId = Number(this.mocCommentId);

    if (isNaN(mocCommentId) || mocCommentId <= 0) {

      this.alertService.showMessage(`Invalid Moc Comment ID: "${this.mocCommentId}"`,
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
      // This is the most targeted way: clear only this MocComment + relations

      this.mocCommentService.ClearRecordCache(mocCommentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.mocCommentService.GetMocComment(mocCommentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (mocCommentData) => {

        //
        // Success path — mocCommentData can legitimately be null if 404'd but request succeeded
        //
        if (!mocCommentData) {

          this.handleMocCommentNotFound(mocCommentId);

        } else {

          this.mocCommentData = mocCommentData;
          this.buildFormValues(this.mocCommentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MocComment loaded successfully',
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
        this.handleMocCommentLoadError(error, mocCommentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMocCommentNotFound(mocCommentId: number): void {

    this.mocCommentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MocComment #${mocCommentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMocCommentLoadError(error: any, mocCommentId: number): void {

    let message = 'Failed to load Moc Comment.';
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
          message = 'You do not have permission to view this Moc Comment.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Moc Comment #${mocCommentId} was not found.`;
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

    console.error(`Moc Comment load failed (ID: ${mocCommentId})`, error);

    //
    // Reset UI to safe state
    //
    this.mocCommentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(mocCommentData: MocCommentData | null) {

    if (mocCommentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocCommentForm.reset({
        publishedMocId: null,
        commenterTenantGuid: '',
        commentText: '',
        postedDate: '',
        mocCommentId: null,
        isEdited: false,
        isHidden: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocCommentForm.reset({
        publishedMocId: mocCommentData.publishedMocId,
        commenterTenantGuid: mocCommentData.commenterTenantGuid ?? '',
        commentText: mocCommentData.commentText ?? '',
        postedDate: isoUtcStringToDateTimeLocal(mocCommentData.postedDate) ?? '',
        mocCommentId: mocCommentData.mocCommentId,
        isEdited: mocCommentData.isEdited ?? false,
        isHidden: mocCommentData.isHidden ?? false,
        active: mocCommentData.active ?? true,
        deleted: mocCommentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocCommentForm.markAsPristine();
    this.mocCommentForm.markAsUntouched();
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

    if (this.mocCommentService.userIsBMCMocCommentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Moc Comments", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.mocCommentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocCommentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocCommentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocCommentSubmitData: MocCommentSubmitData = {
        id: this.mocCommentData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        commenterTenantGuid: formValue.commenterTenantGuid!.trim(),
        commentText: formValue.commentText!.trim(),
        postedDate: dateTimeLocalToIsoUtc(formValue.postedDate!.trim())!,
        mocCommentId: formValue.mocCommentId ? Number(formValue.mocCommentId) : null,
        isEdited: !!formValue.isEdited,
        isHidden: !!formValue.isHidden,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.mocCommentService.PutMocComment(mocCommentSubmitData.id, mocCommentSubmitData)
      : this.mocCommentService.PostMocComment(mocCommentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMocCommentData) => {

        this.mocCommentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Moc Comment's detail page
          //
          this.mocCommentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.mocCommentForm.markAsUntouched();

          this.router.navigate(['/moccomments', savedMocCommentData.id]);
          this.alertService.showMessage('Moc Comment added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.mocCommentData = savedMocCommentData;
          this.buildFormValues(this.mocCommentData);

          this.alertService.showMessage("Moc Comment saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Moc Comment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Comment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Comment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCMocCommentReader(): boolean {
    return this.mocCommentService.userIsBMCMocCommentReader();
  }

  public userIsBMCMocCommentWriter(): boolean {
    return this.mocCommentService.userIsBMCMocCommentWriter();
  }
}

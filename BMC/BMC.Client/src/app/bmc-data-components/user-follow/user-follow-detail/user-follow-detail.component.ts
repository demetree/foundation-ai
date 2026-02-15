/*
   GENERATED FORM FOR THE USERFOLLOW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserFollow table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-follow-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserFollowService, UserFollowData, UserFollowSubmitData } from '../../../bmc-data-services/user-follow.service';
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
interface UserFollowFormValues {
  followerTenantGuid: string,
  followedTenantGuid: string,
  followedDate: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-follow-detail',
  templateUrl: './user-follow-detail.component.html',
  styleUrls: ['./user-follow-detail.component.scss']
})

export class UserFollowDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserFollowFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userFollowForm: FormGroup = this.fb.group({
        followerTenantGuid: ['', Validators.required],
        followedTenantGuid: ['', Validators.required],
        followedDate: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public userFollowId: string | null = null;
  public userFollowData: UserFollowData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userFollows$ = this.userFollowService.GetUserFollowList();

  private destroy$ = new Subject<void>();

  constructor(
    public userFollowService: UserFollowService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userFollowId from the route parameters
    this.userFollowId = this.route.snapshot.paramMap.get('userFollowId');

    if (this.userFollowId === 'new' ||
        this.userFollowId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userFollowData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userFollowForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userFollowForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Follow';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Follow';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userFollowForm.dirty) {
      return confirm('You have unsaved User Follow changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userFollowId != null && this.userFollowId !== 'new') {

      const id = parseInt(this.userFollowId, 10);

      if (!isNaN(id)) {
        return { userFollowId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserFollow data for the current userFollowId.
  *
  * Fully respects the UserFollowService caching strategy and error handling strategy.
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
    if (!this.userFollowService.userIsBMCUserFollowReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserFollows.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userFollowId
    //
    if (!this.userFollowId) {

      this.alertService.showMessage('No UserFollow ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userFollowId = Number(this.userFollowId);

    if (isNaN(userFollowId) || userFollowId <= 0) {

      this.alertService.showMessage(`Invalid User Follow ID: "${this.userFollowId}"`,
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
      // This is the most targeted way: clear only this UserFollow + relations

      this.userFollowService.ClearRecordCache(userFollowId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userFollowService.GetUserFollow(userFollowId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userFollowData) => {

        //
        // Success path — userFollowData can legitimately be null if 404'd but request succeeded
        //
        if (!userFollowData) {

          this.handleUserFollowNotFound(userFollowId);

        } else {

          this.userFollowData = userFollowData;
          this.buildFormValues(this.userFollowData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserFollow loaded successfully',
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
        this.handleUserFollowLoadError(error, userFollowId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserFollowNotFound(userFollowId: number): void {

    this.userFollowData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserFollow #${userFollowId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserFollowLoadError(error: any, userFollowId: number): void {

    let message = 'Failed to load User Follow.';
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
          message = 'You do not have permission to view this User Follow.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Follow #${userFollowId} was not found.`;
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

    console.error(`User Follow load failed (ID: ${userFollowId})`, error);

    //
    // Reset UI to safe state
    //
    this.userFollowData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userFollowData: UserFollowData | null) {

    if (userFollowData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userFollowForm.reset({
        followerTenantGuid: '',
        followedTenantGuid: '',
        followedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userFollowForm.reset({
        followerTenantGuid: userFollowData.followerTenantGuid ?? '',
        followedTenantGuid: userFollowData.followedTenantGuid ?? '',
        followedDate: isoUtcStringToDateTimeLocal(userFollowData.followedDate) ?? '',
        active: userFollowData.active ?? true,
        deleted: userFollowData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userFollowForm.markAsPristine();
    this.userFollowForm.markAsUntouched();
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

    if (this.userFollowService.userIsBMCUserFollowWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Follows", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userFollowForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userFollowForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userFollowForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userFollowSubmitData: UserFollowSubmitData = {
        id: this.userFollowData?.id || 0,
        followerTenantGuid: formValue.followerTenantGuid!.trim(),
        followedTenantGuid: formValue.followedTenantGuid!.trim(),
        followedDate: dateTimeLocalToIsoUtc(formValue.followedDate!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userFollowService.PutUserFollow(userFollowSubmitData.id, userFollowSubmitData)
      : this.userFollowService.PostUserFollow(userFollowSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserFollowData) => {

        this.userFollowService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Follow's detail page
          //
          this.userFollowForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userFollowForm.markAsUntouched();

          this.router.navigate(['/userfollows', savedUserFollowData.id]);
          this.alertService.showMessage('User Follow added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userFollowData = savedUserFollowData;
          this.buildFormValues(this.userFollowData);

          this.alertService.showMessage("User Follow saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Follow.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Follow.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Follow could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserFollowReader(): boolean {
    return this.userFollowService.userIsBMCUserFollowReader();
  }

  public userIsBMCUserFollowWriter(): boolean {
    return this.userFollowService.userIsBMCUserFollowWriter();
  }
}

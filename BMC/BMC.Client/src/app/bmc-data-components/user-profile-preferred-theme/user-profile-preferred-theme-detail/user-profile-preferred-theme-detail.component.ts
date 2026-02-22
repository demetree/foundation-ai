/*
   GENERATED FORM FOR THE USERPROFILEPREFERREDTHEME TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfilePreferredTheme table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-preferred-theme-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfilePreferredThemeService, UserProfilePreferredThemeData, UserProfilePreferredThemeSubmitData } from '../../../bmc-data-services/user-profile-preferred-theme.service';
import { UserProfileService } from '../../../bmc-data-services/user-profile.service';
import { LegoThemeService } from '../../../bmc-data-services/lego-theme.service';
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
interface UserProfilePreferredThemeFormValues {
  userProfileId: number | bigint,       // For FK link number
  legoThemeId: number | bigint,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-profile-preferred-theme-detail',
  templateUrl: './user-profile-preferred-theme-detail.component.html',
  styleUrls: ['./user-profile-preferred-theme-detail.component.scss']
})

export class UserProfilePreferredThemeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfilePreferredThemeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfilePreferredThemeForm: FormGroup = this.fb.group({
        userProfileId: [null, Validators.required],
        legoThemeId: [null, Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public userProfilePreferredThemeId: string | null = null;
  public userProfilePreferredThemeData: UserProfilePreferredThemeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userProfilePreferredThemes$ = this.userProfilePreferredThemeService.GetUserProfilePreferredThemeList();
  public userProfiles$ = this.userProfileService.GetUserProfileList();
  public legoThemes$ = this.legoThemeService.GetLegoThemeList();

  private destroy$ = new Subject<void>();

  constructor(
    public userProfilePreferredThemeService: UserProfilePreferredThemeService,
    public userProfileService: UserProfileService,
    public legoThemeService: LegoThemeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userProfilePreferredThemeId from the route parameters
    this.userProfilePreferredThemeId = this.route.snapshot.paramMap.get('userProfilePreferredThemeId');

    if (this.userProfilePreferredThemeId === 'new' ||
        this.userProfilePreferredThemeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userProfilePreferredThemeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userProfilePreferredThemeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfilePreferredThemeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Profile Preferred Theme';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Profile Preferred Theme';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userProfilePreferredThemeForm.dirty) {
      return confirm('You have unsaved User Profile Preferred Theme changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userProfilePreferredThemeId != null && this.userProfilePreferredThemeId !== 'new') {

      const id = parseInt(this.userProfilePreferredThemeId, 10);

      if (!isNaN(id)) {
        return { userProfilePreferredThemeId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserProfilePreferredTheme data for the current userProfilePreferredThemeId.
  *
  * Fully respects the UserProfilePreferredThemeService caching strategy and error handling strategy.
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
    if (!this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserProfilePreferredThemes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userProfilePreferredThemeId
    //
    if (!this.userProfilePreferredThemeId) {

      this.alertService.showMessage('No UserProfilePreferredTheme ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userProfilePreferredThemeId = Number(this.userProfilePreferredThemeId);

    if (isNaN(userProfilePreferredThemeId) || userProfilePreferredThemeId <= 0) {

      this.alertService.showMessage(`Invalid User Profile Preferred Theme ID: "${this.userProfilePreferredThemeId}"`,
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
      // This is the most targeted way: clear only this UserProfilePreferredTheme + relations

      this.userProfilePreferredThemeService.ClearRecordCache(userProfilePreferredThemeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userProfilePreferredThemeService.GetUserProfilePreferredTheme(userProfilePreferredThemeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userProfilePreferredThemeData) => {

        //
        // Success path — userProfilePreferredThemeData can legitimately be null if 404'd but request succeeded
        //
        if (!userProfilePreferredThemeData) {

          this.handleUserProfilePreferredThemeNotFound(userProfilePreferredThemeId);

        } else {

          this.userProfilePreferredThemeData = userProfilePreferredThemeData;
          this.buildFormValues(this.userProfilePreferredThemeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserProfilePreferredTheme loaded successfully',
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
        this.handleUserProfilePreferredThemeLoadError(error, userProfilePreferredThemeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserProfilePreferredThemeNotFound(userProfilePreferredThemeId: number): void {

    this.userProfilePreferredThemeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserProfilePreferredTheme #${userProfilePreferredThemeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserProfilePreferredThemeLoadError(error: any, userProfilePreferredThemeId: number): void {

    let message = 'Failed to load User Profile Preferred Theme.';
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
          message = 'You do not have permission to view this User Profile Preferred Theme.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Profile Preferred Theme #${userProfilePreferredThemeId} was not found.`;
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

    console.error(`User Profile Preferred Theme load failed (ID: ${userProfilePreferredThemeId})`, error);

    //
    // Reset UI to safe state
    //
    this.userProfilePreferredThemeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userProfilePreferredThemeData: UserProfilePreferredThemeData | null) {

    if (userProfilePreferredThemeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfilePreferredThemeForm.reset({
        userProfileId: null,
        legoThemeId: null,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfilePreferredThemeForm.reset({
        userProfileId: userProfilePreferredThemeData.userProfileId,
        legoThemeId: userProfilePreferredThemeData.legoThemeId,
        sequence: userProfilePreferredThemeData.sequence?.toString() ?? '',
        active: userProfilePreferredThemeData.active ?? true,
        deleted: userProfilePreferredThemeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfilePreferredThemeForm.markAsPristine();
    this.userProfilePreferredThemeForm.markAsUntouched();
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

    if (this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Profile Preferred Themes", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userProfilePreferredThemeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfilePreferredThemeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfilePreferredThemeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfilePreferredThemeSubmitData: UserProfilePreferredThemeSubmitData = {
        id: this.userProfilePreferredThemeData?.id || 0,
        userProfileId: Number(formValue.userProfileId),
        legoThemeId: Number(formValue.legoThemeId),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userProfilePreferredThemeService.PutUserProfilePreferredTheme(userProfilePreferredThemeSubmitData.id, userProfilePreferredThemeSubmitData)
      : this.userProfilePreferredThemeService.PostUserProfilePreferredTheme(userProfilePreferredThemeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserProfilePreferredThemeData) => {

        this.userProfilePreferredThemeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Profile Preferred Theme's detail page
          //
          this.userProfilePreferredThemeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userProfilePreferredThemeForm.markAsUntouched();

          this.router.navigate(['/userprofilepreferredthemes', savedUserProfilePreferredThemeData.id]);
          this.alertService.showMessage('User Profile Preferred Theme added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userProfilePreferredThemeData = savedUserProfilePreferredThemeData;
          this.buildFormValues(this.userProfilePreferredThemeData);

          this.alertService.showMessage("User Profile Preferred Theme saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Profile Preferred Theme.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Preferred Theme.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Preferred Theme could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserProfilePreferredThemeReader(): boolean {
    return this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeReader();
  }

  public userIsBMCUserProfilePreferredThemeWriter(): boolean {
    return this.userProfilePreferredThemeService.userIsBMCUserProfilePreferredThemeWriter();
  }
}

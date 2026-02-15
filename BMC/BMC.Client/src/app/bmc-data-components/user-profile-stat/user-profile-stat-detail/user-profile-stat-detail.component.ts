/*
   GENERATED FORM FOR THE USERPROFILESTAT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileStat table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-stat-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileStatService, UserProfileStatData, UserProfileStatSubmitData } from '../../../bmc-data-services/user-profile-stat.service';
import { UserProfileService } from '../../../bmc-data-services/user-profile.service';
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
interface UserProfileStatFormValues {
  userProfileId: number | bigint,       // For FK link number
  totalPartsOwned: string,     // Stored as string for form input, converted to number on submit.
  totalUniquePartsOwned: string,     // Stored as string for form input, converted to number on submit.
  totalSetsOwned: string,     // Stored as string for form input, converted to number on submit.
  totalMocsPublished: string,     // Stored as string for form input, converted to number on submit.
  totalFollowers: string,     // Stored as string for form input, converted to number on submit.
  totalFollowing: string,     // Stored as string for form input, converted to number on submit.
  totalLikesReceived: string,     // Stored as string for form input, converted to number on submit.
  totalAchievementPoints: string,     // Stored as string for form input, converted to number on submit.
  lastCalculatedDate: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-profile-stat-detail',
  templateUrl: './user-profile-stat-detail.component.html',
  styleUrls: ['./user-profile-stat-detail.component.scss']
})

export class UserProfileStatDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileStatFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileStatForm: FormGroup = this.fb.group({
        userProfileId: [null, Validators.required],
        totalPartsOwned: ['', Validators.required],
        totalUniquePartsOwned: ['', Validators.required],
        totalSetsOwned: ['', Validators.required],
        totalMocsPublished: ['', Validators.required],
        totalFollowers: ['', Validators.required],
        totalFollowing: ['', Validators.required],
        totalLikesReceived: ['', Validators.required],
        totalAchievementPoints: ['', Validators.required],
        lastCalculatedDate: [''],
        active: [true],
        deleted: [false],
      });


  public userProfileStatId: string | null = null;
  public userProfileStatData: UserProfileStatData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userProfileStats$ = this.userProfileStatService.GetUserProfileStatList();
  public userProfiles$ = this.userProfileService.GetUserProfileList();

  private destroy$ = new Subject<void>();

  constructor(
    public userProfileStatService: UserProfileStatService,
    public userProfileService: UserProfileService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userProfileStatId from the route parameters
    this.userProfileStatId = this.route.snapshot.paramMap.get('userProfileStatId');

    if (this.userProfileStatId === 'new' ||
        this.userProfileStatId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userProfileStatData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userProfileStatForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileStatForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Profile Stat';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Profile Stat';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userProfileStatForm.dirty) {
      return confirm('You have unsaved User Profile Stat changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userProfileStatId != null && this.userProfileStatId !== 'new') {

      const id = parseInt(this.userProfileStatId, 10);

      if (!isNaN(id)) {
        return { userProfileStatId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserProfileStat data for the current userProfileStatId.
  *
  * Fully respects the UserProfileStatService caching strategy and error handling strategy.
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
    if (!this.userProfileStatService.userIsBMCUserProfileStatReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserProfileStats.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userProfileStatId
    //
    if (!this.userProfileStatId) {

      this.alertService.showMessage('No UserProfileStat ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userProfileStatId = Number(this.userProfileStatId);

    if (isNaN(userProfileStatId) || userProfileStatId <= 0) {

      this.alertService.showMessage(`Invalid User Profile Stat ID: "${this.userProfileStatId}"`,
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
      // This is the most targeted way: clear only this UserProfileStat + relations

      this.userProfileStatService.ClearRecordCache(userProfileStatId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userProfileStatService.GetUserProfileStat(userProfileStatId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userProfileStatData) => {

        //
        // Success path — userProfileStatData can legitimately be null if 404'd but request succeeded
        //
        if (!userProfileStatData) {

          this.handleUserProfileStatNotFound(userProfileStatId);

        } else {

          this.userProfileStatData = userProfileStatData;
          this.buildFormValues(this.userProfileStatData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserProfileStat loaded successfully',
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
        this.handleUserProfileStatLoadError(error, userProfileStatId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserProfileStatNotFound(userProfileStatId: number): void {

    this.userProfileStatData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserProfileStat #${userProfileStatId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserProfileStatLoadError(error: any, userProfileStatId: number): void {

    let message = 'Failed to load User Profile Stat.';
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
          message = 'You do not have permission to view this User Profile Stat.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Profile Stat #${userProfileStatId} was not found.`;
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

    console.error(`User Profile Stat load failed (ID: ${userProfileStatId})`, error);

    //
    // Reset UI to safe state
    //
    this.userProfileStatData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userProfileStatData: UserProfileStatData | null) {

    if (userProfileStatData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileStatForm.reset({
        userProfileId: null,
        totalPartsOwned: '',
        totalUniquePartsOwned: '',
        totalSetsOwned: '',
        totalMocsPublished: '',
        totalFollowers: '',
        totalFollowing: '',
        totalLikesReceived: '',
        totalAchievementPoints: '',
        lastCalculatedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileStatForm.reset({
        userProfileId: userProfileStatData.userProfileId,
        totalPartsOwned: userProfileStatData.totalPartsOwned?.toString() ?? '',
        totalUniquePartsOwned: userProfileStatData.totalUniquePartsOwned?.toString() ?? '',
        totalSetsOwned: userProfileStatData.totalSetsOwned?.toString() ?? '',
        totalMocsPublished: userProfileStatData.totalMocsPublished?.toString() ?? '',
        totalFollowers: userProfileStatData.totalFollowers?.toString() ?? '',
        totalFollowing: userProfileStatData.totalFollowing?.toString() ?? '',
        totalLikesReceived: userProfileStatData.totalLikesReceived?.toString() ?? '',
        totalAchievementPoints: userProfileStatData.totalAchievementPoints?.toString() ?? '',
        lastCalculatedDate: isoUtcStringToDateTimeLocal(userProfileStatData.lastCalculatedDate) ?? '',
        active: userProfileStatData.active ?? true,
        deleted: userProfileStatData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileStatForm.markAsPristine();
    this.userProfileStatForm.markAsUntouched();
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

    if (this.userProfileStatService.userIsBMCUserProfileStatWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Profile Stats", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userProfileStatForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileStatForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileStatForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileStatSubmitData: UserProfileStatSubmitData = {
        id: this.userProfileStatData?.id || 0,
        userProfileId: Number(formValue.userProfileId),
        totalPartsOwned: Number(formValue.totalPartsOwned),
        totalUniquePartsOwned: Number(formValue.totalUniquePartsOwned),
        totalSetsOwned: Number(formValue.totalSetsOwned),
        totalMocsPublished: Number(formValue.totalMocsPublished),
        totalFollowers: Number(formValue.totalFollowers),
        totalFollowing: Number(formValue.totalFollowing),
        totalLikesReceived: Number(formValue.totalLikesReceived),
        totalAchievementPoints: Number(formValue.totalAchievementPoints),
        lastCalculatedDate: formValue.lastCalculatedDate ? dateTimeLocalToIsoUtc(formValue.lastCalculatedDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userProfileStatService.PutUserProfileStat(userProfileStatSubmitData.id, userProfileStatSubmitData)
      : this.userProfileStatService.PostUserProfileStat(userProfileStatSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserProfileStatData) => {

        this.userProfileStatService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Profile Stat's detail page
          //
          this.userProfileStatForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userProfileStatForm.markAsUntouched();

          this.router.navigate(['/userprofilestats', savedUserProfileStatData.id]);
          this.alertService.showMessage('User Profile Stat added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userProfileStatData = savedUserProfileStatData;
          this.buildFormValues(this.userProfileStatData);

          this.alertService.showMessage("User Profile Stat saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Profile Stat.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Stat.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Stat could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserProfileStatReader(): boolean {
    return this.userProfileStatService.userIsBMCUserProfileStatReader();
  }

  public userIsBMCUserProfileStatWriter(): boolean {
    return this.userProfileStatService.userIsBMCUserProfileStatWriter();
  }
}

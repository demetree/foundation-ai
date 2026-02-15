/*
   GENERATED FORM FOR THE USERPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileService, UserProfileData, UserProfileSubmitData } from '../../../bmc-data-services/user-profile.service';
import { UserProfileChangeHistoryService } from '../../../bmc-data-services/user-profile-change-history.service';
import { UserProfileLinkService } from '../../../bmc-data-services/user-profile-link.service';
import { UserProfileStatService } from '../../../bmc-data-services/user-profile-stat.service';
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
interface UserProfileFormValues {
  displayName: string,
  bio: string | null,
  location: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  bannerFileName: string | null,
  bannerSize: string | null,     // Stored as string for form input, converted to number on submit.
  bannerData: string | null,
  bannerMimeType: string | null,
  websiteUrl: string | null,
  isPublic: boolean,
  memberSinceDate: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-profile-detail',
  templateUrl: './user-profile-detail.component.html',
  styleUrls: ['./user-profile-detail.component.scss']
})

export class UserProfileDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileForm: FormGroup = this.fb.group({
        displayName: ['', Validators.required],
        bio: [''],
        location: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        bannerFileName: [''],
        bannerSize: [''],
        bannerData: [''],
        bannerMimeType: [''],
        websiteUrl: [''],
        isPublic: [false],
        memberSinceDate: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public userProfileId: string | null = null;
  public userProfileData: UserProfileData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userProfiles$ = this.userProfileService.GetUserProfileList();
  public userProfileChangeHistories$ = this.userProfileChangeHistoryService.GetUserProfileChangeHistoryList();
  public userProfileLinks$ = this.userProfileLinkService.GetUserProfileLinkList();
  public userProfileStats$ = this.userProfileStatService.GetUserProfileStatList();

  private destroy$ = new Subject<void>();

  constructor(
    public userProfileService: UserProfileService,
    public userProfileChangeHistoryService: UserProfileChangeHistoryService,
    public userProfileLinkService: UserProfileLinkService,
    public userProfileStatService: UserProfileStatService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userProfileId from the route parameters
    this.userProfileId = this.route.snapshot.paramMap.get('userProfileId');

    if (this.userProfileId === 'new' ||
        this.userProfileId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userProfileData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userProfileForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Profile';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Profile';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userProfileForm.dirty) {
      return confirm('You have unsaved User Profile changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userProfileId != null && this.userProfileId !== 'new') {

      const id = parseInt(this.userProfileId, 10);

      if (!isNaN(id)) {
        return { userProfileId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserProfile data for the current userProfileId.
  *
  * Fully respects the UserProfileService caching strategy and error handling strategy.
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
    if (!this.userProfileService.userIsBMCUserProfileReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserProfiles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userProfileId
    //
    if (!this.userProfileId) {

      this.alertService.showMessage('No UserProfile ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userProfileId = Number(this.userProfileId);

    if (isNaN(userProfileId) || userProfileId <= 0) {

      this.alertService.showMessage(`Invalid User Profile ID: "${this.userProfileId}"`,
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
      // This is the most targeted way: clear only this UserProfile + relations

      this.userProfileService.ClearRecordCache(userProfileId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userProfileService.GetUserProfile(userProfileId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userProfileData) => {

        //
        // Success path — userProfileData can legitimately be null if 404'd but request succeeded
        //
        if (!userProfileData) {

          this.handleUserProfileNotFound(userProfileId);

        } else {

          this.userProfileData = userProfileData;
          this.buildFormValues(this.userProfileData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserProfile loaded successfully',
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
        this.handleUserProfileLoadError(error, userProfileId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserProfileNotFound(userProfileId: number): void {

    this.userProfileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserProfile #${userProfileId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserProfileLoadError(error: any, userProfileId: number): void {

    let message = 'Failed to load User Profile.';
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
          message = 'You do not have permission to view this User Profile.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Profile #${userProfileId} was not found.`;
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

    console.error(`User Profile load failed (ID: ${userProfileId})`, error);

    //
    // Reset UI to safe state
    //
    this.userProfileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userProfileData: UserProfileData | null) {

    if (userProfileData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileForm.reset({
        displayName: '',
        bio: '',
        location: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        bannerFileName: '',
        bannerSize: '',
        bannerData: '',
        bannerMimeType: '',
        websiteUrl: '',
        isPublic: false,
        memberSinceDate: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileForm.reset({
        displayName: userProfileData.displayName ?? '',
        bio: userProfileData.bio ?? '',
        location: userProfileData.location ?? '',
        avatarFileName: userProfileData.avatarFileName ?? '',
        avatarSize: userProfileData.avatarSize?.toString() ?? '',
        avatarData: userProfileData.avatarData ?? '',
        avatarMimeType: userProfileData.avatarMimeType ?? '',
        bannerFileName: userProfileData.bannerFileName ?? '',
        bannerSize: userProfileData.bannerSize?.toString() ?? '',
        bannerData: userProfileData.bannerData ?? '',
        bannerMimeType: userProfileData.bannerMimeType ?? '',
        websiteUrl: userProfileData.websiteUrl ?? '',
        isPublic: userProfileData.isPublic ?? false,
        memberSinceDate: isoUtcStringToDateTimeLocal(userProfileData.memberSinceDate) ?? '',
        versionNumber: userProfileData.versionNumber?.toString() ?? '',
        active: userProfileData.active ?? true,
        deleted: userProfileData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileForm.markAsPristine();
    this.userProfileForm.markAsUntouched();
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

    if (this.userProfileService.userIsBMCUserProfileWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Profiles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userProfileForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileSubmitData: UserProfileSubmitData = {
        id: this.userProfileData?.id || 0,
        displayName: formValue.displayName!.trim(),
        bio: formValue.bio?.trim() || null,
        location: formValue.location?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        bannerFileName: formValue.bannerFileName?.trim() || null,
        bannerSize: formValue.bannerSize ? Number(formValue.bannerSize) : null,
        bannerData: formValue.bannerData?.trim() || null,
        bannerMimeType: formValue.bannerMimeType?.trim() || null,
        websiteUrl: formValue.websiteUrl?.trim() || null,
        isPublic: !!formValue.isPublic,
        memberSinceDate: formValue.memberSinceDate ? dateTimeLocalToIsoUtc(formValue.memberSinceDate.trim()) : null,
        versionNumber: this.userProfileData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userProfileService.PutUserProfile(userProfileSubmitData.id, userProfileSubmitData)
      : this.userProfileService.PostUserProfile(userProfileSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserProfileData) => {

        this.userProfileService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Profile's detail page
          //
          this.userProfileForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userProfileForm.markAsUntouched();

          this.router.navigate(['/userprofiles', savedUserProfileData.id]);
          this.alertService.showMessage('User Profile added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userProfileData = savedUserProfileData;
          this.buildFormValues(this.userProfileData);

          this.alertService.showMessage("User Profile saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Profile.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserProfileReader(): boolean {
    return this.userProfileService.userIsBMCUserProfileReader();
  }

  public userIsBMCUserProfileWriter(): boolean {
    return this.userProfileService.userIsBMCUserProfileWriter();
  }
}

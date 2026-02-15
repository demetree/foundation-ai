/*
   GENERATED FORM FOR THE USERPROFILELINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileLinkService, UserProfileLinkData, UserProfileLinkSubmitData } from '../../../bmc-data-services/user-profile-link.service';
import { UserProfileService } from '../../../bmc-data-services/user-profile.service';
import { UserProfileLinkTypeService } from '../../../bmc-data-services/user-profile-link-type.service';
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
interface UserProfileLinkFormValues {
  userProfileId: number | bigint,       // For FK link number
  userProfileLinkTypeId: number | bigint,       // For FK link number
  url: string,
  displayLabel: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-profile-link-detail',
  templateUrl: './user-profile-link-detail.component.html',
  styleUrls: ['./user-profile-link-detail.component.scss']
})

export class UserProfileLinkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileLinkForm: FormGroup = this.fb.group({
        userProfileId: [null, Validators.required],
        userProfileLinkTypeId: [null, Validators.required],
        url: ['', Validators.required],
        displayLabel: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public userProfileLinkId: string | null = null;
  public userProfileLinkData: UserProfileLinkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userProfileLinks$ = this.userProfileLinkService.GetUserProfileLinkList();
  public userProfiles$ = this.userProfileService.GetUserProfileList();
  public userProfileLinkTypes$ = this.userProfileLinkTypeService.GetUserProfileLinkTypeList();

  private destroy$ = new Subject<void>();

  constructor(
    public userProfileLinkService: UserProfileLinkService,
    public userProfileService: UserProfileService,
    public userProfileLinkTypeService: UserProfileLinkTypeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userProfileLinkId from the route parameters
    this.userProfileLinkId = this.route.snapshot.paramMap.get('userProfileLinkId');

    if (this.userProfileLinkId === 'new' ||
        this.userProfileLinkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userProfileLinkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userProfileLinkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Profile Link';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Profile Link';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userProfileLinkForm.dirty) {
      return confirm('You have unsaved User Profile Link changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userProfileLinkId != null && this.userProfileLinkId !== 'new') {

      const id = parseInt(this.userProfileLinkId, 10);

      if (!isNaN(id)) {
        return { userProfileLinkId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserProfileLink data for the current userProfileLinkId.
  *
  * Fully respects the UserProfileLinkService caching strategy and error handling strategy.
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
    if (!this.userProfileLinkService.userIsBMCUserProfileLinkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserProfileLinks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userProfileLinkId
    //
    if (!this.userProfileLinkId) {

      this.alertService.showMessage('No UserProfileLink ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userProfileLinkId = Number(this.userProfileLinkId);

    if (isNaN(userProfileLinkId) || userProfileLinkId <= 0) {

      this.alertService.showMessage(`Invalid User Profile Link ID: "${this.userProfileLinkId}"`,
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
      // This is the most targeted way: clear only this UserProfileLink + relations

      this.userProfileLinkService.ClearRecordCache(userProfileLinkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userProfileLinkService.GetUserProfileLink(userProfileLinkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userProfileLinkData) => {

        //
        // Success path — userProfileLinkData can legitimately be null if 404'd but request succeeded
        //
        if (!userProfileLinkData) {

          this.handleUserProfileLinkNotFound(userProfileLinkId);

        } else {

          this.userProfileLinkData = userProfileLinkData;
          this.buildFormValues(this.userProfileLinkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserProfileLink loaded successfully',
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
        this.handleUserProfileLinkLoadError(error, userProfileLinkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserProfileLinkNotFound(userProfileLinkId: number): void {

    this.userProfileLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserProfileLink #${userProfileLinkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserProfileLinkLoadError(error: any, userProfileLinkId: number): void {

    let message = 'Failed to load User Profile Link.';
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
          message = 'You do not have permission to view this User Profile Link.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Profile Link #${userProfileLinkId} was not found.`;
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

    console.error(`User Profile Link load failed (ID: ${userProfileLinkId})`, error);

    //
    // Reset UI to safe state
    //
    this.userProfileLinkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userProfileLinkData: UserProfileLinkData | null) {

    if (userProfileLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileLinkForm.reset({
        userProfileId: null,
        userProfileLinkTypeId: null,
        url: '',
        displayLabel: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileLinkForm.reset({
        userProfileId: userProfileLinkData.userProfileId,
        userProfileLinkTypeId: userProfileLinkData.userProfileLinkTypeId,
        url: userProfileLinkData.url ?? '',
        displayLabel: userProfileLinkData.displayLabel ?? '',
        sequence: userProfileLinkData.sequence?.toString() ?? '',
        active: userProfileLinkData.active ?? true,
        deleted: userProfileLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileLinkForm.markAsPristine();
    this.userProfileLinkForm.markAsUntouched();
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

    if (this.userProfileLinkService.userIsBMCUserProfileLinkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Profile Links", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userProfileLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileLinkSubmitData: UserProfileLinkSubmitData = {
        id: this.userProfileLinkData?.id || 0,
        userProfileId: Number(formValue.userProfileId),
        userProfileLinkTypeId: Number(formValue.userProfileLinkTypeId),
        url: formValue.url!.trim(),
        displayLabel: formValue.displayLabel?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userProfileLinkService.PutUserProfileLink(userProfileLinkSubmitData.id, userProfileLinkSubmitData)
      : this.userProfileLinkService.PostUserProfileLink(userProfileLinkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserProfileLinkData) => {

        this.userProfileLinkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Profile Link's detail page
          //
          this.userProfileLinkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userProfileLinkForm.markAsUntouched();

          this.router.navigate(['/userprofilelinks', savedUserProfileLinkData.id]);
          this.alertService.showMessage('User Profile Link added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userProfileLinkData = savedUserProfileLinkData;
          this.buildFormValues(this.userProfileLinkData);

          this.alertService.showMessage("User Profile Link saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Profile Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserProfileLinkReader(): boolean {
    return this.userProfileLinkService.userIsBMCUserProfileLinkReader();
  }

  public userIsBMCUserProfileLinkWriter(): boolean {
    return this.userProfileLinkService.userIsBMCUserProfileLinkWriter();
  }
}

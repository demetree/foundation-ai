/*
   GENERATED FORM FOR THE USERACHIEVEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserAchievement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-achievement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserAchievementService, UserAchievementData, UserAchievementSubmitData } from '../../../bmc-data-services/user-achievement.service';
import { AchievementService } from '../../../bmc-data-services/achievement.service';
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
interface UserAchievementFormValues {
  achievementId: number | bigint,       // For FK link number
  earnedDate: string,
  isDisplayed: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-achievement-detail',
  templateUrl: './user-achievement-detail.component.html',
  styleUrls: ['./user-achievement-detail.component.scss']
})

export class UserAchievementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserAchievementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userAchievementForm: FormGroup = this.fb.group({
        achievementId: [null, Validators.required],
        earnedDate: ['', Validators.required],
        isDisplayed: [false],
        active: [true],
        deleted: [false],
      });


  public userAchievementId: string | null = null;
  public userAchievementData: UserAchievementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userAchievements$ = this.userAchievementService.GetUserAchievementList();
  public achievements$ = this.achievementService.GetAchievementList();

  private destroy$ = new Subject<void>();

  constructor(
    public userAchievementService: UserAchievementService,
    public achievementService: AchievementService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userAchievementId from the route parameters
    this.userAchievementId = this.route.snapshot.paramMap.get('userAchievementId');

    if (this.userAchievementId === 'new' ||
        this.userAchievementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userAchievementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userAchievementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userAchievementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Achievement';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Achievement';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userAchievementForm.dirty) {
      return confirm('You have unsaved User Achievement changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userAchievementId != null && this.userAchievementId !== 'new') {

      const id = parseInt(this.userAchievementId, 10);

      if (!isNaN(id)) {
        return { userAchievementId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserAchievement data for the current userAchievementId.
  *
  * Fully respects the UserAchievementService caching strategy and error handling strategy.
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
    if (!this.userAchievementService.userIsBMCUserAchievementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserAchievements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userAchievementId
    //
    if (!this.userAchievementId) {

      this.alertService.showMessage('No UserAchievement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userAchievementId = Number(this.userAchievementId);

    if (isNaN(userAchievementId) || userAchievementId <= 0) {

      this.alertService.showMessage(`Invalid User Achievement ID: "${this.userAchievementId}"`,
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
      // This is the most targeted way: clear only this UserAchievement + relations

      this.userAchievementService.ClearRecordCache(userAchievementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userAchievementService.GetUserAchievement(userAchievementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userAchievementData) => {

        //
        // Success path — userAchievementData can legitimately be null if 404'd but request succeeded
        //
        if (!userAchievementData) {

          this.handleUserAchievementNotFound(userAchievementId);

        } else {

          this.userAchievementData = userAchievementData;
          this.buildFormValues(this.userAchievementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserAchievement loaded successfully',
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
        this.handleUserAchievementLoadError(error, userAchievementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserAchievementNotFound(userAchievementId: number): void {

    this.userAchievementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserAchievement #${userAchievementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserAchievementLoadError(error: any, userAchievementId: number): void {

    let message = 'Failed to load User Achievement.';
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
          message = 'You do not have permission to view this User Achievement.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Achievement #${userAchievementId} was not found.`;
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

    console.error(`User Achievement load failed (ID: ${userAchievementId})`, error);

    //
    // Reset UI to safe state
    //
    this.userAchievementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userAchievementData: UserAchievementData | null) {

    if (userAchievementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userAchievementForm.reset({
        achievementId: null,
        earnedDate: '',
        isDisplayed: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userAchievementForm.reset({
        achievementId: userAchievementData.achievementId,
        earnedDate: isoUtcStringToDateTimeLocal(userAchievementData.earnedDate) ?? '',
        isDisplayed: userAchievementData.isDisplayed ?? false,
        active: userAchievementData.active ?? true,
        deleted: userAchievementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userAchievementForm.markAsPristine();
    this.userAchievementForm.markAsUntouched();
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

    if (this.userAchievementService.userIsBMCUserAchievementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Achievements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userAchievementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userAchievementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userAchievementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userAchievementSubmitData: UserAchievementSubmitData = {
        id: this.userAchievementData?.id || 0,
        achievementId: Number(formValue.achievementId),
        earnedDate: dateTimeLocalToIsoUtc(formValue.earnedDate!.trim())!,
        isDisplayed: !!formValue.isDisplayed,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userAchievementService.PutUserAchievement(userAchievementSubmitData.id, userAchievementSubmitData)
      : this.userAchievementService.PostUserAchievement(userAchievementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserAchievementData) => {

        this.userAchievementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Achievement's detail page
          //
          this.userAchievementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userAchievementForm.markAsUntouched();

          this.router.navigate(['/userachievements', savedUserAchievementData.id]);
          this.alertService.showMessage('User Achievement added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userAchievementData = savedUserAchievementData;
          this.buildFormValues(this.userAchievementData);

          this.alertService.showMessage("User Achievement saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Achievement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Achievement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Achievement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserAchievementReader(): boolean {
    return this.userAchievementService.userIsBMCUserAchievementReader();
  }

  public userIsBMCUserAchievementWriter(): boolean {
    return this.userAchievementService.userIsBMCUserAchievementWriter();
  }
}

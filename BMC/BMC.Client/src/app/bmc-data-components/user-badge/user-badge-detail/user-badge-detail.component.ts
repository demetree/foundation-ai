/*
   GENERATED FORM FOR THE USERBADGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserBadge table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-badge-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserBadgeService, UserBadgeData, UserBadgeSubmitData } from '../../../bmc-data-services/user-badge.service';
import { UserBadgeAssignmentService } from '../../../bmc-data-services/user-badge-assignment.service';
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
interface UserBadgeFormValues {
  name: string,
  description: string,
  iconCssClass: string | null,
  iconImagePath: string | null,
  badgeColor: string | null,
  isAutomatic: boolean,
  automaticCriteriaCode: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-badge-detail',
  templateUrl: './user-badge-detail.component.html',
  styleUrls: ['./user-badge-detail.component.scss']
})

export class UserBadgeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserBadgeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userBadgeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        iconCssClass: [''],
        iconImagePath: [''],
        badgeColor: [''],
        isAutomatic: [false],
        automaticCriteriaCode: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public userBadgeId: string | null = null;
  public userBadgeData: UserBadgeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userBadges$ = this.userBadgeService.GetUserBadgeList();
  public userBadgeAssignments$ = this.userBadgeAssignmentService.GetUserBadgeAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public userBadgeService: UserBadgeService,
    public userBadgeAssignmentService: UserBadgeAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userBadgeId from the route parameters
    this.userBadgeId = this.route.snapshot.paramMap.get('userBadgeId');

    if (this.userBadgeId === 'new' ||
        this.userBadgeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userBadgeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userBadgeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userBadgeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Badge';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Badge';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userBadgeForm.dirty) {
      return confirm('You have unsaved User Badge changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userBadgeId != null && this.userBadgeId !== 'new') {

      const id = parseInt(this.userBadgeId, 10);

      if (!isNaN(id)) {
        return { userBadgeId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserBadge data for the current userBadgeId.
  *
  * Fully respects the UserBadgeService caching strategy and error handling strategy.
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
    if (!this.userBadgeService.userIsBMCUserBadgeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserBadges.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userBadgeId
    //
    if (!this.userBadgeId) {

      this.alertService.showMessage('No UserBadge ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userBadgeId = Number(this.userBadgeId);

    if (isNaN(userBadgeId) || userBadgeId <= 0) {

      this.alertService.showMessage(`Invalid User Badge ID: "${this.userBadgeId}"`,
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
      // This is the most targeted way: clear only this UserBadge + relations

      this.userBadgeService.ClearRecordCache(userBadgeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userBadgeService.GetUserBadge(userBadgeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userBadgeData) => {

        //
        // Success path — userBadgeData can legitimately be null if 404'd but request succeeded
        //
        if (!userBadgeData) {

          this.handleUserBadgeNotFound(userBadgeId);

        } else {

          this.userBadgeData = userBadgeData;
          this.buildFormValues(this.userBadgeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserBadge loaded successfully',
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
        this.handleUserBadgeLoadError(error, userBadgeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserBadgeNotFound(userBadgeId: number): void {

    this.userBadgeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserBadge #${userBadgeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserBadgeLoadError(error: any, userBadgeId: number): void {

    let message = 'Failed to load User Badge.';
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
          message = 'You do not have permission to view this User Badge.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Badge #${userBadgeId} was not found.`;
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

    console.error(`User Badge load failed (ID: ${userBadgeId})`, error);

    //
    // Reset UI to safe state
    //
    this.userBadgeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userBadgeData: UserBadgeData | null) {

    if (userBadgeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userBadgeForm.reset({
        name: '',
        description: '',
        iconCssClass: '',
        iconImagePath: '',
        badgeColor: '',
        isAutomatic: false,
        automaticCriteriaCode: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userBadgeForm.reset({
        name: userBadgeData.name ?? '',
        description: userBadgeData.description ?? '',
        iconCssClass: userBadgeData.iconCssClass ?? '',
        iconImagePath: userBadgeData.iconImagePath ?? '',
        badgeColor: userBadgeData.badgeColor ?? '',
        isAutomatic: userBadgeData.isAutomatic ?? false,
        automaticCriteriaCode: userBadgeData.automaticCriteriaCode ?? '',
        sequence: userBadgeData.sequence?.toString() ?? '',
        active: userBadgeData.active ?? true,
        deleted: userBadgeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userBadgeForm.markAsPristine();
    this.userBadgeForm.markAsUntouched();
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

    if (this.userBadgeService.userIsBMCUserBadgeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Badges", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userBadgeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userBadgeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userBadgeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userBadgeSubmitData: UserBadgeSubmitData = {
        id: this.userBadgeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        iconCssClass: formValue.iconCssClass?.trim() || null,
        iconImagePath: formValue.iconImagePath?.trim() || null,
        badgeColor: formValue.badgeColor?.trim() || null,
        isAutomatic: !!formValue.isAutomatic,
        automaticCriteriaCode: formValue.automaticCriteriaCode?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userBadgeService.PutUserBadge(userBadgeSubmitData.id, userBadgeSubmitData)
      : this.userBadgeService.PostUserBadge(userBadgeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserBadgeData) => {

        this.userBadgeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Badge's detail page
          //
          this.userBadgeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userBadgeForm.markAsUntouched();

          this.router.navigate(['/userbadges', savedUserBadgeData.id]);
          this.alertService.showMessage('User Badge added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userBadgeData = savedUserBadgeData;
          this.buildFormValues(this.userBadgeData);

          this.alertService.showMessage("User Badge saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Badge.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Badge.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Badge could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserBadgeReader(): boolean {
    return this.userBadgeService.userIsBMCUserBadgeReader();
  }

  public userIsBMCUserBadgeWriter(): boolean {
    return this.userBadgeService.userIsBMCUserBadgeWriter();
  }
}

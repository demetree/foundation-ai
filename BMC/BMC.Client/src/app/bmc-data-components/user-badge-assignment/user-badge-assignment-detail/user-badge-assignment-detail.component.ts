/*
   GENERATED FORM FOR THE USERBADGEASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserBadgeAssignment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-badge-assignment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserBadgeAssignmentService, UserBadgeAssignmentData, UserBadgeAssignmentSubmitData } from '../../../bmc-data-services/user-badge-assignment.service';
import { UserBadgeService } from '../../../bmc-data-services/user-badge.service';
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
interface UserBadgeAssignmentFormValues {
  userBadgeId: number | bigint,       // For FK link number
  awardedDate: string,
  awardedByTenantGuid: string | null,
  reason: string | null,
  isDisplayed: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-badge-assignment-detail',
  templateUrl: './user-badge-assignment-detail.component.html',
  styleUrls: ['./user-badge-assignment-detail.component.scss']
})

export class UserBadgeAssignmentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserBadgeAssignmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userBadgeAssignmentForm: FormGroup = this.fb.group({
        userBadgeId: [null, Validators.required],
        awardedDate: ['', Validators.required],
        awardedByTenantGuid: [''],
        reason: [''],
        isDisplayed: [false],
        active: [true],
        deleted: [false],
      });


  public userBadgeAssignmentId: string | null = null;
  public userBadgeAssignmentData: UserBadgeAssignmentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userBadgeAssignments$ = this.userBadgeAssignmentService.GetUserBadgeAssignmentList();
  public userBadges$ = this.userBadgeService.GetUserBadgeList();

  private destroy$ = new Subject<void>();

  constructor(
    public userBadgeAssignmentService: UserBadgeAssignmentService,
    public userBadgeService: UserBadgeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userBadgeAssignmentId from the route parameters
    this.userBadgeAssignmentId = this.route.snapshot.paramMap.get('userBadgeAssignmentId');

    if (this.userBadgeAssignmentId === 'new' ||
        this.userBadgeAssignmentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userBadgeAssignmentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userBadgeAssignmentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userBadgeAssignmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Badge Assignment';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Badge Assignment';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userBadgeAssignmentForm.dirty) {
      return confirm('You have unsaved User Badge Assignment changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userBadgeAssignmentId != null && this.userBadgeAssignmentId !== 'new') {

      const id = parseInt(this.userBadgeAssignmentId, 10);

      if (!isNaN(id)) {
        return { userBadgeAssignmentId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserBadgeAssignment data for the current userBadgeAssignmentId.
  *
  * Fully respects the UserBadgeAssignmentService caching strategy and error handling strategy.
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
    if (!this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserBadgeAssignments.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userBadgeAssignmentId
    //
    if (!this.userBadgeAssignmentId) {

      this.alertService.showMessage('No UserBadgeAssignment ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userBadgeAssignmentId = Number(this.userBadgeAssignmentId);

    if (isNaN(userBadgeAssignmentId) || userBadgeAssignmentId <= 0) {

      this.alertService.showMessage(`Invalid User Badge Assignment ID: "${this.userBadgeAssignmentId}"`,
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
      // This is the most targeted way: clear only this UserBadgeAssignment + relations

      this.userBadgeAssignmentService.ClearRecordCache(userBadgeAssignmentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userBadgeAssignmentService.GetUserBadgeAssignment(userBadgeAssignmentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userBadgeAssignmentData) => {

        //
        // Success path — userBadgeAssignmentData can legitimately be null if 404'd but request succeeded
        //
        if (!userBadgeAssignmentData) {

          this.handleUserBadgeAssignmentNotFound(userBadgeAssignmentId);

        } else {

          this.userBadgeAssignmentData = userBadgeAssignmentData;
          this.buildFormValues(this.userBadgeAssignmentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserBadgeAssignment loaded successfully',
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
        this.handleUserBadgeAssignmentLoadError(error, userBadgeAssignmentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserBadgeAssignmentNotFound(userBadgeAssignmentId: number): void {

    this.userBadgeAssignmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserBadgeAssignment #${userBadgeAssignmentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserBadgeAssignmentLoadError(error: any, userBadgeAssignmentId: number): void {

    let message = 'Failed to load User Badge Assignment.';
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
          message = 'You do not have permission to view this User Badge Assignment.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Badge Assignment #${userBadgeAssignmentId} was not found.`;
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

    console.error(`User Badge Assignment load failed (ID: ${userBadgeAssignmentId})`, error);

    //
    // Reset UI to safe state
    //
    this.userBadgeAssignmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userBadgeAssignmentData: UserBadgeAssignmentData | null) {

    if (userBadgeAssignmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userBadgeAssignmentForm.reset({
        userBadgeId: null,
        awardedDate: '',
        awardedByTenantGuid: '',
        reason: '',
        isDisplayed: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userBadgeAssignmentForm.reset({
        userBadgeId: userBadgeAssignmentData.userBadgeId,
        awardedDate: isoUtcStringToDateTimeLocal(userBadgeAssignmentData.awardedDate) ?? '',
        awardedByTenantGuid: userBadgeAssignmentData.awardedByTenantGuid ?? '',
        reason: userBadgeAssignmentData.reason ?? '',
        isDisplayed: userBadgeAssignmentData.isDisplayed ?? false,
        active: userBadgeAssignmentData.active ?? true,
        deleted: userBadgeAssignmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userBadgeAssignmentForm.markAsPristine();
    this.userBadgeAssignmentForm.markAsUntouched();
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

    if (this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Badge Assignments", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userBadgeAssignmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userBadgeAssignmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userBadgeAssignmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userBadgeAssignmentSubmitData: UserBadgeAssignmentSubmitData = {
        id: this.userBadgeAssignmentData?.id || 0,
        userBadgeId: Number(formValue.userBadgeId),
        awardedDate: dateTimeLocalToIsoUtc(formValue.awardedDate!.trim())!,
        awardedByTenantGuid: formValue.awardedByTenantGuid?.trim() || null,
        reason: formValue.reason?.trim() || null,
        isDisplayed: !!formValue.isDisplayed,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userBadgeAssignmentService.PutUserBadgeAssignment(userBadgeAssignmentSubmitData.id, userBadgeAssignmentSubmitData)
      : this.userBadgeAssignmentService.PostUserBadgeAssignment(userBadgeAssignmentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserBadgeAssignmentData) => {

        this.userBadgeAssignmentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Badge Assignment's detail page
          //
          this.userBadgeAssignmentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userBadgeAssignmentForm.markAsUntouched();

          this.router.navigate(['/userbadgeassignments', savedUserBadgeAssignmentData.id]);
          this.alertService.showMessage('User Badge Assignment added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userBadgeAssignmentData = savedUserBadgeAssignmentData;
          this.buildFormValues(this.userBadgeAssignmentData);

          this.alertService.showMessage("User Badge Assignment saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Badge Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Badge Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Badge Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserBadgeAssignmentReader(): boolean {
    return this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentReader();
  }

  public userIsBMCUserBadgeAssignmentWriter(): boolean {
    return this.userBadgeAssignmentService.userIsBMCUserBadgeAssignmentWriter();
  }
}

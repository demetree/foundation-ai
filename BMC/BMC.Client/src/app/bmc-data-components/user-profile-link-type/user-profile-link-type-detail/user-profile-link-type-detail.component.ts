/*
   GENERATED FORM FOR THE USERPROFILELINKTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserProfileLinkType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-profile-link-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserProfileLinkTypeService, UserProfileLinkTypeData, UserProfileLinkTypeSubmitData } from '../../../bmc-data-services/user-profile-link-type.service';
import { UserProfileLinkService } from '../../../bmc-data-services/user-profile-link.service';
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
interface UserProfileLinkTypeFormValues {
  name: string,
  description: string,
  iconCssClass: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-profile-link-type-detail',
  templateUrl: './user-profile-link-type-detail.component.html',
  styleUrls: ['./user-profile-link-type-detail.component.scss']
})

export class UserProfileLinkTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserProfileLinkTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userProfileLinkTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        iconCssClass: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public userProfileLinkTypeId: string | null = null;
  public userProfileLinkTypeData: UserProfileLinkTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userProfileLinkTypes$ = this.userProfileLinkTypeService.GetUserProfileLinkTypeList();
  public userProfileLinks$ = this.userProfileLinkService.GetUserProfileLinkList();

  private destroy$ = new Subject<void>();

  constructor(
    public userProfileLinkTypeService: UserProfileLinkTypeService,
    public userProfileLinkService: UserProfileLinkService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userProfileLinkTypeId from the route parameters
    this.userProfileLinkTypeId = this.route.snapshot.paramMap.get('userProfileLinkTypeId');

    if (this.userProfileLinkTypeId === 'new' ||
        this.userProfileLinkTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userProfileLinkTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userProfileLinkTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userProfileLinkTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Profile Link Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Profile Link Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userProfileLinkTypeForm.dirty) {
      return confirm('You have unsaved User Profile Link Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userProfileLinkTypeId != null && this.userProfileLinkTypeId !== 'new') {

      const id = parseInt(this.userProfileLinkTypeId, 10);

      if (!isNaN(id)) {
        return { userProfileLinkTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserProfileLinkType data for the current userProfileLinkTypeId.
  *
  * Fully respects the UserProfileLinkTypeService caching strategy and error handling strategy.
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
    if (!this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserProfileLinkTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userProfileLinkTypeId
    //
    if (!this.userProfileLinkTypeId) {

      this.alertService.showMessage('No UserProfileLinkType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userProfileLinkTypeId = Number(this.userProfileLinkTypeId);

    if (isNaN(userProfileLinkTypeId) || userProfileLinkTypeId <= 0) {

      this.alertService.showMessage(`Invalid User Profile Link Type ID: "${this.userProfileLinkTypeId}"`,
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
      // This is the most targeted way: clear only this UserProfileLinkType + relations

      this.userProfileLinkTypeService.ClearRecordCache(userProfileLinkTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userProfileLinkTypeService.GetUserProfileLinkType(userProfileLinkTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userProfileLinkTypeData) => {

        //
        // Success path — userProfileLinkTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!userProfileLinkTypeData) {

          this.handleUserProfileLinkTypeNotFound(userProfileLinkTypeId);

        } else {

          this.userProfileLinkTypeData = userProfileLinkTypeData;
          this.buildFormValues(this.userProfileLinkTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserProfileLinkType loaded successfully',
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
        this.handleUserProfileLinkTypeLoadError(error, userProfileLinkTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserProfileLinkTypeNotFound(userProfileLinkTypeId: number): void {

    this.userProfileLinkTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserProfileLinkType #${userProfileLinkTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserProfileLinkTypeLoadError(error: any, userProfileLinkTypeId: number): void {

    let message = 'Failed to load User Profile Link Type.';
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
          message = 'You do not have permission to view this User Profile Link Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Profile Link Type #${userProfileLinkTypeId} was not found.`;
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

    console.error(`User Profile Link Type load failed (ID: ${userProfileLinkTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.userProfileLinkTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userProfileLinkTypeData: UserProfileLinkTypeData | null) {

    if (userProfileLinkTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userProfileLinkTypeForm.reset({
        name: '',
        description: '',
        iconCssClass: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userProfileLinkTypeForm.reset({
        name: userProfileLinkTypeData.name ?? '',
        description: userProfileLinkTypeData.description ?? '',
        iconCssClass: userProfileLinkTypeData.iconCssClass ?? '',
        sequence: userProfileLinkTypeData.sequence?.toString() ?? '',
        active: userProfileLinkTypeData.active ?? true,
        deleted: userProfileLinkTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userProfileLinkTypeForm.markAsPristine();
    this.userProfileLinkTypeForm.markAsUntouched();
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

    if (this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Profile Link Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userProfileLinkTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userProfileLinkTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userProfileLinkTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userProfileLinkTypeSubmitData: UserProfileLinkTypeSubmitData = {
        id: this.userProfileLinkTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        iconCssClass: formValue.iconCssClass?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userProfileLinkTypeService.PutUserProfileLinkType(userProfileLinkTypeSubmitData.id, userProfileLinkTypeSubmitData)
      : this.userProfileLinkTypeService.PostUserProfileLinkType(userProfileLinkTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserProfileLinkTypeData) => {

        this.userProfileLinkTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Profile Link Type's detail page
          //
          this.userProfileLinkTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userProfileLinkTypeForm.markAsUntouched();

          this.router.navigate(['/userprofilelinktypes', savedUserProfileLinkTypeData.id]);
          this.alertService.showMessage('User Profile Link Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userProfileLinkTypeData = savedUserProfileLinkTypeData;
          this.buildFormValues(this.userProfileLinkTypeData);

          this.alertService.showMessage("User Profile Link Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Profile Link Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Profile Link Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Profile Link Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserProfileLinkTypeReader(): boolean {
    return this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeReader();
  }

  public userIsBMCUserProfileLinkTypeWriter(): boolean {
    return this.userProfileLinkTypeService.userIsBMCUserProfileLinkTypeWriter();
  }
}

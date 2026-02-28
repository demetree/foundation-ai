/*
   GENERATED FORM FOR THE USERSETLIST TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserSetList table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-set-list-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserSetListService, UserSetListData, UserSetListSubmitData } from '../../../bmc-data-services/user-set-list.service';
import { UserSetListChangeHistoryService } from '../../../bmc-data-services/user-set-list-change-history.service';
import { UserSetListItemService } from '../../../bmc-data-services/user-set-list-item.service';
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
interface UserSetListFormValues {
  name: string,
  isBuildable: boolean,
  rebrickableListId: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-set-list-detail',
  templateUrl: './user-set-list-detail.component.html',
  styleUrls: ['./user-set-list-detail.component.scss']
})

export class UserSetListDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserSetListFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userSetListForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        isBuildable: [false],
        rebrickableListId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public userSetListId: string | null = null;
  public userSetListData: UserSetListData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userSetLists$ = this.userSetListService.GetUserSetListList();
  public userSetListChangeHistories$ = this.userSetListChangeHistoryService.GetUserSetListChangeHistoryList();
  public userSetListItems$ = this.userSetListItemService.GetUserSetListItemList();

  private destroy$ = new Subject<void>();

  constructor(
    public userSetListService: UserSetListService,
    public userSetListChangeHistoryService: UserSetListChangeHistoryService,
    public userSetListItemService: UserSetListItemService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userSetListId from the route parameters
    this.userSetListId = this.route.snapshot.paramMap.get('userSetListId');

    if (this.userSetListId === 'new' ||
        this.userSetListId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userSetListData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userSetListForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userSetListForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Set List';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Set List';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userSetListForm.dirty) {
      return confirm('You have unsaved User Set List changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userSetListId != null && this.userSetListId !== 'new') {

      const id = parseInt(this.userSetListId, 10);

      if (!isNaN(id)) {
        return { userSetListId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserSetList data for the current userSetListId.
  *
  * Fully respects the UserSetListService caching strategy and error handling strategy.
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
    if (!this.userSetListService.userIsBMCUserSetListReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserSetLists.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userSetListId
    //
    if (!this.userSetListId) {

      this.alertService.showMessage('No UserSetList ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userSetListId = Number(this.userSetListId);

    if (isNaN(userSetListId) || userSetListId <= 0) {

      this.alertService.showMessage(`Invalid User Set List ID: "${this.userSetListId}"`,
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
      // This is the most targeted way: clear only this UserSetList + relations

      this.userSetListService.ClearRecordCache(userSetListId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userSetListService.GetUserSetList(userSetListId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userSetListData) => {

        //
        // Success path — userSetListData can legitimately be null if 404'd but request succeeded
        //
        if (!userSetListData) {

          this.handleUserSetListNotFound(userSetListId);

        } else {

          this.userSetListData = userSetListData;
          this.buildFormValues(this.userSetListData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserSetList loaded successfully',
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
        this.handleUserSetListLoadError(error, userSetListId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserSetListNotFound(userSetListId: number): void {

    this.userSetListData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserSetList #${userSetListId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserSetListLoadError(error: any, userSetListId: number): void {

    let message = 'Failed to load User Set List.';
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
          message = 'You do not have permission to view this User Set List.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Set List #${userSetListId} was not found.`;
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

    console.error(`User Set List load failed (ID: ${userSetListId})`, error);

    //
    // Reset UI to safe state
    //
    this.userSetListData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userSetListData: UserSetListData | null) {

    if (userSetListData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userSetListForm.reset({
        name: '',
        isBuildable: false,
        rebrickableListId: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userSetListForm.reset({
        name: userSetListData.name ?? '',
        isBuildable: userSetListData.isBuildable ?? false,
        rebrickableListId: userSetListData.rebrickableListId?.toString() ?? '',
        versionNumber: userSetListData.versionNumber?.toString() ?? '',
        active: userSetListData.active ?? true,
        deleted: userSetListData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userSetListForm.markAsPristine();
    this.userSetListForm.markAsUntouched();
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

    if (this.userSetListService.userIsBMCUserSetListWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Set Lists", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userSetListForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userSetListForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userSetListForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userSetListSubmitData: UserSetListSubmitData = {
        id: this.userSetListData?.id || 0,
        name: formValue.name!.trim(),
        isBuildable: !!formValue.isBuildable,
        rebrickableListId: formValue.rebrickableListId ? Number(formValue.rebrickableListId) : null,
        versionNumber: this.userSetListData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userSetListService.PutUserSetList(userSetListSubmitData.id, userSetListSubmitData)
      : this.userSetListService.PostUserSetList(userSetListSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserSetListData) => {

        this.userSetListService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Set List's detail page
          //
          this.userSetListForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userSetListForm.markAsUntouched();

          this.router.navigate(['/usersetlists', savedUserSetListData.id]);
          this.alertService.showMessage('User Set List added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userSetListData = savedUserSetListData;
          this.buildFormValues(this.userSetListData);

          this.alertService.showMessage("User Set List saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Set List.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Set List.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Set List could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserSetListReader(): boolean {
    return this.userSetListService.userIsBMCUserSetListReader();
  }

  public userIsBMCUserSetListWriter(): boolean {
    return this.userSetListService.userIsBMCUserSetListWriter();
  }
}

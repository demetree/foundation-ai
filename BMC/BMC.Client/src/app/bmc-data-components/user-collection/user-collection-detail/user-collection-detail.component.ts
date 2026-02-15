/*
   GENERATED FORM FOR THE USERCOLLECTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserCollection table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-collection-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserCollectionService, UserCollectionData, UserCollectionSubmitData } from '../../../bmc-data-services/user-collection.service';
import { UserCollectionChangeHistoryService } from '../../../bmc-data-services/user-collection-change-history.service';
import { UserCollectionPartService } from '../../../bmc-data-services/user-collection-part.service';
import { UserWishlistItemService } from '../../../bmc-data-services/user-wishlist-item.service';
import { UserCollectionSetImportService } from '../../../bmc-data-services/user-collection-set-import.service';
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
interface UserCollectionFormValues {
  name: string,
  description: string,
  isDefault: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-collection-detail',
  templateUrl: './user-collection-detail.component.html',
  styleUrls: ['./user-collection-detail.component.scss']
})

export class UserCollectionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserCollectionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userCollectionForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isDefault: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public userCollectionId: string | null = null;
  public userCollectionData: UserCollectionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userCollections$ = this.userCollectionService.GetUserCollectionList();
  public userCollectionChangeHistories$ = this.userCollectionChangeHistoryService.GetUserCollectionChangeHistoryList();
  public userCollectionParts$ = this.userCollectionPartService.GetUserCollectionPartList();
  public userWishlistItems$ = this.userWishlistItemService.GetUserWishlistItemList();
  public userCollectionSetImports$ = this.userCollectionSetImportService.GetUserCollectionSetImportList();

  private destroy$ = new Subject<void>();

  constructor(
    public userCollectionService: UserCollectionService,
    public userCollectionChangeHistoryService: UserCollectionChangeHistoryService,
    public userCollectionPartService: UserCollectionPartService,
    public userWishlistItemService: UserWishlistItemService,
    public userCollectionSetImportService: UserCollectionSetImportService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userCollectionId from the route parameters
    this.userCollectionId = this.route.snapshot.paramMap.get('userCollectionId');

    if (this.userCollectionId === 'new' ||
        this.userCollectionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userCollectionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userCollectionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userCollectionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Collection';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Collection';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userCollectionForm.dirty) {
      return confirm('You have unsaved User Collection changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userCollectionId != null && this.userCollectionId !== 'new') {

      const id = parseInt(this.userCollectionId, 10);

      if (!isNaN(id)) {
        return { userCollectionId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserCollection data for the current userCollectionId.
  *
  * Fully respects the UserCollectionService caching strategy and error handling strategy.
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
    if (!this.userCollectionService.userIsBMCUserCollectionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserCollections.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userCollectionId
    //
    if (!this.userCollectionId) {

      this.alertService.showMessage('No UserCollection ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userCollectionId = Number(this.userCollectionId);

    if (isNaN(userCollectionId) || userCollectionId <= 0) {

      this.alertService.showMessage(`Invalid User Collection ID: "${this.userCollectionId}"`,
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
      // This is the most targeted way: clear only this UserCollection + relations

      this.userCollectionService.ClearRecordCache(userCollectionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userCollectionService.GetUserCollection(userCollectionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userCollectionData) => {

        //
        // Success path — userCollectionData can legitimately be null if 404'd but request succeeded
        //
        if (!userCollectionData) {

          this.handleUserCollectionNotFound(userCollectionId);

        } else {

          this.userCollectionData = userCollectionData;
          this.buildFormValues(this.userCollectionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserCollection loaded successfully',
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
        this.handleUserCollectionLoadError(error, userCollectionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserCollectionNotFound(userCollectionId: number): void {

    this.userCollectionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserCollection #${userCollectionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserCollectionLoadError(error: any, userCollectionId: number): void {

    let message = 'Failed to load User Collection.';
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
          message = 'You do not have permission to view this User Collection.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Collection #${userCollectionId} was not found.`;
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

    console.error(`User Collection load failed (ID: ${userCollectionId})`, error);

    //
    // Reset UI to safe state
    //
    this.userCollectionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userCollectionData: UserCollectionData | null) {

    if (userCollectionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userCollectionForm.reset({
        name: '',
        description: '',
        isDefault: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userCollectionForm.reset({
        name: userCollectionData.name ?? '',
        description: userCollectionData.description ?? '',
        isDefault: userCollectionData.isDefault ?? false,
        versionNumber: userCollectionData.versionNumber?.toString() ?? '',
        active: userCollectionData.active ?? true,
        deleted: userCollectionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userCollectionForm.markAsPristine();
    this.userCollectionForm.markAsUntouched();
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

    if (this.userCollectionService.userIsBMCUserCollectionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Collections", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userCollectionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userCollectionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userCollectionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userCollectionSubmitData: UserCollectionSubmitData = {
        id: this.userCollectionData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isDefault: !!formValue.isDefault,
        versionNumber: this.userCollectionData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userCollectionService.PutUserCollection(userCollectionSubmitData.id, userCollectionSubmitData)
      : this.userCollectionService.PostUserCollection(userCollectionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserCollectionData) => {

        this.userCollectionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Collection's detail page
          //
          this.userCollectionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userCollectionForm.markAsUntouched();

          this.router.navigate(['/usercollections', savedUserCollectionData.id]);
          this.alertService.showMessage('User Collection added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userCollectionData = savedUserCollectionData;
          this.buildFormValues(this.userCollectionData);

          this.alertService.showMessage("User Collection saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Collection.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserCollectionReader(): boolean {
    return this.userCollectionService.userIsBMCUserCollectionReader();
  }

  public userIsBMCUserCollectionWriter(): boolean {
    return this.userCollectionService.userIsBMCUserCollectionWriter();
  }
}

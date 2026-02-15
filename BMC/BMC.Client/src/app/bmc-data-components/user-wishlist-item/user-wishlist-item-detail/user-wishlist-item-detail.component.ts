/*
   GENERATED FORM FOR THE USERWISHLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserWishlistItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-wishlist-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserWishlistItemService, UserWishlistItemData, UserWishlistItemSubmitData } from '../../../bmc-data-services/user-wishlist-item.service';
import { UserCollectionService } from '../../../bmc-data-services/user-collection.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
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
interface UserWishlistItemFormValues {
  userCollectionId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint | null,       // For FK link number
  quantityDesired: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-wishlist-item-detail',
  templateUrl: './user-wishlist-item-detail.component.html',
  styleUrls: ['./user-wishlist-item-detail.component.scss']
})

export class UserWishlistItemDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserWishlistItemFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userWishlistItemForm: FormGroup = this.fb.group({
        userCollectionId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null],
        quantityDesired: [''],
        notes: [''],
        active: [true],
        deleted: [false],
      });


  public userWishlistItemId: string | null = null;
  public userWishlistItemData: UserWishlistItemData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userWishlistItems$ = this.userWishlistItemService.GetUserWishlistItemList();
  public userCollections$ = this.userCollectionService.GetUserCollectionList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();

  private destroy$ = new Subject<void>();

  constructor(
    public userWishlistItemService: UserWishlistItemService,
    public userCollectionService: UserCollectionService,
    public brickPartService: BrickPartService,
    public brickColourService: BrickColourService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userWishlistItemId from the route parameters
    this.userWishlistItemId = this.route.snapshot.paramMap.get('userWishlistItemId');

    if (this.userWishlistItemId === 'new' ||
        this.userWishlistItemId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userWishlistItemData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userWishlistItemForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userWishlistItemForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Wishlist Item';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Wishlist Item';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userWishlistItemForm.dirty) {
      return confirm('You have unsaved User Wishlist Item changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userWishlistItemId != null && this.userWishlistItemId !== 'new') {

      const id = parseInt(this.userWishlistItemId, 10);

      if (!isNaN(id)) {
        return { userWishlistItemId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserWishlistItem data for the current userWishlistItemId.
  *
  * Fully respects the UserWishlistItemService caching strategy and error handling strategy.
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
    if (!this.userWishlistItemService.userIsBMCUserWishlistItemReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserWishlistItems.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userWishlistItemId
    //
    if (!this.userWishlistItemId) {

      this.alertService.showMessage('No UserWishlistItem ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userWishlistItemId = Number(this.userWishlistItemId);

    if (isNaN(userWishlistItemId) || userWishlistItemId <= 0) {

      this.alertService.showMessage(`Invalid User Wishlist Item ID: "${this.userWishlistItemId}"`,
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
      // This is the most targeted way: clear only this UserWishlistItem + relations

      this.userWishlistItemService.ClearRecordCache(userWishlistItemId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userWishlistItemService.GetUserWishlistItem(userWishlistItemId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userWishlistItemData) => {

        //
        // Success path — userWishlistItemData can legitimately be null if 404'd but request succeeded
        //
        if (!userWishlistItemData) {

          this.handleUserWishlistItemNotFound(userWishlistItemId);

        } else {

          this.userWishlistItemData = userWishlistItemData;
          this.buildFormValues(this.userWishlistItemData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserWishlistItem loaded successfully',
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
        this.handleUserWishlistItemLoadError(error, userWishlistItemId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserWishlistItemNotFound(userWishlistItemId: number): void {

    this.userWishlistItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserWishlistItem #${userWishlistItemId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserWishlistItemLoadError(error: any, userWishlistItemId: number): void {

    let message = 'Failed to load User Wishlist Item.';
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
          message = 'You do not have permission to view this User Wishlist Item.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Wishlist Item #${userWishlistItemId} was not found.`;
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

    console.error(`User Wishlist Item load failed (ID: ${userWishlistItemId})`, error);

    //
    // Reset UI to safe state
    //
    this.userWishlistItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userWishlistItemData: UserWishlistItemData | null) {

    if (userWishlistItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userWishlistItemForm.reset({
        userCollectionId: null,
        brickPartId: null,
        brickColourId: null,
        quantityDesired: '',
        notes: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userWishlistItemForm.reset({
        userCollectionId: userWishlistItemData.userCollectionId,
        brickPartId: userWishlistItemData.brickPartId,
        brickColourId: userWishlistItemData.brickColourId,
        quantityDesired: userWishlistItemData.quantityDesired?.toString() ?? '',
        notes: userWishlistItemData.notes ?? '',
        active: userWishlistItemData.active ?? true,
        deleted: userWishlistItemData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userWishlistItemForm.markAsPristine();
    this.userWishlistItemForm.markAsUntouched();
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

    if (this.userWishlistItemService.userIsBMCUserWishlistItemWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Wishlist Items", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userWishlistItemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userWishlistItemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userWishlistItemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userWishlistItemSubmitData: UserWishlistItemSubmitData = {
        id: this.userWishlistItemData?.id || 0,
        userCollectionId: Number(formValue.userCollectionId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: formValue.brickColourId ? Number(formValue.brickColourId) : null,
        quantityDesired: formValue.quantityDesired ? Number(formValue.quantityDesired) : null,
        notes: formValue.notes?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userWishlistItemService.PutUserWishlistItem(userWishlistItemSubmitData.id, userWishlistItemSubmitData)
      : this.userWishlistItemService.PostUserWishlistItem(userWishlistItemSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserWishlistItemData) => {

        this.userWishlistItemService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Wishlist Item's detail page
          //
          this.userWishlistItemForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userWishlistItemForm.markAsUntouched();

          this.router.navigate(['/userwishlistitems', savedUserWishlistItemData.id]);
          this.alertService.showMessage('User Wishlist Item added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userWishlistItemData = savedUserWishlistItemData;
          this.buildFormValues(this.userWishlistItemData);

          this.alertService.showMessage("User Wishlist Item saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Wishlist Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Wishlist Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Wishlist Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserWishlistItemReader(): boolean {
    return this.userWishlistItemService.userIsBMCUserWishlistItemReader();
  }

  public userIsBMCUserWishlistItemWriter(): boolean {
    return this.userWishlistItemService.userIsBMCUserWishlistItemWriter();
  }
}

/*
   GENERATED FORM FOR THE USERPARTLISTITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserPartListItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-part-list-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserPartListItemService, UserPartListItemData, UserPartListItemSubmitData } from '../../../bmc-data-services/user-part-list-item.service';
import { UserPartListService } from '../../../bmc-data-services/user-part-list.service';
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
interface UserPartListItemFormValues {
  userPartListId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  quantity: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-part-list-item-detail',
  templateUrl: './user-part-list-item-detail.component.html',
  styleUrls: ['./user-part-list-item-detail.component.scss']
})

export class UserPartListItemDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserPartListItemFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userPartListItemForm: FormGroup = this.fb.group({
        userPartListId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        quantity: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public userPartListItemId: string | null = null;
  public userPartListItemData: UserPartListItemData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userPartListItems$ = this.userPartListItemService.GetUserPartListItemList();
  public userPartLists$ = this.userPartListService.GetUserPartListList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();

  private destroy$ = new Subject<void>();

  constructor(
    public userPartListItemService: UserPartListItemService,
    public userPartListService: UserPartListService,
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

    // Get the userPartListItemId from the route parameters
    this.userPartListItemId = this.route.snapshot.paramMap.get('userPartListItemId');

    if (this.userPartListItemId === 'new' ||
        this.userPartListItemId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userPartListItemData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userPartListItemForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userPartListItemForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Part List Item';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Part List Item';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userPartListItemForm.dirty) {
      return confirm('You have unsaved User Part List Item changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userPartListItemId != null && this.userPartListItemId !== 'new') {

      const id = parseInt(this.userPartListItemId, 10);

      if (!isNaN(id)) {
        return { userPartListItemId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserPartListItem data for the current userPartListItemId.
  *
  * Fully respects the UserPartListItemService caching strategy and error handling strategy.
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
    if (!this.userPartListItemService.userIsBMCUserPartListItemReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserPartListItems.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userPartListItemId
    //
    if (!this.userPartListItemId) {

      this.alertService.showMessage('No UserPartListItem ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userPartListItemId = Number(this.userPartListItemId);

    if (isNaN(userPartListItemId) || userPartListItemId <= 0) {

      this.alertService.showMessage(`Invalid User Part List Item ID: "${this.userPartListItemId}"`,
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
      // This is the most targeted way: clear only this UserPartListItem + relations

      this.userPartListItemService.ClearRecordCache(userPartListItemId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userPartListItemService.GetUserPartListItem(userPartListItemId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userPartListItemData) => {

        //
        // Success path — userPartListItemData can legitimately be null if 404'd but request succeeded
        //
        if (!userPartListItemData) {

          this.handleUserPartListItemNotFound(userPartListItemId);

        } else {

          this.userPartListItemData = userPartListItemData;
          this.buildFormValues(this.userPartListItemData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserPartListItem loaded successfully',
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
        this.handleUserPartListItemLoadError(error, userPartListItemId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserPartListItemNotFound(userPartListItemId: number): void {

    this.userPartListItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserPartListItem #${userPartListItemId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserPartListItemLoadError(error: any, userPartListItemId: number): void {

    let message = 'Failed to load User Part List Item.';
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
          message = 'You do not have permission to view this User Part List Item.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Part List Item #${userPartListItemId} was not found.`;
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

    console.error(`User Part List Item load failed (ID: ${userPartListItemId})`, error);

    //
    // Reset UI to safe state
    //
    this.userPartListItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userPartListItemData: UserPartListItemData | null) {

    if (userPartListItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userPartListItemForm.reset({
        userPartListId: null,
        brickPartId: null,
        brickColourId: null,
        quantity: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userPartListItemForm.reset({
        userPartListId: userPartListItemData.userPartListId,
        brickPartId: userPartListItemData.brickPartId,
        brickColourId: userPartListItemData.brickColourId,
        quantity: userPartListItemData.quantity?.toString() ?? '',
        active: userPartListItemData.active ?? true,
        deleted: userPartListItemData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userPartListItemForm.markAsPristine();
    this.userPartListItemForm.markAsUntouched();
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

    if (this.userPartListItemService.userIsBMCUserPartListItemWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Part List Items", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userPartListItemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userPartListItemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userPartListItemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userPartListItemSubmitData: UserPartListItemSubmitData = {
        id: this.userPartListItemData?.id || 0,
        userPartListId: Number(formValue.userPartListId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        quantity: Number(formValue.quantity),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userPartListItemService.PutUserPartListItem(userPartListItemSubmitData.id, userPartListItemSubmitData)
      : this.userPartListItemService.PostUserPartListItem(userPartListItemSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserPartListItemData) => {

        this.userPartListItemService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Part List Item's detail page
          //
          this.userPartListItemForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userPartListItemForm.markAsUntouched();

          this.router.navigate(['/userpartlistitems', savedUserPartListItemData.id]);
          this.alertService.showMessage('User Part List Item added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userPartListItemData = savedUserPartListItemData;
          this.buildFormValues(this.userPartListItemData);

          this.alertService.showMessage("User Part List Item saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Part List Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Part List Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Part List Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserPartListItemReader(): boolean {
    return this.userPartListItemService.userIsBMCUserPartListItemReader();
  }

  public userIsBMCUserPartListItemWriter(): boolean {
    return this.userPartListItemService.userIsBMCUserPartListItemWriter();
  }
}

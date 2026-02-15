/*
   GENERATED FORM FOR THE USERCOLLECTIONPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserCollectionPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-collection-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserCollectionPartService, UserCollectionPartData, UserCollectionPartSubmitData } from '../../../bmc-data-services/user-collection-part.service';
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
interface UserCollectionPartFormValues {
  userCollectionId: number | bigint,       // For FK link number
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  quantityOwned: string | null,     // Stored as string for form input, converted to number on submit.
  quantityUsed: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-collection-part-detail',
  templateUrl: './user-collection-part-detail.component.html',
  styleUrls: ['./user-collection-part-detail.component.scss']
})

export class UserCollectionPartDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserCollectionPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userCollectionPartForm: FormGroup = this.fb.group({
        userCollectionId: [null, Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        quantityOwned: [''],
        quantityUsed: [''],
        active: [true],
        deleted: [false],
      });


  public userCollectionPartId: string | null = null;
  public userCollectionPartData: UserCollectionPartData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userCollectionParts$ = this.userCollectionPartService.GetUserCollectionPartList();
  public userCollections$ = this.userCollectionService.GetUserCollectionList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();

  private destroy$ = new Subject<void>();

  constructor(
    public userCollectionPartService: UserCollectionPartService,
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

    // Get the userCollectionPartId from the route parameters
    this.userCollectionPartId = this.route.snapshot.paramMap.get('userCollectionPartId');

    if (this.userCollectionPartId === 'new' ||
        this.userCollectionPartId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userCollectionPartData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userCollectionPartForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userCollectionPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Collection Part';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Collection Part';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userCollectionPartForm.dirty) {
      return confirm('You have unsaved User Collection Part changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userCollectionPartId != null && this.userCollectionPartId !== 'new') {

      const id = parseInt(this.userCollectionPartId, 10);

      if (!isNaN(id)) {
        return { userCollectionPartId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserCollectionPart data for the current userCollectionPartId.
  *
  * Fully respects the UserCollectionPartService caching strategy and error handling strategy.
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
    if (!this.userCollectionPartService.userIsBMCUserCollectionPartReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserCollectionParts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userCollectionPartId
    //
    if (!this.userCollectionPartId) {

      this.alertService.showMessage('No UserCollectionPart ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userCollectionPartId = Number(this.userCollectionPartId);

    if (isNaN(userCollectionPartId) || userCollectionPartId <= 0) {

      this.alertService.showMessage(`Invalid User Collection Part ID: "${this.userCollectionPartId}"`,
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
      // This is the most targeted way: clear only this UserCollectionPart + relations

      this.userCollectionPartService.ClearRecordCache(userCollectionPartId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userCollectionPartService.GetUserCollectionPart(userCollectionPartId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userCollectionPartData) => {

        //
        // Success path — userCollectionPartData can legitimately be null if 404'd but request succeeded
        //
        if (!userCollectionPartData) {

          this.handleUserCollectionPartNotFound(userCollectionPartId);

        } else {

          this.userCollectionPartData = userCollectionPartData;
          this.buildFormValues(this.userCollectionPartData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserCollectionPart loaded successfully',
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
        this.handleUserCollectionPartLoadError(error, userCollectionPartId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserCollectionPartNotFound(userCollectionPartId: number): void {

    this.userCollectionPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserCollectionPart #${userCollectionPartId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserCollectionPartLoadError(error: any, userCollectionPartId: number): void {

    let message = 'Failed to load User Collection Part.';
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
          message = 'You do not have permission to view this User Collection Part.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Collection Part #${userCollectionPartId} was not found.`;
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

    console.error(`User Collection Part load failed (ID: ${userCollectionPartId})`, error);

    //
    // Reset UI to safe state
    //
    this.userCollectionPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userCollectionPartData: UserCollectionPartData | null) {

    if (userCollectionPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userCollectionPartForm.reset({
        userCollectionId: null,
        brickPartId: null,
        brickColourId: null,
        quantityOwned: '',
        quantityUsed: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userCollectionPartForm.reset({
        userCollectionId: userCollectionPartData.userCollectionId,
        brickPartId: userCollectionPartData.brickPartId,
        brickColourId: userCollectionPartData.brickColourId,
        quantityOwned: userCollectionPartData.quantityOwned?.toString() ?? '',
        quantityUsed: userCollectionPartData.quantityUsed?.toString() ?? '',
        active: userCollectionPartData.active ?? true,
        deleted: userCollectionPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userCollectionPartForm.markAsPristine();
    this.userCollectionPartForm.markAsUntouched();
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

    if (this.userCollectionPartService.userIsBMCUserCollectionPartWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Collection Parts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userCollectionPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userCollectionPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userCollectionPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userCollectionPartSubmitData: UserCollectionPartSubmitData = {
        id: this.userCollectionPartData?.id || 0,
        userCollectionId: Number(formValue.userCollectionId),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        quantityOwned: formValue.quantityOwned ? Number(formValue.quantityOwned) : null,
        quantityUsed: formValue.quantityUsed ? Number(formValue.quantityUsed) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userCollectionPartService.PutUserCollectionPart(userCollectionPartSubmitData.id, userCollectionPartSubmitData)
      : this.userCollectionPartService.PostUserCollectionPart(userCollectionPartSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserCollectionPartData) => {

        this.userCollectionPartService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Collection Part's detail page
          //
          this.userCollectionPartForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userCollectionPartForm.markAsUntouched();

          this.router.navigate(['/usercollectionparts', savedUserCollectionPartData.id]);
          this.alertService.showMessage('User Collection Part added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userCollectionPartData = savedUserCollectionPartData;
          this.buildFormValues(this.userCollectionPartData);

          this.alertService.showMessage("User Collection Part saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Collection Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserCollectionPartReader(): boolean {
    return this.userCollectionPartService.userIsBMCUserCollectionPartReader();
  }

  public userIsBMCUserCollectionPartWriter(): boolean {
    return this.userCollectionPartService.userIsBMCUserCollectionPartWriter();
  }
}

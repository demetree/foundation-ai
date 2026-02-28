/*
   GENERATED FORM FOR THE USERLOSTPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserLostPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-lost-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserLostPartService, UserLostPartData, UserLostPartSubmitData } from '../../../bmc-data-services/user-lost-part.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
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
interface UserLostPartFormValues {
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  legoSetId: number | bigint | null,       // For FK link number
  lostQuantity: string,     // Stored as string for form input, converted to number on submit.
  rebrickableInvPartId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-lost-part-detail',
  templateUrl: './user-lost-part-detail.component.html',
  styleUrls: ['./user-lost-part-detail.component.scss']
})

export class UserLostPartDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserLostPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userLostPartForm: FormGroup = this.fb.group({
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        legoSetId: [null],
        lostQuantity: ['', Validators.required],
        rebrickableInvPartId: [''],
        active: [true],
        deleted: [false],
      });


  public userLostPartId: string | null = null;
  public userLostPartData: UserLostPartData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userLostParts$ = this.userLostPartService.GetUserLostPartList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();
  public legoSets$ = this.legoSetService.GetLegoSetList();

  private destroy$ = new Subject<void>();

  constructor(
    public userLostPartService: UserLostPartService,
    public brickPartService: BrickPartService,
    public brickColourService: BrickColourService,
    public legoSetService: LegoSetService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userLostPartId from the route parameters
    this.userLostPartId = this.route.snapshot.paramMap.get('userLostPartId');

    if (this.userLostPartId === 'new' ||
        this.userLostPartId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userLostPartData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userLostPartForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userLostPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Lost Part';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Lost Part';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userLostPartForm.dirty) {
      return confirm('You have unsaved User Lost Part changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userLostPartId != null && this.userLostPartId !== 'new') {

      const id = parseInt(this.userLostPartId, 10);

      if (!isNaN(id)) {
        return { userLostPartId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserLostPart data for the current userLostPartId.
  *
  * Fully respects the UserLostPartService caching strategy and error handling strategy.
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
    if (!this.userLostPartService.userIsBMCUserLostPartReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserLostParts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userLostPartId
    //
    if (!this.userLostPartId) {

      this.alertService.showMessage('No UserLostPart ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userLostPartId = Number(this.userLostPartId);

    if (isNaN(userLostPartId) || userLostPartId <= 0) {

      this.alertService.showMessage(`Invalid User Lost Part ID: "${this.userLostPartId}"`,
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
      // This is the most targeted way: clear only this UserLostPart + relations

      this.userLostPartService.ClearRecordCache(userLostPartId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userLostPartService.GetUserLostPart(userLostPartId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userLostPartData) => {

        //
        // Success path — userLostPartData can legitimately be null if 404'd but request succeeded
        //
        if (!userLostPartData) {

          this.handleUserLostPartNotFound(userLostPartId);

        } else {

          this.userLostPartData = userLostPartData;
          this.buildFormValues(this.userLostPartData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserLostPart loaded successfully',
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
        this.handleUserLostPartLoadError(error, userLostPartId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserLostPartNotFound(userLostPartId: number): void {

    this.userLostPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserLostPart #${userLostPartId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserLostPartLoadError(error: any, userLostPartId: number): void {

    let message = 'Failed to load User Lost Part.';
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
          message = 'You do not have permission to view this User Lost Part.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Lost Part #${userLostPartId} was not found.`;
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

    console.error(`User Lost Part load failed (ID: ${userLostPartId})`, error);

    //
    // Reset UI to safe state
    //
    this.userLostPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userLostPartData: UserLostPartData | null) {

    if (userLostPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userLostPartForm.reset({
        brickPartId: null,
        brickColourId: null,
        legoSetId: null,
        lostQuantity: '',
        rebrickableInvPartId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userLostPartForm.reset({
        brickPartId: userLostPartData.brickPartId,
        brickColourId: userLostPartData.brickColourId,
        legoSetId: userLostPartData.legoSetId,
        lostQuantity: userLostPartData.lostQuantity?.toString() ?? '',
        rebrickableInvPartId: userLostPartData.rebrickableInvPartId?.toString() ?? '',
        active: userLostPartData.active ?? true,
        deleted: userLostPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userLostPartForm.markAsPristine();
    this.userLostPartForm.markAsUntouched();
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

    if (this.userLostPartService.userIsBMCUserLostPartWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Lost Parts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userLostPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userLostPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userLostPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userLostPartSubmitData: UserLostPartSubmitData = {
        id: this.userLostPartData?.id || 0,
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        legoSetId: formValue.legoSetId ? Number(formValue.legoSetId) : null,
        lostQuantity: Number(formValue.lostQuantity),
        rebrickableInvPartId: formValue.rebrickableInvPartId ? Number(formValue.rebrickableInvPartId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userLostPartService.PutUserLostPart(userLostPartSubmitData.id, userLostPartSubmitData)
      : this.userLostPartService.PostUserLostPart(userLostPartSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserLostPartData) => {

        this.userLostPartService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Lost Part's detail page
          //
          this.userLostPartForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userLostPartForm.markAsUntouched();

          this.router.navigate(['/userlostparts', savedUserLostPartData.id]);
          this.alertService.showMessage('User Lost Part added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userLostPartData = savedUserLostPartData;
          this.buildFormValues(this.userLostPartData);

          this.alertService.showMessage("User Lost Part saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Lost Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Lost Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Lost Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserLostPartReader(): boolean {
    return this.userLostPartService.userIsBMCUserLostPartReader();
  }

  public userIsBMCUserLostPartWriter(): boolean {
    return this.userLostPartService.userIsBMCUserLostPartWriter();
  }
}

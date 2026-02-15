/*
   GENERATED FORM FOR THE USERCOLLECTIONSETIMPORT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from UserCollectionSetImport table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to user-collection-set-import-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { UserCollectionSetImportService, UserCollectionSetImportData, UserCollectionSetImportSubmitData } from '../../../bmc-data-services/user-collection-set-import.service';
import { UserCollectionService } from '../../../bmc-data-services/user-collection.service';
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
interface UserCollectionSetImportFormValues {
  userCollectionId: number | bigint,       // For FK link number
  legoSetId: number | bigint,       // For FK link number
  quantity: string | null,     // Stored as string for form input, converted to number on submit.
  importedDate: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-user-collection-set-import-detail',
  templateUrl: './user-collection-set-import-detail.component.html',
  styleUrls: ['./user-collection-set-import-detail.component.scss']
})

export class UserCollectionSetImportDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<UserCollectionSetImportFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public userCollectionSetImportForm: FormGroup = this.fb.group({
        userCollectionId: [null, Validators.required],
        legoSetId: [null, Validators.required],
        quantity: [''],
        importedDate: [''],
        active: [true],
        deleted: [false],
      });


  public userCollectionSetImportId: string | null = null;
  public userCollectionSetImportData: UserCollectionSetImportData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  userCollectionSetImports$ = this.userCollectionSetImportService.GetUserCollectionSetImportList();
  public userCollections$ = this.userCollectionService.GetUserCollectionList();
  public legoSets$ = this.legoSetService.GetLegoSetList();

  private destroy$ = new Subject<void>();

  constructor(
    public userCollectionSetImportService: UserCollectionSetImportService,
    public userCollectionService: UserCollectionService,
    public legoSetService: LegoSetService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the userCollectionSetImportId from the route parameters
    this.userCollectionSetImportId = this.route.snapshot.paramMap.get('userCollectionSetImportId');

    if (this.userCollectionSetImportId === 'new' ||
        this.userCollectionSetImportId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.userCollectionSetImportData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.userCollectionSetImportForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.userCollectionSetImportForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New User Collection Set Import';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit User Collection Set Import';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.userCollectionSetImportForm.dirty) {
      return confirm('You have unsaved User Collection Set Import changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.userCollectionSetImportId != null && this.userCollectionSetImportId !== 'new') {

      const id = parseInt(this.userCollectionSetImportId, 10);

      if (!isNaN(id)) {
        return { userCollectionSetImportId: id };
      }
    }

    return null;
  }


/*
  * Loads the UserCollectionSetImport data for the current userCollectionSetImportId.
  *
  * Fully respects the UserCollectionSetImportService caching strategy and error handling strategy.
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
    if (!this.userCollectionSetImportService.userIsBMCUserCollectionSetImportReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read UserCollectionSetImports.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate userCollectionSetImportId
    //
    if (!this.userCollectionSetImportId) {

      this.alertService.showMessage('No UserCollectionSetImport ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const userCollectionSetImportId = Number(this.userCollectionSetImportId);

    if (isNaN(userCollectionSetImportId) || userCollectionSetImportId <= 0) {

      this.alertService.showMessage(`Invalid User Collection Set Import ID: "${this.userCollectionSetImportId}"`,
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
      // This is the most targeted way: clear only this UserCollectionSetImport + relations

      this.userCollectionSetImportService.ClearRecordCache(userCollectionSetImportId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.userCollectionSetImportService.GetUserCollectionSetImport(userCollectionSetImportId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (userCollectionSetImportData) => {

        //
        // Success path — userCollectionSetImportData can legitimately be null if 404'd but request succeeded
        //
        if (!userCollectionSetImportData) {

          this.handleUserCollectionSetImportNotFound(userCollectionSetImportId);

        } else {

          this.userCollectionSetImportData = userCollectionSetImportData;
          this.buildFormValues(this.userCollectionSetImportData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'UserCollectionSetImport loaded successfully',
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
        this.handleUserCollectionSetImportLoadError(error, userCollectionSetImportId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleUserCollectionSetImportNotFound(userCollectionSetImportId: number): void {

    this.userCollectionSetImportData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `UserCollectionSetImport #${userCollectionSetImportId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleUserCollectionSetImportLoadError(error: any, userCollectionSetImportId: number): void {

    let message = 'Failed to load User Collection Set Import.';
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
          message = 'You do not have permission to view this User Collection Set Import.';
          title = 'Forbidden';
          break;
        case 404:
          message = `User Collection Set Import #${userCollectionSetImportId} was not found.`;
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

    console.error(`User Collection Set Import load failed (ID: ${userCollectionSetImportId})`, error);

    //
    // Reset UI to safe state
    //
    this.userCollectionSetImportData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(userCollectionSetImportData: UserCollectionSetImportData | null) {

    if (userCollectionSetImportData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.userCollectionSetImportForm.reset({
        userCollectionId: null,
        legoSetId: null,
        quantity: '',
        importedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.userCollectionSetImportForm.reset({
        userCollectionId: userCollectionSetImportData.userCollectionId,
        legoSetId: userCollectionSetImportData.legoSetId,
        quantity: userCollectionSetImportData.quantity?.toString() ?? '',
        importedDate: isoUtcStringToDateTimeLocal(userCollectionSetImportData.importedDate) ?? '',
        active: userCollectionSetImportData.active ?? true,
        deleted: userCollectionSetImportData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.userCollectionSetImportForm.markAsPristine();
    this.userCollectionSetImportForm.markAsUntouched();
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

    if (this.userCollectionSetImportService.userIsBMCUserCollectionSetImportWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to User Collection Set Imports", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.userCollectionSetImportForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.userCollectionSetImportForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.userCollectionSetImportForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const userCollectionSetImportSubmitData: UserCollectionSetImportSubmitData = {
        id: this.userCollectionSetImportData?.id || 0,
        userCollectionId: Number(formValue.userCollectionId),
        legoSetId: Number(formValue.legoSetId),
        quantity: formValue.quantity ? Number(formValue.quantity) : null,
        importedDate: formValue.importedDate ? dateTimeLocalToIsoUtc(formValue.importedDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.userCollectionSetImportService.PutUserCollectionSetImport(userCollectionSetImportSubmitData.id, userCollectionSetImportSubmitData)
      : this.userCollectionSetImportService.PostUserCollectionSetImport(userCollectionSetImportSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedUserCollectionSetImportData) => {

        this.userCollectionSetImportService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created User Collection Set Import's detail page
          //
          this.userCollectionSetImportForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.userCollectionSetImportForm.markAsUntouched();

          this.router.navigate(['/usercollectionsetimports', savedUserCollectionSetImportData.id]);
          this.alertService.showMessage('User Collection Set Import added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.userCollectionSetImportData = savedUserCollectionSetImportData;
          this.buildFormValues(this.userCollectionSetImportData);

          this.alertService.showMessage("User Collection Set Import saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this User Collection Set Import.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the User Collection Set Import.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('User Collection Set Import could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCUserCollectionSetImportReader(): boolean {
    return this.userCollectionSetImportService.userIsBMCUserCollectionSetImportReader();
  }

  public userIsBMCUserCollectionSetImportWriter(): boolean {
    return this.userCollectionSetImportService.userIsBMCUserCollectionSetImportWriter();
  }
}

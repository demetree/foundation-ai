/*
   GENERATED FORM FOR THE ACHIEVEMENTCATEGORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AchievementCategory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to achievement-category-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AchievementCategoryService, AchievementCategoryData, AchievementCategorySubmitData } from '../../../bmc-data-services/achievement-category.service';
import { AchievementService } from '../../../bmc-data-services/achievement.service';
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
interface AchievementCategoryFormValues {
  name: string,
  description: string,
  iconCssClass: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-achievement-category-detail',
  templateUrl: './achievement-category-detail.component.html',
  styleUrls: ['./achievement-category-detail.component.scss']
})

export class AchievementCategoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AchievementCategoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public achievementCategoryForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        iconCssClass: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public achievementCategoryId: string | null = null;
  public achievementCategoryData: AchievementCategoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  achievementCategories$ = this.achievementCategoryService.GetAchievementCategoryList();
  public achievements$ = this.achievementService.GetAchievementList();

  private destroy$ = new Subject<void>();

  constructor(
    public achievementCategoryService: AchievementCategoryService,
    public achievementService: AchievementService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the achievementCategoryId from the route parameters
    this.achievementCategoryId = this.route.snapshot.paramMap.get('achievementCategoryId');

    if (this.achievementCategoryId === 'new' ||
        this.achievementCategoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.achievementCategoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.achievementCategoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.achievementCategoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Achievement Category';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Achievement Category';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.achievementCategoryForm.dirty) {
      return confirm('You have unsaved Achievement Category changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.achievementCategoryId != null && this.achievementCategoryId !== 'new') {

      const id = parseInt(this.achievementCategoryId, 10);

      if (!isNaN(id)) {
        return { achievementCategoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the AchievementCategory data for the current achievementCategoryId.
  *
  * Fully respects the AchievementCategoryService caching strategy and error handling strategy.
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
    if (!this.achievementCategoryService.userIsBMCAchievementCategoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AchievementCategories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate achievementCategoryId
    //
    if (!this.achievementCategoryId) {

      this.alertService.showMessage('No AchievementCategory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const achievementCategoryId = Number(this.achievementCategoryId);

    if (isNaN(achievementCategoryId) || achievementCategoryId <= 0) {

      this.alertService.showMessage(`Invalid Achievement Category ID: "${this.achievementCategoryId}"`,
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
      // This is the most targeted way: clear only this AchievementCategory + relations

      this.achievementCategoryService.ClearRecordCache(achievementCategoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.achievementCategoryService.GetAchievementCategory(achievementCategoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (achievementCategoryData) => {

        //
        // Success path — achievementCategoryData can legitimately be null if 404'd but request succeeded
        //
        if (!achievementCategoryData) {

          this.handleAchievementCategoryNotFound(achievementCategoryId);

        } else {

          this.achievementCategoryData = achievementCategoryData;
          this.buildFormValues(this.achievementCategoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AchievementCategory loaded successfully',
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
        this.handleAchievementCategoryLoadError(error, achievementCategoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAchievementCategoryNotFound(achievementCategoryId: number): void {

    this.achievementCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AchievementCategory #${achievementCategoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAchievementCategoryLoadError(error: any, achievementCategoryId: number): void {

    let message = 'Failed to load Achievement Category.';
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
          message = 'You do not have permission to view this Achievement Category.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Achievement Category #${achievementCategoryId} was not found.`;
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

    console.error(`Achievement Category load failed (ID: ${achievementCategoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.achievementCategoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(achievementCategoryData: AchievementCategoryData | null) {

    if (achievementCategoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.achievementCategoryForm.reset({
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
        this.achievementCategoryForm.reset({
        name: achievementCategoryData.name ?? '',
        description: achievementCategoryData.description ?? '',
        iconCssClass: achievementCategoryData.iconCssClass ?? '',
        sequence: achievementCategoryData.sequence?.toString() ?? '',
        active: achievementCategoryData.active ?? true,
        deleted: achievementCategoryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.achievementCategoryForm.markAsPristine();
    this.achievementCategoryForm.markAsUntouched();
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

    if (this.achievementCategoryService.userIsBMCAchievementCategoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Achievement Categories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.achievementCategoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.achievementCategoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.achievementCategoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const achievementCategorySubmitData: AchievementCategorySubmitData = {
        id: this.achievementCategoryData?.id || 0,
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
      ? this.achievementCategoryService.PutAchievementCategory(achievementCategorySubmitData.id, achievementCategorySubmitData)
      : this.achievementCategoryService.PostAchievementCategory(achievementCategorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAchievementCategoryData) => {

        this.achievementCategoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Achievement Category's detail page
          //
          this.achievementCategoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.achievementCategoryForm.markAsUntouched();

          this.router.navigate(['/achievementcategories', savedAchievementCategoryData.id]);
          this.alertService.showMessage('Achievement Category added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.achievementCategoryData = savedAchievementCategoryData;
          this.buildFormValues(this.achievementCategoryData);

          this.alertService.showMessage("Achievement Category saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Achievement Category.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Achievement Category.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Achievement Category could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCAchievementCategoryReader(): boolean {
    return this.achievementCategoryService.userIsBMCAchievementCategoryReader();
  }

  public userIsBMCAchievementCategoryWriter(): boolean {
    return this.achievementCategoryService.userIsBMCAchievementCategoryWriter();
  }
}

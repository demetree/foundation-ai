/*
   GENERATED FORM FOR THE ACHIEVEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Achievement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to achievement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AchievementService, AchievementData, AchievementSubmitData } from '../../../bmc-data-services/achievement.service';
import { AchievementCategoryService } from '../../../bmc-data-services/achievement-category.service';
import { UserAchievementService } from '../../../bmc-data-services/user-achievement.service';
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
interface AchievementFormValues {
  achievementCategoryId: number | bigint,       // For FK link number
  name: string,
  description: string,
  iconCssClass: string | null,
  iconImagePath: string | null,
  criteria: string | null,
  criteriaCode: string | null,
  pointValue: string,     // Stored as string for form input, converted to number on submit.
  rarity: string,
  isActive: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-achievement-detail',
  templateUrl: './achievement-detail.component.html',
  styleUrls: ['./achievement-detail.component.scss']
})

export class AchievementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AchievementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public achievementForm: FormGroup = this.fb.group({
        achievementCategoryId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        iconCssClass: [''],
        iconImagePath: [''],
        criteria: [''],
        criteriaCode: [''],
        pointValue: ['', Validators.required],
        rarity: ['', Validators.required],
        isActive: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public achievementId: string | null = null;
  public achievementData: AchievementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  achievements$ = this.achievementService.GetAchievementList();
  public achievementCategories$ = this.achievementCategoryService.GetAchievementCategoryList();
  public userAchievements$ = this.userAchievementService.GetUserAchievementList();

  private destroy$ = new Subject<void>();

  constructor(
    public achievementService: AchievementService,
    public achievementCategoryService: AchievementCategoryService,
    public userAchievementService: UserAchievementService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the achievementId from the route parameters
    this.achievementId = this.route.snapshot.paramMap.get('achievementId');

    if (this.achievementId === 'new' ||
        this.achievementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.achievementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.achievementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.achievementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Achievement';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Achievement';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.achievementForm.dirty) {
      return confirm('You have unsaved Achievement changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.achievementId != null && this.achievementId !== 'new') {

      const id = parseInt(this.achievementId, 10);

      if (!isNaN(id)) {
        return { achievementId: id };
      }
    }

    return null;
  }


/*
  * Loads the Achievement data for the current achievementId.
  *
  * Fully respects the AchievementService caching strategy and error handling strategy.
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
    if (!this.achievementService.userIsBMCAchievementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Achievements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate achievementId
    //
    if (!this.achievementId) {

      this.alertService.showMessage('No Achievement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const achievementId = Number(this.achievementId);

    if (isNaN(achievementId) || achievementId <= 0) {

      this.alertService.showMessage(`Invalid Achievement ID: "${this.achievementId}"`,
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
      // This is the most targeted way: clear only this Achievement + relations

      this.achievementService.ClearRecordCache(achievementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.achievementService.GetAchievement(achievementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (achievementData) => {

        //
        // Success path — achievementData can legitimately be null if 404'd but request succeeded
        //
        if (!achievementData) {

          this.handleAchievementNotFound(achievementId);

        } else {

          this.achievementData = achievementData;
          this.buildFormValues(this.achievementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Achievement loaded successfully',
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
        this.handleAchievementLoadError(error, achievementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAchievementNotFound(achievementId: number): void {

    this.achievementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Achievement #${achievementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAchievementLoadError(error: any, achievementId: number): void {

    let message = 'Failed to load Achievement.';
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
          message = 'You do not have permission to view this Achievement.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Achievement #${achievementId} was not found.`;
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

    console.error(`Achievement load failed (ID: ${achievementId})`, error);

    //
    // Reset UI to safe state
    //
    this.achievementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(achievementData: AchievementData | null) {

    if (achievementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.achievementForm.reset({
        achievementCategoryId: null,
        name: '',
        description: '',
        iconCssClass: '',
        iconImagePath: '',
        criteria: '',
        criteriaCode: '',
        pointValue: '',
        rarity: '',
        isActive: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.achievementForm.reset({
        achievementCategoryId: achievementData.achievementCategoryId,
        name: achievementData.name ?? '',
        description: achievementData.description ?? '',
        iconCssClass: achievementData.iconCssClass ?? '',
        iconImagePath: achievementData.iconImagePath ?? '',
        criteria: achievementData.criteria ?? '',
        criteriaCode: achievementData.criteriaCode ?? '',
        pointValue: achievementData.pointValue?.toString() ?? '',
        rarity: achievementData.rarity ?? '',
        isActive: achievementData.isActive ?? false,
        sequence: achievementData.sequence?.toString() ?? '',
        active: achievementData.active ?? true,
        deleted: achievementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.achievementForm.markAsPristine();
    this.achievementForm.markAsUntouched();
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

    if (this.achievementService.userIsBMCAchievementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Achievements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.achievementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.achievementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.achievementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const achievementSubmitData: AchievementSubmitData = {
        id: this.achievementData?.id || 0,
        achievementCategoryId: Number(formValue.achievementCategoryId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        iconCssClass: formValue.iconCssClass?.trim() || null,
        iconImagePath: formValue.iconImagePath?.trim() || null,
        criteria: formValue.criteria?.trim() || null,
        criteriaCode: formValue.criteriaCode?.trim() || null,
        pointValue: Number(formValue.pointValue),
        rarity: formValue.rarity!.trim(),
        isActive: !!formValue.isActive,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.achievementService.PutAchievement(achievementSubmitData.id, achievementSubmitData)
      : this.achievementService.PostAchievement(achievementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAchievementData) => {

        this.achievementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Achievement's detail page
          //
          this.achievementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.achievementForm.markAsUntouched();

          this.router.navigate(['/achievements', savedAchievementData.id]);
          this.alertService.showMessage('Achievement added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.achievementData = savedAchievementData;
          this.buildFormValues(this.achievementData);

          this.alertService.showMessage("Achievement saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Achievement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Achievement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Achievement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCAchievementReader(): boolean {
    return this.achievementService.userIsBMCAchievementReader();
  }

  public userIsBMCAchievementWriter(): boolean {
    return this.achievementService.userIsBMCAchievementWriter();
  }
}

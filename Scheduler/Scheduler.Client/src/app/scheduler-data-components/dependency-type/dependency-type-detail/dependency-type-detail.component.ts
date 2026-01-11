/*
   GENERATED FORM FOR THE DEPENDENCYTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from DependencyType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to dependency-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { DependencyTypeService, DependencyTypeData, DependencyTypeSubmitData } from '../../../scheduler-data-services/dependency-type.service';
import { ScheduledEventDependencyService } from '../../../scheduler-data-services/scheduled-event-dependency.service';
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
interface DependencyTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-dependency-type-detail',
  templateUrl: './dependency-type-detail.component.html',
  styleUrls: ['./dependency-type-detail.component.scss']
})

export class DependencyTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<DependencyTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public dependencyTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public dependencyTypeId: string | null = null;
  public dependencyTypeData: DependencyTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  dependencyTypes$ = this.dependencyTypeService.GetDependencyTypeList();
  public scheduledEventDependencies$ = this.scheduledEventDependencyService.GetScheduledEventDependencyList();

  private destroy$ = new Subject<void>();

  constructor(
    public dependencyTypeService: DependencyTypeService,
    public scheduledEventDependencyService: ScheduledEventDependencyService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the dependencyTypeId from the route parameters
    this.dependencyTypeId = this.route.snapshot.paramMap.get('dependencyTypeId');

    if (this.dependencyTypeId === 'new' ||
        this.dependencyTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.dependencyTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.dependencyTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.dependencyTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Dependency Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Dependency Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.dependencyTypeForm.dirty) {
      return confirm('You have unsaved Dependency Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.dependencyTypeId != null && this.dependencyTypeId !== 'new') {

      const id = parseInt(this.dependencyTypeId, 10);

      if (!isNaN(id)) {
        return { dependencyTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the DependencyType data for the current dependencyTypeId.
  *
  * Fully respects the DependencyTypeService caching strategy and error handling strategy.
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
    if (!this.dependencyTypeService.userIsSchedulerDependencyTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read DependencyTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate dependencyTypeId
    //
    if (!this.dependencyTypeId) {

      this.alertService.showMessage('No DependencyType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const dependencyTypeId = Number(this.dependencyTypeId);

    if (isNaN(dependencyTypeId) || dependencyTypeId <= 0) {

      this.alertService.showMessage(`Invalid Dependency Type ID: "${this.dependencyTypeId}"`,
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
      // This is the most targeted way: clear only this DependencyType + relations

      this.dependencyTypeService.ClearRecordCache(dependencyTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.dependencyTypeService.GetDependencyType(dependencyTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (dependencyTypeData) => {

        //
        // Success path — dependencyTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!dependencyTypeData) {

          this.handleDependencyTypeNotFound(dependencyTypeId);

        } else {

          this.dependencyTypeData = dependencyTypeData;
          this.buildFormValues(this.dependencyTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'DependencyType loaded successfully',
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
        this.handleDependencyTypeLoadError(error, dependencyTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleDependencyTypeNotFound(dependencyTypeId: number): void {

    this.dependencyTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `DependencyType #${dependencyTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleDependencyTypeLoadError(error: any, dependencyTypeId: number): void {

    let message = 'Failed to load Dependency Type.';
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
          message = 'You do not have permission to view this Dependency Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Dependency Type #${dependencyTypeId} was not found.`;
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

    console.error(`Dependency Type load failed (ID: ${dependencyTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.dependencyTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(dependencyTypeData: DependencyTypeData | null) {

    if (dependencyTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.dependencyTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.dependencyTypeForm.reset({
        name: dependencyTypeData.name ?? '',
        description: dependencyTypeData.description ?? '',
        sequence: dependencyTypeData.sequence?.toString() ?? '',
        color: dependencyTypeData.color ?? '',
        active: dependencyTypeData.active ?? true,
        deleted: dependencyTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.dependencyTypeForm.markAsPristine();
    this.dependencyTypeForm.markAsUntouched();
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

    if (this.dependencyTypeService.userIsSchedulerDependencyTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Dependency Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.dependencyTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.dependencyTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.dependencyTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const dependencyTypeSubmitData: DependencyTypeSubmitData = {
        id: this.dependencyTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.dependencyTypeService.PutDependencyType(dependencyTypeSubmitData.id, dependencyTypeSubmitData)
      : this.dependencyTypeService.PostDependencyType(dependencyTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedDependencyTypeData) => {

        this.dependencyTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Dependency Type's detail page
          //
          this.dependencyTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.dependencyTypeForm.markAsUntouched();

          this.router.navigate(['/dependencytypes', savedDependencyTypeData.id]);
          this.alertService.showMessage('Dependency Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.dependencyTypeData = savedDependencyTypeData;
          this.buildFormValues(this.dependencyTypeData);

          this.alertService.showMessage("Dependency Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Dependency Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Dependency Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Dependency Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerDependencyTypeReader(): boolean {
    return this.dependencyTypeService.userIsSchedulerDependencyTypeReader();
  }

  public userIsSchedulerDependencyTypeWriter(): boolean {
    return this.dependencyTypeService.userIsSchedulerDependencyTypeWriter();
  }
}

/*
   GENERATED FORM FOR THE SUBMODEL TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Submodel table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelService, SubmodelData, SubmodelSubmitData } from '../../../bmc-data-services/submodel.service';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { SubmodelChangeHistoryService } from '../../../bmc-data-services/submodel-change-history.service';
import { SubmodelPlacedBrickService } from '../../../bmc-data-services/submodel-placed-brick.service';
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
interface SubmodelFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  description: string,
  submodelId: number | bigint | null,       // For FK link number
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-submodel-detail',
  templateUrl: './submodel-detail.component.html',
  styleUrls: ['./submodel-detail.component.scss']
})

export class SubmodelDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        description: ['', Validators.required],
        submodelId: [null],
        sequence: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public submodelId: string | null = null;
  public submodelData: SubmodelData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  submodels$ = this.submodelService.GetSubmodelList();
  public projects$ = this.projectService.GetProjectList();
  public submodelChangeHistories$ = this.submodelChangeHistoryService.GetSubmodelChangeHistoryList();
  public submodelPlacedBricks$ = this.submodelPlacedBrickService.GetSubmodelPlacedBrickList();

  private destroy$ = new Subject<void>();

  constructor(
    public submodelService: SubmodelService,
    public projectService: ProjectService,
    public submodelChangeHistoryService: SubmodelChangeHistoryService,
    public submodelPlacedBrickService: SubmodelPlacedBrickService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the submodelId from the route parameters
    this.submodelId = this.route.snapshot.paramMap.get('submodelId');

    if (this.submodelId === 'new' ||
        this.submodelId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.submodelData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.submodelForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Submodel';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Submodel';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.submodelForm.dirty) {
      return confirm('You have unsaved Submodel changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.submodelId != null && this.submodelId !== 'new') {

      const id = parseInt(this.submodelId, 10);

      if (!isNaN(id)) {
        return { submodelId: id };
      }
    }

    return null;
  }


/*
  * Loads the Submodel data for the current submodelId.
  *
  * Fully respects the SubmodelService caching strategy and error handling strategy.
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
    if (!this.submodelService.userIsBMCSubmodelReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Submodels.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate submodelId
    //
    if (!this.submodelId) {

      this.alertService.showMessage('No Submodel ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const submodelId = Number(this.submodelId);

    if (isNaN(submodelId) || submodelId <= 0) {

      this.alertService.showMessage(`Invalid Submodel ID: "${this.submodelId}"`,
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
      // This is the most targeted way: clear only this Submodel + relations

      this.submodelService.ClearRecordCache(submodelId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.submodelService.GetSubmodel(submodelId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (submodelData) => {

        //
        // Success path — submodelData can legitimately be null if 404'd but request succeeded
        //
        if (!submodelData) {

          this.handleSubmodelNotFound(submodelId);

        } else {

          this.submodelData = submodelData;
          this.buildFormValues(this.submodelData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Submodel loaded successfully',
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
        this.handleSubmodelLoadError(error, submodelId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSubmodelNotFound(submodelId: number): void {

    this.submodelData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Submodel #${submodelId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSubmodelLoadError(error: any, submodelId: number): void {

    let message = 'Failed to load Submodel.';
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
          message = 'You do not have permission to view this Submodel.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Submodel #${submodelId} was not found.`;
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

    console.error(`Submodel load failed (ID: ${submodelId})`, error);

    //
    // Reset UI to safe state
    //
    this.submodelData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(submodelData: SubmodelData | null) {

    if (submodelData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelForm.reset({
        projectId: null,
        name: '',
        description: '',
        submodelId: null,
        sequence: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.submodelForm.reset({
        projectId: submodelData.projectId,
        name: submodelData.name ?? '',
        description: submodelData.description ?? '',
        submodelId: submodelData.submodelId,
        sequence: submodelData.sequence?.toString() ?? '',
        versionNumber: submodelData.versionNumber?.toString() ?? '',
        active: submodelData.active ?? true,
        deleted: submodelData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.submodelForm.markAsPristine();
    this.submodelForm.markAsUntouched();
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

    if (this.submodelService.userIsBMCSubmodelWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Submodels", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.submodelForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelSubmitData: SubmodelSubmitData = {
        id: this.submodelData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        submodelId: formValue.submodelId ? Number(formValue.submodelId) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        versionNumber: this.submodelData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.submodelService.PutSubmodel(submodelSubmitData.id, submodelSubmitData)
      : this.submodelService.PostSubmodel(submodelSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSubmodelData) => {

        this.submodelService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Submodel's detail page
          //
          this.submodelForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.submodelForm.markAsUntouched();

          this.router.navigate(['/submodels', savedSubmodelData.id]);
          this.alertService.showMessage('Submodel added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.submodelData = savedSubmodelData;
          this.buildFormValues(this.submodelData);

          this.alertService.showMessage("Submodel saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Submodel.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCSubmodelReader(): boolean {
    return this.submodelService.userIsBMCSubmodelReader();
  }

  public userIsBMCSubmodelWriter(): boolean {
    return this.submodelService.userIsBMCSubmodelWriter();
  }
}

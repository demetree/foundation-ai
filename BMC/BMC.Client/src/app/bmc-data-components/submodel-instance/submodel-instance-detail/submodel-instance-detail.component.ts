/*
   GENERATED FORM FOR THE SUBMODELINSTANCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SubmodelInstance table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-instance-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelInstanceService, SubmodelInstanceData, SubmodelInstanceSubmitData } from '../../../bmc-data-services/submodel-instance.service';
import { SubmodelService } from '../../../bmc-data-services/submodel.service';
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
interface SubmodelInstanceFormValues {
  submodelId: number | bigint,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationX: string | null,     // Stored as string for form input, converted to number on submit.
  rotationY: string | null,     // Stored as string for form input, converted to number on submit.
  rotationZ: string | null,     // Stored as string for form input, converted to number on submit.
  rotationW: string | null,     // Stored as string for form input, converted to number on submit.
  colourCode: string,     // Stored as string for form input, converted to number on submit.
  buildStepNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-submodel-instance-detail',
  templateUrl: './submodel-instance-detail.component.html',
  styleUrls: ['./submodel-instance-detail.component.scss']
})

export class SubmodelInstanceDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelInstanceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelInstanceForm: FormGroup = this.fb.group({
        submodelId: [null, Validators.required],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        rotationX: [''],
        rotationY: [''],
        rotationZ: [''],
        rotationW: [''],
        colourCode: ['', Validators.required],
        buildStepNumber: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public submodelInstanceId: string | null = null;
  public submodelInstanceData: SubmodelInstanceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  submodelInstances$ = this.submodelInstanceService.GetSubmodelInstanceList();
  public submodels$ = this.submodelService.GetSubmodelList();

  private destroy$ = new Subject<void>();

  constructor(
    public submodelInstanceService: SubmodelInstanceService,
    public submodelService: SubmodelService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the submodelInstanceId from the route parameters
    this.submodelInstanceId = this.route.snapshot.paramMap.get('submodelInstanceId');

    if (this.submodelInstanceId === 'new' ||
        this.submodelInstanceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.submodelInstanceData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.submodelInstanceForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelInstanceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Submodel Instance';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Submodel Instance';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.submodelInstanceForm.dirty) {
      return confirm('You have unsaved Submodel Instance changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.submodelInstanceId != null && this.submodelInstanceId !== 'new') {

      const id = parseInt(this.submodelInstanceId, 10);

      if (!isNaN(id)) {
        return { submodelInstanceId: id };
      }
    }

    return null;
  }


/*
  * Loads the SubmodelInstance data for the current submodelInstanceId.
  *
  * Fully respects the SubmodelInstanceService caching strategy and error handling strategy.
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
    if (!this.submodelInstanceService.userIsBMCSubmodelInstanceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SubmodelInstances.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate submodelInstanceId
    //
    if (!this.submodelInstanceId) {

      this.alertService.showMessage('No SubmodelInstance ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const submodelInstanceId = Number(this.submodelInstanceId);

    if (isNaN(submodelInstanceId) || submodelInstanceId <= 0) {

      this.alertService.showMessage(`Invalid Submodel Instance ID: "${this.submodelInstanceId}"`,
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
      // This is the most targeted way: clear only this SubmodelInstance + relations

      this.submodelInstanceService.ClearRecordCache(submodelInstanceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.submodelInstanceService.GetSubmodelInstance(submodelInstanceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (submodelInstanceData) => {

        //
        // Success path — submodelInstanceData can legitimately be null if 404'd but request succeeded
        //
        if (!submodelInstanceData) {

          this.handleSubmodelInstanceNotFound(submodelInstanceId);

        } else {

          this.submodelInstanceData = submodelInstanceData;
          this.buildFormValues(this.submodelInstanceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SubmodelInstance loaded successfully',
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
        this.handleSubmodelInstanceLoadError(error, submodelInstanceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSubmodelInstanceNotFound(submodelInstanceId: number): void {

    this.submodelInstanceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SubmodelInstance #${submodelInstanceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSubmodelInstanceLoadError(error: any, submodelInstanceId: number): void {

    let message = 'Failed to load Submodel Instance.';
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
          message = 'You do not have permission to view this Submodel Instance.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Submodel Instance #${submodelInstanceId} was not found.`;
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

    console.error(`Submodel Instance load failed (ID: ${submodelInstanceId})`, error);

    //
    // Reset UI to safe state
    //
    this.submodelInstanceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(submodelInstanceData: SubmodelInstanceData | null) {

    if (submodelInstanceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelInstanceForm.reset({
        submodelId: null,
        positionX: '',
        positionY: '',
        positionZ: '',
        rotationX: '',
        rotationY: '',
        rotationZ: '',
        rotationW: '',
        colourCode: '',
        buildStepNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.submodelInstanceForm.reset({
        submodelId: submodelInstanceData.submodelId,
        positionX: submodelInstanceData.positionX?.toString() ?? '',
        positionY: submodelInstanceData.positionY?.toString() ?? '',
        positionZ: submodelInstanceData.positionZ?.toString() ?? '',
        rotationX: submodelInstanceData.rotationX?.toString() ?? '',
        rotationY: submodelInstanceData.rotationY?.toString() ?? '',
        rotationZ: submodelInstanceData.rotationZ?.toString() ?? '',
        rotationW: submodelInstanceData.rotationW?.toString() ?? '',
        colourCode: submodelInstanceData.colourCode?.toString() ?? '',
        buildStepNumber: submodelInstanceData.buildStepNumber?.toString() ?? '',
        active: submodelInstanceData.active ?? true,
        deleted: submodelInstanceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.submodelInstanceForm.markAsPristine();
    this.submodelInstanceForm.markAsUntouched();
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

    if (this.submodelInstanceService.userIsBMCSubmodelInstanceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Submodel Instances", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.submodelInstanceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelInstanceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelInstanceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelInstanceSubmitData: SubmodelInstanceSubmitData = {
        id: this.submodelInstanceData?.id || 0,
        submodelId: Number(formValue.submodelId),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        rotationX: formValue.rotationX ? Number(formValue.rotationX) : null,
        rotationY: formValue.rotationY ? Number(formValue.rotationY) : null,
        rotationZ: formValue.rotationZ ? Number(formValue.rotationZ) : null,
        rotationW: formValue.rotationW ? Number(formValue.rotationW) : null,
        colourCode: Number(formValue.colourCode),
        buildStepNumber: Number(formValue.buildStepNumber),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.submodelInstanceService.PutSubmodelInstance(submodelInstanceSubmitData.id, submodelInstanceSubmitData)
      : this.submodelInstanceService.PostSubmodelInstance(submodelInstanceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSubmodelInstanceData) => {

        this.submodelInstanceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Submodel Instance's detail page
          //
          this.submodelInstanceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.submodelInstanceForm.markAsUntouched();

          this.router.navigate(['/submodelinstances', savedSubmodelInstanceData.id]);
          this.alertService.showMessage('Submodel Instance added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.submodelInstanceData = savedSubmodelInstanceData;
          this.buildFormValues(this.submodelInstanceData);

          this.alertService.showMessage("Submodel Instance saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Submodel Instance.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Instance.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Instance could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCSubmodelInstanceReader(): boolean {
    return this.submodelInstanceService.userIsBMCSubmodelInstanceReader();
  }

  public userIsBMCSubmodelInstanceWriter(): boolean {
    return this.submodelInstanceService.userIsBMCSubmodelInstanceWriter();
  }
}

/*
   GENERATED FORM FOR THE RESOURCETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceTypeService, ResourceTypeData, ResourceTypeSubmitData } from '../../../scheduler-data-services/resource-type.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
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
interface ResourceTypeFormValues {
  name: string,
  description: string,
  isBillable: boolean | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-resource-type-detail',
  templateUrl: './resource-type-detail.component.html',
  styleUrls: ['./resource-type-detail.component.scss']
})

export class ResourceTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isBillable: [false],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public resourceTypeId: string | null = null;
  public resourceTypeData: ResourceTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  resourceTypes$ = this.resourceTypeService.GetResourceTypeList();
  public icons$ = this.iconService.GetIconList();
  public resources$ = this.resourceService.GetResourceList();

  private destroy$ = new Subject<void>();

  constructor(
    public resourceTypeService: ResourceTypeService,
    public iconService: IconService,
    public resourceService: ResourceService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the resourceTypeId from the route parameters
    this.resourceTypeId = this.route.snapshot.paramMap.get('resourceTypeId');

    if (this.resourceTypeId === 'new' ||
        this.resourceTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.resourceTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.resourceTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Resource Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Resource Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.resourceTypeForm.dirty) {
      return confirm('You have unsaved Resource Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.resourceTypeId != null && this.resourceTypeId !== 'new') {

      const id = parseInt(this.resourceTypeId, 10);

      if (!isNaN(id)) {
        return { resourceTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the ResourceType data for the current resourceTypeId.
  *
  * Fully respects the ResourceTypeService caching strategy and error handling strategy.
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
    if (!this.resourceTypeService.userIsSchedulerResourceTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ResourceTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate resourceTypeId
    //
    if (!this.resourceTypeId) {

      this.alertService.showMessage('No ResourceType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const resourceTypeId = Number(this.resourceTypeId);

    if (isNaN(resourceTypeId) || resourceTypeId <= 0) {

      this.alertService.showMessage(`Invalid Resource Type ID: "${this.resourceTypeId}"`,
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
      // This is the most targeted way: clear only this ResourceType + relations

      this.resourceTypeService.ClearRecordCache(resourceTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.resourceTypeService.GetResourceType(resourceTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (resourceTypeData) => {

        //
        // Success path — resourceTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!resourceTypeData) {

          this.handleResourceTypeNotFound(resourceTypeId);

        } else {

          this.resourceTypeData = resourceTypeData;
          this.buildFormValues(this.resourceTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ResourceType loaded successfully',
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
        this.handleResourceTypeLoadError(error, resourceTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleResourceTypeNotFound(resourceTypeId: number): void {

    this.resourceTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ResourceType #${resourceTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleResourceTypeLoadError(error: any, resourceTypeId: number): void {

    let message = 'Failed to load Resource Type.';
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
          message = 'You do not have permission to view this Resource Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Resource Type #${resourceTypeId} was not found.`;
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

    console.error(`Resource Type load failed (ID: ${resourceTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.resourceTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(resourceTypeData: ResourceTypeData | null) {

    if (resourceTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceTypeForm.reset({
        name: '',
        description: '',
        isBillable: false,
        sequence: '',
        iconId: null,
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceTypeForm.reset({
        name: resourceTypeData.name ?? '',
        description: resourceTypeData.description ?? '',
        isBillable: resourceTypeData.isBillable ?? false,
        sequence: resourceTypeData.sequence?.toString() ?? '',
        iconId: resourceTypeData.iconId,
        color: resourceTypeData.color ?? '',
        active: resourceTypeData.active ?? true,
        deleted: resourceTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceTypeForm.markAsPristine();
    this.resourceTypeForm.markAsUntouched();
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

    if (this.resourceTypeService.userIsSchedulerResourceTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Resource Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.resourceTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceTypeSubmitData: ResourceTypeSubmitData = {
        id: this.resourceTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isBillable: formValue.isBillable == true ? true : formValue.isBillable == false ? false : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.resourceTypeService.PutResourceType(resourceTypeSubmitData.id, resourceTypeSubmitData)
      : this.resourceTypeService.PostResourceType(resourceTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedResourceTypeData) => {

        this.resourceTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Resource Type's detail page
          //
          this.resourceTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.resourceTypeForm.markAsUntouched();

          this.router.navigate(['/resourcetypes', savedResourceTypeData.id]);
          this.alertService.showMessage('Resource Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.resourceTypeData = savedResourceTypeData;
          this.buildFormValues(this.resourceTypeData);

          this.alertService.showMessage("Resource Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Resource Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerResourceTypeReader(): boolean {
    return this.resourceTypeService.userIsSchedulerResourceTypeReader();
  }

  public userIsSchedulerResourceTypeWriter(): boolean {
    return this.resourceTypeService.userIsSchedulerResourceTypeWriter();
  }
}

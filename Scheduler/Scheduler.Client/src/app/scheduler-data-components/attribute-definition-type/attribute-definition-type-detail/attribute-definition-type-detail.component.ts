/*
   GENERATED FORM FOR THE ATTRIBUTEDEFINITIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AttributeDefinitionType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to attribute-definition-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AttributeDefinitionTypeService, AttributeDefinitionTypeData, AttributeDefinitionTypeSubmitData } from '../../../scheduler-data-services/attribute-definition-type.service';
import { AttributeDefinitionService } from '../../../scheduler-data-services/attribute-definition.service';
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
interface AttributeDefinitionTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-attribute-definition-type-detail',
  templateUrl: './attribute-definition-type-detail.component.html',
  styleUrls: ['./attribute-definition-type-detail.component.scss']
})

export class AttributeDefinitionTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AttributeDefinitionTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public attributeDefinitionTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public attributeDefinitionTypeId: string | null = null;
  public attributeDefinitionTypeData: AttributeDefinitionTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  attributeDefinitionTypes$ = this.attributeDefinitionTypeService.GetAttributeDefinitionTypeList();
  public attributeDefinitions$ = this.attributeDefinitionService.GetAttributeDefinitionList();

  private destroy$ = new Subject<void>();

  constructor(
    public attributeDefinitionTypeService: AttributeDefinitionTypeService,
    public attributeDefinitionService: AttributeDefinitionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the attributeDefinitionTypeId from the route parameters
    this.attributeDefinitionTypeId = this.route.snapshot.paramMap.get('attributeDefinitionTypeId');

    if (this.attributeDefinitionTypeId === 'new' ||
        this.attributeDefinitionTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.attributeDefinitionTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.attributeDefinitionTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.attributeDefinitionTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Attribute Definition Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Attribute Definition Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.attributeDefinitionTypeForm.dirty) {
      return confirm('You have unsaved Attribute Definition Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.attributeDefinitionTypeId != null && this.attributeDefinitionTypeId !== 'new') {

      const id = parseInt(this.attributeDefinitionTypeId, 10);

      if (!isNaN(id)) {
        return { attributeDefinitionTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the AttributeDefinitionType data for the current attributeDefinitionTypeId.
  *
  * Fully respects the AttributeDefinitionTypeService caching strategy and error handling strategy.
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
    if (!this.attributeDefinitionTypeService.userIsSchedulerAttributeDefinitionTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AttributeDefinitionTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate attributeDefinitionTypeId
    //
    if (!this.attributeDefinitionTypeId) {

      this.alertService.showMessage('No AttributeDefinitionType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const attributeDefinitionTypeId = Number(this.attributeDefinitionTypeId);

    if (isNaN(attributeDefinitionTypeId) || attributeDefinitionTypeId <= 0) {

      this.alertService.showMessage(`Invalid Attribute Definition Type ID: "${this.attributeDefinitionTypeId}"`,
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
      // This is the most targeted way: clear only this AttributeDefinitionType + relations

      this.attributeDefinitionTypeService.ClearRecordCache(attributeDefinitionTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.attributeDefinitionTypeService.GetAttributeDefinitionType(attributeDefinitionTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (attributeDefinitionTypeData) => {

        //
        // Success path — attributeDefinitionTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!attributeDefinitionTypeData) {

          this.handleAttributeDefinitionTypeNotFound(attributeDefinitionTypeId);

        } else {

          this.attributeDefinitionTypeData = attributeDefinitionTypeData;
          this.buildFormValues(this.attributeDefinitionTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AttributeDefinitionType loaded successfully',
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
        this.handleAttributeDefinitionTypeLoadError(error, attributeDefinitionTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAttributeDefinitionTypeNotFound(attributeDefinitionTypeId: number): void {

    this.attributeDefinitionTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AttributeDefinitionType #${attributeDefinitionTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAttributeDefinitionTypeLoadError(error: any, attributeDefinitionTypeId: number): void {

    let message = 'Failed to load Attribute Definition Type.';
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
          message = 'You do not have permission to view this Attribute Definition Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Attribute Definition Type #${attributeDefinitionTypeId} was not found.`;
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

    console.error(`Attribute Definition Type load failed (ID: ${attributeDefinitionTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.attributeDefinitionTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(attributeDefinitionTypeData: AttributeDefinitionTypeData | null) {

    if (attributeDefinitionTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.attributeDefinitionTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.attributeDefinitionTypeForm.reset({
        name: attributeDefinitionTypeData.name ?? '',
        description: attributeDefinitionTypeData.description ?? '',
        sequence: attributeDefinitionTypeData.sequence?.toString() ?? '',
        active: attributeDefinitionTypeData.active ?? true,
        deleted: attributeDefinitionTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.attributeDefinitionTypeForm.markAsPristine();
    this.attributeDefinitionTypeForm.markAsUntouched();
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

    if (this.attributeDefinitionTypeService.userIsSchedulerAttributeDefinitionTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Attribute Definition Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.attributeDefinitionTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.attributeDefinitionTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.attributeDefinitionTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const attributeDefinitionTypeSubmitData: AttributeDefinitionTypeSubmitData = {
        id: this.attributeDefinitionTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.attributeDefinitionTypeService.PutAttributeDefinitionType(attributeDefinitionTypeSubmitData.id, attributeDefinitionTypeSubmitData)
      : this.attributeDefinitionTypeService.PostAttributeDefinitionType(attributeDefinitionTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAttributeDefinitionTypeData) => {

        this.attributeDefinitionTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Attribute Definition Type's detail page
          //
          this.attributeDefinitionTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.attributeDefinitionTypeForm.markAsUntouched();

          this.router.navigate(['/attributedefinitiontypes', savedAttributeDefinitionTypeData.id]);
          this.alertService.showMessage('Attribute Definition Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.attributeDefinitionTypeData = savedAttributeDefinitionTypeData;
          this.buildFormValues(this.attributeDefinitionTypeData);

          this.alertService.showMessage("Attribute Definition Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Attribute Definition Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Attribute Definition Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Attribute Definition Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerAttributeDefinitionTypeReader(): boolean {
    return this.attributeDefinitionTypeService.userIsSchedulerAttributeDefinitionTypeReader();
  }

  public userIsSchedulerAttributeDefinitionTypeWriter(): boolean {
    return this.attributeDefinitionTypeService.userIsSchedulerAttributeDefinitionTypeWriter();
  }
}

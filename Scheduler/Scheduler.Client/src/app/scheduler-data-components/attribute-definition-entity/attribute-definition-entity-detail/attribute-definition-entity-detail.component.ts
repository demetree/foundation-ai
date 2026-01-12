/*
   GENERATED FORM FOR THE ATTRIBUTEDEFINITIONENTITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AttributeDefinitionEntity table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to attribute-definition-entity-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AttributeDefinitionEntityService, AttributeDefinitionEntityData, AttributeDefinitionEntitySubmitData } from '../../../scheduler-data-services/attribute-definition-entity.service';
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
interface AttributeDefinitionEntityFormValues {
  name: string,
  description: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-attribute-definition-entity-detail',
  templateUrl: './attribute-definition-entity-detail.component.html',
  styleUrls: ['./attribute-definition-entity-detail.component.scss']
})

export class AttributeDefinitionEntityDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AttributeDefinitionEntityFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public attributeDefinitionEntityForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public attributeDefinitionEntityId: string | null = null;
  public attributeDefinitionEntityData: AttributeDefinitionEntityData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  attributeDefinitionEntities$ = this.attributeDefinitionEntityService.GetAttributeDefinitionEntityList();
  public attributeDefinitions$ = this.attributeDefinitionService.GetAttributeDefinitionList();

  private destroy$ = new Subject<void>();

  constructor(
    public attributeDefinitionEntityService: AttributeDefinitionEntityService,
    public attributeDefinitionService: AttributeDefinitionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the attributeDefinitionEntityId from the route parameters
    this.attributeDefinitionEntityId = this.route.snapshot.paramMap.get('attributeDefinitionEntityId');

    if (this.attributeDefinitionEntityId === 'new' ||
        this.attributeDefinitionEntityId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.attributeDefinitionEntityData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.attributeDefinitionEntityForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.attributeDefinitionEntityForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Attribute Definition Entity';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Attribute Definition Entity';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.attributeDefinitionEntityForm.dirty) {
      return confirm('You have unsaved Attribute Definition Entity changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.attributeDefinitionEntityId != null && this.attributeDefinitionEntityId !== 'new') {

      const id = parseInt(this.attributeDefinitionEntityId, 10);

      if (!isNaN(id)) {
        return { attributeDefinitionEntityId: id };
      }
    }

    return null;
  }


/*
  * Loads the AttributeDefinitionEntity data for the current attributeDefinitionEntityId.
  *
  * Fully respects the AttributeDefinitionEntityService caching strategy and error handling strategy.
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
    if (!this.attributeDefinitionEntityService.userIsSchedulerAttributeDefinitionEntityReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AttributeDefinitionEntities.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate attributeDefinitionEntityId
    //
    if (!this.attributeDefinitionEntityId) {

      this.alertService.showMessage('No AttributeDefinitionEntity ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const attributeDefinitionEntityId = Number(this.attributeDefinitionEntityId);

    if (isNaN(attributeDefinitionEntityId) || attributeDefinitionEntityId <= 0) {

      this.alertService.showMessage(`Invalid Attribute Definition Entity ID: "${this.attributeDefinitionEntityId}"`,
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
      // This is the most targeted way: clear only this AttributeDefinitionEntity + relations

      this.attributeDefinitionEntityService.ClearRecordCache(attributeDefinitionEntityId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.attributeDefinitionEntityService.GetAttributeDefinitionEntity(attributeDefinitionEntityId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (attributeDefinitionEntityData) => {

        //
        // Success path — attributeDefinitionEntityData can legitimately be null if 404'd but request succeeded
        //
        if (!attributeDefinitionEntityData) {

          this.handleAttributeDefinitionEntityNotFound(attributeDefinitionEntityId);

        } else {

          this.attributeDefinitionEntityData = attributeDefinitionEntityData;
          this.buildFormValues(this.attributeDefinitionEntityData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AttributeDefinitionEntity loaded successfully',
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
        this.handleAttributeDefinitionEntityLoadError(error, attributeDefinitionEntityId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAttributeDefinitionEntityNotFound(attributeDefinitionEntityId: number): void {

    this.attributeDefinitionEntityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AttributeDefinitionEntity #${attributeDefinitionEntityId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAttributeDefinitionEntityLoadError(error: any, attributeDefinitionEntityId: number): void {

    let message = 'Failed to load Attribute Definition Entity.';
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
          message = 'You do not have permission to view this Attribute Definition Entity.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Attribute Definition Entity #${attributeDefinitionEntityId} was not found.`;
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

    console.error(`Attribute Definition Entity load failed (ID: ${attributeDefinitionEntityId})`, error);

    //
    // Reset UI to safe state
    //
    this.attributeDefinitionEntityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(attributeDefinitionEntityData: AttributeDefinitionEntityData | null) {

    if (attributeDefinitionEntityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.attributeDefinitionEntityForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.attributeDefinitionEntityForm.reset({
        name: attributeDefinitionEntityData.name ?? '',
        description: attributeDefinitionEntityData.description ?? '',
        active: attributeDefinitionEntityData.active ?? true,
        deleted: attributeDefinitionEntityData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.attributeDefinitionEntityForm.markAsPristine();
    this.attributeDefinitionEntityForm.markAsUntouched();
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

    if (this.attributeDefinitionEntityService.userIsSchedulerAttributeDefinitionEntityWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Attribute Definition Entities", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.attributeDefinitionEntityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.attributeDefinitionEntityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.attributeDefinitionEntityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const attributeDefinitionEntitySubmitData: AttributeDefinitionEntitySubmitData = {
        id: this.attributeDefinitionEntityData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.attributeDefinitionEntityService.PutAttributeDefinitionEntity(attributeDefinitionEntitySubmitData.id, attributeDefinitionEntitySubmitData)
      : this.attributeDefinitionEntityService.PostAttributeDefinitionEntity(attributeDefinitionEntitySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAttributeDefinitionEntityData) => {

        this.attributeDefinitionEntityService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Attribute Definition Entity's detail page
          //
          this.attributeDefinitionEntityForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.attributeDefinitionEntityForm.markAsUntouched();

          this.router.navigate(['/attributedefinitionentities', savedAttributeDefinitionEntityData.id]);
          this.alertService.showMessage('Attribute Definition Entity added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.attributeDefinitionEntityData = savedAttributeDefinitionEntityData;
          this.buildFormValues(this.attributeDefinitionEntityData);

          this.alertService.showMessage("Attribute Definition Entity saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Attribute Definition Entity.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Attribute Definition Entity.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Attribute Definition Entity could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerAttributeDefinitionEntityReader(): boolean {
    return this.attributeDefinitionEntityService.userIsSchedulerAttributeDefinitionEntityReader();
  }

  public userIsSchedulerAttributeDefinitionEntityWriter(): boolean {
    return this.attributeDefinitionEntityService.userIsSchedulerAttributeDefinitionEntityWriter();
  }
}

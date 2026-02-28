/*
   GENERATED FORM FOR THE CONNECTORTYPECOMPATIBILITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConnectorTypeCompatibility table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to connector-type-compatibility-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConnectorTypeCompatibilityService, ConnectorTypeCompatibilityData, ConnectorTypeCompatibilitySubmitData } from '../../../bmc-data-services/connector-type-compatibility.service';
import { ConnectorTypeService } from '../../../bmc-data-services/connector-type.service';
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
interface ConnectorTypeCompatibilityFormValues {
  maleConnectorTypeId: number | bigint,       // For FK link number
  femaleConnectorTypeId: number | bigint,       // For FK link number
  connectionStrength: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-connector-type-compatibility-detail',
  templateUrl: './connector-type-compatibility-detail.component.html',
  styleUrls: ['./connector-type-compatibility-detail.component.scss']
})

export class ConnectorTypeCompatibilityDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConnectorTypeCompatibilityFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public connectorTypeCompatibilityForm: FormGroup = this.fb.group({
        maleConnectorTypeId: [null, Validators.required],
        femaleConnectorTypeId: [null, Validators.required],
        connectionStrength: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public connectorTypeCompatibilityId: string | null = null;
  public connectorTypeCompatibilityData: ConnectorTypeCompatibilityData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  connectorTypeCompatibilities$ = this.connectorTypeCompatibilityService.GetConnectorTypeCompatibilityList();
  public connectorTypes$ = this.connectorTypeService.GetConnectorTypeList();

  private destroy$ = new Subject<void>();

  constructor(
    public connectorTypeCompatibilityService: ConnectorTypeCompatibilityService,
    public connectorTypeService: ConnectorTypeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the connectorTypeCompatibilityId from the route parameters
    this.connectorTypeCompatibilityId = this.route.snapshot.paramMap.get('connectorTypeCompatibilityId');

    if (this.connectorTypeCompatibilityId === 'new' ||
        this.connectorTypeCompatibilityId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.connectorTypeCompatibilityData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.connectorTypeCompatibilityForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.connectorTypeCompatibilityForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Connector Type Compatibility';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Connector Type Compatibility';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.connectorTypeCompatibilityForm.dirty) {
      return confirm('You have unsaved Connector Type Compatibility changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.connectorTypeCompatibilityId != null && this.connectorTypeCompatibilityId !== 'new') {

      const id = parseInt(this.connectorTypeCompatibilityId, 10);

      if (!isNaN(id)) {
        return { connectorTypeCompatibilityId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConnectorTypeCompatibility data for the current connectorTypeCompatibilityId.
  *
  * Fully respects the ConnectorTypeCompatibilityService caching strategy and error handling strategy.
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
    if (!this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConnectorTypeCompatibilities.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate connectorTypeCompatibilityId
    //
    if (!this.connectorTypeCompatibilityId) {

      this.alertService.showMessage('No ConnectorTypeCompatibility ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const connectorTypeCompatibilityId = Number(this.connectorTypeCompatibilityId);

    if (isNaN(connectorTypeCompatibilityId) || connectorTypeCompatibilityId <= 0) {

      this.alertService.showMessage(`Invalid Connector Type Compatibility ID: "${this.connectorTypeCompatibilityId}"`,
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
      // This is the most targeted way: clear only this ConnectorTypeCompatibility + relations

      this.connectorTypeCompatibilityService.ClearRecordCache(connectorTypeCompatibilityId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.connectorTypeCompatibilityService.GetConnectorTypeCompatibility(connectorTypeCompatibilityId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (connectorTypeCompatibilityData) => {

        //
        // Success path — connectorTypeCompatibilityData can legitimately be null if 404'd but request succeeded
        //
        if (!connectorTypeCompatibilityData) {

          this.handleConnectorTypeCompatibilityNotFound(connectorTypeCompatibilityId);

        } else {

          this.connectorTypeCompatibilityData = connectorTypeCompatibilityData;
          this.buildFormValues(this.connectorTypeCompatibilityData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConnectorTypeCompatibility loaded successfully',
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
        this.handleConnectorTypeCompatibilityLoadError(error, connectorTypeCompatibilityId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConnectorTypeCompatibilityNotFound(connectorTypeCompatibilityId: number): void {

    this.connectorTypeCompatibilityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConnectorTypeCompatibility #${connectorTypeCompatibilityId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConnectorTypeCompatibilityLoadError(error: any, connectorTypeCompatibilityId: number): void {

    let message = 'Failed to load Connector Type Compatibility.';
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
          message = 'You do not have permission to view this Connector Type Compatibility.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Connector Type Compatibility #${connectorTypeCompatibilityId} was not found.`;
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

    console.error(`Connector Type Compatibility load failed (ID: ${connectorTypeCompatibilityId})`, error);

    //
    // Reset UI to safe state
    //
    this.connectorTypeCompatibilityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(connectorTypeCompatibilityData: ConnectorTypeCompatibilityData | null) {

    if (connectorTypeCompatibilityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.connectorTypeCompatibilityForm.reset({
        maleConnectorTypeId: null,
        femaleConnectorTypeId: null,
        connectionStrength: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.connectorTypeCompatibilityForm.reset({
        maleConnectorTypeId: connectorTypeCompatibilityData.maleConnectorTypeId,
        femaleConnectorTypeId: connectorTypeCompatibilityData.femaleConnectorTypeId,
        connectionStrength: connectorTypeCompatibilityData.connectionStrength ?? '',
        active: connectorTypeCompatibilityData.active ?? true,
        deleted: connectorTypeCompatibilityData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.connectorTypeCompatibilityForm.markAsPristine();
    this.connectorTypeCompatibilityForm.markAsUntouched();
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

    if (this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Connector Type Compatibilities", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.connectorTypeCompatibilityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.connectorTypeCompatibilityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.connectorTypeCompatibilityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const connectorTypeCompatibilitySubmitData: ConnectorTypeCompatibilitySubmitData = {
        id: this.connectorTypeCompatibilityData?.id || 0,
        maleConnectorTypeId: Number(formValue.maleConnectorTypeId),
        femaleConnectorTypeId: Number(formValue.femaleConnectorTypeId),
        connectionStrength: formValue.connectionStrength!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.connectorTypeCompatibilityService.PutConnectorTypeCompatibility(connectorTypeCompatibilitySubmitData.id, connectorTypeCompatibilitySubmitData)
      : this.connectorTypeCompatibilityService.PostConnectorTypeCompatibility(connectorTypeCompatibilitySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConnectorTypeCompatibilityData) => {

        this.connectorTypeCompatibilityService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Connector Type Compatibility's detail page
          //
          this.connectorTypeCompatibilityForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.connectorTypeCompatibilityForm.markAsUntouched();

          this.router.navigate(['/connectortypecompatibilities', savedConnectorTypeCompatibilityData.id]);
          this.alertService.showMessage('Connector Type Compatibility added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.connectorTypeCompatibilityData = savedConnectorTypeCompatibilityData;
          this.buildFormValues(this.connectorTypeCompatibilityData);

          this.alertService.showMessage("Connector Type Compatibility saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Connector Type Compatibility.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Connector Type Compatibility.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Connector Type Compatibility could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCConnectorTypeCompatibilityReader(): boolean {
    return this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityReader();
  }

  public userIsBMCConnectorTypeCompatibilityWriter(): boolean {
    return this.connectorTypeCompatibilityService.userIsBMCConnectorTypeCompatibilityWriter();
  }
}

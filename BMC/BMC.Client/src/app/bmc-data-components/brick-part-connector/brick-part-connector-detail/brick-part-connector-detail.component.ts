/*
   GENERATED FORM FOR THE BRICKPARTCONNECTOR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPartConnector table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-connector-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickPartConnectorService, BrickPartConnectorData, BrickPartConnectorSubmitData } from '../../../bmc-data-services/brick-part-connector.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
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
interface BrickPartConnectorFormValues {
  brickPartId: number | bigint,       // For FK link number
  connectorTypeId: number | bigint,       // For FK link number
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  orientationX: string | null,     // Stored as string for form input, converted to number on submit.
  orientationY: string | null,     // Stored as string for form input, converted to number on submit.
  orientationZ: string | null,     // Stored as string for form input, converted to number on submit.
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-part-connector-detail',
  templateUrl: './brick-part-connector-detail.component.html',
  styleUrls: ['./brick-part-connector-detail.component.scss']
})

export class BrickPartConnectorDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickPartConnectorFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickPartConnectorForm: FormGroup = this.fb.group({
        brickPartId: [null, Validators.required],
        connectorTypeId: [null, Validators.required],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        orientationX: [''],
        orientationY: [''],
        orientationZ: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public brickPartConnectorId: string | null = null;
  public brickPartConnectorData: BrickPartConnectorData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickPartConnectors$ = this.brickPartConnectorService.GetBrickPartConnectorList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public connectorTypes$ = this.connectorTypeService.GetConnectorTypeList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickPartConnectorService: BrickPartConnectorService,
    public brickPartService: BrickPartService,
    public connectorTypeService: ConnectorTypeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickPartConnectorId from the route parameters
    this.brickPartConnectorId = this.route.snapshot.paramMap.get('brickPartConnectorId');

    if (this.brickPartConnectorId === 'new' ||
        this.brickPartConnectorId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickPartConnectorData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickPartConnectorForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickPartConnectorForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Part Connector';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Part Connector';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickPartConnectorForm.dirty) {
      return confirm('You have unsaved Brick Part Connector changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickPartConnectorId != null && this.brickPartConnectorId !== 'new') {

      const id = parseInt(this.brickPartConnectorId, 10);

      if (!isNaN(id)) {
        return { brickPartConnectorId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickPartConnector data for the current brickPartConnectorId.
  *
  * Fully respects the BrickPartConnectorService caching strategy and error handling strategy.
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
    if (!this.brickPartConnectorService.userIsBMCBrickPartConnectorReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickPartConnectors.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickPartConnectorId
    //
    if (!this.brickPartConnectorId) {

      this.alertService.showMessage('No BrickPartConnector ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickPartConnectorId = Number(this.brickPartConnectorId);

    if (isNaN(brickPartConnectorId) || brickPartConnectorId <= 0) {

      this.alertService.showMessage(`Invalid Brick Part Connector ID: "${this.brickPartConnectorId}"`,
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
      // This is the most targeted way: clear only this BrickPartConnector + relations

      this.brickPartConnectorService.ClearRecordCache(brickPartConnectorId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickPartConnectorService.GetBrickPartConnector(brickPartConnectorId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickPartConnectorData) => {

        //
        // Success path — brickPartConnectorData can legitimately be null if 404'd but request succeeded
        //
        if (!brickPartConnectorData) {

          this.handleBrickPartConnectorNotFound(brickPartConnectorId);

        } else {

          this.brickPartConnectorData = brickPartConnectorData;
          this.buildFormValues(this.brickPartConnectorData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickPartConnector loaded successfully',
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
        this.handleBrickPartConnectorLoadError(error, brickPartConnectorId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickPartConnectorNotFound(brickPartConnectorId: number): void {

    this.brickPartConnectorData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickPartConnector #${brickPartConnectorId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickPartConnectorLoadError(error: any, brickPartConnectorId: number): void {

    let message = 'Failed to load Brick Part Connector.';
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
          message = 'You do not have permission to view this Brick Part Connector.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Part Connector #${brickPartConnectorId} was not found.`;
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

    console.error(`Brick Part Connector load failed (ID: ${brickPartConnectorId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickPartConnectorData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickPartConnectorData: BrickPartConnectorData | null) {

    if (brickPartConnectorData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickPartConnectorForm.reset({
        brickPartId: null,
        connectorTypeId: null,
        positionX: '',
        positionY: '',
        positionZ: '',
        orientationX: '',
        orientationY: '',
        orientationZ: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickPartConnectorForm.reset({
        brickPartId: brickPartConnectorData.brickPartId,
        connectorTypeId: brickPartConnectorData.connectorTypeId,
        positionX: brickPartConnectorData.positionX?.toString() ?? '',
        positionY: brickPartConnectorData.positionY?.toString() ?? '',
        positionZ: brickPartConnectorData.positionZ?.toString() ?? '',
        orientationX: brickPartConnectorData.orientationX?.toString() ?? '',
        orientationY: brickPartConnectorData.orientationY?.toString() ?? '',
        orientationZ: brickPartConnectorData.orientationZ?.toString() ?? '',
        sequence: brickPartConnectorData.sequence?.toString() ?? '',
        active: brickPartConnectorData.active ?? true,
        deleted: brickPartConnectorData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickPartConnectorForm.markAsPristine();
    this.brickPartConnectorForm.markAsUntouched();
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

    if (this.brickPartConnectorService.userIsBMCBrickPartConnectorWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Part Connectors", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickPartConnectorForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickPartConnectorForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickPartConnectorForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickPartConnectorSubmitData: BrickPartConnectorSubmitData = {
        id: this.brickPartConnectorData?.id || 0,
        brickPartId: Number(formValue.brickPartId),
        connectorTypeId: Number(formValue.connectorTypeId),
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        orientationX: formValue.orientationX ? Number(formValue.orientationX) : null,
        orientationY: formValue.orientationY ? Number(formValue.orientationY) : null,
        orientationZ: formValue.orientationZ ? Number(formValue.orientationZ) : null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickPartConnectorService.PutBrickPartConnector(brickPartConnectorSubmitData.id, brickPartConnectorSubmitData)
      : this.brickPartConnectorService.PostBrickPartConnector(brickPartConnectorSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickPartConnectorData) => {

        this.brickPartConnectorService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Part Connector's detail page
          //
          this.brickPartConnectorForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickPartConnectorForm.markAsUntouched();

          this.router.navigate(['/brickpartconnectors', savedBrickPartConnectorData.id]);
          this.alertService.showMessage('Brick Part Connector added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickPartConnectorData = savedBrickPartConnectorData;
          this.buildFormValues(this.brickPartConnectorData);

          this.alertService.showMessage("Brick Part Connector saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Part Connector.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part Connector.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part Connector could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickPartConnectorReader(): boolean {
    return this.brickPartConnectorService.userIsBMCBrickPartConnectorReader();
  }

  public userIsBMCBrickPartConnectorWriter(): boolean {
    return this.brickPartConnectorService.userIsBMCBrickPartConnectorWriter();
  }
}

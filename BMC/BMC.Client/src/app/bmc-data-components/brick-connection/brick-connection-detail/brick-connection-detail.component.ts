/*
   GENERATED FORM FOR THE BRICKCONNECTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickConnection table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-connection-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickConnectionService, BrickConnectionData, BrickConnectionSubmitData } from '../../../bmc-data-services/brick-connection.service';
import { ProjectService } from '../../../bmc-data-services/project.service';
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
interface BrickConnectionFormValues {
  projectId: number | bigint,       // For FK link number
  sourcePlacedBrickId: string | null,     // Stored as string for form input, converted to number on submit.
  sourceConnectorId: string | null,     // Stored as string for form input, converted to number on submit.
  targetPlacedBrickId: string | null,     // Stored as string for form input, converted to number on submit.
  targetConnectorId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-connection-detail',
  templateUrl: './brick-connection-detail.component.html',
  styleUrls: ['./brick-connection-detail.component.scss']
})

export class BrickConnectionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickConnectionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickConnectionForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        sourcePlacedBrickId: [''],
        sourceConnectorId: [''],
        targetPlacedBrickId: [''],
        targetConnectorId: [''],
        active: [true],
        deleted: [false],
      });


  public brickConnectionId: string | null = null;
  public brickConnectionData: BrickConnectionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickConnections$ = this.brickConnectionService.GetBrickConnectionList();
  public projects$ = this.projectService.GetProjectList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickConnectionService: BrickConnectionService,
    public projectService: ProjectService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickConnectionId from the route parameters
    this.brickConnectionId = this.route.snapshot.paramMap.get('brickConnectionId');

    if (this.brickConnectionId === 'new' ||
        this.brickConnectionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickConnectionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickConnectionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickConnectionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Connection';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Connection';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickConnectionForm.dirty) {
      return confirm('You have unsaved Brick Connection changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickConnectionId != null && this.brickConnectionId !== 'new') {

      const id = parseInt(this.brickConnectionId, 10);

      if (!isNaN(id)) {
        return { brickConnectionId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickConnection data for the current brickConnectionId.
  *
  * Fully respects the BrickConnectionService caching strategy and error handling strategy.
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
    if (!this.brickConnectionService.userIsBMCBrickConnectionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickConnections.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickConnectionId
    //
    if (!this.brickConnectionId) {

      this.alertService.showMessage('No BrickConnection ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickConnectionId = Number(this.brickConnectionId);

    if (isNaN(brickConnectionId) || brickConnectionId <= 0) {

      this.alertService.showMessage(`Invalid Brick Connection ID: "${this.brickConnectionId}"`,
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
      // This is the most targeted way: clear only this BrickConnection + relations

      this.brickConnectionService.ClearRecordCache(brickConnectionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickConnectionService.GetBrickConnection(brickConnectionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickConnectionData) => {

        //
        // Success path — brickConnectionData can legitimately be null if 404'd but request succeeded
        //
        if (!brickConnectionData) {

          this.handleBrickConnectionNotFound(brickConnectionId);

        } else {

          this.brickConnectionData = brickConnectionData;
          this.buildFormValues(this.brickConnectionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickConnection loaded successfully',
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
        this.handleBrickConnectionLoadError(error, brickConnectionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickConnectionNotFound(brickConnectionId: number): void {

    this.brickConnectionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickConnection #${brickConnectionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickConnectionLoadError(error: any, brickConnectionId: number): void {

    let message = 'Failed to load Brick Connection.';
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
          message = 'You do not have permission to view this Brick Connection.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Connection #${brickConnectionId} was not found.`;
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

    console.error(`Brick Connection load failed (ID: ${brickConnectionId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickConnectionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickConnectionData: BrickConnectionData | null) {

    if (brickConnectionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickConnectionForm.reset({
        projectId: null,
        sourcePlacedBrickId: '',
        sourceConnectorId: '',
        targetPlacedBrickId: '',
        targetConnectorId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickConnectionForm.reset({
        projectId: brickConnectionData.projectId,
        sourcePlacedBrickId: brickConnectionData.sourcePlacedBrickId?.toString() ?? '',
        sourceConnectorId: brickConnectionData.sourceConnectorId?.toString() ?? '',
        targetPlacedBrickId: brickConnectionData.targetPlacedBrickId?.toString() ?? '',
        targetConnectorId: brickConnectionData.targetConnectorId?.toString() ?? '',
        active: brickConnectionData.active ?? true,
        deleted: brickConnectionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickConnectionForm.markAsPristine();
    this.brickConnectionForm.markAsUntouched();
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

    if (this.brickConnectionService.userIsBMCBrickConnectionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Connections", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickConnectionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickConnectionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickConnectionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickConnectionSubmitData: BrickConnectionSubmitData = {
        id: this.brickConnectionData?.id || 0,
        projectId: Number(formValue.projectId),
        sourcePlacedBrickId: formValue.sourcePlacedBrickId ? Number(formValue.sourcePlacedBrickId) : null,
        sourceConnectorId: formValue.sourceConnectorId ? Number(formValue.sourceConnectorId) : null,
        targetPlacedBrickId: formValue.targetPlacedBrickId ? Number(formValue.targetPlacedBrickId) : null,
        targetConnectorId: formValue.targetConnectorId ? Number(formValue.targetConnectorId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickConnectionService.PutBrickConnection(brickConnectionSubmitData.id, brickConnectionSubmitData)
      : this.brickConnectionService.PostBrickConnection(brickConnectionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickConnectionData) => {

        this.brickConnectionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Connection's detail page
          //
          this.brickConnectionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickConnectionForm.markAsUntouched();

          this.router.navigate(['/brickconnections', savedBrickConnectionData.id]);
          this.alertService.showMessage('Brick Connection added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickConnectionData = savedBrickConnectionData;
          this.buildFormValues(this.brickConnectionData);

          this.alertService.showMessage("Brick Connection saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Connection.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Connection.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Connection could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickConnectionReader(): boolean {
    return this.brickConnectionService.userIsBMCBrickConnectionReader();
  }

  public userIsBMCBrickConnectionWriter(): boolean {
    return this.brickConnectionService.userIsBMCBrickConnectionWriter();
  }
}

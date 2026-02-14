/*
   GENERATED FORM FOR THE PARTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PartType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to part-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PartTypeService, PartTypeData, PartTypeSubmitData } from '../../../bmc-data-services/part-type.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
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
interface PartTypeFormValues {
  name: string,
  description: string,
  isUserVisible: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-part-type-detail',
  templateUrl: './part-type-detail.component.html',
  styleUrls: ['./part-type-detail.component.scss']
})

export class PartTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PartTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public partTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isUserVisible: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public partTypeId: string | null = null;
  public partTypeData: PartTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  partTypes$ = this.partTypeService.GetPartTypeList();
  public brickParts$ = this.brickPartService.GetBrickPartList();

  private destroy$ = new Subject<void>();

  constructor(
    public partTypeService: PartTypeService,
    public brickPartService: BrickPartService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the partTypeId from the route parameters
    this.partTypeId = this.route.snapshot.paramMap.get('partTypeId');

    if (this.partTypeId === 'new' ||
        this.partTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.partTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.partTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.partTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Part Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Part Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.partTypeForm.dirty) {
      return confirm('You have unsaved Part Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.partTypeId != null && this.partTypeId !== 'new') {

      const id = parseInt(this.partTypeId, 10);

      if (!isNaN(id)) {
        return { partTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the PartType data for the current partTypeId.
  *
  * Fully respects the PartTypeService caching strategy and error handling strategy.
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
    if (!this.partTypeService.userIsBMCPartTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PartTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate partTypeId
    //
    if (!this.partTypeId) {

      this.alertService.showMessage('No PartType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const partTypeId = Number(this.partTypeId);

    if (isNaN(partTypeId) || partTypeId <= 0) {

      this.alertService.showMessage(`Invalid Part Type ID: "${this.partTypeId}"`,
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
      // This is the most targeted way: clear only this PartType + relations

      this.partTypeService.ClearRecordCache(partTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.partTypeService.GetPartType(partTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (partTypeData) => {

        //
        // Success path — partTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!partTypeData) {

          this.handlePartTypeNotFound(partTypeId);

        } else {

          this.partTypeData = partTypeData;
          this.buildFormValues(this.partTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PartType loaded successfully',
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
        this.handlePartTypeLoadError(error, partTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePartTypeNotFound(partTypeId: number): void {

    this.partTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PartType #${partTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePartTypeLoadError(error: any, partTypeId: number): void {

    let message = 'Failed to load Part Type.';
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
          message = 'You do not have permission to view this Part Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Part Type #${partTypeId} was not found.`;
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

    console.error(`Part Type load failed (ID: ${partTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.partTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(partTypeData: PartTypeData | null) {

    if (partTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.partTypeForm.reset({
        name: '',
        description: '',
        isUserVisible: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.partTypeForm.reset({
        name: partTypeData.name ?? '',
        description: partTypeData.description ?? '',
        isUserVisible: partTypeData.isUserVisible ?? false,
        sequence: partTypeData.sequence?.toString() ?? '',
        active: partTypeData.active ?? true,
        deleted: partTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.partTypeForm.markAsPristine();
    this.partTypeForm.markAsUntouched();
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

    if (this.partTypeService.userIsBMCPartTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Part Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.partTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.partTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.partTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const partTypeSubmitData: PartTypeSubmitData = {
        id: this.partTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isUserVisible: !!formValue.isUserVisible,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.partTypeService.PutPartType(partTypeSubmitData.id, partTypeSubmitData)
      : this.partTypeService.PostPartType(partTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPartTypeData) => {

        this.partTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Part Type's detail page
          //
          this.partTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.partTypeForm.markAsUntouched();

          this.router.navigate(['/parttypes', savedPartTypeData.id]);
          this.alertService.showMessage('Part Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.partTypeData = savedPartTypeData;
          this.buildFormValues(this.partTypeData);

          this.alertService.showMessage("Part Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Part Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Part Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Part Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCPartTypeReader(): boolean {
    return this.partTypeService.userIsBMCPartTypeReader();
  }

  public userIsBMCPartTypeWriter(): boolean {
    return this.partTypeService.userIsBMCPartTypeWriter();
  }
}

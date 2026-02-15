/*
   GENERATED FORM FOR THE BRICKELEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickElement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-element-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickElementService, BrickElementData, BrickElementSubmitData } from '../../../bmc-data-services/brick-element.service';
import { BrickPartService } from '../../../bmc-data-services/brick-part.service';
import { BrickColourService } from '../../../bmc-data-services/brick-colour.service';
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
interface BrickElementFormValues {
  elementId: string,
  brickPartId: number | bigint,       // For FK link number
  brickColourId: number | bigint,       // For FK link number
  designId: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-element-detail',
  templateUrl: './brick-element-detail.component.html',
  styleUrls: ['./brick-element-detail.component.scss']
})

export class BrickElementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickElementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickElementForm: FormGroup = this.fb.group({
        elementId: ['', Validators.required],
        brickPartId: [null, Validators.required],
        brickColourId: [null, Validators.required],
        designId: [''],
        active: [true],
        deleted: [false],
      });


  public brickElementId: string | null = null;
  public brickElementData: BrickElementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickElements$ = this.brickElementService.GetBrickElementList();
  public brickParts$ = this.brickPartService.GetBrickPartList();
  public brickColours$ = this.brickColourService.GetBrickColourList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickElementService: BrickElementService,
    public brickPartService: BrickPartService,
    public brickColourService: BrickColourService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickElementId from the route parameters
    this.brickElementId = this.route.snapshot.paramMap.get('brickElementId');

    if (this.brickElementId === 'new' ||
        this.brickElementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickElementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickElementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickElementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Element';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Element';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickElementForm.dirty) {
      return confirm('You have unsaved Brick Element changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickElementId != null && this.brickElementId !== 'new') {

      const id = parseInt(this.brickElementId, 10);

      if (!isNaN(id)) {
        return { brickElementId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickElement data for the current brickElementId.
  *
  * Fully respects the BrickElementService caching strategy and error handling strategy.
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
    if (!this.brickElementService.userIsBMCBrickElementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickElements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickElementId
    //
    if (!this.brickElementId) {

      this.alertService.showMessage('No BrickElement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickElementId = Number(this.brickElementId);

    if (isNaN(brickElementId) || brickElementId <= 0) {

      this.alertService.showMessage(`Invalid Brick Element ID: "${this.brickElementId}"`,
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
      // This is the most targeted way: clear only this BrickElement + relations

      this.brickElementService.ClearRecordCache(brickElementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickElementService.GetBrickElement(brickElementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickElementData) => {

        //
        // Success path — brickElementData can legitimately be null if 404'd but request succeeded
        //
        if (!brickElementData) {

          this.handleBrickElementNotFound(brickElementId);

        } else {

          this.brickElementData = brickElementData;
          this.buildFormValues(this.brickElementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickElement loaded successfully',
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
        this.handleBrickElementLoadError(error, brickElementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickElementNotFound(brickElementId: number): void {

    this.brickElementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickElement #${brickElementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickElementLoadError(error: any, brickElementId: number): void {

    let message = 'Failed to load Brick Element.';
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
          message = 'You do not have permission to view this Brick Element.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Element #${brickElementId} was not found.`;
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

    console.error(`Brick Element load failed (ID: ${brickElementId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickElementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickElementData: BrickElementData | null) {

    if (brickElementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickElementForm.reset({
        elementId: '',
        brickPartId: null,
        brickColourId: null,
        designId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickElementForm.reset({
        elementId: brickElementData.elementId ?? '',
        brickPartId: brickElementData.brickPartId,
        brickColourId: brickElementData.brickColourId,
        designId: brickElementData.designId ?? '',
        active: brickElementData.active ?? true,
        deleted: brickElementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickElementForm.markAsPristine();
    this.brickElementForm.markAsUntouched();
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

    if (this.brickElementService.userIsBMCBrickElementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Elements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickElementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickElementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickElementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickElementSubmitData: BrickElementSubmitData = {
        id: this.brickElementData?.id || 0,
        elementId: formValue.elementId!.trim(),
        brickPartId: Number(formValue.brickPartId),
        brickColourId: Number(formValue.brickColourId),
        designId: formValue.designId?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickElementService.PutBrickElement(brickElementSubmitData.id, brickElementSubmitData)
      : this.brickElementService.PostBrickElement(brickElementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickElementData) => {

        this.brickElementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Element's detail page
          //
          this.brickElementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickElementForm.markAsUntouched();

          this.router.navigate(['/brickelements', savedBrickElementData.id]);
          this.alertService.showMessage('Brick Element added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickElementData = savedBrickElementData;
          this.buildFormValues(this.brickElementData);

          this.alertService.showMessage("Brick Element saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Element.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Element.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Element could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickElementReader(): boolean {
    return this.brickElementService.userIsBMCBrickElementReader();
  }

  public userIsBMCBrickElementWriter(): boolean {
    return this.brickElementService.userIsBMCBrickElementWriter();
  }
}

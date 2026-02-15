/*
   GENERATED FORM FOR THE RENDERPRESET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RenderPreset table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to render-preset-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RenderPresetService, RenderPresetData, RenderPresetSubmitData } from '../../../bmc-data-services/render-preset.service';
import { ProjectRenderService } from '../../../bmc-data-services/project-render.service';
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
interface RenderPresetFormValues {
  name: string,
  description: string,
  resolutionWidth: string | null,     // Stored as string for form input, converted to number on submit.
  resolutionHeight: string | null,     // Stored as string for form input, converted to number on submit.
  backgroundColorHex: string | null,
  enableShadows: boolean,
  enableReflections: boolean,
  lightingMode: string | null,
  antiAliasLevel: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-render-preset-detail',
  templateUrl: './render-preset-detail.component.html',
  styleUrls: ['./render-preset-detail.component.scss']
})

export class RenderPresetDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RenderPresetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public renderPresetForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        resolutionWidth: [''],
        resolutionHeight: [''],
        backgroundColorHex: [''],
        enableShadows: [false],
        enableReflections: [false],
        lightingMode: [''],
        antiAliasLevel: [''],
        active: [true],
        deleted: [false],
      });


  public renderPresetId: string | null = null;
  public renderPresetData: RenderPresetData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  renderPresets$ = this.renderPresetService.GetRenderPresetList();
  public projectRenders$ = this.projectRenderService.GetProjectRenderList();

  private destroy$ = new Subject<void>();

  constructor(
    public renderPresetService: RenderPresetService,
    public projectRenderService: ProjectRenderService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the renderPresetId from the route parameters
    this.renderPresetId = this.route.snapshot.paramMap.get('renderPresetId');

    if (this.renderPresetId === 'new' ||
        this.renderPresetId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.renderPresetData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.renderPresetForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.renderPresetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Render Preset';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Render Preset';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.renderPresetForm.dirty) {
      return confirm('You have unsaved Render Preset changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.renderPresetId != null && this.renderPresetId !== 'new') {

      const id = parseInt(this.renderPresetId, 10);

      if (!isNaN(id)) {
        return { renderPresetId: id };
      }
    }

    return null;
  }


/*
  * Loads the RenderPreset data for the current renderPresetId.
  *
  * Fully respects the RenderPresetService caching strategy and error handling strategy.
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
    if (!this.renderPresetService.userIsBMCRenderPresetReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read RenderPresets.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate renderPresetId
    //
    if (!this.renderPresetId) {

      this.alertService.showMessage('No RenderPreset ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const renderPresetId = Number(this.renderPresetId);

    if (isNaN(renderPresetId) || renderPresetId <= 0) {

      this.alertService.showMessage(`Invalid Render Preset ID: "${this.renderPresetId}"`,
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
      // This is the most targeted way: clear only this RenderPreset + relations

      this.renderPresetService.ClearRecordCache(renderPresetId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.renderPresetService.GetRenderPreset(renderPresetId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (renderPresetData) => {

        //
        // Success path — renderPresetData can legitimately be null if 404'd but request succeeded
        //
        if (!renderPresetData) {

          this.handleRenderPresetNotFound(renderPresetId);

        } else {

          this.renderPresetData = renderPresetData;
          this.buildFormValues(this.renderPresetData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'RenderPreset loaded successfully',
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
        this.handleRenderPresetLoadError(error, renderPresetId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleRenderPresetNotFound(renderPresetId: number): void {

    this.renderPresetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `RenderPreset #${renderPresetId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleRenderPresetLoadError(error: any, renderPresetId: number): void {

    let message = 'Failed to load Render Preset.';
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
          message = 'You do not have permission to view this Render Preset.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Render Preset #${renderPresetId} was not found.`;
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

    console.error(`Render Preset load failed (ID: ${renderPresetId})`, error);

    //
    // Reset UI to safe state
    //
    this.renderPresetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(renderPresetData: RenderPresetData | null) {

    if (renderPresetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.renderPresetForm.reset({
        name: '',
        description: '',
        resolutionWidth: '',
        resolutionHeight: '',
        backgroundColorHex: '',
        enableShadows: false,
        enableReflections: false,
        lightingMode: '',
        antiAliasLevel: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.renderPresetForm.reset({
        name: renderPresetData.name ?? '',
        description: renderPresetData.description ?? '',
        resolutionWidth: renderPresetData.resolutionWidth?.toString() ?? '',
        resolutionHeight: renderPresetData.resolutionHeight?.toString() ?? '',
        backgroundColorHex: renderPresetData.backgroundColorHex ?? '',
        enableShadows: renderPresetData.enableShadows ?? false,
        enableReflections: renderPresetData.enableReflections ?? false,
        lightingMode: renderPresetData.lightingMode ?? '',
        antiAliasLevel: renderPresetData.antiAliasLevel?.toString() ?? '',
        active: renderPresetData.active ?? true,
        deleted: renderPresetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.renderPresetForm.markAsPristine();
    this.renderPresetForm.markAsUntouched();
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

    if (this.renderPresetService.userIsBMCRenderPresetWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Render Presets", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.renderPresetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.renderPresetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.renderPresetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const renderPresetSubmitData: RenderPresetSubmitData = {
        id: this.renderPresetData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        resolutionWidth: formValue.resolutionWidth ? Number(formValue.resolutionWidth) : null,
        resolutionHeight: formValue.resolutionHeight ? Number(formValue.resolutionHeight) : null,
        backgroundColorHex: formValue.backgroundColorHex?.trim() || null,
        enableShadows: !!formValue.enableShadows,
        enableReflections: !!formValue.enableReflections,
        lightingMode: formValue.lightingMode?.trim() || null,
        antiAliasLevel: formValue.antiAliasLevel ? Number(formValue.antiAliasLevel) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.renderPresetService.PutRenderPreset(renderPresetSubmitData.id, renderPresetSubmitData)
      : this.renderPresetService.PostRenderPreset(renderPresetSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedRenderPresetData) => {

        this.renderPresetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Render Preset's detail page
          //
          this.renderPresetForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.renderPresetForm.markAsUntouched();

          this.router.navigate(['/renderpresets', savedRenderPresetData.id]);
          this.alertService.showMessage('Render Preset added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.renderPresetData = savedRenderPresetData;
          this.buildFormValues(this.renderPresetData);

          this.alertService.showMessage("Render Preset saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Render Preset.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Render Preset.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Render Preset could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCRenderPresetReader(): boolean {
    return this.renderPresetService.userIsBMCRenderPresetReader();
  }

  public userIsBMCRenderPresetWriter(): boolean {
    return this.renderPresetService.userIsBMCRenderPresetWriter();
  }
}

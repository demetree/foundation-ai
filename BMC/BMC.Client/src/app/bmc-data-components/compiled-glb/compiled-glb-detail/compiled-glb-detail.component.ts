/*
   GENERATED FORM FOR THE COMPILEDGLB TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CompiledGlb table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to compiled-glb-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CompiledGlbService, CompiledGlbData, CompiledGlbSubmitData } from '../../../bmc-data-services/compiled-glb.service';
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
interface CompiledGlbFormValues {
  projectId: number | bigint,       // For FK link number
  projectVersionNumber: string,     // Stored as string for form input, converted to number on submit.
  includesEdgeLines: boolean,
  glbData: string | null,
  glbSizeBytes: string,     // Stored as string for form input, converted to number on submit.
  triangleCount: string | null,     // Stored as string for form input, converted to number on submit.
  stepCount: string | null,     // Stored as string for form input, converted to number on submit.
  compiledAt: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-compiled-glb-detail',
  templateUrl: './compiled-glb-detail.component.html',
  styleUrls: ['./compiled-glb-detail.component.scss']
})

export class CompiledGlbDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CompiledGlbFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public compiledGlbForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        projectVersionNumber: ['', Validators.required],
        includesEdgeLines: [false],
        glbData: [''],
        glbSizeBytes: ['', Validators.required],
        triangleCount: [''],
        stepCount: [''],
        compiledAt: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public compiledGlbId: string | null = null;
  public compiledGlbData: CompiledGlbData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  compiledGlbs$ = this.compiledGlbService.GetCompiledGlbList();
  public projects$ = this.projectService.GetProjectList();

  private destroy$ = new Subject<void>();

  constructor(
    public compiledGlbService: CompiledGlbService,
    public projectService: ProjectService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the compiledGlbId from the route parameters
    this.compiledGlbId = this.route.snapshot.paramMap.get('compiledGlbId');

    if (this.compiledGlbId === 'new' ||
        this.compiledGlbId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.compiledGlbData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.compiledGlbForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.compiledGlbForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Compiled Glb';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Compiled Glb';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.compiledGlbForm.dirty) {
      return confirm('You have unsaved Compiled Glb changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.compiledGlbId != null && this.compiledGlbId !== 'new') {

      const id = parseInt(this.compiledGlbId, 10);

      if (!isNaN(id)) {
        return { compiledGlbId: id };
      }
    }

    return null;
  }


/*
  * Loads the CompiledGlb data for the current compiledGlbId.
  *
  * Fully respects the CompiledGlbService caching strategy and error handling strategy.
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
    if (!this.compiledGlbService.userIsBMCCompiledGlbReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read CompiledGlbs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate compiledGlbId
    //
    if (!this.compiledGlbId) {

      this.alertService.showMessage('No CompiledGlb ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const compiledGlbId = Number(this.compiledGlbId);

    if (isNaN(compiledGlbId) || compiledGlbId <= 0) {

      this.alertService.showMessage(`Invalid Compiled Glb ID: "${this.compiledGlbId}"`,
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
      // This is the most targeted way: clear only this CompiledGlb + relations

      this.compiledGlbService.ClearRecordCache(compiledGlbId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.compiledGlbService.GetCompiledGlb(compiledGlbId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (compiledGlbData) => {

        //
        // Success path — compiledGlbData can legitimately be null if 404'd but request succeeded
        //
        if (!compiledGlbData) {

          this.handleCompiledGlbNotFound(compiledGlbId);

        } else {

          this.compiledGlbData = compiledGlbData;
          this.buildFormValues(this.compiledGlbData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'CompiledGlb loaded successfully',
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
        this.handleCompiledGlbLoadError(error, compiledGlbId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCompiledGlbNotFound(compiledGlbId: number): void {

    this.compiledGlbData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `CompiledGlb #${compiledGlbId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCompiledGlbLoadError(error: any, compiledGlbId: number): void {

    let message = 'Failed to load Compiled Glb.';
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
          message = 'You do not have permission to view this Compiled Glb.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Compiled Glb #${compiledGlbId} was not found.`;
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

    console.error(`Compiled Glb load failed (ID: ${compiledGlbId})`, error);

    //
    // Reset UI to safe state
    //
    this.compiledGlbData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(compiledGlbData: CompiledGlbData | null) {

    if (compiledGlbData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.compiledGlbForm.reset({
        projectId: null,
        projectVersionNumber: '',
        includesEdgeLines: false,
        glbData: '',
        glbSizeBytes: '',
        triangleCount: '',
        stepCount: '',
        compiledAt: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.compiledGlbForm.reset({
        projectId: compiledGlbData.projectId,
        projectVersionNumber: compiledGlbData.projectVersionNumber?.toString() ?? '',
        includesEdgeLines: compiledGlbData.includesEdgeLines ?? false,
        glbData: compiledGlbData.glbData ?? '',
        glbSizeBytes: compiledGlbData.glbSizeBytes?.toString() ?? '',
        triangleCount: compiledGlbData.triangleCount?.toString() ?? '',
        stepCount: compiledGlbData.stepCount?.toString() ?? '',
        compiledAt: isoUtcStringToDateTimeLocal(compiledGlbData.compiledAt) ?? '',
        active: compiledGlbData.active ?? true,
        deleted: compiledGlbData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.compiledGlbForm.markAsPristine();
    this.compiledGlbForm.markAsUntouched();
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

    if (this.compiledGlbService.userIsBMCCompiledGlbWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Compiled Glbs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.compiledGlbForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.compiledGlbForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.compiledGlbForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const compiledGlbSubmitData: CompiledGlbSubmitData = {
        id: this.compiledGlbData?.id || 0,
        projectId: Number(formValue.projectId),
        projectVersionNumber: Number(formValue.projectVersionNumber),
        includesEdgeLines: !!formValue.includesEdgeLines,
        glbData: formValue.glbData?.trim() || null,
        glbSizeBytes: Number(formValue.glbSizeBytes),
        triangleCount: formValue.triangleCount ? Number(formValue.triangleCount) : null,
        stepCount: formValue.stepCount ? Number(formValue.stepCount) : null,
        compiledAt: dateTimeLocalToIsoUtc(formValue.compiledAt!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.compiledGlbService.PutCompiledGlb(compiledGlbSubmitData.id, compiledGlbSubmitData)
      : this.compiledGlbService.PostCompiledGlb(compiledGlbSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCompiledGlbData) => {

        this.compiledGlbService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Compiled Glb's detail page
          //
          this.compiledGlbForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.compiledGlbForm.markAsUntouched();

          this.router.navigate(['/compiledglbs', savedCompiledGlbData.id]);
          this.alertService.showMessage('Compiled Glb added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.compiledGlbData = savedCompiledGlbData;
          this.buildFormValues(this.compiledGlbData);

          this.alertService.showMessage("Compiled Glb saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Compiled Glb.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Compiled Glb.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Compiled Glb could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCCompiledGlbReader(): boolean {
    return this.compiledGlbService.userIsBMCCompiledGlbReader();
  }

  public userIsBMCCompiledGlbWriter(): boolean {
    return this.compiledGlbService.userIsBMCCompiledGlbWriter();
  }
}

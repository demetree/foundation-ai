/*
   GENERATED FORM FOR THE PROJECTEXPORT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectExport table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-export-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectExportService, ProjectExportData, ProjectExportSubmitData } from '../../../bmc-data-services/project-export.service';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { ExportFormatService } from '../../../bmc-data-services/export-format.service';
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
interface ProjectExportFormValues {
  projectId: number | bigint,       // For FK link number
  exportFormatId: number | bigint,       // For FK link number
  name: string,
  outputFilePath: string | null,
  exportedDate: string | null,
  includeInstructions: boolean,
  includePartsList: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-project-export-detail',
  templateUrl: './project-export-detail.component.html',
  styleUrls: ['./project-export-detail.component.scss']
})

export class ProjectExportDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectExportFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectExportForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        exportFormatId: [null, Validators.required],
        name: ['', Validators.required],
        outputFilePath: [''],
        exportedDate: [''],
        includeInstructions: [false],
        includePartsList: [false],
        active: [true],
        deleted: [false],
      });


  public projectExportId: string | null = null;
  public projectExportData: ProjectExportData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  projectExports$ = this.projectExportService.GetProjectExportList();
  public projects$ = this.projectService.GetProjectList();
  public exportFormats$ = this.exportFormatService.GetExportFormatList();

  private destroy$ = new Subject<void>();

  constructor(
    public projectExportService: ProjectExportService,
    public projectService: ProjectService,
    public exportFormatService: ExportFormatService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the projectExportId from the route parameters
    this.projectExportId = this.route.snapshot.paramMap.get('projectExportId');

    if (this.projectExportId === 'new' ||
        this.projectExportId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.projectExportData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.projectExportForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectExportForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Project Export';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Project Export';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.projectExportForm.dirty) {
      return confirm('You have unsaved Project Export changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.projectExportId != null && this.projectExportId !== 'new') {

      const id = parseInt(this.projectExportId, 10);

      if (!isNaN(id)) {
        return { projectExportId: id };
      }
    }

    return null;
  }


/*
  * Loads the ProjectExport data for the current projectExportId.
  *
  * Fully respects the ProjectExportService caching strategy and error handling strategy.
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
    if (!this.projectExportService.userIsBMCProjectExportReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ProjectExports.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate projectExportId
    //
    if (!this.projectExportId) {

      this.alertService.showMessage('No ProjectExport ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const projectExportId = Number(this.projectExportId);

    if (isNaN(projectExportId) || projectExportId <= 0) {

      this.alertService.showMessage(`Invalid Project Export ID: "${this.projectExportId}"`,
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
      // This is the most targeted way: clear only this ProjectExport + relations

      this.projectExportService.ClearRecordCache(projectExportId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.projectExportService.GetProjectExport(projectExportId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (projectExportData) => {

        //
        // Success path — projectExportData can legitimately be null if 404'd but request succeeded
        //
        if (!projectExportData) {

          this.handleProjectExportNotFound(projectExportId);

        } else {

          this.projectExportData = projectExportData;
          this.buildFormValues(this.projectExportData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ProjectExport loaded successfully',
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
        this.handleProjectExportLoadError(error, projectExportId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleProjectExportNotFound(projectExportId: number): void {

    this.projectExportData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ProjectExport #${projectExportId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleProjectExportLoadError(error: any, projectExportId: number): void {

    let message = 'Failed to load Project Export.';
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
          message = 'You do not have permission to view this Project Export.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Project Export #${projectExportId} was not found.`;
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

    console.error(`Project Export load failed (ID: ${projectExportId})`, error);

    //
    // Reset UI to safe state
    //
    this.projectExportData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(projectExportData: ProjectExportData | null) {

    if (projectExportData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectExportForm.reset({
        projectId: null,
        exportFormatId: null,
        name: '',
        outputFilePath: '',
        exportedDate: '',
        includeInstructions: false,
        includePartsList: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectExportForm.reset({
        projectId: projectExportData.projectId,
        exportFormatId: projectExportData.exportFormatId,
        name: projectExportData.name ?? '',
        outputFilePath: projectExportData.outputFilePath ?? '',
        exportedDate: isoUtcStringToDateTimeLocal(projectExportData.exportedDate) ?? '',
        includeInstructions: projectExportData.includeInstructions ?? false,
        includePartsList: projectExportData.includePartsList ?? false,
        active: projectExportData.active ?? true,
        deleted: projectExportData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectExportForm.markAsPristine();
    this.projectExportForm.markAsUntouched();
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

    if (this.projectExportService.userIsBMCProjectExportWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Project Exports", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.projectExportForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectExportForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectExportForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectExportSubmitData: ProjectExportSubmitData = {
        id: this.projectExportData?.id || 0,
        projectId: Number(formValue.projectId),
        exportFormatId: Number(formValue.exportFormatId),
        name: formValue.name!.trim(),
        outputFilePath: formValue.outputFilePath?.trim() || null,
        exportedDate: formValue.exportedDate ? dateTimeLocalToIsoUtc(formValue.exportedDate.trim()) : null,
        includeInstructions: !!formValue.includeInstructions,
        includePartsList: !!formValue.includePartsList,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.projectExportService.PutProjectExport(projectExportSubmitData.id, projectExportSubmitData)
      : this.projectExportService.PostProjectExport(projectExportSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedProjectExportData) => {

        this.projectExportService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Project Export's detail page
          //
          this.projectExportForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.projectExportForm.markAsUntouched();

          this.router.navigate(['/projectexports', savedProjectExportData.id]);
          this.alertService.showMessage('Project Export added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.projectExportData = savedProjectExportData;
          this.buildFormValues(this.projectExportData);

          this.alertService.showMessage("Project Export saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Project Export.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Export.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Export could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCProjectExportReader(): boolean {
    return this.projectExportService.userIsBMCProjectExportReader();
  }

  public userIsBMCProjectExportWriter(): boolean {
    return this.projectExportService.userIsBMCProjectExportWriter();
  }
}

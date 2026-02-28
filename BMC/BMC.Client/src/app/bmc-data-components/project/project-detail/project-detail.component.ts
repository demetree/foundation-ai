/*
   GENERATED FORM FOR THE PROJECT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Project table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectService, ProjectData, ProjectSubmitData } from '../../../bmc-data-services/project.service';
import { ProjectChangeHistoryService } from '../../../bmc-data-services/project-change-history.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
import { BrickConnectionService } from '../../../bmc-data-services/brick-connection.service';
import { SubmodelService } from '../../../bmc-data-services/submodel.service';
import { ProjectTagAssignmentService } from '../../../bmc-data-services/project-tag-assignment.service';
import { ProjectCameraPresetService } from '../../../bmc-data-services/project-camera-preset.service';
import { ProjectReferenceImageService } from '../../../bmc-data-services/project-reference-image.service';
import { ModelDocumentService } from '../../../bmc-data-services/model-document.service';
import { BuildManualService } from '../../../bmc-data-services/build-manual.service';
import { ProjectRenderService } from '../../../bmc-data-services/project-render.service';
import { ProjectExportService } from '../../../bmc-data-services/project-export.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
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
interface ProjectFormValues {
  name: string,
  description: string,
  notes: string | null,
  thumbnailImagePath: string | null,
  partCount: string | null,     // Stored as string for form input, converted to number on submit.
  lastBuildDate: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-project-detail',
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.scss']
})

export class ProjectDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        notes: [''],
        thumbnailImagePath: [''],
        partCount: [''],
        lastBuildDate: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public projectId: string | null = null;
  public projectData: ProjectData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  projects$ = this.projectService.GetProjectList();
  public projectChangeHistories$ = this.projectChangeHistoryService.GetProjectChangeHistoryList();
  public placedBricks$ = this.placedBrickService.GetPlacedBrickList();
  public brickConnections$ = this.brickConnectionService.GetBrickConnectionList();
  public submodels$ = this.submodelService.GetSubmodelList();
  public projectTagAssignments$ = this.projectTagAssignmentService.GetProjectTagAssignmentList();
  public projectCameraPresets$ = this.projectCameraPresetService.GetProjectCameraPresetList();
  public projectReferenceImages$ = this.projectReferenceImageService.GetProjectReferenceImageList();
  public modelDocuments$ = this.modelDocumentService.GetModelDocumentList();
  public buildManuals$ = this.buildManualService.GetBuildManualList();
  public projectRenders$ = this.projectRenderService.GetProjectRenderList();
  public projectExports$ = this.projectExportService.GetProjectExportList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  private destroy$ = new Subject<void>();

  constructor(
    public projectService: ProjectService,
    public projectChangeHistoryService: ProjectChangeHistoryService,
    public placedBrickService: PlacedBrickService,
    public brickConnectionService: BrickConnectionService,
    public submodelService: SubmodelService,
    public projectTagAssignmentService: ProjectTagAssignmentService,
    public projectCameraPresetService: ProjectCameraPresetService,
    public projectReferenceImageService: ProjectReferenceImageService,
    public modelDocumentService: ModelDocumentService,
    public buildManualService: BuildManualService,
    public projectRenderService: ProjectRenderService,
    public projectExportService: ProjectExportService,
    public publishedMocService: PublishedMocService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the projectId from the route parameters
    this.projectId = this.route.snapshot.paramMap.get('projectId');

    if (this.projectId === 'new' ||
        this.projectId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.projectData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.projectForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Project';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Project';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.projectForm.dirty) {
      return confirm('You have unsaved Project changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.projectId != null && this.projectId !== 'new') {

      const id = parseInt(this.projectId, 10);

      if (!isNaN(id)) {
        return { projectId: id };
      }
    }

    return null;
  }


/*
  * Loads the Project data for the current projectId.
  *
  * Fully respects the ProjectService caching strategy and error handling strategy.
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
    if (!this.projectService.userIsBMCProjectReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Projects.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate projectId
    //
    if (!this.projectId) {

      this.alertService.showMessage('No Project ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const projectId = Number(this.projectId);

    if (isNaN(projectId) || projectId <= 0) {

      this.alertService.showMessage(`Invalid Project ID: "${this.projectId}"`,
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
      // This is the most targeted way: clear only this Project + relations

      this.projectService.ClearRecordCache(projectId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.projectService.GetProject(projectId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (projectData) => {

        //
        // Success path — projectData can legitimately be null if 404'd but request succeeded
        //
        if (!projectData) {

          this.handleProjectNotFound(projectId);

        } else {

          this.projectData = projectData;
          this.buildFormValues(this.projectData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Project loaded successfully',
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
        this.handleProjectLoadError(error, projectId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleProjectNotFound(projectId: number): void {

    this.projectData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Project #${projectId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleProjectLoadError(error: any, projectId: number): void {

    let message = 'Failed to load Project.';
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
          message = 'You do not have permission to view this Project.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Project #${projectId} was not found.`;
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

    console.error(`Project load failed (ID: ${projectId})`, error);

    //
    // Reset UI to safe state
    //
    this.projectData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(projectData: ProjectData | null) {

    if (projectData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectForm.reset({
        name: '',
        description: '',
        notes: '',
        thumbnailImagePath: '',
        partCount: '',
        lastBuildDate: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectForm.reset({
        name: projectData.name ?? '',
        description: projectData.description ?? '',
        notes: projectData.notes ?? '',
        thumbnailImagePath: projectData.thumbnailImagePath ?? '',
        partCount: projectData.partCount?.toString() ?? '',
        lastBuildDate: isoUtcStringToDateTimeLocal(projectData.lastBuildDate) ?? '',
        versionNumber: projectData.versionNumber?.toString() ?? '',
        active: projectData.active ?? true,
        deleted: projectData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectForm.markAsPristine();
    this.projectForm.markAsUntouched();
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

    if (this.projectService.userIsBMCProjectWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Projects", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.projectForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectSubmitData: ProjectSubmitData = {
        id: this.projectData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        notes: formValue.notes?.trim() || null,
        thumbnailImagePath: formValue.thumbnailImagePath?.trim() || null,
        partCount: formValue.partCount ? Number(formValue.partCount) : null,
        lastBuildDate: formValue.lastBuildDate ? dateTimeLocalToIsoUtc(formValue.lastBuildDate.trim()) : null,
        versionNumber: this.projectData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.projectService.PutProject(projectSubmitData.id, projectSubmitData)
      : this.projectService.PostProject(projectSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedProjectData) => {

        this.projectService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Project's detail page
          //
          this.projectForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.projectForm.markAsUntouched();

          this.router.navigate(['/projects', savedProjectData.id]);
          this.alertService.showMessage('Project added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.projectData = savedProjectData;
          this.buildFormValues(this.projectData);

          this.alertService.showMessage("Project saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Project.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCProjectReader(): boolean {
    return this.projectService.userIsBMCProjectReader();
  }

  public userIsBMCProjectWriter(): boolean {
    return this.projectService.userIsBMCProjectWriter();
  }
}

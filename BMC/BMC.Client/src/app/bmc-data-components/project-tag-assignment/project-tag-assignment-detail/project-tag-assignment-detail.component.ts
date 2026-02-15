/*
   GENERATED FORM FOR THE PROJECTTAGASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectTagAssignment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-tag-assignment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectTagAssignmentService, ProjectTagAssignmentData, ProjectTagAssignmentSubmitData } from '../../../bmc-data-services/project-tag-assignment.service';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { ProjectTagService } from '../../../bmc-data-services/project-tag.service';
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
interface ProjectTagAssignmentFormValues {
  projectId: number | bigint,       // For FK link number
  projectTagId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-project-tag-assignment-detail',
  templateUrl: './project-tag-assignment-detail.component.html',
  styleUrls: ['./project-tag-assignment-detail.component.scss']
})

export class ProjectTagAssignmentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectTagAssignmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectTagAssignmentForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        projectTagId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });


  public projectTagAssignmentId: string | null = null;
  public projectTagAssignmentData: ProjectTagAssignmentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  projectTagAssignments$ = this.projectTagAssignmentService.GetProjectTagAssignmentList();
  public projects$ = this.projectService.GetProjectList();
  public projectTags$ = this.projectTagService.GetProjectTagList();

  private destroy$ = new Subject<void>();

  constructor(
    public projectTagAssignmentService: ProjectTagAssignmentService,
    public projectService: ProjectService,
    public projectTagService: ProjectTagService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the projectTagAssignmentId from the route parameters
    this.projectTagAssignmentId = this.route.snapshot.paramMap.get('projectTagAssignmentId');

    if (this.projectTagAssignmentId === 'new' ||
        this.projectTagAssignmentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.projectTagAssignmentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.projectTagAssignmentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectTagAssignmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Project Tag Assignment';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Project Tag Assignment';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.projectTagAssignmentForm.dirty) {
      return confirm('You have unsaved Project Tag Assignment changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.projectTagAssignmentId != null && this.projectTagAssignmentId !== 'new') {

      const id = parseInt(this.projectTagAssignmentId, 10);

      if (!isNaN(id)) {
        return { projectTagAssignmentId: id };
      }
    }

    return null;
  }


/*
  * Loads the ProjectTagAssignment data for the current projectTagAssignmentId.
  *
  * Fully respects the ProjectTagAssignmentService caching strategy and error handling strategy.
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
    if (!this.projectTagAssignmentService.userIsBMCProjectTagAssignmentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ProjectTagAssignments.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate projectTagAssignmentId
    //
    if (!this.projectTagAssignmentId) {

      this.alertService.showMessage('No ProjectTagAssignment ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const projectTagAssignmentId = Number(this.projectTagAssignmentId);

    if (isNaN(projectTagAssignmentId) || projectTagAssignmentId <= 0) {

      this.alertService.showMessage(`Invalid Project Tag Assignment ID: "${this.projectTagAssignmentId}"`,
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
      // This is the most targeted way: clear only this ProjectTagAssignment + relations

      this.projectTagAssignmentService.ClearRecordCache(projectTagAssignmentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.projectTagAssignmentService.GetProjectTagAssignment(projectTagAssignmentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (projectTagAssignmentData) => {

        //
        // Success path — projectTagAssignmentData can legitimately be null if 404'd but request succeeded
        //
        if (!projectTagAssignmentData) {

          this.handleProjectTagAssignmentNotFound(projectTagAssignmentId);

        } else {

          this.projectTagAssignmentData = projectTagAssignmentData;
          this.buildFormValues(this.projectTagAssignmentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ProjectTagAssignment loaded successfully',
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
        this.handleProjectTagAssignmentLoadError(error, projectTagAssignmentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleProjectTagAssignmentNotFound(projectTagAssignmentId: number): void {

    this.projectTagAssignmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ProjectTagAssignment #${projectTagAssignmentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleProjectTagAssignmentLoadError(error: any, projectTagAssignmentId: number): void {

    let message = 'Failed to load Project Tag Assignment.';
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
          message = 'You do not have permission to view this Project Tag Assignment.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Project Tag Assignment #${projectTagAssignmentId} was not found.`;
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

    console.error(`Project Tag Assignment load failed (ID: ${projectTagAssignmentId})`, error);

    //
    // Reset UI to safe state
    //
    this.projectTagAssignmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(projectTagAssignmentData: ProjectTagAssignmentData | null) {

    if (projectTagAssignmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectTagAssignmentForm.reset({
        projectId: null,
        projectTagId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectTagAssignmentForm.reset({
        projectId: projectTagAssignmentData.projectId,
        projectTagId: projectTagAssignmentData.projectTagId,
        active: projectTagAssignmentData.active ?? true,
        deleted: projectTagAssignmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectTagAssignmentForm.markAsPristine();
    this.projectTagAssignmentForm.markAsUntouched();
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

    if (this.projectTagAssignmentService.userIsBMCProjectTagAssignmentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Project Tag Assignments", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.projectTagAssignmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectTagAssignmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectTagAssignmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectTagAssignmentSubmitData: ProjectTagAssignmentSubmitData = {
        id: this.projectTagAssignmentData?.id || 0,
        projectId: Number(formValue.projectId),
        projectTagId: Number(formValue.projectTagId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.projectTagAssignmentService.PutProjectTagAssignment(projectTagAssignmentSubmitData.id, projectTagAssignmentSubmitData)
      : this.projectTagAssignmentService.PostProjectTagAssignment(projectTagAssignmentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedProjectTagAssignmentData) => {

        this.projectTagAssignmentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Project Tag Assignment's detail page
          //
          this.projectTagAssignmentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.projectTagAssignmentForm.markAsUntouched();

          this.router.navigate(['/projecttagassignments', savedProjectTagAssignmentData.id]);
          this.alertService.showMessage('Project Tag Assignment added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.projectTagAssignmentData = savedProjectTagAssignmentData;
          this.buildFormValues(this.projectTagAssignmentData);

          this.alertService.showMessage("Project Tag Assignment saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Project Tag Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Tag Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Tag Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCProjectTagAssignmentReader(): boolean {
    return this.projectTagAssignmentService.userIsBMCProjectTagAssignmentReader();
  }

  public userIsBMCProjectTagAssignmentWriter(): boolean {
    return this.projectTagAssignmentService.userIsBMCProjectTagAssignmentWriter();
  }
}

/*
   GENERATED FORM FOR THE PROJECTREFERENCEIMAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ProjectReferenceImage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to project-reference-image-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ProjectReferenceImageService, ProjectReferenceImageData, ProjectReferenceImageSubmitData } from '../../../bmc-data-services/project-reference-image.service';
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
interface ProjectReferenceImageFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  imageFilePath: string | null,
  opacity: string | null,     // Stored as string for form input, converted to number on submit.
  positionX: string | null,     // Stored as string for form input, converted to number on submit.
  positionY: string | null,     // Stored as string for form input, converted to number on submit.
  positionZ: string | null,     // Stored as string for form input, converted to number on submit.
  scaleX: string | null,     // Stored as string for form input, converted to number on submit.
  scaleY: string | null,     // Stored as string for form input, converted to number on submit.
  isVisible: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-project-reference-image-detail',
  templateUrl: './project-reference-image-detail.component.html',
  styleUrls: ['./project-reference-image-detail.component.scss']
})

export class ProjectReferenceImageDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ProjectReferenceImageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public projectReferenceImageForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        imageFilePath: [''],
        opacity: [''],
        positionX: [''],
        positionY: [''],
        positionZ: [''],
        scaleX: [''],
        scaleY: [''],
        isVisible: [false],
        active: [true],
        deleted: [false],
      });


  public projectReferenceImageId: string | null = null;
  public projectReferenceImageData: ProjectReferenceImageData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  projectReferenceImages$ = this.projectReferenceImageService.GetProjectReferenceImageList();
  public projects$ = this.projectService.GetProjectList();

  private destroy$ = new Subject<void>();

  constructor(
    public projectReferenceImageService: ProjectReferenceImageService,
    public projectService: ProjectService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the projectReferenceImageId from the route parameters
    this.projectReferenceImageId = this.route.snapshot.paramMap.get('projectReferenceImageId');

    if (this.projectReferenceImageId === 'new' ||
        this.projectReferenceImageId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.projectReferenceImageData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.projectReferenceImageForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.projectReferenceImageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Project Reference Image';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Project Reference Image';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.projectReferenceImageForm.dirty) {
      return confirm('You have unsaved Project Reference Image changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.projectReferenceImageId != null && this.projectReferenceImageId !== 'new') {

      const id = parseInt(this.projectReferenceImageId, 10);

      if (!isNaN(id)) {
        return { projectReferenceImageId: id };
      }
    }

    return null;
  }


/*
  * Loads the ProjectReferenceImage data for the current projectReferenceImageId.
  *
  * Fully respects the ProjectReferenceImageService caching strategy and error handling strategy.
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
    if (!this.projectReferenceImageService.userIsBMCProjectReferenceImageReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ProjectReferenceImages.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate projectReferenceImageId
    //
    if (!this.projectReferenceImageId) {

      this.alertService.showMessage('No ProjectReferenceImage ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const projectReferenceImageId = Number(this.projectReferenceImageId);

    if (isNaN(projectReferenceImageId) || projectReferenceImageId <= 0) {

      this.alertService.showMessage(`Invalid Project Reference Image ID: "${this.projectReferenceImageId}"`,
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
      // This is the most targeted way: clear only this ProjectReferenceImage + relations

      this.projectReferenceImageService.ClearRecordCache(projectReferenceImageId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.projectReferenceImageService.GetProjectReferenceImage(projectReferenceImageId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (projectReferenceImageData) => {

        //
        // Success path — projectReferenceImageData can legitimately be null if 404'd but request succeeded
        //
        if (!projectReferenceImageData) {

          this.handleProjectReferenceImageNotFound(projectReferenceImageId);

        } else {

          this.projectReferenceImageData = projectReferenceImageData;
          this.buildFormValues(this.projectReferenceImageData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ProjectReferenceImage loaded successfully',
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
        this.handleProjectReferenceImageLoadError(error, projectReferenceImageId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleProjectReferenceImageNotFound(projectReferenceImageId: number): void {

    this.projectReferenceImageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ProjectReferenceImage #${projectReferenceImageId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleProjectReferenceImageLoadError(error: any, projectReferenceImageId: number): void {

    let message = 'Failed to load Project Reference Image.';
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
          message = 'You do not have permission to view this Project Reference Image.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Project Reference Image #${projectReferenceImageId} was not found.`;
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

    console.error(`Project Reference Image load failed (ID: ${projectReferenceImageId})`, error);

    //
    // Reset UI to safe state
    //
    this.projectReferenceImageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(projectReferenceImageData: ProjectReferenceImageData | null) {

    if (projectReferenceImageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.projectReferenceImageForm.reset({
        projectId: null,
        name: '',
        imageFilePath: '',
        opacity: '',
        positionX: '',
        positionY: '',
        positionZ: '',
        scaleX: '',
        scaleY: '',
        isVisible: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.projectReferenceImageForm.reset({
        projectId: projectReferenceImageData.projectId,
        name: projectReferenceImageData.name ?? '',
        imageFilePath: projectReferenceImageData.imageFilePath ?? '',
        opacity: projectReferenceImageData.opacity?.toString() ?? '',
        positionX: projectReferenceImageData.positionX?.toString() ?? '',
        positionY: projectReferenceImageData.positionY?.toString() ?? '',
        positionZ: projectReferenceImageData.positionZ?.toString() ?? '',
        scaleX: projectReferenceImageData.scaleX?.toString() ?? '',
        scaleY: projectReferenceImageData.scaleY?.toString() ?? '',
        isVisible: projectReferenceImageData.isVisible ?? false,
        active: projectReferenceImageData.active ?? true,
        deleted: projectReferenceImageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.projectReferenceImageForm.markAsPristine();
    this.projectReferenceImageForm.markAsUntouched();
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

    if (this.projectReferenceImageService.userIsBMCProjectReferenceImageWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Project Reference Images", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.projectReferenceImageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.projectReferenceImageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.projectReferenceImageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const projectReferenceImageSubmitData: ProjectReferenceImageSubmitData = {
        id: this.projectReferenceImageData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        imageFilePath: formValue.imageFilePath?.trim() || null,
        opacity: formValue.opacity ? Number(formValue.opacity) : null,
        positionX: formValue.positionX ? Number(formValue.positionX) : null,
        positionY: formValue.positionY ? Number(formValue.positionY) : null,
        positionZ: formValue.positionZ ? Number(formValue.positionZ) : null,
        scaleX: formValue.scaleX ? Number(formValue.scaleX) : null,
        scaleY: formValue.scaleY ? Number(formValue.scaleY) : null,
        isVisible: !!formValue.isVisible,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.projectReferenceImageService.PutProjectReferenceImage(projectReferenceImageSubmitData.id, projectReferenceImageSubmitData)
      : this.projectReferenceImageService.PostProjectReferenceImage(projectReferenceImageSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedProjectReferenceImageData) => {

        this.projectReferenceImageService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Project Reference Image's detail page
          //
          this.projectReferenceImageForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.projectReferenceImageForm.markAsUntouched();

          this.router.navigate(['/projectreferenceimages', savedProjectReferenceImageData.id]);
          this.alertService.showMessage('Project Reference Image added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.projectReferenceImageData = savedProjectReferenceImageData;
          this.buildFormValues(this.projectReferenceImageData);

          this.alertService.showMessage("Project Reference Image saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Project Reference Image.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Project Reference Image.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Project Reference Image could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCProjectReferenceImageReader(): boolean {
    return this.projectReferenceImageService.userIsBMCProjectReferenceImageReader();
  }

  public userIsBMCProjectReferenceImageWriter(): boolean {
    return this.projectReferenceImageService.userIsBMCProjectReferenceImageWriter();
  }
}

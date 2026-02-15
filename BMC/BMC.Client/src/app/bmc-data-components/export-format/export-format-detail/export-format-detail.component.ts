/*
   GENERATED FORM FOR THE EXPORTFORMAT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ExportFormat table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to export-format-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ExportFormatService, ExportFormatData, ExportFormatSubmitData } from '../../../bmc-data-services/export-format.service';
import { ProjectExportService } from '../../../bmc-data-services/project-export.service';
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
interface ExportFormatFormValues {
  name: string,
  description: string,
  fileExtension: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-export-format-detail',
  templateUrl: './export-format-detail.component.html',
  styleUrls: ['./export-format-detail.component.scss']
})

export class ExportFormatDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ExportFormatFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public exportFormatForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        fileExtension: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public exportFormatId: string | null = null;
  public exportFormatData: ExportFormatData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  exportFormats$ = this.exportFormatService.GetExportFormatList();
  public projectExports$ = this.projectExportService.GetProjectExportList();

  private destroy$ = new Subject<void>();

  constructor(
    public exportFormatService: ExportFormatService,
    public projectExportService: ProjectExportService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the exportFormatId from the route parameters
    this.exportFormatId = this.route.snapshot.paramMap.get('exportFormatId');

    if (this.exportFormatId === 'new' ||
        this.exportFormatId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.exportFormatData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.exportFormatForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.exportFormatForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Export Format';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Export Format';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.exportFormatForm.dirty) {
      return confirm('You have unsaved Export Format changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.exportFormatId != null && this.exportFormatId !== 'new') {

      const id = parseInt(this.exportFormatId, 10);

      if (!isNaN(id)) {
        return { exportFormatId: id };
      }
    }

    return null;
  }


/*
  * Loads the ExportFormat data for the current exportFormatId.
  *
  * Fully respects the ExportFormatService caching strategy and error handling strategy.
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
    if (!this.exportFormatService.userIsBMCExportFormatReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ExportFormats.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate exportFormatId
    //
    if (!this.exportFormatId) {

      this.alertService.showMessage('No ExportFormat ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const exportFormatId = Number(this.exportFormatId);

    if (isNaN(exportFormatId) || exportFormatId <= 0) {

      this.alertService.showMessage(`Invalid Export Format ID: "${this.exportFormatId}"`,
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
      // This is the most targeted way: clear only this ExportFormat + relations

      this.exportFormatService.ClearRecordCache(exportFormatId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.exportFormatService.GetExportFormat(exportFormatId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (exportFormatData) => {

        //
        // Success path — exportFormatData can legitimately be null if 404'd but request succeeded
        //
        if (!exportFormatData) {

          this.handleExportFormatNotFound(exportFormatId);

        } else {

          this.exportFormatData = exportFormatData;
          this.buildFormValues(this.exportFormatData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ExportFormat loaded successfully',
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
        this.handleExportFormatLoadError(error, exportFormatId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleExportFormatNotFound(exportFormatId: number): void {

    this.exportFormatData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ExportFormat #${exportFormatId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleExportFormatLoadError(error: any, exportFormatId: number): void {

    let message = 'Failed to load Export Format.';
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
          message = 'You do not have permission to view this Export Format.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Export Format #${exportFormatId} was not found.`;
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

    console.error(`Export Format load failed (ID: ${exportFormatId})`, error);

    //
    // Reset UI to safe state
    //
    this.exportFormatData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(exportFormatData: ExportFormatData | null) {

    if (exportFormatData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.exportFormatForm.reset({
        name: '',
        description: '',
        fileExtension: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.exportFormatForm.reset({
        name: exportFormatData.name ?? '',
        description: exportFormatData.description ?? '',
        fileExtension: exportFormatData.fileExtension ?? '',
        sequence: exportFormatData.sequence?.toString() ?? '',
        active: exportFormatData.active ?? true,
        deleted: exportFormatData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.exportFormatForm.markAsPristine();
    this.exportFormatForm.markAsUntouched();
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

    if (this.exportFormatService.userIsBMCExportFormatWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Export Formats", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.exportFormatForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.exportFormatForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.exportFormatForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const exportFormatSubmitData: ExportFormatSubmitData = {
        id: this.exportFormatData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        fileExtension: formValue.fileExtension?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.exportFormatService.PutExportFormat(exportFormatSubmitData.id, exportFormatSubmitData)
      : this.exportFormatService.PostExportFormat(exportFormatSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedExportFormatData) => {

        this.exportFormatService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Export Format's detail page
          //
          this.exportFormatForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.exportFormatForm.markAsUntouched();

          this.router.navigate(['/exportformats', savedExportFormatData.id]);
          this.alertService.showMessage('Export Format added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.exportFormatData = savedExportFormatData;
          this.buildFormValues(this.exportFormatData);

          this.alertService.showMessage("Export Format saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Export Format.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Export Format.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Export Format could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCExportFormatReader(): boolean {
    return this.exportFormatService.userIsBMCExportFormatReader();
  }

  public userIsBMCExportFormatWriter(): boolean {
    return this.exportFormatService.userIsBMCExportFormatWriter();
  }
}

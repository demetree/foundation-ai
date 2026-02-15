/*
   GENERATED FORM FOR THE CONTENTREPORTREASON TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContentReportReason table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to content-report-reason-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContentReportReasonService, ContentReportReasonData, ContentReportReasonSubmitData } from '../../../bmc-data-services/content-report-reason.service';
import { ContentReportService } from '../../../bmc-data-services/content-report.service';
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
interface ContentReportReasonFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-content-report-reason-detail',
  templateUrl: './content-report-reason-detail.component.html',
  styleUrls: ['./content-report-reason-detail.component.scss']
})

export class ContentReportReasonDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContentReportReasonFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contentReportReasonForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public contentReportReasonId: string | null = null;
  public contentReportReasonData: ContentReportReasonData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  contentReportReasons$ = this.contentReportReasonService.GetContentReportReasonList();
  public contentReports$ = this.contentReportService.GetContentReportList();

  private destroy$ = new Subject<void>();

  constructor(
    public contentReportReasonService: ContentReportReasonService,
    public contentReportService: ContentReportService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the contentReportReasonId from the route parameters
    this.contentReportReasonId = this.route.snapshot.paramMap.get('contentReportReasonId');

    if (this.contentReportReasonId === 'new' ||
        this.contentReportReasonId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contentReportReasonData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.contentReportReasonForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contentReportReasonForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Content Report Reason';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Content Report Reason';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.contentReportReasonForm.dirty) {
      return confirm('You have unsaved Content Report Reason changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.contentReportReasonId != null && this.contentReportReasonId !== 'new') {

      const id = parseInt(this.contentReportReasonId, 10);

      if (!isNaN(id)) {
        return { contentReportReasonId: id };
      }
    }

    return null;
  }


/*
  * Loads the ContentReportReason data for the current contentReportReasonId.
  *
  * Fully respects the ContentReportReasonService caching strategy and error handling strategy.
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
    if (!this.contentReportReasonService.userIsBMCContentReportReasonReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ContentReportReasons.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contentReportReasonId
    //
    if (!this.contentReportReasonId) {

      this.alertService.showMessage('No ContentReportReason ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contentReportReasonId = Number(this.contentReportReasonId);

    if (isNaN(contentReportReasonId) || contentReportReasonId <= 0) {

      this.alertService.showMessage(`Invalid Content Report Reason ID: "${this.contentReportReasonId}"`,
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
      // This is the most targeted way: clear only this ContentReportReason + relations

      this.contentReportReasonService.ClearRecordCache(contentReportReasonId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contentReportReasonService.GetContentReportReason(contentReportReasonId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contentReportReasonData) => {

        //
        // Success path — contentReportReasonData can legitimately be null if 404'd but request succeeded
        //
        if (!contentReportReasonData) {

          this.handleContentReportReasonNotFound(contentReportReasonId);

        } else {

          this.contentReportReasonData = contentReportReasonData;
          this.buildFormValues(this.contentReportReasonData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ContentReportReason loaded successfully',
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
        this.handleContentReportReasonLoadError(error, contentReportReasonId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleContentReportReasonNotFound(contentReportReasonId: number): void {

    this.contentReportReasonData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ContentReportReason #${contentReportReasonId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContentReportReasonLoadError(error: any, contentReportReasonId: number): void {

    let message = 'Failed to load Content Report Reason.';
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
          message = 'You do not have permission to view this Content Report Reason.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Content Report Reason #${contentReportReasonId} was not found.`;
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

    console.error(`Content Report Reason load failed (ID: ${contentReportReasonId})`, error);

    //
    // Reset UI to safe state
    //
    this.contentReportReasonData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(contentReportReasonData: ContentReportReasonData | null) {

    if (contentReportReasonData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contentReportReasonForm.reset({
        name: '',
        description: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contentReportReasonForm.reset({
        name: contentReportReasonData.name ?? '',
        description: contentReportReasonData.description ?? '',
        sequence: contentReportReasonData.sequence?.toString() ?? '',
        active: contentReportReasonData.active ?? true,
        deleted: contentReportReasonData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contentReportReasonForm.markAsPristine();
    this.contentReportReasonForm.markAsUntouched();
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

    if (this.contentReportReasonService.userIsBMCContentReportReasonWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Content Report Reasons", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.contentReportReasonForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contentReportReasonForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contentReportReasonForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contentReportReasonSubmitData: ContentReportReasonSubmitData = {
        id: this.contentReportReasonData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.contentReportReasonService.PutContentReportReason(contentReportReasonSubmitData.id, contentReportReasonSubmitData)
      : this.contentReportReasonService.PostContentReportReason(contentReportReasonSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedContentReportReasonData) => {

        this.contentReportReasonService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Content Report Reason's detail page
          //
          this.contentReportReasonForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.contentReportReasonForm.markAsUntouched();

          this.router.navigate(['/contentreportreasons', savedContentReportReasonData.id]);
          this.alertService.showMessage('Content Report Reason added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.contentReportReasonData = savedContentReportReasonData;
          this.buildFormValues(this.contentReportReasonData);

          this.alertService.showMessage("Content Report Reason saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Content Report Reason.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Content Report Reason.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Content Report Reason could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCContentReportReasonReader(): boolean {
    return this.contentReportReasonService.userIsBMCContentReportReasonReader();
  }

  public userIsBMCContentReportReasonWriter(): boolean {
    return this.contentReportReasonService.userIsBMCContentReportReasonWriter();
  }
}

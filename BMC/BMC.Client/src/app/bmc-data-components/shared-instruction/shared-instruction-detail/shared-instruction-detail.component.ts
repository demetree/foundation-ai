/*
   GENERATED FORM FOR THE SHAREDINSTRUCTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SharedInstruction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to shared-instruction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SharedInstructionService, SharedInstructionData, SharedInstructionSubmitData } from '../../../bmc-data-services/shared-instruction.service';
import { BuildManualService } from '../../../bmc-data-services/build-manual.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { SharedInstructionChangeHistoryService } from '../../../bmc-data-services/shared-instruction-change-history.service';
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
interface SharedInstructionFormValues {
  buildManualId: number | bigint | null,       // For FK link number
  publishedMocId: number | bigint | null,       // For FK link number
  name: string,
  description: string | null,
  formatType: string,
  filePath: string | null,
  isPublished: boolean,
  publishedDate: string | null,
  downloadCount: string,     // Stored as string for form input, converted to number on submit.
  pageCount: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-shared-instruction-detail',
  templateUrl: './shared-instruction-detail.component.html',
  styleUrls: ['./shared-instruction-detail.component.scss']
})

export class SharedInstructionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SharedInstructionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public sharedInstructionForm: FormGroup = this.fb.group({
        buildManualId: [null],
        publishedMocId: [null],
        name: ['', Validators.required],
        description: [''],
        formatType: ['', Validators.required],
        filePath: [''],
        isPublished: [false],
        publishedDate: [''],
        downloadCount: ['', Validators.required],
        pageCount: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public sharedInstructionId: string | null = null;
  public sharedInstructionData: SharedInstructionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  sharedInstructions$ = this.sharedInstructionService.GetSharedInstructionList();
  public buildManuals$ = this.buildManualService.GetBuildManualList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();
  public sharedInstructionChangeHistories$ = this.sharedInstructionChangeHistoryService.GetSharedInstructionChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public sharedInstructionService: SharedInstructionService,
    public buildManualService: BuildManualService,
    public publishedMocService: PublishedMocService,
    public sharedInstructionChangeHistoryService: SharedInstructionChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the sharedInstructionId from the route parameters
    this.sharedInstructionId = this.route.snapshot.paramMap.get('sharedInstructionId');

    if (this.sharedInstructionId === 'new' ||
        this.sharedInstructionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.sharedInstructionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.sharedInstructionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.sharedInstructionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Shared Instruction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Shared Instruction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.sharedInstructionForm.dirty) {
      return confirm('You have unsaved Shared Instruction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.sharedInstructionId != null && this.sharedInstructionId !== 'new') {

      const id = parseInt(this.sharedInstructionId, 10);

      if (!isNaN(id)) {
        return { sharedInstructionId: id };
      }
    }

    return null;
  }


/*
  * Loads the SharedInstruction data for the current sharedInstructionId.
  *
  * Fully respects the SharedInstructionService caching strategy and error handling strategy.
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
    if (!this.sharedInstructionService.userIsBMCSharedInstructionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SharedInstructions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate sharedInstructionId
    //
    if (!this.sharedInstructionId) {

      this.alertService.showMessage('No SharedInstruction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const sharedInstructionId = Number(this.sharedInstructionId);

    if (isNaN(sharedInstructionId) || sharedInstructionId <= 0) {

      this.alertService.showMessage(`Invalid Shared Instruction ID: "${this.sharedInstructionId}"`,
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
      // This is the most targeted way: clear only this SharedInstruction + relations

      this.sharedInstructionService.ClearRecordCache(sharedInstructionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.sharedInstructionService.GetSharedInstruction(sharedInstructionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (sharedInstructionData) => {

        //
        // Success path — sharedInstructionData can legitimately be null if 404'd but request succeeded
        //
        if (!sharedInstructionData) {

          this.handleSharedInstructionNotFound(sharedInstructionId);

        } else {

          this.sharedInstructionData = sharedInstructionData;
          this.buildFormValues(this.sharedInstructionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SharedInstruction loaded successfully',
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
        this.handleSharedInstructionLoadError(error, sharedInstructionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSharedInstructionNotFound(sharedInstructionId: number): void {

    this.sharedInstructionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SharedInstruction #${sharedInstructionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSharedInstructionLoadError(error: any, sharedInstructionId: number): void {

    let message = 'Failed to load Shared Instruction.';
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
          message = 'You do not have permission to view this Shared Instruction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Shared Instruction #${sharedInstructionId} was not found.`;
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

    console.error(`Shared Instruction load failed (ID: ${sharedInstructionId})`, error);

    //
    // Reset UI to safe state
    //
    this.sharedInstructionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(sharedInstructionData: SharedInstructionData | null) {

    if (sharedInstructionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.sharedInstructionForm.reset({
        buildManualId: null,
        publishedMocId: null,
        name: '',
        description: '',
        formatType: '',
        filePath: '',
        isPublished: false,
        publishedDate: '',
        downloadCount: '',
        pageCount: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.sharedInstructionForm.reset({
        buildManualId: sharedInstructionData.buildManualId,
        publishedMocId: sharedInstructionData.publishedMocId,
        name: sharedInstructionData.name ?? '',
        description: sharedInstructionData.description ?? '',
        formatType: sharedInstructionData.formatType ?? '',
        filePath: sharedInstructionData.filePath ?? '',
        isPublished: sharedInstructionData.isPublished ?? false,
        publishedDate: isoUtcStringToDateTimeLocal(sharedInstructionData.publishedDate) ?? '',
        downloadCount: sharedInstructionData.downloadCount?.toString() ?? '',
        pageCount: sharedInstructionData.pageCount?.toString() ?? '',
        versionNumber: sharedInstructionData.versionNumber?.toString() ?? '',
        active: sharedInstructionData.active ?? true,
        deleted: sharedInstructionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.sharedInstructionForm.markAsPristine();
    this.sharedInstructionForm.markAsUntouched();
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

    if (this.sharedInstructionService.userIsBMCSharedInstructionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Shared Instructions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.sharedInstructionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.sharedInstructionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.sharedInstructionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const sharedInstructionSubmitData: SharedInstructionSubmitData = {
        id: this.sharedInstructionData?.id || 0,
        buildManualId: formValue.buildManualId ? Number(formValue.buildManualId) : null,
        publishedMocId: formValue.publishedMocId ? Number(formValue.publishedMocId) : null,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        formatType: formValue.formatType!.trim(),
        filePath: formValue.filePath?.trim() || null,
        isPublished: !!formValue.isPublished,
        publishedDate: formValue.publishedDate ? dateTimeLocalToIsoUtc(formValue.publishedDate.trim()) : null,
        downloadCount: Number(formValue.downloadCount),
        pageCount: formValue.pageCount ? Number(formValue.pageCount) : null,
        versionNumber: this.sharedInstructionData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.sharedInstructionService.PutSharedInstruction(sharedInstructionSubmitData.id, sharedInstructionSubmitData)
      : this.sharedInstructionService.PostSharedInstruction(sharedInstructionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSharedInstructionData) => {

        this.sharedInstructionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Shared Instruction's detail page
          //
          this.sharedInstructionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.sharedInstructionForm.markAsUntouched();

          this.router.navigate(['/sharedinstructions', savedSharedInstructionData.id]);
          this.alertService.showMessage('Shared Instruction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.sharedInstructionData = savedSharedInstructionData;
          this.buildFormValues(this.sharedInstructionData);

          this.alertService.showMessage("Shared Instruction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Shared Instruction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Shared Instruction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Shared Instruction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCSharedInstructionReader(): boolean {
    return this.sharedInstructionService.userIsBMCSharedInstructionReader();
  }

  public userIsBMCSharedInstructionWriter(): boolean {
    return this.sharedInstructionService.userIsBMCSharedInstructionWriter();
  }
}

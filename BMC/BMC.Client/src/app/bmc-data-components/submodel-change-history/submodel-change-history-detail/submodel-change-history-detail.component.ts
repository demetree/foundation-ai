/*
   GENERATED FORM FOR THE SUBMODELCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SubmodelChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to submodel-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SubmodelChangeHistoryService, SubmodelChangeHistoryData, SubmodelChangeHistorySubmitData } from '../../../bmc-data-services/submodel-change-history.service';
import { SubmodelService } from '../../../bmc-data-services/submodel.service';
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
interface SubmodelChangeHistoryFormValues {
  submodelId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-submodel-change-history-detail',
  templateUrl: './submodel-change-history-detail.component.html',
  styleUrls: ['./submodel-change-history-detail.component.scss']
})

export class SubmodelChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SubmodelChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public submodelChangeHistoryForm: FormGroup = this.fb.group({
        submodelId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public submodelChangeHistoryId: string | null = null;
  public submodelChangeHistoryData: SubmodelChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  submodelChangeHistories$ = this.submodelChangeHistoryService.GetSubmodelChangeHistoryList();
  public submodels$ = this.submodelService.GetSubmodelList();

  private destroy$ = new Subject<void>();

  constructor(
    public submodelChangeHistoryService: SubmodelChangeHistoryService,
    public submodelService: SubmodelService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the submodelChangeHistoryId from the route parameters
    this.submodelChangeHistoryId = this.route.snapshot.paramMap.get('submodelChangeHistoryId');

    if (this.submodelChangeHistoryId === 'new' ||
        this.submodelChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.submodelChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.submodelChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.submodelChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Submodel Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Submodel Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.submodelChangeHistoryForm.dirty) {
      return confirm('You have unsaved Submodel Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.submodelChangeHistoryId != null && this.submodelChangeHistoryId !== 'new') {

      const id = parseInt(this.submodelChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { submodelChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the SubmodelChangeHistory data for the current submodelChangeHistoryId.
  *
  * Fully respects the SubmodelChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SubmodelChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate submodelChangeHistoryId
    //
    if (!this.submodelChangeHistoryId) {

      this.alertService.showMessage('No SubmodelChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const submodelChangeHistoryId = Number(this.submodelChangeHistoryId);

    if (isNaN(submodelChangeHistoryId) || submodelChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Submodel Change History ID: "${this.submodelChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this SubmodelChangeHistory + relations

      this.submodelChangeHistoryService.ClearRecordCache(submodelChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.submodelChangeHistoryService.GetSubmodelChangeHistory(submodelChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (submodelChangeHistoryData) => {

        //
        // Success path — submodelChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!submodelChangeHistoryData) {

          this.handleSubmodelChangeHistoryNotFound(submodelChangeHistoryId);

        } else {

          this.submodelChangeHistoryData = submodelChangeHistoryData;
          this.buildFormValues(this.submodelChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SubmodelChangeHistory loaded successfully',
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
        this.handleSubmodelChangeHistoryLoadError(error, submodelChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSubmodelChangeHistoryNotFound(submodelChangeHistoryId: number): void {

    this.submodelChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SubmodelChangeHistory #${submodelChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSubmodelChangeHistoryLoadError(error: any, submodelChangeHistoryId: number): void {

    let message = 'Failed to load Submodel Change History.';
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
          message = 'You do not have permission to view this Submodel Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Submodel Change History #${submodelChangeHistoryId} was not found.`;
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

    console.error(`Submodel Change History load failed (ID: ${submodelChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.submodelChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(submodelChangeHistoryData: SubmodelChangeHistoryData | null) {

    if (submodelChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.submodelChangeHistoryForm.reset({
        submodelId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.submodelChangeHistoryForm.reset({
        submodelId: submodelChangeHistoryData.submodelId,
        versionNumber: submodelChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(submodelChangeHistoryData.timeStamp) ?? '',
        userId: submodelChangeHistoryData.userId?.toString() ?? '',
        data: submodelChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.submodelChangeHistoryForm.markAsPristine();
    this.submodelChangeHistoryForm.markAsUntouched();
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

    if (this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Submodel Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.submodelChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.submodelChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.submodelChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const submodelChangeHistorySubmitData: SubmodelChangeHistorySubmitData = {
        id: this.submodelChangeHistoryData?.id || 0,
        submodelId: Number(formValue.submodelId),
        versionNumber: this.submodelChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.submodelChangeHistoryService.PutSubmodelChangeHistory(submodelChangeHistorySubmitData.id, submodelChangeHistorySubmitData)
      : this.submodelChangeHistoryService.PostSubmodelChangeHistory(submodelChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSubmodelChangeHistoryData) => {

        this.submodelChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Submodel Change History's detail page
          //
          this.submodelChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.submodelChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/submodelchangehistories', savedSubmodelChangeHistoryData.id]);
          this.alertService.showMessage('Submodel Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.submodelChangeHistoryData = savedSubmodelChangeHistoryData;
          this.buildFormValues(this.submodelChangeHistoryData);

          this.alertService.showMessage("Submodel Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Submodel Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Submodel Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Submodel Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCSubmodelChangeHistoryReader(): boolean {
    return this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryReader();
  }

  public userIsBMCSubmodelChangeHistoryWriter(): boolean {
    return this.submodelChangeHistoryService.userIsBMCSubmodelChangeHistoryWriter();
  }
}

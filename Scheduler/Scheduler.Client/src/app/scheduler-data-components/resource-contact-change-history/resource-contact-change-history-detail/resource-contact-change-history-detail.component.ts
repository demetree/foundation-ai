/*
   GENERATED FORM FOR THE RESOURCECONTACTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceContactChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-contact-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceContactChangeHistoryService, ResourceContactChangeHistoryData, ResourceContactChangeHistorySubmitData } from '../../../scheduler-data-services/resource-contact-change-history.service';
import { ResourceContactService } from '../../../scheduler-data-services/resource-contact.service';
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
interface ResourceContactChangeHistoryFormValues {
  resourceContactId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-resource-contact-change-history-detail',
  templateUrl: './resource-contact-change-history-detail.component.html',
  styleUrls: ['./resource-contact-change-history-detail.component.scss']
})

export class ResourceContactChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceContactChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceContactChangeHistoryForm: FormGroup = this.fb.group({
        resourceContactId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public resourceContactChangeHistoryId: string | null = null;
  public resourceContactChangeHistoryData: ResourceContactChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  resourceContactChangeHistories$ = this.resourceContactChangeHistoryService.GetResourceContactChangeHistoryList();
  public resourceContacts$ = this.resourceContactService.GetResourceContactList();

  private destroy$ = new Subject<void>();

  constructor(
    public resourceContactChangeHistoryService: ResourceContactChangeHistoryService,
    public resourceContactService: ResourceContactService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the resourceContactChangeHistoryId from the route parameters
    this.resourceContactChangeHistoryId = this.route.snapshot.paramMap.get('resourceContactChangeHistoryId');

    if (this.resourceContactChangeHistoryId === 'new' ||
        this.resourceContactChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.resourceContactChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.resourceContactChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceContactChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Resource Contact Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Resource Contact Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.resourceContactChangeHistoryForm.dirty) {
      return confirm('You have unsaved Resource Contact Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.resourceContactChangeHistoryId != null && this.resourceContactChangeHistoryId !== 'new') {

      const id = parseInt(this.resourceContactChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { resourceContactChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the ResourceContactChangeHistory data for the current resourceContactChangeHistoryId.
  *
  * Fully respects the ResourceContactChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.resourceContactChangeHistoryService.userIsSchedulerResourceContactChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ResourceContactChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate resourceContactChangeHistoryId
    //
    if (!this.resourceContactChangeHistoryId) {

      this.alertService.showMessage('No ResourceContactChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const resourceContactChangeHistoryId = Number(this.resourceContactChangeHistoryId);

    if (isNaN(resourceContactChangeHistoryId) || resourceContactChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Resource Contact Change History ID: "${this.resourceContactChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this ResourceContactChangeHistory + relations

      this.resourceContactChangeHistoryService.ClearRecordCache(resourceContactChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.resourceContactChangeHistoryService.GetResourceContactChangeHistory(resourceContactChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (resourceContactChangeHistoryData) => {

        //
        // Success path — resourceContactChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!resourceContactChangeHistoryData) {

          this.handleResourceContactChangeHistoryNotFound(resourceContactChangeHistoryId);

        } else {

          this.resourceContactChangeHistoryData = resourceContactChangeHistoryData;
          this.buildFormValues(this.resourceContactChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ResourceContactChangeHistory loaded successfully',
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
        this.handleResourceContactChangeHistoryLoadError(error, resourceContactChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleResourceContactChangeHistoryNotFound(resourceContactChangeHistoryId: number): void {

    this.resourceContactChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ResourceContactChangeHistory #${resourceContactChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleResourceContactChangeHistoryLoadError(error: any, resourceContactChangeHistoryId: number): void {

    let message = 'Failed to load Resource Contact Change History.';
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
          message = 'You do not have permission to view this Resource Contact Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Resource Contact Change History #${resourceContactChangeHistoryId} was not found.`;
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

    console.error(`Resource Contact Change History load failed (ID: ${resourceContactChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.resourceContactChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(resourceContactChangeHistoryData: ResourceContactChangeHistoryData | null) {

    if (resourceContactChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceContactChangeHistoryForm.reset({
        resourceContactId: null,
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
        this.resourceContactChangeHistoryForm.reset({
        resourceContactId: resourceContactChangeHistoryData.resourceContactId,
        versionNumber: resourceContactChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(resourceContactChangeHistoryData.timeStamp) ?? '',
        userId: resourceContactChangeHistoryData.userId?.toString() ?? '',
        data: resourceContactChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.resourceContactChangeHistoryForm.markAsPristine();
    this.resourceContactChangeHistoryForm.markAsUntouched();
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

    if (this.resourceContactChangeHistoryService.userIsSchedulerResourceContactChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Resource Contact Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.resourceContactChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceContactChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceContactChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceContactChangeHistorySubmitData: ResourceContactChangeHistorySubmitData = {
        id: this.resourceContactChangeHistoryData?.id || 0,
        resourceContactId: Number(formValue.resourceContactId),
        versionNumber: this.resourceContactChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.resourceContactChangeHistoryService.PutResourceContactChangeHistory(resourceContactChangeHistorySubmitData.id, resourceContactChangeHistorySubmitData)
      : this.resourceContactChangeHistoryService.PostResourceContactChangeHistory(resourceContactChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedResourceContactChangeHistoryData) => {

        this.resourceContactChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Resource Contact Change History's detail page
          //
          this.resourceContactChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.resourceContactChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/resourcecontactchangehistories', savedResourceContactChangeHistoryData.id]);
          this.alertService.showMessage('Resource Contact Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.resourceContactChangeHistoryData = savedResourceContactChangeHistoryData;
          this.buildFormValues(this.resourceContactChangeHistoryData);

          this.alertService.showMessage("Resource Contact Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Resource Contact Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Contact Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Contact Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerResourceContactChangeHistoryReader(): boolean {
    return this.resourceContactChangeHistoryService.userIsSchedulerResourceContactChangeHistoryReader();
  }

  public userIsSchedulerResourceContactChangeHistoryWriter(): boolean {
    return this.resourceContactChangeHistoryService.userIsSchedulerResourceContactChangeHistoryWriter();
  }
}

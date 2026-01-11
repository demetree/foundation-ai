/*
   GENERATED FORM FOR THE RESOURCEQUALIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceQualification table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-qualification-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceQualificationService, ResourceQualificationData, ResourceQualificationSubmitData } from '../../../scheduler-data-services/resource-qualification.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { ResourceQualificationChangeHistoryService } from '../../../scheduler-data-services/resource-qualification-change-history.service';
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
interface ResourceQualificationFormValues {
  resourceId: number | bigint,       // For FK link number
  qualificationId: number | bigint,       // For FK link number
  issueDate: string | null,
  expiryDate: string | null,
  issuer: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-resource-qualification-detail',
  templateUrl: './resource-qualification-detail.component.html',
  styleUrls: ['./resource-qualification-detail.component.scss']
})

export class ResourceQualificationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceQualificationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceQualificationForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        qualificationId: [null, Validators.required],
        issueDate: [''],
        expiryDate: [''],
        issuer: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public resourceQualificationId: string | null = null;
  public resourceQualificationData: ResourceQualificationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  resourceQualifications$ = this.resourceQualificationService.GetResourceQualificationList();
  public resources$ = this.resourceService.GetResourceList();
  public qualifications$ = this.qualificationService.GetQualificationList();
  public resourceQualificationChangeHistories$ = this.resourceQualificationChangeHistoryService.GetResourceQualificationChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public resourceQualificationService: ResourceQualificationService,
    public resourceService: ResourceService,
    public qualificationService: QualificationService,
    public resourceQualificationChangeHistoryService: ResourceQualificationChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the resourceQualificationId from the route parameters
    this.resourceQualificationId = this.route.snapshot.paramMap.get('resourceQualificationId');

    if (this.resourceQualificationId === 'new' ||
        this.resourceQualificationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.resourceQualificationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.resourceQualificationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceQualificationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Resource Qualification';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Resource Qualification';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.resourceQualificationForm.dirty) {
      return confirm('You have unsaved Resource Qualification changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.resourceQualificationId != null && this.resourceQualificationId !== 'new') {

      const id = parseInt(this.resourceQualificationId, 10);

      if (!isNaN(id)) {
        return { resourceQualificationId: id };
      }
    }

    return null;
  }


/*
  * Loads the ResourceQualification data for the current resourceQualificationId.
  *
  * Fully respects the ResourceQualificationService caching strategy and error handling strategy.
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
    if (!this.resourceQualificationService.userIsSchedulerResourceQualificationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ResourceQualifications.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate resourceQualificationId
    //
    if (!this.resourceQualificationId) {

      this.alertService.showMessage('No ResourceQualification ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const resourceQualificationId = Number(this.resourceQualificationId);

    if (isNaN(resourceQualificationId) || resourceQualificationId <= 0) {

      this.alertService.showMessage(`Invalid Resource Qualification ID: "${this.resourceQualificationId}"`,
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
      // This is the most targeted way: clear only this ResourceQualification + relations

      this.resourceQualificationService.ClearRecordCache(resourceQualificationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.resourceQualificationService.GetResourceQualification(resourceQualificationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (resourceQualificationData) => {

        //
        // Success path — resourceQualificationData can legitimately be null if 404'd but request succeeded
        //
        if (!resourceQualificationData) {

          this.handleResourceQualificationNotFound(resourceQualificationId);

        } else {

          this.resourceQualificationData = resourceQualificationData;
          this.buildFormValues(this.resourceQualificationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ResourceQualification loaded successfully',
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
        this.handleResourceQualificationLoadError(error, resourceQualificationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleResourceQualificationNotFound(resourceQualificationId: number): void {

    this.resourceQualificationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ResourceQualification #${resourceQualificationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleResourceQualificationLoadError(error: any, resourceQualificationId: number): void {

    let message = 'Failed to load Resource Qualification.';
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
          message = 'You do not have permission to view this Resource Qualification.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Resource Qualification #${resourceQualificationId} was not found.`;
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

    console.error(`Resource Qualification load failed (ID: ${resourceQualificationId})`, error);

    //
    // Reset UI to safe state
    //
    this.resourceQualificationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(resourceQualificationData: ResourceQualificationData | null) {

    if (resourceQualificationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceQualificationForm.reset({
        resourceId: null,
        qualificationId: null,
        issueDate: '',
        expiryDate: '',
        issuer: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceQualificationForm.reset({
        resourceId: resourceQualificationData.resourceId,
        qualificationId: resourceQualificationData.qualificationId,
        issueDate: isoUtcStringToDateTimeLocal(resourceQualificationData.issueDate) ?? '',
        expiryDate: isoUtcStringToDateTimeLocal(resourceQualificationData.expiryDate) ?? '',
        issuer: resourceQualificationData.issuer ?? '',
        notes: resourceQualificationData.notes ?? '',
        versionNumber: resourceQualificationData.versionNumber?.toString() ?? '',
        active: resourceQualificationData.active ?? true,
        deleted: resourceQualificationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceQualificationForm.markAsPristine();
    this.resourceQualificationForm.markAsUntouched();
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

    if (this.resourceQualificationService.userIsSchedulerResourceQualificationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Resource Qualifications", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.resourceQualificationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceQualificationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceQualificationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceQualificationSubmitData: ResourceQualificationSubmitData = {
        id: this.resourceQualificationData?.id || 0,
        resourceId: Number(formValue.resourceId),
        qualificationId: Number(formValue.qualificationId),
        issueDate: formValue.issueDate ? dateTimeLocalToIsoUtc(formValue.issueDate.trim()) : null,
        expiryDate: formValue.expiryDate ? dateTimeLocalToIsoUtc(formValue.expiryDate.trim()) : null,
        issuer: formValue.issuer?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.resourceQualificationData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.resourceQualificationService.PutResourceQualification(resourceQualificationSubmitData.id, resourceQualificationSubmitData)
      : this.resourceQualificationService.PostResourceQualification(resourceQualificationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedResourceQualificationData) => {

        this.resourceQualificationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Resource Qualification's detail page
          //
          this.resourceQualificationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.resourceQualificationForm.markAsUntouched();

          this.router.navigate(['/resourcequalifications', savedResourceQualificationData.id]);
          this.alertService.showMessage('Resource Qualification added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.resourceQualificationData = savedResourceQualificationData;
          this.buildFormValues(this.resourceQualificationData);

          this.alertService.showMessage("Resource Qualification saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Resource Qualification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Qualification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Qualification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerResourceQualificationReader(): boolean {
    return this.resourceQualificationService.userIsSchedulerResourceQualificationReader();
  }

  public userIsSchedulerResourceQualificationWriter(): boolean {
    return this.resourceQualificationService.userIsSchedulerResourceQualificationWriter();
  }
}

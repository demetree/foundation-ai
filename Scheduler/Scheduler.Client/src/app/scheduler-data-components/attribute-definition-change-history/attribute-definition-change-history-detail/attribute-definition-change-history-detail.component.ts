/*
   GENERATED FORM FOR THE ATTRIBUTEDEFINITIONCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AttributeDefinitionChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to attribute-definition-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AttributeDefinitionChangeHistoryService, AttributeDefinitionChangeHistoryData, AttributeDefinitionChangeHistorySubmitData } from '../../../scheduler-data-services/attribute-definition-change-history.service';
import { AttributeDefinitionService } from '../../../scheduler-data-services/attribute-definition.service';
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
interface AttributeDefinitionChangeHistoryFormValues {
  attributeDefinitionId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-attribute-definition-change-history-detail',
  templateUrl: './attribute-definition-change-history-detail.component.html',
  styleUrls: ['./attribute-definition-change-history-detail.component.scss']
})

export class AttributeDefinitionChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AttributeDefinitionChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public attributeDefinitionChangeHistoryForm: FormGroup = this.fb.group({
        attributeDefinitionId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public attributeDefinitionChangeHistoryId: string | null = null;
  public attributeDefinitionChangeHistoryData: AttributeDefinitionChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  attributeDefinitionChangeHistories$ = this.attributeDefinitionChangeHistoryService.GetAttributeDefinitionChangeHistoryList();
  public attributeDefinitions$ = this.attributeDefinitionService.GetAttributeDefinitionList();

  private destroy$ = new Subject<void>();

  constructor(
    public attributeDefinitionChangeHistoryService: AttributeDefinitionChangeHistoryService,
    public attributeDefinitionService: AttributeDefinitionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the attributeDefinitionChangeHistoryId from the route parameters
    this.attributeDefinitionChangeHistoryId = this.route.snapshot.paramMap.get('attributeDefinitionChangeHistoryId');

    if (this.attributeDefinitionChangeHistoryId === 'new' ||
        this.attributeDefinitionChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.attributeDefinitionChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.attributeDefinitionChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.attributeDefinitionChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Attribute Definition Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Attribute Definition Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.attributeDefinitionChangeHistoryForm.dirty) {
      return confirm('You have unsaved Attribute Definition Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.attributeDefinitionChangeHistoryId != null && this.attributeDefinitionChangeHistoryId !== 'new') {

      const id = parseInt(this.attributeDefinitionChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { attributeDefinitionChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the AttributeDefinitionChangeHistory data for the current attributeDefinitionChangeHistoryId.
  *
  * Fully respects the AttributeDefinitionChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.attributeDefinitionChangeHistoryService.userIsSchedulerAttributeDefinitionChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AttributeDefinitionChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate attributeDefinitionChangeHistoryId
    //
    if (!this.attributeDefinitionChangeHistoryId) {

      this.alertService.showMessage('No AttributeDefinitionChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const attributeDefinitionChangeHistoryId = Number(this.attributeDefinitionChangeHistoryId);

    if (isNaN(attributeDefinitionChangeHistoryId) || attributeDefinitionChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Attribute Definition Change History ID: "${this.attributeDefinitionChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this AttributeDefinitionChangeHistory + relations

      this.attributeDefinitionChangeHistoryService.ClearRecordCache(attributeDefinitionChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.attributeDefinitionChangeHistoryService.GetAttributeDefinitionChangeHistory(attributeDefinitionChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (attributeDefinitionChangeHistoryData) => {

        //
        // Success path — attributeDefinitionChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!attributeDefinitionChangeHistoryData) {

          this.handleAttributeDefinitionChangeHistoryNotFound(attributeDefinitionChangeHistoryId);

        } else {

          this.attributeDefinitionChangeHistoryData = attributeDefinitionChangeHistoryData;
          this.buildFormValues(this.attributeDefinitionChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AttributeDefinitionChangeHistory loaded successfully',
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
        this.handleAttributeDefinitionChangeHistoryLoadError(error, attributeDefinitionChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAttributeDefinitionChangeHistoryNotFound(attributeDefinitionChangeHistoryId: number): void {

    this.attributeDefinitionChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AttributeDefinitionChangeHistory #${attributeDefinitionChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAttributeDefinitionChangeHistoryLoadError(error: any, attributeDefinitionChangeHistoryId: number): void {

    let message = 'Failed to load Attribute Definition Change History.';
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
          message = 'You do not have permission to view this Attribute Definition Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Attribute Definition Change History #${attributeDefinitionChangeHistoryId} was not found.`;
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

    console.error(`Attribute Definition Change History load failed (ID: ${attributeDefinitionChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.attributeDefinitionChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(attributeDefinitionChangeHistoryData: AttributeDefinitionChangeHistoryData | null) {

    if (attributeDefinitionChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.attributeDefinitionChangeHistoryForm.reset({
        attributeDefinitionId: null,
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
        this.attributeDefinitionChangeHistoryForm.reset({
        attributeDefinitionId: attributeDefinitionChangeHistoryData.attributeDefinitionId,
        versionNumber: attributeDefinitionChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(attributeDefinitionChangeHistoryData.timeStamp) ?? '',
        userId: attributeDefinitionChangeHistoryData.userId?.toString() ?? '',
        data: attributeDefinitionChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.attributeDefinitionChangeHistoryForm.markAsPristine();
    this.attributeDefinitionChangeHistoryForm.markAsUntouched();
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

    if (this.attributeDefinitionChangeHistoryService.userIsSchedulerAttributeDefinitionChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Attribute Definition Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.attributeDefinitionChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.attributeDefinitionChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.attributeDefinitionChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const attributeDefinitionChangeHistorySubmitData: AttributeDefinitionChangeHistorySubmitData = {
        id: this.attributeDefinitionChangeHistoryData?.id || 0,
        attributeDefinitionId: Number(formValue.attributeDefinitionId),
        versionNumber: this.attributeDefinitionChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.attributeDefinitionChangeHistoryService.PutAttributeDefinitionChangeHistory(attributeDefinitionChangeHistorySubmitData.id, attributeDefinitionChangeHistorySubmitData)
      : this.attributeDefinitionChangeHistoryService.PostAttributeDefinitionChangeHistory(attributeDefinitionChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAttributeDefinitionChangeHistoryData) => {

        this.attributeDefinitionChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Attribute Definition Change History's detail page
          //
          this.attributeDefinitionChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.attributeDefinitionChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/attributedefinitionchangehistories', savedAttributeDefinitionChangeHistoryData.id]);
          this.alertService.showMessage('Attribute Definition Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.attributeDefinitionChangeHistoryData = savedAttributeDefinitionChangeHistoryData;
          this.buildFormValues(this.attributeDefinitionChangeHistoryData);

          this.alertService.showMessage("Attribute Definition Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Attribute Definition Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Attribute Definition Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Attribute Definition Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerAttributeDefinitionChangeHistoryReader(): boolean {
    return this.attributeDefinitionChangeHistoryService.userIsSchedulerAttributeDefinitionChangeHistoryReader();
  }

  public userIsSchedulerAttributeDefinitionChangeHistoryWriter(): boolean {
    return this.attributeDefinitionChangeHistoryService.userIsSchedulerAttributeDefinitionChangeHistoryWriter();
  }
}

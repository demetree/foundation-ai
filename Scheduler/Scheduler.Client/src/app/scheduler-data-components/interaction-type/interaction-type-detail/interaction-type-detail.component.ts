/*
   GENERATED FORM FOR THE INTERACTIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from InteractionType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to interaction-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { InteractionTypeService, InteractionTypeData, InteractionTypeSubmitData } from '../../../scheduler-data-services/interaction-type.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ContactInteractionService } from '../../../scheduler-data-services/contact-interaction.service';
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
interface InteractionTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-interaction-type-detail',
  templateUrl: './interaction-type-detail.component.html',
  styleUrls: ['./interaction-type-detail.component.scss']
})

export class InteractionTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<InteractionTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public interactionTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public interactionTypeId: string | null = null;
  public interactionTypeData: InteractionTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  interactionTypes$ = this.interactionTypeService.GetInteractionTypeList();
  public icons$ = this.iconService.GetIconList();
  public contactInteractions$ = this.contactInteractionService.GetContactInteractionList();

  private destroy$ = new Subject<void>();

  constructor(
    public interactionTypeService: InteractionTypeService,
    public iconService: IconService,
    public contactInteractionService: ContactInteractionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the interactionTypeId from the route parameters
    this.interactionTypeId = this.route.snapshot.paramMap.get('interactionTypeId');

    if (this.interactionTypeId === 'new' ||
        this.interactionTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.interactionTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.interactionTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.interactionTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Interaction Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Interaction Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.interactionTypeForm.dirty) {
      return confirm('You have unsaved Interaction Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.interactionTypeId != null && this.interactionTypeId !== 'new') {

      const id = parseInt(this.interactionTypeId, 10);

      if (!isNaN(id)) {
        return { interactionTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the InteractionType data for the current interactionTypeId.
  *
  * Fully respects the InteractionTypeService caching strategy and error handling strategy.
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
    if (!this.interactionTypeService.userIsSchedulerInteractionTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read InteractionTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate interactionTypeId
    //
    if (!this.interactionTypeId) {

      this.alertService.showMessage('No InteractionType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const interactionTypeId = Number(this.interactionTypeId);

    if (isNaN(interactionTypeId) || interactionTypeId <= 0) {

      this.alertService.showMessage(`Invalid Interaction Type ID: "${this.interactionTypeId}"`,
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
      // This is the most targeted way: clear only this InteractionType + relations

      this.interactionTypeService.ClearRecordCache(interactionTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.interactionTypeService.GetInteractionType(interactionTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (interactionTypeData) => {

        //
        // Success path — interactionTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!interactionTypeData) {

          this.handleInteractionTypeNotFound(interactionTypeId);

        } else {

          this.interactionTypeData = interactionTypeData;
          this.buildFormValues(this.interactionTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'InteractionType loaded successfully',
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
        this.handleInteractionTypeLoadError(error, interactionTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleInteractionTypeNotFound(interactionTypeId: number): void {

    this.interactionTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `InteractionType #${interactionTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleInteractionTypeLoadError(error: any, interactionTypeId: number): void {

    let message = 'Failed to load Interaction Type.';
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
          message = 'You do not have permission to view this Interaction Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Interaction Type #${interactionTypeId} was not found.`;
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

    console.error(`Interaction Type load failed (ID: ${interactionTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.interactionTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(interactionTypeData: InteractionTypeData | null) {

    if (interactionTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.interactionTypeForm.reset({
        name: '',
        description: '',
        sequence: '',
        iconId: null,
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.interactionTypeForm.reset({
        name: interactionTypeData.name ?? '',
        description: interactionTypeData.description ?? '',
        sequence: interactionTypeData.sequence?.toString() ?? '',
        iconId: interactionTypeData.iconId,
        color: interactionTypeData.color ?? '',
        active: interactionTypeData.active ?? true,
        deleted: interactionTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.interactionTypeForm.markAsPristine();
    this.interactionTypeForm.markAsUntouched();
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

    if (this.interactionTypeService.userIsSchedulerInteractionTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Interaction Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.interactionTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.interactionTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.interactionTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const interactionTypeSubmitData: InteractionTypeSubmitData = {
        id: this.interactionTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.interactionTypeService.PutInteractionType(interactionTypeSubmitData.id, interactionTypeSubmitData)
      : this.interactionTypeService.PostInteractionType(interactionTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedInteractionTypeData) => {

        this.interactionTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Interaction Type's detail page
          //
          this.interactionTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.interactionTypeForm.markAsUntouched();

          this.router.navigate(['/interactiontypes', savedInteractionTypeData.id]);
          this.alertService.showMessage('Interaction Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.interactionTypeData = savedInteractionTypeData;
          this.buildFormValues(this.interactionTypeData);

          this.alertService.showMessage("Interaction Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Interaction Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Interaction Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Interaction Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerInteractionTypeReader(): boolean {
    return this.interactionTypeService.userIsSchedulerInteractionTypeReader();
  }

  public userIsSchedulerInteractionTypeWriter(): boolean {
    return this.interactionTypeService.userIsSchedulerInteractionTypeWriter();
  }
}

/*
   GENERATED FORM FOR THE PRIORITY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Priority table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to priority-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PriorityService, PriorityData, PrioritySubmitData } from '../../../scheduler-data-services/priority.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { TagService } from '../../../scheduler-data-services/tag.service';
import { ScheduledEventTemplateService } from '../../../scheduler-data-services/scheduled-event-template.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
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
interface PriorityFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-priority-detail',
  templateUrl: './priority-detail.component.html',
  styleUrls: ['./priority-detail.component.scss']
})

export class PriorityDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PriorityFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public priorityForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public priorityId: string | null = null;
  public priorityData: PriorityData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  priorities$ = this.priorityService.GetPriorityList();
  public icons$ = this.iconService.GetIconList();
  public tags$ = this.tagService.GetTagList();
  public scheduledEventTemplates$ = this.scheduledEventTemplateService.GetScheduledEventTemplateList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public contactInteractions$ = this.contactInteractionService.GetContactInteractionList();

  private destroy$ = new Subject<void>();

  constructor(
    public priorityService: PriorityService,
    public iconService: IconService,
    public tagService: TagService,
    public scheduledEventTemplateService: ScheduledEventTemplateService,
    public scheduledEventService: ScheduledEventService,
    public contactInteractionService: ContactInteractionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the priorityId from the route parameters
    this.priorityId = this.route.snapshot.paramMap.get('priorityId');

    if (this.priorityId === 'new' ||
        this.priorityId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.priorityData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.priorityForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.priorityForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Priority';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Priority';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.priorityForm.dirty) {
      return confirm('You have unsaved Priority changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.priorityId != null && this.priorityId !== 'new') {

      const id = parseInt(this.priorityId, 10);

      if (!isNaN(id)) {
        return { priorityId: id };
      }
    }

    return null;
  }


/*
  * Loads the Priority data for the current priorityId.
  *
  * Fully respects the PriorityService caching strategy and error handling strategy.
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
    if (!this.priorityService.userIsSchedulerPriorityReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Priorities.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate priorityId
    //
    if (!this.priorityId) {

      this.alertService.showMessage('No Priority ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const priorityId = Number(this.priorityId);

    if (isNaN(priorityId) || priorityId <= 0) {

      this.alertService.showMessage(`Invalid Priority ID: "${this.priorityId}"`,
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
      // This is the most targeted way: clear only this Priority + relations

      this.priorityService.ClearRecordCache(priorityId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.priorityService.GetPriority(priorityId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (priorityData) => {

        //
        // Success path — priorityData can legitimately be null if 404'd but request succeeded
        //
        if (!priorityData) {

          this.handlePriorityNotFound(priorityId);

        } else {

          this.priorityData = priorityData;
          this.buildFormValues(this.priorityData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Priority loaded successfully',
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
        this.handlePriorityLoadError(error, priorityId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePriorityNotFound(priorityId: number): void {

    this.priorityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Priority #${priorityId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePriorityLoadError(error: any, priorityId: number): void {

    let message = 'Failed to load Priority.';
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
          message = 'You do not have permission to view this Priority.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Priority #${priorityId} was not found.`;
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

    console.error(`Priority load failed (ID: ${priorityId})`, error);

    //
    // Reset UI to safe state
    //
    this.priorityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(priorityData: PriorityData | null) {

    if (priorityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.priorityForm.reset({
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
        this.priorityForm.reset({
        name: priorityData.name ?? '',
        description: priorityData.description ?? '',
        sequence: priorityData.sequence?.toString() ?? '',
        iconId: priorityData.iconId,
        color: priorityData.color ?? '',
        active: priorityData.active ?? true,
        deleted: priorityData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.priorityForm.markAsPristine();
    this.priorityForm.markAsUntouched();
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

    if (this.priorityService.userIsSchedulerPriorityWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Priorities", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.priorityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.priorityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.priorityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const prioritySubmitData: PrioritySubmitData = {
        id: this.priorityData?.id || 0,
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
      ? this.priorityService.PutPriority(prioritySubmitData.id, prioritySubmitData)
      : this.priorityService.PostPriority(prioritySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPriorityData) => {

        this.priorityService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Priority's detail page
          //
          this.priorityForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.priorityForm.markAsUntouched();

          this.router.navigate(['/priorities', savedPriorityData.id]);
          this.alertService.showMessage('Priority added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.priorityData = savedPriorityData;
          this.buildFormValues(this.priorityData);

          this.alertService.showMessage("Priority saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Priority.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Priority.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Priority could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerPriorityReader(): boolean {
    return this.priorityService.userIsSchedulerPriorityReader();
  }

  public userIsSchedulerPriorityWriter(): boolean {
    return this.priorityService.userIsSchedulerPriorityWriter();
  }
}

/*
   GENERATED FORM FOR THE CONTACTINTERACTION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactInteraction table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-interaction-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactInteractionService, ContactInteractionData, ContactInteractionSubmitData } from '../../../scheduler-data-services/contact-interaction.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { InteractionTypeService } from '../../../scheduler-data-services/interaction-type.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { PriorityService } from '../../../scheduler-data-services/priority.service';
import { ContactInteractionChangeHistoryService } from '../../../scheduler-data-services/contact-interaction-change-history.service';
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
interface ContactInteractionFormValues {
  contactId: number | bigint,       // For FK link number
  initiatingContactId: number | bigint | null,       // For FK link number
  interactionTypeId: number | bigint,       // For FK link number
  scheduledEventId: number | bigint | null,       // For FK link number
  startTime: string,
  endTime: string | null,
  notes: string | null,
  location: string | null,
  priorityId: number | bigint | null,       // For FK link number
  externalId: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-contact-interaction-detail',
  templateUrl: './contact-interaction-detail.component.html',
  styleUrls: ['./contact-interaction-detail.component.scss']
})

export class ContactInteractionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactInteractionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactInteractionForm: FormGroup = this.fb.group({
        contactId: [null, Validators.required],
        initiatingContactId: [null],
        interactionTypeId: [null, Validators.required],
        scheduledEventId: [null],
        startTime: ['', Validators.required],
        endTime: [''],
        notes: [''],
        location: [''],
        priorityId: [null],
        externalId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public contactInteractionId: string | null = null;
  public contactInteractionData: ContactInteractionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  contactInteractions$ = this.contactInteractionService.GetContactInteractionList();
  public contacts$ = this.contactService.GetContactList();
  public interactionTypes$ = this.interactionTypeService.GetInteractionTypeList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public priorities$ = this.priorityService.GetPriorityList();
  public contactInteractionChangeHistories$ = this.contactInteractionChangeHistoryService.GetContactInteractionChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public contactInteractionService: ContactInteractionService,
    public contactService: ContactService,
    public interactionTypeService: InteractionTypeService,
    public scheduledEventService: ScheduledEventService,
    public priorityService: PriorityService,
    public contactInteractionChangeHistoryService: ContactInteractionChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the contactInteractionId from the route parameters
    this.contactInteractionId = this.route.snapshot.paramMap.get('contactInteractionId');

    if (this.contactInteractionId === 'new' ||
        this.contactInteractionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contactInteractionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.contactInteractionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactInteractionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Contact Interaction';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Contact Interaction';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.contactInteractionForm.dirty) {
      return confirm('You have unsaved Contact Interaction changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.contactInteractionId != null && this.contactInteractionId !== 'new') {

      const id = parseInt(this.contactInteractionId, 10);

      if (!isNaN(id)) {
        return { contactInteractionId: id };
      }
    }

    return null;
  }


/*
  * Loads the ContactInteraction data for the current contactInteractionId.
  *
  * Fully respects the ContactInteractionService caching strategy and error handling strategy.
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
    if (!this.contactInteractionService.userIsSchedulerContactInteractionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ContactInteractions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contactInteractionId
    //
    if (!this.contactInteractionId) {

      this.alertService.showMessage('No ContactInteraction ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contactInteractionId = Number(this.contactInteractionId);

    if (isNaN(contactInteractionId) || contactInteractionId <= 0) {

      this.alertService.showMessage(`Invalid Contact Interaction ID: "${this.contactInteractionId}"`,
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
      // This is the most targeted way: clear only this ContactInteraction + relations

      this.contactInteractionService.ClearRecordCache(contactInteractionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contactInteractionService.GetContactInteraction(contactInteractionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contactInteractionData) => {

        //
        // Success path — contactInteractionData can legitimately be null if 404'd but request succeeded
        //
        if (!contactInteractionData) {

          this.handleContactInteractionNotFound(contactInteractionId);

        } else {

          this.contactInteractionData = contactInteractionData;
          this.buildFormValues(this.contactInteractionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ContactInteraction loaded successfully',
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
        this.handleContactInteractionLoadError(error, contactInteractionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleContactInteractionNotFound(contactInteractionId: number): void {

    this.contactInteractionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ContactInteraction #${contactInteractionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContactInteractionLoadError(error: any, contactInteractionId: number): void {

    let message = 'Failed to load Contact Interaction.';
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
          message = 'You do not have permission to view this Contact Interaction.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Contact Interaction #${contactInteractionId} was not found.`;
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

    console.error(`Contact Interaction load failed (ID: ${contactInteractionId})`, error);

    //
    // Reset UI to safe state
    //
    this.contactInteractionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(contactInteractionData: ContactInteractionData | null) {

    if (contactInteractionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactInteractionForm.reset({
        contactId: null,
        initiatingContactId: null,
        interactionTypeId: null,
        scheduledEventId: null,
        startTime: '',
        endTime: '',
        notes: '',
        location: '',
        priorityId: null,
        externalId: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactInteractionForm.reset({
        contactId: contactInteractionData.contactId,
        initiatingContactId: contactInteractionData.initiatingContactId,
        interactionTypeId: contactInteractionData.interactionTypeId,
        scheduledEventId: contactInteractionData.scheduledEventId,
        startTime: isoUtcStringToDateTimeLocal(contactInteractionData.startTime) ?? '',
        endTime: isoUtcStringToDateTimeLocal(contactInteractionData.endTime) ?? '',
        notes: contactInteractionData.notes ?? '',
        location: contactInteractionData.location ?? '',
        priorityId: contactInteractionData.priorityId,
        externalId: contactInteractionData.externalId ?? '',
        versionNumber: contactInteractionData.versionNumber?.toString() ?? '',
        active: contactInteractionData.active ?? true,
        deleted: contactInteractionData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactInteractionForm.markAsPristine();
    this.contactInteractionForm.markAsUntouched();
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

    if (this.contactInteractionService.userIsSchedulerContactInteractionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Contact Interactions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.contactInteractionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactInteractionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactInteractionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactInteractionSubmitData: ContactInteractionSubmitData = {
        id: this.contactInteractionData?.id || 0,
        contactId: Number(formValue.contactId),
        initiatingContactId: formValue.initiatingContactId ? Number(formValue.initiatingContactId) : null,
        interactionTypeId: Number(formValue.interactionTypeId),
        scheduledEventId: formValue.scheduledEventId ? Number(formValue.scheduledEventId) : null,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        endTime: formValue.endTime ? dateTimeLocalToIsoUtc(formValue.endTime.trim()) : null,
        notes: formValue.notes?.trim() || null,
        location: formValue.location?.trim() || null,
        priorityId: formValue.priorityId ? Number(formValue.priorityId) : null,
        externalId: formValue.externalId?.trim() || null,
        versionNumber: this.contactInteractionData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.contactInteractionService.PutContactInteraction(contactInteractionSubmitData.id, contactInteractionSubmitData)
      : this.contactInteractionService.PostContactInteraction(contactInteractionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedContactInteractionData) => {

        this.contactInteractionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Contact Interaction's detail page
          //
          this.contactInteractionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.contactInteractionForm.markAsUntouched();

          this.router.navigate(['/contactinteractions', savedContactInteractionData.id]);
          this.alertService.showMessage('Contact Interaction added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.contactInteractionData = savedContactInteractionData;
          this.buildFormValues(this.contactInteractionData);

          this.alertService.showMessage("Contact Interaction saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Contact Interaction.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Interaction.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Interaction could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerContactInteractionReader(): boolean {
    return this.contactInteractionService.userIsSchedulerContactInteractionReader();
  }

  public userIsSchedulerContactInteractionWriter(): boolean {
    return this.contactInteractionService.userIsSchedulerContactInteractionWriter();
  }
}

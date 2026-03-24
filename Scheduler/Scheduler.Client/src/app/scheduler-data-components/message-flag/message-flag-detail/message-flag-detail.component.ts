/*
   GENERATED FORM FOR THE MESSAGEFLAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MessageFlag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to message-flag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MessageFlagService, MessageFlagData, MessageFlagSubmitData } from '../../../scheduler-data-services/message-flag.service';
import { ConversationMessageService } from '../../../scheduler-data-services/conversation-message.service';
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
interface MessageFlagFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  flaggedByUserId: string,     // Stored as string for form input, converted to number on submit.
  reason: string,
  details: string | null,
  status: string,
  reviewedByUserId: string | null,     // Stored as string for form input, converted to number on submit.
  dateTimeReviewed: string | null,
  resolutionNotes: string | null,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-message-flag-detail',
  templateUrl: './message-flag-detail.component.html',
  styleUrls: ['./message-flag-detail.component.scss']
})

export class MessageFlagDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MessageFlagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public messageFlagForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        flaggedByUserId: ['', Validators.required],
        reason: ['', Validators.required],
        details: [''],
        status: ['', Validators.required],
        reviewedByUserId: [''],
        dateTimeReviewed: [''],
        resolutionNotes: [''],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public messageFlagId: string | null = null;
  public messageFlagData: MessageFlagData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  messageFlags$ = this.messageFlagService.GetMessageFlagList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  private destroy$ = new Subject<void>();

  constructor(
    public messageFlagService: MessageFlagService,
    public conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the messageFlagId from the route parameters
    this.messageFlagId = this.route.snapshot.paramMap.get('messageFlagId');

    if (this.messageFlagId === 'new' ||
        this.messageFlagId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.messageFlagData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.messageFlagForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.messageFlagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Message Flag';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Message Flag';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.messageFlagForm.dirty) {
      return confirm('You have unsaved Message Flag changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.messageFlagId != null && this.messageFlagId !== 'new') {

      const id = parseInt(this.messageFlagId, 10);

      if (!isNaN(id)) {
        return { messageFlagId: id };
      }
    }

    return null;
  }


/*
  * Loads the MessageFlag data for the current messageFlagId.
  *
  * Fully respects the MessageFlagService caching strategy and error handling strategy.
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
    if (!this.messageFlagService.userIsSchedulerMessageFlagReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MessageFlags.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate messageFlagId
    //
    if (!this.messageFlagId) {

      this.alertService.showMessage('No MessageFlag ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const messageFlagId = Number(this.messageFlagId);

    if (isNaN(messageFlagId) || messageFlagId <= 0) {

      this.alertService.showMessage(`Invalid Message Flag ID: "${this.messageFlagId}"`,
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
      // This is the most targeted way: clear only this MessageFlag + relations

      this.messageFlagService.ClearRecordCache(messageFlagId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.messageFlagService.GetMessageFlag(messageFlagId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (messageFlagData) => {

        //
        // Success path — messageFlagData can legitimately be null if 404'd but request succeeded
        //
        if (!messageFlagData) {

          this.handleMessageFlagNotFound(messageFlagId);

        } else {

          this.messageFlagData = messageFlagData;
          this.buildFormValues(this.messageFlagData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MessageFlag loaded successfully',
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
        this.handleMessageFlagLoadError(error, messageFlagId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMessageFlagNotFound(messageFlagId: number): void {

    this.messageFlagData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MessageFlag #${messageFlagId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMessageFlagLoadError(error: any, messageFlagId: number): void {

    let message = 'Failed to load Message Flag.';
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
          message = 'You do not have permission to view this Message Flag.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Message Flag #${messageFlagId} was not found.`;
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

    console.error(`Message Flag load failed (ID: ${messageFlagId})`, error);

    //
    // Reset UI to safe state
    //
    this.messageFlagData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(messageFlagData: MessageFlagData | null) {

    if (messageFlagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.messageFlagForm.reset({
        conversationMessageId: null,
        flaggedByUserId: '',
        reason: '',
        details: '',
        status: '',
        reviewedByUserId: '',
        dateTimeReviewed: '',
        resolutionNotes: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.messageFlagForm.reset({
        conversationMessageId: messageFlagData.conversationMessageId,
        flaggedByUserId: messageFlagData.flaggedByUserId?.toString() ?? '',
        reason: messageFlagData.reason ?? '',
        details: messageFlagData.details ?? '',
        status: messageFlagData.status ?? '',
        reviewedByUserId: messageFlagData.reviewedByUserId?.toString() ?? '',
        dateTimeReviewed: isoUtcStringToDateTimeLocal(messageFlagData.dateTimeReviewed) ?? '',
        resolutionNotes: messageFlagData.resolutionNotes ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(messageFlagData.dateTimeCreated) ?? '',
        active: messageFlagData.active ?? true,
        deleted: messageFlagData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.messageFlagForm.markAsPristine();
    this.messageFlagForm.markAsUntouched();
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

    if (this.messageFlagService.userIsSchedulerMessageFlagWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Message Flags", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.messageFlagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.messageFlagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.messageFlagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const messageFlagSubmitData: MessageFlagSubmitData = {
        id: this.messageFlagData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        flaggedByUserId: Number(formValue.flaggedByUserId),
        reason: formValue.reason!.trim(),
        details: formValue.details?.trim() || null,
        status: formValue.status!.trim(),
        reviewedByUserId: formValue.reviewedByUserId ? Number(formValue.reviewedByUserId) : null,
        dateTimeReviewed: formValue.dateTimeReviewed ? dateTimeLocalToIsoUtc(formValue.dateTimeReviewed.trim()) : null,
        resolutionNotes: formValue.resolutionNotes?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.messageFlagService.PutMessageFlag(messageFlagSubmitData.id, messageFlagSubmitData)
      : this.messageFlagService.PostMessageFlag(messageFlagSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMessageFlagData) => {

        this.messageFlagService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Message Flag's detail page
          //
          this.messageFlagForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.messageFlagForm.markAsUntouched();

          this.router.navigate(['/messageflags', savedMessageFlagData.id]);
          this.alertService.showMessage('Message Flag added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.messageFlagData = savedMessageFlagData;
          this.buildFormValues(this.messageFlagData);

          this.alertService.showMessage("Message Flag saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Message Flag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Message Flag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Message Flag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerMessageFlagReader(): boolean {
    return this.messageFlagService.userIsSchedulerMessageFlagReader();
  }

  public userIsSchedulerMessageFlagWriter(): boolean {
    return this.messageFlagService.userIsSchedulerMessageFlagWriter();
  }
}

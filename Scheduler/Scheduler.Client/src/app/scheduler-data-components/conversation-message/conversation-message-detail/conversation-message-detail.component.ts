/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageService, ConversationMessageData, ConversationMessageSubmitData } from '../../../scheduler-data-services/conversation-message.service';
import { ConversationService } from '../../../scheduler-data-services/conversation.service';
import { ConversationChannelService } from '../../../scheduler-data-services/conversation-channel.service';
import { ConversationMessageChangeHistoryService } from '../../../scheduler-data-services/conversation-message-change-history.service';
import { ConversationMessageAttachmentService } from '../../../scheduler-data-services/conversation-message-attachment.service';
import { ConversationMessageUserService } from '../../../scheduler-data-services/conversation-message-user.service';
import { ConversationMessageReactionService } from '../../../scheduler-data-services/conversation-message-reaction.service';
import { ConversationPinService } from '../../../scheduler-data-services/conversation-pin.service';
import { ConversationMessageLinkPreviewService } from '../../../scheduler-data-services/conversation-message-link-preview.service';
import { ConversationThreadUserService } from '../../../scheduler-data-services/conversation-thread-user.service';
import { MessageBookmarkService } from '../../../scheduler-data-services/message-bookmark.service';
import { MessageFlagService } from '../../../scheduler-data-services/message-flag.service';
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
interface ConversationMessageFormValues {
  conversationId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  parentConversationMessageId: number | bigint | null,       // For FK link number
  conversationChannelId: number | bigint | null,       // For FK link number
  dateTimeCreated: string,
  message: string,
  messageType: string | null,
  entity: string | null,
  entityId: string | null,     // Stored as string for form input, converted to number on submit.
  externalURL: string | null,
  forwardedFromMessageId: string | null,     // Stored as string for form input, converted to number on submit.
  forwardedFromUserId: string | null,     // Stored as string for form input, converted to number on submit.
  isScheduled: boolean,
  scheduledDateTime: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-message-detail',
  templateUrl: './conversation-message-detail.component.html',
  styleUrls: ['./conversation-message-detail.component.scss']
})

export class ConversationMessageDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        userId: ['', Validators.required],
        parentConversationMessageId: [null],
        conversationChannelId: [null],
        dateTimeCreated: ['', Validators.required],
        message: ['', Validators.required],
        messageType: [''],
        entity: [''],
        entityId: [''],
        externalURL: [''],
        forwardedFromMessageId: [''],
        forwardedFromUserId: [''],
        isScheduled: [false],
        scheduledDateTime: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public conversationMessageId: string | null = null;
  public conversationMessageData: ConversationMessageData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationMessages$ = this.conversationMessageService.GetConversationMessageList();
  public conversations$ = this.conversationService.GetConversationList();
  public conversationChannels$ = this.conversationChannelService.GetConversationChannelList();
  public conversationMessageChangeHistories$ = this.conversationMessageChangeHistoryService.GetConversationMessageChangeHistoryList();
  public conversationMessageAttachments$ = this.conversationMessageAttachmentService.GetConversationMessageAttachmentList();
  public conversationMessageUsers$ = this.conversationMessageUserService.GetConversationMessageUserList();
  public conversationMessageReactions$ = this.conversationMessageReactionService.GetConversationMessageReactionList();
  public conversationPins$ = this.conversationPinService.GetConversationPinList();
  public conversationMessageLinkPreviews$ = this.conversationMessageLinkPreviewService.GetConversationMessageLinkPreviewList();
  public conversationThreadUsers$ = this.conversationThreadUserService.GetConversationThreadUserList();
  public messageBookmarks$ = this.messageBookmarkService.GetMessageBookmarkList();
  public messageFlags$ = this.messageFlagService.GetMessageFlagList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationMessageService: ConversationMessageService,
    public conversationService: ConversationService,
    public conversationChannelService: ConversationChannelService,
    public conversationMessageChangeHistoryService: ConversationMessageChangeHistoryService,
    public conversationMessageAttachmentService: ConversationMessageAttachmentService,
    public conversationMessageUserService: ConversationMessageUserService,
    public conversationMessageReactionService: ConversationMessageReactionService,
    public conversationPinService: ConversationPinService,
    public conversationMessageLinkPreviewService: ConversationMessageLinkPreviewService,
    public conversationThreadUserService: ConversationThreadUserService,
    public messageBookmarkService: MessageBookmarkService,
    public messageFlagService: MessageFlagService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationMessageId from the route parameters
    this.conversationMessageId = this.route.snapshot.paramMap.get('conversationMessageId');

    if (this.conversationMessageId === 'new' ||
        this.conversationMessageId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationMessageData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationMessageForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation Message';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation Message';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationMessageForm.dirty) {
      return confirm('You have unsaved Conversation Message changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationMessageId != null && this.conversationMessageId !== 'new') {

      const id = parseInt(this.conversationMessageId, 10);

      if (!isNaN(id)) {
        return { conversationMessageId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationMessage data for the current conversationMessageId.
  *
  * Fully respects the ConversationMessageService caching strategy and error handling strategy.
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
    if (!this.conversationMessageService.userIsSchedulerConversationMessageReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationMessages.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationMessageId
    //
    if (!this.conversationMessageId) {

      this.alertService.showMessage('No ConversationMessage ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationMessageId = Number(this.conversationMessageId);

    if (isNaN(conversationMessageId) || conversationMessageId <= 0) {

      this.alertService.showMessage(`Invalid Conversation Message ID: "${this.conversationMessageId}"`,
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
      // This is the most targeted way: clear only this ConversationMessage + relations

      this.conversationMessageService.ClearRecordCache(conversationMessageId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationMessageService.GetConversationMessage(conversationMessageId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationMessageData) => {

        //
        // Success path — conversationMessageData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationMessageData) {

          this.handleConversationMessageNotFound(conversationMessageId);

        } else {

          this.conversationMessageData = conversationMessageData;
          this.buildFormValues(this.conversationMessageData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationMessage loaded successfully',
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
        this.handleConversationMessageLoadError(error, conversationMessageId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationMessageNotFound(conversationMessageId: number): void {

    this.conversationMessageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationMessage #${conversationMessageId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationMessageLoadError(error: any, conversationMessageId: number): void {

    let message = 'Failed to load Conversation Message.';
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
          message = 'You do not have permission to view this Conversation Message.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation Message #${conversationMessageId} was not found.`;
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

    console.error(`Conversation Message load failed (ID: ${conversationMessageId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationMessageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationMessageData: ConversationMessageData | null) {

    if (conversationMessageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageForm.reset({
        conversationId: null,
        userId: '',
        parentConversationMessageId: null,
        conversationChannelId: null,
        dateTimeCreated: '',
        message: '',
        messageType: '',
        entity: '',
        entityId: '',
        externalURL: '',
        forwardedFromMessageId: '',
        forwardedFromUserId: '',
        isScheduled: false,
        scheduledDateTime: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageForm.reset({
        conversationId: conversationMessageData.conversationId,
        userId: conversationMessageData.userId?.toString() ?? '',
        parentConversationMessageId: conversationMessageData.parentConversationMessageId,
        conversationChannelId: conversationMessageData.conversationChannelId,
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationMessageData.dateTimeCreated) ?? '',
        message: conversationMessageData.message ?? '',
        messageType: conversationMessageData.messageType ?? '',
        entity: conversationMessageData.entity ?? '',
        entityId: conversationMessageData.entityId?.toString() ?? '',
        externalURL: conversationMessageData.externalURL ?? '',
        forwardedFromMessageId: conversationMessageData.forwardedFromMessageId?.toString() ?? '',
        forwardedFromUserId: conversationMessageData.forwardedFromUserId?.toString() ?? '',
        isScheduled: conversationMessageData.isScheduled ?? false,
        scheduledDateTime: isoUtcStringToDateTimeLocal(conversationMessageData.scheduledDateTime) ?? '',
        versionNumber: conversationMessageData.versionNumber?.toString() ?? '',
        active: conversationMessageData.active ?? true,
        deleted: conversationMessageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageForm.markAsPristine();
    this.conversationMessageForm.markAsUntouched();
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

    if (this.conversationMessageService.userIsSchedulerConversationMessageWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Messages", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationMessageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageSubmitData: ConversationMessageSubmitData = {
        id: this.conversationMessageData?.id || 0,
        conversationId: Number(formValue.conversationId),
        userId: Number(formValue.userId),
        parentConversationMessageId: formValue.parentConversationMessageId ? Number(formValue.parentConversationMessageId) : null,
        conversationChannelId: formValue.conversationChannelId ? Number(formValue.conversationChannelId) : null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        message: formValue.message!.trim(),
        messageType: formValue.messageType?.trim() || null,
        entity: formValue.entity?.trim() || null,
        entityId: formValue.entityId ? Number(formValue.entityId) : null,
        externalURL: formValue.externalURL?.trim() || null,
        forwardedFromMessageId: formValue.forwardedFromMessageId ? Number(formValue.forwardedFromMessageId) : null,
        forwardedFromUserId: formValue.forwardedFromUserId ? Number(formValue.forwardedFromUserId) : null,
        isScheduled: !!formValue.isScheduled,
        scheduledDateTime: formValue.scheduledDateTime ? dateTimeLocalToIsoUtc(formValue.scheduledDateTime.trim()) : null,
        versionNumber: this.conversationMessageData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationMessageService.PutConversationMessage(conversationMessageSubmitData.id, conversationMessageSubmitData)
      : this.conversationMessageService.PostConversationMessage(conversationMessageSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationMessageData) => {

        this.conversationMessageService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation Message's detail page
          //
          this.conversationMessageForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationMessageForm.markAsUntouched();

          this.router.navigate(['/conversationmessages', savedConversationMessageData.id]);
          this.alertService.showMessage('Conversation Message added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationMessageData = savedConversationMessageData;
          this.buildFormValues(this.conversationMessageData);

          this.alertService.showMessage("Conversation Message saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation Message.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationMessageReader(): boolean {
    return this.conversationMessageService.userIsSchedulerConversationMessageReader();
  }

  public userIsSchedulerConversationMessageWriter(): boolean {
    return this.conversationMessageService.userIsSchedulerConversationMessageWriter();
  }
}

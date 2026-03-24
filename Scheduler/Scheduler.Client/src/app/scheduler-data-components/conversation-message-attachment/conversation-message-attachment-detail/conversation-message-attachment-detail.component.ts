/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGEATTACHMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessageAttachment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-attachment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageAttachmentService, ConversationMessageAttachmentData, ConversationMessageAttachmentSubmitData } from '../../../scheduler-data-services/conversation-message-attachment.service';
import { ConversationMessageService } from '../../../scheduler-data-services/conversation-message.service';
import { ConversationMessageAttachmentChangeHistoryService } from '../../../scheduler-data-services/conversation-message-attachment-change-history.service';
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
interface ConversationMessageAttachmentFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  dateTimeCreated: string,
  contentFileName: string,
  contentSize: string,     // Stored as string for form input, converted to number on submit.
  contentData: string,
  contentMimeType: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-message-attachment-detail',
  templateUrl: './conversation-message-attachment-detail.component.html',
  styleUrls: ['./conversation-message-attachment-detail.component.scss']
})

export class ConversationMessageAttachmentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageAttachmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageAttachmentForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        userId: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        contentFileName: ['', Validators.required],
        contentSize: ['', Validators.required],
        contentData: ['', Validators.required],
        contentMimeType: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public conversationMessageAttachmentId: string | null = null;
  public conversationMessageAttachmentData: ConversationMessageAttachmentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationMessageAttachments$ = this.conversationMessageAttachmentService.GetConversationMessageAttachmentList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();
  public conversationMessageAttachmentChangeHistories$ = this.conversationMessageAttachmentChangeHistoryService.GetConversationMessageAttachmentChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationMessageAttachmentService: ConversationMessageAttachmentService,
    public conversationMessageService: ConversationMessageService,
    public conversationMessageAttachmentChangeHistoryService: ConversationMessageAttachmentChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationMessageAttachmentId from the route parameters
    this.conversationMessageAttachmentId = this.route.snapshot.paramMap.get('conversationMessageAttachmentId');

    if (this.conversationMessageAttachmentId === 'new' ||
        this.conversationMessageAttachmentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationMessageAttachmentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationMessageAttachmentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageAttachmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation Message Attachment';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation Message Attachment';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationMessageAttachmentForm.dirty) {
      return confirm('You have unsaved Conversation Message Attachment changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationMessageAttachmentId != null && this.conversationMessageAttachmentId !== 'new') {

      const id = parseInt(this.conversationMessageAttachmentId, 10);

      if (!isNaN(id)) {
        return { conversationMessageAttachmentId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationMessageAttachment data for the current conversationMessageAttachmentId.
  *
  * Fully respects the ConversationMessageAttachmentService caching strategy and error handling strategy.
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
    if (!this.conversationMessageAttachmentService.userIsSchedulerConversationMessageAttachmentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationMessageAttachments.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationMessageAttachmentId
    //
    if (!this.conversationMessageAttachmentId) {

      this.alertService.showMessage('No ConversationMessageAttachment ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationMessageAttachmentId = Number(this.conversationMessageAttachmentId);

    if (isNaN(conversationMessageAttachmentId) || conversationMessageAttachmentId <= 0) {

      this.alertService.showMessage(`Invalid Conversation Message Attachment ID: "${this.conversationMessageAttachmentId}"`,
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
      // This is the most targeted way: clear only this ConversationMessageAttachment + relations

      this.conversationMessageAttachmentService.ClearRecordCache(conversationMessageAttachmentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationMessageAttachmentService.GetConversationMessageAttachment(conversationMessageAttachmentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationMessageAttachmentData) => {

        //
        // Success path — conversationMessageAttachmentData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationMessageAttachmentData) {

          this.handleConversationMessageAttachmentNotFound(conversationMessageAttachmentId);

        } else {

          this.conversationMessageAttachmentData = conversationMessageAttachmentData;
          this.buildFormValues(this.conversationMessageAttachmentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationMessageAttachment loaded successfully',
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
        this.handleConversationMessageAttachmentLoadError(error, conversationMessageAttachmentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationMessageAttachmentNotFound(conversationMessageAttachmentId: number): void {

    this.conversationMessageAttachmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationMessageAttachment #${conversationMessageAttachmentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationMessageAttachmentLoadError(error: any, conversationMessageAttachmentId: number): void {

    let message = 'Failed to load Conversation Message Attachment.';
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
          message = 'You do not have permission to view this Conversation Message Attachment.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation Message Attachment #${conversationMessageAttachmentId} was not found.`;
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

    console.error(`Conversation Message Attachment load failed (ID: ${conversationMessageAttachmentId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationMessageAttachmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationMessageAttachmentData: ConversationMessageAttachmentData | null) {

    if (conversationMessageAttachmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageAttachmentForm.reset({
        conversationMessageId: null,
        userId: '',
        dateTimeCreated: '',
        contentFileName: '',
        contentSize: '',
        contentData: '',
        contentMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageAttachmentForm.reset({
        conversationMessageId: conversationMessageAttachmentData.conversationMessageId,
        userId: conversationMessageAttachmentData.userId?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationMessageAttachmentData.dateTimeCreated) ?? '',
        contentFileName: conversationMessageAttachmentData.contentFileName ?? '',
        contentSize: conversationMessageAttachmentData.contentSize?.toString() ?? '',
        contentData: conversationMessageAttachmentData.contentData ?? '',
        contentMimeType: conversationMessageAttachmentData.contentMimeType ?? '',
        versionNumber: conversationMessageAttachmentData.versionNumber?.toString() ?? '',
        active: conversationMessageAttachmentData.active ?? true,
        deleted: conversationMessageAttachmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageAttachmentForm.markAsPristine();
    this.conversationMessageAttachmentForm.markAsUntouched();
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

    if (this.conversationMessageAttachmentService.userIsSchedulerConversationMessageAttachmentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Message Attachments", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationMessageAttachmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageAttachmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageAttachmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageAttachmentSubmitData: ConversationMessageAttachmentSubmitData = {
        id: this.conversationMessageAttachmentData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        userId: Number(formValue.userId),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        contentFileName: formValue.contentFileName!.trim(),
        contentSize: Number(formValue.contentSize),
        contentData: formValue.contentData!.trim(),
        contentMimeType: formValue.contentMimeType!.trim(),
        versionNumber: this.conversationMessageAttachmentData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationMessageAttachmentService.PutConversationMessageAttachment(conversationMessageAttachmentSubmitData.id, conversationMessageAttachmentSubmitData)
      : this.conversationMessageAttachmentService.PostConversationMessageAttachment(conversationMessageAttachmentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationMessageAttachmentData) => {

        this.conversationMessageAttachmentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation Message Attachment's detail page
          //
          this.conversationMessageAttachmentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationMessageAttachmentForm.markAsUntouched();

          this.router.navigate(['/conversationmessageattachments', savedConversationMessageAttachmentData.id]);
          this.alertService.showMessage('Conversation Message Attachment added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationMessageAttachmentData = savedConversationMessageAttachmentData;
          this.buildFormValues(this.conversationMessageAttachmentData);

          this.alertService.showMessage("Conversation Message Attachment saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation Message Attachment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Attachment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Attachment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationMessageAttachmentReader(): boolean {
    return this.conversationMessageAttachmentService.userIsSchedulerConversationMessageAttachmentReader();
  }

  public userIsSchedulerConversationMessageAttachmentWriter(): boolean {
    return this.conversationMessageAttachmentService.userIsSchedulerConversationMessageAttachmentWriter();
  }
}

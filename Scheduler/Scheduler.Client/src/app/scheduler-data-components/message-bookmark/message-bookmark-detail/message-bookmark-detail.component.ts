/*
   GENERATED FORM FOR THE MESSAGEBOOKMARK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MessageBookmark table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to message-bookmark-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MessageBookmarkService, MessageBookmarkData, MessageBookmarkSubmitData } from '../../../scheduler-data-services/message-bookmark.service';
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
interface MessageBookmarkFormValues {
  userId: string,     // Stored as string for form input, converted to number on submit.
  conversationMessageId: number | bigint,       // For FK link number
  note: string | null,
  dateTimeCreated: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-message-bookmark-detail',
  templateUrl: './message-bookmark-detail.component.html',
  styleUrls: ['./message-bookmark-detail.component.scss']
})

export class MessageBookmarkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MessageBookmarkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public messageBookmarkForm: FormGroup = this.fb.group({
        userId: ['', Validators.required],
        conversationMessageId: [null, Validators.required],
        note: [''],
        dateTimeCreated: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public messageBookmarkId: string | null = null;
  public messageBookmarkData: MessageBookmarkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  messageBookmarks$ = this.messageBookmarkService.GetMessageBookmarkList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  private destroy$ = new Subject<void>();

  constructor(
    public messageBookmarkService: MessageBookmarkService,
    public conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the messageBookmarkId from the route parameters
    this.messageBookmarkId = this.route.snapshot.paramMap.get('messageBookmarkId');

    if (this.messageBookmarkId === 'new' ||
        this.messageBookmarkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.messageBookmarkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.messageBookmarkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.messageBookmarkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Message Bookmark';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Message Bookmark';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.messageBookmarkForm.dirty) {
      return confirm('You have unsaved Message Bookmark changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.messageBookmarkId != null && this.messageBookmarkId !== 'new') {

      const id = parseInt(this.messageBookmarkId, 10);

      if (!isNaN(id)) {
        return { messageBookmarkId: id };
      }
    }

    return null;
  }


/*
  * Loads the MessageBookmark data for the current messageBookmarkId.
  *
  * Fully respects the MessageBookmarkService caching strategy and error handling strategy.
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
    if (!this.messageBookmarkService.userIsSchedulerMessageBookmarkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MessageBookmarks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate messageBookmarkId
    //
    if (!this.messageBookmarkId) {

      this.alertService.showMessage('No MessageBookmark ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const messageBookmarkId = Number(this.messageBookmarkId);

    if (isNaN(messageBookmarkId) || messageBookmarkId <= 0) {

      this.alertService.showMessage(`Invalid Message Bookmark ID: "${this.messageBookmarkId}"`,
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
      // This is the most targeted way: clear only this MessageBookmark + relations

      this.messageBookmarkService.ClearRecordCache(messageBookmarkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.messageBookmarkService.GetMessageBookmark(messageBookmarkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (messageBookmarkData) => {

        //
        // Success path — messageBookmarkData can legitimately be null if 404'd but request succeeded
        //
        if (!messageBookmarkData) {

          this.handleMessageBookmarkNotFound(messageBookmarkId);

        } else {

          this.messageBookmarkData = messageBookmarkData;
          this.buildFormValues(this.messageBookmarkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MessageBookmark loaded successfully',
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
        this.handleMessageBookmarkLoadError(error, messageBookmarkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMessageBookmarkNotFound(messageBookmarkId: number): void {

    this.messageBookmarkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MessageBookmark #${messageBookmarkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMessageBookmarkLoadError(error: any, messageBookmarkId: number): void {

    let message = 'Failed to load Message Bookmark.';
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
          message = 'You do not have permission to view this Message Bookmark.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Message Bookmark #${messageBookmarkId} was not found.`;
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

    console.error(`Message Bookmark load failed (ID: ${messageBookmarkId})`, error);

    //
    // Reset UI to safe state
    //
    this.messageBookmarkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(messageBookmarkData: MessageBookmarkData | null) {

    if (messageBookmarkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.messageBookmarkForm.reset({
        userId: '',
        conversationMessageId: null,
        note: '',
        dateTimeCreated: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.messageBookmarkForm.reset({
        userId: messageBookmarkData.userId?.toString() ?? '',
        conversationMessageId: messageBookmarkData.conversationMessageId,
        note: messageBookmarkData.note ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(messageBookmarkData.dateTimeCreated) ?? '',
        active: messageBookmarkData.active ?? true,
        deleted: messageBookmarkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.messageBookmarkForm.markAsPristine();
    this.messageBookmarkForm.markAsUntouched();
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

    if (this.messageBookmarkService.userIsSchedulerMessageBookmarkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Message Bookmarks", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.messageBookmarkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.messageBookmarkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.messageBookmarkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const messageBookmarkSubmitData: MessageBookmarkSubmitData = {
        id: this.messageBookmarkData?.id || 0,
        userId: Number(formValue.userId),
        conversationMessageId: Number(formValue.conversationMessageId),
        note: formValue.note?.trim() || null,
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.messageBookmarkService.PutMessageBookmark(messageBookmarkSubmitData.id, messageBookmarkSubmitData)
      : this.messageBookmarkService.PostMessageBookmark(messageBookmarkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMessageBookmarkData) => {

        this.messageBookmarkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Message Bookmark's detail page
          //
          this.messageBookmarkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.messageBookmarkForm.markAsUntouched();

          this.router.navigate(['/messagebookmarks', savedMessageBookmarkData.id]);
          this.alertService.showMessage('Message Bookmark added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.messageBookmarkData = savedMessageBookmarkData;
          this.buildFormValues(this.messageBookmarkData);

          this.alertService.showMessage("Message Bookmark saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Message Bookmark.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Message Bookmark.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Message Bookmark could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerMessageBookmarkReader(): boolean {
    return this.messageBookmarkService.userIsSchedulerMessageBookmarkReader();
  }

  public userIsSchedulerMessageBookmarkWriter(): boolean {
    return this.messageBookmarkService.userIsSchedulerMessageBookmarkWriter();
  }
}

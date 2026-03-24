/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGELINKPREVIEW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessageLinkPreview table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-link-preview-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageLinkPreviewService, ConversationMessageLinkPreviewData, ConversationMessageLinkPreviewSubmitData } from '../../../scheduler-data-services/conversation-message-link-preview.service';
import { ConversationMessageService } from '../../../scheduler-data-services/conversation-message.service';
import { ConversationMessageLinkPreviewChangeHistoryService } from '../../../scheduler-data-services/conversation-message-link-preview-change-history.service';
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
interface ConversationMessageLinkPreviewFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  url: string,
  title: string | null,
  description: string | null,
  imageUrl: string | null,
  siteName: string | null,
  fetchedDateTime: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-message-link-preview-detail',
  templateUrl: './conversation-message-link-preview-detail.component.html',
  styleUrls: ['./conversation-message-link-preview-detail.component.scss']
})

export class ConversationMessageLinkPreviewDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageLinkPreviewFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageLinkPreviewForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        url: ['', Validators.required],
        title: [''],
        description: [''],
        imageUrl: [''],
        siteName: [''],
        fetchedDateTime: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public conversationMessageLinkPreviewId: string | null = null;
  public conversationMessageLinkPreviewData: ConversationMessageLinkPreviewData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationMessageLinkPreviews$ = this.conversationMessageLinkPreviewService.GetConversationMessageLinkPreviewList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();
  public conversationMessageLinkPreviewChangeHistories$ = this.conversationMessageLinkPreviewChangeHistoryService.GetConversationMessageLinkPreviewChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationMessageLinkPreviewService: ConversationMessageLinkPreviewService,
    public conversationMessageService: ConversationMessageService,
    public conversationMessageLinkPreviewChangeHistoryService: ConversationMessageLinkPreviewChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationMessageLinkPreviewId from the route parameters
    this.conversationMessageLinkPreviewId = this.route.snapshot.paramMap.get('conversationMessageLinkPreviewId');

    if (this.conversationMessageLinkPreviewId === 'new' ||
        this.conversationMessageLinkPreviewId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationMessageLinkPreviewData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationMessageLinkPreviewForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageLinkPreviewForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation Message Link Preview';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation Message Link Preview';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationMessageLinkPreviewForm.dirty) {
      return confirm('You have unsaved Conversation Message Link Preview changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationMessageLinkPreviewId != null && this.conversationMessageLinkPreviewId !== 'new') {

      const id = parseInt(this.conversationMessageLinkPreviewId, 10);

      if (!isNaN(id)) {
        return { conversationMessageLinkPreviewId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationMessageLinkPreview data for the current conversationMessageLinkPreviewId.
  *
  * Fully respects the ConversationMessageLinkPreviewService caching strategy and error handling strategy.
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
    if (!this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationMessageLinkPreviews.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationMessageLinkPreviewId
    //
    if (!this.conversationMessageLinkPreviewId) {

      this.alertService.showMessage('No ConversationMessageLinkPreview ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationMessageLinkPreviewId = Number(this.conversationMessageLinkPreviewId);

    if (isNaN(conversationMessageLinkPreviewId) || conversationMessageLinkPreviewId <= 0) {

      this.alertService.showMessage(`Invalid Conversation Message Link Preview ID: "${this.conversationMessageLinkPreviewId}"`,
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
      // This is the most targeted way: clear only this ConversationMessageLinkPreview + relations

      this.conversationMessageLinkPreviewService.ClearRecordCache(conversationMessageLinkPreviewId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationMessageLinkPreviewService.GetConversationMessageLinkPreview(conversationMessageLinkPreviewId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationMessageLinkPreviewData) => {

        //
        // Success path — conversationMessageLinkPreviewData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationMessageLinkPreviewData) {

          this.handleConversationMessageLinkPreviewNotFound(conversationMessageLinkPreviewId);

        } else {

          this.conversationMessageLinkPreviewData = conversationMessageLinkPreviewData;
          this.buildFormValues(this.conversationMessageLinkPreviewData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationMessageLinkPreview loaded successfully',
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
        this.handleConversationMessageLinkPreviewLoadError(error, conversationMessageLinkPreviewId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationMessageLinkPreviewNotFound(conversationMessageLinkPreviewId: number): void {

    this.conversationMessageLinkPreviewData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationMessageLinkPreview #${conversationMessageLinkPreviewId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationMessageLinkPreviewLoadError(error: any, conversationMessageLinkPreviewId: number): void {

    let message = 'Failed to load Conversation Message Link Preview.';
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
          message = 'You do not have permission to view this Conversation Message Link Preview.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation Message Link Preview #${conversationMessageLinkPreviewId} was not found.`;
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

    console.error(`Conversation Message Link Preview load failed (ID: ${conversationMessageLinkPreviewId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationMessageLinkPreviewData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationMessageLinkPreviewData: ConversationMessageLinkPreviewData | null) {

    if (conversationMessageLinkPreviewData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageLinkPreviewForm.reset({
        conversationMessageId: null,
        url: '',
        title: '',
        description: '',
        imageUrl: '',
        siteName: '',
        fetchedDateTime: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageLinkPreviewForm.reset({
        conversationMessageId: conversationMessageLinkPreviewData.conversationMessageId,
        url: conversationMessageLinkPreviewData.url ?? '',
        title: conversationMessageLinkPreviewData.title ?? '',
        description: conversationMessageLinkPreviewData.description ?? '',
        imageUrl: conversationMessageLinkPreviewData.imageUrl ?? '',
        siteName: conversationMessageLinkPreviewData.siteName ?? '',
        fetchedDateTime: isoUtcStringToDateTimeLocal(conversationMessageLinkPreviewData.fetchedDateTime) ?? '',
        versionNumber: conversationMessageLinkPreviewData.versionNumber?.toString() ?? '',
        active: conversationMessageLinkPreviewData.active ?? true,
        deleted: conversationMessageLinkPreviewData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageLinkPreviewForm.markAsPristine();
    this.conversationMessageLinkPreviewForm.markAsUntouched();
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

    if (this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Message Link Previews", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationMessageLinkPreviewForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageLinkPreviewForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageLinkPreviewForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageLinkPreviewSubmitData: ConversationMessageLinkPreviewSubmitData = {
        id: this.conversationMessageLinkPreviewData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        url: formValue.url!.trim(),
        title: formValue.title?.trim() || null,
        description: formValue.description?.trim() || null,
        imageUrl: formValue.imageUrl?.trim() || null,
        siteName: formValue.siteName?.trim() || null,
        fetchedDateTime: dateTimeLocalToIsoUtc(formValue.fetchedDateTime!.trim())!,
        versionNumber: this.conversationMessageLinkPreviewData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationMessageLinkPreviewService.PutConversationMessageLinkPreview(conversationMessageLinkPreviewSubmitData.id, conversationMessageLinkPreviewSubmitData)
      : this.conversationMessageLinkPreviewService.PostConversationMessageLinkPreview(conversationMessageLinkPreviewSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationMessageLinkPreviewData) => {

        this.conversationMessageLinkPreviewService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation Message Link Preview's detail page
          //
          this.conversationMessageLinkPreviewForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationMessageLinkPreviewForm.markAsUntouched();

          this.router.navigate(['/conversationmessagelinkpreviews', savedConversationMessageLinkPreviewData.id]);
          this.alertService.showMessage('Conversation Message Link Preview added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationMessageLinkPreviewData = savedConversationMessageLinkPreviewData;
          this.buildFormValues(this.conversationMessageLinkPreviewData);

          this.alertService.showMessage("Conversation Message Link Preview saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation Message Link Preview.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message Link Preview.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message Link Preview could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationMessageLinkPreviewReader(): boolean {
    return this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewReader();
  }

  public userIsSchedulerConversationMessageLinkPreviewWriter(): boolean {
    return this.conversationMessageLinkPreviewService.userIsSchedulerConversationMessageLinkPreviewWriter();
  }
}

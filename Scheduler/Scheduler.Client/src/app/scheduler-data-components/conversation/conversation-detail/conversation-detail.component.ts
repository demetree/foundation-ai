/*
   GENERATED FORM FOR THE CONVERSATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Conversation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationService, ConversationData, ConversationSubmitData } from '../../../scheduler-data-services/conversation.service';
import { ConversationTypeService } from '../../../scheduler-data-services/conversation-type.service';
import { ConversationUserService } from '../../../scheduler-data-services/conversation-user.service';
import { ConversationChannelService } from '../../../scheduler-data-services/conversation-channel.service';
import { ConversationMessageService } from '../../../scheduler-data-services/conversation-message.service';
import { ConversationPinService } from '../../../scheduler-data-services/conversation-pin.service';
import { ConversationThreadUserService } from '../../../scheduler-data-services/conversation-thread-user.service';
import { CallService } from '../../../scheduler-data-services/call.service';
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
interface ConversationFormValues {
  createdByUserId: string | null,     // Stored as string for form input, converted to number on submit.
  conversationTypeId: number | bigint | null,       // For FK link number
  priority: string,     // Stored as string for form input, converted to number on submit.
  dateTimeCreated: string,
  entity: string | null,
  entityId: string | null,     // Stored as string for form input, converted to number on submit.
  externalURL: string | null,
  name: string | null,
  description: string | null,
  isPublic: boolean | null,
  userId: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-detail',
  templateUrl: './conversation-detail.component.html',
  styleUrls: ['./conversation-detail.component.scss']
})

export class ConversationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationForm: FormGroup = this.fb.group({
        createdByUserId: [''],
        conversationTypeId: [null],
        priority: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        entity: [''],
        entityId: [''],
        externalURL: [''],
        name: [''],
        description: [''],
        isPublic: [false],
        userId: [''],
        active: [true],
        deleted: [false],
      });


  public conversationId: string | null = null;
  public conversationData: ConversationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversations$ = this.conversationService.GetConversationList();
  public conversationTypes$ = this.conversationTypeService.GetConversationTypeList();
  public conversationUsers$ = this.conversationUserService.GetConversationUserList();
  public conversationChannels$ = this.conversationChannelService.GetConversationChannelList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();
  public conversationPins$ = this.conversationPinService.GetConversationPinList();
  public conversationThreadUsers$ = this.conversationThreadUserService.GetConversationThreadUserList();
  public calls$ = this.callService.GetCallList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationService: ConversationService,
    public conversationTypeService: ConversationTypeService,
    public conversationUserService: ConversationUserService,
    public conversationChannelService: ConversationChannelService,
    public conversationMessageService: ConversationMessageService,
    public conversationPinService: ConversationPinService,
    public conversationThreadUserService: ConversationThreadUserService,
    public callService: CallService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationId from the route parameters
    this.conversationId = this.route.snapshot.paramMap.get('conversationId');

    if (this.conversationId === 'new' ||
        this.conversationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationForm.dirty) {
      return confirm('You have unsaved Conversation changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationId != null && this.conversationId !== 'new') {

      const id = parseInt(this.conversationId, 10);

      if (!isNaN(id)) {
        return { conversationId: id };
      }
    }

    return null;
  }


/*
  * Loads the Conversation data for the current conversationId.
  *
  * Fully respects the ConversationService caching strategy and error handling strategy.
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
    if (!this.conversationService.userIsSchedulerConversationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Conversations.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationId
    //
    if (!this.conversationId) {

      this.alertService.showMessage('No Conversation ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationId = Number(this.conversationId);

    if (isNaN(conversationId) || conversationId <= 0) {

      this.alertService.showMessage(`Invalid Conversation ID: "${this.conversationId}"`,
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
      // This is the most targeted way: clear only this Conversation + relations

      this.conversationService.ClearRecordCache(conversationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationService.GetConversation(conversationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationData) => {

        //
        // Success path — conversationData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationData) {

          this.handleConversationNotFound(conversationId);

        } else {

          this.conversationData = conversationData;
          this.buildFormValues(this.conversationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Conversation loaded successfully',
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
        this.handleConversationLoadError(error, conversationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationNotFound(conversationId: number): void {

    this.conversationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Conversation #${conversationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationLoadError(error: any, conversationId: number): void {

    let message = 'Failed to load Conversation.';
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
          message = 'You do not have permission to view this Conversation.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation #${conversationId} was not found.`;
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

    console.error(`Conversation load failed (ID: ${conversationId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationData: ConversationData | null) {

    if (conversationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationForm.reset({
        createdByUserId: '',
        conversationTypeId: null,
        priority: '',
        dateTimeCreated: '',
        entity: '',
        entityId: '',
        externalURL: '',
        name: '',
        description: '',
        isPublic: false,
        userId: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationForm.reset({
        createdByUserId: conversationData.createdByUserId?.toString() ?? '',
        conversationTypeId: conversationData.conversationTypeId,
        priority: conversationData.priority?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationData.dateTimeCreated) ?? '',
        entity: conversationData.entity ?? '',
        entityId: conversationData.entityId?.toString() ?? '',
        externalURL: conversationData.externalURL ?? '',
        name: conversationData.name ?? '',
        description: conversationData.description ?? '',
        isPublic: conversationData.isPublic ?? false,
        userId: conversationData.userId?.toString() ?? '',
        active: conversationData.active ?? true,
        deleted: conversationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationForm.markAsPristine();
    this.conversationForm.markAsUntouched();
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

    if (this.conversationService.userIsSchedulerConversationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversations", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationSubmitData: ConversationSubmitData = {
        id: this.conversationData?.id || 0,
        createdByUserId: formValue.createdByUserId ? Number(formValue.createdByUserId) : null,
        conversationTypeId: formValue.conversationTypeId ? Number(formValue.conversationTypeId) : null,
        priority: Number(formValue.priority),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        entity: formValue.entity?.trim() || null,
        entityId: formValue.entityId ? Number(formValue.entityId) : null,
        externalURL: formValue.externalURL?.trim() || null,
        name: formValue.name?.trim() || null,
        description: formValue.description?.trim() || null,
        isPublic: formValue.isPublic == true ? true : formValue.isPublic == false ? false : null,
        userId: formValue.userId ? Number(formValue.userId) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationService.PutConversation(conversationSubmitData.id, conversationSubmitData)
      : this.conversationService.PostConversation(conversationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationData) => {

        this.conversationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation's detail page
          //
          this.conversationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationForm.markAsUntouched();

          this.router.navigate(['/conversations', savedConversationData.id]);
          this.alertService.showMessage('Conversation added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationData = savedConversationData;
          this.buildFormValues(this.conversationData);

          this.alertService.showMessage("Conversation saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationReader(): boolean {
    return this.conversationService.userIsSchedulerConversationReader();
  }

  public userIsSchedulerConversationWriter(): boolean {
    return this.conversationService.userIsSchedulerConversationWriter();
  }
}

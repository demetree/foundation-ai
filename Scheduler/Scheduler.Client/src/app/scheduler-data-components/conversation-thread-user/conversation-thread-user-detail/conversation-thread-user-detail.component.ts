/*
   GENERATED FORM FOR THE CONVERSATIONTHREADUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationThreadUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-thread-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationThreadUserService, ConversationThreadUserData, ConversationThreadUserSubmitData } from '../../../scheduler-data-services/conversation-thread-user.service';
import { ConversationService } from '../../../scheduler-data-services/conversation.service';
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
interface ConversationThreadUserFormValues {
  conversationId: number | bigint,       // For FK link number
  parentConversationMessageId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  lastReadMessageId: string | null,     // Stored as string for form input, converted to number on submit.
  lastReadDateTime: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-thread-user-detail',
  templateUrl: './conversation-thread-user-detail.component.html',
  styleUrls: ['./conversation-thread-user-detail.component.scss']
})

export class ConversationThreadUserDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationThreadUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationThreadUserForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        parentConversationMessageId: [null, Validators.required],
        userId: ['', Validators.required],
        lastReadMessageId: [''],
        lastReadDateTime: [''],
        active: [true],
        deleted: [false],
      });


  public conversationThreadUserId: string | null = null;
  public conversationThreadUserData: ConversationThreadUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationThreadUsers$ = this.conversationThreadUserService.GetConversationThreadUserList();
  public conversations$ = this.conversationService.GetConversationList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationThreadUserService: ConversationThreadUserService,
    public conversationService: ConversationService,
    public conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationThreadUserId from the route parameters
    this.conversationThreadUserId = this.route.snapshot.paramMap.get('conversationThreadUserId');

    if (this.conversationThreadUserId === 'new' ||
        this.conversationThreadUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationThreadUserData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationThreadUserForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationThreadUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation Thread User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation Thread User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationThreadUserForm.dirty) {
      return confirm('You have unsaved Conversation Thread User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationThreadUserId != null && this.conversationThreadUserId !== 'new') {

      const id = parseInt(this.conversationThreadUserId, 10);

      if (!isNaN(id)) {
        return { conversationThreadUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationThreadUser data for the current conversationThreadUserId.
  *
  * Fully respects the ConversationThreadUserService caching strategy and error handling strategy.
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
    if (!this.conversationThreadUserService.userIsSchedulerConversationThreadUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationThreadUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationThreadUserId
    //
    if (!this.conversationThreadUserId) {

      this.alertService.showMessage('No ConversationThreadUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationThreadUserId = Number(this.conversationThreadUserId);

    if (isNaN(conversationThreadUserId) || conversationThreadUserId <= 0) {

      this.alertService.showMessage(`Invalid Conversation Thread User ID: "${this.conversationThreadUserId}"`,
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
      // This is the most targeted way: clear only this ConversationThreadUser + relations

      this.conversationThreadUserService.ClearRecordCache(conversationThreadUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationThreadUserService.GetConversationThreadUser(conversationThreadUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationThreadUserData) => {

        //
        // Success path — conversationThreadUserData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationThreadUserData) {

          this.handleConversationThreadUserNotFound(conversationThreadUserId);

        } else {

          this.conversationThreadUserData = conversationThreadUserData;
          this.buildFormValues(this.conversationThreadUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationThreadUser loaded successfully',
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
        this.handleConversationThreadUserLoadError(error, conversationThreadUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationThreadUserNotFound(conversationThreadUserId: number): void {

    this.conversationThreadUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationThreadUser #${conversationThreadUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationThreadUserLoadError(error: any, conversationThreadUserId: number): void {

    let message = 'Failed to load Conversation Thread User.';
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
          message = 'You do not have permission to view this Conversation Thread User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation Thread User #${conversationThreadUserId} was not found.`;
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

    console.error(`Conversation Thread User load failed (ID: ${conversationThreadUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationThreadUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationThreadUserData: ConversationThreadUserData | null) {

    if (conversationThreadUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationThreadUserForm.reset({
        conversationId: null,
        parentConversationMessageId: null,
        userId: '',
        lastReadMessageId: '',
        lastReadDateTime: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationThreadUserForm.reset({
        conversationId: conversationThreadUserData.conversationId,
        parentConversationMessageId: conversationThreadUserData.parentConversationMessageId,
        userId: conversationThreadUserData.userId?.toString() ?? '',
        lastReadMessageId: conversationThreadUserData.lastReadMessageId?.toString() ?? '',
        lastReadDateTime: isoUtcStringToDateTimeLocal(conversationThreadUserData.lastReadDateTime) ?? '',
        active: conversationThreadUserData.active ?? true,
        deleted: conversationThreadUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationThreadUserForm.markAsPristine();
    this.conversationThreadUserForm.markAsUntouched();
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

    if (this.conversationThreadUserService.userIsSchedulerConversationThreadUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Thread Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationThreadUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationThreadUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationThreadUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationThreadUserSubmitData: ConversationThreadUserSubmitData = {
        id: this.conversationThreadUserData?.id || 0,
        conversationId: Number(formValue.conversationId),
        parentConversationMessageId: Number(formValue.parentConversationMessageId),
        userId: Number(formValue.userId),
        lastReadMessageId: formValue.lastReadMessageId ? Number(formValue.lastReadMessageId) : null,
        lastReadDateTime: formValue.lastReadDateTime ? dateTimeLocalToIsoUtc(formValue.lastReadDateTime.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationThreadUserService.PutConversationThreadUser(conversationThreadUserSubmitData.id, conversationThreadUserSubmitData)
      : this.conversationThreadUserService.PostConversationThreadUser(conversationThreadUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationThreadUserData) => {

        this.conversationThreadUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation Thread User's detail page
          //
          this.conversationThreadUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationThreadUserForm.markAsUntouched();

          this.router.navigate(['/conversationthreadusers', savedConversationThreadUserData.id]);
          this.alertService.showMessage('Conversation Thread User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationThreadUserData = savedConversationThreadUserData;
          this.buildFormValues(this.conversationThreadUserData);

          this.alertService.showMessage("Conversation Thread User saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation Thread User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Thread User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Thread User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationThreadUserReader(): boolean {
    return this.conversationThreadUserService.userIsSchedulerConversationThreadUserReader();
  }

  public userIsSchedulerConversationThreadUserWriter(): boolean {
    return this.conversationThreadUserService.userIsSchedulerConversationThreadUserWriter();
  }
}

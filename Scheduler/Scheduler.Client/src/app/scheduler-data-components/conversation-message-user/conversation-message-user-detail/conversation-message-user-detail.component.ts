/*
   GENERATED FORM FOR THE CONVERSATIONMESSAGEUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationMessageUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-message-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationMessageUserService, ConversationMessageUserData, ConversationMessageUserSubmitData } from '../../../scheduler-data-services/conversation-message-user.service';
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
interface ConversationMessageUserFormValues {
  conversationMessageId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  dateTimeCreated: string,
  acknowledged: boolean,
  dateTimeAcknowledged: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-message-user-detail',
  templateUrl: './conversation-message-user-detail.component.html',
  styleUrls: ['./conversation-message-user-detail.component.scss']
})

export class ConversationMessageUserDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationMessageUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationMessageUserForm: FormGroup = this.fb.group({
        conversationMessageId: [null, Validators.required],
        userId: ['', Validators.required],
        dateTimeCreated: ['', Validators.required],
        acknowledged: [false],
        dateTimeAcknowledged: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public conversationMessageUserId: string | null = null;
  public conversationMessageUserData: ConversationMessageUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationMessageUsers$ = this.conversationMessageUserService.GetConversationMessageUserList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationMessageUserService: ConversationMessageUserService,
    public conversationMessageService: ConversationMessageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationMessageUserId from the route parameters
    this.conversationMessageUserId = this.route.snapshot.paramMap.get('conversationMessageUserId');

    if (this.conversationMessageUserId === 'new' ||
        this.conversationMessageUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationMessageUserData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationMessageUserForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationMessageUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation Message User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation Message User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationMessageUserForm.dirty) {
      return confirm('You have unsaved Conversation Message User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationMessageUserId != null && this.conversationMessageUserId !== 'new') {

      const id = parseInt(this.conversationMessageUserId, 10);

      if (!isNaN(id)) {
        return { conversationMessageUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationMessageUser data for the current conversationMessageUserId.
  *
  * Fully respects the ConversationMessageUserService caching strategy and error handling strategy.
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
    if (!this.conversationMessageUserService.userIsSchedulerConversationMessageUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationMessageUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationMessageUserId
    //
    if (!this.conversationMessageUserId) {

      this.alertService.showMessage('No ConversationMessageUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationMessageUserId = Number(this.conversationMessageUserId);

    if (isNaN(conversationMessageUserId) || conversationMessageUserId <= 0) {

      this.alertService.showMessage(`Invalid Conversation Message User ID: "${this.conversationMessageUserId}"`,
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
      // This is the most targeted way: clear only this ConversationMessageUser + relations

      this.conversationMessageUserService.ClearRecordCache(conversationMessageUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationMessageUserService.GetConversationMessageUser(conversationMessageUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationMessageUserData) => {

        //
        // Success path — conversationMessageUserData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationMessageUserData) {

          this.handleConversationMessageUserNotFound(conversationMessageUserId);

        } else {

          this.conversationMessageUserData = conversationMessageUserData;
          this.buildFormValues(this.conversationMessageUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationMessageUser loaded successfully',
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
        this.handleConversationMessageUserLoadError(error, conversationMessageUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationMessageUserNotFound(conversationMessageUserId: number): void {

    this.conversationMessageUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationMessageUser #${conversationMessageUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationMessageUserLoadError(error: any, conversationMessageUserId: number): void {

    let message = 'Failed to load Conversation Message User.';
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
          message = 'You do not have permission to view this Conversation Message User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation Message User #${conversationMessageUserId} was not found.`;
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

    console.error(`Conversation Message User load failed (ID: ${conversationMessageUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationMessageUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationMessageUserData: ConversationMessageUserData | null) {

    if (conversationMessageUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationMessageUserForm.reset({
        conversationMessageId: null,
        userId: '',
        dateTimeCreated: '',
        acknowledged: false,
        dateTimeAcknowledged: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationMessageUserForm.reset({
        conversationMessageId: conversationMessageUserData.conversationMessageId,
        userId: conversationMessageUserData.userId?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(conversationMessageUserData.dateTimeCreated) ?? '',
        acknowledged: conversationMessageUserData.acknowledged ?? false,
        dateTimeAcknowledged: isoUtcStringToDateTimeLocal(conversationMessageUserData.dateTimeAcknowledged) ?? '',
        active: conversationMessageUserData.active ?? true,
        deleted: conversationMessageUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationMessageUserForm.markAsPristine();
    this.conversationMessageUserForm.markAsUntouched();
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

    if (this.conversationMessageUserService.userIsSchedulerConversationMessageUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Message Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationMessageUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationMessageUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationMessageUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationMessageUserSubmitData: ConversationMessageUserSubmitData = {
        id: this.conversationMessageUserData?.id || 0,
        conversationMessageId: Number(formValue.conversationMessageId),
        userId: Number(formValue.userId),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        acknowledged: !!formValue.acknowledged,
        dateTimeAcknowledged: dateTimeLocalToIsoUtc(formValue.dateTimeAcknowledged!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationMessageUserService.PutConversationMessageUser(conversationMessageUserSubmitData.id, conversationMessageUserSubmitData)
      : this.conversationMessageUserService.PostConversationMessageUser(conversationMessageUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationMessageUserData) => {

        this.conversationMessageUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation Message User's detail page
          //
          this.conversationMessageUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationMessageUserForm.markAsUntouched();

          this.router.navigate(['/conversationmessageusers', savedConversationMessageUserData.id]);
          this.alertService.showMessage('Conversation Message User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationMessageUserData = savedConversationMessageUserData;
          this.buildFormValues(this.conversationMessageUserData);

          this.alertService.showMessage("Conversation Message User saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation Message User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Message User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Message User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationMessageUserReader(): boolean {
    return this.conversationMessageUserService.userIsSchedulerConversationMessageUserReader();
  }

  public userIsSchedulerConversationMessageUserWriter(): boolean {
    return this.conversationMessageUserService.userIsSchedulerConversationMessageUserWriter();
  }
}

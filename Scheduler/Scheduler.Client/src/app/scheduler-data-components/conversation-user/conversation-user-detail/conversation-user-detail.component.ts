/*
   GENERATED FORM FOR THE CONVERSATIONUSER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationUser table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-user-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationUserService, ConversationUserData, ConversationUserSubmitData } from '../../../scheduler-data-services/conversation-user.service';
import { ConversationService } from '../../../scheduler-data-services/conversation.service';
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
interface ConversationUserFormValues {
  conversationId: number | bigint,       // For FK link number
  userId: string,     // Stored as string for form input, converted to number on submit.
  role: string,
  dateTimeAdded: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-user-detail',
  templateUrl: './conversation-user-detail.component.html',
  styleUrls: ['./conversation-user-detail.component.scss']
})

export class ConversationUserDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationUserFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationUserForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        userId: ['', Validators.required],
        role: ['', Validators.required],
        dateTimeAdded: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public conversationUserId: string | null = null;
  public conversationUserData: ConversationUserData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationUsers$ = this.conversationUserService.GetConversationUserList();
  public conversations$ = this.conversationService.GetConversationList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationUserService: ConversationUserService,
    public conversationService: ConversationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationUserId from the route parameters
    this.conversationUserId = this.route.snapshot.paramMap.get('conversationUserId');

    if (this.conversationUserId === 'new' ||
        this.conversationUserId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationUserData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationUserForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationUserForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation User';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation User';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationUserForm.dirty) {
      return confirm('You have unsaved Conversation User changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationUserId != null && this.conversationUserId !== 'new') {

      const id = parseInt(this.conversationUserId, 10);

      if (!isNaN(id)) {
        return { conversationUserId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationUser data for the current conversationUserId.
  *
  * Fully respects the ConversationUserService caching strategy and error handling strategy.
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
    if (!this.conversationUserService.userIsSchedulerConversationUserReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationUsers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationUserId
    //
    if (!this.conversationUserId) {

      this.alertService.showMessage('No ConversationUser ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationUserId = Number(this.conversationUserId);

    if (isNaN(conversationUserId) || conversationUserId <= 0) {

      this.alertService.showMessage(`Invalid Conversation User ID: "${this.conversationUserId}"`,
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
      // This is the most targeted way: clear only this ConversationUser + relations

      this.conversationUserService.ClearRecordCache(conversationUserId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationUserService.GetConversationUser(conversationUserId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationUserData) => {

        //
        // Success path — conversationUserData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationUserData) {

          this.handleConversationUserNotFound(conversationUserId);

        } else {

          this.conversationUserData = conversationUserData;
          this.buildFormValues(this.conversationUserData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationUser loaded successfully',
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
        this.handleConversationUserLoadError(error, conversationUserId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationUserNotFound(conversationUserId: number): void {

    this.conversationUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationUser #${conversationUserId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationUserLoadError(error: any, conversationUserId: number): void {

    let message = 'Failed to load Conversation User.';
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
          message = 'You do not have permission to view this Conversation User.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation User #${conversationUserId} was not found.`;
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

    console.error(`Conversation User load failed (ID: ${conversationUserId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationUserData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationUserData: ConversationUserData | null) {

    if (conversationUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationUserForm.reset({
        conversationId: null,
        userId: '',
        role: '',
        dateTimeAdded: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationUserForm.reset({
        conversationId: conversationUserData.conversationId,
        userId: conversationUserData.userId?.toString() ?? '',
        role: conversationUserData.role ?? '',
        dateTimeAdded: isoUtcStringToDateTimeLocal(conversationUserData.dateTimeAdded) ?? '',
        active: conversationUserData.active ?? true,
        deleted: conversationUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationUserForm.markAsPristine();
    this.conversationUserForm.markAsUntouched();
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

    if (this.conversationUserService.userIsSchedulerConversationUserWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Users", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationUserSubmitData: ConversationUserSubmitData = {
        id: this.conversationUserData?.id || 0,
        conversationId: Number(formValue.conversationId),
        userId: Number(formValue.userId),
        role: formValue.role!.trim(),
        dateTimeAdded: dateTimeLocalToIsoUtc(formValue.dateTimeAdded!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationUserService.PutConversationUser(conversationUserSubmitData.id, conversationUserSubmitData)
      : this.conversationUserService.PostConversationUser(conversationUserSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationUserData) => {

        this.conversationUserService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation User's detail page
          //
          this.conversationUserForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationUserForm.markAsUntouched();

          this.router.navigate(['/conversationusers', savedConversationUserData.id]);
          this.alertService.showMessage('Conversation User added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationUserData = savedConversationUserData;
          this.buildFormValues(this.conversationUserData);

          this.alertService.showMessage("Conversation User saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationUserReader(): boolean {
    return this.conversationUserService.userIsSchedulerConversationUserReader();
  }

  public userIsSchedulerConversationUserWriter(): boolean {
    return this.conversationUserService.userIsSchedulerConversationUserWriter();
  }
}

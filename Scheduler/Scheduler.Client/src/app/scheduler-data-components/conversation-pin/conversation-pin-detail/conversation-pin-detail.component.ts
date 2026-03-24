/*
   GENERATED FORM FOR THE CONVERSATIONPIN TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationPin table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-pin-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationPinService, ConversationPinData, ConversationPinSubmitData } from '../../../scheduler-data-services/conversation-pin.service';
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
interface ConversationPinFormValues {
  conversationId: number | bigint,       // For FK link number
  conversationMessageId: number | bigint,       // For FK link number
  pinnedByUserId: string,     // Stored as string for form input, converted to number on submit.
  dateTimePinned: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-pin-detail',
  templateUrl: './conversation-pin-detail.component.html',
  styleUrls: ['./conversation-pin-detail.component.scss']
})

export class ConversationPinDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationPinFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationPinForm: FormGroup = this.fb.group({
        conversationId: [null, Validators.required],
        conversationMessageId: [null, Validators.required],
        pinnedByUserId: ['', Validators.required],
        dateTimePinned: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public conversationPinId: string | null = null;
  public conversationPinData: ConversationPinData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationPins$ = this.conversationPinService.GetConversationPinList();
  public conversations$ = this.conversationService.GetConversationList();
  public conversationMessages$ = this.conversationMessageService.GetConversationMessageList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationPinService: ConversationPinService,
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

    // Get the conversationPinId from the route parameters
    this.conversationPinId = this.route.snapshot.paramMap.get('conversationPinId');

    if (this.conversationPinId === 'new' ||
        this.conversationPinId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationPinData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationPinForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationPinForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation Pin';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation Pin';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationPinForm.dirty) {
      return confirm('You have unsaved Conversation Pin changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationPinId != null && this.conversationPinId !== 'new') {

      const id = parseInt(this.conversationPinId, 10);

      if (!isNaN(id)) {
        return { conversationPinId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationPin data for the current conversationPinId.
  *
  * Fully respects the ConversationPinService caching strategy and error handling strategy.
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
    if (!this.conversationPinService.userIsSchedulerConversationPinReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationPins.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationPinId
    //
    if (!this.conversationPinId) {

      this.alertService.showMessage('No ConversationPin ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationPinId = Number(this.conversationPinId);

    if (isNaN(conversationPinId) || conversationPinId <= 0) {

      this.alertService.showMessage(`Invalid Conversation Pin ID: "${this.conversationPinId}"`,
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
      // This is the most targeted way: clear only this ConversationPin + relations

      this.conversationPinService.ClearRecordCache(conversationPinId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationPinService.GetConversationPin(conversationPinId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationPinData) => {

        //
        // Success path — conversationPinData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationPinData) {

          this.handleConversationPinNotFound(conversationPinId);

        } else {

          this.conversationPinData = conversationPinData;
          this.buildFormValues(this.conversationPinData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationPin loaded successfully',
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
        this.handleConversationPinLoadError(error, conversationPinId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationPinNotFound(conversationPinId: number): void {

    this.conversationPinData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationPin #${conversationPinId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationPinLoadError(error: any, conversationPinId: number): void {

    let message = 'Failed to load Conversation Pin.';
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
          message = 'You do not have permission to view this Conversation Pin.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation Pin #${conversationPinId} was not found.`;
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

    console.error(`Conversation Pin load failed (ID: ${conversationPinId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationPinData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationPinData: ConversationPinData | null) {

    if (conversationPinData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationPinForm.reset({
        conversationId: null,
        conversationMessageId: null,
        pinnedByUserId: '',
        dateTimePinned: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationPinForm.reset({
        conversationId: conversationPinData.conversationId,
        conversationMessageId: conversationPinData.conversationMessageId,
        pinnedByUserId: conversationPinData.pinnedByUserId?.toString() ?? '',
        dateTimePinned: isoUtcStringToDateTimeLocal(conversationPinData.dateTimePinned) ?? '',
        active: conversationPinData.active ?? true,
        deleted: conversationPinData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationPinForm.markAsPristine();
    this.conversationPinForm.markAsUntouched();
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

    if (this.conversationPinService.userIsSchedulerConversationPinWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Pins", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationPinForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationPinForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationPinForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationPinSubmitData: ConversationPinSubmitData = {
        id: this.conversationPinData?.id || 0,
        conversationId: Number(formValue.conversationId),
        conversationMessageId: Number(formValue.conversationMessageId),
        pinnedByUserId: Number(formValue.pinnedByUserId),
        dateTimePinned: dateTimeLocalToIsoUtc(formValue.dateTimePinned!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationPinService.PutConversationPin(conversationPinSubmitData.id, conversationPinSubmitData)
      : this.conversationPinService.PostConversationPin(conversationPinSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationPinData) => {

        this.conversationPinService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation Pin's detail page
          //
          this.conversationPinForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationPinForm.markAsUntouched();

          this.router.navigate(['/conversationpins', savedConversationPinData.id]);
          this.alertService.showMessage('Conversation Pin added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationPinData = savedConversationPinData;
          this.buildFormValues(this.conversationPinData);

          this.alertService.showMessage("Conversation Pin saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation Pin.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Pin.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Pin could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationPinReader(): boolean {
    return this.conversationPinService.userIsSchedulerConversationPinReader();
  }

  public userIsSchedulerConversationPinWriter(): boolean {
    return this.conversationPinService.userIsSchedulerConversationPinWriter();
  }
}

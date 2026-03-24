/*
   GENERATED FORM FOR THE CONVERSATIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ConversationType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to conversation-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConversationTypeService, ConversationTypeData, ConversationTypeSubmitData } from '../../../scheduler-data-services/conversation-type.service';
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
interface ConversationTypeFormValues {
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-conversation-type-detail',
  templateUrl: './conversation-type-detail.component.html',
  styleUrls: ['./conversation-type-detail.component.scss']
})

export class ConversationTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConversationTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public conversationTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public conversationTypeId: string | null = null;
  public conversationTypeData: ConversationTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  conversationTypes$ = this.conversationTypeService.GetConversationTypeList();
  public conversations$ = this.conversationService.GetConversationList();

  private destroy$ = new Subject<void>();

  constructor(
    public conversationTypeService: ConversationTypeService,
    public conversationService: ConversationService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the conversationTypeId from the route parameters
    this.conversationTypeId = this.route.snapshot.paramMap.get('conversationTypeId');

    if (this.conversationTypeId === 'new' ||
        this.conversationTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.conversationTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.conversationTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.conversationTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Conversation Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Conversation Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.conversationTypeForm.dirty) {
      return confirm('You have unsaved Conversation Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.conversationTypeId != null && this.conversationTypeId !== 'new') {

      const id = parseInt(this.conversationTypeId, 10);

      if (!isNaN(id)) {
        return { conversationTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the ConversationType data for the current conversationTypeId.
  *
  * Fully respects the ConversationTypeService caching strategy and error handling strategy.
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
    if (!this.conversationTypeService.userIsSchedulerConversationTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ConversationTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate conversationTypeId
    //
    if (!this.conversationTypeId) {

      this.alertService.showMessage('No ConversationType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const conversationTypeId = Number(this.conversationTypeId);

    if (isNaN(conversationTypeId) || conversationTypeId <= 0) {

      this.alertService.showMessage(`Invalid Conversation Type ID: "${this.conversationTypeId}"`,
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
      // This is the most targeted way: clear only this ConversationType + relations

      this.conversationTypeService.ClearRecordCache(conversationTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.conversationTypeService.GetConversationType(conversationTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (conversationTypeData) => {

        //
        // Success path — conversationTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!conversationTypeData) {

          this.handleConversationTypeNotFound(conversationTypeId);

        } else {

          this.conversationTypeData = conversationTypeData;
          this.buildFormValues(this.conversationTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ConversationType loaded successfully',
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
        this.handleConversationTypeLoadError(error, conversationTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleConversationTypeNotFound(conversationTypeId: number): void {

    this.conversationTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ConversationType #${conversationTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleConversationTypeLoadError(error: any, conversationTypeId: number): void {

    let message = 'Failed to load Conversation Type.';
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
          message = 'You do not have permission to view this Conversation Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Conversation Type #${conversationTypeId} was not found.`;
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

    console.error(`Conversation Type load failed (ID: ${conversationTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.conversationTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(conversationTypeData: ConversationTypeData | null) {

    if (conversationTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.conversationTypeForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.conversationTypeForm.reset({
        name: conversationTypeData.name ?? '',
        description: conversationTypeData.description ?? '',
        active: conversationTypeData.active ?? true,
        deleted: conversationTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.conversationTypeForm.markAsPristine();
    this.conversationTypeForm.markAsUntouched();
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

    if (this.conversationTypeService.userIsSchedulerConversationTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Conversation Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.conversationTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.conversationTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.conversationTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const conversationTypeSubmitData: ConversationTypeSubmitData = {
        id: this.conversationTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.conversationTypeService.PutConversationType(conversationTypeSubmitData.id, conversationTypeSubmitData)
      : this.conversationTypeService.PostConversationType(conversationTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedConversationTypeData) => {

        this.conversationTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Conversation Type's detail page
          //
          this.conversationTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.conversationTypeForm.markAsUntouched();

          this.router.navigate(['/conversationtypes', savedConversationTypeData.id]);
          this.alertService.showMessage('Conversation Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.conversationTypeData = savedConversationTypeData;
          this.buildFormValues(this.conversationTypeData);

          this.alertService.showMessage("Conversation Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Conversation Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Conversation Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Conversation Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerConversationTypeReader(): boolean {
    return this.conversationTypeService.userIsSchedulerConversationTypeReader();
  }

  public userIsSchedulerConversationTypeWriter(): boolean {
    return this.conversationTypeService.userIsSchedulerConversationTypeWriter();
  }
}

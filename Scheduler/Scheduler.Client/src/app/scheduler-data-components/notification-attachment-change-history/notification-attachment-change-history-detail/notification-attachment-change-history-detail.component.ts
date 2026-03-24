/*
   GENERATED FORM FOR THE NOTIFICATIONATTACHMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationAttachmentChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-attachment-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationAttachmentChangeHistoryService, NotificationAttachmentChangeHistoryData, NotificationAttachmentChangeHistorySubmitData } from '../../../scheduler-data-services/notification-attachment-change-history.service';
import { NotificationAttachmentService } from '../../../scheduler-data-services/notification-attachment.service';
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
interface NotificationAttachmentChangeHistoryFormValues {
  notificationAttachmentId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-notification-attachment-change-history-detail',
  templateUrl: './notification-attachment-change-history-detail.component.html',
  styleUrls: ['./notification-attachment-change-history-detail.component.scss']
})

export class NotificationAttachmentChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationAttachmentChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationAttachmentChangeHistoryForm: FormGroup = this.fb.group({
        notificationAttachmentId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public notificationAttachmentChangeHistoryId: string | null = null;
  public notificationAttachmentChangeHistoryData: NotificationAttachmentChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  notificationAttachmentChangeHistories$ = this.notificationAttachmentChangeHistoryService.GetNotificationAttachmentChangeHistoryList();
  public notificationAttachments$ = this.notificationAttachmentService.GetNotificationAttachmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public notificationAttachmentChangeHistoryService: NotificationAttachmentChangeHistoryService,
    public notificationAttachmentService: NotificationAttachmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the notificationAttachmentChangeHistoryId from the route parameters
    this.notificationAttachmentChangeHistoryId = this.route.snapshot.paramMap.get('notificationAttachmentChangeHistoryId');

    if (this.notificationAttachmentChangeHistoryId === 'new' ||
        this.notificationAttachmentChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.notificationAttachmentChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.notificationAttachmentChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationAttachmentChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Notification Attachment Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Notification Attachment Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.notificationAttachmentChangeHistoryForm.dirty) {
      return confirm('You have unsaved Notification Attachment Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.notificationAttachmentChangeHistoryId != null && this.notificationAttachmentChangeHistoryId !== 'new') {

      const id = parseInt(this.notificationAttachmentChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { notificationAttachmentChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the NotificationAttachmentChangeHistory data for the current notificationAttachmentChangeHistoryId.
  *
  * Fully respects the NotificationAttachmentChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.notificationAttachmentChangeHistoryService.userIsSchedulerNotificationAttachmentChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read NotificationAttachmentChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate notificationAttachmentChangeHistoryId
    //
    if (!this.notificationAttachmentChangeHistoryId) {

      this.alertService.showMessage('No NotificationAttachmentChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const notificationAttachmentChangeHistoryId = Number(this.notificationAttachmentChangeHistoryId);

    if (isNaN(notificationAttachmentChangeHistoryId) || notificationAttachmentChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Notification Attachment Change History ID: "${this.notificationAttachmentChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this NotificationAttachmentChangeHistory + relations

      this.notificationAttachmentChangeHistoryService.ClearRecordCache(notificationAttachmentChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.notificationAttachmentChangeHistoryService.GetNotificationAttachmentChangeHistory(notificationAttachmentChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (notificationAttachmentChangeHistoryData) => {

        //
        // Success path — notificationAttachmentChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!notificationAttachmentChangeHistoryData) {

          this.handleNotificationAttachmentChangeHistoryNotFound(notificationAttachmentChangeHistoryId);

        } else {

          this.notificationAttachmentChangeHistoryData = notificationAttachmentChangeHistoryData;
          this.buildFormValues(this.notificationAttachmentChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'NotificationAttachmentChangeHistory loaded successfully',
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
        this.handleNotificationAttachmentChangeHistoryLoadError(error, notificationAttachmentChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleNotificationAttachmentChangeHistoryNotFound(notificationAttachmentChangeHistoryId: number): void {

    this.notificationAttachmentChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `NotificationAttachmentChangeHistory #${notificationAttachmentChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleNotificationAttachmentChangeHistoryLoadError(error: any, notificationAttachmentChangeHistoryId: number): void {

    let message = 'Failed to load Notification Attachment Change History.';
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
          message = 'You do not have permission to view this Notification Attachment Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Notification Attachment Change History #${notificationAttachmentChangeHistoryId} was not found.`;
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

    console.error(`Notification Attachment Change History load failed (ID: ${notificationAttachmentChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.notificationAttachmentChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(notificationAttachmentChangeHistoryData: NotificationAttachmentChangeHistoryData | null) {

    if (notificationAttachmentChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationAttachmentChangeHistoryForm.reset({
        notificationAttachmentId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.notificationAttachmentChangeHistoryForm.reset({
        notificationAttachmentId: notificationAttachmentChangeHistoryData.notificationAttachmentId,
        versionNumber: notificationAttachmentChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(notificationAttachmentChangeHistoryData.timeStamp) ?? '',
        userId: notificationAttachmentChangeHistoryData.userId?.toString() ?? '',
        data: notificationAttachmentChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.notificationAttachmentChangeHistoryForm.markAsPristine();
    this.notificationAttachmentChangeHistoryForm.markAsUntouched();
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

    if (this.notificationAttachmentChangeHistoryService.userIsSchedulerNotificationAttachmentChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Notification Attachment Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.notificationAttachmentChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationAttachmentChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationAttachmentChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationAttachmentChangeHistorySubmitData: NotificationAttachmentChangeHistorySubmitData = {
        id: this.notificationAttachmentChangeHistoryData?.id || 0,
        notificationAttachmentId: Number(formValue.notificationAttachmentId),
        versionNumber: this.notificationAttachmentChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.notificationAttachmentChangeHistoryService.PutNotificationAttachmentChangeHistory(notificationAttachmentChangeHistorySubmitData.id, notificationAttachmentChangeHistorySubmitData)
      : this.notificationAttachmentChangeHistoryService.PostNotificationAttachmentChangeHistory(notificationAttachmentChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedNotificationAttachmentChangeHistoryData) => {

        this.notificationAttachmentChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Notification Attachment Change History's detail page
          //
          this.notificationAttachmentChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.notificationAttachmentChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/notificationattachmentchangehistories', savedNotificationAttachmentChangeHistoryData.id]);
          this.alertService.showMessage('Notification Attachment Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.notificationAttachmentChangeHistoryData = savedNotificationAttachmentChangeHistoryData;
          this.buildFormValues(this.notificationAttachmentChangeHistoryData);

          this.alertService.showMessage("Notification Attachment Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Notification Attachment Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Attachment Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Attachment Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerNotificationAttachmentChangeHistoryReader(): boolean {
    return this.notificationAttachmentChangeHistoryService.userIsSchedulerNotificationAttachmentChangeHistoryReader();
  }

  public userIsSchedulerNotificationAttachmentChangeHistoryWriter(): boolean {
    return this.notificationAttachmentChangeHistoryService.userIsSchedulerNotificationAttachmentChangeHistoryWriter();
  }
}

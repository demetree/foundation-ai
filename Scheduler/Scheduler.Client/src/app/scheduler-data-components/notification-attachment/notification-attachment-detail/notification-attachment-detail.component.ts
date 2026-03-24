/*
   GENERATED FORM FOR THE NOTIFICATIONATTACHMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from NotificationAttachment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to notification-attachment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NotificationAttachmentService, NotificationAttachmentData, NotificationAttachmentSubmitData } from '../../../scheduler-data-services/notification-attachment.service';
import { NotificationService } from '../../../scheduler-data-services/notification.service';
import { NotificationAttachmentChangeHistoryService } from '../../../scheduler-data-services/notification-attachment-change-history.service';
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
interface NotificationAttachmentFormValues {
  notificationId: number | bigint,       // For FK link number
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
  selector: 'app-notification-attachment-detail',
  templateUrl: './notification-attachment-detail.component.html',
  styleUrls: ['./notification-attachment-detail.component.scss']
})

export class NotificationAttachmentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<NotificationAttachmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public notificationAttachmentForm: FormGroup = this.fb.group({
        notificationId: [null, Validators.required],
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


  public notificationAttachmentId: string | null = null;
  public notificationAttachmentData: NotificationAttachmentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  notificationAttachments$ = this.notificationAttachmentService.GetNotificationAttachmentList();
  public notifications$ = this.notificationService.GetNotificationList();
  public notificationAttachmentChangeHistories$ = this.notificationAttachmentChangeHistoryService.GetNotificationAttachmentChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public notificationAttachmentService: NotificationAttachmentService,
    public notificationService: NotificationService,
    public notificationAttachmentChangeHistoryService: NotificationAttachmentChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the notificationAttachmentId from the route parameters
    this.notificationAttachmentId = this.route.snapshot.paramMap.get('notificationAttachmentId');

    if (this.notificationAttachmentId === 'new' ||
        this.notificationAttachmentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.notificationAttachmentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.notificationAttachmentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.notificationAttachmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Notification Attachment';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Notification Attachment';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.notificationAttachmentForm.dirty) {
      return confirm('You have unsaved Notification Attachment changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.notificationAttachmentId != null && this.notificationAttachmentId !== 'new') {

      const id = parseInt(this.notificationAttachmentId, 10);

      if (!isNaN(id)) {
        return { notificationAttachmentId: id };
      }
    }

    return null;
  }


/*
  * Loads the NotificationAttachment data for the current notificationAttachmentId.
  *
  * Fully respects the NotificationAttachmentService caching strategy and error handling strategy.
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
    if (!this.notificationAttachmentService.userIsSchedulerNotificationAttachmentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read NotificationAttachments.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate notificationAttachmentId
    //
    if (!this.notificationAttachmentId) {

      this.alertService.showMessage('No NotificationAttachment ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const notificationAttachmentId = Number(this.notificationAttachmentId);

    if (isNaN(notificationAttachmentId) || notificationAttachmentId <= 0) {

      this.alertService.showMessage(`Invalid Notification Attachment ID: "${this.notificationAttachmentId}"`,
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
      // This is the most targeted way: clear only this NotificationAttachment + relations

      this.notificationAttachmentService.ClearRecordCache(notificationAttachmentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.notificationAttachmentService.GetNotificationAttachment(notificationAttachmentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (notificationAttachmentData) => {

        //
        // Success path — notificationAttachmentData can legitimately be null if 404'd but request succeeded
        //
        if (!notificationAttachmentData) {

          this.handleNotificationAttachmentNotFound(notificationAttachmentId);

        } else {

          this.notificationAttachmentData = notificationAttachmentData;
          this.buildFormValues(this.notificationAttachmentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'NotificationAttachment loaded successfully',
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
        this.handleNotificationAttachmentLoadError(error, notificationAttachmentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleNotificationAttachmentNotFound(notificationAttachmentId: number): void {

    this.notificationAttachmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `NotificationAttachment #${notificationAttachmentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleNotificationAttachmentLoadError(error: any, notificationAttachmentId: number): void {

    let message = 'Failed to load Notification Attachment.';
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
          message = 'You do not have permission to view this Notification Attachment.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Notification Attachment #${notificationAttachmentId} was not found.`;
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

    console.error(`Notification Attachment load failed (ID: ${notificationAttachmentId})`, error);

    //
    // Reset UI to safe state
    //
    this.notificationAttachmentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(notificationAttachmentData: NotificationAttachmentData | null) {

    if (notificationAttachmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.notificationAttachmentForm.reset({
        notificationId: null,
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
        this.notificationAttachmentForm.reset({
        notificationId: notificationAttachmentData.notificationId,
        userId: notificationAttachmentData.userId?.toString() ?? '',
        dateTimeCreated: isoUtcStringToDateTimeLocal(notificationAttachmentData.dateTimeCreated) ?? '',
        contentFileName: notificationAttachmentData.contentFileName ?? '',
        contentSize: notificationAttachmentData.contentSize?.toString() ?? '',
        contentData: notificationAttachmentData.contentData ?? '',
        contentMimeType: notificationAttachmentData.contentMimeType ?? '',
        versionNumber: notificationAttachmentData.versionNumber?.toString() ?? '',
        active: notificationAttachmentData.active ?? true,
        deleted: notificationAttachmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.notificationAttachmentForm.markAsPristine();
    this.notificationAttachmentForm.markAsUntouched();
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

    if (this.notificationAttachmentService.userIsSchedulerNotificationAttachmentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Notification Attachments", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.notificationAttachmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.notificationAttachmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.notificationAttachmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const notificationAttachmentSubmitData: NotificationAttachmentSubmitData = {
        id: this.notificationAttachmentData?.id || 0,
        notificationId: Number(formValue.notificationId),
        userId: Number(formValue.userId),
        dateTimeCreated: dateTimeLocalToIsoUtc(formValue.dateTimeCreated!.trim())!,
        contentFileName: formValue.contentFileName!.trim(),
        contentSize: Number(formValue.contentSize),
        contentData: formValue.contentData!.trim(),
        contentMimeType: formValue.contentMimeType!.trim(),
        versionNumber: this.notificationAttachmentData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.notificationAttachmentService.PutNotificationAttachment(notificationAttachmentSubmitData.id, notificationAttachmentSubmitData)
      : this.notificationAttachmentService.PostNotificationAttachment(notificationAttachmentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedNotificationAttachmentData) => {

        this.notificationAttachmentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Notification Attachment's detail page
          //
          this.notificationAttachmentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.notificationAttachmentForm.markAsUntouched();

          this.router.navigate(['/notificationattachments', savedNotificationAttachmentData.id]);
          this.alertService.showMessage('Notification Attachment added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.notificationAttachmentData = savedNotificationAttachmentData;
          this.buildFormValues(this.notificationAttachmentData);

          this.alertService.showMessage("Notification Attachment saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Notification Attachment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Notification Attachment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Notification Attachment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerNotificationAttachmentReader(): boolean {
    return this.notificationAttachmentService.userIsSchedulerNotificationAttachmentReader();
  }

  public userIsSchedulerNotificationAttachmentWriter(): boolean {
    return this.notificationAttachmentService.userIsSchedulerNotificationAttachmentWriter();
  }
}

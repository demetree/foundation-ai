/*
   GENERATED FORM FOR THE ANNOUNCEMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AnnouncementChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to announcement-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AnnouncementChangeHistoryService, AnnouncementChangeHistoryData, AnnouncementChangeHistorySubmitData } from '../../../community-data-services/announcement-change-history.service';
import { AnnouncementService } from '../../../community-data-services/announcement.service';
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
interface AnnouncementChangeHistoryFormValues {
};


@Component({
  selector: 'app-announcement-change-history-detail',
  templateUrl: './announcement-change-history-detail.component.html',
  styleUrls: ['./announcement-change-history-detail.component.scss']
})

export class AnnouncementChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AnnouncementChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public announcementChangeHistoryForm: FormGroup = this.fb.group({
      });


  public announcementChangeHistoryId: string | null = null;
  public announcementChangeHistoryData: AnnouncementChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  announcementChangeHistories$ = this.announcementChangeHistoryService.GetAnnouncementChangeHistoryList();
  public announcements$ = this.announcementService.GetAnnouncementList();

  private destroy$ = new Subject<void>();

  constructor(
    public announcementChangeHistoryService: AnnouncementChangeHistoryService,
    public announcementService: AnnouncementService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the announcementChangeHistoryId from the route parameters
    this.announcementChangeHistoryId = this.route.snapshot.paramMap.get('announcementChangeHistoryId');

    if (this.announcementChangeHistoryId === 'new' ||
        this.announcementChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.announcementChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.announcementChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.announcementChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Announcement Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Announcement Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.announcementChangeHistoryForm.dirty) {
      return confirm('You have unsaved Announcement Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.announcementChangeHistoryId != null && this.announcementChangeHistoryId !== 'new') {

      const id = parseInt(this.announcementChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { announcementChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the AnnouncementChangeHistory data for the current announcementChangeHistoryId.
  *
  * Fully respects the AnnouncementChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AnnouncementChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate announcementChangeHistoryId
    //
    if (!this.announcementChangeHistoryId) {

      this.alertService.showMessage('No AnnouncementChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const announcementChangeHistoryId = Number(this.announcementChangeHistoryId);

    if (isNaN(announcementChangeHistoryId) || announcementChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Announcement Change History ID: "${this.announcementChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this AnnouncementChangeHistory + relations

      this.announcementChangeHistoryService.ClearRecordCache(announcementChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.announcementChangeHistoryService.GetAnnouncementChangeHistory(announcementChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (announcementChangeHistoryData) => {

        //
        // Success path — announcementChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!announcementChangeHistoryData) {

          this.handleAnnouncementChangeHistoryNotFound(announcementChangeHistoryId);

        } else {

          this.announcementChangeHistoryData = announcementChangeHistoryData;
          this.buildFormValues(this.announcementChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AnnouncementChangeHistory loaded successfully',
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
        this.handleAnnouncementChangeHistoryLoadError(error, announcementChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAnnouncementChangeHistoryNotFound(announcementChangeHistoryId: number): void {

    this.announcementChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AnnouncementChangeHistory #${announcementChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAnnouncementChangeHistoryLoadError(error: any, announcementChangeHistoryId: number): void {

    let message = 'Failed to load Announcement Change History.';
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
          message = 'You do not have permission to view this Announcement Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Announcement Change History #${announcementChangeHistoryId} was not found.`;
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

    console.error(`Announcement Change History load failed (ID: ${announcementChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.announcementChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(announcementChangeHistoryData: AnnouncementChangeHistoryData | null) {

    if (announcementChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.announcementChangeHistoryForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.announcementChangeHistoryForm.reset({
      }, { emitEvent: false});
    }

    this.announcementChangeHistoryForm.markAsPristine();
    this.announcementChangeHistoryForm.markAsUntouched();
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

    if (this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Announcement Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.announcementChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.announcementChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.announcementChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const announcementChangeHistorySubmitData: AnnouncementChangeHistorySubmitData = {
        id: this.announcementChangeHistoryData?.id || 0,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.announcementChangeHistoryService.PutAnnouncementChangeHistory(announcementChangeHistorySubmitData.id, announcementChangeHistorySubmitData)
      : this.announcementChangeHistoryService.PostAnnouncementChangeHistory(announcementChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAnnouncementChangeHistoryData) => {

        this.announcementChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Announcement Change History's detail page
          //
          this.announcementChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.announcementChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/announcementchangehistories', savedAnnouncementChangeHistoryData.id]);
          this.alertService.showMessage('Announcement Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.announcementChangeHistoryData = savedAnnouncementChangeHistoryData;
          this.buildFormValues(this.announcementChangeHistoryData);

          this.alertService.showMessage("Announcement Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Announcement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Announcement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Announcement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityAnnouncementChangeHistoryReader(): boolean {
    return this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryReader();
  }

  public userIsCommunityAnnouncementChangeHistoryWriter(): boolean {
    return this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryWriter();
  }
}

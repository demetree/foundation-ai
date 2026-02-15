/*
   GENERATED FORM FOR THE PLATFORMANNOUNCEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PlatformAnnouncement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to platform-announcement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PlatformAnnouncementService, PlatformAnnouncementData, PlatformAnnouncementSubmitData } from '../../../bmc-data-services/platform-announcement.service';
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
interface PlatformAnnouncementFormValues {
  name: string,
  body: string | null,
  announcementType: string | null,
  startDate: string,
  endDate: string | null,
  isActive: boolean,
  priority: string,     // Stored as string for form input, converted to number on submit.
  showOnLandingPage: boolean,
  showOnDashboard: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-platform-announcement-detail',
  templateUrl: './platform-announcement-detail.component.html',
  styleUrls: ['./platform-announcement-detail.component.scss']
})

export class PlatformAnnouncementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PlatformAnnouncementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public platformAnnouncementForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        body: [''],
        announcementType: [''],
        startDate: ['', Validators.required],
        endDate: [''],
        isActive: [false],
        priority: ['', Validators.required],
        showOnLandingPage: [false],
        showOnDashboard: [false],
        active: [true],
        deleted: [false],
      });


  public platformAnnouncementId: string | null = null;
  public platformAnnouncementData: PlatformAnnouncementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  platformAnnouncements$ = this.platformAnnouncementService.GetPlatformAnnouncementList();

  private destroy$ = new Subject<void>();

  constructor(
    public platformAnnouncementService: PlatformAnnouncementService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the platformAnnouncementId from the route parameters
    this.platformAnnouncementId = this.route.snapshot.paramMap.get('platformAnnouncementId');

    if (this.platformAnnouncementId === 'new' ||
        this.platformAnnouncementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.platformAnnouncementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.platformAnnouncementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.platformAnnouncementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Platform Announcement';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Platform Announcement';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.platformAnnouncementForm.dirty) {
      return confirm('You have unsaved Platform Announcement changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.platformAnnouncementId != null && this.platformAnnouncementId !== 'new') {

      const id = parseInt(this.platformAnnouncementId, 10);

      if (!isNaN(id)) {
        return { platformAnnouncementId: id };
      }
    }

    return null;
  }


/*
  * Loads the PlatformAnnouncement data for the current platformAnnouncementId.
  *
  * Fully respects the PlatformAnnouncementService caching strategy and error handling strategy.
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
    if (!this.platformAnnouncementService.userIsBMCPlatformAnnouncementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PlatformAnnouncements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate platformAnnouncementId
    //
    if (!this.platformAnnouncementId) {

      this.alertService.showMessage('No PlatformAnnouncement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const platformAnnouncementId = Number(this.platformAnnouncementId);

    if (isNaN(platformAnnouncementId) || platformAnnouncementId <= 0) {

      this.alertService.showMessage(`Invalid Platform Announcement ID: "${this.platformAnnouncementId}"`,
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
      // This is the most targeted way: clear only this PlatformAnnouncement + relations

      this.platformAnnouncementService.ClearRecordCache(platformAnnouncementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.platformAnnouncementService.GetPlatformAnnouncement(platformAnnouncementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (platformAnnouncementData) => {

        //
        // Success path — platformAnnouncementData can legitimately be null if 404'd but request succeeded
        //
        if (!platformAnnouncementData) {

          this.handlePlatformAnnouncementNotFound(platformAnnouncementId);

        } else {

          this.platformAnnouncementData = platformAnnouncementData;
          this.buildFormValues(this.platformAnnouncementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PlatformAnnouncement loaded successfully',
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
        this.handlePlatformAnnouncementLoadError(error, platformAnnouncementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePlatformAnnouncementNotFound(platformAnnouncementId: number): void {

    this.platformAnnouncementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PlatformAnnouncement #${platformAnnouncementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePlatformAnnouncementLoadError(error: any, platformAnnouncementId: number): void {

    let message = 'Failed to load Platform Announcement.';
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
          message = 'You do not have permission to view this Platform Announcement.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Platform Announcement #${platformAnnouncementId} was not found.`;
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

    console.error(`Platform Announcement load failed (ID: ${platformAnnouncementId})`, error);

    //
    // Reset UI to safe state
    //
    this.platformAnnouncementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(platformAnnouncementData: PlatformAnnouncementData | null) {

    if (platformAnnouncementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.platformAnnouncementForm.reset({
        name: '',
        body: '',
        announcementType: '',
        startDate: '',
        endDate: '',
        isActive: false,
        priority: '',
        showOnLandingPage: false,
        showOnDashboard: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.platformAnnouncementForm.reset({
        name: platformAnnouncementData.name ?? '',
        body: platformAnnouncementData.body ?? '',
        announcementType: platformAnnouncementData.announcementType ?? '',
        startDate: isoUtcStringToDateTimeLocal(platformAnnouncementData.startDate) ?? '',
        endDate: isoUtcStringToDateTimeLocal(platformAnnouncementData.endDate) ?? '',
        isActive: platformAnnouncementData.isActive ?? false,
        priority: platformAnnouncementData.priority?.toString() ?? '',
        showOnLandingPage: platformAnnouncementData.showOnLandingPage ?? false,
        showOnDashboard: platformAnnouncementData.showOnDashboard ?? false,
        active: platformAnnouncementData.active ?? true,
        deleted: platformAnnouncementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.platformAnnouncementForm.markAsPristine();
    this.platformAnnouncementForm.markAsUntouched();
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

    if (this.platformAnnouncementService.userIsBMCPlatformAnnouncementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Platform Announcements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.platformAnnouncementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.platformAnnouncementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.platformAnnouncementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const platformAnnouncementSubmitData: PlatformAnnouncementSubmitData = {
        id: this.platformAnnouncementData?.id || 0,
        name: formValue.name!.trim(),
        body: formValue.body?.trim() || null,
        announcementType: formValue.announcementType?.trim() || null,
        startDate: dateTimeLocalToIsoUtc(formValue.startDate!.trim())!,
        endDate: formValue.endDate ? dateTimeLocalToIsoUtc(formValue.endDate.trim()) : null,
        isActive: !!formValue.isActive,
        priority: Number(formValue.priority),
        showOnLandingPage: !!formValue.showOnLandingPage,
        showOnDashboard: !!formValue.showOnDashboard,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.platformAnnouncementService.PutPlatformAnnouncement(platformAnnouncementSubmitData.id, platformAnnouncementSubmitData)
      : this.platformAnnouncementService.PostPlatformAnnouncement(platformAnnouncementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPlatformAnnouncementData) => {

        this.platformAnnouncementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Platform Announcement's detail page
          //
          this.platformAnnouncementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.platformAnnouncementForm.markAsUntouched();

          this.router.navigate(['/platformannouncements', savedPlatformAnnouncementData.id]);
          this.alertService.showMessage('Platform Announcement added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.platformAnnouncementData = savedPlatformAnnouncementData;
          this.buildFormValues(this.platformAnnouncementData);

          this.alertService.showMessage("Platform Announcement saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Platform Announcement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Platform Announcement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Platform Announcement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCPlatformAnnouncementReader(): boolean {
    return this.platformAnnouncementService.userIsBMCPlatformAnnouncementReader();
  }

  public userIsBMCPlatformAnnouncementWriter(): boolean {
    return this.platformAnnouncementService.userIsBMCPlatformAnnouncementWriter();
  }
}

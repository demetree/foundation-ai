/*
   GENERATED FORM FOR THE SCHEDULELAYERMEMBER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ScheduleLayerMember table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to schedule-layer-member-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ScheduleLayerMemberService, ScheduleLayerMemberData, ScheduleLayerMemberSubmitData } from '../../../alerting-data-services/schedule-layer-member.service';
import { ScheduleLayerService } from '../../../alerting-data-services/schedule-layer.service';
import { ScheduleLayerMemberChangeHistoryService } from '../../../alerting-data-services/schedule-layer-member-change-history.service';
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
interface ScheduleLayerMemberFormValues {
  scheduleLayerId: number | bigint,       // For FK link number
  position: string,     // Stored as string for form input, converted to number on submit.
  securityUserObjectGuid: string,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-schedule-layer-member-detail',
  templateUrl: './schedule-layer-member-detail.component.html',
  styleUrls: ['./schedule-layer-member-detail.component.scss']
})

export class ScheduleLayerMemberDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ScheduleLayerMemberFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public scheduleLayerMemberForm: FormGroup = this.fb.group({
        scheduleLayerId: [null, Validators.required],
        position: ['', Validators.required],
        securityUserObjectGuid: ['', Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public scheduleLayerMemberId: string | null = null;
  public scheduleLayerMemberData: ScheduleLayerMemberData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  scheduleLayerMembers$ = this.scheduleLayerMemberService.GetScheduleLayerMemberList();
  public scheduleLayers$ = this.scheduleLayerService.GetScheduleLayerList();
  public scheduleLayerMemberChangeHistories$ = this.scheduleLayerMemberChangeHistoryService.GetScheduleLayerMemberChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public scheduleLayerMemberService: ScheduleLayerMemberService,
    public scheduleLayerService: ScheduleLayerService,
    public scheduleLayerMemberChangeHistoryService: ScheduleLayerMemberChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the scheduleLayerMemberId from the route parameters
    this.scheduleLayerMemberId = this.route.snapshot.paramMap.get('scheduleLayerMemberId');

    if (this.scheduleLayerMemberId === 'new' ||
        this.scheduleLayerMemberId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.scheduleLayerMemberData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.scheduleLayerMemberForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.scheduleLayerMemberForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Schedule Layer Member';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Schedule Layer Member';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.scheduleLayerMemberForm.dirty) {
      return confirm('You have unsaved Schedule Layer Member changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.scheduleLayerMemberId != null && this.scheduleLayerMemberId !== 'new') {

      const id = parseInt(this.scheduleLayerMemberId, 10);

      if (!isNaN(id)) {
        return { scheduleLayerMemberId: id };
      }
    }

    return null;
  }


/*
  * Loads the ScheduleLayerMember data for the current scheduleLayerMemberId.
  *
  * Fully respects the ScheduleLayerMemberService caching strategy and error handling strategy.
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
    if (!this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ScheduleLayerMembers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate scheduleLayerMemberId
    //
    if (!this.scheduleLayerMemberId) {

      this.alertService.showMessage('No ScheduleLayerMember ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const scheduleLayerMemberId = Number(this.scheduleLayerMemberId);

    if (isNaN(scheduleLayerMemberId) || scheduleLayerMemberId <= 0) {

      this.alertService.showMessage(`Invalid Schedule Layer Member ID: "${this.scheduleLayerMemberId}"`,
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
      // This is the most targeted way: clear only this ScheduleLayerMember + relations

      this.scheduleLayerMemberService.ClearRecordCache(scheduleLayerMemberId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.scheduleLayerMemberService.GetScheduleLayerMember(scheduleLayerMemberId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (scheduleLayerMemberData) => {

        //
        // Success path — scheduleLayerMemberData can legitimately be null if 404'd but request succeeded
        //
        if (!scheduleLayerMemberData) {

          this.handleScheduleLayerMemberNotFound(scheduleLayerMemberId);

        } else {

          this.scheduleLayerMemberData = scheduleLayerMemberData;
          this.buildFormValues(this.scheduleLayerMemberData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ScheduleLayerMember loaded successfully',
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
        this.handleScheduleLayerMemberLoadError(error, scheduleLayerMemberId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleScheduleLayerMemberNotFound(scheduleLayerMemberId: number): void {

    this.scheduleLayerMemberData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ScheduleLayerMember #${scheduleLayerMemberId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleScheduleLayerMemberLoadError(error: any, scheduleLayerMemberId: number): void {

    let message = 'Failed to load Schedule Layer Member.';
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
          message = 'You do not have permission to view this Schedule Layer Member.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Schedule Layer Member #${scheduleLayerMemberId} was not found.`;
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

    console.error(`Schedule Layer Member load failed (ID: ${scheduleLayerMemberId})`, error);

    //
    // Reset UI to safe state
    //
    this.scheduleLayerMemberData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(scheduleLayerMemberData: ScheduleLayerMemberData | null) {

    if (scheduleLayerMemberData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.scheduleLayerMemberForm.reset({
        scheduleLayerId: null,
        position: '',
        securityUserObjectGuid: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.scheduleLayerMemberForm.reset({
        scheduleLayerId: scheduleLayerMemberData.scheduleLayerId,
        position: scheduleLayerMemberData.position?.toString() ?? '',
        securityUserObjectGuid: scheduleLayerMemberData.securityUserObjectGuid ?? '',
        versionNumber: scheduleLayerMemberData.versionNumber?.toString() ?? '',
        active: scheduleLayerMemberData.active ?? true,
        deleted: scheduleLayerMemberData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.scheduleLayerMemberForm.markAsPristine();
    this.scheduleLayerMemberForm.markAsUntouched();
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

    if (this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Schedule Layer Members", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.scheduleLayerMemberForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.scheduleLayerMemberForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.scheduleLayerMemberForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const scheduleLayerMemberSubmitData: ScheduleLayerMemberSubmitData = {
        id: this.scheduleLayerMemberData?.id || 0,
        scheduleLayerId: Number(formValue.scheduleLayerId),
        position: Number(formValue.position),
        securityUserObjectGuid: formValue.securityUserObjectGuid!.trim(),
        versionNumber: this.scheduleLayerMemberData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.scheduleLayerMemberService.PutScheduleLayerMember(scheduleLayerMemberSubmitData.id, scheduleLayerMemberSubmitData)
      : this.scheduleLayerMemberService.PostScheduleLayerMember(scheduleLayerMemberSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedScheduleLayerMemberData) => {

        this.scheduleLayerMemberService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Schedule Layer Member's detail page
          //
          this.scheduleLayerMemberForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.scheduleLayerMemberForm.markAsUntouched();

          this.router.navigate(['/schedulelayermembers', savedScheduleLayerMemberData.id]);
          this.alertService.showMessage('Schedule Layer Member added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.scheduleLayerMemberData = savedScheduleLayerMemberData;
          this.buildFormValues(this.scheduleLayerMemberData);

          this.alertService.showMessage("Schedule Layer Member saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Schedule Layer Member.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Schedule Layer Member.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Schedule Layer Member could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingScheduleLayerMemberReader(): boolean {
    return this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberReader();
  }

  public userIsAlertingScheduleLayerMemberWriter(): boolean {
    return this.scheduleLayerMemberService.userIsAlertingScheduleLayerMemberWriter();
  }
}

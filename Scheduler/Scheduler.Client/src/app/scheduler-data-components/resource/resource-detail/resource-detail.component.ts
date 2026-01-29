/*
   GENERATED FORM FOR THE RESOURCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Resource table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceService, ResourceData, ResourceSubmitData } from '../../../scheduler-data-services/resource.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ResourceTypeService } from '../../../scheduler-data-services/resource-type.service';
import { ShiftPatternService } from '../../../scheduler-data-services/shift-pattern.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { ResourceChangeHistoryService } from '../../../scheduler-data-services/resource-change-history.service';
import { ResourceContactService } from '../../../scheduler-data-services/resource-contact.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { ResourceQualificationService } from '../../../scheduler-data-services/resource-qualification.service';
import { ResourceAvailabilityService } from '../../../scheduler-data-services/resource-availability.service';
import { ResourceShiftService } from '../../../scheduler-data-services/resource-shift.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { EventChargeService } from '../../../scheduler-data-services/event-charge.service';
import { NotificationSubscriptionService } from '../../../scheduler-data-services/notification-subscription.service';
import { VolunteerProfileService } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerGroupMemberService } from '../../../scheduler-data-services/volunteer-group-member.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
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
interface ResourceFormValues {
  name: string,
  description: string | null,
  officeId: number | bigint | null,       // For FK link number
  resourceTypeId: number | bigint,       // For FK link number
  shiftPatternId: number | bigint | null,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  targetWeeklyWorkHours: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  externalId: string | null,
  color: string | null,
  attributes: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-resource-detail',
  templateUrl: './resource-detail.component.html',
  styleUrls: ['./resource-detail.component.scss']
})

export class ResourceDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeId: [null],
        resourceTypeId: [null, Validators.required],
        shiftPatternId: [null],
        timeZoneId: [null, Validators.required],
        targetWeeklyWorkHours: [''],
        notes: [''],
        externalId: [''],
        color: [''],
        attributes: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public resourceId: string | null = null;
  public resourceData: ResourceData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  resources$ = this.resourceService.GetResourceList();
  public offices$ = this.officeService.GetOfficeList();
  public resourceTypes$ = this.resourceTypeService.GetResourceTypeList();
  public shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public resourceChangeHistories$ = this.resourceChangeHistoryService.GetResourceChangeHistoryList();
  public resourceContacts$ = this.resourceContactService.GetResourceContactList();
  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public resourceQualifications$ = this.resourceQualificationService.GetResourceQualificationList();
  public resourceAvailabilities$ = this.resourceAvailabilityService.GetResourceAvailabilityList();
  public resourceShifts$ = this.resourceShiftService.GetResourceShiftList();
  public crewMembers$ = this.crewMemberService.GetCrewMemberList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public eventCharges$ = this.eventChargeService.GetEventChargeList();
  public notificationSubscriptions$ = this.notificationSubscriptionService.GetNotificationSubscriptionList();
  public volunteerProfiles$ = this.volunteerProfileService.GetVolunteerProfileList();
  public volunteerGroupMembers$ = this.volunteerGroupMemberService.GetVolunteerGroupMemberList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public resourceService: ResourceService,
    public officeService: OfficeService,
    public resourceTypeService: ResourceTypeService,
    public shiftPatternService: ShiftPatternService,
    public timeZoneService: TimeZoneService,
    public resourceChangeHistoryService: ResourceChangeHistoryService,
    public resourceContactService: ResourceContactService,
    public rateSheetService: RateSheetService,
    public resourceQualificationService: ResourceQualificationService,
    public resourceAvailabilityService: ResourceAvailabilityService,
    public resourceShiftService: ResourceShiftService,
    public crewMemberService: CrewMemberService,
    public scheduledEventService: ScheduledEventService,
    public eventChargeService: EventChargeService,
    public notificationSubscriptionService: NotificationSubscriptionService,
    public volunteerProfileService: VolunteerProfileService,
    public volunteerGroupMemberService: VolunteerGroupMemberService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the resourceId from the route parameters
    this.resourceId = this.route.snapshot.paramMap.get('resourceId');

    if (this.resourceId === 'new' ||
        this.resourceId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.resourceData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.resourceForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Resource';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Resource';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.resourceForm.dirty) {
      return confirm('You have unsaved Resource changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.resourceId != null && this.resourceId !== 'new') {

      const id = parseInt(this.resourceId, 10);

      if (!isNaN(id)) {
        return { resourceId: id };
      }
    }

    return null;
  }


/*
  * Loads the Resource data for the current resourceId.
  *
  * Fully respects the ResourceService caching strategy and error handling strategy.
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
    if (!this.resourceService.userIsSchedulerResourceReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Resources.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate resourceId
    //
    if (!this.resourceId) {

      this.alertService.showMessage('No Resource ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const resourceId = Number(this.resourceId);

    if (isNaN(resourceId) || resourceId <= 0) {

      this.alertService.showMessage(`Invalid Resource ID: "${this.resourceId}"`,
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
      // This is the most targeted way: clear only this Resource + relations

      this.resourceService.ClearRecordCache(resourceId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.resourceService.GetResource(resourceId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (resourceData) => {

        //
        // Success path — resourceData can legitimately be null if 404'd but request succeeded
        //
        if (!resourceData) {

          this.handleResourceNotFound(resourceId);

        } else {

          this.resourceData = resourceData;
          this.buildFormValues(this.resourceData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Resource loaded successfully',
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
        this.handleResourceLoadError(error, resourceId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleResourceNotFound(resourceId: number): void {

    this.resourceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Resource #${resourceId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleResourceLoadError(error: any, resourceId: number): void {

    let message = 'Failed to load Resource.';
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
          message = 'You do not have permission to view this Resource.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Resource #${resourceId} was not found.`;
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

    console.error(`Resource load failed (ID: ${resourceId})`, error);

    //
    // Reset UI to safe state
    //
    this.resourceData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(resourceData: ResourceData | null) {

    if (resourceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceForm.reset({
        name: '',
        description: '',
        officeId: null,
        resourceTypeId: null,
        shiftPatternId: null,
        timeZoneId: null,
        targetWeeklyWorkHours: '',
        notes: '',
        externalId: '',
        color: '',
        attributes: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceForm.reset({
        name: resourceData.name ?? '',
        description: resourceData.description ?? '',
        officeId: resourceData.officeId,
        resourceTypeId: resourceData.resourceTypeId,
        shiftPatternId: resourceData.shiftPatternId,
        timeZoneId: resourceData.timeZoneId,
        targetWeeklyWorkHours: resourceData.targetWeeklyWorkHours?.toString() ?? '',
        notes: resourceData.notes ?? '',
        externalId: resourceData.externalId ?? '',
        color: resourceData.color ?? '',
        attributes: resourceData.attributes ?? '',
        avatarFileName: resourceData.avatarFileName ?? '',
        avatarSize: resourceData.avatarSize?.toString() ?? '',
        avatarData: resourceData.avatarData ?? '',
        avatarMimeType: resourceData.avatarMimeType ?? '',
        versionNumber: resourceData.versionNumber?.toString() ?? '',
        active: resourceData.active ?? true,
        deleted: resourceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceForm.markAsPristine();
    this.resourceForm.markAsUntouched();
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

    if (this.resourceService.userIsSchedulerResourceWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Resources", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.resourceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceSubmitData: ResourceSubmitData = {
        id: this.resourceData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        resourceTypeId: Number(formValue.resourceTypeId),
        shiftPatternId: formValue.shiftPatternId ? Number(formValue.shiftPatternId) : null,
        timeZoneId: Number(formValue.timeZoneId),
        targetWeeklyWorkHours: formValue.targetWeeklyWorkHours ? Number(formValue.targetWeeklyWorkHours) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.resourceData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.resourceService.PutResource(resourceSubmitData.id, resourceSubmitData)
      : this.resourceService.PostResource(resourceSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedResourceData) => {

        this.resourceService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Resource's detail page
          //
          this.resourceForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.resourceForm.markAsUntouched();

          this.router.navigate(['/resources', savedResourceData.id]);
          this.alertService.showMessage('Resource added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.resourceData = savedResourceData;
          this.buildFormValues(this.resourceData);

          this.alertService.showMessage("Resource saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Resource.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerResourceReader(): boolean {
    return this.resourceService.userIsSchedulerResourceReader();
  }

  public userIsSchedulerResourceWriter(): boolean {
    return this.resourceService.userIsSchedulerResourceWriter();
  }
}

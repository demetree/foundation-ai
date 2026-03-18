/*
   GENERATED FORM FOR THE ICON TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Icon table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to icon-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IconService, IconData, IconSubmitData } from '../../../scheduler-data-services/icon.service';
import { ResourceTypeService } from '../../../scheduler-data-services/resource-type.service';
import { PriorityService } from '../../../scheduler-data-services/priority.service';
import { ContactMethodService } from '../../../scheduler-data-services/contact-method.service';
import { InteractionTypeService } from '../../../scheduler-data-services/interaction-type.service';
import { TagService } from '../../../scheduler-data-services/tag.service';
import { VolunteerStatusService } from '../../../scheduler-data-services/volunteer-status.service';
import { ContactTypeService } from '../../../scheduler-data-services/contact-type.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { RelationshipTypeService } from '../../../scheduler-data-services/relationship-type.service';
import { OfficeTypeService } from '../../../scheduler-data-services/office-type.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { ClientTypeService } from '../../../scheduler-data-services/client-type.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { SchedulingTargetTypeService } from '../../../scheduler-data-services/scheduling-target-type.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { EventTypeService } from '../../../scheduler-data-services/event-type.service';
import { FundService } from '../../../scheduler-data-services/fund.service';
import { CampaignService } from '../../../scheduler-data-services/campaign.service';
import { AppealService } from '../../../scheduler-data-services/appeal.service';
import { HouseholdService } from '../../../scheduler-data-services/household.service';
import { ConstituentJourneyStageService } from '../../../scheduler-data-services/constituent-journey-stage.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { TributeService } from '../../../scheduler-data-services/tribute.service';
import { VolunteerProfileService } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerGroupService } from '../../../scheduler-data-services/volunteer-group.service';
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
interface IconFormValues {
  name: string,
  fontAwesomeCode: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-icon-detail',
  templateUrl: './icon-detail.component.html',
  styleUrls: ['./icon-detail.component.scss']
})

export class IconDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IconFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public iconForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        fontAwesomeCode: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public iconId: string | null = null;
  public iconData: IconData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  icons$ = this.iconService.GetIconList();
  public resourceTypes$ = this.resourceTypeService.GetResourceTypeList();
  public priorities$ = this.priorityService.GetPriorityList();
  public contactMethods$ = this.contactMethodService.GetContactMethodList();
  public interactionTypes$ = this.interactionTypeService.GetInteractionTypeList();
  public tags$ = this.tagService.GetTagList();
  public volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
  public contactTypes$ = this.contactTypeService.GetContactTypeList();
  public contacts$ = this.contactService.GetContactList();
  public relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();
  public officeTypes$ = this.officeTypeService.GetOfficeTypeList();
  public calendars$ = this.calendarService.GetCalendarList();
  public clientTypes$ = this.clientTypeService.GetClientTypeList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public schedulingTargetTypes$ = this.schedulingTargetTypeService.GetSchedulingTargetTypeList();
  public crews$ = this.crewService.GetCrewList();
  public crewMembers$ = this.crewMemberService.GetCrewMemberList();
  public eventTypes$ = this.eventTypeService.GetEventTypeList();
  public funds$ = this.fundService.GetFundList();
  public campaigns$ = this.campaignService.GetCampaignList();
  public appeals$ = this.appealService.GetAppealList();
  public households$ = this.householdService.GetHouseholdList();
  public constituentJourneyStages$ = this.constituentJourneyStageService.GetConstituentJourneyStageList();
  public constituents$ = this.constituentService.GetConstituentList();
  public tributes$ = this.tributeService.GetTributeList();
  public volunteerProfiles$ = this.volunteerProfileService.GetVolunteerProfileList();
  public volunteerGroups$ = this.volunteerGroupService.GetVolunteerGroupList();

  private destroy$ = new Subject<void>();

  constructor(
    public iconService: IconService,
    public resourceTypeService: ResourceTypeService,
    public priorityService: PriorityService,
    public contactMethodService: ContactMethodService,
    public interactionTypeService: InteractionTypeService,
    public tagService: TagService,
    public volunteerStatusService: VolunteerStatusService,
    public contactTypeService: ContactTypeService,
    public contactService: ContactService,
    public relationshipTypeService: RelationshipTypeService,
    public officeTypeService: OfficeTypeService,
    public calendarService: CalendarService,
    public clientTypeService: ClientTypeService,
    public assignmentRoleService: AssignmentRoleService,
    public schedulingTargetTypeService: SchedulingTargetTypeService,
    public crewService: CrewService,
    public crewMemberService: CrewMemberService,
    public eventTypeService: EventTypeService,
    public fundService: FundService,
    public campaignService: CampaignService,
    public appealService: AppealService,
    public householdService: HouseholdService,
    public constituentJourneyStageService: ConstituentJourneyStageService,
    public constituentService: ConstituentService,
    public tributeService: TributeService,
    public volunteerProfileService: VolunteerProfileService,
    public volunteerGroupService: VolunteerGroupService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the iconId from the route parameters
    this.iconId = this.route.snapshot.paramMap.get('iconId');

    if (this.iconId === 'new' ||
        this.iconId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.iconData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.iconForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.iconForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Icon';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Icon';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.iconForm.dirty) {
      return confirm('You have unsaved Icon changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.iconId != null && this.iconId !== 'new') {

      const id = parseInt(this.iconId, 10);

      if (!isNaN(id)) {
        return { iconId: id };
      }
    }

    return null;
  }


/*
  * Loads the Icon data for the current iconId.
  *
  * Fully respects the IconService caching strategy and error handling strategy.
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
    if (!this.iconService.userIsSchedulerIconReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Icons.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate iconId
    //
    if (!this.iconId) {

      this.alertService.showMessage('No Icon ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const iconId = Number(this.iconId);

    if (isNaN(iconId) || iconId <= 0) {

      this.alertService.showMessage(`Invalid Icon ID: "${this.iconId}"`,
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
      // This is the most targeted way: clear only this Icon + relations

      this.iconService.ClearRecordCache(iconId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.iconService.GetIcon(iconId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (iconData) => {

        //
        // Success path — iconData can legitimately be null if 404'd but request succeeded
        //
        if (!iconData) {

          this.handleIconNotFound(iconId);

        } else {

          this.iconData = iconData;
          this.buildFormValues(this.iconData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Icon loaded successfully',
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
        this.handleIconLoadError(error, iconId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleIconNotFound(iconId: number): void {

    this.iconData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Icon #${iconId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleIconLoadError(error: any, iconId: number): void {

    let message = 'Failed to load Icon.';
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
          message = 'You do not have permission to view this Icon.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Icon #${iconId} was not found.`;
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

    console.error(`Icon load failed (ID: ${iconId})`, error);

    //
    // Reset UI to safe state
    //
    this.iconData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(iconData: IconData | null) {

    if (iconData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.iconForm.reset({
        name: '',
        fontAwesomeCode: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.iconForm.reset({
        name: iconData.name ?? '',
        fontAwesomeCode: iconData.fontAwesomeCode ?? '',
        sequence: iconData.sequence?.toString() ?? '',
        active: iconData.active ?? true,
        deleted: iconData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.iconForm.markAsPristine();
    this.iconForm.markAsUntouched();
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

    if (this.iconService.userIsSchedulerIconWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Icons", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.iconForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.iconForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.iconForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const iconSubmitData: IconSubmitData = {
        id: this.iconData?.id || 0,
        name: formValue.name!.trim(),
        fontAwesomeCode: formValue.fontAwesomeCode?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.iconService.PutIcon(iconSubmitData.id, iconSubmitData)
      : this.iconService.PostIcon(iconSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedIconData) => {

        this.iconService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Icon's detail page
          //
          this.iconForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.iconForm.markAsUntouched();

          this.router.navigate(['/icons', savedIconData.id]);
          this.alertService.showMessage('Icon added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.iconData = savedIconData;
          this.buildFormValues(this.iconData);

          this.alertService.showMessage("Icon saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Icon.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Icon.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Icon could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerIconReader(): boolean {
    return this.iconService.userIsSchedulerIconReader();
  }

  public userIsSchedulerIconWriter(): boolean {
    return this.iconService.userIsSchedulerIconWriter();
  }
}

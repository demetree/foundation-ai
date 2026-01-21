/*
   GENERATED FORM FOR THE AUDITEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditEventService, AuditEventData, AuditEventSubmitData } from '../../../auditor-data-services/audit-event.service';
import { AuditUserService } from '../../../auditor-data-services/audit-user.service';
import { AuditSessionService } from '../../../auditor-data-services/audit-session.service';
import { AuditTypeService } from '../../../auditor-data-services/audit-type.service';
import { AuditAccessTypeService } from '../../../auditor-data-services/audit-access-type.service';
import { AuditSourceService } from '../../../auditor-data-services/audit-source.service';
import { AuditUserAgentService } from '../../../auditor-data-services/audit-user-agent.service';
import { AuditModuleService } from '../../../auditor-data-services/audit-module.service';
import { AuditModuleEntityService } from '../../../auditor-data-services/audit-module-entity.service';
import { AuditResourceService } from '../../../auditor-data-services/audit-resource.service';
import { AuditHostSystemService } from '../../../auditor-data-services/audit-host-system.service';
import { AuditEventEntityStateService } from '../../../auditor-data-services/audit-event-entity-state.service';
import { AuditEventErrorMessageService } from '../../../auditor-data-services/audit-event-error-message.service';
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
interface AuditEventFormValues {
  startTime: string,
  stopTime: string,
  completedSuccessfully: boolean,
  auditUserId: number | bigint,       // For FK link number
  auditSessionId: number | bigint,       // For FK link number
  auditTypeId: number | bigint,       // For FK link number
  auditAccessTypeId: number | bigint,       // For FK link number
  auditSourceId: number | bigint,       // For FK link number
  auditUserAgentId: number | bigint,       // For FK link number
  auditModuleId: number | bigint,       // For FK link number
  auditModuleEntityId: number | bigint,       // For FK link number
  auditResourceId: number | bigint,       // For FK link number
  auditHostSystemId: number | bigint,       // For FK link number
  primaryKey: string | null,
  threadId: string | null,     // Stored as string for form input, converted to number on submit.
  message: string,
};


@Component({
  selector: 'app-audit-event-detail',
  templateUrl: './audit-event-detail.component.html',
  styleUrls: ['./audit-event-detail.component.scss']
})

export class AuditEventDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditEventForm: FormGroup = this.fb.group({
        startTime: ['', Validators.required],
        stopTime: ['', Validators.required],
        completedSuccessfully: [false],
        auditUserId: [null, Validators.required],
        auditSessionId: [null, Validators.required],
        auditTypeId: [null, Validators.required],
        auditAccessTypeId: [null, Validators.required],
        auditSourceId: [null, Validators.required],
        auditUserAgentId: [null, Validators.required],
        auditModuleId: [null, Validators.required],
        auditModuleEntityId: [null, Validators.required],
        auditResourceId: [null, Validators.required],
        auditHostSystemId: [null, Validators.required],
        primaryKey: [''],
        threadId: [''],
        message: ['', Validators.required],
      });


  public auditEventId: string | null = null;
  public auditEventData: AuditEventData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditEvents$ = this.auditEventService.GetAuditEventList();
  public auditUsers$ = this.auditUserService.GetAuditUserList();
  public auditSessions$ = this.auditSessionService.GetAuditSessionList();
  public auditTypes$ = this.auditTypeService.GetAuditTypeList();
  public auditAccessTypes$ = this.auditAccessTypeService.GetAuditAccessTypeList();
  public auditSources$ = this.auditSourceService.GetAuditSourceList();
  public auditUserAgents$ = this.auditUserAgentService.GetAuditUserAgentList();
  public auditModules$ = this.auditModuleService.GetAuditModuleList();
  public auditModuleEntities$ = this.auditModuleEntityService.GetAuditModuleEntityList();
  public auditResources$ = this.auditResourceService.GetAuditResourceList();
  public auditHostSystems$ = this.auditHostSystemService.GetAuditHostSystemList();
  public auditEventEntityStates$ = this.auditEventEntityStateService.GetAuditEventEntityStateList();
  public auditEventErrorMessages$ = this.auditEventErrorMessageService.GetAuditEventErrorMessageList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditEventService: AuditEventService,
    public auditUserService: AuditUserService,
    public auditSessionService: AuditSessionService,
    public auditTypeService: AuditTypeService,
    public auditAccessTypeService: AuditAccessTypeService,
    public auditSourceService: AuditSourceService,
    public auditUserAgentService: AuditUserAgentService,
    public auditModuleService: AuditModuleService,
    public auditModuleEntityService: AuditModuleEntityService,
    public auditResourceService: AuditResourceService,
    public auditHostSystemService: AuditHostSystemService,
    public auditEventEntityStateService: AuditEventEntityStateService,
    public auditEventErrorMessageService: AuditEventErrorMessageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditEventId from the route parameters
    this.auditEventId = this.route.snapshot.paramMap.get('auditEventId');

    if (this.auditEventId === 'new' ||
        this.auditEventId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditEventData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.auditEventForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Event';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Event';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditEventForm.dirty) {
      return confirm('You have unsaved Audit Event changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditEventId != null && this.auditEventId !== 'new') {

      const id = parseInt(this.auditEventId, 10);

      if (!isNaN(id)) {
        return { auditEventId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditEvent data for the current auditEventId.
  *
  * Fully respects the AuditEventService caching strategy and error handling strategy.
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
    if (!this.auditEventService.userIsAuditorAuditEventReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditEvents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditEventId
    //
    if (!this.auditEventId) {

      this.alertService.showMessage('No AuditEvent ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditEventId = Number(this.auditEventId);

    if (isNaN(auditEventId) || auditEventId <= 0) {

      this.alertService.showMessage(`Invalid Audit Event ID: "${this.auditEventId}"`,
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
      // This is the most targeted way: clear only this AuditEvent + relations

      this.auditEventService.ClearRecordCache(auditEventId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditEventService.GetAuditEvent(auditEventId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditEventData) => {

        //
        // Success path — auditEventData can legitimately be null if 404'd but request succeeded
        //
        if (!auditEventData) {

          this.handleAuditEventNotFound(auditEventId);

        } else {

          this.auditEventData = auditEventData;
          this.buildFormValues(this.auditEventData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditEvent loaded successfully',
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
        this.handleAuditEventLoadError(error, auditEventId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditEventNotFound(auditEventId: number): void {

    this.auditEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditEvent #${auditEventId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditEventLoadError(error: any, auditEventId: number): void {

    let message = 'Failed to load Audit Event.';
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
          message = 'You do not have permission to view this Audit Event.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Event #${auditEventId} was not found.`;
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

    console.error(`Audit Event load failed (ID: ${auditEventId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditEventData: AuditEventData | null) {

    if (auditEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditEventForm.reset({
        startTime: '',
        stopTime: '',
        completedSuccessfully: false,
        auditUserId: null,
        auditSessionId: null,
        auditTypeId: null,
        auditAccessTypeId: null,
        auditSourceId: null,
        auditUserAgentId: null,
        auditModuleId: null,
        auditModuleEntityId: null,
        auditResourceId: null,
        auditHostSystemId: null,
        primaryKey: '',
        threadId: '',
        message: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditEventForm.reset({
        startTime: isoUtcStringToDateTimeLocal(auditEventData.startTime) ?? '',
        stopTime: isoUtcStringToDateTimeLocal(auditEventData.stopTime) ?? '',
        completedSuccessfully: auditEventData.completedSuccessfully ?? false,
        auditUserId: auditEventData.auditUserId,
        auditSessionId: auditEventData.auditSessionId,
        auditTypeId: auditEventData.auditTypeId,
        auditAccessTypeId: auditEventData.auditAccessTypeId,
        auditSourceId: auditEventData.auditSourceId,
        auditUserAgentId: auditEventData.auditUserAgentId,
        auditModuleId: auditEventData.auditModuleId,
        auditModuleEntityId: auditEventData.auditModuleEntityId,
        auditResourceId: auditEventData.auditResourceId,
        auditHostSystemId: auditEventData.auditHostSystemId,
        primaryKey: auditEventData.primaryKey ?? '',
        threadId: auditEventData.threadId?.toString() ?? '',
        message: auditEventData.message ?? '',
      }, { emitEvent: false});
    }

    this.auditEventForm.markAsPristine();
    this.auditEventForm.markAsUntouched();
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

    if (this.auditEventService.userIsAuditorAuditEventWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Events", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditEventSubmitData: AuditEventSubmitData = {
        id: this.auditEventData?.id || 0,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        stopTime: dateTimeLocalToIsoUtc(formValue.stopTime!.trim())!,
        completedSuccessfully: !!formValue.completedSuccessfully,
        auditUserId: Number(formValue.auditUserId),
        auditSessionId: Number(formValue.auditSessionId),
        auditTypeId: Number(formValue.auditTypeId),
        auditAccessTypeId: Number(formValue.auditAccessTypeId),
        auditSourceId: Number(formValue.auditSourceId),
        auditUserAgentId: Number(formValue.auditUserAgentId),
        auditModuleId: Number(formValue.auditModuleId),
        auditModuleEntityId: Number(formValue.auditModuleEntityId),
        auditResourceId: Number(formValue.auditResourceId),
        auditHostSystemId: Number(formValue.auditHostSystemId),
        primaryKey: formValue.primaryKey?.trim() || null,
        threadId: formValue.threadId ? Number(formValue.threadId) : null,
        message: formValue.message!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditEventService.PutAuditEvent(auditEventSubmitData.id, auditEventSubmitData)
      : this.auditEventService.PostAuditEvent(auditEventSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditEventData) => {

        this.auditEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Event's detail page
          //
          this.auditEventForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditEventForm.markAsUntouched();

          this.router.navigate(['/auditevents', savedAuditEventData.id]);
          this.alertService.showMessage('Audit Event added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditEventData = savedAuditEventData;
          this.buildFormValues(this.auditEventData);

          this.alertService.showMessage("Audit Event saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditEventReader(): boolean {
    return this.auditEventService.userIsAuditorAuditEventReader();
  }

  public userIsAuditorAuditEventWriter(): boolean {
    return this.auditEventService.userIsAuditorAuditEventWriter();
  }
}

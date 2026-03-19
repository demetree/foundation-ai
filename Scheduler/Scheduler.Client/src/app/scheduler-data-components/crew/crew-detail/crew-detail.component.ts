/*
   GENERATED FORM FOR THE CREW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Crew table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to crew-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CrewService, CrewData, CrewSubmitData } from '../../../scheduler-data-services/crew.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { CrewChangeHistoryService } from '../../../scheduler-data-services/crew-change-history.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
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
interface CrewFormValues {
  name: string,
  description: string | null,
  notes: string | null,
  officeId: number | bigint | null,       // For FK link number
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-crew-detail',
  templateUrl: './crew-detail.component.html',
  styleUrls: ['./crew-detail.component.scss']
})

export class CrewDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CrewFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public crewForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        notes: [''],
        officeId: [null],
        iconId: [null],
        color: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public crewId: string | null = null;
  public crewData: CrewData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  crews$ = this.crewService.GetCrewList();
  public offices$ = this.officeService.GetOfficeList();
  public icons$ = this.iconService.GetIconList();
  public crewChangeHistories$ = this.crewChangeHistoryService.GetCrewChangeHistoryList();
  public crewMembers$ = this.crewMemberService.GetCrewMemberList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public documents$ = this.documentService.GetDocumentList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public crewService: CrewService,
    public officeService: OfficeService,
    public iconService: IconService,
    public crewChangeHistoryService: CrewChangeHistoryService,
    public crewMemberService: CrewMemberService,
    public scheduledEventService: ScheduledEventService,
    public documentService: DocumentService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the crewId from the route parameters
    this.crewId = this.route.snapshot.paramMap.get('crewId');

    if (this.crewId === 'new' ||
        this.crewId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.crewData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.crewForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.crewForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Crew';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Crew';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.crewForm.dirty) {
      return confirm('You have unsaved Crew changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.crewId != null && this.crewId !== 'new') {

      const id = parseInt(this.crewId, 10);

      if (!isNaN(id)) {
        return { crewId: id };
      }
    }

    return null;
  }


/*
  * Loads the Crew data for the current crewId.
  *
  * Fully respects the CrewService caching strategy and error handling strategy.
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
    if (!this.crewService.userIsSchedulerCrewReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Crews.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate crewId
    //
    if (!this.crewId) {

      this.alertService.showMessage('No Crew ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const crewId = Number(this.crewId);

    if (isNaN(crewId) || crewId <= 0) {

      this.alertService.showMessage(`Invalid Crew ID: "${this.crewId}"`,
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
      // This is the most targeted way: clear only this Crew + relations

      this.crewService.ClearRecordCache(crewId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.crewService.GetCrew(crewId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (crewData) => {

        //
        // Success path — crewData can legitimately be null if 404'd but request succeeded
        //
        if (!crewData) {

          this.handleCrewNotFound(crewId);

        } else {

          this.crewData = crewData;
          this.buildFormValues(this.crewData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Crew loaded successfully',
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
        this.handleCrewLoadError(error, crewId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCrewNotFound(crewId: number): void {

    this.crewData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Crew #${crewId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCrewLoadError(error: any, crewId: number): void {

    let message = 'Failed to load Crew.';
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
          message = 'You do not have permission to view this Crew.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Crew #${crewId} was not found.`;
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

    console.error(`Crew load failed (ID: ${crewId})`, error);

    //
    // Reset UI to safe state
    //
    this.crewData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(crewData: CrewData | null) {

    if (crewData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.crewForm.reset({
        name: '',
        description: '',
        notes: '',
        officeId: null,
        iconId: null,
        color: '',
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
        this.crewForm.reset({
        name: crewData.name ?? '',
        description: crewData.description ?? '',
        notes: crewData.notes ?? '',
        officeId: crewData.officeId,
        iconId: crewData.iconId,
        color: crewData.color ?? '',
        avatarFileName: crewData.avatarFileName ?? '',
        avatarSize: crewData.avatarSize?.toString() ?? '',
        avatarData: crewData.avatarData ?? '',
        avatarMimeType: crewData.avatarMimeType ?? '',
        versionNumber: crewData.versionNumber?.toString() ?? '',
        active: crewData.active ?? true,
        deleted: crewData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.crewForm.markAsPristine();
    this.crewForm.markAsUntouched();
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

    if (this.crewService.userIsSchedulerCrewWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Crews", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.crewForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.crewForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.crewForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const crewSubmitData: CrewSubmitData = {
        id: this.crewData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        notes: formValue.notes?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.crewData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.crewService.PutCrew(crewSubmitData.id, crewSubmitData)
      : this.crewService.PostCrew(crewSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCrewData) => {

        this.crewService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Crew's detail page
          //
          this.crewForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.crewForm.markAsUntouched();

          this.router.navigate(['/crews', savedCrewData.id]);
          this.alertService.showMessage('Crew added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.crewData = savedCrewData;
          this.buildFormValues(this.crewData);

          this.alertService.showMessage("Crew saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Crew.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Crew.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Crew could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerCrewReader(): boolean {
    return this.crewService.userIsSchedulerCrewReader();
  }

  public userIsSchedulerCrewWriter(): boolean {
    return this.crewService.userIsSchedulerCrewWriter();
  }
}

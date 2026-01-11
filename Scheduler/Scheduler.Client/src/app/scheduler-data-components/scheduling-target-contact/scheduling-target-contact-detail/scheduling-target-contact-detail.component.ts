/*
   GENERATED FORM FOR THE SCHEDULINGTARGETCONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetContact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetContactService, SchedulingTargetContactData, SchedulingTargetContactSubmitData } from '../../../scheduler-data-services/scheduling-target-contact.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { RelationshipTypeService } from '../../../scheduler-data-services/relationship-type.service';
import { SchedulingTargetContactChangeHistoryService } from '../../../scheduler-data-services/scheduling-target-contact-change-history.service';
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
interface SchedulingTargetContactFormValues {
  schedulingTargetId: number | bigint,       // For FK link number
  contactId: number | bigint,       // For FK link number
  isPrimary: boolean,
  relationshipTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-scheduling-target-contact-detail',
  templateUrl: './scheduling-target-contact-detail.component.html',
  styleUrls: ['./scheduling-target-contact-detail.component.scss']
})

export class SchedulingTargetContactDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetContactForm: FormGroup = this.fb.group({
        schedulingTargetId: [null, Validators.required],
        contactId: [null, Validators.required],
        isPrimary: [false],
        relationshipTypeId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public schedulingTargetContactId: string | null = null;
  public schedulingTargetContactData: SchedulingTargetContactData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  schedulingTargetContacts$ = this.schedulingTargetContactService.GetSchedulingTargetContactList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public contacts$ = this.contactService.GetContactList();
  public relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();
  public schedulingTargetContactChangeHistories$ = this.schedulingTargetContactChangeHistoryService.GetSchedulingTargetContactChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public schedulingTargetContactService: SchedulingTargetContactService,
    public schedulingTargetService: SchedulingTargetService,
    public contactService: ContactService,
    public relationshipTypeService: RelationshipTypeService,
    public schedulingTargetContactChangeHistoryService: SchedulingTargetContactChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the schedulingTargetContactId from the route parameters
    this.schedulingTargetContactId = this.route.snapshot.paramMap.get('schedulingTargetContactId');

    if (this.schedulingTargetContactId === 'new' ||
        this.schedulingTargetContactId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.schedulingTargetContactData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.schedulingTargetContactForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetContactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Scheduling Target Contact';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Scheduling Target Contact';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.schedulingTargetContactForm.dirty) {
      return confirm('You have unsaved Scheduling Target Contact changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.schedulingTargetContactId != null && this.schedulingTargetContactId !== 'new') {

      const id = parseInt(this.schedulingTargetContactId, 10);

      if (!isNaN(id)) {
        return { schedulingTargetContactId: id };
      }
    }

    return null;
  }


/*
  * Loads the SchedulingTargetContact data for the current schedulingTargetContactId.
  *
  * Fully respects the SchedulingTargetContactService caching strategy and error handling strategy.
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
    if (!this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SchedulingTargetContacts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate schedulingTargetContactId
    //
    if (!this.schedulingTargetContactId) {

      this.alertService.showMessage('No SchedulingTargetContact ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const schedulingTargetContactId = Number(this.schedulingTargetContactId);

    if (isNaN(schedulingTargetContactId) || schedulingTargetContactId <= 0) {

      this.alertService.showMessage(`Invalid Scheduling Target Contact ID: "${this.schedulingTargetContactId}"`,
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
      // This is the most targeted way: clear only this SchedulingTargetContact + relations

      this.schedulingTargetContactService.ClearRecordCache(schedulingTargetContactId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.schedulingTargetContactService.GetSchedulingTargetContact(schedulingTargetContactId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (schedulingTargetContactData) => {

        //
        // Success path — schedulingTargetContactData can legitimately be null if 404'd but request succeeded
        //
        if (!schedulingTargetContactData) {

          this.handleSchedulingTargetContactNotFound(schedulingTargetContactId);

        } else {

          this.schedulingTargetContactData = schedulingTargetContactData;
          this.buildFormValues(this.schedulingTargetContactData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SchedulingTargetContact loaded successfully',
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
        this.handleSchedulingTargetContactLoadError(error, schedulingTargetContactId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSchedulingTargetContactNotFound(schedulingTargetContactId: number): void {

    this.schedulingTargetContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SchedulingTargetContact #${schedulingTargetContactId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSchedulingTargetContactLoadError(error: any, schedulingTargetContactId: number): void {

    let message = 'Failed to load Scheduling Target Contact.';
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
          message = 'You do not have permission to view this Scheduling Target Contact.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Scheduling Target Contact #${schedulingTargetContactId} was not found.`;
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

    console.error(`Scheduling Target Contact load failed (ID: ${schedulingTargetContactId})`, error);

    //
    // Reset UI to safe state
    //
    this.schedulingTargetContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(schedulingTargetContactData: SchedulingTargetContactData | null) {

    if (schedulingTargetContactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetContactForm.reset({
        schedulingTargetId: null,
        contactId: null,
        isPrimary: false,
        relationshipTypeId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.schedulingTargetContactForm.reset({
        schedulingTargetId: schedulingTargetContactData.schedulingTargetId,
        contactId: schedulingTargetContactData.contactId,
        isPrimary: schedulingTargetContactData.isPrimary ?? false,
        relationshipTypeId: schedulingTargetContactData.relationshipTypeId,
        versionNumber: schedulingTargetContactData.versionNumber?.toString() ?? '',
        active: schedulingTargetContactData.active ?? true,
        deleted: schedulingTargetContactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetContactForm.markAsPristine();
    this.schedulingTargetContactForm.markAsUntouched();
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

    if (this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Scheduling Target Contacts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.schedulingTargetContactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetContactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetContactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetContactSubmitData: SchedulingTargetContactSubmitData = {
        id: this.schedulingTargetContactData?.id || 0,
        schedulingTargetId: Number(formValue.schedulingTargetId),
        contactId: Number(formValue.contactId),
        isPrimary: !!formValue.isPrimary,
        relationshipTypeId: Number(formValue.relationshipTypeId),
        versionNumber: this.schedulingTargetContactData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.schedulingTargetContactService.PutSchedulingTargetContact(schedulingTargetContactSubmitData.id, schedulingTargetContactSubmitData)
      : this.schedulingTargetContactService.PostSchedulingTargetContact(schedulingTargetContactSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSchedulingTargetContactData) => {

        this.schedulingTargetContactService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Scheduling Target Contact's detail page
          //
          this.schedulingTargetContactForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.schedulingTargetContactForm.markAsUntouched();

          this.router.navigate(['/schedulingtargetcontacts', savedSchedulingTargetContactData.id]);
          this.alertService.showMessage('Scheduling Target Contact added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.schedulingTargetContactData = savedSchedulingTargetContactData;
          this.buildFormValues(this.schedulingTargetContactData);

          this.alertService.showMessage("Scheduling Target Contact saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Scheduling Target Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerSchedulingTargetContactReader(): boolean {
    return this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactReader();
  }

  public userIsSchedulerSchedulingTargetContactWriter(): boolean {
    return this.schedulingTargetContactService.userIsSchedulerSchedulingTargetContactWriter();
  }
}

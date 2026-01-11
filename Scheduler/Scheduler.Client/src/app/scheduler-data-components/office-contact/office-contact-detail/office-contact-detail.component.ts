/*
   GENERATED FORM FOR THE OFFICECONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from OfficeContact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to office-contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeContactService, OfficeContactData, OfficeContactSubmitData } from '../../../scheduler-data-services/office-contact.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { RelationshipTypeService } from '../../../scheduler-data-services/relationship-type.service';
import { OfficeContactChangeHistoryService } from '../../../scheduler-data-services/office-contact-change-history.service';
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
interface OfficeContactFormValues {
  officeId: number | bigint,       // For FK link number
  contactId: number | bigint,       // For FK link number
  isPrimary: boolean,
  relationshipTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-office-contact-detail',
  templateUrl: './office-contact-detail.component.html',
  styleUrls: ['./office-contact-detail.component.scss']
})

export class OfficeContactDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<OfficeContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public officeContactForm: FormGroup = this.fb.group({
        officeId: [null, Validators.required],
        contactId: [null, Validators.required],
        isPrimary: [false],
        relationshipTypeId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public officeContactId: string | null = null;
  public officeContactData: OfficeContactData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  officeContacts$ = this.officeContactService.GetOfficeContactList();
  public offices$ = this.officeService.GetOfficeList();
  public contacts$ = this.contactService.GetContactList();
  public relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();
  public officeContactChangeHistories$ = this.officeContactChangeHistoryService.GetOfficeContactChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public officeContactService: OfficeContactService,
    public officeService: OfficeService,
    public contactService: ContactService,
    public relationshipTypeService: RelationshipTypeService,
    public officeContactChangeHistoryService: OfficeContactChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the officeContactId from the route parameters
    this.officeContactId = this.route.snapshot.paramMap.get('officeContactId');

    if (this.officeContactId === 'new' ||
        this.officeContactId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.officeContactData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.officeContactForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.officeContactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Office Contact';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Office Contact';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.officeContactForm.dirty) {
      return confirm('You have unsaved Office Contact changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.officeContactId != null && this.officeContactId !== 'new') {

      const id = parseInt(this.officeContactId, 10);

      if (!isNaN(id)) {
        return { officeContactId: id };
      }
    }

    return null;
  }


/*
  * Loads the OfficeContact data for the current officeContactId.
  *
  * Fully respects the OfficeContactService caching strategy and error handling strategy.
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
    if (!this.officeContactService.userIsSchedulerOfficeContactReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read OfficeContacts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate officeContactId
    //
    if (!this.officeContactId) {

      this.alertService.showMessage('No OfficeContact ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const officeContactId = Number(this.officeContactId);

    if (isNaN(officeContactId) || officeContactId <= 0) {

      this.alertService.showMessage(`Invalid Office Contact ID: "${this.officeContactId}"`,
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
      // This is the most targeted way: clear only this OfficeContact + relations

      this.officeContactService.ClearRecordCache(officeContactId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.officeContactService.GetOfficeContact(officeContactId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (officeContactData) => {

        //
        // Success path — officeContactData can legitimately be null if 404'd but request succeeded
        //
        if (!officeContactData) {

          this.handleOfficeContactNotFound(officeContactId);

        } else {

          this.officeContactData = officeContactData;
          this.buildFormValues(this.officeContactData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'OfficeContact loaded successfully',
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
        this.handleOfficeContactLoadError(error, officeContactId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleOfficeContactNotFound(officeContactId: number): void {

    this.officeContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `OfficeContact #${officeContactId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleOfficeContactLoadError(error: any, officeContactId: number): void {

    let message = 'Failed to load Office Contact.';
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
          message = 'You do not have permission to view this Office Contact.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Office Contact #${officeContactId} was not found.`;
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

    console.error(`Office Contact load failed (ID: ${officeContactId})`, error);

    //
    // Reset UI to safe state
    //
    this.officeContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(officeContactData: OfficeContactData | null) {

    if (officeContactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.officeContactForm.reset({
        officeId: null,
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
        this.officeContactForm.reset({
        officeId: officeContactData.officeId,
        contactId: officeContactData.contactId,
        isPrimary: officeContactData.isPrimary ?? false,
        relationshipTypeId: officeContactData.relationshipTypeId,
        versionNumber: officeContactData.versionNumber?.toString() ?? '',
        active: officeContactData.active ?? true,
        deleted: officeContactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.officeContactForm.markAsPristine();
    this.officeContactForm.markAsUntouched();
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

    if (this.officeContactService.userIsSchedulerOfficeContactWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Office Contacts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.officeContactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.officeContactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.officeContactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const officeContactSubmitData: OfficeContactSubmitData = {
        id: this.officeContactData?.id || 0,
        officeId: Number(formValue.officeId),
        contactId: Number(formValue.contactId),
        isPrimary: !!formValue.isPrimary,
        relationshipTypeId: Number(formValue.relationshipTypeId),
        versionNumber: this.officeContactData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.officeContactService.PutOfficeContact(officeContactSubmitData.id, officeContactSubmitData)
      : this.officeContactService.PostOfficeContact(officeContactSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedOfficeContactData) => {

        this.officeContactService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Office Contact's detail page
          //
          this.officeContactForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.officeContactForm.markAsUntouched();

          this.router.navigate(['/officecontacts', savedOfficeContactData.id]);
          this.alertService.showMessage('Office Contact added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.officeContactData = savedOfficeContactData;
          this.buildFormValues(this.officeContactData);

          this.alertService.showMessage("Office Contact saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Office Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Office Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Office Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerOfficeContactReader(): boolean {
    return this.officeContactService.userIsSchedulerOfficeContactReader();
  }

  public userIsSchedulerOfficeContactWriter(): boolean {
    return this.officeContactService.userIsSchedulerOfficeContactWriter();
  }
}

/*
   GENERATED FORM FOR THE CLIENTCONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ClientContact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to client-contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ClientContactService, ClientContactData, ClientContactSubmitData } from '../../../scheduler-data-services/client-contact.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { RelationshipTypeService } from '../../../scheduler-data-services/relationship-type.service';
import { ClientContactChangeHistoryService } from '../../../scheduler-data-services/client-contact-change-history.service';
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
interface ClientContactFormValues {
  clientId: number | bigint,       // For FK link number
  contactId: number | bigint,       // For FK link number
  isPrimary: boolean,
  relationshipTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-client-contact-detail',
  templateUrl: './client-contact-detail.component.html',
  styleUrls: ['./client-contact-detail.component.scss']
})

export class ClientContactDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ClientContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public clientContactForm: FormGroup = this.fb.group({
        clientId: [null, Validators.required],
        contactId: [null, Validators.required],
        isPrimary: [false],
        relationshipTypeId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public clientContactId: string | null = null;
  public clientContactData: ClientContactData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  clientContacts$ = this.clientContactService.GetClientContactList();
  public clients$ = this.clientService.GetClientList();
  public contacts$ = this.contactService.GetContactList();
  public relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();
  public clientContactChangeHistories$ = this.clientContactChangeHistoryService.GetClientContactChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public clientContactService: ClientContactService,
    public clientService: ClientService,
    public contactService: ContactService,
    public relationshipTypeService: RelationshipTypeService,
    public clientContactChangeHistoryService: ClientContactChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the clientContactId from the route parameters
    this.clientContactId = this.route.snapshot.paramMap.get('clientContactId');

    if (this.clientContactId === 'new' ||
        this.clientContactId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.clientContactData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.clientContactForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.clientContactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Client Contact';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Client Contact';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.clientContactForm.dirty) {
      return confirm('You have unsaved Client Contact changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.clientContactId != null && this.clientContactId !== 'new') {

      const id = parseInt(this.clientContactId, 10);

      if (!isNaN(id)) {
        return { clientContactId: id };
      }
    }

    return null;
  }


/*
  * Loads the ClientContact data for the current clientContactId.
  *
  * Fully respects the ClientContactService caching strategy and error handling strategy.
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
    if (!this.clientContactService.userIsSchedulerClientContactReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ClientContacts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate clientContactId
    //
    if (!this.clientContactId) {

      this.alertService.showMessage('No ClientContact ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const clientContactId = Number(this.clientContactId);

    if (isNaN(clientContactId) || clientContactId <= 0) {

      this.alertService.showMessage(`Invalid Client Contact ID: "${this.clientContactId}"`,
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
      // This is the most targeted way: clear only this ClientContact + relations

      this.clientContactService.ClearRecordCache(clientContactId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.clientContactService.GetClientContact(clientContactId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (clientContactData) => {

        //
        // Success path — clientContactData can legitimately be null if 404'd but request succeeded
        //
        if (!clientContactData) {

          this.handleClientContactNotFound(clientContactId);

        } else {

          this.clientContactData = clientContactData;
          this.buildFormValues(this.clientContactData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ClientContact loaded successfully',
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
        this.handleClientContactLoadError(error, clientContactId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleClientContactNotFound(clientContactId: number): void {

    this.clientContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ClientContact #${clientContactId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleClientContactLoadError(error: any, clientContactId: number): void {

    let message = 'Failed to load Client Contact.';
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
          message = 'You do not have permission to view this Client Contact.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Client Contact #${clientContactId} was not found.`;
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

    console.error(`Client Contact load failed (ID: ${clientContactId})`, error);

    //
    // Reset UI to safe state
    //
    this.clientContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(clientContactData: ClientContactData | null) {

    if (clientContactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.clientContactForm.reset({
        clientId: null,
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
        this.clientContactForm.reset({
        clientId: clientContactData.clientId,
        contactId: clientContactData.contactId,
        isPrimary: clientContactData.isPrimary ?? false,
        relationshipTypeId: clientContactData.relationshipTypeId,
        versionNumber: clientContactData.versionNumber?.toString() ?? '',
        active: clientContactData.active ?? true,
        deleted: clientContactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.clientContactForm.markAsPristine();
    this.clientContactForm.markAsUntouched();
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

    if (this.clientContactService.userIsSchedulerClientContactWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Client Contacts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.clientContactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.clientContactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.clientContactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const clientContactSubmitData: ClientContactSubmitData = {
        id: this.clientContactData?.id || 0,
        clientId: Number(formValue.clientId),
        contactId: Number(formValue.contactId),
        isPrimary: !!formValue.isPrimary,
        relationshipTypeId: Number(formValue.relationshipTypeId),
        versionNumber: this.clientContactData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.clientContactService.PutClientContact(clientContactSubmitData.id, clientContactSubmitData)
      : this.clientContactService.PostClientContact(clientContactSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedClientContactData) => {

        this.clientContactService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Client Contact's detail page
          //
          this.clientContactForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.clientContactForm.markAsUntouched();

          this.router.navigate(['/clientcontacts', savedClientContactData.id]);
          this.alertService.showMessage('Client Contact added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.clientContactData = savedClientContactData;
          this.buildFormValues(this.clientContactData);

          this.alertService.showMessage("Client Contact saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Client Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Client Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Client Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerClientContactReader(): boolean {
    return this.clientContactService.userIsSchedulerClientContactReader();
  }

  public userIsSchedulerClientContactWriter(): boolean {
    return this.clientContactService.userIsSchedulerClientContactWriter();
  }
}

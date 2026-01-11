/*
   GENERATED FORM FOR THE RESOURCECONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceContact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceContactService, ResourceContactData, ResourceContactSubmitData } from '../../../scheduler-data-services/resource-contact.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { RelationshipTypeService } from '../../../scheduler-data-services/relationship-type.service';
import { ResourceContactChangeHistoryService } from '../../../scheduler-data-services/resource-contact-change-history.service';
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
interface ResourceContactFormValues {
  resourceId: number | bigint,       // For FK link number
  contactId: number | bigint,       // For FK link number
  isPrimary: boolean,
  relationshipTypeId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-resource-contact-detail',
  templateUrl: './resource-contact-detail.component.html',
  styleUrls: ['./resource-contact-detail.component.scss']
})

export class ResourceContactDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceContactForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        contactId: [null, Validators.required],
        isPrimary: [false],
        relationshipTypeId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public resourceContactId: string | null = null;
  public resourceContactData: ResourceContactData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  resourceContacts$ = this.resourceContactService.GetResourceContactList();
  public resources$ = this.resourceService.GetResourceList();
  public contacts$ = this.contactService.GetContactList();
  public relationshipTypes$ = this.relationshipTypeService.GetRelationshipTypeList();
  public resourceContactChangeHistories$ = this.resourceContactChangeHistoryService.GetResourceContactChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public resourceContactService: ResourceContactService,
    public resourceService: ResourceService,
    public contactService: ContactService,
    public relationshipTypeService: RelationshipTypeService,
    public resourceContactChangeHistoryService: ResourceContactChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the resourceContactId from the route parameters
    this.resourceContactId = this.route.snapshot.paramMap.get('resourceContactId');

    if (this.resourceContactId === 'new' ||
        this.resourceContactId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.resourceContactData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.resourceContactForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceContactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Resource Contact';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Resource Contact';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.resourceContactForm.dirty) {
      return confirm('You have unsaved Resource Contact changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.resourceContactId != null && this.resourceContactId !== 'new') {

      const id = parseInt(this.resourceContactId, 10);

      if (!isNaN(id)) {
        return { resourceContactId: id };
      }
    }

    return null;
  }


/*
  * Loads the ResourceContact data for the current resourceContactId.
  *
  * Fully respects the ResourceContactService caching strategy and error handling strategy.
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
    if (!this.resourceContactService.userIsSchedulerResourceContactReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ResourceContacts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate resourceContactId
    //
    if (!this.resourceContactId) {

      this.alertService.showMessage('No ResourceContact ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const resourceContactId = Number(this.resourceContactId);

    if (isNaN(resourceContactId) || resourceContactId <= 0) {

      this.alertService.showMessage(`Invalid Resource Contact ID: "${this.resourceContactId}"`,
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
      // This is the most targeted way: clear only this ResourceContact + relations

      this.resourceContactService.ClearRecordCache(resourceContactId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.resourceContactService.GetResourceContact(resourceContactId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (resourceContactData) => {

        //
        // Success path — resourceContactData can legitimately be null if 404'd but request succeeded
        //
        if (!resourceContactData) {

          this.handleResourceContactNotFound(resourceContactId);

        } else {

          this.resourceContactData = resourceContactData;
          this.buildFormValues(this.resourceContactData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ResourceContact loaded successfully',
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
        this.handleResourceContactLoadError(error, resourceContactId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleResourceContactNotFound(resourceContactId: number): void {

    this.resourceContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ResourceContact #${resourceContactId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleResourceContactLoadError(error: any, resourceContactId: number): void {

    let message = 'Failed to load Resource Contact.';
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
          message = 'You do not have permission to view this Resource Contact.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Resource Contact #${resourceContactId} was not found.`;
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

    console.error(`Resource Contact load failed (ID: ${resourceContactId})`, error);

    //
    // Reset UI to safe state
    //
    this.resourceContactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(resourceContactData: ResourceContactData | null) {

    if (resourceContactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceContactForm.reset({
        resourceId: null,
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
        this.resourceContactForm.reset({
        resourceId: resourceContactData.resourceId,
        contactId: resourceContactData.contactId,
        isPrimary: resourceContactData.isPrimary ?? false,
        relationshipTypeId: resourceContactData.relationshipTypeId,
        versionNumber: resourceContactData.versionNumber?.toString() ?? '',
        active: resourceContactData.active ?? true,
        deleted: resourceContactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceContactForm.markAsPristine();
    this.resourceContactForm.markAsUntouched();
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

    if (this.resourceContactService.userIsSchedulerResourceContactWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Resource Contacts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.resourceContactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceContactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceContactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceContactSubmitData: ResourceContactSubmitData = {
        id: this.resourceContactData?.id || 0,
        resourceId: Number(formValue.resourceId),
        contactId: Number(formValue.contactId),
        isPrimary: !!formValue.isPrimary,
        relationshipTypeId: Number(formValue.relationshipTypeId),
        versionNumber: this.resourceContactData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.resourceContactService.PutResourceContact(resourceContactSubmitData.id, resourceContactSubmitData)
      : this.resourceContactService.PostResourceContact(resourceContactSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedResourceContactData) => {

        this.resourceContactService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Resource Contact's detail page
          //
          this.resourceContactForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.resourceContactForm.markAsUntouched();

          this.router.navigate(['/resourcecontacts', savedResourceContactData.id]);
          this.alertService.showMessage('Resource Contact added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.resourceContactData = savedResourceContactData;
          this.buildFormValues(this.resourceContactData);

          this.alertService.showMessage("Resource Contact saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Resource Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerResourceContactReader(): boolean {
    return this.resourceContactService.userIsSchedulerResourceContactReader();
  }

  public userIsSchedulerResourceContactWriter(): boolean {
    return this.resourceContactService.userIsSchedulerResourceContactWriter();
  }
}

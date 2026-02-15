/*
   GENERATED FORM FOR THE CONTACT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Contact table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactService, ContactData, ContactSubmitData } from '../../../scheduler-data-services/contact.service';
import { ContactTypeService } from '../../../scheduler-data-services/contact-type.service';
import { SalutationService } from '../../../scheduler-data-services/salutation.service';
import { ContactMethodService } from '../../../scheduler-data-services/contact-method.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ContactChangeHistoryService } from '../../../scheduler-data-services/contact-change-history.service';
import { ContactTagService } from '../../../scheduler-data-services/contact-tag.service';
import { ContactContactService } from '../../../scheduler-data-services/contact-contact.service';
import { OfficeContactService } from '../../../scheduler-data-services/office-contact.service';
import { ClientContactService } from '../../../scheduler-data-services/client-contact.service';
import { SchedulingTargetContactService } from '../../../scheduler-data-services/scheduling-target-contact.service';
import { ResourceContactService } from '../../../scheduler-data-services/resource-contact.service';
import { ContactInteractionService } from '../../../scheduler-data-services/contact-interaction.service';
import { NotificationSubscriptionService } from '../../../scheduler-data-services/notification-subscription.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
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
interface ContactFormValues {
  contactTypeId: number | bigint,       // For FK link number
  firstName: string,
  middleName: string | null,
  lastName: string,
  salutationId: number | bigint | null,       // For FK link number
  title: string | null,
  birthDate: string | null,
  company: string | null,
  email: string | null,
  phone: string | null,
  mobile: string | null,
  position: string | null,
  webSite: string | null,
  contactMethodId: number | bigint | null,       // For FK link number
  notes: string | null,
  timeZoneId: number | bigint | null,       // For FK link number
  attributes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  externalId: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-contact-detail',
  templateUrl: './contact-detail.component.html',
  styleUrls: ['./contact-detail.component.scss']
})

export class ContactDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactForm: FormGroup = this.fb.group({
        contactTypeId: [null, Validators.required],
        firstName: ['', Validators.required],
        middleName: [''],
        lastName: ['', Validators.required],
        salutationId: [null],
        title: [''],
        birthDate: [''],
        company: [''],
        email: [''],
        phone: [''],
        mobile: [''],
        position: [''],
        webSite: [''],
        contactMethodId: [null],
        notes: [''],
        timeZoneId: [null],
        attributes: [''],
        iconId: [null],
        color: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        externalId: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public contactId: string | null = null;
  public contactData: ContactData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  contacts$ = this.contactService.GetContactList();
  public contactTypes$ = this.contactTypeService.GetContactTypeList();
  public salutations$ = this.salutationService.GetSalutationList();
  public contactMethods$ = this.contactMethodService.GetContactMethodList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public icons$ = this.iconService.GetIconList();
  public contactChangeHistories$ = this.contactChangeHistoryService.GetContactChangeHistoryList();
  public contactTags$ = this.contactTagService.GetContactTagList();
  public contactContacts$ = this.contactContactService.GetContactContactList();
  public officeContacts$ = this.officeContactService.GetOfficeContactList();
  public clientContacts$ = this.clientContactService.GetClientContactList();
  public schedulingTargetContacts$ = this.schedulingTargetContactService.GetSchedulingTargetContactList();
  public resourceContacts$ = this.resourceContactService.GetResourceContactList();
  public contactInteractions$ = this.contactInteractionService.GetContactInteractionList();
  public notificationSubscriptions$ = this.notificationSubscriptionService.GetNotificationSubscriptionList();
  public constituents$ = this.constituentService.GetConstituentList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public contactService: ContactService,
    public contactTypeService: ContactTypeService,
    public salutationService: SalutationService,
    public contactMethodService: ContactMethodService,
    public timeZoneService: TimeZoneService,
    public iconService: IconService,
    public contactChangeHistoryService: ContactChangeHistoryService,
    public contactTagService: ContactTagService,
    public contactContactService: ContactContactService,
    public officeContactService: OfficeContactService,
    public clientContactService: ClientContactService,
    public schedulingTargetContactService: SchedulingTargetContactService,
    public resourceContactService: ResourceContactService,
    public contactInteractionService: ContactInteractionService,
    public notificationSubscriptionService: NotificationSubscriptionService,
    public constituentService: ConstituentService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the contactId from the route parameters
    this.contactId = this.route.snapshot.paramMap.get('contactId');

    if (this.contactId === 'new' ||
        this.contactId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contactData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.contactForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Contact';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Contact';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.contactForm.dirty) {
      return confirm('You have unsaved Contact changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.contactId != null && this.contactId !== 'new') {

      const id = parseInt(this.contactId, 10);

      if (!isNaN(id)) {
        return { contactId: id };
      }
    }

    return null;
  }


/*
  * Loads the Contact data for the current contactId.
  *
  * Fully respects the ContactService caching strategy and error handling strategy.
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
    if (!this.contactService.userIsSchedulerContactReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Contacts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contactId
    //
    if (!this.contactId) {

      this.alertService.showMessage('No Contact ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contactId = Number(this.contactId);

    if (isNaN(contactId) || contactId <= 0) {

      this.alertService.showMessage(`Invalid Contact ID: "${this.contactId}"`,
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
      // This is the most targeted way: clear only this Contact + relations

      this.contactService.ClearRecordCache(contactId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contactService.GetContact(contactId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contactData) => {

        //
        // Success path — contactData can legitimately be null if 404'd but request succeeded
        //
        if (!contactData) {

          this.handleContactNotFound(contactId);

        } else {

          this.contactData = contactData;
          this.buildFormValues(this.contactData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Contact loaded successfully',
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
        this.handleContactLoadError(error, contactId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleContactNotFound(contactId: number): void {

    this.contactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Contact #${contactId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContactLoadError(error: any, contactId: number): void {

    let message = 'Failed to load Contact.';
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
          message = 'You do not have permission to view this Contact.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Contact #${contactId} was not found.`;
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

    console.error(`Contact load failed (ID: ${contactId})`, error);

    //
    // Reset UI to safe state
    //
    this.contactData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(contactData: ContactData | null) {

    if (contactData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactForm.reset({
        contactTypeId: null,
        firstName: '',
        middleName: '',
        lastName: '',
        salutationId: null,
        title: '',
        birthDate: '',
        company: '',
        email: '',
        phone: '',
        mobile: '',
        position: '',
        webSite: '',
        contactMethodId: null,
        notes: '',
        timeZoneId: null,
        attributes: '',
        iconId: null,
        color: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        externalId: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactForm.reset({
        contactTypeId: contactData.contactTypeId,
        firstName: contactData.firstName ?? '',
        middleName: contactData.middleName ?? '',
        lastName: contactData.lastName ?? '',
        salutationId: contactData.salutationId,
        title: contactData.title ?? '',
        birthDate: contactData.birthDate ?? '',
        company: contactData.company ?? '',
        email: contactData.email ?? '',
        phone: contactData.phone ?? '',
        mobile: contactData.mobile ?? '',
        position: contactData.position ?? '',
        webSite: contactData.webSite ?? '',
        contactMethodId: contactData.contactMethodId,
        notes: contactData.notes ?? '',
        timeZoneId: contactData.timeZoneId,
        attributes: contactData.attributes ?? '',
        iconId: contactData.iconId,
        color: contactData.color ?? '',
        avatarFileName: contactData.avatarFileName ?? '',
        avatarSize: contactData.avatarSize?.toString() ?? '',
        avatarData: contactData.avatarData ?? '',
        avatarMimeType: contactData.avatarMimeType ?? '',
        externalId: contactData.externalId ?? '',
        versionNumber: contactData.versionNumber?.toString() ?? '',
        active: contactData.active ?? true,
        deleted: contactData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactForm.markAsPristine();
    this.contactForm.markAsUntouched();
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

    if (this.contactService.userIsSchedulerContactWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Contacts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.contactForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactSubmitData: ContactSubmitData = {
        id: this.contactData?.id || 0,
        contactTypeId: Number(formValue.contactTypeId),
        firstName: formValue.firstName!.trim(),
        middleName: formValue.middleName?.trim() || null,
        lastName: formValue.lastName!.trim(),
        salutationId: formValue.salutationId ? Number(formValue.salutationId) : null,
        title: formValue.title?.trim() || null,
        birthDate: formValue.birthDate ? formValue.birthDate.trim() : null,
        company: formValue.company?.trim() || null,
        email: formValue.email?.trim() || null,
        phone: formValue.phone?.trim() || null,
        mobile: formValue.mobile?.trim() || null,
        position: formValue.position?.trim() || null,
        webSite: formValue.webSite?.trim() || null,
        contactMethodId: formValue.contactMethodId ? Number(formValue.contactMethodId) : null,
        notes: formValue.notes?.trim() || null,
        timeZoneId: formValue.timeZoneId ? Number(formValue.timeZoneId) : null,
        attributes: formValue.attributes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        versionNumber: this.contactData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.contactService.PutContact(contactSubmitData.id, contactSubmitData)
      : this.contactService.PostContact(contactSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedContactData) => {

        this.contactService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Contact's detail page
          //
          this.contactForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.contactForm.markAsUntouched();

          this.router.navigate(['/contacts', savedContactData.id]);
          this.alertService.showMessage('Contact added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.contactData = savedContactData;
          this.buildFormValues(this.contactData);

          this.alertService.showMessage("Contact saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Contact.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerContactReader(): boolean {
    return this.contactService.userIsSchedulerContactReader();
  }

  public userIsSchedulerContactWriter(): boolean {
    return this.contactService.userIsSchedulerContactWriter();
  }
}

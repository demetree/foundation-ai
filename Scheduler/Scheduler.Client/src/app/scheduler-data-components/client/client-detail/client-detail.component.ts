/*
   GENERATED FORM FOR THE CLIENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Client table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to client-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ClientService, ClientData, ClientSubmitData } from '../../../scheduler-data-services/client.service';
import { ClientTypeService } from '../../../scheduler-data-services/client-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { ClientChangeHistoryService } from '../../../scheduler-data-services/client-change-history.service';
import { ClientContactService } from '../../../scheduler-data-services/client-contact.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { SchedulingTargetAddressService } from '../../../scheduler-data-services/scheduling-target-address.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
import { FinancialTransactionService } from '../../../scheduler-data-services/financial-transaction.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
import { ReceiptService } from '../../../scheduler-data-services/receipt.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
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
interface ClientFormValues {
  name: string,
  description: string | null,
  clientTypeId: number | bigint,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  calendarId: number | bigint | null,       // For FK link number
  addressLine1: string,
  addressLine2: string | null,
  city: string,
  postalCode: string | null,
  stateProvinceId: number | bigint,       // For FK link number
  countryId: number | bigint,       // For FK link number
  phone: string | null,
  email: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
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
  selector: 'app-client-detail',
  templateUrl: './client-detail.component.html',
  styleUrls: ['./client-detail.component.scss']
})

export class ClientDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ClientFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public clientForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        clientTypeId: [null, Validators.required],
        currencyId: [null, Validators.required],
        timeZoneId: [null, Validators.required],
        calendarId: [null],
        addressLine1: ['', Validators.required],
        addressLine2: [''],
        city: ['', Validators.required],
        postalCode: [''],
        stateProvinceId: [null, Validators.required],
        countryId: [null, Validators.required],
        phone: [''],
        email: [''],
        latitude: [''],
        longitude: [''],
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


  public clientId: string | null = null;
  public clientData: ClientData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  clients$ = this.clientService.GetClientList();
  public clientTypes$ = this.clientTypeService.GetClientTypeList();
  public currencies$ = this.currencyService.GetCurrencyList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();
  public calendars$ = this.calendarService.GetCalendarList();
  public stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  public countries$ = this.countryService.GetCountryList();
  public clientChangeHistories$ = this.clientChangeHistoryService.GetClientChangeHistoryList();
  public clientContacts$ = this.clientContactService.GetClientContactList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public schedulingTargetAddresses$ = this.schedulingTargetAddressService.GetSchedulingTargetAddressList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();
  public financialTransactions$ = this.financialTransactionService.GetFinancialTransactionList();
  public invoices$ = this.invoiceService.GetInvoiceList();
  public receipts$ = this.receiptService.GetReceiptList();
  public constituents$ = this.constituentService.GetConstituentList();

  private destroy$ = new Subject<void>();

  constructor(
    public clientService: ClientService,
    public clientTypeService: ClientTypeService,
    public currencyService: CurrencyService,
    public timeZoneService: TimeZoneService,
    public calendarService: CalendarService,
    public stateProvinceService: StateProvinceService,
    public countryService: CountryService,
    public clientChangeHistoryService: ClientChangeHistoryService,
    public clientContactService: ClientContactService,
    public schedulingTargetService: SchedulingTargetService,
    public schedulingTargetAddressService: SchedulingTargetAddressService,
    public scheduledEventService: ScheduledEventService,
    public financialTransactionService: FinancialTransactionService,
    public invoiceService: InvoiceService,
    public receiptService: ReceiptService,
    public constituentService: ConstituentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the clientId from the route parameters
    this.clientId = this.route.snapshot.paramMap.get('clientId');

    if (this.clientId === 'new' ||
        this.clientId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.clientData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.clientForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.clientForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Client';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Client';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.clientForm.dirty) {
      return confirm('You have unsaved Client changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.clientId != null && this.clientId !== 'new') {

      const id = parseInt(this.clientId, 10);

      if (!isNaN(id)) {
        return { clientId: id };
      }
    }

    return null;
  }


/*
  * Loads the Client data for the current clientId.
  *
  * Fully respects the ClientService caching strategy and error handling strategy.
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
    if (!this.clientService.userIsSchedulerClientReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Clients.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate clientId
    //
    if (!this.clientId) {

      this.alertService.showMessage('No Client ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const clientId = Number(this.clientId);

    if (isNaN(clientId) || clientId <= 0) {

      this.alertService.showMessage(`Invalid Client ID: "${this.clientId}"`,
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
      // This is the most targeted way: clear only this Client + relations

      this.clientService.ClearRecordCache(clientId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.clientService.GetClient(clientId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (clientData) => {

        //
        // Success path — clientData can legitimately be null if 404'd but request succeeded
        //
        if (!clientData) {

          this.handleClientNotFound(clientId);

        } else {

          this.clientData = clientData;
          this.buildFormValues(this.clientData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Client loaded successfully',
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
        this.handleClientLoadError(error, clientId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleClientNotFound(clientId: number): void {

    this.clientData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Client #${clientId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleClientLoadError(error: any, clientId: number): void {

    let message = 'Failed to load Client.';
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
          message = 'You do not have permission to view this Client.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Client #${clientId} was not found.`;
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

    console.error(`Client load failed (ID: ${clientId})`, error);

    //
    // Reset UI to safe state
    //
    this.clientData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(clientData: ClientData | null) {

    if (clientData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.clientForm.reset({
        name: '',
        description: '',
        clientTypeId: null,
        currencyId: null,
        timeZoneId: null,
        calendarId: null,
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        phone: '',
        email: '',
        latitude: '',
        longitude: '',
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
        this.clientForm.reset({
        name: clientData.name ?? '',
        description: clientData.description ?? '',
        clientTypeId: clientData.clientTypeId,
        currencyId: clientData.currencyId,
        timeZoneId: clientData.timeZoneId,
        calendarId: clientData.calendarId,
        addressLine1: clientData.addressLine1 ?? '',
        addressLine2: clientData.addressLine2 ?? '',
        city: clientData.city ?? '',
        postalCode: clientData.postalCode ?? '',
        stateProvinceId: clientData.stateProvinceId,
        countryId: clientData.countryId,
        phone: clientData.phone ?? '',
        email: clientData.email ?? '',
        latitude: clientData.latitude?.toString() ?? '',
        longitude: clientData.longitude?.toString() ?? '',
        notes: clientData.notes ?? '',
        externalId: clientData.externalId ?? '',
        color: clientData.color ?? '',
        attributes: clientData.attributes ?? '',
        avatarFileName: clientData.avatarFileName ?? '',
        avatarSize: clientData.avatarSize?.toString() ?? '',
        avatarData: clientData.avatarData ?? '',
        avatarMimeType: clientData.avatarMimeType ?? '',
        versionNumber: clientData.versionNumber?.toString() ?? '',
        active: clientData.active ?? true,
        deleted: clientData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.clientForm.markAsPristine();
    this.clientForm.markAsUntouched();
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

    if (this.clientService.userIsSchedulerClientWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Clients", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.clientForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.clientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.clientForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const clientSubmitData: ClientSubmitData = {
        id: this.clientData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        clientTypeId: Number(formValue.clientTypeId),
        currencyId: Number(formValue.currencyId),
        timeZoneId: Number(formValue.timeZoneId),
        calendarId: formValue.calendarId ? Number(formValue.calendarId) : null,
        addressLine1: formValue.addressLine1!.trim(),
        addressLine2: formValue.addressLine2?.trim() || null,
        city: formValue.city!.trim(),
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: Number(formValue.stateProvinceId),
        countryId: Number(formValue.countryId),
        phone: formValue.phone?.trim() || null,
        email: formValue.email?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.clientData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.clientService.PutClient(clientSubmitData.id, clientSubmitData)
      : this.clientService.PostClient(clientSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedClientData) => {

        this.clientService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Client's detail page
          //
          this.clientForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.clientForm.markAsUntouched();

          this.router.navigate(['/clients', savedClientData.id]);
          this.alertService.showMessage('Client added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.clientData = savedClientData;
          this.buildFormValues(this.clientData);

          this.alertService.showMessage("Client saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Client.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Client.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Client could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerClientReader(): boolean {
    return this.clientService.userIsSchedulerClientReader();
  }

  public userIsSchedulerClientWriter(): boolean {
    return this.clientService.userIsSchedulerClientWriter();
  }
}

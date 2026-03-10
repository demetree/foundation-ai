/*
   GENERATED FORM FOR THE INVOICESTATUS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from InvoiceStatus table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to invoice-status-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { InvoiceStatusService, InvoiceStatusData, InvoiceStatusSubmitData } from '../../../scheduler-data-services/invoice-status.service';
import { InvoiceService } from '../../../scheduler-data-services/invoice.service';
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
interface InvoiceStatusFormValues {
  name: string,
  description: string,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-invoice-status-detail',
  templateUrl: './invoice-status-detail.component.html',
  styleUrls: ['./invoice-status-detail.component.scss']
})

export class InvoiceStatusDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<InvoiceStatusFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public invoiceStatusForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public invoiceStatusId: string | null = null;
  public invoiceStatusData: InvoiceStatusData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  invoiceStatuses$ = this.invoiceStatusService.GetInvoiceStatusList();
  public invoices$ = this.invoiceService.GetInvoiceList();

  private destroy$ = new Subject<void>();

  constructor(
    public invoiceStatusService: InvoiceStatusService,
    public invoiceService: InvoiceService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the invoiceStatusId from the route parameters
    this.invoiceStatusId = this.route.snapshot.paramMap.get('invoiceStatusId');

    if (this.invoiceStatusId === 'new' ||
        this.invoiceStatusId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.invoiceStatusData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.invoiceStatusForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.invoiceStatusForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Invoice Status';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Invoice Status';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.invoiceStatusForm.dirty) {
      return confirm('You have unsaved Invoice Status changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.invoiceStatusId != null && this.invoiceStatusId !== 'new') {

      const id = parseInt(this.invoiceStatusId, 10);

      if (!isNaN(id)) {
        return { invoiceStatusId: id };
      }
    }

    return null;
  }


/*
  * Loads the InvoiceStatus data for the current invoiceStatusId.
  *
  * Fully respects the InvoiceStatusService caching strategy and error handling strategy.
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
    if (!this.invoiceStatusService.userIsSchedulerInvoiceStatusReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read InvoiceStatuses.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate invoiceStatusId
    //
    if (!this.invoiceStatusId) {

      this.alertService.showMessage('No InvoiceStatus ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const invoiceStatusId = Number(this.invoiceStatusId);

    if (isNaN(invoiceStatusId) || invoiceStatusId <= 0) {

      this.alertService.showMessage(`Invalid Invoice Status ID: "${this.invoiceStatusId}"`,
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
      // This is the most targeted way: clear only this InvoiceStatus + relations

      this.invoiceStatusService.ClearRecordCache(invoiceStatusId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.invoiceStatusService.GetInvoiceStatus(invoiceStatusId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (invoiceStatusData) => {

        //
        // Success path — invoiceStatusData can legitimately be null if 404'd but request succeeded
        //
        if (!invoiceStatusData) {

          this.handleInvoiceStatusNotFound(invoiceStatusId);

        } else {

          this.invoiceStatusData = invoiceStatusData;
          this.buildFormValues(this.invoiceStatusData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'InvoiceStatus loaded successfully',
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
        this.handleInvoiceStatusLoadError(error, invoiceStatusId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleInvoiceStatusNotFound(invoiceStatusId: number): void {

    this.invoiceStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `InvoiceStatus #${invoiceStatusId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleInvoiceStatusLoadError(error: any, invoiceStatusId: number): void {

    let message = 'Failed to load Invoice Status.';
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
          message = 'You do not have permission to view this Invoice Status.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Invoice Status #${invoiceStatusId} was not found.`;
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

    console.error(`Invoice Status load failed (ID: ${invoiceStatusId})`, error);

    //
    // Reset UI to safe state
    //
    this.invoiceStatusData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(invoiceStatusData: InvoiceStatusData | null) {

    if (invoiceStatusData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.invoiceStatusForm.reset({
        name: '',
        description: '',
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.invoiceStatusForm.reset({
        name: invoiceStatusData.name ?? '',
        description: invoiceStatusData.description ?? '',
        color: invoiceStatusData.color ?? '',
        sequence: invoiceStatusData.sequence?.toString() ?? '',
        active: invoiceStatusData.active ?? true,
        deleted: invoiceStatusData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.invoiceStatusForm.markAsPristine();
    this.invoiceStatusForm.markAsUntouched();
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

    if (this.invoiceStatusService.userIsSchedulerInvoiceStatusWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Invoice Statuses", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.invoiceStatusForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.invoiceStatusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.invoiceStatusForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const invoiceStatusSubmitData: InvoiceStatusSubmitData = {
        id: this.invoiceStatusData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.invoiceStatusService.PutInvoiceStatus(invoiceStatusSubmitData.id, invoiceStatusSubmitData)
      : this.invoiceStatusService.PostInvoiceStatus(invoiceStatusSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedInvoiceStatusData) => {

        this.invoiceStatusService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Invoice Status's detail page
          //
          this.invoiceStatusForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.invoiceStatusForm.markAsUntouched();

          this.router.navigate(['/invoicestatuses', savedInvoiceStatusData.id]);
          this.alertService.showMessage('Invoice Status added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.invoiceStatusData = savedInvoiceStatusData;
          this.buildFormValues(this.invoiceStatusData);

          this.alertService.showMessage("Invoice Status saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Invoice Status.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Invoice Status.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Invoice Status could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerInvoiceStatusReader(): boolean {
    return this.invoiceStatusService.userIsSchedulerInvoiceStatusReader();
  }

  public userIsSchedulerInvoiceStatusWriter(): boolean {
    return this.invoiceStatusService.userIsSchedulerInvoiceStatusWriter();
  }
}

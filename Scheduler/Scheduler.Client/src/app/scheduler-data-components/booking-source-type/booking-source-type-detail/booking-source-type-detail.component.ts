/*
   GENERATED FORM FOR THE BOOKINGSOURCETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BookingSourceType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to booking-source-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BookingSourceTypeService, BookingSourceTypeData, BookingSourceTypeSubmitData } from '../../../scheduler-data-services/booking-source-type.service';
import { ScheduledEventService } from '../../../scheduler-data-services/scheduled-event.service';
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
interface BookingSourceTypeFormValues {
  name: string,
  description: string,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-booking-source-type-detail',
  templateUrl: './booking-source-type-detail.component.html',
  styleUrls: ['./booking-source-type-detail.component.scss']
})

export class BookingSourceTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BookingSourceTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public bookingSourceTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public bookingSourceTypeId: string | null = null;
  public bookingSourceTypeData: BookingSourceTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  bookingSourceTypes$ = this.bookingSourceTypeService.GetBookingSourceTypeList();
  public scheduledEvents$ = this.scheduledEventService.GetScheduledEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public bookingSourceTypeService: BookingSourceTypeService,
    public scheduledEventService: ScheduledEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the bookingSourceTypeId from the route parameters
    this.bookingSourceTypeId = this.route.snapshot.paramMap.get('bookingSourceTypeId');

    if (this.bookingSourceTypeId === 'new' ||
        this.bookingSourceTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.bookingSourceTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.bookingSourceTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.bookingSourceTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Booking Source Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Booking Source Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.bookingSourceTypeForm.dirty) {
      return confirm('You have unsaved Booking Source Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.bookingSourceTypeId != null && this.bookingSourceTypeId !== 'new') {

      const id = parseInt(this.bookingSourceTypeId, 10);

      if (!isNaN(id)) {
        return { bookingSourceTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the BookingSourceType data for the current bookingSourceTypeId.
  *
  * Fully respects the BookingSourceTypeService caching strategy and error handling strategy.
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
    if (!this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BookingSourceTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate bookingSourceTypeId
    //
    if (!this.bookingSourceTypeId) {

      this.alertService.showMessage('No BookingSourceType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const bookingSourceTypeId = Number(this.bookingSourceTypeId);

    if (isNaN(bookingSourceTypeId) || bookingSourceTypeId <= 0) {

      this.alertService.showMessage(`Invalid Booking Source Type ID: "${this.bookingSourceTypeId}"`,
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
      // This is the most targeted way: clear only this BookingSourceType + relations

      this.bookingSourceTypeService.ClearRecordCache(bookingSourceTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.bookingSourceTypeService.GetBookingSourceType(bookingSourceTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (bookingSourceTypeData) => {

        //
        // Success path — bookingSourceTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!bookingSourceTypeData) {

          this.handleBookingSourceTypeNotFound(bookingSourceTypeId);

        } else {

          this.bookingSourceTypeData = bookingSourceTypeData;
          this.buildFormValues(this.bookingSourceTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BookingSourceType loaded successfully',
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
        this.handleBookingSourceTypeLoadError(error, bookingSourceTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBookingSourceTypeNotFound(bookingSourceTypeId: number): void {

    this.bookingSourceTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BookingSourceType #${bookingSourceTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBookingSourceTypeLoadError(error: any, bookingSourceTypeId: number): void {

    let message = 'Failed to load Booking Source Type.';
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
          message = 'You do not have permission to view this Booking Source Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Booking Source Type #${bookingSourceTypeId} was not found.`;
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

    console.error(`Booking Source Type load failed (ID: ${bookingSourceTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.bookingSourceTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(bookingSourceTypeData: BookingSourceTypeData | null) {

    if (bookingSourceTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.bookingSourceTypeForm.reset({
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
        this.bookingSourceTypeForm.reset({
        name: bookingSourceTypeData.name ?? '',
        description: bookingSourceTypeData.description ?? '',
        color: bookingSourceTypeData.color ?? '',
        sequence: bookingSourceTypeData.sequence?.toString() ?? '',
        active: bookingSourceTypeData.active ?? true,
        deleted: bookingSourceTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.bookingSourceTypeForm.markAsPristine();
    this.bookingSourceTypeForm.markAsUntouched();
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

    if (this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Booking Source Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.bookingSourceTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.bookingSourceTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.bookingSourceTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const bookingSourceTypeSubmitData: BookingSourceTypeSubmitData = {
        id: this.bookingSourceTypeData?.id || 0,
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
      ? this.bookingSourceTypeService.PutBookingSourceType(bookingSourceTypeSubmitData.id, bookingSourceTypeSubmitData)
      : this.bookingSourceTypeService.PostBookingSourceType(bookingSourceTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBookingSourceTypeData) => {

        this.bookingSourceTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Booking Source Type's detail page
          //
          this.bookingSourceTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.bookingSourceTypeForm.markAsUntouched();

          this.router.navigate(['/bookingsourcetypes', savedBookingSourceTypeData.id]);
          this.alertService.showMessage('Booking Source Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.bookingSourceTypeData = savedBookingSourceTypeData;
          this.buildFormValues(this.bookingSourceTypeData);

          this.alertService.showMessage("Booking Source Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Booking Source Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Booking Source Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Booking Source Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerBookingSourceTypeReader(): boolean {
    return this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeReader();
  }

  public userIsSchedulerBookingSourceTypeWriter(): boolean {
    return this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeWriter();
  }
}

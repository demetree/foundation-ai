/*
   GENERATED FORM FOR THE CONTACTMETHOD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactMethod table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-method-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactMethodService, ContactMethodData, ContactMethodSubmitData } from '../../../scheduler-data-services/contact-method.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
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
interface ContactMethodFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-contact-method-detail',
  templateUrl: './contact-method-detail.component.html',
  styleUrls: ['./contact-method-detail.component.scss']
})

export class ContactMethodDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactMethodFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactMethodForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public contactMethodId: string | null = null;
  public contactMethodData: ContactMethodData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  contactMethods$ = this.contactMethodService.GetContactMethodList();
  public icons$ = this.iconService.GetIconList();
  public contacts$ = this.contactService.GetContactList();

  private destroy$ = new Subject<void>();

  constructor(
    public contactMethodService: ContactMethodService,
    public iconService: IconService,
    public contactService: ContactService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the contactMethodId from the route parameters
    this.contactMethodId = this.route.snapshot.paramMap.get('contactMethodId');

    if (this.contactMethodId === 'new' ||
        this.contactMethodId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contactMethodData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.contactMethodForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactMethodForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Contact Method';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Contact Method';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.contactMethodForm.dirty) {
      return confirm('You have unsaved Contact Method changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.contactMethodId != null && this.contactMethodId !== 'new') {

      const id = parseInt(this.contactMethodId, 10);

      if (!isNaN(id)) {
        return { contactMethodId: id };
      }
    }

    return null;
  }


/*
  * Loads the ContactMethod data for the current contactMethodId.
  *
  * Fully respects the ContactMethodService caching strategy and error handling strategy.
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
    if (!this.contactMethodService.userIsSchedulerContactMethodReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ContactMethods.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contactMethodId
    //
    if (!this.contactMethodId) {

      this.alertService.showMessage('No ContactMethod ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contactMethodId = Number(this.contactMethodId);

    if (isNaN(contactMethodId) || contactMethodId <= 0) {

      this.alertService.showMessage(`Invalid Contact Method ID: "${this.contactMethodId}"`,
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
      // This is the most targeted way: clear only this ContactMethod + relations

      this.contactMethodService.ClearRecordCache(contactMethodId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contactMethodService.GetContactMethod(contactMethodId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contactMethodData) => {

        //
        // Success path — contactMethodData can legitimately be null if 404'd but request succeeded
        //
        if (!contactMethodData) {

          this.handleContactMethodNotFound(contactMethodId);

        } else {

          this.contactMethodData = contactMethodData;
          this.buildFormValues(this.contactMethodData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ContactMethod loaded successfully',
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
        this.handleContactMethodLoadError(error, contactMethodId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleContactMethodNotFound(contactMethodId: number): void {

    this.contactMethodData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ContactMethod #${contactMethodId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContactMethodLoadError(error: any, contactMethodId: number): void {

    let message = 'Failed to load Contact Method.';
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
          message = 'You do not have permission to view this Contact Method.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Contact Method #${contactMethodId} was not found.`;
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

    console.error(`Contact Method load failed (ID: ${contactMethodId})`, error);

    //
    // Reset UI to safe state
    //
    this.contactMethodData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(contactMethodData: ContactMethodData | null) {

    if (contactMethodData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactMethodForm.reset({
        name: '',
        description: '',
        sequence: '',
        iconId: null,
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactMethodForm.reset({
        name: contactMethodData.name ?? '',
        description: contactMethodData.description ?? '',
        sequence: contactMethodData.sequence?.toString() ?? '',
        iconId: contactMethodData.iconId,
        color: contactMethodData.color ?? '',
        active: contactMethodData.active ?? true,
        deleted: contactMethodData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactMethodForm.markAsPristine();
    this.contactMethodForm.markAsUntouched();
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

    if (this.contactMethodService.userIsSchedulerContactMethodWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Contact Methods", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.contactMethodForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactMethodForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactMethodForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactMethodSubmitData: ContactMethodSubmitData = {
        id: this.contactMethodData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.contactMethodService.PutContactMethod(contactMethodSubmitData.id, contactMethodSubmitData)
      : this.contactMethodService.PostContactMethod(contactMethodSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedContactMethodData) => {

        this.contactMethodService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Contact Method's detail page
          //
          this.contactMethodForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.contactMethodForm.markAsUntouched();

          this.router.navigate(['/contactmethods', savedContactMethodData.id]);
          this.alertService.showMessage('Contact Method added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.contactMethodData = savedContactMethodData;
          this.buildFormValues(this.contactMethodData);

          this.alertService.showMessage("Contact Method saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Contact Method.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Method.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Method could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerContactMethodReader(): boolean {
    return this.contactMethodService.userIsSchedulerContactMethodReader();
  }

  public userIsSchedulerContactMethodWriter(): boolean {
    return this.contactMethodService.userIsSchedulerContactMethodWriter();
  }
}

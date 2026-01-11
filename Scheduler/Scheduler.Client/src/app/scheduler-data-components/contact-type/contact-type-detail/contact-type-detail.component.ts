/*
   GENERATED FORM FOR THE CONTACTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactTypeService, ContactTypeData, ContactTypeSubmitData } from '../../../scheduler-data-services/contact-type.service';
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
interface ContactTypeFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-contact-type-detail',
  templateUrl: './contact-type-detail.component.html',
  styleUrls: ['./contact-type-detail.component.scss']
})

export class ContactTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });


  public contactTypeId: string | null = null;
  public contactTypeData: ContactTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  contactTypes$ = this.contactTypeService.GetContactTypeList();
  public icons$ = this.iconService.GetIconList();
  public contacts$ = this.contactService.GetContactList();

  private destroy$ = new Subject<void>();

  constructor(
    public contactTypeService: ContactTypeService,
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

    // Get the contactTypeId from the route parameters
    this.contactTypeId = this.route.snapshot.paramMap.get('contactTypeId');

    if (this.contactTypeId === 'new' ||
        this.contactTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contactTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.contactTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Contact Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Contact Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.contactTypeForm.dirty) {
      return confirm('You have unsaved Contact Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.contactTypeId != null && this.contactTypeId !== 'new') {

      const id = parseInt(this.contactTypeId, 10);

      if (!isNaN(id)) {
        return { contactTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the ContactType data for the current contactTypeId.
  *
  * Fully respects the ContactTypeService caching strategy and error handling strategy.
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
    if (!this.contactTypeService.userIsSchedulerContactTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ContactTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contactTypeId
    //
    if (!this.contactTypeId) {

      this.alertService.showMessage('No ContactType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contactTypeId = Number(this.contactTypeId);

    if (isNaN(contactTypeId) || contactTypeId <= 0) {

      this.alertService.showMessage(`Invalid Contact Type ID: "${this.contactTypeId}"`,
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
      // This is the most targeted way: clear only this ContactType + relations

      this.contactTypeService.ClearRecordCache(contactTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contactTypeService.GetContactType(contactTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contactTypeData) => {

        //
        // Success path — contactTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!contactTypeData) {

          this.handleContactTypeNotFound(contactTypeId);

        } else {

          this.contactTypeData = contactTypeData;
          this.buildFormValues(this.contactTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ContactType loaded successfully',
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
        this.handleContactTypeLoadError(error, contactTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleContactTypeNotFound(contactTypeId: number): void {

    this.contactTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ContactType #${contactTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContactTypeLoadError(error: any, contactTypeId: number): void {

    let message = 'Failed to load Contact Type.';
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
          message = 'You do not have permission to view this Contact Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Contact Type #${contactTypeId} was not found.`;
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

    console.error(`Contact Type load failed (ID: ${contactTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.contactTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(contactTypeData: ContactTypeData | null) {

    if (contactTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactTypeForm.reset({
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
        this.contactTypeForm.reset({
        name: contactTypeData.name ?? '',
        description: contactTypeData.description ?? '',
        sequence: contactTypeData.sequence?.toString() ?? '',
        iconId: contactTypeData.iconId,
        color: contactTypeData.color ?? '',
        active: contactTypeData.active ?? true,
        deleted: contactTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactTypeForm.markAsPristine();
    this.contactTypeForm.markAsUntouched();
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

    if (this.contactTypeService.userIsSchedulerContactTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Contact Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.contactTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactTypeSubmitData: ContactTypeSubmitData = {
        id: this.contactTypeData?.id || 0,
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
      ? this.contactTypeService.PutContactType(contactTypeSubmitData.id, contactTypeSubmitData)
      : this.contactTypeService.PostContactType(contactTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedContactTypeData) => {

        this.contactTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Contact Type's detail page
          //
          this.contactTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.contactTypeForm.markAsUntouched();

          this.router.navigate(['/contacttypes', savedContactTypeData.id]);
          this.alertService.showMessage('Contact Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.contactTypeData = savedContactTypeData;
          this.buildFormValues(this.contactTypeData);

          this.alertService.showMessage("Contact Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Contact Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerContactTypeReader(): boolean {
    return this.contactTypeService.userIsSchedulerContactTypeReader();
  }

  public userIsSchedulerContactTypeWriter(): boolean {
    return this.contactTypeService.userIsSchedulerContactTypeWriter();
  }
}

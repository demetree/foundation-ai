/*
   GENERATED FORM FOR THE CONTACTTAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactTag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-tag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactTagService, ContactTagData, ContactTagSubmitData } from '../../../scheduler-data-services/contact-tag.service';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { TagService } from '../../../scheduler-data-services/tag.service';
import { ContactTagChangeHistoryService } from '../../../scheduler-data-services/contact-tag-change-history.service';
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
interface ContactTagFormValues {
  contactId: number | bigint,       // For FK link number
  tagId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-contact-tag-detail',
  templateUrl: './contact-tag-detail.component.html',
  styleUrls: ['./contact-tag-detail.component.scss']
})

export class ContactTagDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactTagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactTagForm: FormGroup = this.fb.group({
        contactId: [null, Validators.required],
        tagId: [null, Validators.required],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public contactTagId: string | null = null;
  public contactTagData: ContactTagData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  contactTags$ = this.contactTagService.GetContactTagList();
  public contacts$ = this.contactService.GetContactList();
  public tags$ = this.tagService.GetTagList();
  public contactTagChangeHistories$ = this.contactTagChangeHistoryService.GetContactTagChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public contactTagService: ContactTagService,
    public contactService: ContactService,
    public tagService: TagService,
    public contactTagChangeHistoryService: ContactTagChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the contactTagId from the route parameters
    this.contactTagId = this.route.snapshot.paramMap.get('contactTagId');

    if (this.contactTagId === 'new' ||
        this.contactTagId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contactTagData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.contactTagForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactTagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Contact Tag';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Contact Tag';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.contactTagForm.dirty) {
      return confirm('You have unsaved Contact Tag changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.contactTagId != null && this.contactTagId !== 'new') {

      const id = parseInt(this.contactTagId, 10);

      if (!isNaN(id)) {
        return { contactTagId: id };
      }
    }

    return null;
  }


/*
  * Loads the ContactTag data for the current contactTagId.
  *
  * Fully respects the ContactTagService caching strategy and error handling strategy.
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
    if (!this.contactTagService.userIsSchedulerContactTagReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ContactTags.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contactTagId
    //
    if (!this.contactTagId) {

      this.alertService.showMessage('No ContactTag ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contactTagId = Number(this.contactTagId);

    if (isNaN(contactTagId) || contactTagId <= 0) {

      this.alertService.showMessage(`Invalid Contact Tag ID: "${this.contactTagId}"`,
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
      // This is the most targeted way: clear only this ContactTag + relations

      this.contactTagService.ClearRecordCache(contactTagId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contactTagService.GetContactTag(contactTagId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contactTagData) => {

        //
        // Success path — contactTagData can legitimately be null if 404'd but request succeeded
        //
        if (!contactTagData) {

          this.handleContactTagNotFound(contactTagId);

        } else {

          this.contactTagData = contactTagData;
          this.buildFormValues(this.contactTagData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ContactTag loaded successfully',
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
        this.handleContactTagLoadError(error, contactTagId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleContactTagNotFound(contactTagId: number): void {

    this.contactTagData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ContactTag #${contactTagId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContactTagLoadError(error: any, contactTagId: number): void {

    let message = 'Failed to load Contact Tag.';
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
          message = 'You do not have permission to view this Contact Tag.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Contact Tag #${contactTagId} was not found.`;
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

    console.error(`Contact Tag load failed (ID: ${contactTagId})`, error);

    //
    // Reset UI to safe state
    //
    this.contactTagData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(contactTagData: ContactTagData | null) {

    if (contactTagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactTagForm.reset({
        contactId: null,
        tagId: null,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactTagForm.reset({
        contactId: contactTagData.contactId,
        tagId: contactTagData.tagId,
        versionNumber: contactTagData.versionNumber?.toString() ?? '',
        active: contactTagData.active ?? true,
        deleted: contactTagData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.contactTagForm.markAsPristine();
    this.contactTagForm.markAsUntouched();
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

    if (this.contactTagService.userIsSchedulerContactTagWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Contact Tags", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.contactTagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactTagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactTagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactTagSubmitData: ContactTagSubmitData = {
        id: this.contactTagData?.id || 0,
        contactId: Number(formValue.contactId),
        tagId: Number(formValue.tagId),
        versionNumber: this.contactTagData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.contactTagService.PutContactTag(contactTagSubmitData.id, contactTagSubmitData)
      : this.contactTagService.PostContactTag(contactTagSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedContactTagData) => {

        this.contactTagService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Contact Tag's detail page
          //
          this.contactTagForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.contactTagForm.markAsUntouched();

          this.router.navigate(['/contacttags', savedContactTagData.id]);
          this.alertService.showMessage('Contact Tag added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.contactTagData = savedContactTagData;
          this.buildFormValues(this.contactTagData);

          this.alertService.showMessage("Contact Tag saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Contact Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerContactTagReader(): boolean {
    return this.contactTagService.userIsSchedulerContactTagReader();
  }

  public userIsSchedulerContactTagWriter(): boolean {
    return this.contactTagService.userIsSchedulerContactTagWriter();
  }
}

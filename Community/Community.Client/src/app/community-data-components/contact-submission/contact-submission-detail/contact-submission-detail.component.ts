/*
   GENERATED FORM FOR THE CONTACTSUBMISSION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ContactSubmission table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to contact-submission-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactSubmissionService, ContactSubmissionData, ContactSubmissionSubmitData } from '../../../community-data-services/contact-submission.service';
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
interface ContactSubmissionFormValues {
};


@Component({
  selector: 'app-contact-submission-detail',
  templateUrl: './contact-submission-detail.component.html',
  styleUrls: ['./contact-submission-detail.component.scss']
})

export class ContactSubmissionDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ContactSubmissionFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public contactSubmissionForm: FormGroup = this.fb.group({
      });


  public contactSubmissionId: string | null = null;
  public contactSubmissionData: ContactSubmissionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  contactSubmissions$ = this.contactSubmissionService.GetContactSubmissionList();

  private destroy$ = new Subject<void>();

  constructor(
    public contactSubmissionService: ContactSubmissionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the contactSubmissionId from the route parameters
    this.contactSubmissionId = this.route.snapshot.paramMap.get('contactSubmissionId');

    if (this.contactSubmissionId === 'new' ||
        this.contactSubmissionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contactSubmissionData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.contactSubmissionForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.contactSubmissionForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Contact Submission';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Contact Submission';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.contactSubmissionForm.dirty) {
      return confirm('You have unsaved Contact Submission changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.contactSubmissionId != null && this.contactSubmissionId !== 'new') {

      const id = parseInt(this.contactSubmissionId, 10);

      if (!isNaN(id)) {
        return { contactSubmissionId: id };
      }
    }

    return null;
  }


/*
  * Loads the ContactSubmission data for the current contactSubmissionId.
  *
  * Fully respects the ContactSubmissionService caching strategy and error handling strategy.
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
    if (!this.contactSubmissionService.userIsCommunityContactSubmissionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read ContactSubmissions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contactSubmissionId
    //
    if (!this.contactSubmissionId) {

      this.alertService.showMessage('No ContactSubmission ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contactSubmissionId = Number(this.contactSubmissionId);

    if (isNaN(contactSubmissionId) || contactSubmissionId <= 0) {

      this.alertService.showMessage(`Invalid Contact Submission ID: "${this.contactSubmissionId}"`,
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
      // This is the most targeted way: clear only this ContactSubmission + relations

      this.contactSubmissionService.ClearRecordCache(contactSubmissionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contactSubmissionService.GetContactSubmission(contactSubmissionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contactSubmissionData) => {

        //
        // Success path — contactSubmissionData can legitimately be null if 404'd but request succeeded
        //
        if (!contactSubmissionData) {

          this.handleContactSubmissionNotFound(contactSubmissionId);

        } else {

          this.contactSubmissionData = contactSubmissionData;
          this.buildFormValues(this.contactSubmissionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'ContactSubmission loaded successfully',
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
        this.handleContactSubmissionLoadError(error, contactSubmissionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleContactSubmissionNotFound(contactSubmissionId: number): void {

    this.contactSubmissionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `ContactSubmission #${contactSubmissionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContactSubmissionLoadError(error: any, contactSubmissionId: number): void {

    let message = 'Failed to load Contact Submission.';
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
          message = 'You do not have permission to view this Contact Submission.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Contact Submission #${contactSubmissionId} was not found.`;
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

    console.error(`Contact Submission load failed (ID: ${contactSubmissionId})`, error);

    //
    // Reset UI to safe state
    //
    this.contactSubmissionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(contactSubmissionData: ContactSubmissionData | null) {

    if (contactSubmissionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.contactSubmissionForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.contactSubmissionForm.reset({
      }, { emitEvent: false});
    }

    this.contactSubmissionForm.markAsPristine();
    this.contactSubmissionForm.markAsUntouched();
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

    if (this.contactSubmissionService.userIsCommunityContactSubmissionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Contact Submissions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.contactSubmissionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.contactSubmissionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.contactSubmissionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const contactSubmissionSubmitData: ContactSubmissionSubmitData = {
        id: this.contactSubmissionData?.id || 0,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.contactSubmissionService.PutContactSubmission(contactSubmissionSubmitData.id, contactSubmissionSubmitData)
      : this.contactSubmissionService.PostContactSubmission(contactSubmissionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedContactSubmissionData) => {

        this.contactSubmissionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Contact Submission's detail page
          //
          this.contactSubmissionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.contactSubmissionForm.markAsUntouched();

          this.router.navigate(['/contactsubmissions', savedContactSubmissionData.id]);
          this.alertService.showMessage('Contact Submission added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.contactSubmissionData = savedContactSubmissionData;
          this.buildFormValues(this.contactSubmissionData);

          this.alertService.showMessage("Contact Submission saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Contact Submission.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Contact Submission.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Contact Submission could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityContactSubmissionReader(): boolean {
    return this.contactSubmissionService.userIsCommunityContactSubmissionReader();
  }

  public userIsCommunityContactSubmissionWriter(): boolean {
    return this.contactSubmissionService.userIsCommunityContactSubmissionWriter();
  }
}

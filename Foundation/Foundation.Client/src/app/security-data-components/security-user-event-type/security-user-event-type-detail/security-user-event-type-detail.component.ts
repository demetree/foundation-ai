/*
   GENERATED FORM FOR THE SECURITYUSEREVENTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityUserEventType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-user-event-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserEventTypeService, SecurityUserEventTypeData, SecurityUserEventTypeSubmitData } from '../../../security-data-services/security-user-event-type.service';
import { SecurityUserEventService } from '../../../security-data-services/security-user-event.service';
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
interface SecurityUserEventTypeFormValues {
  name: string,
  description: string | null,
};


@Component({
  selector: 'app-security-user-event-type-detail',
  templateUrl: './security-user-event-type-detail.component.html',
  styleUrls: ['./security-user-event-type-detail.component.scss']
})

export class SecurityUserEventTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityUserEventTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityUserEventTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });


  public securityUserEventTypeId: string | null = null;
  public securityUserEventTypeData: SecurityUserEventTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityUserEventTypes$ = this.securityUserEventTypeService.GetSecurityUserEventTypeList();
  public securityUserEvents$ = this.securityUserEventService.GetSecurityUserEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityUserEventTypeService: SecurityUserEventTypeService,
    public securityUserEventService: SecurityUserEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityUserEventTypeId from the route parameters
    this.securityUserEventTypeId = this.route.snapshot.paramMap.get('securityUserEventTypeId');

    if (this.securityUserEventTypeId === 'new' ||
        this.securityUserEventTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityUserEventTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityUserEventTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityUserEventTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security User Event Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security User Event Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityUserEventTypeForm.dirty) {
      return confirm('You have unsaved Security User Event Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityUserEventTypeId != null && this.securityUserEventTypeId !== 'new') {

      const id = parseInt(this.securityUserEventTypeId, 10);

      if (!isNaN(id)) {
        return { securityUserEventTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityUserEventType data for the current securityUserEventTypeId.
  *
  * Fully respects the SecurityUserEventTypeService caching strategy and error handling strategy.
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
    if (!this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityUserEventTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityUserEventTypeId
    //
    if (!this.securityUserEventTypeId) {

      this.alertService.showMessage('No SecurityUserEventType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityUserEventTypeId = Number(this.securityUserEventTypeId);

    if (isNaN(securityUserEventTypeId) || securityUserEventTypeId <= 0) {

      this.alertService.showMessage(`Invalid Security User Event Type ID: "${this.securityUserEventTypeId}"`,
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
      // This is the most targeted way: clear only this SecurityUserEventType + relations

      this.securityUserEventTypeService.ClearRecordCache(securityUserEventTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityUserEventTypeService.GetSecurityUserEventType(securityUserEventTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityUserEventTypeData) => {

        //
        // Success path — securityUserEventTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!securityUserEventTypeData) {

          this.handleSecurityUserEventTypeNotFound(securityUserEventTypeId);

        } else {

          this.securityUserEventTypeData = securityUserEventTypeData;
          this.buildFormValues(this.securityUserEventTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityUserEventType loaded successfully',
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
        this.handleSecurityUserEventTypeLoadError(error, securityUserEventTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityUserEventTypeNotFound(securityUserEventTypeId: number): void {

    this.securityUserEventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityUserEventType #${securityUserEventTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityUserEventTypeLoadError(error: any, securityUserEventTypeId: number): void {

    let message = 'Failed to load Security User Event Type.';
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
          message = 'You do not have permission to view this Security User Event Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security User Event Type #${securityUserEventTypeId} was not found.`;
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

    console.error(`Security User Event Type load failed (ID: ${securityUserEventTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityUserEventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityUserEventTypeData: SecurityUserEventTypeData | null) {

    if (securityUserEventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserEventTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserEventTypeForm.reset({
        name: securityUserEventTypeData.name ?? '',
        description: securityUserEventTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.securityUserEventTypeForm.markAsPristine();
    this.securityUserEventTypeForm.markAsUntouched();
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

    if (this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security User Event Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityUserEventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserEventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserEventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserEventTypeSubmitData: SecurityUserEventTypeSubmitData = {
        id: this.securityUserEventTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityUserEventTypeService.PutSecurityUserEventType(securityUserEventTypeSubmitData.id, securityUserEventTypeSubmitData)
      : this.securityUserEventTypeService.PostSecurityUserEventType(securityUserEventTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityUserEventTypeData) => {

        this.securityUserEventTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security User Event Type's detail page
          //
          this.securityUserEventTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityUserEventTypeForm.markAsUntouched();

          this.router.navigate(['/securityusereventtypes', savedSecurityUserEventTypeData.id]);
          this.alertService.showMessage('Security User Event Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityUserEventTypeData = savedSecurityUserEventTypeData;
          this.buildFormValues(this.securityUserEventTypeData);

          this.alertService.showMessage("Security User Event Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security User Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityUserEventTypeReader(): boolean {
    return this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeReader();
  }

  public userIsSecuritySecurityUserEventTypeWriter(): boolean {
    return this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeWriter();
  }
}

/*
   GENERATED FORM FOR THE AUDITUSERAGENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditUserAgent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-user-agent-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditUserAgentService, AuditUserAgentData, AuditUserAgentSubmitData } from '../../../auditor-data-services/audit-user-agent.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
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
interface AuditUserAgentFormValues {
  name: string,
  comments: string | null,
  firstAccess: string | null,
};


@Component({
  selector: 'app-audit-user-agent-detail',
  templateUrl: './audit-user-agent-detail.component.html',
  styleUrls: ['./audit-user-agent-detail.component.scss']
})

export class AuditUserAgentDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditUserAgentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditUserAgentForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditUserAgentId: string | null = null;
  public auditUserAgentData: AuditUserAgentData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditUserAgents$ = this.auditUserAgentService.GetAuditUserAgentList();
  public auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditUserAgentService: AuditUserAgentService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditUserAgentId from the route parameters
    this.auditUserAgentId = this.route.snapshot.paramMap.get('auditUserAgentId');

    if (this.auditUserAgentId === 'new' ||
        this.auditUserAgentId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditUserAgentData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.auditUserAgentForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditUserAgentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit User Agent';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit User Agent';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditUserAgentForm.dirty) {
      return confirm('You have unsaved Audit User Agent changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditUserAgentId != null && this.auditUserAgentId !== 'new') {

      const id = parseInt(this.auditUserAgentId, 10);

      if (!isNaN(id)) {
        return { auditUserAgentId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditUserAgent data for the current auditUserAgentId.
  *
  * Fully respects the AuditUserAgentService caching strategy and error handling strategy.
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
    if (!this.auditUserAgentService.userIsAuditorAuditUserAgentReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditUserAgents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditUserAgentId
    //
    if (!this.auditUserAgentId) {

      this.alertService.showMessage('No AuditUserAgent ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditUserAgentId = Number(this.auditUserAgentId);

    if (isNaN(auditUserAgentId) || auditUserAgentId <= 0) {

      this.alertService.showMessage(`Invalid Audit User Agent ID: "${this.auditUserAgentId}"`,
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
      // This is the most targeted way: clear only this AuditUserAgent + relations

      this.auditUserAgentService.ClearRecordCache(auditUserAgentId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditUserAgentService.GetAuditUserAgent(auditUserAgentId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditUserAgentData) => {

        //
        // Success path — auditUserAgentData can legitimately be null if 404'd but request succeeded
        //
        if (!auditUserAgentData) {

          this.handleAuditUserAgentNotFound(auditUserAgentId);

        } else {

          this.auditUserAgentData = auditUserAgentData;
          this.buildFormValues(this.auditUserAgentData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditUserAgent loaded successfully',
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
        this.handleAuditUserAgentLoadError(error, auditUserAgentId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditUserAgentNotFound(auditUserAgentId: number): void {

    this.auditUserAgentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditUserAgent #${auditUserAgentId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditUserAgentLoadError(error: any, auditUserAgentId: number): void {

    let message = 'Failed to load Audit User Agent.';
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
          message = 'You do not have permission to view this Audit User Agent.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit User Agent #${auditUserAgentId} was not found.`;
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

    console.error(`Audit User Agent load failed (ID: ${auditUserAgentId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditUserAgentData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditUserAgentData: AuditUserAgentData | null) {

    if (auditUserAgentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditUserAgentForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditUserAgentForm.reset({
        name: auditUserAgentData.name ?? '',
        comments: auditUserAgentData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditUserAgentData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditUserAgentForm.markAsPristine();
    this.auditUserAgentForm.markAsUntouched();
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

    if (this.auditUserAgentService.userIsAuditorAuditUserAgentWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit User Agents", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditUserAgentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditUserAgentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditUserAgentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditUserAgentSubmitData: AuditUserAgentSubmitData = {
        id: this.auditUserAgentData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditUserAgentService.PutAuditUserAgent(auditUserAgentSubmitData.id, auditUserAgentSubmitData)
      : this.auditUserAgentService.PostAuditUserAgent(auditUserAgentSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditUserAgentData) => {

        this.auditUserAgentService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit User Agent's detail page
          //
          this.auditUserAgentForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditUserAgentForm.markAsUntouched();

          this.router.navigate(['/audituseragents', savedAuditUserAgentData.id]);
          this.alertService.showMessage('Audit User Agent added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditUserAgentData = savedAuditUserAgentData;
          this.buildFormValues(this.auditUserAgentData);

          this.alertService.showMessage("Audit User Agent saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit User Agent.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit User Agent.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit User Agent could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditUserAgentReader(): boolean {
    return this.auditUserAgentService.userIsAuditorAuditUserAgentReader();
  }

  public userIsAuditorAuditUserAgentWriter(): boolean {
    return this.auditUserAgentService.userIsAuditorAuditUserAgentWriter();
  }
}

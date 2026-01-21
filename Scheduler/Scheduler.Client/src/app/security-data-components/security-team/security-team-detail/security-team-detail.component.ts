/*
   GENERATED FORM FOR THE SECURITYTEAM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SecurityTeam table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to security-team-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityTeamService, SecurityTeamData, SecurityTeamSubmitData } from '../../../security-data-services/security-team.service';
import { SecurityDepartmentService } from '../../../security-data-services/security-department.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityTeamUserService } from '../../../security-data-services/security-team-user.service';
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
interface SecurityTeamFormValues {
  securityDepartmentId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-security-team-detail',
  templateUrl: './security-team-detail.component.html',
  styleUrls: ['./security-team-detail.component.scss']
})

export class SecurityTeamDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SecurityTeamFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public securityTeamForm: FormGroup = this.fb.group({
        securityDepartmentId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public securityTeamId: string | null = null;
  public securityTeamData: SecurityTeamData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityTeams$ = this.securityTeamService.GetSecurityTeamList();
  public securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  public securityUsers$ = this.securityUserService.GetSecurityUserList();
  public securityTeamUsers$ = this.securityTeamUserService.GetSecurityTeamUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityTeamService: SecurityTeamService,
    public securityDepartmentService: SecurityDepartmentService,
    public securityUserService: SecurityUserService,
    public securityTeamUserService: SecurityTeamUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityTeamId from the route parameters
    this.securityTeamId = this.route.snapshot.paramMap.get('securityTeamId');

    if (this.securityTeamId === 'new' ||
        this.securityTeamId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityTeamData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.securityTeamForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.securityTeamForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security Team';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security Team';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityTeamForm.dirty) {
      return confirm('You have unsaved Security Team changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityTeamId != null && this.securityTeamId !== 'new') {

      const id = parseInt(this.securityTeamId, 10);

      if (!isNaN(id)) {
        return { securityTeamId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityTeam data for the current securityTeamId.
  *
  * Fully respects the SecurityTeamService caching strategy and error handling strategy.
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
    if (!this.securityTeamService.userIsSecuritySecurityTeamReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityTeams.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityTeamId
    //
    if (!this.securityTeamId) {

      this.alertService.showMessage('No SecurityTeam ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityTeamId = Number(this.securityTeamId);

    if (isNaN(securityTeamId) || securityTeamId <= 0) {

      this.alertService.showMessage(`Invalid Security Team ID: "${this.securityTeamId}"`,
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
      // This is the most targeted way: clear only this SecurityTeam + relations

      this.securityTeamService.ClearRecordCache(securityTeamId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityTeamService.GetSecurityTeam(securityTeamId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityTeamData) => {

        //
        // Success path — securityTeamData can legitimately be null if 404'd but request succeeded
        //
        if (!securityTeamData) {

          this.handleSecurityTeamNotFound(securityTeamId);

        } else {

          this.securityTeamData = securityTeamData;
          this.buildFormValues(this.securityTeamData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityTeam loaded successfully',
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
        this.handleSecurityTeamLoadError(error, securityTeamId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityTeamNotFound(securityTeamId: number): void {

    this.securityTeamData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityTeam #${securityTeamId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityTeamLoadError(error: any, securityTeamId: number): void {

    let message = 'Failed to load Security Team.';
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
          message = 'You do not have permission to view this Security Team.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security Team #${securityTeamId} was not found.`;
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

    console.error(`Security Team load failed (ID: ${securityTeamId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityTeamData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityTeamData: SecurityTeamData | null) {

    if (securityTeamData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityTeamForm.reset({
        securityDepartmentId: null,
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityTeamForm.reset({
        securityDepartmentId: securityTeamData.securityDepartmentId,
        name: securityTeamData.name ?? '',
        description: securityTeamData.description ?? '',
        active: securityTeamData.active ?? true,
        deleted: securityTeamData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityTeamForm.markAsPristine();
    this.securityTeamForm.markAsUntouched();
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

    if (this.securityTeamService.userIsSecuritySecurityTeamWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security Teams", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityTeamForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityTeamForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityTeamForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityTeamSubmitData: SecurityTeamSubmitData = {
        id: this.securityTeamData?.id || 0,
        securityDepartmentId: Number(formValue.securityDepartmentId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityTeamService.PutSecurityTeam(securityTeamSubmitData.id, securityTeamSubmitData)
      : this.securityTeamService.PostSecurityTeam(securityTeamSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityTeamData) => {

        this.securityTeamService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security Team's detail page
          //
          this.securityTeamForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityTeamForm.markAsUntouched();

          this.router.navigate(['/securityteams', savedSecurityTeamData.id]);
          this.alertService.showMessage('Security Team added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityTeamData = savedSecurityTeamData;
          this.buildFormValues(this.securityTeamData);

          this.alertService.showMessage("Security Team saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Security Team.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Team.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Team could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityTeamReader(): boolean {
    return this.securityTeamService.userIsSecuritySecurityTeamReader();
  }

  public userIsSecuritySecurityTeamWriter(): boolean {
    return this.securityTeamService.userIsSecuritySecurityTeamWriter();
  }
}

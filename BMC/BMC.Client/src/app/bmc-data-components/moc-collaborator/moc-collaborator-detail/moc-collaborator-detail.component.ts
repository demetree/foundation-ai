/*
   GENERATED FORM FOR THE MOCCOLLABORATOR TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocCollaborator table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-collaborator-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocCollaboratorService, MocCollaboratorData, MocCollaboratorSubmitData } from '../../../bmc-data-services/moc-collaborator.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
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
interface MocCollaboratorFormValues {
  publishedMocId: number | bigint,       // For FK link number
  collaboratorTenantGuid: string,
  accessLevel: string,
  invitedDate: string,
  acceptedDate: string | null,
  isAccepted: boolean,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-moc-collaborator-detail',
  templateUrl: './moc-collaborator-detail.component.html',
  styleUrls: ['./moc-collaborator-detail.component.scss']
})

export class MocCollaboratorDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocCollaboratorFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocCollaboratorForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        collaboratorTenantGuid: ['', Validators.required],
        accessLevel: ['', Validators.required],
        invitedDate: ['', Validators.required],
        acceptedDate: [''],
        isAccepted: [false],
        active: [true],
        deleted: [false],
      });


  public mocCollaboratorId: string | null = null;
  public mocCollaboratorData: MocCollaboratorData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  mocCollaborators$ = this.mocCollaboratorService.GetMocCollaboratorList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  private destroy$ = new Subject<void>();

  constructor(
    public mocCollaboratorService: MocCollaboratorService,
    public publishedMocService: PublishedMocService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the mocCollaboratorId from the route parameters
    this.mocCollaboratorId = this.route.snapshot.paramMap.get('mocCollaboratorId');

    if (this.mocCollaboratorId === 'new' ||
        this.mocCollaboratorId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.mocCollaboratorData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.mocCollaboratorForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocCollaboratorForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Moc Collaborator';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Moc Collaborator';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.mocCollaboratorForm.dirty) {
      return confirm('You have unsaved Moc Collaborator changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.mocCollaboratorId != null && this.mocCollaboratorId !== 'new') {

      const id = parseInt(this.mocCollaboratorId, 10);

      if (!isNaN(id)) {
        return { mocCollaboratorId: id };
      }
    }

    return null;
  }


/*
  * Loads the MocCollaborator data for the current mocCollaboratorId.
  *
  * Fully respects the MocCollaboratorService caching strategy and error handling strategy.
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
    if (!this.mocCollaboratorService.userIsBMCMocCollaboratorReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MocCollaborators.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate mocCollaboratorId
    //
    if (!this.mocCollaboratorId) {

      this.alertService.showMessage('No MocCollaborator ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const mocCollaboratorId = Number(this.mocCollaboratorId);

    if (isNaN(mocCollaboratorId) || mocCollaboratorId <= 0) {

      this.alertService.showMessage(`Invalid Moc Collaborator ID: "${this.mocCollaboratorId}"`,
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
      // This is the most targeted way: clear only this MocCollaborator + relations

      this.mocCollaboratorService.ClearRecordCache(mocCollaboratorId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.mocCollaboratorService.GetMocCollaborator(mocCollaboratorId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (mocCollaboratorData) => {

        //
        // Success path — mocCollaboratorData can legitimately be null if 404'd but request succeeded
        //
        if (!mocCollaboratorData) {

          this.handleMocCollaboratorNotFound(mocCollaboratorId);

        } else {

          this.mocCollaboratorData = mocCollaboratorData;
          this.buildFormValues(this.mocCollaboratorData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MocCollaborator loaded successfully',
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
        this.handleMocCollaboratorLoadError(error, mocCollaboratorId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMocCollaboratorNotFound(mocCollaboratorId: number): void {

    this.mocCollaboratorData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MocCollaborator #${mocCollaboratorId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMocCollaboratorLoadError(error: any, mocCollaboratorId: number): void {

    let message = 'Failed to load Moc Collaborator.';
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
          message = 'You do not have permission to view this Moc Collaborator.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Moc Collaborator #${mocCollaboratorId} was not found.`;
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

    console.error(`Moc Collaborator load failed (ID: ${mocCollaboratorId})`, error);

    //
    // Reset UI to safe state
    //
    this.mocCollaboratorData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(mocCollaboratorData: MocCollaboratorData | null) {

    if (mocCollaboratorData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocCollaboratorForm.reset({
        publishedMocId: null,
        collaboratorTenantGuid: '',
        accessLevel: '',
        invitedDate: '',
        acceptedDate: '',
        isAccepted: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocCollaboratorForm.reset({
        publishedMocId: mocCollaboratorData.publishedMocId,
        collaboratorTenantGuid: mocCollaboratorData.collaboratorTenantGuid ?? '',
        accessLevel: mocCollaboratorData.accessLevel ?? '',
        invitedDate: isoUtcStringToDateTimeLocal(mocCollaboratorData.invitedDate) ?? '',
        acceptedDate: isoUtcStringToDateTimeLocal(mocCollaboratorData.acceptedDate) ?? '',
        isAccepted: mocCollaboratorData.isAccepted ?? false,
        active: mocCollaboratorData.active ?? true,
        deleted: mocCollaboratorData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocCollaboratorForm.markAsPristine();
    this.mocCollaboratorForm.markAsUntouched();
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

    if (this.mocCollaboratorService.userIsBMCMocCollaboratorWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Moc Collaborators", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.mocCollaboratorForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocCollaboratorForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocCollaboratorForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocCollaboratorSubmitData: MocCollaboratorSubmitData = {
        id: this.mocCollaboratorData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        collaboratorTenantGuid: formValue.collaboratorTenantGuid!.trim(),
        accessLevel: formValue.accessLevel!.trim(),
        invitedDate: dateTimeLocalToIsoUtc(formValue.invitedDate!.trim())!,
        acceptedDate: formValue.acceptedDate ? dateTimeLocalToIsoUtc(formValue.acceptedDate.trim()) : null,
        isAccepted: !!formValue.isAccepted,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.mocCollaboratorService.PutMocCollaborator(mocCollaboratorSubmitData.id, mocCollaboratorSubmitData)
      : this.mocCollaboratorService.PostMocCollaborator(mocCollaboratorSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMocCollaboratorData) => {

        this.mocCollaboratorService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Moc Collaborator's detail page
          //
          this.mocCollaboratorForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.mocCollaboratorForm.markAsUntouched();

          this.router.navigate(['/moccollaborators', savedMocCollaboratorData.id]);
          this.alertService.showMessage('Moc Collaborator added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.mocCollaboratorData = savedMocCollaboratorData;
          this.buildFormValues(this.mocCollaboratorData);

          this.alertService.showMessage("Moc Collaborator saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Moc Collaborator.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Collaborator.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Collaborator could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCMocCollaboratorReader(): boolean {
    return this.mocCollaboratorService.userIsBMCMocCollaboratorReader();
  }

  public userIsBMCMocCollaboratorWriter(): boolean {
    return this.mocCollaboratorService.userIsBMCMocCollaboratorWriter();
  }
}

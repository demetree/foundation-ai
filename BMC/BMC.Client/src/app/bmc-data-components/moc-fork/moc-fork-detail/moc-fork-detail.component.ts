/*
   GENERATED FORM FOR THE MOCFORK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocFork table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-fork-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocForkService, MocForkData, MocForkSubmitData } from '../../../bmc-data-services/moc-fork.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { MocVersionService } from '../../../bmc-data-services/moc-version.service';
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
interface MocForkFormValues {
  forkedMocId: number | bigint,       // For FK link number
  sourceMocId: number | bigint,       // For FK link number
  mocVersionId: number | bigint | null,       // For FK link number
  forkerTenantGuid: string,
  forkedDate: string,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-moc-fork-detail',
  templateUrl: './moc-fork-detail.component.html',
  styleUrls: ['./moc-fork-detail.component.scss']
})

export class MocForkDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocForkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocForkForm: FormGroup = this.fb.group({
        forkedMocId: [null, Validators.required],
        sourceMocId: [null, Validators.required],
        mocVersionId: [null],
        forkerTenantGuid: ['', Validators.required],
        forkedDate: ['', Validators.required],
        active: [true],
        deleted: [false],
      });


  public mocForkId: string | null = null;
  public mocForkData: MocForkData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  mocForks$ = this.mocForkService.GetMocForkList();
  public publishedMocs$ = this.publishedMocService.GetPublishedMocList();
  public mocVersions$ = this.mocVersionService.GetMocVersionList();

  private destroy$ = new Subject<void>();

  constructor(
    public mocForkService: MocForkService,
    public publishedMocService: PublishedMocService,
    public mocVersionService: MocVersionService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the mocForkId from the route parameters
    this.mocForkId = this.route.snapshot.paramMap.get('mocForkId');

    if (this.mocForkId === 'new' ||
        this.mocForkId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.mocForkData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.mocForkForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocForkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Moc Fork';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Moc Fork';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.mocForkForm.dirty) {
      return confirm('You have unsaved Moc Fork changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.mocForkId != null && this.mocForkId !== 'new') {

      const id = parseInt(this.mocForkId, 10);

      if (!isNaN(id)) {
        return { mocForkId: id };
      }
    }

    return null;
  }


/*
  * Loads the MocFork data for the current mocForkId.
  *
  * Fully respects the MocForkService caching strategy and error handling strategy.
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
    if (!this.mocForkService.userIsBMCMocForkReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MocForks.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate mocForkId
    //
    if (!this.mocForkId) {

      this.alertService.showMessage('No MocFork ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const mocForkId = Number(this.mocForkId);

    if (isNaN(mocForkId) || mocForkId <= 0) {

      this.alertService.showMessage(`Invalid Moc Fork ID: "${this.mocForkId}"`,
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
      // This is the most targeted way: clear only this MocFork + relations

      this.mocForkService.ClearRecordCache(mocForkId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.mocForkService.GetMocFork(mocForkId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (mocForkData) => {

        //
        // Success path — mocForkData can legitimately be null if 404'd but request succeeded
        //
        if (!mocForkData) {

          this.handleMocForkNotFound(mocForkId);

        } else {

          this.mocForkData = mocForkData;
          this.buildFormValues(this.mocForkData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MocFork loaded successfully',
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
        this.handleMocForkLoadError(error, mocForkId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMocForkNotFound(mocForkId: number): void {

    this.mocForkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MocFork #${mocForkId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMocForkLoadError(error: any, mocForkId: number): void {

    let message = 'Failed to load Moc Fork.';
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
          message = 'You do not have permission to view this Moc Fork.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Moc Fork #${mocForkId} was not found.`;
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

    console.error(`Moc Fork load failed (ID: ${mocForkId})`, error);

    //
    // Reset UI to safe state
    //
    this.mocForkData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(mocForkData: MocForkData | null) {

    if (mocForkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocForkForm.reset({
        forkedMocId: null,
        sourceMocId: null,
        mocVersionId: null,
        forkerTenantGuid: '',
        forkedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocForkForm.reset({
        forkedMocId: mocForkData.forkedMocId,
        sourceMocId: mocForkData.sourceMocId,
        mocVersionId: mocForkData.mocVersionId,
        forkerTenantGuid: mocForkData.forkerTenantGuid ?? '',
        forkedDate: isoUtcStringToDateTimeLocal(mocForkData.forkedDate) ?? '',
        active: mocForkData.active ?? true,
        deleted: mocForkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocForkForm.markAsPristine();
    this.mocForkForm.markAsUntouched();
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

    if (this.mocForkService.userIsBMCMocForkWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Moc Forks", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.mocForkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocForkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocForkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocForkSubmitData: MocForkSubmitData = {
        id: this.mocForkData?.id || 0,
        forkedMocId: Number(formValue.forkedMocId),
        sourceMocId: Number(formValue.sourceMocId),
        mocVersionId: formValue.mocVersionId ? Number(formValue.mocVersionId) : null,
        forkerTenantGuid: formValue.forkerTenantGuid!.trim(),
        forkedDate: dateTimeLocalToIsoUtc(formValue.forkedDate!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.mocForkService.PutMocFork(mocForkSubmitData.id, mocForkSubmitData)
      : this.mocForkService.PostMocFork(mocForkSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMocForkData) => {

        this.mocForkService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Moc Fork's detail page
          //
          this.mocForkForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.mocForkForm.markAsUntouched();

          this.router.navigate(['/mocforks', savedMocForkData.id]);
          this.alertService.showMessage('Moc Fork added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.mocForkData = savedMocForkData;
          this.buildFormValues(this.mocForkData);

          this.alertService.showMessage("Moc Fork saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Moc Fork.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Fork.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Fork could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCMocForkReader(): boolean {
    return this.mocForkService.userIsBMCMocForkReader();
  }

  public userIsBMCMocForkWriter(): boolean {
    return this.mocForkService.userIsBMCMocForkWriter();
  }
}

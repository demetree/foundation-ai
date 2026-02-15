/*
   GENERATED FORM FOR THE LEGOMINIFIG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoMinifig table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-minifig-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoMinifigService, LegoMinifigData, LegoMinifigSubmitData } from '../../../bmc-data-services/lego-minifig.service';
import { LegoSetMinifigService } from '../../../bmc-data-services/lego-set-minifig.service';
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
interface LegoMinifigFormValues {
  name: string,
  figNumber: string,
  partCount: string,     // Stored as string for form input, converted to number on submit.
  imageUrl: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-lego-minifig-detail',
  templateUrl: './lego-minifig-detail.component.html',
  styleUrls: ['./lego-minifig-detail.component.scss']
})

export class LegoMinifigDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoMinifigFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoMinifigForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        figNumber: ['', Validators.required],
        partCount: ['', Validators.required],
        imageUrl: [''],
        active: [true],
        deleted: [false],
      });


  public legoMinifigId: string | null = null;
  public legoMinifigData: LegoMinifigData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  legoMinifigs$ = this.legoMinifigService.GetLegoMinifigList();
  public legoSetMinifigs$ = this.legoSetMinifigService.GetLegoSetMinifigList();

  private destroy$ = new Subject<void>();

  constructor(
    public legoMinifigService: LegoMinifigService,
    public legoSetMinifigService: LegoSetMinifigService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the legoMinifigId from the route parameters
    this.legoMinifigId = this.route.snapshot.paramMap.get('legoMinifigId');

    if (this.legoMinifigId === 'new' ||
        this.legoMinifigId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.legoMinifigData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.legoMinifigForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoMinifigForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Lego Minifig';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Lego Minifig';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.legoMinifigForm.dirty) {
      return confirm('You have unsaved Lego Minifig changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.legoMinifigId != null && this.legoMinifigId !== 'new') {

      const id = parseInt(this.legoMinifigId, 10);

      if (!isNaN(id)) {
        return { legoMinifigId: id };
      }
    }

    return null;
  }


/*
  * Loads the LegoMinifig data for the current legoMinifigId.
  *
  * Fully respects the LegoMinifigService caching strategy and error handling strategy.
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
    if (!this.legoMinifigService.userIsBMCLegoMinifigReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read LegoMinifigs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate legoMinifigId
    //
    if (!this.legoMinifigId) {

      this.alertService.showMessage('No LegoMinifig ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const legoMinifigId = Number(this.legoMinifigId);

    if (isNaN(legoMinifigId) || legoMinifigId <= 0) {

      this.alertService.showMessage(`Invalid Lego Minifig ID: "${this.legoMinifigId}"`,
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
      // This is the most targeted way: clear only this LegoMinifig + relations

      this.legoMinifigService.ClearRecordCache(legoMinifigId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.legoMinifigService.GetLegoMinifig(legoMinifigId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (legoMinifigData) => {

        //
        // Success path — legoMinifigData can legitimately be null if 404'd but request succeeded
        //
        if (!legoMinifigData) {

          this.handleLegoMinifigNotFound(legoMinifigId);

        } else {

          this.legoMinifigData = legoMinifigData;
          this.buildFormValues(this.legoMinifigData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'LegoMinifig loaded successfully',
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
        this.handleLegoMinifigLoadError(error, legoMinifigId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleLegoMinifigNotFound(legoMinifigId: number): void {

    this.legoMinifigData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `LegoMinifig #${legoMinifigId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleLegoMinifigLoadError(error: any, legoMinifigId: number): void {

    let message = 'Failed to load Lego Minifig.';
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
          message = 'You do not have permission to view this Lego Minifig.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Lego Minifig #${legoMinifigId} was not found.`;
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

    console.error(`Lego Minifig load failed (ID: ${legoMinifigId})`, error);

    //
    // Reset UI to safe state
    //
    this.legoMinifigData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(legoMinifigData: LegoMinifigData | null) {

    if (legoMinifigData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoMinifigForm.reset({
        name: '',
        figNumber: '',
        partCount: '',
        imageUrl: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoMinifigForm.reset({
        name: legoMinifigData.name ?? '',
        figNumber: legoMinifigData.figNumber ?? '',
        partCount: legoMinifigData.partCount?.toString() ?? '',
        imageUrl: legoMinifigData.imageUrl ?? '',
        active: legoMinifigData.active ?? true,
        deleted: legoMinifigData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoMinifigForm.markAsPristine();
    this.legoMinifigForm.markAsUntouched();
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

    if (this.legoMinifigService.userIsBMCLegoMinifigWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Lego Minifigs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.legoMinifigForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoMinifigForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoMinifigForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoMinifigSubmitData: LegoMinifigSubmitData = {
        id: this.legoMinifigData?.id || 0,
        name: formValue.name!.trim(),
        figNumber: formValue.figNumber!.trim(),
        partCount: Number(formValue.partCount),
        imageUrl: formValue.imageUrl?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.legoMinifigService.PutLegoMinifig(legoMinifigSubmitData.id, legoMinifigSubmitData)
      : this.legoMinifigService.PostLegoMinifig(legoMinifigSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedLegoMinifigData) => {

        this.legoMinifigService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Lego Minifig's detail page
          //
          this.legoMinifigForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.legoMinifigForm.markAsUntouched();

          this.router.navigate(['/legominifigs', savedLegoMinifigData.id]);
          this.alertService.showMessage('Lego Minifig added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.legoMinifigData = savedLegoMinifigData;
          this.buildFormValues(this.legoMinifigData);

          this.alertService.showMessage("Lego Minifig saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Lego Minifig.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Minifig.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Minifig could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCLegoMinifigReader(): boolean {
    return this.legoMinifigService.userIsBMCLegoMinifigReader();
  }

  public userIsBMCLegoMinifigWriter(): boolean {
    return this.legoMinifigService.userIsBMCLegoMinifigWriter();
  }
}

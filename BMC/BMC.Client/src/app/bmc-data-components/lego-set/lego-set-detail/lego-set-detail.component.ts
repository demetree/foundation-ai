/*
   GENERATED FORM FOR THE LEGOSET TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from LegoSet table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to lego-set-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { LegoSetService, LegoSetData, LegoSetSubmitData } from '../../../bmc-data-services/lego-set.service';
import { LegoThemeService } from '../../../bmc-data-services/lego-theme.service';
import { LegoSetPartService } from '../../../bmc-data-services/lego-set-part.service';
import { UserCollectionSetImportService } from '../../../bmc-data-services/user-collection-set-import.service';
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
interface LegoSetFormValues {
  name: string,
  setNumber: string,
  year: string,     // Stored as string for form input, converted to number on submit.
  partCount: string,     // Stored as string for form input, converted to number on submit.
  legoThemeId: number | bigint | null,       // For FK link number
  imageUrl: string | null,
  brickLinkUrl: string | null,
  rebrickableUrl: string | null,
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-lego-set-detail',
  templateUrl: './lego-set-detail.component.html',
  styleUrls: ['./lego-set-detail.component.scss']
})

export class LegoSetDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<LegoSetFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public legoSetForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        setNumber: ['', Validators.required],
        year: ['', Validators.required],
        partCount: ['', Validators.required],
        legoThemeId: [null],
        imageUrl: [''],
        brickLinkUrl: [''],
        rebrickableUrl: [''],
        active: [true],
        deleted: [false],
      });


  public legoSetId: string | null = null;
  public legoSetData: LegoSetData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  legoSets$ = this.legoSetService.GetLegoSetList();
  public legoThemes$ = this.legoThemeService.GetLegoThemeList();
  public legoSetParts$ = this.legoSetPartService.GetLegoSetPartList();
  public userCollectionSetImports$ = this.userCollectionSetImportService.GetUserCollectionSetImportList();

  private destroy$ = new Subject<void>();

  constructor(
    public legoSetService: LegoSetService,
    public legoThemeService: LegoThemeService,
    public legoSetPartService: LegoSetPartService,
    public userCollectionSetImportService: UserCollectionSetImportService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the legoSetId from the route parameters
    this.legoSetId = this.route.snapshot.paramMap.get('legoSetId');

    if (this.legoSetId === 'new' ||
        this.legoSetId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.legoSetData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.legoSetForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.legoSetForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Lego Set';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Lego Set';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.legoSetForm.dirty) {
      return confirm('You have unsaved Lego Set changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.legoSetId != null && this.legoSetId !== 'new') {

      const id = parseInt(this.legoSetId, 10);

      if (!isNaN(id)) {
        return { legoSetId: id };
      }
    }

    return null;
  }


/*
  * Loads the LegoSet data for the current legoSetId.
  *
  * Fully respects the LegoSetService caching strategy and error handling strategy.
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
    if (!this.legoSetService.userIsBMCLegoSetReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read LegoSets.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate legoSetId
    //
    if (!this.legoSetId) {

      this.alertService.showMessage('No LegoSet ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const legoSetId = Number(this.legoSetId);

    if (isNaN(legoSetId) || legoSetId <= 0) {

      this.alertService.showMessage(`Invalid Lego Set ID: "${this.legoSetId}"`,
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
      // This is the most targeted way: clear only this LegoSet + relations

      this.legoSetService.ClearRecordCache(legoSetId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.legoSetService.GetLegoSet(legoSetId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (legoSetData) => {

        //
        // Success path — legoSetData can legitimately be null if 404'd but request succeeded
        //
        if (!legoSetData) {

          this.handleLegoSetNotFound(legoSetId);

        } else {

          this.legoSetData = legoSetData;
          this.buildFormValues(this.legoSetData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'LegoSet loaded successfully',
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
        this.handleLegoSetLoadError(error, legoSetId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleLegoSetNotFound(legoSetId: number): void {

    this.legoSetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `LegoSet #${legoSetId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleLegoSetLoadError(error: any, legoSetId: number): void {

    let message = 'Failed to load Lego Set.';
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
          message = 'You do not have permission to view this Lego Set.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Lego Set #${legoSetId} was not found.`;
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

    console.error(`Lego Set load failed (ID: ${legoSetId})`, error);

    //
    // Reset UI to safe state
    //
    this.legoSetData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(legoSetData: LegoSetData | null) {

    if (legoSetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.legoSetForm.reset({
        name: '',
        setNumber: '',
        year: '',
        partCount: '',
        legoThemeId: null,
        imageUrl: '',
        brickLinkUrl: '',
        rebrickableUrl: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.legoSetForm.reset({
        name: legoSetData.name ?? '',
        setNumber: legoSetData.setNumber ?? '',
        year: legoSetData.year?.toString() ?? '',
        partCount: legoSetData.partCount?.toString() ?? '',
        legoThemeId: legoSetData.legoThemeId,
        imageUrl: legoSetData.imageUrl ?? '',
        brickLinkUrl: legoSetData.brickLinkUrl ?? '',
        rebrickableUrl: legoSetData.rebrickableUrl ?? '',
        active: legoSetData.active ?? true,
        deleted: legoSetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.legoSetForm.markAsPristine();
    this.legoSetForm.markAsUntouched();
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

    if (this.legoSetService.userIsBMCLegoSetWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Lego Sets", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.legoSetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.legoSetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.legoSetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const legoSetSubmitData: LegoSetSubmitData = {
        id: this.legoSetData?.id || 0,
        name: formValue.name!.trim(),
        setNumber: formValue.setNumber!.trim(),
        year: Number(formValue.year),
        partCount: Number(formValue.partCount),
        legoThemeId: formValue.legoThemeId ? Number(formValue.legoThemeId) : null,
        imageUrl: formValue.imageUrl?.trim() || null,
        brickLinkUrl: formValue.brickLinkUrl?.trim() || null,
        rebrickableUrl: formValue.rebrickableUrl?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.legoSetService.PutLegoSet(legoSetSubmitData.id, legoSetSubmitData)
      : this.legoSetService.PostLegoSet(legoSetSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedLegoSetData) => {

        this.legoSetService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Lego Set's detail page
          //
          this.legoSetForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.legoSetForm.markAsUntouched();

          this.router.navigate(['/legosets', savedLegoSetData.id]);
          this.alertService.showMessage('Lego Set added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.legoSetData = savedLegoSetData;
          this.buildFormValues(this.legoSetData);

          this.alertService.showMessage("Lego Set saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Lego Set.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Lego Set.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Lego Set could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCLegoSetReader(): boolean {
    return this.legoSetService.userIsBMCLegoSetReader();
  }

  public userIsBMCLegoSetWriter(): boolean {
    return this.legoSetService.userIsBMCLegoSetWriter();
  }
}

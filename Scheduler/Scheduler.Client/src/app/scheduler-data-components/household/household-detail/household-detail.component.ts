/*
   GENERATED FORM FOR THE HOUSEHOLD TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Household table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to household-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { HouseholdService, HouseholdData, HouseholdSubmitData } from '../../../scheduler-data-services/household.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { HouseholdChangeHistoryService } from '../../../scheduler-data-services/household-change-history.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
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
interface HouseholdFormValues {
  name: string,
  description: string | null,
  schedulingTargetId: number | bigint | null,       // For FK link number
  formalSalutation: string | null,
  informalSalutation: string | null,
  addressee: string | null,
  totalHouseholdGiving: string,     // Stored as string for form input, converted to number on submit.
  lastHouseholdGiftDate: string | null,
  notes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-household-detail',
  templateUrl: './household-detail.component.html',
  styleUrls: ['./household-detail.component.scss']
})

export class HouseholdDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<HouseholdFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public householdForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        schedulingTargetId: [null],
        formalSalutation: [''],
        informalSalutation: [''],
        addressee: [''],
        totalHouseholdGiving: ['', Validators.required],
        lastHouseholdGiftDate: [''],
        notes: [''],
        iconId: [null],
        color: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public householdId: string | null = null;
  public householdData: HouseholdData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  households$ = this.householdService.GetHouseholdList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public icons$ = this.iconService.GetIconList();
  public householdChangeHistories$ = this.householdChangeHistoryService.GetHouseholdChangeHistoryList();
  public constituents$ = this.constituentService.GetConstituentList();
  public documents$ = this.documentService.GetDocumentList();

  private destroy$ = new Subject<void>();

  constructor(
    public householdService: HouseholdService,
    public schedulingTargetService: SchedulingTargetService,
    public iconService: IconService,
    public householdChangeHistoryService: HouseholdChangeHistoryService,
    public constituentService: ConstituentService,
    public documentService: DocumentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the householdId from the route parameters
    this.householdId = this.route.snapshot.paramMap.get('householdId');

    if (this.householdId === 'new' ||
        this.householdId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.householdData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.householdForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.householdForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Household';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Household';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.householdForm.dirty) {
      return confirm('You have unsaved Household changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.householdId != null && this.householdId !== 'new') {

      const id = parseInt(this.householdId, 10);

      if (!isNaN(id)) {
        return { householdId: id };
      }
    }

    return null;
  }


/*
  * Loads the Household data for the current householdId.
  *
  * Fully respects the HouseholdService caching strategy and error handling strategy.
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
    if (!this.householdService.userIsSchedulerHouseholdReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Households.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate householdId
    //
    if (!this.householdId) {

      this.alertService.showMessage('No Household ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const householdId = Number(this.householdId);

    if (isNaN(householdId) || householdId <= 0) {

      this.alertService.showMessage(`Invalid Household ID: "${this.householdId}"`,
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
      // This is the most targeted way: clear only this Household + relations

      this.householdService.ClearRecordCache(householdId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.householdService.GetHousehold(householdId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (householdData) => {

        //
        // Success path — householdData can legitimately be null if 404'd but request succeeded
        //
        if (!householdData) {

          this.handleHouseholdNotFound(householdId);

        } else {

          this.householdData = householdData;
          this.buildFormValues(this.householdData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Household loaded successfully',
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
        this.handleHouseholdLoadError(error, householdId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleHouseholdNotFound(householdId: number): void {

    this.householdData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Household #${householdId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleHouseholdLoadError(error: any, householdId: number): void {

    let message = 'Failed to load Household.';
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
          message = 'You do not have permission to view this Household.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Household #${householdId} was not found.`;
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

    console.error(`Household load failed (ID: ${householdId})`, error);

    //
    // Reset UI to safe state
    //
    this.householdData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(householdData: HouseholdData | null) {

    if (householdData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.householdForm.reset({
        name: '',
        description: '',
        schedulingTargetId: null,
        formalSalutation: '',
        informalSalutation: '',
        addressee: '',
        totalHouseholdGiving: '',
        lastHouseholdGiftDate: '',
        notes: '',
        iconId: null,
        color: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.householdForm.reset({
        name: householdData.name ?? '',
        description: householdData.description ?? '',
        schedulingTargetId: householdData.schedulingTargetId,
        formalSalutation: householdData.formalSalutation ?? '',
        informalSalutation: householdData.informalSalutation ?? '',
        addressee: householdData.addressee ?? '',
        totalHouseholdGiving: householdData.totalHouseholdGiving?.toString() ?? '',
        lastHouseholdGiftDate: householdData.lastHouseholdGiftDate ?? '',
        notes: householdData.notes ?? '',
        iconId: householdData.iconId,
        color: householdData.color ?? '',
        avatarFileName: householdData.avatarFileName ?? '',
        avatarSize: householdData.avatarSize?.toString() ?? '',
        avatarData: householdData.avatarData ?? '',
        avatarMimeType: householdData.avatarMimeType ?? '',
        versionNumber: householdData.versionNumber?.toString() ?? '',
        active: householdData.active ?? true,
        deleted: householdData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.householdForm.markAsPristine();
    this.householdForm.markAsUntouched();
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

    if (this.householdService.userIsSchedulerHouseholdWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Households", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.householdForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.householdForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.householdForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const householdSubmitData: HouseholdSubmitData = {
        id: this.householdData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        schedulingTargetId: formValue.schedulingTargetId ? Number(formValue.schedulingTargetId) : null,
        formalSalutation: formValue.formalSalutation?.trim() || null,
        informalSalutation: formValue.informalSalutation?.trim() || null,
        addressee: formValue.addressee?.trim() || null,
        totalHouseholdGiving: Number(formValue.totalHouseholdGiving),
        lastHouseholdGiftDate: formValue.lastHouseholdGiftDate ? formValue.lastHouseholdGiftDate.trim() : null,
        notes: formValue.notes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.householdData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.householdService.PutHousehold(householdSubmitData.id, householdSubmitData)
      : this.householdService.PostHousehold(householdSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedHouseholdData) => {

        this.householdService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Household's detail page
          //
          this.householdForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.householdForm.markAsUntouched();

          this.router.navigate(['/households', savedHouseholdData.id]);
          this.alertService.showMessage('Household added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.householdData = savedHouseholdData;
          this.buildFormValues(this.householdData);

          this.alertService.showMessage("Household saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Household.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Household.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Household could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerHouseholdReader(): boolean {
    return this.householdService.userIsSchedulerHouseholdReader();
  }

  public userIsSchedulerHouseholdWriter(): boolean {
    return this.householdService.userIsSchedulerHouseholdWriter();
  }
}

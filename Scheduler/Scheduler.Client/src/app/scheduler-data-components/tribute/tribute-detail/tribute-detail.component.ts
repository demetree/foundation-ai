/*
   GENERATED FORM FOR THE TRIBUTE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Tribute table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tribute-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TributeService, TributeData, TributeSubmitData } from '../../../scheduler-data-services/tribute.service';
import { TributeTypeService } from '../../../scheduler-data-services/tribute-type.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { TributeChangeHistoryService } from '../../../scheduler-data-services/tribute-change-history.service';
import { GiftService } from '../../../scheduler-data-services/gift.service';
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
interface TributeFormValues {
  name: string,
  description: string | null,
  tributeTypeId: number | bigint | null,       // For FK link number
  defaultAcknowledgeeId: number | bigint | null,       // For FK link number
  startDate: string | null,
  endDate: string | null,
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
  selector: 'app-tribute-detail',
  templateUrl: './tribute-detail.component.html',
  styleUrls: ['./tribute-detail.component.scss']
})

export class TributeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TributeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public tributeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        tributeTypeId: [null],
        defaultAcknowledgeeId: [null],
        startDate: [''],
        endDate: [''],
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


  public tributeId: string | null = null;
  public tributeData: TributeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  tributes$ = this.tributeService.GetTributeList();
  public tributeTypes$ = this.tributeTypeService.GetTributeTypeList();
  public constituents$ = this.constituentService.GetConstituentList();
  public icons$ = this.iconService.GetIconList();
  public tributeChangeHistories$ = this.tributeChangeHistoryService.GetTributeChangeHistoryList();
  public gifts$ = this.giftService.GetGiftList();

  private destroy$ = new Subject<void>();

  constructor(
    public tributeService: TributeService,
    public tributeTypeService: TributeTypeService,
    public constituentService: ConstituentService,
    public iconService: IconService,
    public tributeChangeHistoryService: TributeChangeHistoryService,
    public giftService: GiftService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the tributeId from the route parameters
    this.tributeId = this.route.snapshot.paramMap.get('tributeId');

    if (this.tributeId === 'new' ||
        this.tributeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.tributeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.tributeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.tributeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Tribute';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Tribute';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.tributeForm.dirty) {
      return confirm('You have unsaved Tribute changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.tributeId != null && this.tributeId !== 'new') {

      const id = parseInt(this.tributeId, 10);

      if (!isNaN(id)) {
        return { tributeId: id };
      }
    }

    return null;
  }


/*
  * Loads the Tribute data for the current tributeId.
  *
  * Fully respects the TributeService caching strategy and error handling strategy.
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
    if (!this.tributeService.userIsSchedulerTributeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Tributes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate tributeId
    //
    if (!this.tributeId) {

      this.alertService.showMessage('No Tribute ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const tributeId = Number(this.tributeId);

    if (isNaN(tributeId) || tributeId <= 0) {

      this.alertService.showMessage(`Invalid Tribute ID: "${this.tributeId}"`,
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
      // This is the most targeted way: clear only this Tribute + relations

      this.tributeService.ClearRecordCache(tributeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.tributeService.GetTribute(tributeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (tributeData) => {

        //
        // Success path — tributeData can legitimately be null if 404'd but request succeeded
        //
        if (!tributeData) {

          this.handleTributeNotFound(tributeId);

        } else {

          this.tributeData = tributeData;
          this.buildFormValues(this.tributeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Tribute loaded successfully',
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
        this.handleTributeLoadError(error, tributeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleTributeNotFound(tributeId: number): void {

    this.tributeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Tribute #${tributeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleTributeLoadError(error: any, tributeId: number): void {

    let message = 'Failed to load Tribute.';
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
          message = 'You do not have permission to view this Tribute.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Tribute #${tributeId} was not found.`;
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

    console.error(`Tribute load failed (ID: ${tributeId})`, error);

    //
    // Reset UI to safe state
    //
    this.tributeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(tributeData: TributeData | null) {

    if (tributeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.tributeForm.reset({
        name: '',
        description: '',
        tributeTypeId: null,
        defaultAcknowledgeeId: null,
        startDate: '',
        endDate: '',
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
        this.tributeForm.reset({
        name: tributeData.name ?? '',
        description: tributeData.description ?? '',
        tributeTypeId: tributeData.tributeTypeId,
        defaultAcknowledgeeId: tributeData.defaultAcknowledgeeId,
        startDate: tributeData.startDate ?? '',
        endDate: tributeData.endDate ?? '',
        iconId: tributeData.iconId,
        color: tributeData.color ?? '',
        avatarFileName: tributeData.avatarFileName ?? '',
        avatarSize: tributeData.avatarSize?.toString() ?? '',
        avatarData: tributeData.avatarData ?? '',
        avatarMimeType: tributeData.avatarMimeType ?? '',
        versionNumber: tributeData.versionNumber?.toString() ?? '',
        active: tributeData.active ?? true,
        deleted: tributeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.tributeForm.markAsPristine();
    this.tributeForm.markAsUntouched();
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

    if (this.tributeService.userIsSchedulerTributeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Tributes", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.tributeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.tributeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.tributeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const tributeSubmitData: TributeSubmitData = {
        id: this.tributeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        tributeTypeId: formValue.tributeTypeId ? Number(formValue.tributeTypeId) : null,
        defaultAcknowledgeeId: formValue.defaultAcknowledgeeId ? Number(formValue.defaultAcknowledgeeId) : null,
        startDate: formValue.startDate ? formValue.startDate.trim() : null,
        endDate: formValue.endDate ? formValue.endDate.trim() : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.tributeData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.tributeService.PutTribute(tributeSubmitData.id, tributeSubmitData)
      : this.tributeService.PostTribute(tributeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedTributeData) => {

        this.tributeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Tribute's detail page
          //
          this.tributeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.tributeForm.markAsUntouched();

          this.router.navigate(['/tributes', savedTributeData.id]);
          this.alertService.showMessage('Tribute added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.tributeData = savedTributeData;
          this.buildFormValues(this.tributeData);

          this.alertService.showMessage("Tribute saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Tribute.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tribute.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tribute could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerTributeReader(): boolean {
    return this.tributeService.userIsSchedulerTributeReader();
  }

  public userIsSchedulerTributeWriter(): boolean {
    return this.tributeService.userIsSchedulerTributeWriter();
  }
}

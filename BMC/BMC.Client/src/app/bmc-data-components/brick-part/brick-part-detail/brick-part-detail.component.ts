/*
   GENERATED FORM FOR THE BRICKPART TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickPart table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-part-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickPartService, BrickPartData, BrickPartSubmitData } from '../../../bmc-data-services/brick-part.service';
import { PartTypeService } from '../../../bmc-data-services/part-type.service';
import { BrickCategoryService } from '../../../bmc-data-services/brick-category.service';
import { BrickPartChangeHistoryService } from '../../../bmc-data-services/brick-part-change-history.service';
import { BrickPartConnectorService } from '../../../bmc-data-services/brick-part-connector.service';
import { BrickPartColourService } from '../../../bmc-data-services/brick-part-colour.service';
import { PlacedBrickService } from '../../../bmc-data-services/placed-brick.service';
import { LegoSetPartService } from '../../../bmc-data-services/lego-set-part.service';
import { BrickPartRelationshipService } from '../../../bmc-data-services/brick-part-relationship.service';
import { BrickElementService } from '../../../bmc-data-services/brick-element.service';
import { UserCollectionPartService } from '../../../bmc-data-services/user-collection-part.service';
import { UserWishlistItemService } from '../../../bmc-data-services/user-wishlist-item.service';
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
interface BrickPartFormValues {
  name: string,
  ldrawPartId: string,
  ldrawTitle: string | null,
  ldrawCategory: string | null,
  partTypeId: number | bigint,       // For FK link number
  keywords: string | null,
  author: string | null,
  brickCategoryId: number | bigint,       // For FK link number
  rebrickablePartNum: string | null,
  widthLdu: string | null,     // Stored as string for form input, converted to number on submit.
  heightLdu: string | null,     // Stored as string for form input, converted to number on submit.
  depthLdu: string | null,     // Stored as string for form input, converted to number on submit.
  massGrams: string | null,     // Stored as string for form input, converted to number on submit.
  geometryFilePath: string | null,
  toothCount: string | null,     // Stored as string for form input, converted to number on submit.
  gearRatio: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-brick-part-detail',
  templateUrl: './brick-part-detail.component.html',
  styleUrls: ['./brick-part-detail.component.scss']
})

export class BrickPartDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickPartFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickPartForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        ldrawPartId: ['', Validators.required],
        ldrawTitle: [''],
        ldrawCategory: [''],
        partTypeId: [null, Validators.required],
        keywords: [''],
        author: [''],
        brickCategoryId: [null, Validators.required],
        rebrickablePartNum: [''],
        widthLdu: [''],
        heightLdu: [''],
        depthLdu: [''],
        massGrams: [''],
        geometryFilePath: [''],
        toothCount: [''],
        gearRatio: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public brickPartId: string | null = null;
  public brickPartData: BrickPartData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  brickParts$ = this.brickPartService.GetBrickPartList();
  public partTypes$ = this.partTypeService.GetPartTypeList();
  public brickCategories$ = this.brickCategoryService.GetBrickCategoryList();
  public brickPartChangeHistories$ = this.brickPartChangeHistoryService.GetBrickPartChangeHistoryList();
  public brickPartConnectors$ = this.brickPartConnectorService.GetBrickPartConnectorList();
  public brickPartColours$ = this.brickPartColourService.GetBrickPartColourList();
  public placedBricks$ = this.placedBrickService.GetPlacedBrickList();
  public legoSetParts$ = this.legoSetPartService.GetLegoSetPartList();
  public brickPartRelationships$ = this.brickPartRelationshipService.GetBrickPartRelationshipList();
  public brickElements$ = this.brickElementService.GetBrickElementList();
  public userCollectionParts$ = this.userCollectionPartService.GetUserCollectionPartList();
  public userWishlistItems$ = this.userWishlistItemService.GetUserWishlistItemList();

  private destroy$ = new Subject<void>();

  constructor(
    public brickPartService: BrickPartService,
    public partTypeService: PartTypeService,
    public brickCategoryService: BrickCategoryService,
    public brickPartChangeHistoryService: BrickPartChangeHistoryService,
    public brickPartConnectorService: BrickPartConnectorService,
    public brickPartColourService: BrickPartColourService,
    public placedBrickService: PlacedBrickService,
    public legoSetPartService: LegoSetPartService,
    public brickPartRelationshipService: BrickPartRelationshipService,
    public brickElementService: BrickElementService,
    public userCollectionPartService: UserCollectionPartService,
    public userWishlistItemService: UserWishlistItemService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the brickPartId from the route parameters
    this.brickPartId = this.route.snapshot.paramMap.get('brickPartId');

    if (this.brickPartId === 'new' ||
        this.brickPartId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.brickPartData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.brickPartForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickPartForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Brick Part';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Brick Part';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.brickPartForm.dirty) {
      return confirm('You have unsaved Brick Part changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.brickPartId != null && this.brickPartId !== 'new') {

      const id = parseInt(this.brickPartId, 10);

      if (!isNaN(id)) {
        return { brickPartId: id };
      }
    }

    return null;
  }


/*
  * Loads the BrickPart data for the current brickPartId.
  *
  * Fully respects the BrickPartService caching strategy and error handling strategy.
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
    if (!this.brickPartService.userIsBMCBrickPartReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read BrickParts.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate brickPartId
    //
    if (!this.brickPartId) {

      this.alertService.showMessage('No BrickPart ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const brickPartId = Number(this.brickPartId);

    if (isNaN(brickPartId) || brickPartId <= 0) {

      this.alertService.showMessage(`Invalid Brick Part ID: "${this.brickPartId}"`,
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
      // This is the most targeted way: clear only this BrickPart + relations

      this.brickPartService.ClearRecordCache(brickPartId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.brickPartService.GetBrickPart(brickPartId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (brickPartData) => {

        //
        // Success path — brickPartData can legitimately be null if 404'd but request succeeded
        //
        if (!brickPartData) {

          this.handleBrickPartNotFound(brickPartId);

        } else {

          this.brickPartData = brickPartData;
          this.buildFormValues(this.brickPartData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'BrickPart loaded successfully',
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
        this.handleBrickPartLoadError(error, brickPartId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleBrickPartNotFound(brickPartId: number): void {

    this.brickPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `BrickPart #${brickPartId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleBrickPartLoadError(error: any, brickPartId: number): void {

    let message = 'Failed to load Brick Part.';
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
          message = 'You do not have permission to view this Brick Part.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Brick Part #${brickPartId} was not found.`;
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

    console.error(`Brick Part load failed (ID: ${brickPartId})`, error);

    //
    // Reset UI to safe state
    //
    this.brickPartData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(brickPartData: BrickPartData | null) {

    if (brickPartData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickPartForm.reset({
        name: '',
        ldrawPartId: '',
        ldrawTitle: '',
        ldrawCategory: '',
        partTypeId: null,
        keywords: '',
        author: '',
        brickCategoryId: null,
        rebrickablePartNum: '',
        widthLdu: '',
        heightLdu: '',
        depthLdu: '',
        massGrams: '',
        geometryFilePath: '',
        toothCount: '',
        gearRatio: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickPartForm.reset({
        name: brickPartData.name ?? '',
        ldrawPartId: brickPartData.ldrawPartId ?? '',
        ldrawTitle: brickPartData.ldrawTitle ?? '',
        ldrawCategory: brickPartData.ldrawCategory ?? '',
        partTypeId: brickPartData.partTypeId,
        keywords: brickPartData.keywords ?? '',
        author: brickPartData.author ?? '',
        brickCategoryId: brickPartData.brickCategoryId,
        rebrickablePartNum: brickPartData.rebrickablePartNum ?? '',
        widthLdu: brickPartData.widthLdu?.toString() ?? '',
        heightLdu: brickPartData.heightLdu?.toString() ?? '',
        depthLdu: brickPartData.depthLdu?.toString() ?? '',
        massGrams: brickPartData.massGrams?.toString() ?? '',
        geometryFilePath: brickPartData.geometryFilePath ?? '',
        toothCount: brickPartData.toothCount?.toString() ?? '',
        gearRatio: brickPartData.gearRatio?.toString() ?? '',
        versionNumber: brickPartData.versionNumber?.toString() ?? '',
        active: brickPartData.active ?? true,
        deleted: brickPartData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickPartForm.markAsPristine();
    this.brickPartForm.markAsUntouched();
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

    if (this.brickPartService.userIsBMCBrickPartWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Brick Parts", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.brickPartForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickPartForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickPartForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickPartSubmitData: BrickPartSubmitData = {
        id: this.brickPartData?.id || 0,
        name: formValue.name!.trim(),
        ldrawPartId: formValue.ldrawPartId!.trim(),
        ldrawTitle: formValue.ldrawTitle?.trim() || null,
        ldrawCategory: formValue.ldrawCategory?.trim() || null,
        partTypeId: Number(formValue.partTypeId),
        keywords: formValue.keywords?.trim() || null,
        author: formValue.author?.trim() || null,
        brickCategoryId: Number(formValue.brickCategoryId),
        rebrickablePartNum: formValue.rebrickablePartNum?.trim() || null,
        widthLdu: formValue.widthLdu ? Number(formValue.widthLdu) : null,
        heightLdu: formValue.heightLdu ? Number(formValue.heightLdu) : null,
        depthLdu: formValue.depthLdu ? Number(formValue.depthLdu) : null,
        massGrams: formValue.massGrams ? Number(formValue.massGrams) : null,
        geometryFilePath: formValue.geometryFilePath?.trim() || null,
        toothCount: formValue.toothCount ? Number(formValue.toothCount) : null,
        gearRatio: formValue.gearRatio ? Number(formValue.gearRatio) : null,
        versionNumber: this.brickPartData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.brickPartService.PutBrickPart(brickPartSubmitData.id, brickPartSubmitData)
      : this.brickPartService.PostBrickPart(brickPartSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedBrickPartData) => {

        this.brickPartService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Brick Part's detail page
          //
          this.brickPartForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.brickPartForm.markAsUntouched();

          this.router.navigate(['/brickparts', savedBrickPartData.id]);
          this.alertService.showMessage('Brick Part added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.brickPartData = savedBrickPartData;
          this.buildFormValues(this.brickPartData);

          this.alertService.showMessage("Brick Part saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Brick Part.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Part.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Part could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsBMCBrickPartReader(): boolean {
    return this.brickPartService.userIsBMCBrickPartReader();
  }

  public userIsBMCBrickPartWriter(): boolean {
    return this.brickPartService.userIsBMCBrickPartWriter();
  }
}

/*
   GENERATED FORM FOR THE PAGECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PageChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to page-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PageChangeHistoryService, PageChangeHistoryData, PageChangeHistorySubmitData } from '../../../community-data-services/page-change-history.service';
import { PageService } from '../../../community-data-services/page.service';
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
interface PageChangeHistoryFormValues {
  pageId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};


@Component({
  selector: 'app-page-change-history-detail',
  templateUrl: './page-change-history-detail.component.html',
  styleUrls: ['./page-change-history-detail.component.scss']
})

export class PageChangeHistoryDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PageChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pageChangeHistoryForm: FormGroup = this.fb.group({
        pageId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });


  public pageChangeHistoryId: string | null = null;
  public pageChangeHistoryData: PageChangeHistoryData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  pageChangeHistories$ = this.pageChangeHistoryService.GetPageChangeHistoryList();
  public pages$ = this.pageService.GetPageList();

  private destroy$ = new Subject<void>();

  constructor(
    public pageChangeHistoryService: PageChangeHistoryService,
    public pageService: PageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the pageChangeHistoryId from the route parameters
    this.pageChangeHistoryId = this.route.snapshot.paramMap.get('pageChangeHistoryId');

    if (this.pageChangeHistoryId === 'new' ||
        this.pageChangeHistoryId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.pageChangeHistoryData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.pageChangeHistoryForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pageChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Page Change History';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Page Change History';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.pageChangeHistoryForm.dirty) {
      return confirm('You have unsaved Page Change History changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.pageChangeHistoryId != null && this.pageChangeHistoryId !== 'new') {

      const id = parseInt(this.pageChangeHistoryId, 10);

      if (!isNaN(id)) {
        return { pageChangeHistoryId: id };
      }
    }

    return null;
  }


/*
  * Loads the PageChangeHistory data for the current pageChangeHistoryId.
  *
  * Fully respects the PageChangeHistoryService caching strategy and error handling strategy.
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
    if (!this.pageChangeHistoryService.userIsCommunityPageChangeHistoryReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read PageChangeHistories.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate pageChangeHistoryId
    //
    if (!this.pageChangeHistoryId) {

      this.alertService.showMessage('No PageChangeHistory ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const pageChangeHistoryId = Number(this.pageChangeHistoryId);

    if (isNaN(pageChangeHistoryId) || pageChangeHistoryId <= 0) {

      this.alertService.showMessage(`Invalid Page Change History ID: "${this.pageChangeHistoryId}"`,
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
      // This is the most targeted way: clear only this PageChangeHistory + relations

      this.pageChangeHistoryService.ClearRecordCache(pageChangeHistoryId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.pageChangeHistoryService.GetPageChangeHistory(pageChangeHistoryId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (pageChangeHistoryData) => {

        //
        // Success path — pageChangeHistoryData can legitimately be null if 404'd but request succeeded
        //
        if (!pageChangeHistoryData) {

          this.handlePageChangeHistoryNotFound(pageChangeHistoryId);

        } else {

          this.pageChangeHistoryData = pageChangeHistoryData;
          this.buildFormValues(this.pageChangeHistoryData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'PageChangeHistory loaded successfully',
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
        this.handlePageChangeHistoryLoadError(error, pageChangeHistoryId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePageChangeHistoryNotFound(pageChangeHistoryId: number): void {

    this.pageChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `PageChangeHistory #${pageChangeHistoryId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePageChangeHistoryLoadError(error: any, pageChangeHistoryId: number): void {

    let message = 'Failed to load Page Change History.';
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
          message = 'You do not have permission to view this Page Change History.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Page Change History #${pageChangeHistoryId} was not found.`;
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

    console.error(`Page Change History load failed (ID: ${pageChangeHistoryId})`, error);

    //
    // Reset UI to safe state
    //
    this.pageChangeHistoryData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(pageChangeHistoryData: PageChangeHistoryData | null) {

    if (pageChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pageChangeHistoryForm.reset({
        pageId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pageChangeHistoryForm.reset({
        pageId: pageChangeHistoryData.pageId,
        versionNumber: pageChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(pageChangeHistoryData.timeStamp) ?? '',
        userId: pageChangeHistoryData.userId?.toString() ?? '',
        data: pageChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.pageChangeHistoryForm.markAsPristine();
    this.pageChangeHistoryForm.markAsUntouched();
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

    if (this.pageChangeHistoryService.userIsCommunityPageChangeHistoryWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Page Change Histories", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.pageChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pageChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pageChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pageChangeHistorySubmitData: PageChangeHistorySubmitData = {
        id: this.pageChangeHistoryData?.id || 0,
        pageId: Number(formValue.pageId),
        versionNumber: this.pageChangeHistoryData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.pageChangeHistoryService.PutPageChangeHistory(pageChangeHistorySubmitData.id, pageChangeHistorySubmitData)
      : this.pageChangeHistoryService.PostPageChangeHistory(pageChangeHistorySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPageChangeHistoryData) => {

        this.pageChangeHistoryService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Page Change History's detail page
          //
          this.pageChangeHistoryForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.pageChangeHistoryForm.markAsUntouched();

          this.router.navigate(['/pagechangehistories', savedPageChangeHistoryData.id]);
          this.alertService.showMessage('Page Change History added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.pageChangeHistoryData = savedPageChangeHistoryData;
          this.buildFormValues(this.pageChangeHistoryData);

          this.alertService.showMessage("Page Change History saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Page Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Page Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Page Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityPageChangeHistoryReader(): boolean {
    return this.pageChangeHistoryService.userIsCommunityPageChangeHistoryReader();
  }

  public userIsCommunityPageChangeHistoryWriter(): boolean {
    return this.pageChangeHistoryService.userIsCommunityPageChangeHistoryWriter();
  }
}

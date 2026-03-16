/*
   GENERATED FORM FOR THE PAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Page table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to page-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PageService, PageData, PageSubmitData } from '../../../community-data-services/page.service';
import { PageChangeHistoryService } from '../../../community-data-services/page-change-history.service';
import { MenuItemService } from '../../../community-data-services/menu-item.service';
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
interface PageFormValues {
  title: string,
  slug: string,
  body: string | null,
  metaDescription: string | null,
  featuredImageUrl: string | null,
  isPublished: boolean,
  publishedDate: string | null,
  sortOrder: string | null,     // Stored as string for form input, converted to number on submit.
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-page-detail',
  templateUrl: './page-detail.component.html',
  styleUrls: ['./page-detail.component.scss']
})

export class PageDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pageForm: FormGroup = this.fb.group({
        title: ['', Validators.required],
        slug: ['', Validators.required],
        body: [''],
        metaDescription: [''],
        featuredImageUrl: [''],
        isPublished: [false],
        publishedDate: [''],
        sortOrder: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public pageId: string | null = null;
  public pageData: PageData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  pages$ = this.pageService.GetPageList();
  public pageChangeHistories$ = this.pageChangeHistoryService.GetPageChangeHistoryList();
  public menuItems$ = this.menuItemService.GetMenuItemList();

  private destroy$ = new Subject<void>();

  constructor(
    public pageService: PageService,
    public pageChangeHistoryService: PageChangeHistoryService,
    public menuItemService: MenuItemService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the pageId from the route parameters
    this.pageId = this.route.snapshot.paramMap.get('pageId');

    if (this.pageId === 'new' ||
        this.pageId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.pageData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.pageForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Page';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Page';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.pageForm.dirty) {
      return confirm('You have unsaved Page changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.pageId != null && this.pageId !== 'new') {

      const id = parseInt(this.pageId, 10);

      if (!isNaN(id)) {
        return { pageId: id };
      }
    }

    return null;
  }


/*
  * Loads the Page data for the current pageId.
  *
  * Fully respects the PageService caching strategy and error handling strategy.
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
    if (!this.pageService.userIsCommunityPageReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Pages.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate pageId
    //
    if (!this.pageId) {

      this.alertService.showMessage('No Page ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const pageId = Number(this.pageId);

    if (isNaN(pageId) || pageId <= 0) {

      this.alertService.showMessage(`Invalid Page ID: "${this.pageId}"`,
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
      // This is the most targeted way: clear only this Page + relations

      this.pageService.ClearRecordCache(pageId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.pageService.GetPage(pageId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (pageData) => {

        //
        // Success path — pageData can legitimately be null if 404'd but request succeeded
        //
        if (!pageData) {

          this.handlePageNotFound(pageId);

        } else {

          this.pageData = pageData;
          this.buildFormValues(this.pageData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Page loaded successfully',
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
        this.handlePageLoadError(error, pageId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handlePageNotFound(pageId: number): void {

    this.pageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Page #${pageId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handlePageLoadError(error: any, pageId: number): void {

    let message = 'Failed to load Page.';
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
          message = 'You do not have permission to view this Page.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Page #${pageId} was not found.`;
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

    console.error(`Page load failed (ID: ${pageId})`, error);

    //
    // Reset UI to safe state
    //
    this.pageData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(pageData: PageData | null) {

    if (pageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pageForm.reset({
        title: '',
        slug: '',
        body: '',
        metaDescription: '',
        featuredImageUrl: '',
        isPublished: false,
        publishedDate: '',
        sortOrder: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pageForm.reset({
        title: pageData.title ?? '',
        slug: pageData.slug ?? '',
        body: pageData.body ?? '',
        metaDescription: pageData.metaDescription ?? '',
        featuredImageUrl: pageData.featuredImageUrl ?? '',
        isPublished: pageData.isPublished ?? false,
        publishedDate: isoUtcStringToDateTimeLocal(pageData.publishedDate) ?? '',
        sortOrder: pageData.sortOrder?.toString() ?? '',
        versionNumber: pageData.versionNumber?.toString() ?? '',
        active: pageData.active ?? true,
        deleted: pageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.pageForm.markAsPristine();
    this.pageForm.markAsUntouched();
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

    if (this.pageService.userIsCommunityPageWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Pages", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.pageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pageSubmitData: PageSubmitData = {
        id: this.pageData?.id || 0,
        title: formValue.title!.trim(),
        slug: formValue.slug!.trim(),
        body: formValue.body?.trim() || null,
        metaDescription: formValue.metaDescription?.trim() || null,
        featuredImageUrl: formValue.featuredImageUrl?.trim() || null,
        isPublished: !!formValue.isPublished,
        publishedDate: formValue.publishedDate ? dateTimeLocalToIsoUtc(formValue.publishedDate.trim()) : null,
        sortOrder: formValue.sortOrder ? Number(formValue.sortOrder) : null,
        versionNumber: this.pageData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.pageService.PutPage(pageSubmitData.id, pageSubmitData)
      : this.pageService.PostPage(pageSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedPageData) => {

        this.pageService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Page's detail page
          //
          this.pageForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.pageForm.markAsUntouched();

          this.router.navigate(['/pages', savedPageData.id]);
          this.alertService.showMessage('Page added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.pageData = savedPageData;
          this.buildFormValues(this.pageData);

          this.alertService.showMessage("Page saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Page.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Page.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Page could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityPageReader(): boolean {
    return this.pageService.userIsCommunityPageReader();
  }

  public userIsCommunityPageWriter(): boolean {
    return this.pageService.userIsCommunityPageWriter();
  }
}

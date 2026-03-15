/*
   GENERATED FORM FOR THE MENUITEM TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MenuItem table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to menu-item-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MenuItemService, MenuItemData, MenuItemSubmitData } from '../../../community-data-services/menu-item.service';
import { MenuService } from '../../../community-data-services/menu.service';
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
interface MenuItemFormValues {
};


@Component({
  selector: 'app-menu-item-detail',
  templateUrl: './menu-item-detail.component.html',
  styleUrls: ['./menu-item-detail.component.scss']
})

export class MenuItemDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MenuItemFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public menuItemForm: FormGroup = this.fb.group({
      });


  public menuItemId: string | null = null;
  public menuItemData: MenuItemData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  menuItems$ = this.menuItemService.GetMenuItemList();
  public menus$ = this.menuService.GetMenuList();
  public pages$ = this.pageService.GetPageList();

  private destroy$ = new Subject<void>();

  constructor(
    public menuItemService: MenuItemService,
    public menuService: MenuService,
    public pageService: PageService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the menuItemId from the route parameters
    this.menuItemId = this.route.snapshot.paramMap.get('menuItemId');

    if (this.menuItemId === 'new' ||
        this.menuItemId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.menuItemData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.menuItemForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.menuItemForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Menu Item';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Menu Item';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.menuItemForm.dirty) {
      return confirm('You have unsaved Menu Item changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.menuItemId != null && this.menuItemId !== 'new') {

      const id = parseInt(this.menuItemId, 10);

      if (!isNaN(id)) {
        return { menuItemId: id };
      }
    }

    return null;
  }


/*
  * Loads the MenuItem data for the current menuItemId.
  *
  * Fully respects the MenuItemService caching strategy and error handling strategy.
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
    if (!this.menuItemService.userIsCommunityMenuItemReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read MenuItems.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate menuItemId
    //
    if (!this.menuItemId) {

      this.alertService.showMessage('No MenuItem ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const menuItemId = Number(this.menuItemId);

    if (isNaN(menuItemId) || menuItemId <= 0) {

      this.alertService.showMessage(`Invalid Menu Item ID: "${this.menuItemId}"`,
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
      // This is the most targeted way: clear only this MenuItem + relations

      this.menuItemService.ClearRecordCache(menuItemId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.menuItemService.GetMenuItem(menuItemId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (menuItemData) => {

        //
        // Success path — menuItemData can legitimately be null if 404'd but request succeeded
        //
        if (!menuItemData) {

          this.handleMenuItemNotFound(menuItemId);

        } else {

          this.menuItemData = menuItemData;
          this.buildFormValues(this.menuItemData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'MenuItem loaded successfully',
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
        this.handleMenuItemLoadError(error, menuItemId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleMenuItemNotFound(menuItemId: number): void {

    this.menuItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `MenuItem #${menuItemId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleMenuItemLoadError(error: any, menuItemId: number): void {

    let message = 'Failed to load Menu Item.';
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
          message = 'You do not have permission to view this Menu Item.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Menu Item #${menuItemId} was not found.`;
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

    console.error(`Menu Item load failed (ID: ${menuItemId})`, error);

    //
    // Reset UI to safe state
    //
    this.menuItemData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(menuItemData: MenuItemData | null) {

    if (menuItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.menuItemForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.menuItemForm.reset({
      }, { emitEvent: false});
    }

    this.menuItemForm.markAsPristine();
    this.menuItemForm.markAsUntouched();
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

    if (this.menuItemService.userIsCommunityMenuItemWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Menu Items", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.menuItemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.menuItemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.menuItemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const menuItemSubmitData: MenuItemSubmitData = {
        id: this.menuItemData?.id || 0,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.menuItemService.PutMenuItem(menuItemSubmitData.id, menuItemSubmitData)
      : this.menuItemService.PostMenuItem(menuItemSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedMenuItemData) => {

        this.menuItemService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Menu Item's detail page
          //
          this.menuItemForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.menuItemForm.markAsUntouched();

          this.router.navigate(['/menuitems', savedMenuItemData.id]);
          this.alertService.showMessage('Menu Item added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.menuItemData = savedMenuItemData;
          this.buildFormValues(this.menuItemData);

          this.alertService.showMessage("Menu Item saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Menu Item.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Menu Item.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Menu Item could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsCommunityMenuItemReader(): boolean {
    return this.menuItemService.userIsCommunityMenuItemReader();
  }

  public userIsCommunityMenuItemWriter(): boolean {
    return this.menuItemService.userIsCommunityMenuItemWriter();
  }
}

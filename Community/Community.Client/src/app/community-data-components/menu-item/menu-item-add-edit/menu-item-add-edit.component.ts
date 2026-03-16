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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MenuItemService, MenuItemData, MenuItemSubmitData } from '../../../community-data-services/menu-item.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { MenuService } from '../../../community-data-services/menu.service';
import { PageService } from '../../../community-data-services/page.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MenuItemFormValues {
  menuId: number | bigint,       // For FK link number
  label: string,
  url: string | null,
  pageId: number | bigint | null,       // For FK link number
  parentMenuItemId: number | bigint | null,       // For FK link number
  iconClass: string | null,
  openInNewTab: boolean,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-menu-item-add-edit',
  templateUrl: './menu-item-add-edit.component.html',
  styleUrls: ['./menu-item-add-edit.component.scss']
})
export class MenuItemAddEditComponent {
  @ViewChild('menuItemModal') menuItemModal!: TemplateRef<any>;
  @Output() menuItemChanged = new Subject<MenuItemData[]>();
  @Input() menuItemSubmitData: MenuItemSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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
        menuId: [null, Validators.required],
        label: ['', Validators.required],
        url: [''],
        pageId: [null],
        parentMenuItemId: [null],
        iconClass: [''],
        openInNewTab: [false],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  menuItems$ = this.menuItemService.GetMenuItemList();
  menus$ = this.menuService.GetMenuList();
  pages$ = this.pageService.GetPageList();

  constructor(
    private modalService: NgbModal,
    private menuItemService: MenuItemService,
    private menuService: MenuService,
    private pageService: PageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(menuItemData?: MenuItemData) {

    if (menuItemData != null) {

      if (!this.menuItemService.userIsCommunityMenuItemReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Menu Items`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.menuItemSubmitData = this.menuItemService.ConvertToMenuItemSubmitData(menuItemData);
      this.isEditMode = true;
      this.objectGuid = menuItemData.objectGuid;

      this.buildFormValues(menuItemData);

    } else {

      if (!this.menuItemService.userIsCommunityMenuItemWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Menu Items`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.menuItemForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.menuItemModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.menuItemService.userIsCommunityMenuItemWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Menu Items`,
        '',
        MessageSeverity.info
      );
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
        id: this.menuItemSubmitData?.id || 0,
        menuId: Number(formValue.menuId),
        label: formValue.label!.trim(),
        url: formValue.url?.trim() || null,
        pageId: formValue.pageId ? Number(formValue.pageId) : null,
        parentMenuItemId: formValue.parentMenuItemId ? Number(formValue.parentMenuItemId) : null,
        iconClass: formValue.iconClass?.trim() || null,
        openInNewTab: !!formValue.openInNewTab,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMenuItem(menuItemSubmitData);
      } else {
        this.addMenuItem(menuItemSubmitData);
      }
  }

  private addMenuItem(menuItemData: MenuItemSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    menuItemData.active = true;
    menuItemData.deleted = false;
    this.menuItemService.PostMenuItem(menuItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMenuItem) => {

        this.menuItemService.ClearAllCaches();

        this.menuItemChanged.next([newMenuItem]);

        this.alertService.showMessage("Menu Item added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/menuitem', newMenuItem.id]);
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


  private updateMenuItem(menuItemData: MenuItemSubmitData) {
    this.menuItemService.PutMenuItem(menuItemData.id, menuItemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMenuItem) => {

        this.menuItemService.ClearAllCaches();

        this.menuItemChanged.next([updatedMenuItem]);

        this.alertService.showMessage("Menu Item updated successfully", '', MessageSeverity.success);

        this.closeModal();
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



  private buildFormValues(menuItemData: MenuItemData | null) {

    if (menuItemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.menuItemForm.reset({
        menuId: null,
        label: '',
        url: '',
        pageId: null,
        parentMenuItemId: null,
        iconClass: '',
        openInNewTab: false,
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.menuItemForm.reset({
        menuId: menuItemData.menuId,
        label: menuItemData.label ?? '',
        url: menuItemData.url ?? '',
        pageId: menuItemData.pageId,
        parentMenuItemId: menuItemData.parentMenuItemId,
        iconClass: menuItemData.iconClass ?? '',
        openInNewTab: menuItemData.openInNewTab ?? false,
        sequence: menuItemData.sequence?.toString() ?? '',
        active: menuItemData.active ?? true,
        deleted: menuItemData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.menuItemForm.markAsPristine();
    this.menuItemForm.markAsUntouched();
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


  public userIsCommunityMenuItemReader(): boolean {
    return this.menuItemService.userIsCommunityMenuItemReader();
  }

  public userIsCommunityMenuItemWriter(): boolean {
    return this.menuItemService.userIsCommunityMenuItemWriter();
  }
}

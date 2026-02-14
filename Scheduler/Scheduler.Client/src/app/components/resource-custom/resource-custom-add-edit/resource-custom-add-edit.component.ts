import { Component, ViewChild, Output, Input, TemplateRef, SimpleChanges } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceService, ResourceData, ResourceSubmitData } from '../../../scheduler-data-services/resource.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ResourceTypeService } from '../../../scheduler-data-services/resource-type.service';
import { ShiftPatternService } from '../../../scheduler-data-services/shift-pattern.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { AuthService } from '../../../services/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';

@Component({
  selector: 'app-resource-custom-add-edit',
  templateUrl: './resource-custom-add-edit.component.html',
  styleUrls: ['./resource-custom-add-edit.component.scss'],
  animations: [
    trigger('collapse', [
      state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
      state('true', style({ height: '*', opacity: 1 })),
      transition('false <=> true', animate('300ms ease-in-out'))
    ])
  ]
})
export class ResourceCustomAddEditComponent {
  @ViewChild('resourceModal') resourceModal!: TemplateRef<any>;
  @Output() resourceChanged = new Subject<ResourceData[]>();
  @Input() resourceSubmitData: ResourceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;
  @Input() officeId: number | null = null;      // to preset the office to use when adding a contact


  public resourceData: ResourceData | null = null;

  public currentAvatarUrl: string | null = null;
  public isAvatarPanelOpen = false;
  public isDragOver = false;

  public attributesParsed: any = {};

  onDynamicAttributeChange(data: any) {
    this.attributesParsed = data;
    this.resourceForm.markAsDirty();
  }

  resourceForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    officeId: [this.officeId ?? this.currentUserService.defaultOfficeId],
    resourceTypeId: [null, Validators.required],
    shiftPatternId: [null],
    timeZoneId: [this.currentUserService.defaultTimeZoneId, Validators.required],
    targetWeeklyWorkHours: [''],
    externalId: [''],
    color: [''],
    attributes: [''],
    avatarFileName: [''],
    avatarSize: [''],
    avatarData: [''],
    avatarMimeType: [''],
    versionNumber: [''],
    active: [true],
    deleted: [false],
  });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  public resources$ = this.resourceService.GetResourceList();
  public offices$ = this.officeService.GetOfficeList();
  public resourceTypes$ = this.resourceTypeService.GetResourceTypeList();
  public shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();
  public timeZones$ = this.timeZoneService.GetTimeZoneList();

  constructor(
    private modalService: NgbModal,
    private resourceService: ResourceService,
    private officeService: OfficeService,
    private resourceTypeService: ResourceTypeService,
    private shiftPatternService: ShiftPatternService,
    private timeZoneService: TimeZoneService,
    private authService: AuthService,
    private alertService: AlertService,
    private schedulerHelperService: SchedulerHelperService,
    private currentUserService: CurrentUserService,
    private router: Router,
    private fb: FormBuilder) {
  }

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['officeId']) {

      if (this.resourceData != null) {
        this.resourceData.officeId = this.officeId as number;
      }

      this.resourceForm.patchValue({
        officeId: this.officeId
      });
    }
  }

  openModal(resourceData?: ResourceData) {

    if (resourceData != null) {

      if (!this.resourceService.userIsSchedulerResourceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Resources`,
          '',
          MessageSeverity.info
        );
        return;
      }

      this.resourceData = resourceData;

      this.resourceSubmitData = this.resourceService.ConvertToResourceSubmitData(resourceData);
      this.isEditMode = true;
      this.objectGuid = resourceData.objectGuid;

      this.buildFormValues(resourceData);

    } else {

      if (!this.resourceService.userIsSchedulerResourceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Resources`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.resourceData = null;

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.resourceModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    if (!file.type.startsWith('image/')) {
      this.alertService.showMessage('Invalid file type', 'Please drop an image file', MessageSeverity.warn);
      return;
    }

    // Reuse existing logic — simulate file input change
    const fakeEvent = { target: { files: [file] } } as any;
    this.onAvatarSelected(fakeEvent);
  }


  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    // Enforce 2MB limit
    if (file.size > 2 * 1024 * 1024) {
      this.alertService.showMessage(
        'Image too large',
        'Please select an image under 2MB',
        MessageSeverity.warn
      );
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const result = e.target?.result as string;

      if (!result) return;

      // Extract only the base64 part (remove data:image/png;base64, prefix)
      const base64Data = result.split(',')[1];

      if (!base64Data) {
        this.alertService.showMessage('Invalid image data', '', MessageSeverity.error);
        return;
      }

      this.currentAvatarUrl = result; // Full data URL for preview (includes prefix)

      // Populate form fields
      this.resourceForm.patchValue({
        avatarFileName: file.name,
        avatarSize: file.size,
        avatarData: base64Data,         // ← Only the raw base64 string
        avatarMimeType: file.type
      });

      this.resourceForm.markAsDirty();
    };

    reader.onerror = () => {
      this.alertService.showMessage('Failed to read file', '', MessageSeverity.error);
    };

    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.currentAvatarUrl = null;
    this.resourceForm.patchValue({
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    });
    this.resourceForm.markAsDirty();
  }

  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.isAvatarPanelOpen = false;
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.resourceService.userIsSchedulerResourceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Resources`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.resourceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    // Note not falling back to user default values on null here becase we want them to be able to create records with null linksif they set the form that way
    //
    const resourceSubmitData: ResourceSubmitData = {
      id: this.resourceSubmitData?.id || 0,
      name: formValue.name!.trim(),
      description: formValue.description?.trim() || null,
      officeId: formValue.officeId ? Number(formValue.officeId) : null,
      resourceTypeId: Number(formValue.resourceTypeId),
      shiftPatternId: formValue.shiftPatternId ? Number(formValue.shiftPatternId) : null,
      timeZoneId: Number(formValue.timeZoneId),
      targetWeeklyWorkHours: formValue.targetWeeklyWorkHours ? Number(formValue.targetWeeklyWorkHours) : null,
      notes: formValue.notes?.trim() || null,
      externalId: formValue.externalId?.trim() || null,
      color: formValue.color?.trim() || null,
      attributes: Object.keys(this.attributesParsed).length > 0 ? JSON.stringify(this.attributesParsed) : null,
      avatarFileName: formValue.avatarFileName?.trim() || null,
      avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
      avatarData: formValue.avatarData?.trim() || null,
      avatarMimeType: formValue.avatarMimeType?.trim() || null,
      versionNumber: this.resourceSubmitData?.versionNumber ?? 0,
      active: !!formValue.active,
      deleted: !!formValue.deleted,
    };

    if (this.isEditMode) {
      this.updateResource(resourceSubmitData);
    } else {
      this.addResource(resourceSubmitData);
    }
  }

  private addResource(resourceData: ResourceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    resourceData.versionNumber = 0;
    resourceData.active = true;
    resourceData.deleted = false;
    this.resourceService.PostResource(resourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newResource) => {

        this.resourceService.ClearAllCaches();

        this.resourceChanged.next([newResource]);

        this.alertService.showMessage("Resource added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/resource', newResource.id]);
        }
      },
      error: (err) => {
        let errorMessage: string;

        // Check if err is an Error object (e.g., new Error('message'))
        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        // Check if err is a ServerError object with status and error properties
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Resource.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Resource.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Resource could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }


  private updateResource(resourceData: ResourceSubmitData) {
    this.resourceService.PutResource(resourceData.id, resourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedResource) => {

        this.resourceService.ClearAllCaches();

        this.resourceChanged.next([updatedResource]);

        this.alertService.showMessage("Resource updated successfully", '', MessageSeverity.success);

        this.closeModal();
      },
      error: (err) => {
        let errorMessage: string;

        // Check if err is an Error object (e.g., new Error('message'))
        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        // Check if err is a ServerError object with status and error properties
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Resource.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Resource.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Resource could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(resourceData: ResourceData | null) {

    if (resourceData == null) {

      this.attributesParsed = {};
      this.currentAvatarUrl = null;
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceForm.reset({
        name: '',
        description: '',
        officeId: this.officeId ?? this.currentUserService.defaultOfficeId,
        resourceTypeId: null,
        shiftPatternId: null,
        timeZoneId: this.currentUserService.defaultTimeZoneId,
        targetWeeklyWorkHours: '',
        notes: '',
        externalId: '',
        color: '',
        attributes: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
      }, { emitEvent: false });

    }
    else {

      try {
        this.attributesParsed = resourceData.attributes ? JSON.parse(resourceData.attributes) : {};
      } catch (e) {
        this.attributesParsed = {};
      }

      // Reconstruct full data URL for preview if we have base64 data
      if (resourceData.avatarData && resourceData.avatarMimeType) {
        this.currentAvatarUrl = `data:${resourceData.avatarMimeType};base64,${resourceData.avatarData}`;
      } else {
        this.currentAvatarUrl = null;
      }

      //
      // Reset the form with properly formatted values that support dates in datetime-local inputs
      //
      this.resourceForm.reset({
        name: resourceData.name ?? '',
        description: resourceData.description ?? '',
        officeId: this.officeId ?? resourceData.officeId ?? this.currentUserService.defaultOfficeId,
        resourceTypeId: resourceData.resourceTypeId,
        shiftPatternId: resourceData.shiftPatternId,
        timeZoneId: resourceData.timeZoneId ?? this.currentUserService.defaultTimeZoneId,
        targetWeeklyWorkHours: resourceData.targetWeeklyWorkHours?.toString() ?? '',
        notes: resourceData.notes ?? '',
        externalId: resourceData.externalId ?? '',
        color: resourceData.color ?? '',
        attributes: resourceData.attributes ?? '',
        avatarFileName: resourceData.avatarFileName ?? '',
        avatarSize: resourceData.avatarSize?.toString() ?? '',
        avatarData: resourceData.avatarData ?? '',
        avatarMimeType: resourceData.avatarMimeType ?? '',
        versionNumber: resourceData.versionNumber?.toString() ?? '',
        active: resourceData.active ?? true,
        deleted: resourceData.deleted ?? false,
      }, { emitEvent: false });
    }

    this.resourceForm.markAsPristine();
    this.resourceForm.markAsUntouched();
  }

  public userIsSchedulerResourceReader(): boolean {
    return this.resourceService.userIsSchedulerResourceReader();
  }

  public userIsSchedulerResourceWriter(): boolean {
    return this.resourceService.userIsSchedulerResourceWriter();
  }

  public userIsSchedulerAdministrator(): boolean {
    return this.authService.isSchedulerAdministrator;
  }

  public userIsFoundationAdministrator(): boolean {
    return this.authService.isFoundationAdmin;
  }
}

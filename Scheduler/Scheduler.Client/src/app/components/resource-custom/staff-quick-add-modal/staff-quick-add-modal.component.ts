import { Component, Output, ViewChild, TemplateRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Subject, finalize, take } from 'rxjs';
import { ResourceService, ResourceData, ResourceSubmitData } from '../../../scheduler-data-services/resource.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CurrentUserService } from '../../../services/current-user.service';
import { ResourceTypeService } from '../../../scheduler-data-services/resource-type.service';
import { TerminologyService } from '../../../services/terminology.service';

@Component({
  selector: 'app-staff-quick-add-modal',
  templateUrl: './staff-quick-add-modal.component.html',
  styleUrls: ['./staff-quick-add-modal.component.scss']
})
export class StaffQuickAddModalComponent implements OnInit {
  @ViewChild('quickAddModal') quickAddModal!: TemplateRef<any>;
  @Output() resourceChanged = new Subject<ResourceData[]>();

  public isSaving = false;
  public modalIsDisplayed = false;
  private modalRef?: NgbModalRef;
  private defaultResourceTypeId: number = 1;

  quickAddForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: ['']
  });

  constructor(
    private modalService: NgbModal,
    private resourceService: ResourceService,
    private resourceTypeService: ResourceTypeService,
    private alertService: AlertService,
    private currentUserService: CurrentUserService,
    private fb: FormBuilder,
    public terminology: TerminologyService
  ) {}

  ngOnInit() {
    this.resourceTypeService.GetResourceTypeList().pipe(take(1)).subscribe((types: any) => {
      if (types && types.length > 0) {
        this.defaultResourceTypeId = types[0].id; // Use first available type
      }
    });
  }

  public openModal() {
    this.quickAddForm.reset();
    this.modalIsDisplayed = true;
    this.modalRef = this.modalService.open(this.quickAddModal, { backdrop: 'static', centered: true });
  }

  public closeModal() {
    this.modalIsDisplayed = false;
    this.modalRef?.dismiss();
  }

  public submitForm() {
    if (this.quickAddForm.invalid) {
      this.quickAddForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    const formValue = this.quickAddForm.value;

    const newResource: ResourceSubmitData = {
      id: 0,
      name: formValue.name,
      description: formValue.description,
      resourceTypeId: this.defaultResourceTypeId,
      timeZoneId: this.currentUserService.defaultTimeZoneId || 0,
      officeId: this.currentUserService.defaultOfficeId as any,
      versionNumber: 0,
      active: true,
      deleted: false,
      // required generics
      shiftPatternId: null,
      targetWeeklyWorkHours: null,
      notes: null,
      externalId: null,
      color: null,
      attributes: null,
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    };

    this.resourceService.PostResource(newResource).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (res: any) => {
        this.resourceService.ClearAllCaches();
        this.resourceChanged.next([res]);
        this.alertService.showMessage(`${this.terminology.getTerm('Resource')} added successfully`, '', MessageSeverity.success);
        this.closeModal();
      },
      error: (err: any) => {
        this.alertService.showMessage(`Could not save ${this.terminology.getTerm('Resource')}`, '', MessageSeverity.error);
      }
    });
  }
}

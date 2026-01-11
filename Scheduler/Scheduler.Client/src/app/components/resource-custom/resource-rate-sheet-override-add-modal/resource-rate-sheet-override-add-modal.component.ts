import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RateSheetService, RateSheetSubmitData } from '../../../scheduler-data-services/rate-sheet.service';
import { AssignmentRoleService, AssignmentRoleData } from '../../../scheduler-data-services/assignment-role.service';
import { CurrencyService, CurrencyData } from '../../../scheduler-data-services/currency.service';
import { RateTypeService, RateTypeData } from '../../../scheduler-data-services/rate-type.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-resource-rate-sheet-override-add-modal',
  templateUrl: './resource-rate-sheet-override-add-modal.component.html',
  styleUrls: ['./resource-rate-sheet-override-add-modal.component.scss']
})
export class ResourceRateOverrideAddModalComponent implements OnInit {
  @Input() resourceId!: number;
  @Input() resourceName!: string;

  public rateForm: FormGroup;
  public isSaving = false;

  // Dropdown data
  public assignmentRoles$: Observable<AssignmentRoleData[]>;
  public currencies$: Observable<CurrencyData[]>;
  public rateTypes$: Observable<RateTypeData[]>;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private rateSheetService: RateSheetService,
    private assignmentRoleService: AssignmentRoleService,
    private currencyService: CurrencyService,
    private rateTypeService: RateTypeService,
    private alertService: AlertService
  ) {
    this.rateForm = this.fb.group({
      rateTypeId: [null, Validators.required],
      assignmentRoleId: [null],
      effectiveDate: ['', Validators.required],
      costRate: [null, [Validators.required, Validators.min(0)]],
      billingRate: [null, [Validators.required, Validators.min(0)]],
      currencyId: [null, Validators.required],
      notes: ['']
    });

    // Load dropdowns
    this.assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList({ active: true });
    this.currencies$ = this.currencyService.GetCurrencyList({ active: true });
    this.rateTypes$ = this.rateTypeService.GetRateTypeList({ active: true });
  }

  ngOnInit(): void {


    // Default effective date to today
    const today = new Date().toISOString().split('T')[0];
    this.rateForm.patchValue({ effectiveDate: today });
  }

  public submit(): void {
    if (this.isSaving || !this.rateForm.valid) {
      return;
    }

    this.isSaving = true;

    const formValue = this.rateForm.value;

    const submitData: RateSheetSubmitData = {
      id: 0,
      resourceId: this.resourceId,
      assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
      effectiveDate: formValue.effectiveDate,
      costRate: Number(formValue.costRate),
      billingRate: Number(formValue.billingRate),
      currencyId: Number(formValue.currencyId),
      schedulingTargetId: null,
      rateTypeId: Number(formValue.rateTypeId),
      notes: formValue.notes,
      officeId: null,     // Not binding to the office here.  
      versionNumber: 1,
      active: true,
      deleted: false
    };

    this.rateSheetService.PostRateSheet(submitData).subscribe({
      next: (newRate) => {

        this.rateSheetService.ClearAllCaches();

        this.alertService.showMessage(
          'Rate override added successfully',
          '',
          MessageSeverity.success
        );
        this.activeModal.close(newRate);
      },
      error: (err) => {
        this.alertService.showMessage(
          'Failed to add rate override',
          err.message || 'Unknown error',
          MessageSeverity.error
        );
        this.isSaving = false;
      }
    });
  }

  public cancel(): void {
    this.activeModal.dismiss('cancel');
  }
}

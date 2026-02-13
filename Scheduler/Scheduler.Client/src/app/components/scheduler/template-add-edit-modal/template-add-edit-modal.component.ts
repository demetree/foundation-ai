/**
 *
 * TemplateAddEditModalComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * NgbModal form for creating and editing event templates.
 * Supports all ScheduledEventTemplateData fields.
 *
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import {
    ScheduledEventTemplateService,
    ScheduledEventTemplateData,
    ScheduledEventTemplateSubmitData
} from '../../../scheduler-data-services/scheduled-event-template.service';
import { PriorityService, PriorityData } from '../../../scheduler-data-services/priority.service';
import { SchedulingTargetTypeService, SchedulingTargetTypeData } from '../../../scheduler-data-services/scheduling-target-type.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { lastValueFrom } from 'rxjs';

@Component({
    selector: 'app-template-add-edit-modal',
    templateUrl: './template-add-edit-modal.component.html',
    styleUrls: ['./template-add-edit-modal.component.scss']
})
export class TemplateAddEditModalComponent implements OnInit {

    @Input() template: ScheduledEventTemplateData | null = null;

    templateForm!: FormGroup;
    isEditMode: boolean = false;
    saving: boolean = false;

    priorities: PriorityData[] = [];
    targetTypes: SchedulingTargetTypeData[] = [];


    constructor(
        public activeModal: NgbActiveModal,
        private fb: FormBuilder,
        private templateService: ScheduledEventTemplateService,
        private priorityService: PriorityService,
        private targetTypeService: SchedulingTargetTypeService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.isEditMode = !!this.template;
        this.buildForm();
        this.loadLookups();

        if (this.template) {
            this.populateForm(this.template);
        }
    }


    // -------------------------------------------------------------------------
    // Form Setup
    // -------------------------------------------------------------------------

    private buildForm(): void {
        this.templateForm = this.fb.group({
            name: ['', Validators.required],
            description: [''],
            defaultDurationMinutes: [60, [Validators.required, Validators.min(1)]],
            defaultAllDay: [false],
            schedulingTargetTypeId: [null],
            priorityId: [null],
            defaultLocationPattern: ['']
        });
    }

    private populateForm(t: ScheduledEventTemplateData): void {
        this.templateForm.patchValue({
            name: t.name,
            description: t.description || '',
            defaultDurationMinutes: Number(t.defaultDurationMinutes),
            defaultAllDay: t.defaultAllDay || false,
            schedulingTargetTypeId: t.schedulingTargetTypeId || null,
            priorityId: t.priorityId || null,
            defaultLocationPattern: t.defaultLocationPattern || ''
        });
    }


    // -------------------------------------------------------------------------
    // Lookup Loading
    // -------------------------------------------------------------------------

    private loadLookups(): void {
        this.priorityService.GetPriorityList({ active: true }).subscribe(p => {
            this.priorities = p;
        });
        this.targetTypeService.GetSchedulingTargetTypeList({ active: true }).subscribe(t => {
            this.targetTypes = t;
        });
    }


    // -------------------------------------------------------------------------
    // Duration Helpers (for hours/minutes split display)
    // -------------------------------------------------------------------------

    get durationHours(): number {
        const total = this.templateForm.get('defaultDurationMinutes')?.value || 0;
        return Math.floor(total / 60);
    }
    set durationHours(h: number) {
        const mins = this.durationMinutesPart;
        this.templateForm.patchValue({ defaultDurationMinutes: (h * 60) + mins });
    }

    get durationMinutesPart(): number {
        const total = this.templateForm.get('defaultDurationMinutes')?.value || 0;
        return total % 60;
    }
    set durationMinutesPart(m: number) {
        const hours = this.durationHours;
        this.templateForm.patchValue({ defaultDurationMinutes: (hours * 60) + m });
    }


    // -------------------------------------------------------------------------
    // Save
    // -------------------------------------------------------------------------

    async save(): Promise<void> {
        if (this.templateForm.invalid || this.saving) return;
        this.saving = true;

        try {
            const formVal = this.templateForm.value;

            const submitData = new ScheduledEventTemplateSubmitData();
            submitData.name = formVal.name.trim();
            submitData.description = formVal.description?.trim() || null;
            submitData.defaultDurationMinutes = formVal.defaultDurationMinutes;
            submitData.defaultAllDay = formVal.defaultAllDay || false;
            submitData.schedulingTargetTypeId = formVal.schedulingTargetTypeId || null;
            submitData.priorityId = formVal.priorityId || null;
            submitData.defaultLocationPattern = formVal.defaultLocationPattern?.trim() || null;
            submitData.active = true;
            submitData.deleted = false;

            if (this.isEditMode && this.template) {
                submitData.id = this.template.id;
                submitData.versionNumber = this.template.versionNumber;
                await lastValueFrom(this.templateService.PutScheduledEventTemplate(submitData.id, submitData));
            } else {
                submitData.id = 0 as any;
                submitData.versionNumber = 0 as any;
                await lastValueFrom(this.templateService.PostScheduledEventTemplate(submitData));
            }

            this.saving = false;
            this.alertService.showMessage(
                this.isEditMode ? 'Template updated' : 'Template created',
                '', MessageSeverity.success
            );
            this.activeModal.close(true);

        } catch (err: any) {
            this.saving = false;
            this.alertService.showMessage('Failed to save template', err?.message || '', MessageSeverity.error);
        }
    }
}

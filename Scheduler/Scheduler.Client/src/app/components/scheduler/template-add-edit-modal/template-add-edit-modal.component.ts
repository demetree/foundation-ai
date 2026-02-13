/**
 *
 * TemplateAddEditModalComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * NgbModal form for creating and editing event templates.
 * Supports all ScheduledEventTemplateData fields including qualification requirements.
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
import { QualificationService, QualificationData } from '../../../scheduler-data-services/qualification.service';
import {
    ScheduledEventTemplateQualificationRequirementService,
    ScheduledEventTemplateQualificationRequirementData,
    ScheduledEventTemplateQualificationRequirementSubmitData
} from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement.service';
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

    // Qualification requirements
    allQualifications: QualificationData[] = [];
    existingQualReqs: ScheduledEventTemplateQualificationRequirementData[] = [];
    pendingNewQualReqs: number[] = [];    // qualificationIds to add
    deletedQualReqIds: number[] = [];     // existing requirement IDs to remove
    selectedNewQualId: number | null = null;


    constructor(
        public activeModal: NgbActiveModal,
        private fb: FormBuilder,
        private templateService: ScheduledEventTemplateService,
        private priorityService: PriorityService,
        private targetTypeService: SchedulingTargetTypeService,
        private qualificationService: QualificationService,
        private templateQualReqService: ScheduledEventTemplateQualificationRequirementService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.isEditMode = !!this.template;
        this.buildForm();
        this.loadLookups();

        if (this.template) {
            this.populateForm(this.template);
            this.loadExistingQualReqs();
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
        this.qualificationService.GetQualificationList({ active: true }).subscribe(q => {
            this.allQualifications = q;
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
    // Qualification Requirements
    // -------------------------------------------------------------------------

    private loadExistingQualReqs(): void {
        if (!this.template?.id) return;
        this.templateQualReqService.GetScheduledEventTemplateQualificationRequirementList({
            scheduledEventTemplateId: this.template.id,
            active: true
        }).subscribe({
            next: (reqs) => {
                this.existingQualReqs = reqs;
            },
            error: () => {
                this.existingQualReqs = [];
            }
        });
    }

    addQualReq(): void {
        if (!this.selectedNewQualId) return;
        const qualId = Number(this.selectedNewQualId);

        // Avoid duplicates
        const alreadyExists = this.existingQualReqs.some(
            r => Number(r.qualificationId) === qualId && !this.deletedQualReqIds.includes(Number(r.id))
        );
        const alreadyPending = this.pendingNewQualReqs.includes(qualId);
        if (alreadyExists || alreadyPending) return;

        this.pendingNewQualReqs.push(qualId);
        this.selectedNewQualId = null;
    }

    removeExistingQualReq(reqId: number): void {
        this.deletedQualReqIds.push(reqId);
        this.existingQualReqs = this.existingQualReqs.filter(r => Number(r.id) !== reqId);
    }

    removePendingQualReq(index: number): void {
        this.pendingNewQualReqs.splice(index, 1);
    }

    getQualificationName(qualId: number): string {
        const qual = this.allQualifications.find(q => Number(q.id) === qualId);
        return qual?.name || `Qualification #${qualId}`;
    }

    private async handleQualReqsSave(templateId: number | bigint): Promise<void> {
        // Delete removed requirements
        for (const reqId of this.deletedQualReqIds) {
            await lastValueFrom(
                this.templateQualReqService.DeleteScheduledEventTemplateQualificationRequirement(reqId)
            );
        }

        // Create new requirements
        for (const qualId of this.pendingNewQualReqs) {
            const submitData: ScheduledEventTemplateQualificationRequirementSubmitData = {
                id: 0 as any,
                scheduledEventTemplateId: templateId as any,
                qualificationId: qualId,
                isRequired: true,
                versionNumber: 0 as any,
                active: true,
                deleted: false
            };
            await lastValueFrom(
                this.templateQualReqService.PostScheduledEventTemplateQualificationRequirement(submitData)
            );
        }

        this.deletedQualReqIds = [];
        this.pendingNewQualReqs = [];
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

            let savedTemplate: ScheduledEventTemplateData;

            if (this.isEditMode && this.template) {
                submitData.id = this.template.id;
                submitData.versionNumber = this.template.versionNumber;
                savedTemplate = await lastValueFrom(this.templateService.PutScheduledEventTemplate(submitData.id, submitData));
            } else {
                submitData.id = 0 as any;
                submitData.versionNumber = 0 as any;
                savedTemplate = await lastValueFrom(this.templateService.PostScheduledEventTemplate(submitData));
            }

            // Persist qualification requirements
            await this.handleQualReqsSave(savedTemplate.id);

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

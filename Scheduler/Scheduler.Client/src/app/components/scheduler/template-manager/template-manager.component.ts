/**
 *
 * TemplateManagerComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Standalone page for managing event templates. Provides:
 *   - Card grid of all active templates
 *   - Search / filter
 *   - Create / Edit / Delete via modal
 *
 */

import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import {
    ScheduledEventTemplateService,
    ScheduledEventTemplateData
} from '../../../scheduler-data-services/scheduled-event-template.service';
import { PriorityService, PriorityData } from '../../../scheduler-data-services/priority.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TemplateAddEditModalComponent } from '../template-add-edit-modal/template-add-edit-modal.component';

@Component({
    selector: 'app-template-manager',
    templateUrl: './template-manager.component.html',
    styleUrls: ['./template-manager.component.scss']
})
export class TemplateManagerComponent implements OnInit {

    templates: ScheduledEventTemplateData[] = [];
    filteredTemplates: ScheduledEventTemplateData[] = [];
    priorities: PriorityData[] = [];
    searchTerm: string = '';
    loading: boolean = false;


    constructor(
        private templateService: ScheduledEventTemplateService,
        private priorityService: PriorityService,
        private alertService: AlertService,
        private modalService: NgbModal
    ) { }


    ngOnInit(): void {
        this.loadTemplates();
        this.loadPriorities();
    }


    // -------------------------------------------------------------------------
    // Data Loading
    // -------------------------------------------------------------------------

    loadTemplates(): void {
        this.loading = true;
        this.templateService.ClearAllCaches();
        this.templateService.GetScheduledEventTemplateList({
            active: true,
            deleted: false,
            includeRelations: true
        }).subscribe({
            next: (data) => {
                this.templates = data;
                this.applyFilter();
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.alertService.showMessage('Failed to load templates', '', MessageSeverity.error);
            }
        });
    }


    loadPriorities(): void {
        this.priorityService.GetPriorityList({ active: true }).subscribe(p => {
            this.priorities = p;
        });
    }


    // -------------------------------------------------------------------------
    // Filtering
    // -------------------------------------------------------------------------

    applyFilter(): void {
        const term = this.searchTerm.toLowerCase().trim();
        if (!term) {
            this.filteredTemplates = [...this.templates];
        } else {
            this.filteredTemplates = this.templates.filter(t =>
                t.name.toLowerCase().includes(term) ||
                (t.description && t.description.toLowerCase().includes(term))
            );
        }
    }


    // -------------------------------------------------------------------------
    // CRUD Actions
    // -------------------------------------------------------------------------

    openCreateModal(): void {
        const modalRef = this.modalService.open(TemplateAddEditModalComponent, { size: 'lg' });
        modalRef.result.then((saved) => {
            if (saved) this.loadTemplates();
        }, () => { });
    }


    openEditModal(template: ScheduledEventTemplateData): void {
        const modalRef = this.modalService.open(TemplateAddEditModalComponent, { size: 'lg' });
        modalRef.componentInstance.template = template;
        modalRef.result.then((saved) => {
            if (saved) this.loadTemplates();
        }, () => { });
    }


    deleteTemplate(template: ScheduledEventTemplateData, event: Event): void {
        event.stopPropagation();

        if (!confirm(`Delete template "${template.name}"?`)) return;

        const submitData = template.ConvertToSubmitData();
        submitData.active = false;
        submitData.deleted = true;

        this.templateService.PutScheduledEventTemplate(template.id, submitData).subscribe({
            next: () => {
                this.alertService.showMessage('Template deleted', '', MessageSeverity.success);
                this.loadTemplates();
            },
            error: () => {
                this.alertService.showMessage('Failed to delete template', '', MessageSeverity.error);
            }
        });
    }


    // -------------------------------------------------------------------------
    // Display Helpers
    // -------------------------------------------------------------------------

    formatDuration(minutes: number | bigint): string {
        const m = Number(minutes);
        if (m < 60) return `${m}m`;
        const h = Math.floor(m / 60);
        const remainder = m % 60;
        return remainder > 0 ? `${h}h ${remainder}m` : `${h}h`;
    }

    getPriorityName(id: number | bigint): string {
        const p = this.priorities.find(p => p.id === id);
        return p?.name || '';
    }
}

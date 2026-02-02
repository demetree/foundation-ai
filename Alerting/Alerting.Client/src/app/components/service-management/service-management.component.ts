import { Component, OnInit, OnDestroy, ViewChild, TemplateRef } from '@angular/core';
import { Subject, BehaviorSubject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { ServiceService, ServiceData, ServiceSubmitData, ServiceQueryParameters } from '../../alerting-data-services/service.service';
import { EscalationPolicyService, EscalationPolicyData } from '../../alerting-data-services/escalation-policy.service';
import { IntegrationData } from '../../alerting-data-services/integration.service';
import { AlertService } from '../../services/alert.service';

@Component({
    selector: 'app-service-management',
    templateUrl: './service-management.component.html',
    styleUrls: ['./service-management.component.scss']
})
export class ServiceManagementComponent implements OnInit, OnDestroy {
    @ViewChild('addEditModal') addEditModalTemplate!: TemplateRef<unknown>;

    // Data
    services: ServiceData[] = [];
    escalationPolicies: EscalationPolicyData[] = [];

    // State
    isLoading = true;
    loadingFilteredCount = false;
    errorMessage: string | null = null;

    // Pagination
    currentPage = 1;
    pageSize = 10;
    totalPages = 1;
    totalCount$ = new BehaviorSubject<number>(0);
    filteredCount$ = new BehaviorSubject<number>(0);

    // Filter
    filterText = '';
    private filterSubject = new Subject<string>();

    // Modal
    private modalRef: NgbModalRef | null = null;
    isAddMode = true;
    editingService: ServiceData | null = null;
    isSaving = false;

    // Form fields
    formName = '';
    formDescription = '';
    formEscalationPolicyId: number | null = null;
    formActive = true;

    // Integration counts
    integrationCounts: Map<number | bigint, number> = new Map();

    private destroy$ = new Subject<void>();

    // For template access
    Math = Math;

    constructor(
        private serviceService: ServiceService,
        private escalationPolicyService: EscalationPolicyService,
        private alertService: AlertService,
        private modalService: NgbModal
    ) { }

    ngOnInit(): void {
        this.loadServices();
        this.loadEscalationPolicies();
        this.setupFilterDebounce();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.modalRef?.close();
    }

    private setupFilterDebounce(): void {
        this.filterSubject
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                takeUntil(this.destroy$)
            )
            .subscribe(filter => {
                this.filterText = filter;
                this.currentPage = 1;
                this.loadServices();
                this.loadFilteredCount(filter);
            });
    }

    loadServices(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const params: Partial<ServiceQueryParameters> = {
            active: true,
            deleted: false,
            pageSize: this.pageSize,
            pageNumber: this.currentPage,
            includeRelations: true,
            anyStringContains: this.filterText?.trim() || null
        };

        this.serviceService.GetServiceList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (services) => {
                    this.services = services;
                    this.isLoading = false;
                    this.loadTotalCount();
                    this.loadIntegrationCounts(services);
                },
                error: (err) => {
                    console.error('Error loading services:', err);
                    this.errorMessage = 'Failed to load services. Please try again.';
                    this.isLoading = false;
                }
            });
    }

    private loadTotalCount(): void {
        this.serviceService.GetServicesRowCount({ active: true, deleted: false })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    const total = Number(count);
                    this.totalCount$.next(total);
                    if (!this.filterText) {
                        this.filteredCount$.next(total);
                    }
                    this.totalPages = Math.ceil(total / this.pageSize);
                }
            });
    }

    private loadFilteredCount(filterText: string): void {
        if (!filterText || filterText.trim() === '') {
            const totalCount = this.totalCount$.getValue();
            this.filteredCount$.next(totalCount);
            this.totalPages = Math.ceil(totalCount / this.pageSize);
            return;
        }

        this.loadingFilteredCount = true;

        this.serviceService.GetServicesRowCount({
            active: true,
            deleted: false,
            anyStringContains: filterText.trim()
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    const filtered = Number(count);
                    this.filteredCount$.next(filtered);
                    this.totalPages = Math.ceil(filtered / this.pageSize);
                    this.loadingFilteredCount = false;
                },
                error: () => {
                    this.loadingFilteredCount = false;
                }
            });
    }

    private loadEscalationPolicies(): void {
        this.escalationPolicyService.GetEscalationPolicyList({ active: true, deleted: false })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (policies) => {
                    this.escalationPolicies = policies;
                }
            });
    }

    private loadIntegrationCounts(services: ServiceData[]): void {
        services.forEach(service => {
            service.HasIntegrations.then((has: boolean) => {
                if (has) {
                    service.Integrations.then((integrations: IntegrationData[]) => {
                        this.integrationCounts.set(service.id, integrations.length);
                    });
                } else {
                    this.integrationCounts.set(service.id, 0);
                }
            });
        });
    }

    // Filter
    onFilterChange(value: string): void {
        this.filterSubject.next(value);
    }

    clearFilter(): void {
        this.filterText = '';
        this.filterSubject.next('');
    }

    // Pagination
    onPageChange(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
            this.loadServices();
        }
    }

    // Status helpers
    getStatusBadgeClass(service: ServiceData): string {
        if (service.deleted) return 'badge-deleted';
        return service.active ? 'badge-active' : 'badge-inactive';
    }

    getStatusText(service: ServiceData): string {
        if (service.deleted) return 'Deleted';
        return service.active ? 'Active' : 'Inactive';
    }

    getIntegrationCount(service: ServiceData): number {
        return this.integrationCounts.get(service.id) || 0;
    }

    // Modal operations
    openAddModal(): void {
        this.isAddMode = true;
        this.editingService = null;
        this.resetForm();
        this.modalRef = this.modalService.open(this.addEditModalTemplate, {
            size: 'lg',
            backdrop: 'static'
        });
    }

    openEditModal(service: ServiceData): void {
        this.isAddMode = false;
        this.editingService = service;
        this.formName = service.name;
        this.formDescription = service.description || '';
        this.formEscalationPolicyId = service.escalationPolicyId ? Number(service.escalationPolicyId) : null;
        this.formActive = service.active;
        this.modalRef = this.modalService.open(this.addEditModalTemplate, {
            size: 'lg',
            backdrop: 'static'
        });
    }

    closeModal(): void {
        this.modalRef?.close();
        this.modalRef = null;
        this.resetForm();
    }

    private resetForm(): void {
        this.formName = '';
        this.formDescription = '';
        this.formEscalationPolicyId = null;
        this.formActive = true;
        this.isSaving = false;
    }

    saveService(): void {
        if (!this.formName?.trim()) {
            this.alertService.showErrorMessage('Validation Error', 'Service name is required');
            return;
        }

        this.isSaving = true;

        if (this.isAddMode) {
            this.createService();
        } else {
            this.updateService();
        }
    }

    private createService(): void {
        const submitData: ServiceSubmitData = {
            id: 0,
            name: this.formName.trim(),
            description: this.formDescription?.trim() || null,
            escalationPolicyId: this.formEscalationPolicyId,
            ownerTeamObjectGuid: null,
            versionNumber: 0,
            active: this.formActive,
            deleted: false
        };

        this.serviceService.PostService(submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.alertService.showSuccessMessage('Success', 'Service created successfully');
                    this.closeModal();
                    this.loadServices();
                },
                error: (err) => {
                    console.error('Error creating service:', err);
                    this.alertService.showErrorMessage('Error', 'Failed to create service');
                    this.isSaving = false;
                }
            });
    }

    private updateService(): void {
        if (!this.editingService) return;

        const submitData: ServiceSubmitData = {
            id: Number(this.editingService.id),
            name: this.formName.trim(),
            description: this.formDescription?.trim() || null,
            escalationPolicyId: this.formEscalationPolicyId,
            ownerTeamObjectGuid: this.editingService.ownerTeamObjectGuid || null,
            versionNumber: Number(this.editingService.versionNumber),
            active: this.formActive,
            deleted: this.editingService.deleted
        };

        this.serviceService.PutService(submitData.id, submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.alertService.showSuccessMessage('Success', 'Service updated successfully');
                    this.closeModal();
                    this.loadServices();
                },
                error: (err) => {
                    console.error('Error updating service:', err);
                    this.alertService.showErrorMessage('Error', 'Failed to update service');
                    this.isSaving = false;
                }
            });
    }

    deleteService(service: ServiceData): void {
        if (!confirm(`Are you sure you want to delete "${service.name}"? This will also affect any integrations using this service.`)) {
            return;
        }

        const submitData: ServiceSubmitData = {
            id: Number(service.id),
            name: service.name,
            description: service.description,
            escalationPolicyId: service.escalationPolicyId ? Number(service.escalationPolicyId) : null,
            ownerTeamObjectGuid: service.ownerTeamObjectGuid || null,
            versionNumber: Number(service.versionNumber),
            active: false,
            deleted: true
        };

        this.serviceService.PutService(submitData.id, submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.alertService.showSuccessMessage('Success', 'Service deleted successfully');
                    this.loadServices();
                },
                error: (err) => {
                    console.error('Error deleting service:', err);
                    this.alertService.showErrorMessage('Error', 'Failed to delete service');
                }
            });
    }

    // Track by
    trackById(index: number, item: ServiceData): number | bigint {
        return item.id;
    }
}

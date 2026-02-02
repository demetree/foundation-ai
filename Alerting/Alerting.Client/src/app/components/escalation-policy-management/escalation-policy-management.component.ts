import { Component, OnInit, OnDestroy, ViewChild, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, BehaviorSubject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { EscalationPolicyService, EscalationPolicyData, EscalationPolicySubmitData, EscalationPolicyQueryParameters } from '../../alerting-data-services/escalation-policy.service';
import { ServiceData } from '../../alerting-data-services/service.service';
import { AlertService } from '../../services/alert.service';

@Component({
    selector: 'app-escalation-policy-management',
    templateUrl: './escalation-policy-management.component.html',
    styleUrls: ['./escalation-policy-management.component.scss']
})
export class EscalationPolicyManagementComponent implements OnInit, OnDestroy {
    @ViewChild('addEditModal') addEditModalTemplate!: TemplateRef<unknown>;

    // Data
    policies: EscalationPolicyData[] = [];

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
    editingPolicy: EscalationPolicyData | null = null;
    isSaving = false;

    // Form fields
    formName = '';
    formDescription = '';
    formActive = true;

    // Service counts
    serviceCounts: Map<number | bigint, number> = new Map();
    // Rule counts
    ruleCounts: Map<number | bigint, number> = new Map();

    private destroy$ = new Subject<void>();

    // For template access
    Math = Math;

    constructor(
        private router: Router,
        private escalationPolicyService: EscalationPolicyService,
        private alertService: AlertService,
        private modalService: NgbModal
    ) { }

    ngOnInit(): void {
        this.loadPolicies();
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
                this.loadPolicies();
                this.loadFilteredCount(filter);
            });
    }

    loadPolicies(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const params: Partial<EscalationPolicyQueryParameters> = {
            active: true,
            deleted: false,
            pageSize: this.pageSize,
            pageNumber: this.currentPage,
            includeRelations: true,
            anyStringContains: this.filterText?.trim() || null
        };

        this.escalationPolicyService.GetEscalationPolicyList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (policies) => {
                    this.policies = policies;
                    this.isLoading = false;
                    this.loadTotalCount();
                    this.loadRelatedCounts(policies);
                },
                error: (err) => {
                    console.error('Error loading escalation policies:', err);
                    this.errorMessage = 'Failed to load escalation policies. Please try again.';
                    this.isLoading = false;
                }
            });
    }

    private loadTotalCount(): void {
        this.escalationPolicyService.GetEscalationPoliciesRowCount({ active: true, deleted: false })
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

        this.escalationPolicyService.GetEscalationPoliciesRowCount({
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

    private loadRelatedCounts(policies: EscalationPolicyData[]): void {
        policies.forEach(policy => {
            // Load service counts
            policy.HasServices.then((has: boolean) => {
                if (has) {
                    policy.Services.then((services: ServiceData[]) => {
                        this.serviceCounts.set(policy.id, services.length);
                    });
                } else {
                    this.serviceCounts.set(policy.id, 0);
                }
            });

            // Load rule counts from the observable
            policy.EscalationRulesCount$.pipe(takeUntil(this.destroy$)).subscribe({
                next: (count) => {
                    this.ruleCounts.set(policy.id, Number(count));
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
            this.loadPolicies();
        }
    }

    // Status helpers
    getStatusBadgeClass(policy: EscalationPolicyData): string {
        if (policy.deleted) return 'badge-deleted';
        return policy.active ? 'badge-active' : 'badge-inactive';
    }

    getStatusText(policy: EscalationPolicyData): string {
        if (policy.deleted) return 'Deleted';
        return policy.active ? 'Active' : 'Inactive';
    }

    getServiceCount(policy: EscalationPolicyData): number {
        return this.serviceCounts.get(policy.id) || 0;
    }

    getRuleCount(policy: EscalationPolicyData): number {
        return this.ruleCounts.get(policy.id) || 0;
    }

    // Modal operations
    openAddModal(): void {
        this.isAddMode = true;
        this.editingPolicy = null;
        this.resetForm();
        this.modalRef = this.modalService.open(this.addEditModalTemplate, {
            size: 'lg',
            backdrop: 'static'
        });
    }

    openEditModal(policy: EscalationPolicyData): void {
        // Navigate to full-page editor instead of modal
        this.router.navigate(['/escalation-policy-management', policy.id, 'edit']);
    }

    closeModal(): void {
        this.modalRef?.close();
        this.modalRef = null;
        this.resetForm();
    }

    private resetForm(): void {
        this.formName = '';
        this.formDescription = '';
        this.formActive = true;
        this.isSaving = false;
    }

    savePolicy(): void {
        if (!this.formName?.trim()) {
            this.alertService.showErrorMessage('Validation Error', 'Policy name is required');
            return;
        }

        this.isSaving = true;

        if (this.isAddMode) {
            this.createPolicy();
        } else {
            this.updatePolicy();
        }
    }

    private createPolicy(): void {
        const submitData: EscalationPolicySubmitData = {
            id: 0,
            name: this.formName.trim(),
            description: this.formDescription?.trim() || null,
            versionNumber: 0,
            active: this.formActive,
            deleted: false
        };

        this.escalationPolicyService.PostEscalationPolicy(submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.alertService.showSuccessMessage('Success', 'Escalation policy created successfully');
                    this.closeModal();
                    this.loadPolicies();
                },
                error: (err) => {
                    console.error('Error creating escalation policy:', err);
                    this.alertService.showErrorMessage('Error', 'Failed to create escalation policy');
                    this.isSaving = false;
                }
            });
    }

    private updatePolicy(): void {
        if (!this.editingPolicy) return;

        const submitData: EscalationPolicySubmitData = {
            id: Number(this.editingPolicy.id),
            name: this.formName.trim(),
            description: this.formDescription?.trim() || null,
            versionNumber: Number(this.editingPolicy.versionNumber),
            active: this.formActive,
            deleted: this.editingPolicy.deleted
        };

        this.escalationPolicyService.PutEscalationPolicy(submitData.id, submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.alertService.showSuccessMessage('Success', 'Escalation policy updated successfully');
                    this.closeModal();
                    this.loadPolicies();
                },
                error: (err) => {
                    console.error('Error updating escalation policy:', err);
                    this.alertService.showErrorMessage('Error', 'Failed to update escalation policy');
                    this.isSaving = false;
                }
            });
    }

    deletePolicy(policy: EscalationPolicyData): void {
        if (!confirm(`Are you sure you want to delete "${policy.name}"? Services using this policy will be affected.`)) {
            return;
        }

        const submitData: EscalationPolicySubmitData = {
            id: Number(policy.id),
            name: policy.name,
            description: policy.description,
            versionNumber: Number(policy.versionNumber),
            active: false,
            deleted: true
        };

        this.escalationPolicyService.PutEscalationPolicy(submitData.id, submitData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.alertService.showSuccessMessage('Success', 'Escalation policy deleted successfully');
                    this.loadPolicies();
                },
                error: (err) => {
                    console.error('Error deleting escalation policy:', err);
                    this.alertService.showErrorMessage('Error', 'Failed to delete escalation policy');
                }
            });
    }

    // Track by
    trackById(index: number, item: EscalationPolicyData): number | bigint {
        return item.id;
    }
}

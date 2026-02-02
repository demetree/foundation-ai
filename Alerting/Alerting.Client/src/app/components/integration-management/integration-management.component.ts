//
// Integration Management Component
//
// Premium listing and management view for Alerting Integrations with API key reveal.
//

import { Component, OnInit, OnDestroy, HostListener, ViewChild, TemplateRef } from '@angular/core';
import { Subject, BehaviorSubject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { IntegrationService, IntegrationData, IntegrationQueryParameters } from '../../alerting-data-services/integration.service';
import { ServiceService, ServiceData } from '../../alerting-data-services/service.service';
import { AlertService } from '../../services/alert.service';

@Component({
    selector: 'app-integration-management',
    templateUrl: './integration-management.component.html',
    styleUrls: ['./integration-management.component.scss']
})
export class IntegrationManagementComponent implements OnInit, OnDestroy {

    //
    // Lifecycle management
    //
    private destroy$ = new Subject<void>();

    //
    // Loading and counts
    //
    public isLoading: boolean = true;
    public loadingFilteredCount: boolean = false;
    public totalCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);
    public filteredCount$: BehaviorSubject<number | null> = new BehaviorSubject<number | null>(null);
    public errorMessage: string | null = null;

    //
    // Data
    //
    public integrations: IntegrationData[] = [];
    public services: ServiceData[] = [];

    //
    // Filter state
    //
    public filterText: string = '';
    private filterTextSubject = new Subject<string>();

    //
    // Pagination
    //
    public currentPage: number = 1;
    public pageSize: number = 20;
    public Math = Math;

    //
    // Responsive state
    //
    public isSmallScreen: boolean = false;
    private readonly SMALL_SCREEN_BREAKPOINT = 768;

    //
    // Modal state
    //
    @ViewChild('addEditModal') addEditModal!: TemplateRef<any>;
    private modalRef: NgbModalRef | null = null;
    public isAddMode: boolean = true;
    public editingIntegration: IntegrationData | null = null;
    public isSaving: boolean = false;

    // Form fields
    public formName: string = '';
    public formDescription: string = '';
    public formServiceId: number | null = null;
    public formWebhookUrl: string = '';
    public formActive: boolean = true;

    // API Key reveal
    public revealedApiKeyId: number | null = null;
    public generatedApiKey: string | null = null;


    constructor(
        private integrationService: IntegrationService,
        private serviceService: ServiceService,
        private alertService: AlertService,
        private modalService: NgbModal
    ) { }


    ngOnInit(): void {
        this.checkScreenSize();
        this.setupFilterDebounce();
        this.loadServices();
        this.loadIntegrations();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    @HostListener('window:resize')
    onResize(): void {
        this.checkScreenSize();
    }


    private checkScreenSize(): void {
        this.isSmallScreen = window.innerWidth < this.SMALL_SCREEN_BREAKPOINT;
    }


    //
    // Data Loading
    //

    private loadServices(): void {
        this.serviceService.GetServiceList({ active: true, deleted: false })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (services) => {
                    this.services = services;
                },
                error: (err) => {
                    console.error('Error loading services:', err);
                }
            });
    }


    loadIntegrations(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const params: any = {
            active: true,
            deleted: false,
            pageSize: this.pageSize,
            pageNumber: this.currentPage,
            includeRelations: true
        };

        if (this.filterText && this.filterText.trim()) {
            params.anyStringContains = this.filterText.trim();
        }

        this.integrationService.GetIntegrationList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (integrations) => {
                    this.integrations = integrations;
                    this.isLoading = false;
                    this.loadTotalCount();
                },
                error: (err) => {
                    console.error('Error loading integrations:', err);
                    this.errorMessage = 'Failed to load integrations. Please try again.';
                    this.isLoading = false;
                }
            });
    }


    private loadTotalCount(): void {
        const params: any = { active: true, deleted: false };

        if (this.filterText && this.filterText.trim()) {
            params.anyStringContains = this.filterText.trim();
        }

        this.integrationService.GetIntegrationsRowCount(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    const numCount = Number(count);
                    this.filteredCount$.next(numCount);
                    if (!this.filterText) {
                        this.totalCount$.next(numCount);
                    }
                },
                error: (err) => {
                    console.error('Error loading integration count:', err);
                }
            });
    }


    private setupFilterDebounce(): void {
        this.filterTextSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(filterText => {
            this.currentPage = 1;
            this.loadIntegrations();
        });
    }


    //
    // Filter handling
    //

    onFilterChange(filterText: string): void {
        this.filterText = filterText;
        this.filterTextSubject.next(filterText);
    }


    clearFilter(): void {
        this.filterText = '';
        this.filterTextSubject.next('');
    }


    //
    // Pagination
    //

    onPageChange(page: number): void {
        this.currentPage = page;
        this.loadIntegrations();
    }


    get totalPages(): number {
        const count = this.filteredCount$.getValue() ?? 0;
        return Math.ceil(count / this.pageSize);
    }


    //
    // Modal handling
    //

    openAddModal(): void {
        this.isAddMode = true;
        this.editingIntegration = null;
        this.resetForm();
        this.modalRef = this.modalService.open(this.addEditModal, {
            size: 'md',
            backdrop: 'static',
            centered: true
        });
    }


    openEditModal(integration: IntegrationData): void {
        this.isAddMode = false;
        this.editingIntegration = integration;
        this.formName = integration.name;
        this.formDescription = integration.description || '';
        this.formServiceId = Number(integration.serviceId);
        this.formWebhookUrl = integration.callbackWebhookUrl || '';
        this.formActive = integration.active;
        this.modalRef = this.modalService.open(this.addEditModal, {
            size: 'md',
            backdrop: 'static',
            centered: true
        });
    }


    closeModal(): void {
        if (this.modalRef) {
            this.modalRef.close();
            this.modalRef = null;
        }
        this.resetForm();
    }


    private resetForm(): void {
        this.formName = '';
        this.formDescription = '';
        this.formServiceId = this.services.length > 0 ? Number(this.services[0].id) : null;
        this.formWebhookUrl = '';
        this.formActive = true;
        this.generatedApiKey = null;
    }


    saveIntegration(): void {
        if (!this.formName.trim() || !this.formServiceId) {
            this.alertService.showErrorMessage('Error', 'Name and Service are required');
            return;
        }

        this.isSaving = true;

        if (this.isAddMode) {
            // Generate the plain API key to show to the user
            const plainApiKey = this.generateApiKey();

            // Hash the API key with SHA256 before storing (matches backend validation)
            this.hashApiKey(plainApiKey).then(hashedKey => {
                // Create new integration with hashed key
                const newIntegration: any = {
                    id: 0,
                    serviceId: this.formServiceId,
                    name: this.formName.trim(),
                    description: this.formDescription.trim() || null,
                    apiKeyHash: hashedKey, // Store the HASHED key
                    callbackWebhookUrl: this.formWebhookUrl.trim() || null,
                    versionNumber: 0,
                    active: this.formActive,
                    deleted: false
                };

                // Store the PLAIN API key to show to user (only shown once on creation)
                this.generatedApiKey = plainApiKey;

                this.integrationService.PostIntegration(newIntegration)
                    .pipe(takeUntil(this.destroy$))
                    .subscribe({
                        next: (created) => {
                            this.alertService.showSuccessMessage('Success', 'Integration created successfully');
                            // Keep modal open to show the API key
                            this.isAddMode = false;
                            this.editingIntegration = created;
                            this.isSaving = false;
                            this.loadIntegrations();
                        },
                        error: (err) => {
                            console.error('Error creating integration:', err);
                            this.alertService.showErrorMessage('Error', 'Failed to create integration');
                            this.isSaving = false;
                        }
                    });
            }).catch(err => {
                console.error('Error hashing API key:', err);
                this.alertService.showErrorMessage('Error', 'Failed to generate API key');
                this.isSaving = false;
            });
        } else {
            // Update existing integration
            const updateData: any = {
                id: Number(this.editingIntegration!.id),
                serviceId: this.formServiceId,
                name: this.formName.trim(),
                description: this.formDescription.trim() || null,
                apiKeyHash: this.editingIntegration!.apiKeyHash,
                callbackWebhookUrl: this.formWebhookUrl.trim() || null,
                versionNumber: Number(this.editingIntegration!.versionNumber),
                active: this.formActive,
                deleted: false
            };

            this.integrationService.PutIntegration(updateData.id, updateData)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                    next: () => {
                        this.alertService.showSuccessMessage('Success', 'Integration updated successfully');
                        this.closeModal();
                        this.loadIntegrations();
                    },
                    error: (err) => {
                        console.error('Error updating integration:', err);
                        this.alertService.showErrorMessage('Error', 'Failed to update integration');
                        this.isSaving = false;
                    }
                });
        }
    }


    //
    // API Key handling
    //

    private generateApiKey(): string {
        // Generate a random API key (32 character hex string)
        const array = new Uint8Array(16);
        crypto.getRandomValues(array);
        return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
    }


    /**
     * Hash an API key using SHA-256 and return Base64 encoded string.
     * This matches the backend HashApiKey() method in AlertingService.
     */
    private async hashApiKey(apiKey: string): Promise<string> {
        const encoder = new TextEncoder();
        const data = encoder.encode(apiKey);
        const hashBuffer = await crypto.subtle.digest('SHA-256', data);
        const hashArray = new Uint8Array(hashBuffer);
        // Convert to Base64 (matches C# Convert.ToBase64String())
        return btoa(String.fromCharCode(...hashArray));
    }


    copyApiKey(apiKey: string): void {
        navigator.clipboard.writeText(apiKey).then(() => {
            this.alertService.showSuccessMessage('Success', 'API Key copied to clipboard');
        }).catch(() => {
            this.alertService.showErrorMessage('Error', 'Failed to copy API Key');
        });
    }


    toggleApiKeyReveal(integration: IntegrationData): void {
        if (this.revealedApiKeyId === Number(integration.id)) {
            this.revealedApiKeyId = null;
        } else {
            this.revealedApiKeyId = Number(integration.id);
        }
    }


    //
    // Delete handling
    //

    deleteIntegration(integration: IntegrationData): void {
        if (!confirm(`Are you sure you want to delete "${integration.name}"? This will invalidate the API key.`)) {
            return;
        }

        const deleteData: any = {
            id: Number(integration.id),
            serviceId: Number(integration.serviceId),
            name: integration.name,
            description: integration.description,
            apiKeyHash: integration.apiKeyHash,
            callbackWebhookUrl: integration.callbackWebhookUrl,
            versionNumber: Number(integration.versionNumber),
            active: false,
            deleted: true
        };

        this.integrationService.PutIntegration(deleteData.id, deleteData)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.alertService.showSuccessMessage('Success', 'Integration deleted successfully');
                    this.loadIntegrations();
                },
                error: (err) => {
                    console.error('Error deleting integration:', err);
                    this.alertService.showErrorMessage('Error', 'Failed to delete integration');
                }
            });
    }


    //
    // Display helpers
    //

    getStatusBadgeClass(integration: IntegrationData): string {
        if (integration.deleted) {
            return 'badge-deleted';
        }
        return integration.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(integration: IntegrationData): string {
        if (integration.deleted) {
            return 'Deleted';
        }
        return integration.active ? 'Active' : 'Inactive';
    }


    trackById(index: number, integration: IntegrationData): number | bigint {
        return integration.id;
    }
}

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
import { IncidentEventTypeService, IncidentEventTypeData } from '../../alerting-data-services/incident-event-type.service';
import { IntegrationManagementService, CreateIntegrationRequest, UpdateIntegrationRequest, IntegrationDto } from '../../services/integration-management.service';
import { AlertService } from '../../services/alert.service';
import { NavigationService } from '../../utility-services/navigation.service';

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
  public incidentEventTypes: IncidentEventTypeData[] = [];

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
  public formMaxRetryAttempts: number | null = null;
  public formRetryBackoffSeconds: number | null = null;
  public formCallbackEventTypeIds: number[] = [];

  // API Key reveal
  public revealedApiKeyId: number | null = null;
  public generatedApiKey: string | null = null;


  constructor(
    private integrationService: IntegrationService,
    private integrationManagementService: IntegrationManagementService,
    private serviceService: ServiceService,
    private incidentEventTypeService: IncidentEventTypeService,
    private alertService: AlertService,
    private modalService: NgbModal,
    private navigationService: NavigationService
  ) { }


  ngOnInit(): void {
    this.checkScreenSize();
    this.setupFilterDebounce();
    this.loadServices();
    this.loadIncidentEventTypes();
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


  private loadIncidentEventTypes(): void {
    this.incidentEventTypeService.GetIncidentEventTypeList({ active: true, deleted: false })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (eventTypes) => {
          this.incidentEventTypes = eventTypes;
        },
        error: (err) => {
          console.error('Error loading incident event types:', err);
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
    this.isSaving = true; // Reuse as loading indicator

    // Fetch full integration data from management API (includes child tables)
    this.integrationManagementService.getIntegration(Number(integration.id))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (fullData) => {
          this.formName = fullData.name;
          this.formDescription = fullData.description || '';
          this.formServiceId = fullData.serviceId;
          this.formWebhookUrl = fullData.webhookUrl || '';
          this.formActive = fullData.active;
          this.formMaxRetryAttempts = fullData.maxRetryAttempts ?? null;
          this.formRetryBackoffSeconds = fullData.retryBackoffSeconds ?? null;
          // Load existing callback event types from the management DTO
          this.formCallbackEventTypeIds = fullData.callbackEventTypes
            ?.map(et => et.id) ?? [];
          this.isSaving = false;

          this.modalRef = this.modalService.open(this.addEditModal, {
            size: 'lg',
            backdrop: 'static',
            centered: true
          });
        },
        error: (err) => {
          console.error('Error loading integration details:', err);
          this.alertService.showErrorMessage('Error', 'Failed to load integration details');
          this.isSaving = false;
        }
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
    this.formMaxRetryAttempts = null;
    this.formRetryBackoffSeconds = null;
    this.formCallbackEventTypeIds = [];
    this.generatedApiKey = null;
  }


  saveIntegration(): void {
    if (!this.formName.trim() || !this.formServiceId) {
      this.alertService.showErrorMessage('Error', 'Name and Service are required');
      return;
    }

    this.isSaving = true;

    if (this.isAddMode) {
      // Use the new backend API for creation - key is generated server-side
      const request: CreateIntegrationRequest = {
        name: this.formName.trim(),
        description: this.formDescription.trim() || undefined,
        serviceId: this.formServiceId!,
        webhookUrl: this.formWebhookUrl.trim() || undefined,
        maxRetryAttempts: this.formMaxRetryAttempts ?? undefined,
        retryBackoffSeconds: this.formRetryBackoffSeconds ?? undefined,
        callbackEventTypeIds: this.formCallbackEventTypeIds.length > 0 ? this.formCallbackEventTypeIds : undefined
      };

      this.integrationManagementService.createIntegration(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (created) => {
            this.alertService.showSuccessMessage('Success', 'Integration created successfully');
            // Store the API key returned from server (only shown once)
            this.generatedApiKey = created.apiKey;
            // Keep modal open to show the API key
            this.isAddMode = false;
            this.editingIntegration = {
              id: BigInt(created.id),
              objectGuid: created.objectGuid,
              serviceId: BigInt(created.serviceId),
              serviceName: created.serviceName,
              name: created.name,
              description: created.description,
              callbackWebhookUrl: created.webhookUrl,
              versionNumber: BigInt(created.versionNumber),
              active: created.active,
              deleted: false,
              apiKeyHash: '' // Hash is never exposed to client
            } as unknown as IntegrationData;
            this.isSaving = false;

            this.integrationService.ClearAllCaches();

            this.loadIntegrations();
          },
          error: (err) => {
            console.error('Error creating integration:', err);
            this.alertService.showErrorMessage('Error', 'Failed to create integration');
            this.isSaving = false;
          }
        });
    } else {
      // Use the new backend API for updates
      const request: UpdateIntegrationRequest = {
        name: this.formName.trim(),
        description: this.formDescription.trim() || undefined,
        webhookUrl: this.formWebhookUrl.trim() || undefined,
        active: this.formActive,
        maxRetryAttempts: this.formMaxRetryAttempts ?? undefined,
        retryBackoffSeconds: this.formRetryBackoffSeconds ?? undefined,
        callbackEventTypeIds: this.formCallbackEventTypeIds
      };

      this.integrationManagementService.updateIntegration(Number(this.editingIntegration!.id), request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.alertService.showSuccessMessage('Success', 'Integration updated successfully');
            this.closeModal();

            this.integrationService.ClearAllCaches();

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


  toggleEventType(eventTypeId: number | bigint): void {
    const id = Number(eventTypeId);
    const index = this.formCallbackEventTypeIds.indexOf(id);
    if (index >= 0) {
      this.formCallbackEventTypeIds.splice(index, 1);
    } else {
      this.formCallbackEventTypeIds.push(id);
    }
  }


  isEventTypeSelected(eventTypeId: number | bigint): boolean {
    return this.formCallbackEventTypeIds.includes(Number(eventTypeId));
  }


  selectAllEventTypes(): void {
    this.formCallbackEventTypeIds = this.incidentEventTypes.map(e => Number(e.id));
  }


  clearAllEventTypes(): void {
    this.formCallbackEventTypeIds = [];
  }


  //
  // Delete handling
  //

  deleteIntegration(integration: IntegrationData): void {
    if (!confirm(`Are you sure you want to delete "${integration.name}"? This will invalidate the API key.`)) {
      return;
    }

    this.integrationManagementService.deleteIntegration(Number(integration.id))
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.alertService.showSuccessMessage('Success', 'Integration deleted successfully');

          this.integrationService.ClearAllCaches();

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

  // Navigation
  goBack(): void {
    this.navigationService.goBack();
  }

  canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }
}

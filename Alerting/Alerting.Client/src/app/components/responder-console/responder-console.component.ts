import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { IncidentService, IncidentData, IncidentQueryParameters } from '../../alerting-data-services/incident.service';
import { IncidentStatusTypeService, IncidentStatusTypeData } from '../../alerting-data-services/incident-status-type.service';
import { SeverityTypeService, SeverityTypeData } from '../../alerting-data-services/severity-type.service';
import { ServiceService, ServiceData } from '../../alerting-data-services/service.service';
import { IncidentNoteService, IncidentNoteData, IncidentNoteSubmitData } from '../../alerting-data-services/incident-note.service';
import { AlertService } from '../../services/alert.service';
import { AuthService } from '../../services/auth.service';

/**
 * Responder Console Component
 * 
 * Mobile-first alert landing page for incident responders.
 * Shows the primary incident they were alerted about and all their active incidents.
 */
@Component({
    selector: 'app-responder-console',
    templateUrl: './responder-console.component.html',
    styleUrls: ['./responder-console.component.scss']
})
export class ResponderConsoleComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Loading states
    isLoading = true;
    isSaving = false;
    errorMessage: string | null = null;

    // Route param
    focusedIncidentId: number | null = null;

    // Data
    primaryIncident: IncidentData | null = null;
    myIncidents: IncidentData[] = [];
    currentUserGuid: string = '';

    // Lookups
    statusTypes: IncidentStatusTypeData[] = [];
    severityTypes: SeverityTypeData[] = [];
    services: ServiceData[] = [];

    // Quick note
    showQuickNote = false;
    quickNoteContent = '';
    isAddingNote = false;

    // Notes preview for primary incident
    primaryIncidentNotes: IncidentNoteData[] = [];
    isLoadingNotes = false;
    showAllNotes = false;
    readonly NOTES_PREVIEW_COUNT = 3;

    // Auto-refresh
    private refreshInterval: any = null;
    autoRefreshEnabled = true;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private incidentService: IncidentService,
        private incidentStatusTypeService: IncidentStatusTypeService,
        private severityTypeService: SeverityTypeService,
        private serviceService: ServiceService,
        private incidentNoteService: IncidentNoteService,
        private alertService: AlertService,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        this.currentUserGuid = this.authService.currentUser?.id || '';

        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            this.focusedIncidentId = params['id'] ? +params['id'] : null;
            this.loadData();
        });

        // Auto-refresh every 30 seconds
        this.startAutoRefresh();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }

    private startAutoRefresh(): void {
        this.refreshInterval = setInterval(() => {
            if (this.autoRefreshEnabled && !this.isSaving) {
                this.loadIncidents(false);
            }
        }, 30000);
    }

    private stopAutoRefresh(): void {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
        }
    }

    /**
     * Load all data
     */
    private loadData(): void {
        this.isLoading = true;
        this.errorMessage = null;

        forkJoin({
            statusTypes: this.incidentStatusTypeService.GetIncidentStatusTypeList({ active: true }),
            severityTypes: this.severityTypeService.GetSeverityTypeList({ active: true }),
            services: this.serviceService.GetServiceList({ active: true })
        }).pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.statusTypes = result.statusTypes;
                    this.severityTypes = result.severityTypes;
                    this.services = result.services;
                    this.loadIncidents(true);
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load data';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });
    }

    /**
     * Load user's incidents
     */
    private loadIncidents(showLoading: boolean): void {
        if (showLoading) {
            this.isLoading = true;
        }

        // Get incidents where user is assignee or active (not resolved)
        const params = new IncidentQueryParameters();
        params.active = true;
        params.deleted = false;
        params.includeRelations = true;

        this.incidentService.GetIncidentList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (incidents) => {
                    // Filter to incidents relevant to this user
                    // For now, show all non-resolved incidents (can refine with notification history)
                    const resolvedStatus = this.statusTypes.find(s => s.name === 'Resolved');
                    const resolvedId = resolvedStatus?.id;

                    this.myIncidents = incidents
                        .filter(i => i.incidentStatusTypeId !== resolvedId)
                        .sort((a, b) => {
                            // Sort by severity then by creation time
                            const sevA = this.getSeverityOrder(a.severityTypeId);
                            const sevB = this.getSeverityOrder(b.severityTypeId);
                            if (sevA !== sevB) return sevA - sevB;
                            return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
                        });

                    // Set primary incident
                    if (this.focusedIncidentId) {
                        this.primaryIncident = this.myIncidents.find(i => i.id === this.focusedIncidentId) || null;
                        // Remove from list to avoid duplication
                        this.myIncidents = this.myIncidents.filter(i => i.id !== this.focusedIncidentId);
                    } else if (this.myIncidents.length > 0) {
                        // Default to most critical/recent
                        this.primaryIncident = this.myIncidents[0];
                        this.myIncidents = this.myIncidents.slice(1);
                    }

                    // Load notes for primary incident
                    if (this.primaryIncident) {
                        this.loadPrimaryIncidentNotes();
                    }

                    this.isLoading = false;
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load incidents';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });
    }

    /**
     * Get severity order for sorting (lower = more severe)
     */
    private getSeverityOrder(severityTypeId: number | bigint): number {
        const severity = this.severityTypes.find(s => s.id === severityTypeId);
        const name = severity?.name?.toLowerCase() || '';
        switch (name) {
            case 'critical': return 1;
            case 'high': return 2;
            case 'medium': return 3;
            case 'low': return 4;
            default: return 5;
        }
    }

    // ===== Display Helpers =====

    getSeverityName(incident: IncidentData): string {
        const severity = this.severityTypes.find(s => s.id === incident.severityTypeId);
        return severity?.name || 'Unknown';
    }

    getSeverityClass(incident: IncidentData): string {
        const name = this.getSeverityName(incident).toLowerCase();
        return `severity-${name}`;
    }

    getStatusName(incident: IncidentData): string {
        const status = this.statusTypes.find(s => s.id === incident.incidentStatusTypeId);
        return status?.name || 'Unknown';
    }

    getStatusClass(incident: IncidentData): string {
        const name = this.getStatusName(incident).toLowerCase();
        return `status-${name}`;
    }

    getServiceName(incident: IncidentData): string {
        const service = this.services.find(s => s.id === incident.serviceId);
        return service?.name || 'Unknown Service';
    }

    getTimeAgo(dateString: string): string {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMins / 60);
        const diffDays = Math.floor(diffHours / 24);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m`;
        if (diffHours < 24) return `${diffHours}h ${diffMins % 60}m`;
        return `${diffDays}d`;
    }

    isTriggered(incident: IncidentData): boolean {
        return this.getStatusName(incident).toLowerCase() === 'triggered';
    }

    isAcknowledged(incident: IncidentData): boolean {
        return this.getStatusName(incident).toLowerCase() === 'acknowledged';
    }

    // ===== Actions =====

    async acknowledgeIncident(incident: IncidentData): Promise<void> {
        if (this.isSaving) return;

        this.isSaving = true;
        try {
            const submitData = incident.ConvertToSubmitData();
            const acknowledgedStatus = this.statusTypes.find(s => s.name === 'Acknowledged');
            if (acknowledgedStatus) {
                submitData.incidentStatusTypeId = acknowledgedStatus.id as number;
            }
            submitData.acknowledgedAt = new Date().toISOString();

            await this.incidentService.PutIncident(incident.id as number, submitData).toPromise();

            this.alertService.showSuccessMessage('Incident acknowledged', null);
            this.loadIncidents(false);
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to acknowledge', error);
        } finally {
            this.isSaving = false;
        }
    }

    async resolveIncident(incident: IncidentData): Promise<void> {
        if (this.isSaving) return;

        this.isSaving = true;
        try {
            const submitData = incident.ConvertToSubmitData();
            const resolvedStatus = this.statusTypes.find(s => s.name === 'Resolved');
            if (resolvedStatus) {
                submitData.incidentStatusTypeId = resolvedStatus.id as number;
            }
            submitData.resolvedAt = new Date().toISOString();

            await this.incidentService.PutIncident(incident.id as number, submitData).toPromise();

            this.alertService.showSuccessMessage('Incident resolved', null);
            this.loadIncidents(false);
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to resolve', error);
        } finally {
            this.isSaving = false;
        }
    }

    toggleQuickNote(): void {
        this.showQuickNote = !this.showQuickNote;
        if (!this.showQuickNote) {
            this.quickNoteContent = '';
        }
    }

    async addQuickNote(): Promise<void> {
        if (!this.primaryIncident || !this.quickNoteContent.trim() || this.isAddingNote) return;

        this.isAddingNote = true;
        try {
            const noteData: IncidentNoteSubmitData = {
                id: 0,
                incidentId: this.primaryIncident.id as number,
                authorObjectGuid: this.currentUserGuid,
                createdAt: new Date().toISOString(),
                content: this.quickNoteContent.trim(),
                versionNumber: 1,
                active: true,
                deleted: false
            };

            await this.incidentNoteService.PostIncidentNote(noteData).toPromise();

            this.quickNoteContent = '';
            this.showQuickNote = false;
            this.alertService.showSuccessMessage('Note added', null);

            // Refresh notes preview
            this.loadPrimaryIncidentNotes();
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to add note', error);
        } finally {
            this.isAddingNote = false;
        }
    }

    /**
     * Load notes for the primary incident
     */
    loadPrimaryIncidentNotes(): void {
        if (!this.primaryIncident) {
            this.primaryIncidentNotes = [];
            return;
        }

        this.isLoadingNotes = true;
        this.incidentNoteService.GetIncidentNoteList({
            incidentId: this.primaryIncident.id as number,
            active: true,
            deleted: false
        }).pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (notes) => {
                    // Sort by most recent first
                    this.primaryIncidentNotes = notes.sort((a, b) =>
                        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
                    );
                    this.isLoadingNotes = false;
                },
                error: (error) => {
                    console.error('Failed to load notes', error);
                    this.primaryIncidentNotes = [];
                    this.isLoadingNotes = false;
                }
            });
    }

    /**
     * Get notes to display in preview (limited count unless expanded)
     */
    get visibleNotes(): IncidentNoteData[] {
        if (this.showAllNotes) {
            return this.primaryIncidentNotes;
        }
        return this.primaryIncidentNotes.slice(0, this.NOTES_PREVIEW_COUNT);
    }

    /**
     * Toggle showing all notes
     */
    toggleShowAllNotes(): void {
        this.showAllNotes = !this.showAllNotes;
    }

    /**
     * Get formatted time for note display
     */
    getNoteTimeAgo(dateString: string): string {
        return this.getTimeAgo(dateString);
    }

    // ===== Navigation =====

    viewIncidentDetails(incident: IncidentData): void {
        this.router.navigate(['/incidents', incident.id]);
    }

    focusOnIncident(incident: IncidentData): void {
        this.router.navigate(['/respond', incident.id]);
    }

    goToDashboard(): void {
        this.router.navigate(['/incident-dashboard']);
    }

    refresh(): void {
        this.loadIncidents(true);
    }

    goToMyShift(): void {
        this.router.navigate(['/my-shift']);
    }
}

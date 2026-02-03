import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { IncidentService, IncidentData, IncidentSubmitData } from '../../alerting-data-services/incident.service';
import { IncidentStatusTypeService, IncidentStatusTypeData } from '../../alerting-data-services/incident-status-type.service';
import { SeverityTypeService, SeverityTypeData } from '../../alerting-data-services/severity-type.service';
import { ServiceService, ServiceData } from '../../alerting-data-services/service.service';
import { IncidentTimelineEventData } from '../../alerting-data-services/incident-timeline-event.service';
import { IncidentNoteService, IncidentNoteData, IncidentNoteSubmitData } from '../../alerting-data-services/incident-note.service';
import { IncidentNotificationData } from '../../alerting-data-services/incident-notification.service';
import { IncidentEventTypeService, IncidentEventTypeData } from '../../alerting-data-services/incident-event-type.service';
import { AlertService } from '../../services/alert.service';
import { AuthService } from '../../services/auth.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';

/**
 * Incident Viewer Component
 * 
 * Full-page detail view for a single incident with:
 * - Hero header with severity/status badges
 * - Timeline of events
 * - Notes section with add capability
 * - Notification history
 * - Action buttons (Acknowledge, Resolve, Assign)
 */
@Component({
    selector: 'app-incident-viewer',
    templateUrl: './incident-viewer.component.html',
    styleUrls: ['./incident-viewer.component.scss']
})
export class IncidentViewerComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Loading states
    isLoading = true;
    isLoadingTimeline = true;
    isLoadingNotes = true;
    isLoadingNotifications = true;
    isSaving = false;
    errorMessage: string | null = null;

    // Core data
    incidentId: number = 0;
    incident: IncidentData | null = null;

    // Related data
    timelineEvents: IncidentTimelineEventData[] = [];
    notes: IncidentNoteData[] = [];
    notifications: IncidentNotificationData[] = [];

    // Lookups
    statusTypes: IncidentStatusTypeData[] = [];
    severityTypes: SeverityTypeData[] = [];
    services: ServiceData[] = [];
    eventTypes: IncidentEventTypeData[] = [];
    userMap: Map<string, AlertingUser> = new Map();

    // New note
    newNoteContent: string = '';
    isAddingNote = false;

    // Active section tab
    activeSection: 'timeline' | 'notes' | 'notifications' = 'timeline';

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private incidentService: IncidentService,
        private incidentStatusTypeService: IncidentStatusTypeService,
        private severityTypeService: SeverityTypeService,
        private serviceService: ServiceService,
        private incidentNoteService: IncidentNoteService,
        private incidentEventTypeService: IncidentEventTypeService,
        private alertService: AlertService,
        private authService: AuthService,
        private alertingUserService: AlertingUserService
    ) { }

    ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            this.incidentId = +params['id'];
            this.loadData();
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Load all data
     */
    private loadData(): void {
        this.isLoading = true;
        this.errorMessage = null;

        // Load lookups first
        forkJoin({
            statusTypes: this.incidentStatusTypeService.GetIncidentStatusTypeList({ active: true }),
            severityTypes: this.severityTypeService.GetSeverityTypeList({ active: true }),
            services: this.serviceService.GetServiceList({ active: true }),
            eventTypes: this.incidentEventTypeService.GetIncidentEventTypeList({ active: true }),
            users: this.alertingUserService.getUsers()
        }).pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.statusTypes = result.statusTypes;
                    this.severityTypes = result.severityTypes;
                    this.services = result.services;
                    this.eventTypes = result.eventTypes;

                    // Build user lookup map
                    this.userMap.clear();
                    for (const user of result.users) {
                        this.userMap.set(user.objectGuid, user);
                    }

                    // Now load the incident
                    this.loadIncident();
                },
                error: (error) => {
                    this.errorMessage = 'Failed to load data';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });
    }

    /**
     * Load the incident
     */
    private loadIncident(): void {
        this.incidentService.GetIncident(this.incidentId, true)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (incident) => {
                    this.incident = incident;
                    this.isLoading = false;

                    // Load related data
                    this.loadTimeline();
                    this.loadNotes();
                    this.loadNotifications();
                },
                error: (error) => {
                    this.errorMessage = 'Incident not found';
                    this.alertService.showHttpErrorMessage('Error', error);
                    this.isLoading = false;
                }
            });
    }

    /**
     * Load timeline events
     */
    private loadTimeline(): void {
        if (!this.incident) return;

        this.isLoadingTimeline = true;
        this.incident.IncidentTimelineEvents.then(events => {
            this.timelineEvents = events.sort((a, b) =>
                new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
            );
            this.isLoadingTimeline = false;
        }).catch(() => {
            this.isLoadingTimeline = false;
        });
    }

    /**
     * Load notes
     */
    private loadNotes(): void {
        if (!this.incident) return;

        this.isLoadingNotes = true;
        this.incident.IncidentNotes.then(notes => {
            this.notes = notes.sort((a, b) =>
                new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
            );
            this.isLoadingNotes = false;
        }).catch(() => {
            this.isLoadingNotes = false;
        });
    }

    /**
     * Load notifications
     */
    private loadNotifications(): void {
        if (!this.incident) return;

        this.isLoadingNotifications = true;
        this.incident.IncidentNotifications.then(notifications => {
            this.notifications = notifications.sort((a, b) =>
                new Date(b.firstNotifiedAt).getTime() - new Date(a.firstNotifiedAt).getTime()
            );
            this.isLoadingNotifications = false;
        }).catch(() => {
            this.isLoadingNotifications = false;
        });
    }

    /**
     * Navigate back to dashboard
     */
    goBack(): void {
        this.router.navigate(['/incident-dashboard']);
    }

    /**
     * Get severity name
     */
    getSeverityName(): string {
        if (!this.incident) return 'Unknown';
        const severity = this.severityTypes.find(s => s.id === this.incident!.severityTypeId);
        return severity?.name || 'Unknown';
    }

    /**
     * Get severity class
     */
    getSeverityClass(): string {
        const name = this.getSeverityName().toLowerCase();
        switch (name) {
            case 'critical': return 'severity-critical';
            case 'high': return 'severity-high';
            case 'medium': return 'severity-medium';
            case 'low': return 'severity-low';
            default: return 'severity-low';
        }
    }

    /**
     * Get status name
     */
    getStatusName(): string {
        if (!this.incident) return 'Unknown';
        const status = this.statusTypes.find(s => s.id === this.incident!.incidentStatusTypeId);
        return status?.name || 'Unknown';
    }

    /**
     * Get status class
     */
    getStatusClass(): string {
        const name = this.getStatusName().toLowerCase();
        switch (name) {
            case 'triggered': return 'status-triggered';
            case 'acknowledged': return 'status-acknowledged';
            case 'resolved': return 'status-resolved';
            default: return 'status-triggered';
        }
    }

    /**
     * Get service name
     */
    getServiceName(): string {
        if (!this.incident) return 'Unknown Service';
        const service = this.services.find(s => s.id === this.incident!.serviceId);
        return service?.name || 'Unknown Service';
    }

    /**
     * Get event type name
     */
    getEventTypeName(eventTypeId: number | bigint): string {
        const eventType = this.eventTypes.find(e => e.id === eventTypeId);
        return eventType?.name || 'Unknown Event';
    }

    /**
     * Get user display name from GUID
     */
    getUserDisplayName(userGuid: string | null | undefined): string {
        if (!userGuid) return 'Unknown User';
        const user = this.userMap.get(userGuid);
        return user?.displayName || user?.accountName || userGuid;
    }

    /**
     * Get event icon class
     */
    getEventIcon(eventTypeId: number | bigint): string {
        const name = this.getEventTypeName(eventTypeId).toLowerCase();
        switch (name) {
            case 'triggered': return 'fa-bolt text-danger';
            case 'escalated': return 'fa-arrow-up text-warning';
            case 'acknowledged': return 'fa-check text-info';
            case 'resolved': return 'fa-check-double text-success';
            case 'noteadded': return 'fa-sticky-note text-secondary';
            case 'notificationsent': return 'fa-bell text-primary';
            default: return 'fa-circle text-muted';
        }
    }

    /**
     * Check if triggered
     */
    isTriggered(): boolean {
        return this.getStatusName().toLowerCase() === 'triggered';
    }

    /**
     * Check if resolved
     */
    isResolved(): boolean {
        return this.getStatusName().toLowerCase() === 'resolved';
    }

    /**
     * Format date/time
     */
    formatDateTime(dateString: string): string {
        const date = new Date(dateString);
        return date.toLocaleString();
    }

    /**
     * Format relative time
     */
    getRelativeTime(dateString: string): string {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMins / 60);
        const diffDays = Math.floor(diffHours / 24);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;
        return date.toLocaleDateString();
    }

    /**
     * Calculate duration
     */
    getDuration(): string {
        if (!this.incident) return '';

        const start = new Date(this.incident.createdAt);
        const end = this.incident.resolvedAt
            ? new Date(this.incident.resolvedAt)
            : new Date();

        const diffMs = end.getTime() - start.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMins / 60);
        const remainingMins = diffMins % 60;

        if (diffHours === 0) return `${remainingMins}m`;
        return `${diffHours}h ${remainingMins}m`;
    }

    /**
     * Acknowledge incident
     */
    async acknowledgeIncident(): Promise<void> {
        if (!this.incident || this.isSaving) return;

        this.isSaving = true;
        try {
            const submitData = this.incident.ConvertToSubmitData();
            const acknowledgedStatus = this.statusTypes.find(s => s.name === 'Acknowledged');
            if (acknowledgedStatus) {
                submitData.incidentStatusTypeId = acknowledgedStatus.id as number;
            }
            submitData.acknowledgedAt = new Date().toISOString();

            await this.incidentService.PutIncident(this.incident.id as number, submitData).toPromise();

            this.alertService.showSuccessMessage('Incident acknowledged', null);
            this.loadIncident();
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to acknowledge', error);
        } finally {
            this.isSaving = false;
        }
    }

    /**
     * Resolve incident
     */
    async resolveIncident(): Promise<void> {
        if (!this.incident || this.isSaving) return;

        this.isSaving = true;
        try {
            const submitData = this.incident.ConvertToSubmitData();
            const resolvedStatus = this.statusTypes.find(s => s.name === 'Resolved');
            if (resolvedStatus) {
                submitData.incidentStatusTypeId = resolvedStatus.id as number;
            }
            submitData.resolvedAt = new Date().toISOString();

            await this.incidentService.PutIncident(this.incident.id as number, submitData).toPromise();

            this.alertService.showSuccessMessage('Incident resolved', null);
            this.loadIncident();
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to resolve', error);
        } finally {
            this.isSaving = false;
        }
    }

    /**
     * Add a note
     */
    async addNote(): Promise<void> {
        if (!this.incident || !this.newNoteContent.trim() || this.isAddingNote) return;

        this.isAddingNote = true;
        try {
            const noteData: IncidentNoteSubmitData = {
                id: 0,
                incidentId: this.incident.id as number,
                authorObjectGuid: this.authService.currentUser?.id || '',
                createdAt: new Date().toISOString(),
                content: this.newNoteContent.trim(),
                versionNumber: 1,
                active: true,
                deleted: false
            };

            await this.incidentNoteService.PostIncidentNote(noteData).toPromise();

            this.newNoteContent = '';
            this.alertService.showSuccessMessage('Note added', null);

            // Refresh notes
            this.incident.ClearIncidentNotesCache();
            this.loadNotes();
        } catch (error: any) {
            this.alertService.showHttpErrorMessage('Failed to add note', error);
        } finally {
            this.isAddingNote = false;
        }
    }

    /**
     * Refresh all data
     */
    refresh(): void {
        this.loadData();
    }

    /**
     * Switch section tab
     */
    setSection(section: 'timeline' | 'notes' | 'notifications'): void {
        this.activeSection = section;
    }
}

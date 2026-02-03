//
// Alert Test Harness Component
//
// Developer testing tool for the Alerting backend APIs.
// Allows triggering alerts, viewing incidents, and testing ack/resolve workflows.
//
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import {
  AlertTestHarnessService,
  AlertPayload,
  AlertResponse,
  IncidentDto,
  IncidentDetailDto,
  IncidentStatsDto,
  NoteDto
} from '../../services/alert-test-harness.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { IntegrationService, IntegrationData } from '../../alerting-data-services/integration.service';


@Component({
  selector: 'app-test-harness',
  templateUrl: './test-harness.component.html',
  styleUrls: ['./test-harness.component.scss']
})
export class TestHarnessComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();

  //
  // Tab management
  //
  activeTab = 'trigger';

  //
  // Trigger Alert form
  //
  apiKey = '';
  alertPayload: AlertPayload = {
    incidentKey: '',
    title: 'Test Alert',
    description: 'This is a test alert from the harness',
    severity: 'High'
  };
  triggerLoading = false;
  lastTriggerResponse: AlertResponse | null = null;

  //
  // Integrations dropdown
  //
  integrations: IntegrationData[] = [];
  selectedIntegrationId: number | null = null;

  //
  // Incidents list
  //
  incidents: IncidentDto[] = [];
  incidentsLoading = false;
  includeResolved = false;

  //
  // Incident detail
  //
  selectedIncident: IncidentDetailDto | null = null;
  detailLoading = false;
  newNoteContent = '';
  addingNote = false;

  //
  // Stats
  //
  stats: IncidentStatsDto | null = null;
  statsLoading = false;

  //
  // Severity options
  //
  severityOptions = ['Critical', 'High', 'Medium', 'Low', 'Info'];


  constructor(
    private testHarnessService: AlertTestHarnessService,
    private integrationService: IntegrationService,
    private alertService: AlertService
  ) { }


  ngOnInit(): void {
    this.loadIntegrations();
    this.loadIncidents();
    this.loadStats();
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  //
  // Tab navigation
  //
  selectTab(tab: string): void {
    this.activeTab = tab;
  }


  //
  // Load integrations for API key dropdown
  //
  loadIntegrations(): void {
    this.integrationService.GetIntegrationList({ active: true, deleted: false })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (integrations: IntegrationData[]) => {
          this.integrations = integrations;
        },
        error: (err: Error) => {
          console.error('Failed to load integrations:', err);
        }
      });
  }


  //
  // Trigger Alert
  //
  triggerAlert(): void {
    if (!this.apiKey.trim()) {
      this.alertService.showMessage('Validation', 'API Key is required', MessageSeverity.warn);
      return;
    }

    if (!this.alertPayload.title?.trim()) {
      this.alertService.showMessage('Validation', 'Title is required', MessageSeverity.warn);
      return;
    }

    this.triggerLoading = true;
    this.lastTriggerResponse = null;

    this.testHarnessService.triggerAlert(this.apiKey, this.alertPayload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: AlertResponse) => {
          this.lastTriggerResponse = response;
          this.triggerLoading = false;

          if (response.success) {
            this.alertService.showMessage(
              'Success',
              `Incident ${response.isNew ? 'created' : 'updated'}: ${response.incidentKey}`,
              MessageSeverity.success
            );

            this.integrationService.ClearAllCaches();

            this.loadIncidents();
            this.loadStats();

          }
        },
        error: (err: Error) => {
          this.triggerLoading = false;
          this.lastTriggerResponse = {
            success: false,
            message: err.message || 'Request failed',
            incidentId: 0,
            incidentKey: '',
            isNew: false
          };
          console.error('Trigger failed:', err);
        }
      });
  }


  //
  // Load Incidents
  //
  loadIncidents(): void {
    this.incidentsLoading = true;

    this.testHarnessService.getIncidents(undefined, undefined, this.includeResolved)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (incidents: IncidentDto[]) => {
          this.incidents = incidents;
          this.incidentsLoading = false;
        },
        error: (err: Error) => {
          console.error('Failed to load incidents:', err);
          this.incidentsLoading = false;
        }
      });
  }


  //
  // View incident detail
  //
  viewIncidentDetail(incident: IncidentDto): void {
    this.detailLoading = true;
    this.selectedIncident = null;

    this.testHarnessService.getIncidentDetail(incident.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (detail: IncidentDetailDto) => {
          this.selectedIncident = detail;
          this.detailLoading = false;
        },
        error: (err: Error) => {
          console.error('Failed to load incident detail:', err);
          this.detailLoading = false;
        }
      });
  }


  closeDetail(): void {
    this.selectedIncident = null;
  }


  //
  // Acknowledge incident
  //
  acknowledgeIncident(id: number): void {
    this.testHarnessService.acknowledgeIncident(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.alertService.showMessage('Success', 'Incident acknowledged', MessageSeverity.success);
          this.loadIncidents();
          this.loadStats();
          if (this.selectedIncident?.id === id) {
            this.viewIncidentDetail({ id } as IncidentDto);
          }
        },
        error: (err: Error) => {
          this.alertService.showMessage('Error', 'Failed to acknowledge: ' + err.message, MessageSeverity.error);
        }
      });
  }


  //
  // Resolve incident
  //
  resolveIncident(id: number): void {
    this.testHarnessService.resolveIncident(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.alertService.showMessage('Success', 'Incident resolved', MessageSeverity.success);
          this.loadIncidents();
          this.loadStats();
          if (this.selectedIncident?.id === id) {
            this.viewIncidentDetail({ id } as IncidentDto);
          }
        },
        error: (err: Error) => {
          this.alertService.showMessage('Error', 'Failed to resolve: ' + err.message, MessageSeverity.error);
        }
      });
  }


  //
  // Add note
  //
  addNote(): void {
    if (!this.selectedIncident || !this.newNoteContent.trim()) return;

    this.addingNote = true;

    this.testHarnessService.addNote(this.selectedIncident.id, this.newNoteContent)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (note: NoteDto) => {
          this.alertService.showMessage('Success', 'Note added', MessageSeverity.success);
          this.newNoteContent = '';
          this.addingNote = false;
          this.viewIncidentDetail({ id: this.selectedIncident!.id } as IncidentDto);
        },
        error: (err: Error) => {
          this.alertService.showMessage('Error', 'Failed to add note: ' + err.message, MessageSeverity.error);
          this.addingNote = false;
        }
      });
  }


  //
  // Load stats
  //
  loadStats(): void {
    this.statsLoading = true;

    this.testHarnessService.getStats()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats: IncidentStatsDto) => {
          this.stats = stats;
          this.statsLoading = false;
        },
        error: (err: Error) => {
          console.error('Failed to load stats:', err);
          this.statsLoading = false;
        }
      });
  }


  //
  // Helpers
  //
  getSeverityClass(severity: string): string {
    switch (severity?.toLowerCase()) {
      case 'critical': return 'severity-critical';
      case 'high': return 'severity-high';
      case 'medium': return 'severity-medium';
      case 'low': return 'severity-low';
      case 'info': return 'severity-info';
      default: return '';
    }
  }


  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'triggered': return 'status-triggered';
      case 'acknowledged': return 'status-acknowledged';
      case 'resolved': return 'status-resolved';
      default: return '';
    }
  }


  generateIncidentKey(): void {
    this.alertPayload.incidentKey = 'test-' + Date.now();
  }


  getObjectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }
}

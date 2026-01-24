//
// User Activity Tab Component
//
// Displays user's login attempts, security events, and audit event timeline.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityUserData } from '../../../security-data-services/security-user.service';
import { SecurityUserEventService, SecurityUserEventQueryParameters, SecurityUserEventData } from '../../../security-data-services/security-user-event.service';
import { LoginAttemptService, LoginAttemptQueryParameters, LoginAttemptData } from '../../../security-data-services/login-attempt.service';
import { AuditEventService, AuditEventData } from '../../../auditor-data-services/audit-event.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-user-activity-tab',
    templateUrl: './user-activity-tab.component.html',
    styleUrls: ['./user-activity-tab.component.scss']
})
export class UserActivityTabComponent implements OnInit, OnChanges {

    @Input() user: SecurityUserData | null = null;

    //
    // Login attempts
    //
    public loginAttempts: LoginAttemptData[] = [];
    public loadingAttempts: boolean = false;

    //
    // User events
    //
    public events: SecurityUserEventData[] = [];
    public loadingEvents: boolean = false;

    //
    // Audit events
    //
    public auditEvents: AuditEventData[] = [];
    public loadingAuditEvents: boolean = false;

    //
    // Audit event stats
    //
    public auditEventStats = {
        total: 0,
        successful: 0,
        failed: 0,
        lastActivity: null as Date | null
    };


    constructor(
        private loginAttemptService: LoginAttemptService,
        private securityUserEventService: SecurityUserEventService,
        private auditEventService: AuditEventService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        this.loadData();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['user'] && !changes['user'].firstChange) {
            this.loadData();
        }
    }


    private loadData(): void {
        if (!this.user) return;
        this.loadLoginAttempts();
        this.loadEvents();
        this.loadAuditEvents();
    }


    private loadLoginAttempts(): void {
        if (!this.user) return;

        this.loadingAttempts = true;

        // LoginAttemptQueryParameters uses userName for filtering, not securityUserId
        const params: any = {
            userName: this.user.accountName,
            deleted: false,
            pageSize: 20
        };

        this.loginAttemptService.GetLoginAttemptList(params).subscribe({
            next: (data) => {
                // Sort by timeStamp descending (most recent first)
                this.loginAttempts = (data ?? []).sort((a, b) => {
                    const aTime = new Date(a.timeStamp).getTime();
                    const bTime = new Date(b.timeStamp).getTime();
                    return bTime - aTime;
                }).slice(0, 20);
                this.loadingAttempts = false;
            },
            error: () => {
                this.loginAttempts = [];
                this.loadingAttempts = false;
            }
        });
    }


    private loadEvents(): void {
        if (!this.user) return;

        this.loadingEvents = true;

        const params = new SecurityUserEventQueryParameters();
        params.securityUserId = Number(this.user.id);
        params.deleted = false;
        params.includeRelations = true;
        params.pageSize = 20;

        this.securityUserEventService.GetSecurityUserEventList(params).subscribe({
            next: (data) => {
                // Sort by timestamp descending
                this.events = (data ?? []).sort((a, b) => {
                    const aTime = new Date(a.timeStamp ?? '').getTime();
                    const bTime = new Date(b.timeStamp ?? '').getTime();
                    return bTime - aTime;
                }).slice(0, 20);
                this.loadingEvents = false;
            },
            error: () => {
                this.events = [];
                this.loadingEvents = false;
            }
        });
    }


    private loadAuditEvents(): void {
        if (!this.user) return;

        this.loadingAuditEvents = true;

        // Query audit events filtering by audit user name matching the security user's account name
        const params: any = {
            userName: this.user.accountName,
            deleted: false,
            includeRelations: true,
            pageSize: 50
        };

        this.auditEventService.GetAuditEventList(params).subscribe({
            next: (data) => {
                // Sort by startTime descending (most recent first)
                this.auditEvents = (data ?? []).sort((a, b) => {
                    const aTime = new Date(a.startTime).getTime();
                    const bTime = new Date(b.startTime).getTime();
                    return bTime - aTime;
                }).slice(0, 30);

                // Calculate stats
                this.auditEventStats = {
                    total: data?.length ?? 0,
                    successful: (data ?? []).filter(e => e.completedSuccessfully).length,
                    failed: (data ?? []).filter(e => !e.completedSuccessfully).length,
                    lastActivity: this.auditEvents.length > 0 ? new Date(this.auditEvents[0].startTime) : null
                };

                this.loadingAuditEvents = false;
            },
            error: () => {
                this.auditEvents = [];
                this.auditEventStats = { total: 0, successful: 0, failed: 0, lastActivity: null };
                this.loadingAuditEvents = false;
            }
        });
    }


    //
    // Display helpers
    //
    public formatDate(dateString: string | null): string {
        if (dateString == null) return '—';
        return new Date(dateString).toLocaleString();
    }


    public formatRelativeTime(dateString: string | null): string {
        if (dateString == null) return 'Never';

        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;

        return date.toLocaleDateString();
    }


    public getEventIcon(event: AuditEventData): string {
        const typeName = event.auditType?.name?.toLowerCase() ?? '';

        if (typeName.includes('create') || typeName.includes('add')) return 'fa-plus-circle text-success';
        if (typeName.includes('update') || typeName.includes('edit') || typeName.includes('modify')) return 'fa-pen-to-square text-primary';
        if (typeName.includes('delete') || typeName.includes('remove')) return 'fa-trash-alt text-danger';
        if (typeName.includes('login') || typeName.includes('auth')) return 'fa-right-to-bracket text-info';
        if (typeName.includes('view') || typeName.includes('read') || typeName.includes('get')) return 'fa-eye text-secondary';
        if (typeName.includes('export') || typeName.includes('download')) return 'fa-download text-warning';

        return 'fa-circle-dot text-muted';
    }
}

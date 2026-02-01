//
// Overview Component - Foundation Admin Dashboard
//
// This component provides a high-level administrative overview of the Foundation system,
// focusing on Security and Auditing metrics.
//
// It follows the pattern established in the Scheduler Overview component but adapted
// for the Foundation Administration context.
//

import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { forkJoin, Subject, Observable, of } from 'rxjs';
import { takeUntil, map, catchError } from 'rxjs/operators';

//
// Foundation Core Services
//
import { AuthService } from '../../services/auth.service';
import { UtilityService } from '../../utility-services/utility.service';

//
// Models
//
import { User } from '../../models/user.model';

//
// Data Services
//
import { LoginAttemptService, LoginAttemptData, LoginAttemptQueryParameters } from '../../security-data-services/login-attempt.service';
import { AuditEventService, AuditEventData, AuditEventQueryParameters } from '../../auditor-data-services/audit-event.service';
import { SecurityUserService, SecurityUserData } from '../../security-data-services/security-user.service';

//
// System Monitoring Services
//
import { TelemetryService, TelemetrySummaryResponse } from '../../services/telemetry.service';
import { SystemHealthService, AuthenticatedUsersInfo } from '../../services/system-health.service';

//
// Chart.js Imports
//
import { ChartConfiguration, ChartOptions, ChartType } from 'chart.js';


//
// Interface definitions for dashboard metrics
//
interface SystemHealthSummary {
  totalUsers: number;
  activeUsers: number;
  lockedOutUsers: number;
  failedLoginsToday: number;
  errorsToday: number;
  // New extended metrics
  failedEventsLastHour: number;
  activeUsersToday: number;
  totalEventsToday: number;
}

interface RecentActivityItem {
  id: number | bigint;
  timestamp: Date;
  description: string;
  type: 'login' | 'audit' | 'error';
  user?: string;
  status: 'success' | 'warning' | 'danger';
}

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class OverviewComponent implements OnInit, OnDestroy, AfterViewInit {

  //
  // Lifecycle management
  //
  private destroy$ = new Subject<void>();

  //
  // Loading state
  //
  public loading: boolean = true;
  public today: Date = new Date();

  //
  // User Info
  //
  public userGreeting: string = '';
  public currentUser: User | null = null;

  //
  // Dashboard Metrics
  //
  public healthSummary: SystemHealthSummary = {
    totalUsers: 0,
    activeUsers: 0,
    lockedOutUsers: 0,
    failedLoginsToday: 0,
    errorsToday: 0,
    failedEventsLastHour: 0,
    activeUsersToday: 0,
    totalEventsToday: 0
  };

  public recentActivity: RecentActivityItem[] = [];
  public recentLoginAttempts: LoginAttemptData[] = [];
  public recentAuditEvents: AuditEventData[] = [];

  //
  // Most Active Modules
  //
  public mostActiveModules: { name: string; count: number; percentage: number }[] = [];

  //
  // Fleet Health Metrics (from Telemetry)
  //
  public fleetSummary: TelemetrySummaryResponse | null = null;
  public fleetOnlineCount: number = 0;
  public fleetTotalCount: number = 0;
  public avgCpuPercent: number = 0;
  public avgMemoryPercent: number = 0;
  public fleetLoading: boolean = true;

  //
  // Security Posture Metrics
  //
  public securityScore: number = 100;
  public ipAnomalyCount: number = 0;
  public loginSuccessRate: number = 100;
  public loginTrend: 'up' | 'down' | 'stable' = 'stable';

  //
  // Active Sessions
  //
  public activeSessionCount: number = 0;

  //
  // Navigation Cards Data
  //
  public navCards: { route: string; icon: string; label: string; count: number | string; color: string }[] = [];

  //
  // Chart Data - Login Activity
  //
  public loginActivityChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Successful Logins',
        borderColor: '#11998e', // Success Green
        backgroundColor: 'rgba(17, 153, 142, 0.1)',
        fill: true,
        tension: 0.4
      },
      {
        data: [],
        label: 'Failed Attempts',
        borderColor: '#f5576c', // Warning Red
        backgroundColor: 'rgba(245, 87, 108, 0.1)',
        fill: true,
        tension: 0.4
      }
    ]
  };

  public loginActivityChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        grid: {
          display: true,
          color: 'rgba(0,0,0,0.05)'
        }
      },
      x: {
        grid: {
          display: false
        }
      }
    }
  };

  public loginActivityChartType: 'line' = 'line';


  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private titleService: Title,
    private authService: AuthService,
    private utilityService: UtilityService,
    private loginAttemptService: LoginAttemptService,
    private auditEventService: AuditEventService,
    private securityUserService: SecurityUserService,
    private telemetryService: TelemetryService,
    private systemHealthService: SystemHealthService
  ) {
    this.setGreeting();
  }

  ngOnInit(): void {
    //
    // Set Page Title
    //
    this.titleService.setTitle('Administrative Overview');

    //
    // Load Data
    //
    this.loadDashboardData();
  }

  ngAfterViewInit(): void {
    // Chart initialization if needed manually, though ng2-charts handles most
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  //
  // Initialization Logic
  //
  private setGreeting(): void {
    const hour = new Date().getHours();
    if (hour < 12) {
      this.userGreeting = 'Good morning';
    } else if (hour < 17) {
      this.userGreeting = 'Good afternoon';
    } else {
      this.userGreeting = 'Good evening';
    }
  }

  //
  // Data Loading
  //
  private loadDashboardData(): void {
    this.loading = true;
    this.fleetLoading = true;

    //
    // Prepare time ranges
    //
    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate()).toISOString();
    const endOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59).toISOString();
    const sevenDaysAgo = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 7).toISOString();
    const oneHourAgo = new Date(now.getTime() - 60 * 60 * 1000).toISOString();

    //
    // Queries
    //
    const loginParams = new LoginAttemptQueryParameters();
    loginParams.timeStamp = sevenDaysAgo; // abusing param slightly to imply "since", handled by data processing usually, or we depend on sort
    loginParams.pageSize = 100;
    loginParams.pageNumber = 1;

    const auditParams = new AuditEventQueryParameters();
    auditParams.startTime = startOfToday;
    auditParams.pageSize = 100;
    auditParams.pageNumber = 1;
    // We ideally want errors or warnings, filtering client side if necessary or via 'message' if possible
    // auditParams.completedSuccessfully = false; // If this param existed for failures

    const userParams = { active: true, includeRelations: false };

    //
    // ForkJoin for parallel loading (existing data)
    //
    forkJoin({
      users: this.securityUserService.GetSecurityUserList(null), // Get all users for stats
      recentLogins: this.loginAttemptService.GetLoginAttemptList(loginParams),
      todaysAuditEvents: this.auditEventService.GetAuditEventList(auditParams)
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (data) => {
        this.processDashboardData(data);
        this.loading = false;
      },
      error: (err) => {
        console.error('Dashboard load failed', err);
        // Fallback or specific error handling
        this.loading = false;
      }
    });

    //
    // Load Fleet Telemetry Data (separate call for resilience)
    //
    this.telemetryService.getSummary().pipe(
      takeUntil(this.destroy$),
      catchError(err => {
        console.warn('Fleet telemetry unavailable:', err.message);
        return of(null);
      })
    ).subscribe(summary => {
      this.fleetSummary = summary;
      this.processFleetData(summary);
      this.fleetLoading = false;
    });

    //
    // Load Active Sessions (separate call for resilience)
    //
    this.systemHealthService.getAuthenticatedUsers().pipe(
      takeUntil(this.destroy$),
      catchError(err => {
        console.warn('Session data unavailable:', err.message);
        return of(null);
      })
    ).subscribe((sessions: AuthenticatedUsersInfo | null) => {
      if (sessions) {
        this.activeSessionCount = sessions.totalCount || sessions.sessions?.length || 0;
      }
    });

    // Also get current user details
    this.currentUser = this.authService.currentUser;
  }

  //
  // Process Fleet Telemetry Data
  //
  private processFleetData(summary: TelemetrySummaryResponse | null): void {
    if (!summary) {
      return;
    }

    //
    // Fleet counts
    //
    const apps = summary.latestSnapshots || [];
    this.fleetTotalCount = apps.length;
    this.fleetOnlineCount = apps.filter(a => a.isOnline).length;

    //
    // Average CPU and Memory across online apps
    //
    const onlineApps = apps.filter(a => a.isOnline);
    if (onlineApps.length > 0) {
      const cpuValues = onlineApps.map(a => a.systemCpuPercent ?? a.cpuPercent ?? 0);
      const memValues = onlineApps.map(a => a.systemMemoryPercent ?? 0);
      this.avgCpuPercent = Math.round(cpuValues.reduce((a, b) => a + b, 0) / cpuValues.length);
      this.avgMemoryPercent = Math.round(memValues.reduce((a, b) => a + b, 0) / memValues.length);
    }

    //
    // Update navigation cards with fleet status
    //
    this.updateNavCards();
  }

  //
  // Data Processing
  //
  private processDashboardData(data: any): void {
    const { users, recentLogins, todaysAuditEvents } = data;

    //
    // System Health Metrics
    //
    const allUsers = users as SecurityUserData[];
    this.healthSummary.totalUsers = allUsers.length;
    this.healthSummary.activeUsers = allUsers.filter(u => u.active === true).length;
    // Assuming 'locked out' might be tracked via status or specific fields, currently just placeholder or checking generic active state
    this.healthSummary.lockedOutUsers = allUsers.filter(u => u.active === false).length;

    //
    // Login Metrics
    //
    const loginAttempts = recentLogins as LoginAttemptData[];

    // Filter for today's attempts
    const todayStr = new Date().toDateString();
    const todaysLogins = loginAttempts.filter(l => new Date(l.timeStamp).toDateString() === todayStr);

    // Count failed logins today. 
    // Assuming 'value' 'Success' or similar, or checking a status field. 
    // LoginAttemptData has 'value'. Let's assume it contains "Success" or "Failure" or similar
    // For safety, let's look for "fail" in value or empty user/resource
    this.healthSummary.failedLoginsToday = todaysLogins.filter(l =>
      (l.value && l.value.toLowerCase().includes('fail')) ||
      (l.value && l.value.toLowerCase().includes('bad'))
    ).length;

    //
    // Audit Metrics
    //
    const auditEvents = todaysAuditEvents as AuditEventData[];
    // Filter for errors: completedSuccessfully == false
    this.healthSummary.errorsToday = auditEvents.filter(e => e.completedSuccessfully === false).length;
    this.healthSummary.totalEventsToday = auditEvents.length;

    //
    // Failed events in last hour
    //
    const oneHourAgo = new Date(Date.now() - 60 * 60 * 1000);
    this.healthSummary.failedEventsLastHour = auditEvents.filter(e =>
      !e.completedSuccessfully && new Date(e.startTime) >= oneHourAgo
    ).length;

    //
    // Active users today (unique users with audit events)
    //
    const uniqueUserIds = new Set(auditEvents.map(e => e.auditUserId));
    this.healthSummary.activeUsersToday = uniqueUserIds.size;

    //
    // Most active modules
    //
    const moduleCounts: Record<string, number> = {};
    auditEvents.forEach(e => {
      const moduleName = e.auditModule?.name || 'Unknown';
      moduleCounts[moduleName] = (moduleCounts[moduleName] || 0) + 1;
    });

    const totalEvents = auditEvents.length || 1; // Avoid divide by zero
    this.mostActiveModules = Object.entries(moduleCounts)
      .map(([name, count]) => ({
        name,
        count,
        percentage: Math.round((count / totalEvents) * 100)
      }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 5);

    //
    // Build Recent Activity Feed (Merge Logins and Audits)
    //
    this.recentActivity = [];

    // Add recent logins (last 5)
    loginAttempts.slice(0, 5).forEach(l => {
      const isSuccess = l.value?.toLowerCase().includes('success');
      this.recentActivity.push({
        id: l.id,
        timestamp: new Date(l.timeStamp),
        description: `Login Attempt: ${l.value || 'Unknown'}`,
        type: 'login',
        user: l.userName || 'Anonymous',
        status: isSuccess ? 'success' : 'danger'
      });
    });

    // Add recent errors (last 5)
    auditEvents
      .filter(e => e.completedSuccessfully === false)
      .slice(0, 5)
      .forEach(e => {
        this.recentActivity.push({
          id: e.id,
          timestamp: new Date(e.startTime),
          description: e.message || 'System Error',
          type: 'error',
          user: 'System', // Would need relation to resolve user name
          status: 'danger'
        });
      });

    // Sort by date desc
    this.recentActivity.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());
    this.recentActivity = this.recentActivity.slice(0, 10); // Keep top 10

    //
    // Build Chart Data (Past 7 Days)
    //
    this.buildLoginChart(loginAttempts);

    //
    // Calculate Security Score
    //
    this.calculateSecurityScore(loginAttempts);

    //
    // Update Navigation Cards
    //
    this.updateNavCards();
  }

  private buildLoginChart(allLogins: LoginAttemptData[]): void {
    const labels: string[] = [];
    const successData: number[] = [];
    const failData: number[] = [];

    // Last 7 days
    for (let i = 6; i >= 0; i--) {
      const d = new Date();
      d.setDate(d.getDate() - i);
      const dateStr = d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      labels.push(dateStr);

      // Filter logins for this day
      const dayLogins = allLogins.filter(l =>
        new Date(l.timeStamp).toDateString() === d.toDateString()
      );

      const successes = dayLogins.filter(l => l.value?.toLowerCase().includes('success')).length;
      const failures = dayLogins.length - successes;

      successData.push(successes);
      failData.push(failures);
    }

    this.loginActivityChartData = {
      labels: labels,
      datasets: [
        {
          data: successData,
          label: 'Successful Logins',
          borderColor: '#11998e', // Success Green
          backgroundColor: 'rgba(17, 153, 142, 0.1)',
          fill: true,
          tension: 0.4
        },
        {
          data: failData,
          label: 'Failed Attempts',
          borderColor: '#f5576c', // Warning Red
          backgroundColor: 'rgba(245, 87, 108, 0.1)',
          fill: true,
          tension: 0.4
        }
      ]
    };
  }

  //
  // Navigation
  //
  public navigateToUsers(): void {
    this.router.navigate(['/users']);
  }

  public navigateToAudit(): void {
    this.router.navigate(['/auditevents']);
  }

  public navigateToLogins(): void {
    this.router.navigate(['/loginattempts']);
  }

  public navigateToSystems(): void {
    this.router.navigate(['/systems-dashboard']);
  }

  public navigateTo(route: string): void {
    this.router.navigate([route]);
  }

  //
  // Update Navigation Cards Data
  //
  private updateNavCards(): void {
    this.navCards = [
      {
        route: '/users',
        icon: 'fa-solid fa-users',
        label: 'Users',
        count: this.healthSummary.activeUsers,
        color: 'primary'
      },
      {
        route: '/tenants',
        icon: 'fa-solid fa-building',
        label: 'Tenants',
        count: '--',
        color: 'info'
      },
      {
        route: '/modules',
        icon: 'fa-solid fa-puzzle-piece',
        label: 'Modules',
        count: this.mostActiveModules.length || '--',
        color: 'purple'
      },
      {
        route: '/auditevents',
        icon: 'fa-solid fa-clipboard-list',
        label: 'Audit',
        count: this.healthSummary.totalEventsToday,
        color: 'warning'
      },
      {
        route: '/loginattempts',
        icon: 'fa-solid fa-right-to-bracket',
        label: 'Logins',
        count: '--',
        color: 'danger'
      },
      {
        route: '/systems-dashboard',
        icon: 'fa-solid fa-satellite-dish',
        label: 'Systems',
        count: `${this.fleetOnlineCount}/${this.fleetTotalCount}`,
        color: 'success'
      }
    ];
  }

  //
  // Calculate Security Posture Score
  //
  private calculateSecurityScore(loginAttempts: LoginAttemptData[]): void {
    const todayStr = new Date().toDateString();
    const todaysLogins = loginAttempts.filter(l => new Date(l.timeStamp).toDateString() === todayStr);

    const successes = todaysLogins.filter(l =>
      l.success === true ||
      (l.value && l.value.toLowerCase().includes('success'))
    ).length;

    const total = todaysLogins.length;
    this.loginSuccessRate = total > 0 ? Math.round((successes / total) * 100) : 100;

    //
    // Calculate IP anomalies (IPs with >50% failure rate and 3+ failures)
    //
    const failures = todaysLogins.filter(l =>
      l.success === false ||
      (l.value && (l.value.toLowerCase().includes('fail') || l.value.toLowerCase().includes('bad')))
    );

    const ipFailureCounts = new Map<string, { total: number; failures: number }>();
    todaysLogins.forEach(l => {
      const ip = l.ipAddress || 'unknown';
      const existing = ipFailureCounts.get(ip) || { total: 0, failures: 0 };
      existing.total++;
      if (l.success === false || (l.value && l.value.toLowerCase().includes('fail'))) {
        existing.failures++;
      }
      ipFailureCounts.set(ip, existing);
    });

    this.ipAnomalyCount = Array.from(ipFailureCounts.values())
      .filter(v => v.total >= 3 && (v.failures / v.total) > 0.5)
      .length;

    //
    // Calculate overall security score (0-100)
    // Base 100, deduct for failures and anomalies
    //
    let score = 100;
    score -= Math.min(20, (100 - this.loginSuccessRate) * 2); // Up to 20 points for failure rate
    score -= Math.min(30, this.ipAnomalyCount * 10); // Up to 30 points for anomalies
    score -= Math.min(20, this.healthSummary.errorsToday * 2); // Up to 20 points for system errors
    this.securityScore = Math.max(0, Math.round(score));

    //
    // Determine login trend (compare today vs yesterday)
    //
    const yesterdayStr = new Date(Date.now() - 24 * 60 * 60 * 1000).toDateString();
    const yesterdayLogins = loginAttempts.filter(l => new Date(l.timeStamp).toDateString() === yesterdayStr);
    const yesterdayFailures = yesterdayLogins.filter(l =>
      l.success === false || (l.value && l.value.toLowerCase().includes('fail'))
    ).length;
    const todayFailures = failures.length;

    if (todayFailures < yesterdayFailures) {
      this.loginTrend = 'down'; // Fewer failures = improving
    } else if (todayFailures > yesterdayFailures) {
      this.loginTrend = 'up'; // More failures = worsening
    } else {
      this.loginTrend = 'stable';
    }
  }
}

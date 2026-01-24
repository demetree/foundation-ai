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
    private securityUserService: SecurityUserService
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
    // ForkJoin for parallel loading
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

    // Also get current user details
    this.currentUser = this.authService.currentUser;
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
}

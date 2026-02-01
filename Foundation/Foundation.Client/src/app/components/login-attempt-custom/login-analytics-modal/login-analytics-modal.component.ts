//
// Login Analytics Modal Component
//
// Detailed analytics modal for login attempt visualization with multi-chart analysis.
// Following Pattern 80 (Detailed Infrastructure Snapshot Modals) from Foundation Premium UI Patterns.
// AI-assisted development - February 2026
//

import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ChartConfiguration, ChartData } from 'chart.js';

import { LoginAttemptData } from '../../../security-data-services/login-attempt.service';


//
// Computed analytics data interfaces
//
interface UserActivitySummary {
    userName: string;
    totalAttempts: number;
    successCount: number;
    failureCount: number;
    successRate: number;
}


interface FailureReasonSummary {
    reason: string;
    count: number;
    percentage: number;
}


interface IpAnalysisSummary {
    ipAddress: string;
    totalAttempts: number;
    failureCount: number;
    failureRate: number;
    isAnomaly: boolean;
}


@Component({
    selector: 'app-login-analytics-modal',
    templateUrl: './login-analytics-modal.component.html',
    styleUrls: ['./login-analytics-modal.component.scss']
})
export class LoginAnalyticsModalComponent implements OnInit {

    //
    // Input: Login attempt data from parent (already filtered)
    //
    @Input() attempts: LoginAttemptData[] = [];
    @Input() timeRangeLabel: string = '';

    //
    // Computed analytics data
    //
    topUsers: UserActivitySummary[] = [];
    failureReasons: FailureReasonSummary[] = [];
    ipAnalysis: IpAnalysisSummary[] = [];

    //
    // Summary statistics
    //
    stats = {
        totalAttempts: 0,
        successCount: 0,
        failureCount: 0,
        successRate: 0,
        uniqueUsers: 0,
        uniqueIps: 0
    };

    //
    // Chart 1: Success vs Failure Trend Line Chart
    //
    trendChartData: ChartData<'line'> = { labels: [], datasets: [] };
    trendChartOptions: ChartConfiguration<'line'>['options'] = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: true,
                position: 'top',
                labels: {
                    usePointStyle: true,
                    padding: 15
                }
            },
            tooltip: {
                mode: 'index',
                intersect: false
            }
        },
        scales: {
            x: {
                display: true,
                grid: { display: false }
            },
            y: {
                display: true,
                beginAtZero: true,
                grid: { color: 'rgba(0,0,0,0.05)' }
            }
        },
        elements: {
            line: { tension: 0.4, borderWidth: 2 },
            point: { radius: 3, hitRadius: 10, hoverRadius: 5 }
        }
    };

    //
    // Chart 2: Failure Breakdown Donut Chart
    //
    failureDonutData: ChartData<'doughnut'> = { labels: [], datasets: [] };
    failureDonutOptions: ChartConfiguration<'doughnut'>['options'] = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: true,
                position: 'right',
                labels: {
                    usePointStyle: true,
                    padding: 12
                }
            },
            tooltip: {
                callbacks: {
                    label: (context) => {
                        const label = context.label || '';
                        const value = context.raw as number;
                        const total = (context.dataset.data as number[]).reduce((a, b) => a + b, 0);
                        const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
                        return `${label}: ${value} (${percentage}%)`;
                    }
                }
            }
        },
        cutout: '60%'
    };

    //
    // Chart 3: Hourly Distribution Bar Chart
    //
    hourlyChartData: ChartData<'bar'> = { labels: [], datasets: [] };
    hourlyChartOptions: ChartConfiguration<'bar'>['options'] = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: true,
                position: 'top',
                labels: {
                    usePointStyle: true,
                    padding: 15
                }
            },
            tooltip: {
                mode: 'index',
                intersect: false
            }
        },
        scales: {
            x: {
                display: true,
                grid: { display: false },
                stacked: true
            },
            y: {
                display: true,
                beginAtZero: true,
                grid: { color: 'rgba(0,0,0,0.05)' },
                stacked: true
            }
        }
    };


    //
    // Getter: Count of anomalous IP addresses (for template binding)
    //
    get anomalyCount(): number {
        return this.ipAnalysis.filter(ip => ip.isAnomaly).length;
    }


    constructor(public activeModal: NgbActiveModal) { }


    ngOnInit(): void {
        this.computeAllAnalytics();
    }


    //
    // Master analytics computation
    //
    private computeAllAnalytics(): void {
        if (this.attempts.length === 0) {
            return;
        }

        this.computeSummaryStats();
        this.computeTrendChart();
        this.computeFailureBreakdown();
        this.computeHourlyDistribution();
        this.computeTopUsers();
        this.computeIpAnalysis();
    }


    //
    // Compute summary statistics
    //
    private computeSummaryStats(): void {
        const successes = this.attempts.filter(a => this.isSuccess(a));
        const failures = this.attempts.filter(a => !this.isSuccess(a));

        this.stats.totalAttempts = this.attempts.length;
        this.stats.successCount = successes.length;
        this.stats.failureCount = failures.length;
        this.stats.successRate = this.attempts.length > 0
            ? Math.round((successes.length / this.attempts.length) * 100)
            : 0;

        const uniqueUserNames = new Set(this.attempts.map(a => a.userName?.toLowerCase()).filter(u => u));
        this.stats.uniqueUsers = uniqueUserNames.size;

        const uniqueIps = new Set(this.attempts.map(a => a.ipAddress).filter(ip => ip));
        this.stats.uniqueIps = uniqueIps.size;
    }


    //
    // Compute success vs failure trend chart (hourly buckets)
    //
    private computeTrendChart(): void {
        const timestamps = this.attempts.map(a => new Date(a.timeStamp).getTime());
        const minTime = Math.min(...timestamps);
        const maxTime = Math.max(...timestamps);

        //
        // Use hourly buckets for shorter ranges, daily for longer
        //
        const rangeMs = maxTime - minTime;
        const dayMs = 24 * 60 * 60 * 1000;
        const hourMs = 60 * 60 * 1000;
        const useDailyBuckets = rangeMs > 3 * dayMs;
        const bucketMs = useDailyBuckets ? dayMs : hourMs;

        const buckets = new Map<number, { success: number; failure: number }>();

        //
        // Initialize buckets
        //
        const startBucket = Math.floor(minTime / bucketMs) * bucketMs;
        const endBucket = Math.ceil(maxTime / bucketMs) * bucketMs;
        for (let t = startBucket; t <= endBucket; t += bucketMs) {
            buckets.set(t, { success: 0, failure: 0 });
        }

        //
        // Fill buckets
        //
        this.attempts.forEach(a => {
            const bucket = Math.floor(new Date(a.timeStamp).getTime() / bucketMs) * bucketMs;
            const b = buckets.get(bucket);
            if (b) {
                if (this.isSuccess(a)) {
                    b.success++;
                } else {
                    b.failure++;
                }
            }
        });

        //
        // Build chart data
        //
        const sortedBuckets = Array.from(buckets.entries()).sort((a, b) => a[0] - b[0]);

        this.trendChartData = {
            labels: sortedBuckets.map(([t]) => {
                const d = new Date(t);
                if (useDailyBuckets) {
                    return d.toLocaleDateString([], { month: 'short', day: 'numeric' });
                }
                return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            }),
            datasets: [
                {
                    label: 'Success',
                    data: sortedBuckets.map(([, v]) => v.success),
                    borderColor: '#28a745',
                    backgroundColor: 'rgba(40, 167, 69, 0.1)',
                    fill: true
                },
                {
                    label: 'Failure',
                    data: sortedBuckets.map(([, v]) => v.failure),
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    fill: true
                }
            ]
        };
    }


    //
    // Compute failure breakdown by reason
    //
    private computeFailureBreakdown(): void {
        const failures = this.attempts.filter(a => !this.isSuccess(a));

        if (failures.length === 0) {
            this.failureDonutData = { labels: [], datasets: [] };
            this.failureReasons = [];
            return;
        }

        //
        // Categorize failure reasons from the 'value' field
        //
        const reasonCounts = new Map<string, number>();

        failures.forEach(a => {
            const reason = this.categorizeFailureReason(a.value || '');
            reasonCounts.set(reason, (reasonCounts.get(reason) || 0) + 1);
        });

        //
        // Build failure reasons summary
        //
        this.failureReasons = Array.from(reasonCounts.entries())
            .map(([reason, count]) => ({
                reason,
                count,
                percentage: Math.round((count / failures.length) * 100)
            }))
            .sort((a, b) => b.count - a.count);

        //
        // Color palette for donut chart
        //
        const colors = ['#dc3545', '#e74c3c', '#c0392b', '#e67e22', '#d35400', '#f39c12', '#e84393', '#9b59b6'];

        this.failureDonutData = {
            labels: this.failureReasons.map(r => r.reason),
            datasets: [{
                data: this.failureReasons.map(r => r.count),
                backgroundColor: this.failureReasons.map((_, i) => colors[i % colors.length]),
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        };
    }


    //
    // Categorize failure reason from value string
    //
    private categorizeFailureReason(value: string): string {
        const lower = value.toLowerCase();

        if (lower.includes('invalid') || lower.includes('incorrect') || lower.includes('wrong')) {
            return 'Invalid Credentials';
        }
        if (lower.includes('locked') || lower.includes('lockout')) {
            return 'Account Locked';
        }
        if (lower.includes('expired')) {
            return 'Password Expired';
        }
        if (lower.includes('disabled') || lower.includes('inactive')) {
            return 'Account Disabled';
        }
        if (lower.includes('not found') || lower.includes('unknown')) {
            return 'User Not Found';
        }
        if (lower.includes('mfa') || lower.includes('two-factor') || lower.includes('2fa')) {
            return 'MFA Failed';
        }
        if (lower.includes('timeout') || lower.includes('expired session')) {
            return 'Session Timeout';
        }
        if (lower.includes('deny') || lower.includes('denied') || lower.includes('unauthorized')) {
            return 'Access Denied';
        }

        return 'Other';
    }


    //
    // Compute hourly distribution (hour of day 0-23)
    //
    private computeHourlyDistribution(): void {
        const hourlySuccess = new Array(24).fill(0);
        const hourlyFailure = new Array(24).fill(0);

        this.attempts.forEach(a => {
            const hour = new Date(a.timeStamp).getHours();
            if (this.isSuccess(a)) {
                hourlySuccess[hour]++;
            } else {
                hourlyFailure[hour]++;
            }
        });

        //
        // Generate hour labels (0:00 - 23:00)
        //
        const labels = Array.from({ length: 24 }, (_, i) =>
            `${i.toString().padStart(2, '0')}:00`
        );

        this.hourlyChartData = {
            labels,
            datasets: [
                {
                    label: 'Success',
                    data: hourlySuccess,
                    backgroundColor: 'rgba(40, 167, 69, 0.7)',
                    borderColor: '#28a745',
                    borderWidth: 1
                },
                {
                    label: 'Failure',
                    data: hourlyFailure,
                    backgroundColor: 'rgba(220, 53, 69, 0.7)',
                    borderColor: '#dc3545',
                    borderWidth: 1
                }
            ]
        };
    }


    //
    // Compute top users by activity
    //
    private computeTopUsers(): void {
        const userMap = new Map<string, { total: number; success: number; failure: number }>();

        this.attempts.forEach(a => {
            const userName = a.userName || 'Unknown';
            const existing = userMap.get(userName) || { total: 0, success: 0, failure: 0 };
            existing.total++;

            if (this.isSuccess(a)) {
                existing.success++;
            } else {
                existing.failure++;
            }

            userMap.set(userName, existing);
        });

        this.topUsers = Array.from(userMap.entries())
            .map(([userName, data]) => ({
                userName,
                totalAttempts: data.total,
                successCount: data.success,
                failureCount: data.failure,
                successRate: data.total > 0 ? Math.round((data.success / data.total) * 100) : 0
            }))
            .sort((a, b) => b.totalAttempts - a.totalAttempts)
            .slice(0, 10);
    }


    //
    // Compute IP address analysis
    //
    private computeIpAnalysis(): void {
        const ipMap = new Map<string, { total: number; failure: number }>();

        this.attempts.forEach(a => {
            const ip = a.ipAddress || 'Unknown';
            const existing = ipMap.get(ip) || { total: 0, failure: 0 };
            existing.total++;

            if (!this.isSuccess(a)) {
                existing.failure++;
            }

            ipMap.set(ip, existing);
        });

        //
        // Calculate anomaly threshold (IPs with >50% failure rate and >3 attempts)
        //
        this.ipAnalysis = Array.from(ipMap.entries())
            .map(([ipAddress, data]) => {
                const failureRate = data.total > 0 ? Math.round((data.failure / data.total) * 100) : 0;
                return {
                    ipAddress,
                    totalAttempts: data.total,
                    failureCount: data.failure,
                    failureRate,
                    isAnomaly: failureRate > 50 && data.failure >= 3
                };
            })
            .sort((a, b) => b.failureCount - a.failureCount)
            .slice(0, 10);
    }


    //
    // Success determination helper (matches parent component logic)
    //
    private isSuccess(attempt: LoginAttemptData): boolean {
        const success = attempt.success;

        if (success === true) {
            return true;
        }
        if (success === false) {
            return false;
        }

        //
        // Fallback heuristic for historical data without success field
        //
        const value = (attempt.value || '').toLowerCase();

        const failureIndicators = ['fail', 'error', 'invalid', 'denied', 'locked', 'expired', 'disabled', 'not found', 'unauthorized'];
        for (const indicator of failureIndicators) {
            if (value.includes(indicator)) {
                return false;
            }
        }

        const successIndicators = ['success', 'ok', 'authenticated', 'granted'];
        for (const indicator of successIndicators) {
            if (value.includes(indicator)) {
                return true;
            }
        }

        return true;
    }


    //
    // Helper: Get success rate badge class
    //
    getSuccessRateBadgeClass(rate: number): string {
        if (rate >= 90) {
            return 'bg-success';
        }
        if (rate >= 70) {
            return 'bg-warning text-dark';
        }
        return 'bg-danger';
    }


    //
    // Close modal
    //
    close(): void {
        this.activeModal.dismiss();
    }
}

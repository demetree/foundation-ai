import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../services/auth.service'


interface UpcomingEvent {
  id: number;
  name: string;
  startDateTime: string;
  endDateTime: string;
  location: string;
  schedulingTargetName: string;
}

interface DepositItem {
  chargeId: number;
  eventId: number;
  eventName: string;
  chargeType: string;
  amount: number;
  eventDate: string | null;
}

interface DepositsResponse {
  count: number;
  totalAmount: number;
  deposits: DepositItem[];
}

interface RecentTxn {
  id: number;
  transactionDate: string;
  description: string;
  amount: number;
  isRevenue: boolean;
  categoryName: string;
}

interface YtdSummary {
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;
}


@Component({
  selector: 'app-overview-coordinator-tab',
  templateUrl: './overview-coordinator-tab.component.html',
  styleUrls: ['./overview-coordinator-tab.component.scss']
})
export class OverviewCoordinatorTabComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();

  loading = true;

  // Section data
  upcomingEvents: UpcomingEvent[] = [];
  outstandingDeposits: DepositItem[] = [];
  depositsTotal = 0;
  recentTransactions: RecentTxn[] = [];
  ytdSummary: YtdSummary = { totalRevenue: 0, totalExpenses: 0, netIncome: 0 };

  constructor(private http: HttpClient, private authService: AuthService) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  private loadData(): void {
    this.loading = true;

    const authenticationHeaders = this.authService.GetAuthenticationHeaders();


    const now = new Date();
    const sevenDaysLater = new Date(now);
    sevenDaysLater.setDate(sevenDaysLater.getDate() + 7);
    const year = now.getFullYear();

    forkJoin({
      events: this.http.get<any[]>('/api/ScheduledEvents', {
        params: {
          startDateTime: now.toISOString(),
          endDateTime: sevenDaysLater.toISOString(),
          active: 'true',
          includeRelations: 'true'
        },
        headers: authenticationHeaders
      }),
      deposits: this.http.get<DepositsResponse>('/api/FinancialTransactions/OutstandingDeposits'),
      transactions: this.http.get<any[]>('/api/FinancialTransactions', {
        params: {
          includeRelations: 'true',
          pageSize: '10',
          pageNumber: '1'
        }, headers: authenticationHeaders
      }),
      summary: this.http.get<any>('/api/FinancialTransactions/Summary', {
        params: { year: year.toString() },
        headers: authenticationHeaders
      }, )
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (data) => {
        // Upcoming events
        this.upcomingEvents = (data.events ?? [])
          .map((e: any) => ({
            id: e.id,
            name: e.name,
            startDateTime: e.startDateTime,
            endDateTime: e.endDateTime,
            location: e.location || '—',
            schedulingTargetName: e.schedulingTarget?.name || '—'
          }))
          .sort((a: UpcomingEvent, b: UpcomingEvent) =>
            new Date(a.startDateTime).getTime() - new Date(b.startDateTime).getTime()
          )
          .slice(0, 8);

        // Outstanding deposits
        this.outstandingDeposits = data.deposits?.deposits ?? [];
        this.depositsTotal = data.deposits?.totalAmount ?? 0;

        // Recent transactions
        this.recentTransactions = (data.transactions ?? [])
          .map((t: any) => ({
            id: t.id,
            transactionDate: t.transactionDate,
            description: t.description || '—',
            amount: t.amount ?? 0,
            isRevenue: t.isRevenue ?? false,
            categoryName: t.financialCategory?.name || '—'
          }))
          .slice(0, 10);

        // YTD summary
        this.ytdSummary = {
          totalRevenue: data.summary?.totalRevenue ?? 0,
          totalExpenses: data.summary?.totalExpenses ?? 0,
          netIncome: (data.summary?.totalRevenue ?? 0) - (data.summary?.totalExpenses ?? 0)
        };

        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}

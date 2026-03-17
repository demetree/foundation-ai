// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { PledgeService, PledgeData } from '../../../scheduler-data-services/pledge.service';
import { GiftService, GiftData } from '../../../scheduler-data-services/gift.service';
import { CampaignService, CampaignData } from '../../../scheduler-data-services/campaign.service';
import { FundService, FundData } from '../../../scheduler-data-services/fund.service';


interface PledgeRow {
    pledge: PledgeData;
    constituentName: string;
    fundName: string;
    campaignName: string;
    totalPledged: number;
    totalGifted: number;
    balance: number;
    percentFulfilled: number;
}


@Component({
    selector: 'app-pledge-dashboard',
    templateUrl: './pledge-dashboard.component.html',
    styleUrls: ['./pledge-dashboard.component.scss']
})
export class PledgeDashboardComponent implements OnInit {

    public isLoading = true;
    public pledgeRows: PledgeRow[] = [];
    public filteredRows: PledgeRow[] = [];

    // Filters
    public campaigns: CampaignData[] = [];
    public funds: FundData[] = [];
    public selectedCampaignId: number | null = null;
    public selectedFundId: number | null = null;
    public statusFilter: 'all' | 'active' | 'fulfilled' = 'all';

    // Totals
    public totalPledged = 0;
    public totalGifted = 0;
    public totalBalance = 0;
    public overallPercent = 0;

    constructor(
        private pledgeService: PledgeService,
        private giftService: GiftService,
        private campaignService: CampaignService,
        private fundService: FundService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.loadData();
    }


    private loadData(): void {
        this.isLoading = true;

        forkJoin({
            pledges: this.pledgeService.GetPledgeList({ active: true, deleted: false, includeRelations: true, pageSize: 5000 }),
            gifts: this.giftService.GetGiftList({ active: true, deleted: false, includeRelations: false }),
            campaigns: this.campaignService.GetCampaignList({ active: true, deleted: false }),
            funds: this.fundService.GetFundList({ active: true, deleted: false })
        }).subscribe({
            next: (data) => {
                this.campaigns = data.campaigns ?? [];
                this.funds = data.funds ?? [];
                this.buildDashboard(data.pledges ?? [], data.gifts ?? []);
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load pledge data', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private buildDashboard(pledges: PledgeData[], gifts: GiftData[]): void {
        // Sum gifts by pledgeId
        const giftByPledge = new Map<number, number>();
        for (const g of gifts) {
            if (g.pledgeId) {
                const pid = Number(g.pledgeId);
                giftByPledge.set(pid, (giftByPledge.get(pid) || 0) + (g.amount || 0));
            }
        }

        this.pledgeRows = pledges.map(p => {
            const totalPledged = p.totalAmount || 0;
            const totalGifted = giftByPledge.get(Number(p.id)) || 0;
            const balance = Math.max(0, totalPledged - totalGifted);
            const percentFulfilled = totalPledged > 0 ? Math.min(100, (totalGifted / totalPledged) * 100) : 0;

            return {
                pledge: p,
                constituentName: `${(p.constituent as any)?.firstName || ''} ${(p.constituent as any)?.lastName || ''}`.trim()
                    || (p.constituent as any)?.organizationName || 'Unknown',
                fundName: (p.fund as any)?.name || '—',
                campaignName: (p.campaign as any)?.name || '—',
                totalPledged,
                totalGifted,
                balance,
                percentFulfilled
            };
        }).sort((a, b) => b.balance - a.balance);

        this.applyFilters();
    }


    onFilterChange(): void {
        this.applyFilters();
    }


    private applyFilters(): void {
        let rows = [...this.pledgeRows];

        if (this.selectedCampaignId) {
            rows = rows.filter(r => Number(r.pledge.campaignId) === this.selectedCampaignId);
        }
        if (this.selectedFundId) {
            rows = rows.filter(r => Number(r.pledge.fundId) === this.selectedFundId);
        }
        if (this.statusFilter === 'active') {
            rows = rows.filter(r => r.balance > 0);
        } else if (this.statusFilter === 'fulfilled') {
            rows = rows.filter(r => r.balance <= 0);
        }

        this.filteredRows = rows;

        this.totalPledged = rows.reduce((s, r) => s + r.totalPledged, 0);
        this.totalGifted = rows.reduce((s, r) => s + r.totalGifted, 0);
        this.totalBalance = rows.reduce((s, r) => s + r.balance, 0);
        this.overallPercent = this.totalPledged > 0
            ? Math.min(100, (this.totalGifted / this.totalPledged) * 100) : 0;
    }


    getProgressClass(percent: number): string {
        if (percent >= 100) return 'bg-success';
        if (percent >= 50) return 'bg-info';
        if (percent >= 25) return 'bg-warning';
        return 'bg-danger';
    }


    formatCurrency(amount: number): string { return '$' + amount.toFixed(2); }

    goBack(): void { this.router.navigate(['/finances']); }
}

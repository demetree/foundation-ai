// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { ConstituentService, ConstituentBasicListData } from '../../../scheduler-data-services/constituent.service';
import { FundService, FundData } from '../../../scheduler-data-services/fund.service';
import { CampaignService, CampaignData } from '../../../scheduler-data-services/campaign.service';
import { PledgeService, PledgeData } from '../../../scheduler-data-services/pledge.service';
import { PaymentTypeService, PaymentTypeData } from '../../../scheduler-data-services/payment-type.service';


@Component({
    selector: 'app-gift-entry',
    templateUrl: './gift-entry.component.html',
    styleUrls: ['./gift-entry.component.scss']
})
export class GiftEntryComponent implements OnInit {

    public isLoading = true;
    public isSaving = false;

    // Lookup data
    public constituents: ConstituentBasicListData[] = [];
    public funds: FundData[] = [];
    public campaigns: CampaignData[] = [];
    public pledges: PledgeData[] = [];
    public paymentTypes: PaymentTypeData[] = [];

    // Form fields
    public constituentId: number | null = null;
    public amount: number | null = null;
    public fundId: number | null = null;
    public campaignId: number | null = null;
    public paymentTypeId: number | null = null;
    public receivedDate: string = new Date().toISOString().slice(0, 10);
    public pledgeId: number | null = null;
    public referenceNumber = '';
    public notes = '';

    // Constituent search
    public constituentSearch = '';
    public filteredConstituents: ConstituentBasicListData[] = [];
    public showConstituentDropdown = false;
    public selectedConstituentName = '';

    // Matching pledges for selected constituent
    public matchingPledges: PledgeData[] = [];

    constructor(
        private http: HttpClient,
        private constituentService: ConstituentService,
        private fundService: FundService,
        private campaignService: CampaignService,
        private pledgeService: PledgeService,
        private paymentTypeService: PaymentTypeService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string
    ) { }


    ngOnInit(): void {
        this.loadLookups();
    }


    private loadLookups(): void {
        forkJoin({
            constituents: this.constituentService.GetConstituentsBasicListData({ active: true, deleted: false }),
            funds: this.fundService.GetFundList({ active: true, deleted: false }),
            campaigns: this.campaignService.GetCampaignList({ active: true, deleted: false }),
            paymentTypes: this.paymentTypeService.GetPaymentTypeList({ active: true, deleted: false }),
            pledges: this.pledgeService.GetPledgeList({ active: true, deleted: false, includeRelations: true, pageSize: 5000 })
        }).subscribe({
            next: (data) => {
                this.constituents = data.constituents ?? [];
                this.funds = data.funds ?? [];
                this.campaigns = data.campaigns ?? [];
                this.paymentTypes = data.paymentTypes ?? [];
                this.pledges = data.pledges ?? [];
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load form data', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    onConstituentSearch(): void {
        const q = this.constituentSearch.toLowerCase().trim();
        if (q.length < 2) {
            this.filteredConstituents = [];
            this.showConstituentDropdown = false;
            return;
        }
        this.filteredConstituents = this.constituents.filter(c => {
            return (c.name || '').toLowerCase().includes(q);
        }).slice(0, 20);
        this.showConstituentDropdown = this.filteredConstituents.length > 0;
    }


    selectConstituent(c: ConstituentBasicListData): void {
        this.constituentId = Number(c.id);
        this.selectedConstituentName = c.name || 'Constituent';
        this.constituentSearch = this.selectedConstituentName;
        this.showConstituentDropdown = false;

        // Load matching pledges for this constituent
        this.matchingPledges = this.pledges.filter(p =>
            Number(p.constituentId) === this.constituentId &&
            (p.balanceAmount > 0)
        );
        this.pledgeId = null;
    }


    clearConstituent(): void {
        this.constituentId = null;
        this.constituentSearch = '';
        this.selectedConstituentName = '';
        this.matchingPledges = [];
        this.pledgeId = null;
    }


    get isValid(): boolean {
        return this.constituentId != null && this.amount != null && this.amount > 0
            && this.fundId != null && this.paymentTypeId != null && !!this.receivedDate;
    }


    save(): void {
        if (!this.isValid || this.isSaving) return;
        this.isSaving = true;

        const body = {
            constituentId: this.constituentId,
            amount: this.amount,
            fundId: this.fundId,
            paymentTypeId: this.paymentTypeId,
            receivedDate: new Date(this.receivedDate).toISOString(),
            pledgeId: this.pledgeId,
            campaignId: this.campaignId,
            referenceNumber: this.referenceNumber || null,
            notes: this.notes || null
        };

        const headers = new HttpHeaders({
            'Authorization': 'Bearer ' + this.authService.accessToken
        });

        this.http.post<any>(`${this.baseUrl}api/Gifts/RecordGift`, body, { headers }).subscribe({
            next: (res) => {
                this.alertService.showMessage('Gift Recorded', res.message || 'Gift recorded successfully.', MessageSeverity.success);
                this.isSaving = false;
                this.resetForm();
            },
            error: (err) => {
                const msg = err?.error || err?.message || 'Failed to record gift';
                this.alertService.showMessage('Error', msg, MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }


    resetForm(): void {
        this.constituentId = null;
        this.constituentSearch = '';
        this.selectedConstituentName = '';
        this.amount = null;
        this.fundId = null;
        this.campaignId = null;
        this.paymentTypeId = null;
        this.receivedDate = new Date().toISOString().slice(0, 10);
        this.pledgeId = null;
        this.referenceNumber = '';
        this.notes = '';
        this.matchingPledges = [];
    }


    goBack(): void {
        this.router.navigate(['/finances']);
    }


    formatCurrency(amount: number | null | undefined): string {
        if (amount == null) return '$0.00';
        return '$' + amount.toFixed(2);
    }
}

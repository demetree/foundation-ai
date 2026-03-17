import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../services/auth.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { NavigationService } from '../../../utility-services/navigation.service';
import { Router } from '@angular/router';


interface AuditEntry {
    id: number;
    financialTransactionId: number;
    versionNumber: number;
    timestamp: string;
    userId: number;
    userName: string;
    action: string;
    fieldChanges: FieldChange[] | null;
    reason: string | null;
}

interface FieldChange {
    field: string;
    oldValue: string;
    newValue: string;
}


@Component({
    selector: 'app-audit-log-viewer',
    templateUrl: './audit-log-viewer.component.html',
    styleUrls: ['./audit-log-viewer.component.scss']
})
export class AuditLogViewerComponent implements OnInit {

    public entries: AuditEntry[] = [];
    public isLoading = true;

    // Filters
    public filterTransactionId: number | null = null;
    public filterAction: string = '';
    public filterUser: string = '';
    public filterDateFrom: string = '';
    public filterDateTo: string = '';

    // Expanded rows
    public expandedIds = new Set<number>();

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private alertService: AlertService,
        private navigationService: NavigationService,
        private router: Router,
        @Inject('BASE_URL') private baseUrl: string
    ) { }


    ngOnInit(): void {
        this.loadAuditLog();
    }


    public loadAuditLog(): void {
        this.isLoading = true;
        const headers = this.authService.GetAuthenticationHeaders();

        let params: any = { maxResults: 500 };
        if (this.filterTransactionId) params.transactionId = this.filterTransactionId;
        if (this.filterDateFrom) params.fromDate = this.filterDateFrom;
        if (this.filterDateTo) params.toDate = this.filterDateTo;

        this.http.get<AuditEntry[]>(
            `${this.baseUrl}api/FinancialTransactions/AuditLog`,
            { headers, params }
        ).subscribe({
            next: (data) => {
                this.entries = data ?? [];
                this.isLoading = false;
            },
            error: (err: any) => {
                this.alertService.showMessage('Error', 'Failed to load audit log', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    public get filteredEntries(): AuditEntry[] {
        let result = this.entries;

        if (this.filterAction) {
            result = result.filter(e => e.action?.toLowerCase() === this.filterAction.toLowerCase());
        }

        if (this.filterUser) {
            const search = this.filterUser.toLowerCase();
            result = result.filter(e => e.userName?.toLowerCase().includes(search));
        }

        return result;
    }


    public toggleExpand(id: number): void {
        if (this.expandedIds.has(id)) {
            this.expandedIds.delete(id);
        } else {
            this.expandedIds.add(id);
        }
    }


    public isExpanded(id: number): boolean {
        return this.expandedIds.has(id);
    }


    public getActionBadgeClass(action: string): string {
        switch (action?.toLowerCase()) {
            case 'created': return 'badge-created';
            case 'updated': return 'badge-updated';
            case 'voided': return 'badge-voided';
            default: return 'badge-unknown';
        }
    }


    public getActionIcon(action: string): string {
        switch (action?.toLowerCase()) {
            case 'created': return 'fa-plus-circle';
            case 'updated': return 'fa-pencil';
            case 'voided': return 'fa-ban';
            default: return 'fa-question-circle';
        }
    }


    public getFieldLabel(field: string): string {
        const labels: Record<string, string> = {
            financialCategoryId: 'Category',
            transactionDate: 'Date',
            amount: 'Amount',
            taxAmount: 'Tax',
            totalAmount: 'Total',
            description: 'Description',
            isRevenue: 'Type',
            journalEntryType: 'Journal Entry',
            currencyId: 'Currency',
            financialOfficeId: 'Office',
            scheduledEventId: 'Event',
            contactId: 'Contact',
            clientId: 'Client',
            referenceNumber: 'Reference #',
            notes: 'Notes',
            fiscalPeriodId: 'Fiscal Period',
            deleted: 'Status'
        };
        return labels[field] || field;
    }


    public formatValue(field: string, value: string): string {
        if (!value || value === '') return '—';
        if (field === 'isRevenue') return value === 'True' ? 'Revenue' : 'Expense';
        if (field === 'deleted') return value === 'True' ? 'Voided' : 'Active';
        if (field === 'transactionDate') {
            try {
                return new Date(value).toLocaleDateString();
            } catch { return value; }
        }
        if (['amount', 'taxAmount', 'totalAmount'].includes(field)) {
            const n = parseFloat(value);
            return isNaN(n) ? value : `$${n.toFixed(2)}`;
        }
        return value;
    }


    public onFilterApply(): void {
        this.loadAuditLog();
    }


    public clearFilters(): void {
        this.filterTransactionId = null;
        this.filterAction = '';
        this.filterUser = '';
        this.filterDateFrom = '';
        this.filterDateTo = '';
        this.loadAuditLog();
    }


    public trackById(index: number, item: AuditEntry): number {
        return item.id;
    }


    public goBack(): void {
        this.navigationService.goBack();
    }

    public canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    public goToDashboard(): void {
        this.router.navigate(['/finances']);
    }
}

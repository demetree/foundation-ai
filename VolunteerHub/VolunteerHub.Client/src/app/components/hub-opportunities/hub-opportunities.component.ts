import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HubApiService } from '../../services/hub-api.service';
import { Opportunity } from '../../models/hub-models';

@Component({
    selector: 'app-hub-opportunities',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './hub-opportunities.component.html',
    styleUrls: ['./hub-opportunities.component.scss']
})
export class HubOpportunitiesComponent implements OnInit {
    opportunities: Opportunity[] = [];
    loading = true;
    signingUp: number | null = null;
    successMessage = '';
    errorMessage = '';

    // Search/filter state
    searchTerm = '';
    fromDate = '';
    toDate = '';
    private searchTimeout: any = null;

    constructor(private api: HubApiService) { }

    ngOnInit(): void {
        this.loadOpportunities();
    }

    loadOpportunities(): void {
        this.loading = true;

        const fromDate = this.fromDate ? new Date(this.fromDate) : undefined;
        const toDate = this.toDate ? new Date(this.toDate) : undefined;
        const search = this.searchTerm.trim() || undefined;

        this.api.getOpportunities(search, fromDate, toDate).subscribe({
            next: (data) => {
                this.opportunities = data;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.errorMessage = 'Failed to load opportunities.';
            }
        });
    }

    onSearch(): void {
        if (this.searchTimeout) {
            clearTimeout(this.searchTimeout);
        }

        this.searchTimeout = setTimeout(() => {
            this.loadOpportunities();
        }, 300);
    }

    onDateChange(): void {
        this.loadOpportunities();
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.fromDate = '';
        this.toDate = '';
        this.loadOpportunities();
    }

    get hasActiveFilters(): boolean {
        return this.searchTerm.trim().length > 0
            || this.fromDate.length > 0
            || this.toDate.length > 0;
    }

    signUp(eventId: number): void {
        this.signingUp = eventId;
        this.successMessage = '';
        this.errorMessage = '';

        this.api.signUpForOpportunity(eventId).subscribe({
            next: (res) => {
                this.signingUp = null;
                this.successMessage = res.message || 'Successfully signed up!';
                // Refresh list to update sign-up status
                this.loadOpportunities();
            },
            error: (err) => {
                this.signingUp = null;
                this.errorMessage = err.error?.error || 'Sign-up failed. Please try again.';
            }
        });
    }

    formatDate(dateStr: string): string {
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-US', {
            weekday: 'short', month: 'short', day: 'numeric',
            hour: 'numeric', minute: '2-digit'
        });
    }
}

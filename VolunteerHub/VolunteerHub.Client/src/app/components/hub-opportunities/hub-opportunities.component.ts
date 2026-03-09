import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HubApiService } from '../../services/hub-api.service';

@Component({
    selector: 'app-hub-opportunities',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './hub-opportunities.component.html',
    styleUrls: ['./hub-opportunities.component.scss']
})
export class HubOpportunitiesComponent implements OnInit {
    opportunities: any[] = [];
    loading = true;
    signingUp: number | null = null;
    successMessage = '';
    errorMessage = '';

    constructor(private api: HubApiService) { }

    ngOnInit(): void {
        this.loadOpportunities();
    }

    loadOpportunities(): void {
        this.loading = true;
        this.api.getOpportunities().subscribe({
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

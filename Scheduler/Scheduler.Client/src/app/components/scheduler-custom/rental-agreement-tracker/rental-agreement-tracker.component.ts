// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { ScheduledEventService, ScheduledEventData } from '../../../scheduler-data-services/scheduled-event.service';


//
// Rental agreement data stored in ScheduledEvent.attributes JSON
//
interface RentalAgreement {
    agreementStatus: 'draft' | 'signed' | 'expired' | 'none';
    signedDate: string | null;
    expiryDate: string | null;
    contactName: string | null;
    rentalNotes: string | null;
}

interface RentalRow {
    event: ScheduledEventData;
    agreement: RentalAgreement;
}


@Component({
    selector: 'app-rental-agreement-tracker',
    templateUrl: './rental-agreement-tracker.component.html',
    styleUrls: ['./rental-agreement-tracker.component.scss']
})
export class RentalAgreementTrackerComponent implements OnInit {

    public isLoading = true;
    public statusFilter: 'all' | 'draft' | 'signed' | 'expired' = 'all';
    public searchTerm = '';
    public allRows: RentalRow[] = [];

    // Summary counts
    public draftCount = 0;
    public signedCount = 0;
    public expiredCount = 0;

    constructor(
        private eventService: ScheduledEventService,
        private alertService: AlertService,
        private authService: AuthService,
        private router: Router
    ) { }


    ngOnInit(): void {
        this.loadEvents();
    }


    private loadEvents(): void {
        this.isLoading = true;
        this.eventService.GetScheduledEventList({ active: true, deleted: false }).subscribe({
            next: (events) => {
                this.allRows = (events ?? [])
                    .map(evt => {
                        const agreement = this.parseAgreement(evt.attributes);
                        return { event: evt, agreement };
                    })
                    .filter(r => r.agreement.agreementStatus !== 'none');

                this.draftCount = this.allRows.filter(r => r.agreement.agreementStatus === 'draft').length;
                this.signedCount = this.allRows.filter(r => r.agreement.agreementStatus === 'signed').length;
                this.expiredCount = this.allRows.filter(r => r.agreement.agreementStatus === 'expired').length;
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Failed to load events', '', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private parseAgreement(attributes: string | null): RentalAgreement {
        if (!attributes) return { agreementStatus: 'none', signedDate: null, expiryDate: null, contactName: null, rentalNotes: null };
        try {
            const parsed = JSON.parse(attributes);
            if (parsed.rentalAgreement) {
                return {
                    agreementStatus: parsed.rentalAgreement.status ?? 'none',
                    signedDate: parsed.rentalAgreement.signedDate ?? null,
                    expiryDate: parsed.rentalAgreement.expiryDate ?? null,
                    contactName: parsed.rentalAgreement.contactName ?? null,
                    rentalNotes: parsed.rentalAgreement.notes ?? null,
                };
            }
            return { agreementStatus: 'none', signedDate: null, expiryDate: null, contactName: null, rentalNotes: null };
        } catch {
            return { agreementStatus: 'none', signedDate: null, expiryDate: null, contactName: null, rentalNotes: null };
        }
    }


    get filteredRows(): RentalRow[] {
        let result = this.allRows;
        if (this.statusFilter !== 'all') {
            result = result.filter(r => r.agreement.agreementStatus === this.statusFilter);
        }
        if (this.searchTerm) {
            const q = this.searchTerm.toLowerCase();
            result = result.filter(r =>
                (r.event.name ?? '').toLowerCase().includes(q) ||
                (r.agreement.contactName ?? '').toLowerCase().includes(q)
            );
        }
        return result;
    }


    formatDate(dateStr: string | null): string {
        if (!dateStr) return '—';
        try {
            return new Date(dateStr).toLocaleDateString('en-CA', { year: 'numeric', month: 'short', day: 'numeric' });
        } catch { return dateStr; }
    }


    navigateToEvent(eventId: bigint | number): void {
        this.router.navigate(['/scheduling/events', eventId]);
    }

    goBack(): void {
        this.router.navigate(['/scheduling']);
    }
}

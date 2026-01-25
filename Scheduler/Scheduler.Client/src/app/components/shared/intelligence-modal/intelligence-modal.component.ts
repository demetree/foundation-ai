
import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IntelligenceService, IntelligenceContext, IntelligenceDossier } from '../../../services/intelligence.service';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'app-intelligence-modal',
    templateUrl: './intelligence-modal.component.html',
    styleUrls: ['./intelligence-modal.component.scss']
})
export class IntelligenceModalComponent implements OnInit {

    @Input() context!: IntelligenceContext;

    public dossier: IntelligenceDossier | null = null;
    public isLoading: boolean = true;
    public error: string | null = null;

    constructor(
        public activeModal: NgbActiveModal,
        private intelligenceService: IntelligenceService
    ) { }

    ngOnInit(): void {
        if (this.context) {
            this.loadIntelligence();
        } else {
            this.error = "No context provided for intelligence analysis.";
            this.isLoading = false;
        }
    }

    private loadIntelligence(): void {
        this.isLoading = true;
        this.error = null;

        // Subscribe to the shared stream
        // Note: In a real app, we might want to filter this stream by correlationId to ensure we don't get mixed results,
        // but for this task we assume single-user interactive mode.
        this.intelligenceService.dossier$
            .pipe(
                finalize(() => {
                    // This finalize might not trigger on 'next', only on complete/error of the stream? 
                    // Actually, dossier$ is long-lived. We should handle loading state inside the subscription.
                })
            )
            .subscribe({
                next: (data) => {
                    this.dossier = data;
                    this.isLoading = false;

                    if (data.status === 'failed') {
                        this.error = "Unable to gather intelligence.";
                    }
                    if (data.status === 'unverified') {
                        this.error = "Intelligence gathered but failed verification standards.";
                    }
                },
                error: (err) => {
                    console.error('Intelligence gathering failed', err);
                    this.error = "Unable to gather intelligence at this time.";
                    this.isLoading = false;
                }
            });

        // Trigger the request
        this.intelligenceService.getIntelligence(this.context);
    }

    public close(): void {
        this.activeModal.dismiss();
    }
}

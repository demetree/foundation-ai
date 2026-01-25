
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

        this.intelligenceService.getIntelligence(this.context)
            .pipe(
                finalize(() => {
                    // unexpected termination safety
                })
            )
            .subscribe({
                next: (data) => {
                    this.dossier = data;
                    this.isLoading = false;
                },
                error: (err) => {
                    console.error('Intelligence gathering failed', err);
                    this.error = "Unable to gather intelligence at this time.";
                    this.isLoading = false;
                }
            });
    }

    public close(): void {
        this.activeModal.dismiss();
    }
}

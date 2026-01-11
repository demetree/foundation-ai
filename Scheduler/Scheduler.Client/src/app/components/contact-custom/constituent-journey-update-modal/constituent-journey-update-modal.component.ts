
import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable } from 'rxjs';
import { ConstituentJourneyStageData, ConstituentJourneyStageService } from '../../../scheduler-data-services/constituent-journey-stage.service';
import { ConstituentData, ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-constituent-journey-update-modal',
    templateUrl: './constituent-journey-update-modal.component.html',
    styleUrls: ['./constituent-journey-update-modal.component.scss']
})
export class ConstituentJourneyUpdateModalComponent implements OnInit {

    @Input() constituent!: ConstituentData;

    public stages$: Observable<ConstituentJourneyStageData[]>;
    public selectedStageId: number | bigint | null = null;
    public isSaving: boolean = false;

    constructor(
        public activeModal: NgbActiveModal,
        private constituentJourneyStageService: ConstituentJourneyStageService,
        private constituentService: ConstituentService,
        private alertService: AlertService
    ) {
        this.stages$ = this.constituentJourneyStageService.GetConstituentJourneyStageList({
            active: true,
            deleted: false,
            // Sort by sequence? The service generally returns list order, but let's hope the table default sort handles it or we map it.
            // Usually generic lists are ID sorted. We might need to sort client side.
        });
    }

    ngOnInit(): void {
        if (this.constituent) {
            this.selectedStageId = this.constituent.constituentJourneyStageId;
        }
    }

    public save(): void {
        if (!this.selectedStageId) return;

        this.isSaving = true;

        // Create submit data
        const submitData = this.constituent.ConvertToSubmitData();
        submitData.constituentJourneyStageId = this.selectedStageId;
        submitData.dateEnteredCurrentStage = new Date().toISOString(); // Update date entered

        this.constituentService.PutConstituent(this.constituent.id, submitData).subscribe({
            next: (updatedConstituent) => {
                this.alertService.showMessage('Journey stage updated successfully.', 'Success', MessageSeverity.success);
                this.activeModal.close(updatedConstituent);
            },
            error: (err) => {
                this.alertService.showMessage('Failed to update stage.', 'Error', MessageSeverity.error);
                this.isSaving = false;
            }
        });
    }

    public dismiss(): void {
        this.activeModal.dismiss();
    }
}

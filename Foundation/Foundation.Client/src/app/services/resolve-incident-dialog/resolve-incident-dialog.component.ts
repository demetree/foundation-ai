//
// Resolve Incident Dialog Component
//
// Modal dialog for resolving an incident with optional resolution note.
//
import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

export interface ResolveIncidentResult {
    confirmed: boolean;
    resolution?: string;
}

@Component({
    selector: 'app-resolve-incident-dialog',
    templateUrl: './resolve-incident-dialog.component.html',
    styleUrls: ['./resolve-incident-dialog.component.scss']
})
export class ResolveIncidentDialogComponent {
    @Input() incidentKey: string = '';
    @Input() incidentTitle: string = '';

    resolution: string = '';

    constructor(public activeModal: NgbActiveModal) { }

    confirm(): void {
        const result: ResolveIncidentResult = {
            confirmed: true,
            resolution: this.resolution.trim() || undefined
        };
        this.activeModal.close(result);
    }

    cancel(): void {
        this.activeModal.dismiss({ confirmed: false });
    }
}

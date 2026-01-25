import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

export type SessionActionType = 'revoke' | 'revoke-and-lock';

export interface SessionActionResult {
    confirmed: boolean;
    action: SessionActionType;
    reason: string;
}

@Component({
    selector: 'app-session-action-dialog',
    templateUrl: './session-action-dialog.component.html',
})
export class SessionActionDialogComponent {
    @Input() title: string = 'Session Action';
    @Input() username: string = '';
    @Input() showLockOption: boolean = true;

    selectedAction: SessionActionType = 'revoke';
    reason: string = '';

    constructor(public activeModal: NgbActiveModal) { }

    confirm(): void {
        const result: SessionActionResult = {
            confirmed: true,
            action: this.selectedAction,
            reason: this.reason.trim() || 'Administrative action'
        };
        this.activeModal.close(result);
    }

    cancel(): void {
        const result: SessionActionResult = {
            confirmed: false,
            action: 'revoke',
            reason: ''
        };
        this.activeModal.dismiss(result);
    }
}

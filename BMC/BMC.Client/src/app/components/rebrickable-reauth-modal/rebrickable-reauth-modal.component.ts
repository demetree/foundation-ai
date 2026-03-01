import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { RebrickableSyncService, ConnectRequest, SyncStatus } from '../../services/rebrickable-sync.service';

@Component({
    selector: 'app-rebrickable-reauth-modal',
    templateUrl: './rebrickable-reauth-modal.component.html',
    styleUrls: ['./rebrickable-reauth-modal.component.scss']
})
export class RebrickableReauthModalComponent implements OnInit {
    @Input() warningMessage = '';
    @Output() close = new EventEmitter<void>();
    @Output() reauthSuccess = new EventEmitter<void>();

    // Form state
    apiToken = '';
    userToken = '';
    authMode = '';
    integrationMode = '';

    submitting = false;
    error = '';
    success = false;

    constructor(private syncService: RebrickableSyncService) { }


    ngOnInit(): void {
        // Load current status to pre-fill auth mode
        this.syncService.getStatus().subscribe({
            next: (status: SyncStatus) => {
                this.authMode = status.authMode || 'TokenOnly';
                this.integrationMode = status.integrationMode || 'RealTime';
            }
        });
    }


    get showApiToken(): boolean {
        return true; // always needed
    }


    get showUserToken(): boolean {
        return this.authMode === 'TokenOnly' || this.authMode === 'SessionOnly';
    }


    get authModeLabel(): string {
        switch (this.authMode) {
            case 'ApiToken': return 'Login Once';
            case 'TokenOnly': return 'Token Only';
            case 'SessionOnly': return 'Session Only';
            default: return this.authMode;
        }
    }


    submit(): void {
        if (!this.apiToken.trim()) {
            this.error = 'API key is required.';
            return;
        }

        this.submitting = true;
        this.error = '';

        const request: ConnectRequest = {
            apiToken: this.apiToken.trim(),
            userToken: this.showUserToken ? this.userToken.trim() : undefined,
            authMode: this.authMode,
            integrationMode: this.integrationMode
        };

        this.syncService.reauthenticate(request).subscribe({
            next: (result) => {
                this.submitting = false;
                if (result.reauthenticated) {
                    this.success = true;
                    this.reauthSuccess.emit();
                    setTimeout(() => this.close.emit(), 1500);
                } else {
                    this.error = 'Re-authentication failed. Please check your credentials.';
                }
            },
            error: (err) => {
                this.submitting = false;
                this.error = err.error?.detail || err.error?.title || 'Re-authentication failed.';
            }
        });
    }


    dismiss(): void {
        this.close.emit();
    }
}

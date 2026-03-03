import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { RebrickableSyncService, SyncStatus, SyncTransaction } from '../../services/rebrickable-sync.service';
import { BrickSetSyncService, BrickSetSyncStatus, BrickSetTransaction } from '../../services/brickset-sync.service';
import { BrickLinkSyncService, BrickLinkStatus, BrickLinkTransaction } from '../../services/bricklink-sync.service';
import { BrickEconomySyncService, BrickEconomyStatus, BrickEconomyTransaction } from '../../services/brickeconomy-sync.service';
import { BrickOwlSyncService, BrickOwlStatus, BrickOwlTransaction } from '../../services/brickowl-sync.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { ConfirmationService } from '../../services/confirmation-service';


//
// Integration Management Component
//
// AI-generated component — provides a dedicated page for managing external integrations.
// Currently supports Rebrickable integration with an extensible card pattern for future
// integrations (e.g., BrickLink, BMC Public API).
//
// Extracted from the My Collection component where Rebrickable settings were previously embedded.
//


@Component({
    selector: 'app-integration-management',
    templateUrl: './integration-management.component.html',
    styleUrl: './integration-management.component.scss'
})
export class IntegrationManagementComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();


    // ───────────────────── Rebrickable State ─────────────────────

    //
    // Loading / connection state
    //
    loadingSync = false;
    syncStatus: SyncStatus | null = null;
    connecting = false;
    pulling = false;
    showTokenInput = false;
    showSyncModeEdit = false;

    //
    // Credential input fields
    //
    apiTokenInput = '';
    usernameInput = '';
    passwordInput = '';
    userTokenInput = '';

    //
    // Selected modes
    //
    selectedMode = 'RealTime';
    selectedAuthMode = 'ApiToken';

    //
    // Transaction log
    //
    transactions: SyncTransaction[] = [];
    transactionsTotalCount = 0;
    transactionsPage = 1;
    transactionsPageSize = 20;
    transactionDirectionFilter: string | null = null;
    transactionSuccessFilter: boolean | null = null;


    //
    // Auth mode options — defines the trust levels available for Rebrickable connection
    //
    authModes = [
        { value: 'ApiToken', label: 'Login Once', icon: 'fas fa-key', desc: 'API key + username/password — encrypted token stored in database' },
        { value: 'TokenOnly', label: 'Token Only', icon: 'fas fa-shield-alt', desc: 'Paste API key + user token directly — your password never touches BMC' },
        { value: 'SessionOnly', label: 'Session Only', icon: 'fas fa-lock', desc: 'API key + username/password — nothing saved to database, re-enter each session' }
    ];

    //
    // Integration mode options — defines how sync works when connected
    //
    integrationModes = [
        { value: 'None', label: 'No Integration', icon: 'fas fa-ban', desc: 'Rebrickable sync disabled' },
        { value: 'RealTime', label: 'Real-Time Sync', icon: 'fas fa-bolt', desc: 'Push & pull on every change' },
        { value: 'PushOnly', label: 'Push Only', icon: 'fas fa-upload', desc: 'BMC → Rebrickable only' },
        { value: 'ImportOnly', label: 'Import Only', icon: 'fas fa-download', desc: 'Rebrickable → BMC only' }
    ];


    // ───────────────────── BrickSet State ─────────────────────

    bsLoading = false;
    bsStatus: BrickSetSyncStatus | null = null;
    bsConnecting = false;
    bsShowForm = false;
    bsShowSyncModeEdit = false;

    // Credential inputs
    bsUsernameInput = '';
    bsPasswordInput = '';

    // Selected sync direction
    bsSelectedDirection = 'EnrichOnly';

    // Transaction log
    bsTransactions: BrickSetTransaction[] = [];
    bsTxTotalCount = 0;
    bsTxPage = 1;
    bsTxPageSize = 20;
    bsTxDirectionFilter: string | null = null;
    bsTxSuccessFilter: boolean | null = null;

    // Sync direction options
    bsSyncDirections = [
        { value: 'None', label: 'Disabled', icon: 'fas fa-ban', desc: 'No BrickSet integration' },
        { value: 'EnrichOnly', label: 'Enrich Only', icon: 'fas fa-database', desc: 'Pull pricing, ratings, and metadata into BMC sets' },
        { value: 'Full', label: 'Full Sync', icon: 'fas fa-sync-alt', desc: 'Bidirectional collection sync (future)' }
    ];


    // ───────────────────── BrickLink State ─────────────────────

    blLoading = false;
    blStatus: BrickLinkStatus | null = null;
    blConnecting = false;
    blShowForm = false;
    blTokenValueInput = '';
    blTokenSecretInput = '';

    // Transaction log
    blTransactions: BrickLinkTransaction[] = [];
    blTxTotalCount = 0;
    blTxPage = 1;
    blTxPageSize = 20;
    blTxDirectionFilter: string | null = null;
    blTxSuccessFilter: boolean | null = null;


    // ───────────────────── BrickEconomy State ─────────────────────

    beLoading = false;
    beStatus: BrickEconomyStatus | null = null;
    beConnecting = false;
    beShowForm = false;
    beApiKeyInput = '';

    // Transaction log
    beTransactions: BrickEconomyTransaction[] = [];
    beTxTotalCount = 0;
    beTxPage = 1;
    beTxPageSize = 20;
    beTxDirectionFilter: string | null = null;
    beTxSuccessFilter: boolean | null = null;


    // ───────────────────── Brick Owl State ─────────────────────

    boLoading = false;
    boStatus: BrickOwlStatus | null = null;
    boConnecting = false;
    boShowForm = false;
    boApiKeyInput = '';

    // Transaction log
    boTransactions: BrickOwlTransaction[] = [];
    boTxTotalCount = 0;
    boTxPage = 1;
    boTxPageSize = 20;
    boTxDirectionFilter: string | null = null;
    boTxSuccessFilter: boolean | null = null;


    constructor(
        private syncService: RebrickableSyncService,
        private bsSyncService: BrickSetSyncService,
        private blSyncService: BrickLinkSyncService,
        private beSyncService: BrickEconomySyncService,
        private boSyncService: BrickOwlSyncService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService
    ) { }


    ngOnInit(): void {
        this.loadRebrickableData();
        this.loadBrickSetData();
        this.loadBrickLinkData();
        this.loadBrickEconomyData();
        this.loadBrickOwlData();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    // ───────────────────── Rebrickable Data ─────────────────────

    getCurrentModeOption() {
        return this.integrationModes.find(m => m.value === this.selectedMode);
    }


    loadRebrickableData(): void {
        this.loadingSync = true;
        this.syncService.getStatus().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loadingSync = false)
        ).subscribe({
            next: (status) => {
                this.syncStatus = status;
                this.selectedMode = status.integrationMode || 'None';

                //
                // Pre-select the auth mode to match the current connection
                //
                if (status.authMode) {
                    this.selectedAuthMode = status.authMode;
                }

                this.loadTransactions();
            },
            error: () => {
                this.syncStatus = null;
                this.loadTransactions();
            }
        });
    }


    loadTransactions(): void {
        this.syncService.getTransactions(
            this.transactionsPageSize,
            this.transactionsPage,
            this.transactionDirectionFilter || undefined,
            this.transactionSuccessFilter !== null ? this.transactionSuccessFilter : undefined
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (page) => {
                this.transactions = page.results;
                this.transactionsTotalCount = page.totalCount;
            },
            error: () => {
                this.transactions = [];
                this.transactionsTotalCount = 0;
            }
        });
    }


    // ───────────────────── Connection Actions ─────────────────────

    connectToRebrickable(): void {
        if (!this.apiTokenInput.trim()) {
            return;
        }

        //
        // Validate required fields based on auth mode
        //
        if (this.selectedAuthMode === 'TokenOnly') {
            if (!this.userTokenInput.trim()) {
                return;
            }
        } else {
            if (!this.usernameInput.trim() || !this.passwordInput.trim()) {
                return;
            }
        }

        this.connecting = true;
        this.syncService.connect({
            apiToken: this.apiTokenInput.trim(),
            username: this.selectedAuthMode !== 'TokenOnly' ? this.usernameInput.trim() : undefined,
            password: this.selectedAuthMode !== 'TokenOnly' ? this.passwordInput : undefined,
            userToken: this.selectedAuthMode === 'TokenOnly' ? this.userTokenInput.trim() : undefined,
            authMode: this.selectedAuthMode,
            integrationMode: this.selectedMode
        }).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.connecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Connected', 'Successfully connected to Rebrickable!', MessageSeverity.success);
                this.clearCredentialInputs();
                this.showTokenInput = false;
                this.loadRebrickableData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Connection failed. Check your credentials.';
                this.alertService.showMessage('Connection Failed', msg, MessageSeverity.error);
            }
        });
    }


    reauthenticateRebrickable(): void {
        if (!this.apiTokenInput.trim()) {
            return;
        }

        this.connecting = true;
        this.syncService.reauthenticate({
            apiToken: this.apiTokenInput.trim(),
            username: this.selectedAuthMode !== 'TokenOnly' ? this.usernameInput.trim() : undefined,
            password: this.selectedAuthMode !== 'TokenOnly' ? this.passwordInput : undefined,
            userToken: this.selectedAuthMode === 'TokenOnly' ? this.userTokenInput.trim() : undefined,
            authMode: this.selectedAuthMode,
            integrationMode: this.selectedMode
        }).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.connecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Re-authenticated', 'Token refreshed successfully!', MessageSeverity.success);
                this.clearCredentialInputs();
                this.showTokenInput = false;
                this.loadRebrickableData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Re-authentication failed.';
                this.alertService.showMessage('Re-auth Failed', msg, MessageSeverity.error);
            }
        });
    }


    checkTokenHealth(): void {
        this.syncService.checkTokenHealth().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                if (result.valid) {
                    this.alertService.showMessage('Token Valid', 'Your Rebrickable token is healthy.', MessageSeverity.success);
                } else {
                    this.alertService.showMessage('Token Invalid', result.error || 'Token validation failed.', MessageSeverity.warn);
                }
                this.loadRebrickableData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Could not check token health.', MessageSeverity.error);
            }
        });
    }


    async disconnectFromRebrickable(): Promise<void> {
        const confirmed = await this.confirmationService.confirm(
            'Disconnect Rebrickable',
            'This will remove your API token and stop sync. Your BMC data will not be affected.'
        );

        if (!confirmed) {
            return;
        }

        this.syncService.disconnect().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Disconnected', 'Rebrickable integration disabled.', MessageSeverity.success);
                this.loadRebrickableData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to disconnect.', MessageSeverity.error);
            }
        });
    }


    updateSyncSettings(): void {
        this.syncService.updateSettings({
            integrationMode: this.selectedMode
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Settings Updated', `Integration mode set to ${this.selectedMode}`, MessageSeverity.success);
                this.loadRebrickableData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to update settings.', MessageSeverity.error);
            }
        });
    }


    triggerFullPull(): void {
        this.pulling = true;
        this.syncService.pullFull().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.pulling = false)
        ).subscribe({
            next: (result) => {
                this.alertService.showMessage(
                    'Import Complete',
                    `Created ${result.totalCreated}, updated ${result.totalUpdated}` +
                    (result.errorCount > 0 ? `, ${result.errorCount} errors` : ''),
                    result.errorCount > 0 ? MessageSeverity.warn : MessageSeverity.success
                );
                this.loadRebrickableData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Pull failed. Check the communications log.', MessageSeverity.error);
            }
        });
    }


    // ───────────────────── Transaction Filters / Pagination ─────────────────────

    setTransactionFilter(direction: string | null, success: boolean | null): void {
        this.transactionDirectionFilter = direction;
        this.transactionSuccessFilter = success;
        this.transactionsPage = 1;
        this.loadTransactions();
    }


    nextTransactionsPage(): void {
        const maxPage = Math.ceil(this.transactionsTotalCount / this.transactionsPageSize);
        if (this.transactionsPage < maxPage) {
            this.transactionsPage++;
            this.loadTransactions();
        }
    }


    prevTransactionsPage(): void {
        if (this.transactionsPage > 1) {
            this.transactionsPage--;
            this.loadTransactions();
        }
    }


    // ───────────────────── Utility ─────────────────────

    getStatusCodeClass(code: number): string {
        if (code >= 200 && code < 300) {
            return 'status-success';
        }
        if (code >= 400 && code < 500) {
            return 'status-client-error';
        }
        if (code >= 500) {
            return 'status-server-error';
        }
        return 'status-unknown';
    }


    getDirectionIcon(direction: string): string {
        return direction === 'Push' ? 'fas fa-arrow-up' : 'fas fa-arrow-down';
    }


    getDirectionClass(direction: string): string {
        return direction === 'Push' ? 'direction-push' : 'direction-pull';
    }


    formatRelativeTime(dateStr: string): string {
        const date = new Date(dateStr);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);

        if (diffMins < 1) {
            return 'just now';
        }
        if (diffMins < 60) {
            return `${diffMins}m ago`;
        }
        const diffHours = Math.floor(diffMins / 60);
        if (diffHours < 24) {
            return `${diffHours}h ago`;
        }
        const diffDays = Math.floor(diffHours / 24);
        if (diffDays < 7) {
            return `${diffDays}d ago`;
        }
        return date.toLocaleDateString();
    }


    /// <summary>
    /// Clear all credential input fields after a successful operation.
    /// </summary>
    private clearCredentialInputs(): void {
        this.apiTokenInput = '';
        this.usernameInput = '';
        this.passwordInput = '';
        this.userTokenInput = '';
    }


    // ═══════════════════════════════════════════════════════════════
    //  BRICKSET METHODS
    // ═══════════════════════════════════════════════════════════════

    loadBrickSetData(): void {
        this.bsLoading = true;
        this.bsSyncService.getStatus().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.bsLoading = false)
        ).subscribe({
            next: (status) => {
                this.bsStatus = status;
                this.bsSelectedDirection = status.syncDirection || 'None';
                this.loadBsTransactions();
            },
            error: () => {
                this.bsStatus = null;
                this.loadBsTransactions();
            }
        });
    }


    loadBsTransactions(): void {
        this.bsSyncService.getTransactions(
            this.bsTxPageSize,
            this.bsTxPage,
            this.bsTxDirectionFilter || undefined,
            this.bsTxSuccessFilter !== null ? this.bsTxSuccessFilter : undefined
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (page) => {
                this.bsTransactions = page.results;
                this.bsTxTotalCount = page.totalCount;
            },
            error: () => {
                this.bsTransactions = [];
                this.bsTxTotalCount = 0;
            }
        });
    }


    connectToBrickSet(): void {
        if (!this.bsUsernameInput.trim() || !this.bsPasswordInput.trim()) return;

        this.bsConnecting = true;
        this.bsSyncService.connect({
            username: this.bsUsernameInput.trim(),
            password: this.bsPasswordInput,
            syncDirection: this.bsSelectedDirection
        }).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.bsConnecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Connected', 'Successfully connected to BrickSet!', MessageSeverity.success);
                this.bsUsernameInput = '';
                this.bsPasswordInput = '';
                this.bsShowForm = false;
                this.loadBrickSetData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Connection failed. Check your credentials.';
                this.alertService.showMessage('Connection Failed', msg, MessageSeverity.error);
            }
        });
    }


    async disconnectFromBrickSet(): Promise<void> {
        const confirmed = await this.confirmationService.confirm(
            'Disconnect BrickSet',
            'This will remove your stored credentials and stop enrichment. Your enriched data will be preserved.'
        );
        if (!confirmed) return;

        this.bsSyncService.disconnect().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Disconnected', 'BrickSet integration disabled.', MessageSeverity.success);
                this.loadBrickSetData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to disconnect.', MessageSeverity.error);
            }
        });
    }


    checkBsHashHealth(): void {
        this.bsSyncService.checkHashHealth().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                if (result.valid) {
                    this.alertService.showMessage('Hash Valid', 'Your BrickSet session is healthy.', MessageSeverity.success);
                } else {
                    this.alertService.showMessage('Hash Invalid', result.error || 'Session expired — attempting refresh...', MessageSeverity.warn);
                }
                this.loadBrickSetData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Could not check BrickSet session health.', MessageSeverity.error);
            }
        });
    }


    updateBsSettings(): void {
        this.bsSyncService.updateSettings({
            syncDirection: this.bsSelectedDirection
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Settings Updated', `BrickSet sync direction set to ${this.bsSelectedDirection}`, MessageSeverity.success);
                this.loadBrickSetData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to update BrickSet settings.', MessageSeverity.error);
            }
        });
    }


    getCurrentBsDirectionOption() {
        return this.bsSyncDirections.find(d => d.value === this.bsSelectedDirection);
    }


    setBsTxFilter(direction: string | null, success: boolean | null): void {
        this.bsTxDirectionFilter = direction;
        this.bsTxSuccessFilter = success;
        this.bsTxPage = 1;
        this.loadBsTransactions();
    }


    nextBsTxPage(): void {
        const maxPage = Math.ceil(this.bsTxTotalCount / this.bsTxPageSize);
        if (this.bsTxPage < maxPage) {
            this.bsTxPage++;
            this.loadBsTransactions();
        }
    }


    prevBsTxPage(): void {
        if (this.bsTxPage > 1) {
            this.bsTxPage--;
            this.loadBsTransactions();
        }
    }


    // ═══════════════════════════════════════════════════════════════
    //  BRICKLINK METHODS
    // ═══════════════════════════════════════════════════════════════

    loadBrickLinkData(): void {
        this.blLoading = true;
        this.blSyncService.getStatus().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.blLoading = false)
        ).subscribe({
            next: (status) => {
                this.blStatus = status;
                this.loadBlTransactions();
            },
            error: () => {
                this.blStatus = null;
                this.loadBlTransactions();
            }
        });
    }


    connectToBrickLink(): void {
        if (!this.blTokenValueInput.trim() || !this.blTokenSecretInput.trim()) return;

        this.blConnecting = true;
        this.blSyncService.connect(this.blTokenValueInput.trim(), this.blTokenSecretInput.trim()).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.blConnecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Connected', 'Successfully connected to BrickLink!', MessageSeverity.success);
                this.blTokenValueInput = '';
                this.blTokenSecretInput = '';
                this.blShowForm = false;
                this.loadBrickLinkData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Connection failed. Check your OAuth tokens.';
                this.alertService.showMessage('Connection Failed', msg, MessageSeverity.error);
            }
        });
    }


    async disconnectFromBrickLink(): Promise<void> {
        const confirmed = await this.confirmationService.confirm(
            'Disconnect BrickLink',
            'This will remove your stored OAuth tokens. Your enriched data will be preserved.'
        );
        if (!confirmed) return;

        this.blSyncService.disconnect().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Disconnected', 'BrickLink integration disabled.', MessageSeverity.success);
                this.loadBrickLinkData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to disconnect.', MessageSeverity.error);
            }
        });
    }


    checkBlTokenHealth(): void {
        this.blSyncService.getTokenHealth().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                if (result.valid) {
                    this.alertService.showMessage('Token Valid', 'Your BrickLink OAuth tokens are healthy.', MessageSeverity.success);
                } else {
                    this.alertService.showMessage('Token Invalid', result.error || 'Token validation failed.', MessageSeverity.warn);
                }
                this.loadBrickLinkData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Could not check BrickLink token health.', MessageSeverity.error);
            }
        });
    }


    loadBlTransactions(): void {
        this.blSyncService.getTransactions(
            this.blTxPageSize,
            this.blTxPage,
            this.blTxDirectionFilter || undefined,
            this.blTxSuccessFilter !== null ? this.blTxSuccessFilter : undefined
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (page) => {
                this.blTransactions = page.results;
                this.blTxTotalCount = page.totalCount;
            },
            error: () => {
                this.blTransactions = [];
                this.blTxTotalCount = 0;
            }
        });
    }


    setBlTxFilter(direction: string | null, success: boolean | null): void {
        this.blTxDirectionFilter = direction;
        this.blTxSuccessFilter = success;
        this.blTxPage = 1;
        this.loadBlTransactions();
    }


    nextBlTxPage(): void {
        const maxPage = Math.ceil(this.blTxTotalCount / this.blTxPageSize);
        if (this.blTxPage < maxPage) {
            this.blTxPage++;
            this.loadBlTransactions();
        }
    }


    prevBlTxPage(): void {
        if (this.blTxPage > 1) {
            this.blTxPage--;
            this.loadBlTransactions();
        }
    }


    // ═══════════════════════════════════════════════════════════════
    //  BRICKECONOMY METHODS
    // ═══════════════════════════════════════════════════════════════

    loadBrickEconomyData(): void {
        this.beLoading = true;
        this.beSyncService.getStatus().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.beLoading = false)
        ).subscribe({
            next: (status) => {
                this.beStatus = status;
                this.loadBeTransactions();
            },
            error: () => {
                this.beStatus = null;
                this.loadBeTransactions();
            }
        });
    }


    connectToBrickEconomy(): void {
        if (!this.beApiKeyInput.trim()) return;

        this.beConnecting = true;
        this.beSyncService.connect(this.beApiKeyInput.trim()).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.beConnecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Connected', 'Successfully connected to BrickEconomy!', MessageSeverity.success);
                this.beApiKeyInput = '';
                this.beShowForm = false;
                this.loadBrickEconomyData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Connection failed. Check your API key.';
                this.alertService.showMessage('Connection Failed', msg, MessageSeverity.error);
            }
        });
    }


    async disconnectFromBrickEconomy(): Promise<void> {
        const confirmed = await this.confirmationService.confirm(
            'Disconnect BrickEconomy',
            'This will remove your stored API key. Your enriched data will be preserved.'
        );
        if (!confirmed) return;

        this.beSyncService.disconnect().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Disconnected', 'BrickEconomy integration disabled.', MessageSeverity.success);
                this.loadBrickEconomyData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to disconnect.', MessageSeverity.error);
            }
        });
    }


    loadBeTransactions(): void {
        this.beSyncService.getTransactions(
            this.beTxPageSize,
            this.beTxPage,
            this.beTxDirectionFilter || undefined,
            this.beTxSuccessFilter !== null ? this.beTxSuccessFilter : undefined
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (page) => {
                this.beTransactions = page.results;
                this.beTxTotalCount = page.totalCount;
            },
            error: () => {
                this.beTransactions = [];
                this.beTxTotalCount = 0;
            }
        });
    }


    setBeTxFilter(direction: string | null, success: boolean | null): void {
        this.beTxDirectionFilter = direction;
        this.beTxSuccessFilter = success;
        this.beTxPage = 1;
        this.loadBeTransactions();
    }


    nextBeTxPage(): void {
        const maxPage = Math.ceil(this.beTxTotalCount / this.beTxPageSize);
        if (this.beTxPage < maxPage) {
            this.beTxPage++;
            this.loadBeTransactions();
        }
    }


    prevBeTxPage(): void {
        if (this.beTxPage > 1) {
            this.beTxPage--;
            this.loadBeTransactions();
        }
    }


    // ═══════════════════════════════════════════════════════════════
    //  BRICK OWL METHODS
    // ═══════════════════════════════════════════════════════════════

    loadBrickOwlData(): void {
        this.boLoading = true;
        this.boSyncService.getStatus().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.boLoading = false)
        ).subscribe({
            next: (status) => {
                this.boStatus = status;
                this.loadBoTransactions();
            },
            error: () => {
                this.boStatus = null;
                this.loadBoTransactions();
            }
        });
    }


    connectToBrickOwl(): void {
        if (!this.boApiKeyInput.trim()) return;

        this.boConnecting = true;
        this.boSyncService.connect(this.boApiKeyInput.trim()).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.boConnecting = false)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Connected', 'Successfully connected to Brick Owl!', MessageSeverity.success);
                this.boApiKeyInput = '';
                this.boShowForm = false;
                this.loadBrickOwlData();
            },
            error: (err) => {
                const msg = err?.error?.error || 'Connection failed. Check your API key.';
                this.alertService.showMessage('Connection Failed', msg, MessageSeverity.error);
            }
        });
    }


    async disconnectFromBrickOwl(): Promise<void> {
        const confirmed = await this.confirmationService.confirm(
            'Disconnect Brick Owl',
            'This will remove your stored API key. Your enriched data will be preserved.'
        );
        if (!confirmed) return;

        this.boSyncService.disconnect().pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Disconnected', 'Brick Owl integration disabled.', MessageSeverity.success);
                this.loadBrickOwlData();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to disconnect.', MessageSeverity.error);
            }
        });
    }


    loadBoTransactions(): void {
        this.boSyncService.getTransactions(
            this.boTxPageSize,
            this.boTxPage,
            this.boTxDirectionFilter || undefined,
            this.boTxSuccessFilter !== null ? this.boTxSuccessFilter : undefined
        ).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (page) => {
                this.boTransactions = page.results;
                this.boTxTotalCount = page.totalCount;
            },
            error: () => {
                this.boTransactions = [];
                this.boTxTotalCount = 0;
            }
        });
    }


    setBoTxFilter(direction: string | null, success: boolean | null): void {
        this.boTxDirectionFilter = direction;
        this.boTxSuccessFilter = success;
        this.boTxPage = 1;
        this.loadBoTransactions();
    }


    nextBoTxPage(): void {
        const maxPage = Math.ceil(this.boTxTotalCount / this.boTxPageSize);
        if (this.boTxPage < maxPage) {
            this.boTxPage++;
            this.loadBoTransactions();
        }
    }


    prevBoTxPage(): void {
        if (this.boTxPage > 1) {
            this.boTxPage--;
            this.loadBoTransactions();
        }
    }
}

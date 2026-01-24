import { Component, OnInit, OnDestroy } from '@angular/core';
import { LogViewerService, LogFolder, LogFileInfo, LogEntry, RemoteApplication } from '../../../services/log-viewer.service';
import { Subject, interval, Subscription } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';

@Component({
    selector: 'app-log-viewer-listing',
    templateUrl: './log-viewer-listing.component.html',
    styleUrls: ['./log-viewer-listing.component.scss']
})
export class LogViewerListingComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();

    //
    // Remote application support
    //
    applications: RemoteApplication[] = [];
    selectedApp: string = '';  // Empty = local, otherwise app name
    loadingApps: boolean = false;

    // Data
    folders: LogFolder[] = [];
    files: LogFileInfo[] = [];
    entries: LogEntry[] = [];

    // Selection state
    selectedFolder: string = '';
    selectedFile: string = '';

    // Filters
    levelFilter: string = 'All';
    searchText: string = '';
    levels: string[] = ['All', 'Error', 'Warning', 'Information', 'System', 'Debug'];

    // Pagination
    currentPage: number = 1;
    pageSize: number = 100;
    totalCount: number = 0;

    // Level counts for badges
    levelCounts: { [level: string]: number } = {};

    // Loading states
    loadingFolders: boolean = false;
    loadingFiles: boolean = false;
    loadingEntries: boolean = false;

    // Auto-refresh
    autoRefreshEnabled: boolean = false;
    autoRefreshInterval: number = 10;
    autoRefreshCountdown: number = 10;
    private autoRefreshSubscription: Subscription | null = null;
    private countdownSubscription: Subscription | null = null;

    // Expanded rows
    expandedRows: Set<number> = new Set();

    // Make Math available in template
    Math = Math;


    constructor(private logViewerService: LogViewerService) { }


    ngOnInit(): void {
        this.loadApplications();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.stopAutoRefresh();
    }


    //
    // Application selection
    //
    private loadApplications(): void {
        this.loadingApps = true;
        this.logViewerService.getRemoteApplications()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (apps) => {
                    this.applications = apps;
                    this.loadingApps = false;

                    // Find local app (isSelf=true) and select it by default
                    const localApp = apps.find(a => a.isSelf);
                    if (localApp) {
                        this.selectedApp = localApp.name;
                    } else if (apps.length > 0) {
                        this.selectedApp = apps[0].name;
                    }

                    this.loadFolders();
                },
                error: (error) => {
                    console.error('Error loading applications:', error);
                    this.loadingApps = false;
                    // Fallback to local-only mode
                    this.loadFolders();
                }
            });
    }


    onAppChange(): void {
        this.folders = [];
        this.files = [];
        this.entries = [];
        this.selectedFolder = '';
        this.selectedFile = '';
        this.loadFolders();
    }


    private loadFolders(): void {
        this.loadingFolders = true;

        //
        // Use remote or local endpoint based on selected app
        //
        const isLocalApp = this.applications.find(a => a.name === this.selectedApp)?.isSelf ?? true;

        const folderObservable = isLocalApp || !this.selectedApp
            ? this.logViewerService.getFolders()
            : this.logViewerService.getRemoteFolders(this.selectedApp);

        folderObservable
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (folders) => {
                    this.folders = folders;
                    this.loadingFolders = false;

                    // Auto-select first folder if available
                    if (folders.length > 0 && !this.selectedFolder) {
                        this.selectedFolder = folders[0].name;
                        this.onFolderChange();
                    }
                },
                error: (error) => {
                    console.error('Error loading folders:', error);
                    this.loadingFolders = false;
                }
            });
    }


    onFolderChange(): void {
        if (!this.selectedFolder) {
            this.files = [];
            this.entries = [];
            return;
        }

        this.loadingFiles = true;
        this.selectedFile = '';
        this.entries = [];

        //
        // Use remote or local endpoint based on selected app
        //
        const isLocalApp = this.applications.find(a => a.name === this.selectedApp)?.isSelf ?? true;

        const filesObservable = isLocalApp || !this.selectedApp
            ? this.logViewerService.getFiles(this.selectedFolder)
            : this.logViewerService.getRemoteFiles(this.selectedApp, this.selectedFolder);

        filesObservable
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (files) => {
                    this.files = files;
                    this.loadingFiles = false;

                    // Auto-select most recent file (first in list, since sorted by date desc)
                    if (files.length > 0) {
                        this.selectedFile = files[0].fileName;
                        this.loadEntries();
                    }
                },
                error: (error) => {
                    console.error('Error loading files:', error);
                    this.loadingFiles = false;
                }
            });
    }


    onFileChange(): void {
        if (this.selectedFile) {
            this.currentPage = 1;
            this.loadEntries();
        }
    }


    loadEntries(): void {
        if (!this.selectedFolder || !this.selectedFile) {
            return;
        }

        this.loadingEntries = true;
        const skip = (this.currentPage - 1) * this.pageSize;

        //
        // Use remote or local endpoint based on selected app
        //
        const isLocalApp = this.applications.find(a => a.name === this.selectedApp)?.isSelf ?? true;

        const entriesObservable = isLocalApp || !this.selectedApp
            ? this.logViewerService.getEntries(
                this.selectedFolder,
                this.selectedFile,
                skip,
                this.pageSize,
                this.levelFilter,
                this.searchText
            )
            : this.logViewerService.getRemoteEntries(
                this.selectedApp,
                this.selectedFolder,
                this.selectedFile,
                skip,
                this.pageSize,
                this.levelFilter,
                this.searchText
            );

        entriesObservable
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (response) => {
                    this.entries = response.entries;
                    this.totalCount = response.totalCount;
                    this.levelCounts = response.levelCounts || {};
                    this.loadingEntries = false;
                },
                error: (error) => {
                    console.error('Error loading entries:', error);
                    this.loadingEntries = false;
                }
            });
    }


    refresh(): void {
        this.loadEntries();
    }


    applyFilters(): void {
        this.currentPage = 1;
        this.loadEntries();
    }


    clearFilters(): void {
        this.levelFilter = 'All';
        this.searchText = '';
        this.currentPage = 1;
        this.loadEntries();
    }


    //
    // Pagination
    //

    get totalPages(): number {
        return Math.ceil(this.totalCount / this.pageSize);
    }


    goToPage(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
            this.loadEntries();
        }
    }


    get visiblePages(): number[] {
        const pages: number[] = [];
        const start = Math.max(1, this.currentPage - 2);
        const end = Math.min(this.totalPages, this.currentPage + 2);

        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }


    //
    // Row expansion
    //

    toggleRow(lineNumber: number): void {
        if (this.expandedRows.has(lineNumber)) {
            this.expandedRows.delete(lineNumber);
        } else {
            this.expandedRows.add(lineNumber);
        }
    }


    isRowExpanded(lineNumber: number): boolean {
        return this.expandedRows.has(lineNumber);
    }


    //
    // Auto-refresh
    //

    toggleAutoRefresh(): void {
        this.autoRefreshEnabled = !this.autoRefreshEnabled;
        if (this.autoRefreshEnabled) {
            this.startAutoRefresh();
        } else {
            this.stopAutoRefresh();
        }
    }


    setAutoRefreshInterval(seconds: number): void {
        this.autoRefreshInterval = seconds;
        if (this.autoRefreshEnabled) {
            this.stopAutoRefresh();
            this.startAutoRefresh();
        }
    }


    private startAutoRefresh(): void {
        this.autoRefreshCountdown = this.autoRefreshInterval;

        // Countdown timer
        this.countdownSubscription = interval(1000)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
                this.autoRefreshCountdown--;
                if (this.autoRefreshCountdown <= 0) {
                    this.autoRefreshCountdown = this.autoRefreshInterval;
                }
            });

        // Refresh timer
        this.autoRefreshSubscription = interval(this.autoRefreshInterval * 1000)
            .pipe(
                takeUntil(this.destroy$),
                switchMap(() => {
                    this.loadingEntries = true;
                    const skip = (this.currentPage - 1) * this.pageSize;
                    return this.logViewerService.getEntries(
                        this.selectedFolder,
                        this.selectedFile,
                        skip,
                        this.pageSize,
                        this.levelFilter,
                        this.searchText
                    );
                })
            )
            .subscribe({
                next: (response) => {
                    this.entries = response.entries;
                    this.totalCount = response.totalCount;
                    this.levelCounts = response.levelCounts || {};
                    this.loadingEntries = false;
                    this.autoRefreshCountdown = this.autoRefreshInterval;
                },
                error: (error) => {
                    console.error('Auto-refresh error:', error);
                    this.loadingEntries = false;
                }
            });
    }


    private stopAutoRefresh(): void {
        if (this.autoRefreshSubscription) {
            this.autoRefreshSubscription.unsubscribe();
            this.autoRefreshSubscription = null;
        }
        if (this.countdownSubscription) {
            this.countdownSubscription.unsubscribe();
            this.countdownSubscription = null;
        }
        this.autoRefreshCountdown = this.autoRefreshInterval;
    }


    //
    // UI Helpers
    //

    getLevelClass(level: string): string {
        switch (level.toLowerCase()) {
            case 'error': return 'level-error';
            case 'warning': return 'level-warning';
            case 'information': return 'level-info';
            case 'system': return 'level-system';
            case 'debug': return 'level-debug';
            default: return 'level-default';
        }
    }


    getLevelBadgeClass(level: string): string {
        switch (level.toLowerCase()) {
            case 'error': return 'bg-danger';
            case 'warning': return 'bg-warning text-dark';
            case 'information': return 'bg-info text-dark';
            case 'system': return 'bg-primary';
            case 'debug': return 'bg-secondary';
            default: return 'bg-secondary';
        }
    }


    isMultiLine(message: string): boolean {
        return message?.includes('\n') || (message?.length > 150);
    }


    truncateMessage(message: string, maxLength: number = 150): string {
        if (!message) return '';
        const firstLine = message.split('\n')[0];
        if (firstLine.length <= maxLength) return firstLine;
        return firstLine.substring(0, maxLength) + '...';
    }


    getSelectedFileErrors(): number {
        const file = this.files.find(f => f.fileName === this.selectedFile);
        return file?.errorCount || 0;
    }


    //
    // Download Methods
    //
    downloadCurrentFile(): void {
        if (!this.selectedFolder || !this.selectedFile) {
            return;
        }

        this.logViewerService.downloadFile(this.selectedFolder, this.selectedFile).subscribe({
            next: (blob) => {
                this.saveBlobAsFile(blob, this.selectedFile);
            },
            error: (error) => {
                console.error('Failed to download file:', error);
            }
        });
    }


    downloadAllFiles(): void {
        if (!this.selectedFolder) {
            return;
        }

        //
        // Generate a meaningful filename
        //
        const sanitizedFolderName = this.selectedFolder.replace(/\s/g, '_').replace(/[()]/g, '');
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
        const zipFileName = `logs_${sanitizedFolderName}_${timestamp}.zip`;

        this.logViewerService.downloadAllFiles(this.selectedFolder).subscribe({
            next: (blob) => {
                this.saveBlobAsFile(blob, zipFileName);
            },
            error: (error) => {
                console.error('Failed to download all files:', error);
            }
        });
    }


    private saveBlobAsFile(blob: Blob, fileName: string): void {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
    }
}

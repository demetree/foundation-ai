import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';


/**
 *
 * MOCHub Repo — GitHub-style repository detail page for a single published MOC.
 *
 * Displays the MOC header (name, author, star/fork/visibility badges),
 * tabbed content (README, Versions, Forks), version history, and action buttons.
 *
 * API: GET /api/mochub/moc/{id}
 * API: GET /api/mochub/moc/{id}/versions
 * API: GET /api/mochub/moc/{id}/forks
 * API: POST /api/mochub/moc/{id}/commit
 * API: POST /api/mochub/moc/{id}/fork
 *
 */
@Component({
    selector: 'app-mochub-repo',
    templateUrl: './mochub-repo.component.html',
    styleUrl: './mochub-repo.component.scss'
})
export class MochubRepoComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    //
    // Core data
    //
    moc: any = null;
    versions: any[] = [];
    forks: any[] = [];
    loading = true;
    versionsLoading = true;

    //
    // UI state
    //
    activeTab: 'readme' | 'versions' | 'forks' = 'readme';
    commitModalOpen = false;
    commitMessage = '';
    committing = false;
    forking = false;
    editingReadme = false;
    readmeEditText = '';
    savingReadme = false;

    //
    // Route params
    //
    mocId: number = 0;

    //
    // Ownership
    //  NOTE: multi-tenancy bypass — we compare tenantGuid in the response
    //  with the logged-in user's tenantGuid. The public API returns MOCs from
    //  any tenant, so we check ownership client-side for the action buttons.
    //
    isOwner = false;


    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private http: HttpClient,
        private authService: AuthService
    ) { }


    ngOnInit(): void {
        this.route.params.pipe(
            takeUntil(this.destroy$),
            switchMap(params => {
                this.mocId = +params['id'];
                this.loading = true;
                return this.http.get<any>(`/api/mochub/moc/${this.mocId}`);
            })
        ).subscribe({
            next: (moc) => {
                this.moc = moc;
                this.loading = false;

                //
                // Ownership detection:
                // The JWT token doesn't expose tenantGuid, so for now we show
                // owner actions (commit, settings) to any logged-in user.
                // In a multi-tenant environment, the server still enforces
                // actual ownership via tenantGuid comparison on write endpoints.
                //
                this.isOwner = this.authService.isLoggedIn;

                this.loadVersions();
                this.loadForks();
            },
            error: () => {
                this.loading = false;
            }
        });
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Data Loading
    //

    loadVersions(): void {
        this.versionsLoading = true;
        this.http.get<any>(`/api/mochub/moc/${this.mocId}/versions`).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (response) => {
                this.versions = response?.items || response || [];
                this.versionsLoading = false;
            },
            error: () => {
                this.versions = [];
                this.versionsLoading = false;
            }
        });
    }

    loadForks(): void {
        this.http.get<any[]>(`/api/mochub/moc/${this.mocId}/forks`).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (forks) => { this.forks = forks || []; },
            error: () => { this.forks = []; }
        });
    }


    //
    // Actions
    //

    commitVersion(): void {
        if (!this.commitMessage.trim() || this.committing) return;

        this.committing = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.post<any>(`/api/mochub/moc/${this.mocId}/commit`, {
            commitMessage: this.commitMessage
        }, { headers }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.committing = false;
                this.commitModalOpen = false;
                this.commitMessage = '';
                this.loadVersions();
            },
            error: () => {
                this.committing = false;
            }
        });
    }


    forkMoc(): void {
        if (this.forking) return;

        this.forking = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.post<any>(`/api/mochub/moc/${this.mocId}/fork`, {}, { headers }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                this.forking = false;
                // Navigate to the newly forked MOC
                this.router.navigate(['/mochub/moc', result.newPublishedMocId]);
            },
            error: () => {
                this.forking = false;
            }
        });
    }


    //
    // README Editing
    //

    startEditReadme(): void {
        this.editingReadme = true;
        this.readmeEditText = this.moc?.readmeMarkdown || '';
    }

    cancelEditReadme(): void {
        this.editingReadme = false;
        this.readmeEditText = '';
    }

    saveReadme(): void {
        if (this.savingReadme) return;

        this.savingReadme = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.put<any>(`/api/mochub/moc/${this.mocId}`, {
            readmeMarkdown: this.readmeEditText
        }, { headers }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.moc.readmeMarkdown = this.readmeEditText;
                this.editingReadme = false;
                this.savingReadme = false;
                this.readmeEditText = '';
            },
            error: () => {
                this.savingReadme = false;
            }
        });
    }


    //
    // Navigation
    //

    setTab(tab: 'readme' | 'versions' | 'forks'): void {
        this.activeTab = tab;
    }

    goToExplore(): void {
        this.router.navigate(['/mochub']);
    }

    goToVersion(version: any): void {
        // Future: navigate to version diff view
    }


    //
    // Helpers
    //

    formatDate(dateString: string): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }

    formatRelativeDate(dateString: string): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

        if (diffDays === 0) return 'today';
        if (diffDays === 1) return 'yesterday';
        if (diffDays < 30) return `${diffDays} days ago`;
        if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`;
        return `${Math.floor(diffDays / 365)} years ago`;
    }

    formatNumber(n: number): string {
        if (n == null) return '0';
        if (n >= 1000) return (n / 1000).toFixed(1) + 'k';
        return n.toString();
    }

    getDeltaClass(delta: number | null): string {
        if (delta == null || delta === 0) return '';
        return delta > 0 ? 'delta-add' : 'delta-remove';
    }

    getDeltaText(delta: number | null): string {
        if (delta == null || delta === 0) return '';
        return delta > 0 ? `+${delta}` : `${delta}`;
    }

    getThumbnailUrl(): string {
        return `/api/mochub/moc/${this.mocId}/thumbnail`;
    }
}

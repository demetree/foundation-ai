import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';


/**
 *
 * MOCHub Explore — the main landing page for the GitHub-style MOC publishing platform.
 *
 * Shows trending, recent, and featured public MOCs with search and sort controls.
 * Accessible to both anonymous and authenticated users.
 *
 * API: GET /api/mochub/explore
 * API: GET /api/mochub/explore/search
 *
 */
@Component({
    selector: 'app-mochub-explore',
    templateUrl: './mochub-explore.component.html',
    styleUrl: './mochub-explore.component.scss'
})
export class MochubExploreComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    mocs: any[] = [];
    loading = true;
    errorMessage = '';
    totalCount = 0;
    pageNumber = 1;
    pageSize = 20;

    searchTerm = '';
    sortBy = 'trending';
    tags = '';
    publishModalOpen = false;
    thumbnailLoaded = new Set<number>();

    // My MOCs
    myMocs: any[] = [];
    myMocsLoading = false;

    sortOptions = [
        { value: 'trending', label: 'Trending', icon: 'fas fa-fire' },
        { value: 'recent', label: 'Recently Published', icon: 'fas fa-clock' },
        { value: 'stars', label: 'Most Liked', icon: 'fas fa-star' },
        { value: 'forks', label: 'Most Forked', icon: 'fas fa-code-branch' },
        { value: 'parts', label: 'Most Parts', icon: 'fas fa-cubes' }
    ];


    constructor(
        private router: Router,
        private http: HttpClient,
        public authService: AuthService
    ) { }


    ngOnInit(): void {
        this.searchSubject.pipe(
            debounceTime(400),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.pageNumber = 1;
            this.loadMocs();
        });

        this.loadMocs();

        if (this.authService.isLoggedIn) {
            this.loadMyMocs();
        }
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    loadMocs(): void {
        this.loading = true;

        const isSearch = !!this.searchTerm || !!this.tags;
        const baseUrl = isSearch ? '/api/mochub/explore/search' : '/api/mochub/explore';

        const params: any = {
            pageSize: this.pageSize.toString(),
            pageNumber: this.pageNumber.toString(),
            sort: this.sortBy
        };

        if (this.searchTerm) params.q = this.searchTerm;
        if (this.tags) params.tags = this.tags;

        this.http.get<any>(baseUrl, { params }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (response) => {
                this.mocs = response.items || [];
                this.totalCount = response.totalCount || 0;
                this.loading = false;
                this.errorMessage = '';
            },
            error: () => {
                this.mocs = [];
                this.loading = false;
                this.errorMessage = 'Failed to load MOCs. Please try again.';
            }
        });
    }


    loadMyMocs(): void {
        this.myMocsLoading = true;

        this.http.get<any[]>('/api/mochub/my-mocs', {
            headers: this.authService.GetAuthenticationHeaders()
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (mocs) => {
                this.myMocs = mocs || [];
                this.myMocsLoading = false;
            },
            error: () => {
                this.myMocs = [];
                this.myMocsLoading = false;
            }
        });
    }


    //
    // Event handlers
    //

    onSearch(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchSubject.next(term);
    }

    setSort(sort: string): void {
        this.sortBy = sort;
        this.pageNumber = 1;
        this.loadMocs();
    }

    navigateToMoc(moc: any): void {
        this.router.navigate(['/mochub/moc', moc.id]);
    }

    nextPage(): void {
        if (this.pageNumber * this.pageSize < this.totalCount) {
            this.pageNumber++;
            this.loadMocs();
        }
    }

    prevPage(): void {
        if (this.pageNumber > 1) {
            this.pageNumber--;
            this.loadMocs();
        }
    }

    openPublishModal(): void {
        this.publishModalOpen = true;
    }

    onPublishModalClosed(): void {
        this.publishModalOpen = false;
    }

    onMocPublished(): void {
        this.publishModalOpen = false;
        this.loadMocs();
    }


    //
    // Admin — Seed MOC from Steps
    //

    showSeedForm = false;
    seedProjectId: number | null = null;
    seeding = false;
    seedResult: any = null;
    seedError = '';
    seedProjects: any[] = [];
    seedProjectsLoading = false;

    get isAdmin(): boolean {
        return this.authService?.isBMCAdministrator === true;
    }

    toggleSeedForm(): void {
        this.showSeedForm = !this.showSeedForm;
        this.seedResult = null;
        this.seedError = '';

        if (this.showSeedForm && this.seedProjects.length === 0) {
            this.loadSeedProjects();
        }
    }

    loadSeedProjects(): void {
        this.seedProjectsLoading = true;

        this.http.get<any[]>('/api/projects', {
            headers: this.authService.GetAuthenticationHeaders()
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (projects) => {
                this.seedProjects = (projects || []).sort((a, b) =>
                    (a.name || a.projectName || '').localeCompare(b.name || b.projectName || '')
                );
                this.seedProjectsLoading = false;
            },
            error: () => {
                this.seedProjects = [];
                this.seedProjectsLoading = false;
                this.seedError = 'Failed to load projects.';
            }
        });
    }

    seedMocFromSteps(): void {
        if (!this.seedProjectId || this.seeding) return;

        this.seeding = true;
        this.seedResult = null;
        this.seedError = '';

        this.http.post<any>('/api/mochub/admin/seed-from-steps', {
            projectId: this.seedProjectId
        }, {
            headers: this.authService.GetAuthenticationHeaders()
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                this.seedResult = result;
                this.seeding = false;
            },
            error: (err) => {
                this.seedError = err.error?.detail || err.error || 'Seed failed. Check the project.';
                this.seeding = false;
            }
        });
    }


    //
    // Helpers
    //

    get totalPages(): number {
        return Math.ceil(this.totalCount / this.pageSize);
    }

    formatDate(dateString: string): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }

    formatNumber(n: number): string {
        if (n == null) return '0';
        if (n >= 1000) return (n / 1000).toFixed(1) + 'k';
        return n.toString();
    }
}

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, finalize } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ProjectService, ProjectSummary } from '../../services/project.service';
import { AuthService } from '../../services/auth.service';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { ConfirmationService } from '../../services/confirmation-service';
import { UploadModelModalComponent } from '../upload-model-modal/upload-model-modal.component';
import { ManualEditorService } from '../../services/manual-editor.service';


@Component({
    selector: 'app-my-projects',
    templateUrl: './my-projects.component.html',
    styleUrl: './my-projects.component.scss'
})
export class MyProjectsComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    // State
    loading = true;
    projects: ProjectSummary[] = [];
    filteredProjects: ProjectSummary[] = [];
    displayedProjects: ProjectSummary[] = [];
    searchTerm = '';
    viewMode: 'grid' | 'list' = 'grid';
    sortBy: 'recent' | 'name' | 'parts' = 'recent';
    thumbnailUrls: Map<number, string> = new Map();

    // Pagination
    pageSize = 12;
    currentPage = 1;
    totalPages = 1;


    constructor(
        public router: Router,
        private http: HttpClient,
        private projectService: ProjectService,
        private authService: AuthService,
        private alertService: AlertService,
        private confirmationService: ConfirmationService,
        private modalService: NgbModal,
        private manualEditorService: ManualEditorService
    ) { }


    ngOnInit(): void {
        this.searchSubject.pipe(
            debounceTime(250),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.currentPage = 1;
            this.applyFilters();
        });

        this.loadProjects();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    // ───────────────────── Data Loading ─────────────────────

    loadProjects(): void {
        this.loading = true;
        this.projectService.getMyProjects().pipe(
            takeUntil(this.destroy$),
            finalize(() => this.loading = false)
        ).subscribe({
            next: (projects) => {
                this.projects = projects;
                this.applyFilters();
                this.loadThumbnails(projects);
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load projects', MessageSeverity.error);
            }
        });
    }


    /**
     * Fetch thumbnails via authenticated HTTP and create blob URLs.
     * <img> tags can't send Authorization headers, so we fetch as blobs.
     */
    private loadThumbnails(projects: ProjectSummary[]): void {
        const headers = this.authService.GetAuthenticationHeaders().delete('Content-Type');

        for (const project of projects) {
            this.http.get(`/api/moc/project/${project.id}/thumbnail`, {
                headers: headers,
                responseType: 'blob'
            }).subscribe({
                next: (blob) => {
                    if (blob.size > 0) {
                        this.thumbnailUrls.set(project.id, URL.createObjectURL(blob));
                    }
                },
                error: () => { /* no thumbnail — leave the placeholder */ }
            });
        }
    }


    getThumbnailUrl(projectId: number): string | null {
        return this.thumbnailUrls.get(projectId) ?? null;
    }


    // ───────────────────── Filtering & Sorting ─────────────────────

    onSearch(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchSubject.next(term);
    }


    applyFilters(): void {
        let result = [...this.projects];

        // Search
        if (this.searchTerm) {
            const lower = this.searchTerm.toLowerCase();
            result = result.filter(p =>
                p.name?.toLowerCase().includes(lower) ||
                p.description?.toLowerCase().includes(lower) ||
                p.notes?.toLowerCase().includes(lower)
            );
        }

        // Sort
        switch (this.sortBy) {
            case 'recent':
                result.sort((a, b) => {
                    const da = a.lastBuildDate ? new Date(a.lastBuildDate).getTime() : 0;
                    const db = b.lastBuildDate ? new Date(b.lastBuildDate).getTime() : 0;
                    return db - da;
                });
                break;
            case 'name':
                result.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
                break;
            case 'parts':
                result.sort((a, b) => (b.partCount || 0) - (a.partCount || 0));
                break;
        }

        this.filteredProjects = result;
        this.totalPages = Math.max(1, Math.ceil(result.length / this.pageSize));
        if (this.currentPage > this.totalPages) this.currentPage = 1;
        this.updateDisplayedProjects();
    }


    updateDisplayedProjects(): void {
        const start = (this.currentPage - 1) * this.pageSize;
        this.displayedProjects = this.filteredProjects.slice(start, start + this.pageSize);
    }


    setSort(sortBy: 'recent' | 'name' | 'parts'): void {
        this.sortBy = sortBy;
        this.currentPage = 1;
        this.applyFilters();
    }


    // ───────────────────── Pagination ─────────────────────

    nextPage(): void {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.updateDisplayedProjects();
        }
    }


    prevPage(): void {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.updateDisplayedProjects();
        }
    }


    goToPage(page: number): void {
        this.currentPage = page;
        this.updateDisplayedProjects();
    }


    getVisiblePages(): number[] {
        const pages: number[] = [];
        const start = Math.max(1, this.currentPage - 2);
        const end = Math.min(this.totalPages, start + 4);
        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }


    // ───────────────────── Actions ─────────────────────

    openUploadModal(): void {
        const modalRef = this.modalService.open(UploadModelModalComponent, {
            centered: true,
            size: 'lg',
            backdrop: 'static',
            keyboard: true
        });

        modalRef.result.then(
            (result) => {
                if (result) {
                    this.alertService.showMessage(
                        'Import Complete',
                        `Created project "${result.projectName}" with ${result.totalPartCount} parts (${result.resolvedPartCount} resolved)`,
                        MessageSeverity.success
                    );
                    this.loadProjects();
                }
            },
            () => { /* dismissed */ }
        );
    }


    async deleteProject(project: ProjectSummary, event: Event): Promise<void> {
        event.stopPropagation();

        const confirmed = await this.confirmationService.confirm(
            'Delete Project',
            `Delete "${project.name}"? This action cannot be undone.`
        );

        if (!confirmed) return;

        this.projectService.deleteProject(project.id).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.alertService.showMessage('Deleted', `"${project.name}" has been deleted`, MessageSeverity.success);
                this.loadProjects();
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to delete project', MessageSeverity.error);
            }
        });
    }


    createManual(project: ProjectSummary, event: Event): void {
        event.stopPropagation();

        this.manualEditorService.createManual({
            projectId: project.id,
            name: `${project.name || 'Untitled'} — Instructions`,
            description: `Build instructions for ${project.name}`,
            pageWidthMm: 210,
            pageHeightMm: 297,
            isPublished: false,
            active: true,
            deleted: false
        }).subscribe({
            next: (manual) => {
                this.alertService.showMessage(
                    'Manual Created',
                    `Created instruction manual for "${project.name}"`,
                    MessageSeverity.success
                );
                this.router.navigate(['/manual-editor', manual.id]);
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to create manual', MessageSeverity.error);
            }
        });
    }


    // ───────────────────── Utility ─────────────────────

    getTotalParts(): number {
        return this.projects.reduce((sum, p) => sum + (p.partCount || 0), 0);
    }

    getRecentCount(): number {
        const oneWeekAgo = new Date();
        oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);
        return this.projects.filter(p => {
            if (!p.lastBuildDate) return false;
            return new Date(p.lastBuildDate) >= oneWeekAgo;
        }).length;
    }

    getRelativeDate(dateStr: string | null): string {
        if (!dateStr) return 'Never';
        const date = new Date(dateStr);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;
        if (diffDays < 30) return `${Math.floor(diffDays / 7)}w ago`;
        return date.toLocaleDateString();
    }

    getFormatIcon(name: string): string {
        if (!name) return 'fas fa-file';
        const lower = name.toLowerCase();
        if (lower.endsWith('.io')) return 'fas fa-cube';
        if (lower.endsWith('.mpd')) return 'fas fa-layer-group';
        if (lower.endsWith('.ldr')) return 'fas fa-file-code';
        if (lower.endsWith('.lxf')) return 'fas fa-drafting-compass';
        return 'fas fa-file';
    }
}

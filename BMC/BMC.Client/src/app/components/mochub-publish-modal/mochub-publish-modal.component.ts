import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';


/**
 *
 * MOCHub Publish Modal — allows users to publish one of their projects
 * to MOCHub as a public (or private) MOC.
 *
 * Workflow:
 * 1. Loads user's projects from the API
 * 2. User selects a project
 * 3. User fills in name, description, tags, visibility, README
 * 4. Submits to POST /api/mochub/publish
 *
 * Opened from the mochub-explore page via a "Publish" button (visible when logged in).
 *
 */
@Component({
    selector: 'app-mochub-publish-modal',
    templateUrl: './mochub-publish-modal.component.html',
    styleUrl: './mochub-publish-modal.component.scss'
})
export class MochubPublishModalComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    @Output() closed = new EventEmitter<void>();
    @Output() published = new EventEmitter<any>();

    //
    // Wizard state
    //
    step: 'select' | 'details' | 'publishing' | 'done' = 'select';

    //
    // Projects
    //
    projects: any[] = [];
    projectsLoading = true;
    selectedProject: any = null;

    //
    // MOC details
    //
    mocName = '';
    mocDescription = '';
    mocTags = '';
    mocVisibility = 'Public';
    mocReadme = '';
    commitMessage = 'Initial publish';
    allowForking = true;

    //
    // Status
    //
    publishing = false;
    errorMessage = '';
    publishedMocId: number | null = null;


    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    ngOnInit(): void {
        this.loadProjects();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    //
    // Data loading
    //

    loadProjects(): void {
        this.projectsLoading = true;

        this.http.get<any[]>('/api/projects', {
            headers: this.authService.GetAuthenticationHeaders()
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (projects) => {
                this.projects = projects || [];
                this.projectsLoading = false;
            },
            error: () => {
                this.projects = [];
                this.projectsLoading = false;
            }
        });
    }


    //
    // Wizard navigation
    //

    selectProject(project: any): void {
        this.selectedProject = project;
        this.mocName = project.name || project.projectName || '';
        this.mocDescription = project.description || '';
        this.step = 'details';
    }

    goBack(): void {
        if (this.step === 'details') {
            this.step = 'select';
            this.selectedProject = null;
        }
    }


    //
    // Publish
    //

    publish(): void {
        if (!this.mocName.trim() || this.publishing) return;

        this.publishing = true;
        this.errorMessage = '';
        this.step = 'publishing';

        const body = {
            projectId: this.selectedProject.id || this.selectedProject.projectId,
            name: this.mocName.trim(),
            description: this.mocDescription.trim(),
            tags: this.mocTags.trim(),
            visibility: this.mocVisibility,
            readmeMarkdown: this.mocReadme.trim(),
            commitMessage: this.commitMessage.trim(),
            allowForking: this.allowForking
        };

        this.http.post<any>('/api/mochub/publish', body, {
            headers: this.authService.GetAuthenticationHeaders()
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (result) => {
                this.publishing = false;
                this.publishedMocId = result.publishedMocId || result.id;
                this.step = 'done';
                this.published.emit(result);
            },
            error: (err) => {
                this.publishing = false;
                this.errorMessage = err.error?.message || 'Failed to publish MOC. Please try again.';
                this.step = 'details';
            }
        });
    }


    //
    // Actions
    //

    close(): void {
        this.closed.emit();
    }

    formatDate(dateString: string): string {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }
}

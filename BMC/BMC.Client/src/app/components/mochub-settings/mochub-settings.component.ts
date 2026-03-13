import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';


/**
 *
 * MOCHub Settings — manages MOC metadata, visibility, and danger zone actions.
 *
 * Owner-only page accessible from the repo detail header via the ⚙ Settings button.
 *
 * API: GET  /api/mochub/moc/{id}        — load current metadata
 * API: PUT  /api/mochub/moc/{id}        — update metadata
 * API: DELETE /api/mochub/moc/{id}      — unpublish (soft-delete)
 *
 */
@Component({
    selector: 'app-mochub-settings',
    templateUrl: './mochub-settings.component.html',
    styleUrl: './mochub-settings.component.scss'
})
export class MochubSettingsComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    //
    // MOC data
    //
    moc: any = null;
    mocId: number = 0;
    loading = true;

    //
    // Form state
    //
    name = '';
    description = '';
    tags = '';
    visibility = 'Public';
    licenseName = '';
    allowForking = true;
    saving = false;
    saveSuccess = false;

    //
    // Danger zone
    //
    unpublishConfirmText = '';
    unpublishing = false;

    visibilityOptions = [
        { value: 'Public', label: 'Public', icon: 'fas fa-globe-americas', desc: 'Anyone can see this MOC' },
        { value: 'Unlisted', label: 'Unlisted', icon: 'fas fa-eye-slash', desc: 'Only accessible via direct link' },
        { value: 'Private', label: 'Private', icon: 'fas fa-lock', desc: 'Only you can see this MOC' }
    ];

    licenseOptions = [
        '', 'CC BY 4.0', 'CC BY-SA 4.0', 'CC BY-NC 4.0', 'CC BY-NC-SA 4.0', 'CC0 1.0', 'MIT', 'All Rights Reserved'
    ];


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
                // Populate form with current values
                //
                this.name = moc.name || '';
                this.description = moc.description || '';
                this.tags = moc.tags || '';
                this.visibility = moc.visibility || 'Public';
                this.licenseName = moc.licenseName || '';
                this.allowForking = moc.allowForking !== false;
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
    // Save metadata
    //

    saveSettings(): void {
        if (this.saving) return;

        this.saving = true;
        this.saveSuccess = false;

        const headers = this.authService.GetAuthenticationHeaders();
        const body = {
            name: this.name,
            description: this.description,
            tags: this.tags,
            visibility: this.visibility,
            licenseName: this.licenseName,
            allowForking: this.allowForking
        };

        this.http.put<any>(`/api/mochub/moc/${this.mocId}`, body, { headers }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.saving = false;
                this.saveSuccess = true;
                setTimeout(() => this.saveSuccess = false, 3000);
            },
            error: () => {
                this.saving = false;
            }
        });
    }


    //
    // Danger zone — unpublish
    //

    unpublish(): void {
        if (this.unpublishing) return;
        if (this.unpublishConfirmText !== this.moc?.name) return;

        this.unpublishing = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.delete(`/api/mochub/moc/${this.mocId}`, { headers }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: () => {
                this.unpublishing = false;
                this.router.navigate(['/mochub']);
            },
            error: () => {
                this.unpublishing = false;
            }
        });
    }


    //
    // Navigation
    //

    goBack(): void {
        this.router.navigate(['/mochub/moc', this.mocId]);
    }
}

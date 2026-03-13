import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';

import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';


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
    // Version diff viewer state
    //
    selectedVersion: any = null;
    diffData: any = null;
    diffLoading = false;
    diff3dActive = false;
    diff3dLoading = false;

    //
    // Three.js diff viewer objects
    //
    @ViewChild('diffCanvas', { static: false }) diffCanvas!: ElementRef<HTMLCanvasElement>;
    private diffScene: THREE.Scene | null = null;
    private diffCamera: THREE.PerspectiveCamera | null = null;
    private diffRenderer: THREE.WebGLRenderer | null = null;
    private diffControls: OrbitControls | null = null;
    private diffAnimationFrameId: number | null = null;
    private diffModelGroup: THREE.Group | null = null;

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
        private authService: AuthService,
        private sanitizer: DomSanitizer
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
        this.cleanupDiffScene();
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
        //
        // Toggle: clicking the same version again closes the diff
        //
        if (this.selectedVersion?.versionNumber === version.versionNumber) {
            this.closeDiff();
            return;
        }

        this.selectedVersion = version;
        this.diffData = null;
        this.diff3dActive = false;
        this.cleanupDiffScene();

        //
        // Don't load diff for first version (nothing to compare with)
        //
        if (version.versionNumber <= 1) {
            this.diffData = { addedCount: 0, removedCount: 0, entries: [], isFirstVersion: true };
            return;
        }

        this.diffLoading = true;

        this.http.get<any>(`/api/mochub/moc/${this.mocId}/versions/${version.versionNumber}/diff`).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (diff) => {
                this.diffData = diff;
                this.diffLoading = false;
            },
            error: () => {
                this.diffData = { addedCount: 0, removedCount: 0, entries: [], error: true };
                this.diffLoading = false;
            }
        });
    }


    closeDiff(): void {
        this.selectedVersion = null;
        this.diffData = null;
        this.diff3dActive = false;
        this.cleanupDiffScene();
    }


    toggle3dDiff(): void {
        if (this.diff3dActive) {
            this.diff3dActive = false;
            this.cleanupDiffScene();
            return;
        }

        this.diff3dActive = true;
        this.diff3dLoading = true;

        //
        // Wait for the canvas to render in the DOM, then load the 3D diff
        //
        setTimeout(() => this.loadDiffMpd(), 0);
    }


    goToSettings(): void {
        this.router.navigate(['/mochub/moc', this.mocId, 'settings']);
    }


    //
    // Three.js diff viewer
    //

    private async loadDiffMpd(): Promise<void> {
        if (!this.diffCanvas || !this.selectedVersion) {
            this.diff3dLoading = false;
            return;
        }

        this.initDiffScene();

        //
        // Fetch the compiled GLB from the server
        //
        const versionNum = this.selectedVersion.versionNumber;
        const url = `/api/mochub/moc/${this.mocId}/versions/${versionNum}/viewer-glb`;

        try {
            const glbBuffer = await this.http.get(url, { responseType: 'arraybuffer' }).toPromise();

            if (!glbBuffer || !this.diffScene) {
                this.diff3dLoading = false;
                return;
            }

            //
            // Load the GLB using GLTFLoader.parse()
            //
            const loader = new GLTFLoader();

            const gltf = await new Promise<any>((resolve, reject) => {
                loader.parse(glbBuffer, '', (result: any) => resolve(result), reject);
            });

            const group = gltf.scene;

            this.diffModelGroup = group;
            this.diffScene!.add(group);
            this.centreAndFrameDiffModel(group);

            this.diff3dLoading = false;

        } catch (err) {
            console.error('[DiffViewer] Failed to load version GLB:', err);
            this.diff3dLoading = false;
        }
    }


    private initDiffScene(): void {
        const canvas = this.diffCanvas.nativeElement;
        const container = canvas.parentElement!;
        const width = container.clientWidth;
        const height = container.clientHeight || 400;

        //
        // Scene
        //
        this.diffScene = new THREE.Scene();

        //
        // Camera
        //
        this.diffCamera = new THREE.PerspectiveCamera(45, width / height, 0.1, 10000);
        this.diffCamera.position.set(300, 200, 400);
        this.diffCamera.lookAt(0, 0, 0);

        //
        // Renderer
        //
        this.diffRenderer = new THREE.WebGLRenderer({ canvas, antialias: true, alpha: true });
        this.diffRenderer.setClearColor(0x000000, 0);
        this.diffRenderer.setSize(width, height);
        this.diffRenderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.diffRenderer.toneMapping = THREE.ACESFilmicToneMapping;
        this.diffRenderer.toneMappingExposure = 1.0;

        //
        // Controls
        //
        this.diffControls = new OrbitControls(this.diffCamera, this.diffRenderer.domElement);
        this.diffControls.enableDamping = true;
        this.diffControls.dampingFactor = 0.08;

        //
        // Lighting — same as moc-viewer
        //
        this.diffScene.add(new THREE.AmbientLight(0xffffff, 0.4));

        const keyLight = new THREE.DirectionalLight(0xfff5e6, 1.0);
        keyLight.position.set(300, 400, 300);
        this.diffScene.add(keyLight);

        const fillLight = new THREE.DirectionalLight(0xb0d4f1, 0.4);
        fillLight.position.set(-200, 150, -150);
        this.diffScene.add(fillLight);

        this.diffScene.add(new THREE.HemisphereLight(0x87ceeb, 0x362f2d, 0.3));

        //
        // Grid
        //
        const computedBorder = getComputedStyle(document.documentElement)
            .getPropertyValue('--bmc-border').trim() || '#333355';
        const gridColour = new THREE.Color(computedBorder);
        const grid = new THREE.GridHelper(1000, 50, gridColour, gridColour);
        (grid.material as THREE.Material).opacity = 0.3;
        (grid.material as THREE.Material).transparent = true;
        this.diffScene.add(grid);

        //
        // Render loop
        //
        const animate = () => {
            this.diffAnimationFrameId = requestAnimationFrame(animate);
            this.diffControls?.update();
            if (this.diffRenderer && this.diffScene && this.diffCamera) {
                this.diffRenderer.render(this.diffScene, this.diffCamera);
            }
        };

        animate();
    }


    private centreAndFrameDiffModel(group: THREE.Group): void {
        const box = new THREE.Box3().setFromObject(group);
        const centre = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());

        //
        // Centre the model at the world origin
        //
        group.position.sub(centre);

        if (!this.diffCamera || !this.diffControls) {
            return;
        }

        //
        // Calculate the ideal camera distance so the model fills the viewport.
        // Use the tighter of horizontal/vertical FOV so it doesn't clip on
        // narrow or short viewports.
        //
        const fovV = this.diffCamera.fov * (Math.PI / 180);
        const fovH = 2 * Math.atan(Math.tan(fovV / 2) * this.diffCamera.aspect);

        const distanceV = (size.y / 2) / Math.tan(fovV / 2);
        const distanceH = (Math.max(size.x, size.z) / 2) / Math.tan(fovH / 2);
        const fitDistance = Math.max(distanceV, distanceH);

        //
        // 1.2× padding factor — enough margin without wasting space
        //
        const PADDING = 1.2;
        const distance = fitDistance * PADDING;

        this.diffCamera.position.set(distance * 0.7, distance * 0.5, distance * 0.7);
        this.diffCamera.lookAt(0, 0, 0);
        this.diffCamera.near = Math.max(0.1, distance * 0.01);
        this.diffCamera.far = distance * 10;
        this.diffCamera.updateProjectionMatrix();

        this.diffControls.target.set(0, 0, 0);
        this.diffControls.minDistance = distance * 0.1;
        this.diffControls.update();

        //
        // Position the grid at the bottom of the model so it acts as a floor
        //
        const floorY = -size.y / 2;
        const grid = this.diffScene?.children.find(c => c instanceof THREE.GridHelper);
        if (grid) {
            grid.position.y = floorY;
        }
    }


    private cleanupDiffScene(): void {
        if (this.diffAnimationFrameId != null) {
            cancelAnimationFrame(this.diffAnimationFrameId);
            this.diffAnimationFrameId = null;
        }

        if (this.diffModelGroup) {
            this.diffModelGroup.traverse(child => {
                if ((child as any).geometry) (child as any).geometry.dispose();
                if ((child as any).material) {
                    const mat = (child as any).material;
                    if (Array.isArray(mat)) mat.forEach((m: any) => m.dispose());
                    else mat.dispose();
                }
            });
            this.diffModelGroup = null;
        }

        this.diffControls?.dispose();
        this.diffRenderer?.dispose();
        this.diffScene = null;
        this.diffCamera = null;
        this.diffRenderer = null;
        this.diffControls = null;
    }


    navigateToFork(fork: any): void {
        if (fork.forkedMocId) {
            this.router.navigate(['/mochub/moc', fork.forkedMocId]);
        }
    }


    //
    // Helpers
    //

    private _renderedReadme: SafeHtml | null = null;
    private _readmeSource: string | null = null;

    getRenderedReadme(): SafeHtml {
        const source = this.moc?.readmeMarkdown || '';
        if (source !== this._readmeSource) {
            this._readmeSource = source;
            if (source) {
                const html = marked.parse(source, { async: false }) as string;
                this._renderedReadme = this.sanitizer.bypassSecurityTrustHtml(html);
            } else {
                this._renderedReadme = '';
            }
        }
        return this._renderedReadme || '';
    }

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

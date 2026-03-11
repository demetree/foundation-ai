/*
    AI-Developed — MOC 3D Viewer Component

    Renders a complete assembled MOC project using Three.js + LDrawLoader.
    Fetches a self-contained MPD from the server via /api/moc/project/{id}/viewer-mpd,
    which includes all PlacedBrick positions, submodels, and custom part geometry.

    Reuses camera, lighting, and LDraw loading patterns from catalog-part-detail.
*/

import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';

import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { LDrawLoader } from 'three/examples/jsm/loaders/LDrawLoader.js';
import { LDrawConditionalLineMaterial } from 'three/examples/jsm/materials/LDrawConditionalLineMaterial.js';

import { ProjectService, ProjectViewerSummary } from '../../services/project.service';
import { AuthService } from '../../services/auth.service';
import { LDrawFileCacheService } from '../../services/ldraw-file-cache.service';
import { HttpClient } from '@angular/common/http';


@Component({
    selector: 'app-moc-viewer',
    templateUrl: './moc-viewer.component.html',
    styleUrls: ['./moc-viewer.component.scss']
})
export class MocViewerComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild('viewerCanvas', { static: false }) viewerCanvas!: ElementRef<HTMLCanvasElement>;

    // Project state
    projectId: number | null = null;
    summary: ProjectViewerSummary | null = null;
    isLoadingSummary = true;
    isLoadingModel = false;
    modelLoadError: string | null = null;
    modelLoadProgress = 0;

    // Build step state
    totalSteps = 0;
    currentStep = 0; // 0 = show all
    private stepGroups: THREE.Group[] = [];

    // Three.js objects
    private scene!: THREE.Scene;
    private camera!: THREE.PerspectiveCamera;
    private renderer!: THREE.WebGLRenderer;
    private controls!: OrbitControls;
    private animationFrameId: number | null = null;
    private resizeObserver: ResizeObserver | null = null;
    private sceneReady = false;
    private modelGroup: THREE.Group | null = null;

    // Sidebar state
    sidebarOpen = true;
    autoRotate = true;

    private baseUrl: string;
    private destroy$ = new Subject<void>();


    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private http: HttpClient,
        private projectService: ProjectService,
        public authService: AuthService,
        private fileCacheService: LDrawFileCacheService,
        @Inject('BASE_URL') baseUrl: string
    ) {
        this.baseUrl = baseUrl;
    }


    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('projectId');
        if (id) {
            this.projectId = parseInt(id, 10);
            this.loadProjectSummary();
        }
    }


    ngAfterViewInit(): void {
        // Scene init happens after summary loads
    }


    ngOnDestroy(): void {
        this.stopAnimation();
        this.cleanupThreeJs();
        this.destroy$.next();
        this.destroy$.complete();
    }


    goBack(): void {
        this.location.back();
    }


    // ────────────────────────────────────────────────────────────────
    //  Data Loading
    // ────────────────────────────────────────────────────────────────

    private loadProjectSummary(): void {
        if (this.projectId == null) return;

        this.isLoadingSummary = true;
        this.projectService.getProjectSummary(this.projectId).pipe(
            takeUntil(this.destroy$),
            finalize(() => this.isLoadingSummary = false)
        ).subscribe({
            next: (summary) => {
                this.summary = summary;
                document.title = `${summary.name} — MOC Viewer`;
                // Start 3D viewer after summary loads
                setTimeout(() => this.initViewerAndLoadModel(), 0);
            },
            error: (err) => {
                console.error('[MocViewer] Failed to load project summary:', err);
                this.modelLoadError = 'Failed to load project details.';
            }
        });
    }


    // ────────────────────────────────────────────────────────────────
    //  Three.js Scene Setup
    // ────────────────────────────────────────────────────────────────

    private async initViewerAndLoadModel(): Promise<void> {
        if (this.viewerCanvas == null || this.projectId == null) {
            return;
        }

        //
        // Initialise the LDraw file cache — hydrates THREE.Cache from IndexedDB
        //
        await this.fileCacheService.initialise();

        this.initScene();
        this.startAnimation();
        await this.loadMpdModel();
    }


    private initScene(): void {
        const canvas = this.viewerCanvas.nativeElement;
        const container = canvas.parentElement!;
        const width = container.clientWidth;
        const height = container.clientHeight;

        // Scene
        this.scene = new THREE.Scene();

        // Camera — wider FOV for large models
        this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 10000);
        this.camera.position.set(300, 200, 400);
        this.camera.lookAt(0, 0, 0);

        // Renderer
        this.renderer = new THREE.WebGLRenderer({
            canvas: canvas,
            antialias: true,
            alpha: true
        });

        this.renderer.setClearColor(0x000000, 0);
        this.renderer.setSize(width, height);
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
        this.renderer.toneMappingExposure = 1.0;

        // Orbit controls
        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.08;
        this.controls.enablePan = true;
        this.controls.minDistance = 10;
        this.controls.maxDistance = 5000;
        this.controls.autoRotate = this.autoRotate;
        this.controls.autoRotateSpeed = 0.8;

        // Lighting
        this.setupLighting();

        // Ground plane
        this.addGroundPlane();

        // Responsive resize
        this.resizeObserver = new ResizeObserver(() => this.onResize());
        this.resizeObserver.observe(canvas.parentElement!);

        this.sceneReady = true;
    }


    private setupLighting(): void {
        // Ambient light
        const ambient = new THREE.AmbientLight(0xffffff, 0.4);
        this.scene.add(ambient);

        // Key light (warm)
        const keyLight = new THREE.DirectionalLight(0xfff5e6, 1.0);
        keyLight.position.set(300, 400, 300);
        keyLight.castShadow = true;
        keyLight.shadow.mapSize.width = 2048;
        keyLight.shadow.mapSize.height = 2048;
        keyLight.shadow.camera.near = 0.5;
        keyLight.shadow.camera.far = 2000;
        this.scene.add(keyLight);

        // Fill light (cool)
        const fillLight = new THREE.DirectionalLight(0xb0d4f1, 0.4);
        fillLight.position.set(-200, 150, -150);
        this.scene.add(fillLight);

        // Rim light
        const rimLight = new THREE.DirectionalLight(0xffa726, 0.3);
        rimLight.position.set(0, -50, -300);
        this.scene.add(rimLight);

        // Hemisphere
        const hemisphere = new THREE.HemisphereLight(0x87ceeb, 0x362f2d, 0.3);
        this.scene.add(hemisphere);
    }


    private addGroundPlane(): void {
        //
        // Read the theme border colour from CSS custom properties so the grid
        // adapts automatically when the user switches themes.
        //
        const computedBorder = getComputedStyle(document.documentElement)
            .getPropertyValue('--bmc-border').trim() || '#333355';
        const gridColour = new THREE.Color(computedBorder);

        const gridHelper = new THREE.GridHelper(1000, 50, gridColour, gridColour);
        gridHelper.position.y = -0.5;
        (gridHelper.material as THREE.Material).opacity = 0.3;
        (gridHelper.material as THREE.Material).transparent = true;
        this.scene.add(gridHelper);
    }


    // ────────────────────────────────────────────────────────────────
    //  LDraw MPD Loading
    // ────────────────────────────────────────────────────────────────

    private async loadMpdModel(): Promise<void> {
        if (this.projectId == null) return;

        this.isLoadingModel = true;
        this.modelLoadError = null;

        //
        // Step 1: Fetch the self-contained MPD from the server
        //
        let mpdText: string;

        try {
            mpdText = await this.projectService.getViewerMpd(this.projectId).toPromise() ?? '';
        } catch (err) {
            console.error('[MocViewer] Failed to fetch viewer MPD:', err);
            this.modelLoadError = 'Failed to load model data from the server.';
            this.isLoadingModel = false;
            return;
        }

        if (!mpdText || mpdText.trim().length === 0) {
            this.modelLoadError = 'Model data is empty — the project may not have any placed bricks.';
            this.isLoadingModel = false;
            return;
        }

        //
        // Step 2: Parse the MPD using LDrawLoader
        //
        const loader = new LDrawLoader();
        loader.setConditionalLineMaterial(LDrawConditionalLineMaterial);

        //
        // Set auth headers + parts library path for resolving standard parts
        //
        if (this.authService.isLoggedIn) {
            loader.setRequestHeader({
                'Authorization': `Bearer ${this.authService.accessToken}`
            });
        }

        const fileEndpoint = this.baseUrl + 'api/ldraw/file/';
        loader.setPartsLibraryPath(fileEndpoint);

        //
        // Preload LDraw colour configuration
        //
        const preloadUrl = fileEndpoint + 'LDConfig.ldr';

        try {
            await (loader as any).preloadMaterials(preloadUrl);
        } catch (err) {
            console.warn('[MocViewer] Material preload failed (using defaults):', err);
        }

        //
        // Parse the MPD text (not loading from URL — parsing in-memory text)
        //
        try {
            const group = await new Promise<THREE.Group>((resolve, reject) => {
                //
                // LDrawLoader.parse() takes text content and returns a Group
                //
                (loader as any).parse(mpdText, (group: THREE.Group) => {
                    resolve(group);
                }, undefined, (error: any) => {
                    reject(error);
                });
            });

            console.log('[MocViewer] Model parsed:', group.children.length, 'children');

            //
            // LDraw coordinate system uses Y-up with Y pointing downward
            //
            group.rotation.x = Math.PI;

            this.modelGroup = group;
            this.scene.add(group);
            this.centreAndFrameModel(group);

            //
            // Discover build steps from the parsed model
            //
            this.discoverBuildSteps(group);

            this.isLoadingModel = false;

        } catch (err) {
            console.error('[MocViewer] Model parse error:', err);
            this.modelLoadError = 'Failed to parse the 3D model.';
            this.isLoadingModel = false;
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Build Step Navigation
    // ────────────────────────────────────────────────────────────────

    private discoverBuildSteps(group: THREE.Group): void {
        //
        // LDrawLoader groups children by step separators.
        // Each direct child with userData.buildingStep is a step group.
        //
        this.stepGroups = [];

        group.traverse(child => {
            if (child.userData && child.userData['buildingStep'] !== undefined) {
                this.stepGroups.push(child as THREE.Group);
            }
        });

        this.totalSteps = this.stepGroups.length;
        this.currentStep = 0; // 0 = show all
    }


    setStep(step: number): void {
        this.currentStep = step;

        if (step === 0) {
            // Show all
            this.stepGroups.forEach(g => g.visible = true);
        } else {
            // Show steps 1..step, hide rest
            this.stepGroups.forEach((g, i) => {
                g.visible = (i + 1) <= step;
            });
        }
    }


    nextStep(): void {
        if (this.currentStep < this.totalSteps) {
            this.setStep(this.currentStep + 1);
        }
    }


    prevStep(): void {
        if (this.currentStep > 0) {
            this.setStep(this.currentStep - 1);
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Camera Framing
    // ────────────────────────────────────────────────────────────────

    private centreAndFrameModel(object: THREE.Object3D): void {
        const box = new THREE.Box3().setFromObject(object);
        const centre = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());

        object.position.sub(centre);

        // Position camera based on model size — further out for larger assemblies
        const maxDim = Math.max(size.x, size.y, size.z);
        const fov = this.camera.fov * (Math.PI / 180);
        const distance = maxDim / (2 * Math.tan(fov / 2)) * 2.5;

        this.camera.position.set(distance * 0.7, distance * 0.5, distance * 0.7);
        this.camera.lookAt(0, 0, 0);
        this.camera.far = distance * 10;
        this.camera.updateProjectionMatrix();

        this.controls.target.set(0, 0, 0);
        this.controls.update();
    }


    resetCamera(): void {
        if (this.modelGroup) {
            this.centreAndFrameModel(this.modelGroup);
        }
    }


    toggleAutoRotate(): void {
        this.autoRotate = !this.autoRotate;
        if (this.controls) {
            this.controls.autoRotate = this.autoRotate;
        }
    }


    toggleSidebar(): void {
        this.sidebarOpen = !this.sidebarOpen;
        // Trigger resize after animation completes
        setTimeout(() => this.onResize(), 350);
    }


    // ────────────────────────────────────────────────────────────────
    //  Export
    // ────────────────────────────────────────────────────────────────

    exportAs(format: 'ldr' | 'mpd' | 'io'): void {
        if (this.projectId == null) return;

        //
        // Trigger download via a hidden link
        //
        const url = this.projectService.getExportUrl(this.projectId, format);
        const link = document.createElement('a');
        link.href = url;
        link.download = '';

        //
        // Inject auth token as query parameter for download
        // (browser can't send Authorization header on a plain <a> click)
        //
        if (this.authService.isLoggedIn) {
            // Use the fetch + blob approach for authenticated downloads
            this.http.get(url, {
                headers: this.authService.GetAuthenticationHeaders(),
                responseType: 'blob'
            }).subscribe({
                next: (blob) => {
                    const blobUrl = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = blobUrl;
                    a.download = `${this.summary?.name ?? 'export'}.${format}`;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(blobUrl);
                },
                error: (err) => {
                    console.error('[MocViewer] Export failed:', err);
                }
            });
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Animation Loop
    // ────────────────────────────────────────────────────────────────

    private startAnimation(): void {
        const animate = (): void => {
            this.animationFrameId = requestAnimationFrame(animate);

            if (this.controls) {
                this.controls.update();
            }

            if (this.renderer && this.scene && this.camera) {
                this.renderer.render(this.scene, this.camera);
            }
        };

        animate();
    }


    private stopAnimation(): void {
        if (this.animationFrameId !== null) {
            cancelAnimationFrame(this.animationFrameId);
            this.animationFrameId = null;
        }
    }


    private onResize(): void {
        if (this.viewerCanvas == null || this.sceneReady === false) {
            return;
        }

        const container = this.viewerCanvas.nativeElement.parentElement!;
        const width = container.clientWidth;
        const height = container.clientHeight;

        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(width, height);
    }


    private cleanupThreeJs(): void {
        if (this.resizeObserver) {
            this.resizeObserver.disconnect();
            this.resizeObserver = null;
        }

        if (this.renderer) {
            this.renderer.dispose();
        }

        if (this.controls) {
            this.controls.dispose();
        }
    }
}

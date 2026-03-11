/*
    AI-Developed — MOC 3D Viewer Component

    Renders a complete assembled MOC project using Three.js + LDrawLoader.
    Fetches a self-contained MPD from the server via /api/moc/project/{id}/viewer-mpd,
    which includes all PlacedBrick positions, submodels, and custom part geometry.

    Reuses camera, lighting, and LDraw loading patterns from catalog-part-detail.
*/

import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit, Inject, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { Subject, Subscription, firstValueFrom } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';

import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { LDrawLoader } from 'three/examples/jsm/loaders/LDrawLoader.js';
import { LDrawConditionalLineMaterial } from 'three/examples/jsm/materials/LDrawConditionalLineMaterial.js';

import { ProjectService, ProjectViewerSummary } from '../../services/project.service';
import { AuthService } from '../../services/auth.service';
import { LDrawFileCacheService } from '../../services/ldraw-file-cache.service';
import { ManualGeneratorSignalrService, StepProgressEvent, GenerationCompleteEvent, ManualOptionsDto } from '../../services/manual-generator-signalr.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DomSanitizer, SafeUrl, SafeHtml } from '@angular/platform-browser';


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

    // STL export state
    exportingStl = false;
    stlFormat: 'binary' | 'ascii' = 'binary';

    // ────────────────────────────────────────────────────────────────
    //  Tab System
    // ────────────────────────────────────────────────────────────────
    activeViewerTab: '3d' | 'render' | 'manual' = '3d';
    poseMode = false;

    // ────────────────────────────────────────────────────────────────
    //  Server Render Tab — State
    // ────────────────────────────────────────────────────────────────
    renderWidth = 512;
    renderHeight = 512;
    renderElevation = 30;
    renderAzimuth = -45;
    flipView = false;
    renderEdges = true;
    smoothShading = true;
    antiAliasMode: 'none' | '2x' | '4x' = 'none';
    rendererType: 'rasterizer' | 'raytrace' = 'rasterizer';
    outputFormat: 'png' | 'webp' | 'svg' | 'gif' = 'png';
    webpQuality = 90;
    backgroundHex = '';
    gradientTopHex = '';
    gradientBottomHex = '';
    enablePbr = true;
    exposure = 1.0;
    aperture = 0;

    // Render output state
    rendering = false;
    renderError = '';
    renderTimeMs = 0;
    renderedImageUrl: SafeUrl | null = null;
    private renderedBlobUrl: string | null = null;
    renderedFormat = 'png';

    // Size presets
    sizeCategory: 'standard' | 'desktop' | 'mobile' = 'standard';
    sizeCategories = [
        { key: 'standard' as const, label: 'Standard', icon: 'bi-grid-3x3' },
        { key: 'desktop' as const, label: 'Desktop', icon: 'bi-display' },
        { key: 'mobile' as const, label: 'Mobile', icon: 'bi-phone' },
    ];
    sizePresets: { label: string; w: number; h: number; category: string }[] = [
        { label: '256²', w: 256, h: 256, category: 'standard' },
        { label: '512²', w: 512, h: 512, category: 'standard' },
        { label: '768²', w: 768, h: 768, category: 'standard' },
        { label: '1024²', w: 1024, h: 1024, category: 'standard' },
        { label: 'HD', w: 1920, h: 1080, category: 'desktop' },
        { label: '2K', w: 2560, h: 1440, category: 'desktop' },
        { label: '4K', w: 3840, h: 2160, category: 'desktop' },
        { label: 'Ultrawide', w: 3440, h: 1440, category: 'desktop' },
        { label: 'Phone', w: 1080, h: 1920, category: 'mobile' },
        { label: 'Phone+', w: 1284, h: 2778, category: 'mobile' },
        { label: 'Tablet', w: 2048, h: 2732, category: 'mobile' },
        { label: 'Square', w: 1080, h: 1080, category: 'mobile' },
    ];
    anglePresets = [
        { label: 'Standard', icon: 'bi-box', elevation: 30, azimuth: -45 },
        { label: 'Front', icon: 'bi-square', elevation: 0, azimuth: 0 },
        { label: 'Top', icon: 'bi-arrow-down', elevation: 90, azimuth: 0 },
        { label: 'Side', icon: 'bi-arrow-right', elevation: 0, azimuth: -90 },
        { label: '3/4 High', icon: 'bi-triangle', elevation: 45, azimuth: -45 },
        { label: '3/4 Low', icon: 'bi-dash-lg', elevation: 15, azimuth: -45 },
    ];
    bgPresets = [
        { label: 'None', icon: 'bi-x-circle', top: '', bottom: '', bg: '' },
        { label: 'Dark', icon: 'bi-moon', top: '#1a1a2e', bottom: '#16213e', bg: '' },
        { label: 'Sunset', icon: 'bi-brightness-high', top: '#ff6b6b', bottom: '#ffd93d', bg: '' },
        { label: 'Ocean', icon: 'bi-water', top: '#0f3460', bottom: '#1a508b', bg: '' },
        { label: 'Forest', icon: 'bi-tree', top: '#1b4332', bottom: '#2d6a4f', bg: '' },
        { label: 'Midnight', icon: 'bi-stars', top: '#0d0d2b', bottom: '#1a1a40', bg: '' },
        { label: 'Blush', icon: 'bi-heart', top: '#ee9ca7', bottom: '#ffdde1', bg: '' },
        { label: 'Slate', icon: 'bi-cloud', top: '#2c3e50', bottom: '#4ca1af', bg: '' },
        { label: 'Studio', icon: 'bi-lightbulb', top: '#e8e8e8', bottom: '#f5f5f5', bg: '' },
    ];

    // ────────────────────────────────────────────────────────────────
    //  Build Manual Tab — State
    // ────────────────────────────────────────────────────────────────
    isAnalysing = false;
    analysis: any = null;
    manualOptions: ManualOptionsDto = {
        pageSize: 'a4',
        imageSize: 800,
        elevation: 30,
        azimuth: -45,
        renderEdges: true,
        smoothShading: true,
        outputFormat: 'html',
        renderer: 'rasterizer',
        enablePbr: true,
        exposure: 1.0,
        aperture: 0
    };
    isGenerating = false;
    generationProgress = 0;
    generationTotal = 0;
    currentPreview: string | null = null;
    generationError: string | null = null;
    isReconnecting = false;
    generatedHtml: string | null = null;
    pdfDownloadUrl: string | null = null;
    htmlDownloadUrl: string | null = null;
    resultStats: { totalSteps: number; totalParts: number; renderTimeMs: number } | null = null;
    manualCurrentPage = 0;
    manualPages: SafeHtml[] = [];
    private manualSubs: Subscription[] = [];

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
        private signalr: ManualGeneratorSignalrService,
        private sanitizer: DomSanitizer,
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
        this.revokeRenderBlob();
        this.manualSubs.forEach(s => s.unsubscribe());
        this.signalr.disconnect();
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
        // Step 2a: Extract bundled LDraw library files from the MPD
        //
        // The server embeds all LDraw dependency files (parts, primitives,
        // LDConfig.ldr) as inline "0 FILE" blocks. We extract them into a
        // map and intercept the LDrawLoader's fetchData method so it resolves
        // files from memory rather than issuing individual HTTP requests.
        //
        const { strippedMpd, fileMap } = this.extractBundledFiles(mpdText);

        if (Object.keys(fileMap).length > 0) {
            console.log(`[MocViewer] Pre-seeded ${Object.keys(fileMap).length} bundled LDraw files`);

            //
            // Access the internal LDrawParsedCache and monkey-patch fetchData
            // to serve from the local map before hitting the network.
            //
            const partsGeometryCache = (loader as any).partsCache;
            if (partsGeometryCache && partsGeometryCache.parseCache) {
                const parseCache = partsGeometryCache.parseCache;
                const originalFetchData = parseCache.fetchData.bind(parseCache);

                parseCache.fetchData = async (fileName: string) => {
                    //
                    // Try the local file map first — check bare name, then
                    // with standard LDraw directory prefixes stripped.
                    //
                    // The server already bundled ALL files it could find in
                    // the LDraw parts library.  If a file isn't here, it
                    // does not exist.  Throwing immediately prevents the
                    // LDrawLoader's 12-attempt trial-and-error HTTP flood.
                    //
                    const lowerName = fileName.toLowerCase();
                    if (fileMap[lowerName]) {
                        return fileMap[lowerName];
                    }

                    // Strip directory prefix and try bare filename
                    const bareName = lowerName.includes('/') ? lowerName.substring(lowerName.lastIndexOf('/') + 1) : lowerName;
                    if (fileMap[bareName]) {
                        return fileMap[bareName];
                    }

                    // Try with standard LDraw directory prefixes
                    const prefixes = ['parts/', 'p/', 'parts/s/', 'p/48/', 'p/8/', 'models/'];
                    for (const prefix of prefixes) {
                        if (fileMap[prefix + bareName]) {
                            return fileMap[prefix + bareName];
                        }
                    }

                    // File is not in the LDraw library — throw immediately
                    // instead of making 12 sequential HTTP requests
                    console.debug(`[MocViewer] Skipping unbundled file: ${fileName}`);
                    throw new Error(`LDrawLoader: File "${fileName}" not found in bundled library.`);
                };
            }
        }

        //
        // Preload LDraw colour configuration
        //
        // preloadMaterials() makes a single HTTP request for LDConfig.ldr.
        // With the in-memory LDrawFileService, this completes instantly.
        // Also seed the parse cache so the file isn't fetched again during parsing.
        //
        const ldConfigKey = Object.keys(fileMap).find(k => k.toLowerCase() === 'ldconfig.ldr');
        if (ldConfigKey && fileMap[ldConfigKey]) {
            // Seed the parse cache so LDConfig.ldr isn't fetched during model parsing
            const partsGeometryCache = (loader as any).partsCache;
            if (partsGeometryCache && partsGeometryCache.parseCache) {
                partsGeometryCache.parseCache.setData('LDConfig.ldr', fileMap[ldConfigKey]);
            }
        }

        try {
            await (loader as any).preloadMaterials(fileEndpoint + 'LDConfig.ldr');
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


    /**
     * Extracts all "0 FILE" / "0 NOFILE" blocks from an MPD text string
     * into a map keyed by lowercase filename.
     *
     * The server bundles all LDraw library dependencies inline so the
     * LDrawLoader can resolve them from memory instead of fetching
     * each one individually via HTTP.
     */
    private extractBundledFiles(mpdText: string): { strippedMpd: string; fileMap: Record<string, string> } {
        const fileMap: Record<string, string> = {};
        const lines = mpdText.split('\n');
        let currentFileName: string | null = null;
        let currentContent: string[] = [];

        for (const line of lines) {
            const trimmed = line.trim();

            if (trimmed.startsWith('0 FILE ')) {
                //
                // Save the previous file if we were accumulating one
                //
                if (currentFileName !== null) {
                    fileMap[currentFileName.toLowerCase()] = currentContent.join('\n');
                }

                currentFileName = trimmed.substring(7).trim();
                currentContent = [];
                continue;
            }

            if (trimmed === '0 NOFILE') {
                if (currentFileName !== null) {
                    fileMap[currentFileName.toLowerCase()] = currentContent.join('\n');
                    currentFileName = null;
                    currentContent = [];
                }
                continue;
            }

            if (currentFileName !== null) {
                currentContent.push(line);
            }
        }

        //
        // Save the last file if we were still accumulating
        //
        if (currentFileName !== null) {
            fileMap[currentFileName.toLowerCase()] = currentContent.join('\n');
        }

        return { strippedMpd: mpdText, fileMap };
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


    /**
     * Export the assembled MOC as an STL file via the server-side geometry resolver.
     * Supports both binary (compact) and ASCII (human-readable) formats.
     *
     * AI-generated — Mar 2026.
     */
    exportStl(): void {
        if (this.projectId == null || this.exportingStl) {
            return;
        }

        this.exportingStl = true;
        const url = this.projectService.getStlExportUrl(this.projectId, this.stlFormat);

        this.http.get(url, {
            headers: this.authService.GetAuthenticationHeaders(),
            responseType: 'blob'
        }).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (blob) => {
                const blobUrl = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = blobUrl;
                a.download = `${this.summary?.name ?? 'export'}.stl`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                window.URL.revokeObjectURL(blobUrl);
                this.exportingStl = false;
            },
            error: (err) => {
                console.error('[MocViewer] STL export failed:', err);
                this.exportingStl = false;
            }
        });
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


    // ────────────────────────────────────────────────────────────────
    //  Server Render Tab — Methods
    //
    //  AI-developed — March 2026
    // ────────────────────────────────────────────────────────────────

    get activeSizePresets() {
        return this.sizePresets.filter(p => p.category === this.sizeCategory);
    }

    get effectiveAntiAlias(): string {
        if (this.antiAliasMode === '4x' && (this.renderWidth > 2560 || this.renderHeight > 2560)) {
            return '2x';
        }
        return this.antiAliasMode;
    }

    get sizeDisplayLabel(): string {
        if (this.outputFormat === 'svg') return 'Vector (scalable)';
        return `${this.renderWidth} × ${this.renderHeight} px`;
    }

    selectRenderSize(preset: { w: number; h: number }): void {
        this.renderWidth = preset.w;
        this.renderHeight = preset.h;
    }

    isSelectedSize(preset: { w: number; h: number }): boolean {
        return this.renderWidth === preset.w && this.renderHeight === preset.h;
    }

    selectRenderAngle(preset: { elevation: number; azimuth: number }): void {
        this.renderElevation = preset.elevation;
        this.renderAzimuth = preset.azimuth;
    }

    isSelectedAngle(preset: { elevation: number; azimuth: number }): boolean {
        return this.renderElevation === preset.elevation && this.renderAzimuth === preset.azimuth;
    }

    applyBgPreset(preset: { top: string; bottom: string; bg: string }): void {
        this.gradientTopHex = preset.top;
        this.gradientBottomHex = preset.bottom;
        this.backgroundHex = preset.bg;
    }

    isActiveBgPreset(preset: { top: string; bottom: string }): boolean {
        return this.gradientTopHex === preset.top && this.gradientBottomHex === preset.bottom;
    }


    // Pose Mode — extract camera angles from OrbitControls
    togglePoseMode(): void {
        this.poseMode = !this.poseMode;
        if (this.controls) {
            this.controls.autoRotate = !this.poseMode;
        }
    }

    getCameraAngles(): { elevation: number; azimuth: number } {
        if (this.camera == null || this.controls == null) {
            return { elevation: 30, azimuth: -45 };
        }

        const position = this.camera.position.clone().sub(this.controls.target);
        const distance = position.length();
        if (distance === 0) return { elevation: 0, azimuth: 0 };

        const elevation = Math.round(Math.asin(position.y / distance) * (180 / Math.PI));
        const azimuth = Math.round(Math.atan2(position.x, -position.z) * (180 / Math.PI));
        return { elevation, azimuth };
    }

    applyPoseToRender(): void {
        const angles = this.getCameraAngles();
        this.renderElevation = angles.elevation;
        this.renderAzimuth = angles.azimuth;
        this.activeViewerTab = 'render';
        setTimeout(() => this.renderModel(), 0);
    }


    /**
     * Server-side rendering of the assembled MOC model.
     * Sends render config to /api/moc/export/{id}/render and displays the result.
     */
    renderModel(): void {
        if (this.projectId == null || this.rendering) { return; }

        if (this.outputFormat === 'gif') {
            this.renderTurntable();
            return;
        }

        this.rendering = true;
        this.renderError = '';
        this.revokeRenderBlob();

        const headers = this.authService.GetAuthenticationHeaders();
        const effectiveAzimuth = this.flipView ? this.renderAzimuth + 180 : this.renderAzimuth;

        let url = `/api/moc/export/${this.projectId}/render?width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.renderElevation}&azimuth=${effectiveAzimuth}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}&antiAlias=${this.effectiveAntiAlias}&format=${this.outputFormat}&quality=${this.webpQuality}&renderer=${this.rendererType}`;

        // Pass the current build step so the render matches what the user sees
        if (this.currentStep > 0) {
            url += `&step=${this.currentStep}`;
        }

        if (this.rendererType === 'raytrace') {
            url += `&enablePbr=${this.enablePbr}&exposure=${this.exposure}&aperture=${this.aperture}`;
        }
        if (this.backgroundHex) {
            url += `&backgroundHex=${encodeURIComponent(this.backgroundHex)}`;
        }
        if (this.gradientTopHex && this.gradientBottomHex) {
            url += `&gradientTopHex=${encodeURIComponent(this.gradientTopHex)}&gradientBottomHex=${encodeURIComponent(this.gradientBottomHex)}`;
        }

        const startTime = performance.now();

        this.http.get(url, { headers, responseType: 'blob' })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (blob) => {
                    this.renderTimeMs = Math.round(performance.now() - startTime);
                    this.renderedBlobUrl = URL.createObjectURL(blob);
                    this.renderedImageUrl = this.sanitizer.bypassSecurityTrustUrl(this.renderedBlobUrl);
                    this.renderedFormat = this.outputFormat;
                    this.rendering = false;
                },
                error: (err) => {
                    this.renderTimeMs = 0;
                    this.renderError = err.status === 404 ? 'Model geometry not found.' : 'Render failed. Please try again.';
                    this.rendering = false;
                }
            });
    }


    private renderTurntable(): void {
        this.rendering = true;
        this.renderError = '';
        this.revokeRenderBlob();

        const headers = this.authService.GetAuthenticationHeaders();
        const url = `/api/moc/export/${this.projectId}/render?width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.renderElevation}&format=gif&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}`;

        const startTime = performance.now();

        this.http.get(url, { headers, responseType: 'blob' })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (blob) => {
                    this.renderTimeMs = Math.round(performance.now() - startTime);
                    this.renderedBlobUrl = URL.createObjectURL(blob);
                    this.renderedImageUrl = this.sanitizer.bypassSecurityTrustUrl(this.renderedBlobUrl);
                    this.renderedFormat = 'gif';
                    this.rendering = false;
                },
                error: () => {
                    this.renderTimeMs = 0;
                    this.renderError = 'Turntable render failed.';
                    this.rendering = false;
                }
            });
    }


    downloadRender(): void {
        if (this.renderedBlobUrl == null) return;
        const ext = this.renderedFormat === 'gif' ? 'gif' : this.renderedFormat === 'webp' ? 'webp' : this.renderedFormat === 'svg' ? 'svg' : 'png';
        const baseName = `${this.summary?.name ?? 'render'}_${this.renderWidth}x${this.renderHeight}`;
        const a = document.createElement('a');
        a.href = this.renderedBlobUrl;
        a.download = `${baseName}.${ext}`;
        a.click();
    }


    private revokeRenderBlob(): void {
        if (this.renderedBlobUrl) {
            URL.revokeObjectURL(this.renderedBlobUrl);
            this.renderedBlobUrl = null;
            this.renderedImageUrl = null;
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Build Manual Tab — Methods
    //
    //  AI-developed — March 2026
    // ────────────────────────────────────────────────────────────────

    initManualTab(): void {
        if (this.manualSubs.length > 0) return; // already initialised

        this.manualSubs.push(
            this.signalr.onStepProgress$.subscribe((e: StepProgressEvent) => {
                this.generationProgress = e.step;
                this.generationTotal = e.total;
                this.currentPreview = 'data:image/png;base64,' + e.previewBase64;
            }),
            this.signalr.onComplete$.subscribe((e: GenerationCompleteEvent) => {
                this.isGenerating = false;
                this.generationError = null;
                this.resultStats = {
                    totalSteps: e.totalSteps,
                    totalParts: e.totalParts,
                    renderTimeMs: e.renderTimeMs
                };
                if (e.format === 'pdf') {
                    this.pdfDownloadUrl = e.downloadUrl ?? null;
                } else if (e.downloadUrl) {
                    this.htmlDownloadUrl = e.downloadUrl;
                    fetch(e.downloadUrl, {
                        headers: { Authorization: 'Bearer ' + this.authService.accessToken }
                    })
                        .then(r => r.ok ? r.text() : Promise.reject(r.statusText))
                        .then(html => {
                            this.generatedHtml = html;
                            this.splitManualPages(html);
                        })
                        .catch(() => { this.generationError = 'Failed to download HTML manual.'; });
                } else {
                    this.generatedHtml = e.html;
                    this.splitManualPages(e.html!);
                }
                this.signalr.disconnect();
            }),
            this.signalr.onError$.subscribe((msg: string) => {
                if (this.resultStats != null) return;
                this.isGenerating = false;
                this.generationError = msg;
                this.signalr.disconnect();
            }),
            this.signalr.onConnectionChange$.subscribe((connected: boolean) => {
                if (this.isGenerating) {
                    this.isReconnecting = !connected;
                }
            })
        );
    }


    analyseProject(): void {
        if (this.projectId == null || this.isAnalysing) return;

        this.isAnalysing = true;
        this.analysis = null;

        const headers = this.authService.GetAuthenticationHeaders();
        this.http.post<any>(`/api/manual-generator/analyse-project/${this.projectId}`, null, { headers })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (res) => {
                    this.analysis = res;
                    this.isAnalysing = false;
                },
                error: (err) => {
                    this.generationError = err.error || 'Analysis failed.';
                    this.isAnalysing = false;
                }
            });
    }


    async generateManual(): Promise<void> {
        if (this.projectId == null) return;

        this.initManualTab();
        this.isGenerating = true;
        this.generationProgress = 0;
        this.generationTotal = 0;
        this.currentPreview = null;
        this.generationError = null;
        this.generatedHtml = null;
        this.pdfDownloadUrl = null;
        this.htmlDownloadUrl = null;
        this.resultStats = null;
        this.manualPages = [];

        const headers = new HttpHeaders({
            'Authorization': `Bearer ${this.authService.accessToken}`
        });

        try {
            const response = await firstValueFrom(
                this.http.post<{ generationId: string }>(
                    `/api/manual-generator/generate-project/${this.projectId}`, null, { headers }
                )
            );

            if (response?.generationId == null) {
                this.generationError = 'Server did not return a generation ID.';
                this.isGenerating = false;
                return;
            }

            await this.signalr.connect();
            await this.signalr.generateManual(response.generationId, this.manualOptions);

        } catch (err: any) {
            if (this.resultStats == null) {
                this.generationError = err?.error || 'Failed to start generation.';
                this.isGenerating = false;
            }
        }
    }


    cancelGeneration(): void {
        this.isGenerating = false;
        this.isReconnecting = false;
        this.generationError = 'Generation cancelled.';
        this.signalr.disconnect();
    }


    get progressPercent(): number {
        if (this.generationTotal === 0) return 0;
        return Math.round((this.generationProgress / this.generationTotal) * 100);
    }


    private splitManualPages(html: string): void {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const pageElements = doc.querySelectorAll('.page');
        this.manualPages = [];
        pageElements.forEach(page => {
            this.manualPages.push(this.sanitizer.bypassSecurityTrustHtml(page.outerHTML));
        });
        this.manualCurrentPage = 0;
    }

    manualPrevPage(): void {
        if (this.manualCurrentPage > 0) this.manualCurrentPage--;
    }

    manualNextPage(): void {
        if (this.manualCurrentPage < this.manualPages.length - 1) this.manualCurrentPage++;
    }

    @HostListener('document:keydown', ['$event'])
    onKeyDown(event: KeyboardEvent): void {
        if (this.activeViewerTab !== 'manual' || this.manualPages.length === 0) return;
        if (event.key === 'ArrowLeft') this.manualPrevPage();
        if (event.key === 'ArrowRight') this.manualNextPage();
    }


    downloadManualHtml(): void {
        const downloadName = (this.summary?.name ?? 'manual') + '_build-manual.html';
        if (this.generatedHtml) {
            const blob = new Blob([this.generatedHtml], { type: 'text/html' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = downloadName;
            a.click();
            URL.revokeObjectURL(url);
            return;
        }
        if (this.htmlDownloadUrl) {
            fetch(this.htmlDownloadUrl, {
                headers: { Authorization: 'Bearer ' + this.authService.accessToken }
            })
                .then(r => r.ok ? r.blob() : Promise.reject(r.statusText))
                .then(blob => {
                    const url = URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = downloadName;
                    a.click();
                    URL.revokeObjectURL(url);
                });
        }
    }


    downloadManualPdf(): void {
        if (!this.pdfDownloadUrl) return;
        const downloadName = (this.summary?.name ?? 'manual') + '_build-manual.pdf';
        fetch(this.pdfDownloadUrl, {
            headers: { Authorization: 'Bearer ' + this.authService.accessToken }
        })
            .then(r => r.ok ? r.blob() : Promise.reject(r.statusText))
            .then(blob => {
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = downloadName;
                a.click();
                URL.revokeObjectURL(url);
            });
    }


    printManual(): void {
        if (!this.generatedHtml) return;
        const printWindow = window.open('', '_blank');
        if (printWindow) {
            printWindow.document.write(this.generatedHtml);
            printWindow.document.close();
            printWindow.onload = () => { printWindow.print(); };
        }
    }


    resetManual(): void {
        this.analysis = null;
        this.generatedHtml = null;
        this.pdfDownloadUrl = null;
        this.htmlDownloadUrl = null;
        this.resultStats = null;
        this.manualPages = [];
        this.currentPreview = null;
        this.generationError = null;
    }
}

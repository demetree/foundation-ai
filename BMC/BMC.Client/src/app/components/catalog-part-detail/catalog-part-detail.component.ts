/*
    AI-Developed — This file was significantly developed with AI assistance.
    Original component authored by the project team.
    Colour swatch picker feature (selectColour, applyColourToScene, loadAllColours, colour mode toggle)
    was added with AI assistance — reviewed and adapted to project standards.
*/

import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';

import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { LDrawLoader } from 'three/examples/jsm/loaders/LDrawLoader.js';
import { LDrawConditionalLineMaterial } from 'three/examples/jsm/materials/LDrawConditionalLineMaterial.js';

import { BrickPartService, BrickPartData } from '../../bmc-data-services/brick-part.service';
import { BrickPartConnectorData } from '../../bmc-data-services/brick-part-connector.service';
import { LegoSetPartData } from '../../bmc-data-services/lego-set-part.service';
import { BrickPartColourData } from '../../bmc-data-services/brick-part-colour.service';
import { BrickColourService, BrickColourData } from '../../bmc-data-services/brick-colour.service';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { LDrawFileCacheService } from '../../services/ldraw-file-cache.service';
import { lastValueFrom } from 'rxjs';


@Component({
    selector: 'app-catalog-part-detail',
    templateUrl: './catalog-part-detail.component.html',
    styleUrls: ['./catalog-part-detail.component.scss']
})
export class CatalogPartDetailComponent implements OnInit, OnDestroy, AfterViewInit {

    @ViewChild('rendererCanvas', { static: false }) rendererCanvas!: ElementRef<HTMLCanvasElement>;

    part: BrickPartData | null = null;
    isLoading = true;
    isLoadingModel = false;
    hasGeometry = false;
    modelLoadError: string | null = null;

    connectors: BrickPartConnectorData[] = [];
    colours: BrickPartColourData[] = [];
    isLoadingConnectors = false;
    isLoadingColours = false;

    //
    // Colour picker state
    //
    selectedColour: BrickColourData | null = null;
    colourMode: 'part' | 'all' = 'part';

    //
    // Initial colour ID from query params (when navigating from set detail)
    //
    private initialColourId: bigint | number | null = null;

    //
    // partColours — the merged, deduplicated list of colours for this part.
    // Built from two sources: BrickPartColour records (direct mappings) and
    // LegoSetPart records (colours the part appears in across all sets).
    // This is the list shown in "Part Colours" mode.
    //
    partColours: BrickColourData[] = [];

    allColours: BrickColourData[] = [];
    isLoadingAllColours = false;
    allColoursSearch = '';

    //
    // Tracks whether the full colour list has been fetched at least once
    //
    private allColoursLoaded = false;

    //
    // Tracks whether each of the two colour sources has finished loading,
    // so we know when it is safe to build the merged partColours list.
    //
    private partColoursSourceLoaded = false;
    private setPartsSourceLoaded = false;

    // Set parts panel
    setParts: LegoSetPartData[] = [];
    isLoadingSetParts = false;
    setPartsSearch = '';
    setPartsSortField: 'set' | 'setNum' | 'colour' | 'qty' = 'qty';
    setPartsSortDir: 'asc' | 'desc' = 'desc';

    // Three.js objects
    private scene!: THREE.Scene;
    private camera!: THREE.PerspectiveCamera;
    private renderer!: THREE.WebGLRenderer;
    private controls!: OrbitControls;
    private animationFrameId: number | null = null;
    private resizeObserver: ResizeObserver | null = null;
    private sceneReady = false;

    //
    // When an initialColourId is provided, we defer showing the 3D model
    // until the colour data has loaded and the correct colour can be applied.
    // This prevents the visible flicker from default → correct colour.
    //
    pendingColourReady = false;

    private baseUrl: string;


    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private http: HttpClient,
        private authService: AuthService,
        private brickPartService: BrickPartService,
        private brickColourService: BrickColourService,
        private fileCacheService: LDrawFileCacheService,
        @Inject('BASE_URL') baseUrl: string
    ) {
        this.baseUrl = baseUrl;
    }


    ngOnInit(): void {
        const partId = this.route.snapshot.paramMap.get('partId');

        //
        // Read optional colourId query parameter for pre-selecting a colour
        // when navigating from set detail.
        //
        const colourId = this.route.snapshot.queryParamMap.get('colourId');
        if (colourId) {
            this.initialColourId = parseInt(colourId, 10);
        }

        if (partId) {
            this.loadPart(parseInt(partId, 10));
        }
    }


    ngAfterViewInit(): void {
        // Scene initialisation will happen after part data loads and canvas is available
    }


    ngOnDestroy(): void {
        this.stopAnimation();
        this.cleanupThreeJs();
    }


    goBack(): void {
        this.location.back();
    }


    // ────────────────────────────────────────────────────────────────
    //  Data Loading
    // ────────────────────────────────────────────────────────────────

    private async loadPart(id: number): Promise<void> {
        this.isLoading = true;

        try {
            const part = await lastValueFrom(this.brickPartService.GetBrickPart(id, true));

            this.part = part;
            document.title = `${part.name} — Part Detail`;

            // Load related data
            this.loadConnectors();
            this.loadColours();
            this.loadSetParts();

            // Check for geometry and initialise 3D viewer
            if (part.geometryFilePath) {
                this.hasGeometry = true;

                //
                // If we have an initial colour ID, defer showing the model
                // until colour data is ready (prevents colour flicker).
                //
                if (this.initialColourId != null) {
                    this.pendingColourReady = true;
                }

                // Allow template to render the canvas element first
                setTimeout(() => this.initThreeJsAndLoadModel(), 0);
            }
        }
        catch (error) {
            console.error('Error loading part:', error);
        }
        finally {
            this.isLoading = false;
        }
    }


    private async loadConnectors(): Promise<void> {
        if (this.part == null || typeof this.part.BrickPartConnectors === 'undefined') {
            return;
        }

        this.isLoadingConnectors = true;

        try {
            this.connectors = await this.part.BrickPartConnectors;
        }
        catch {
            this.connectors = [];
        }
        finally {
            this.isLoadingConnectors = false;
        }
    }


    private async loadColours(): Promise<void> {
        if (this.part == null || typeof this.part.BrickPartColours === 'undefined') {
            //
            // No BrickPartColour data available — mark this source as done so the
            // merge can still proceed once set parts finish loading.
            //
            this.partColoursSourceLoaded = true;
            this.buildPartColourList();
            return;
        }

        this.isLoadingColours = true;

        try {
            //
            // Load with includeRelations=true so that each BrickPartColourData has its
            // brickColour nav property populated (needed for hexRgb, name, isTransparent, etc.)
            //
            this.colours = await this.part.BrickPartColours;
        }
        catch {
            this.colours = [];
        }
        finally {
            this.isLoadingColours = false;

            //
            // Mark this source as loaded and attempt to build the merged colour list.
            // The merge will only run once both sources are ready.
            //
            this.partColoursSourceLoaded = true;
            this.buildPartColourList();
        }
    }


    private async loadSetParts(): Promise<void> {
        if (this.part == null || typeof this.part.LegoSetParts === 'undefined') {
            //
            // No set parts data available — mark this source as done.
            //
            this.setPartsSourceLoaded = true;
            this.buildPartColourList();
            return;
        }

        this.isLoadingSetParts = true;

        try {
            this.setParts = await this.part.LegoSetParts;
        }
        catch {
            this.setParts = [];
        }
        finally {
            this.isLoadingSetParts = false;

            //
            // Mark this source as loaded and attempt to build the merged colour list.
            //
            this.setPartsSourceLoaded = true;
            this.buildPartColourList();
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Colour Picker — Part Colour List (merged from two sources)
    // ────────────────────────────────────────────────────────────────

    private buildPartColourList(): void {
        //
        // Wait until both data sources have finished loading before building the list.
        // This method is called from the finally block of both loadColours() and loadSetParts(),
        // so it will be called twice — but only runs the merge on the second call.
        //
        if (this.partColoursSourceLoaded === false || this.setPartsSourceLoaded === false) {
            return;
        }

        //
        // Use a Map keyed by brickColourId to deduplicate colours from both sources.
        //
        const colourMap = new Map<string | number | bigint, BrickColourData>();

        //
        // Source 1: BrickPartColour records (direct part-colour mappings)
        //
        for (const partColour of this.colours) {
            if (partColour.brickColour != null) {
                colourMap.set(partColour.brickColour.id, partColour.brickColour);
            }
        }

        //
        // Source 2: LegoSetPart records — each set appearance carries the colour the part
        // was used in.  This is often a richer source than the direct BrickPartColour table.
        //
        for (const setPart of this.setParts) {
            if (setPart.brickColour != null) {
                colourMap.set(setPart.brickColour.id, setPart.brickColour);
            }
        }

        //
        // Sort the merged list alphabetically by name for a consistent display order.
        //
        this.partColours = Array.from(colourMap.values())
            .sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));

        //
        // If we have an initial colour ID from query params, try to find and select it
        //
        if (this.initialColourId != null) {
            const matchingColour = this.partColours.find(c => c.id === this.initialColourId);
            if (matchingColour != null) {
                this.selectColour(matchingColour);
            }

            //
            // Colour data is now resolved — clear the pending gate.
            // If the model already finished loading, this makes it visible.
            //
            this.pendingColourReady = false;
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Colour Picker — Mode Toggle and All Colours Loading
    // ────────────────────────────────────────────────────────────────

    setColourMode(mode: 'part' | 'all'): void {
        this.colourMode = mode;

        //
        // Fetch the full colour list the first time the user switches to 'all' mode.
        // Subsequent switches reuse the cached list.
        //
        if (mode === 'all' && this.allColoursLoaded === false) {
            this.loadAllColours();
        }
    }


    private async loadAllColours(): Promise<void> {
        this.isLoadingAllColours = true;

        try {
            //
            // Fetch all active colours from the BrickColour table.
            // A large page size is used because the colour table is small enough to load in one request.
            //
            this.allColours = await lastValueFrom(
                this.brickColourService.GetBrickColourList({
                    active: true,
                    deleted: false,
                    pageSize: 500,
                    pageNumber: 1
                })
            );

            this.allColoursLoaded = true;

            //
            // If we have an initial colour ID from query params and haven't selected a colour yet,
            // try to find and select it from the full colour list.
            //
            if (this.initialColourId != null && this.selectedColour == null) {
                const matchingColour = this.allColours.find(c => c.id === this.initialColourId);
                if (matchingColour != null) {
                    this.selectColour(matchingColour);
                }
            }
        }
        catch (error) {
            console.error('Error loading all colours:', error);
            this.allColours = [];
        }
        finally {
            this.isLoadingAllColours = false;
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Colour Picker — Selection and 3D Scene Application
    // ────────────────────────────────────────────────────────────────

    selectColour(colour: BrickColourData | null | undefined): void {
        //
        // Guard against null — can happen if called from a template binding before the nav property is loaded
        //
        if (colour == null) {
            return;
        }

        //
        // Toggle off if the same colour is clicked again
        //
        if (this.selectedColour != null && this.selectedColour.id === colour.id) {
            this.selectedColour = null;
            this.resetSceneColours();
            return;
        }

        this.selectedColour = colour;

        //
        // Apply the selected colour to the 3D scene if it is ready
        //
        if (this.sceneReady === true && colour.hexRgb != null) {
            this.applyColourToScene(colour.hexRgb);
        }
    }


    //
    // AI-Developed — Traverses the THREE.js scene and replaces the colour on all
    // MeshStandardMaterial instances.  Line/edge materials are intentionally left
    // unchanged so that part outlines remain visible.
    //
    private applyColourToScene(hexRgb: string): void {
        if (this.scene == null) {
            return;
        }

        //
        // Normalise the hex value — handle both '05131D' and '#05131D' formats
        //
        const normalisedHex = hexRgb.startsWith('#') ? hexRgb : '#' + hexRgb;
        const colour = new THREE.Color(normalisedHex);

        //
        // Walk every object in the scene and update solid mesh materials
        //
        this.scene.traverse(child => {
            if (child instanceof THREE.Mesh) {
                const material = child.material;

                if (material instanceof THREE.MeshStandardMaterial) {
                    material.color.set(colour);
                    material.needsUpdate = true;
                }
            }
        });
    }


    private resetSceneColours(): void {
        //
        // When the colour selection is cleared, reload the model to restore original colours.
        // This is the simplest approach — re-running the LDraw loader restores the file's
        // original colour definitions.
        //
        if (this.sceneReady === false || this.part == null) {
            return;
        }

        //
        // Remove all Mesh and Group children directly from the scene root.
        // We collect them first to avoid mutating the scene while traversing it.
        //
        const objectsToRemove: THREE.Object3D[] = [];

        this.scene.children.forEach(child => {
            if (child instanceof THREE.Mesh || child instanceof THREE.Group) {
                objectsToRemove.push(child);
            }
        });

        for (const obj of objectsToRemove) {
            if (obj.parent != null) {
                obj.parent.remove(obj);
            }
        }

        //
        // Re-add the ground grid
        //
        this.addGroundPlane();

        //
        // Reload the model with original colours
        //
        if (this.part.geometryFilePath != null) {
            this.loadLDrawModel();
        }
        else {
            this.buildFallbackGeometry();
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Colour Picker — Template Helpers
    // ────────────────────────────────────────────────────────────────

    getSwatchHex(colour: BrickColourData | null | undefined): string {
        if (colour == null) {
            return 'transparent';
        }

        const hex = colour.hexRgb;

        if (hex == null || hex === '') {
            return 'transparent';
        }

        return hex.startsWith('#') ? hex : '#' + hex;
    }


    get filteredAllColours(): BrickColourData[] {
        if (this.allColoursSearch.trim() === '') {
            return this.allColours;
        }

        const query = this.allColoursSearch.toLowerCase();

        return this.allColours.filter(c => {
            return (c.name ?? '').toLowerCase().includes(query);
        });
    }


    get visibleColourCount(): number {
        if (this.colourMode === 'part') {
            return this.partColours.length;
        }

        return this.filteredAllColours.length;
    }


    // ────────────────────────────────────────────────────────────────
    //  Set Parts Panel
    // ────────────────────────────────────────────────────────────────

    get filteredSetParts(): LegoSetPartData[] {
        if (!this.setPartsSearch.trim()) {
            return this.setParts;
        }

        const q = this.setPartsSearch.toLowerCase();

        return this.setParts.filter(sp => {
            const setName = (sp.legoSet?.name ?? '').toLowerCase();
            const colourName = (sp.brickColour?.name ?? '').toLowerCase();
            return setName.includes(q) || colourName.includes(q);
        });
    }


    get sortedSetParts(): LegoSetPartData[] {
        const parts = [...this.filteredSetParts];
        const dir = this.setPartsSortDir === 'asc' ? 1 : -1;

        parts.sort((a, b) => {
            switch (this.setPartsSortField) {
                case 'set':
                    return (a.legoSet?.name ?? '').localeCompare(b.legoSet?.name ?? '') * dir;
                case 'setNum':
                    return (a.legoSet?.setNumber ?? '').localeCompare(b.legoSet?.setNumber ?? '') * dir;
                case 'colour':
                    return (a.brickColour?.name ?? '').localeCompare(b.brickColour?.name ?? '') * dir;
                case 'qty':
                    return (Number(a.quantity ?? 0) - Number(b.quantity ?? 0)) * dir;
                default:
                    return 0;
            }
        });

        return parts;
    }


    sortSetParts(field: 'set' | 'setNum' | 'colour' | 'qty'): void {
        if (this.setPartsSortField === field) {
            this.setPartsSortDir = this.setPartsSortDir === 'asc' ? 'desc' : 'asc';
        } else {
            this.setPartsSortField = field;
            this.setPartsSortDir = field === 'qty' ? 'desc' : 'asc';
        }
    }


    navigateToSet(sp: LegoSetPartData): void {
        if (sp.legoSet?.id) {
            this.router.navigate(['/lego/sets', sp.legoSet.id]);
        }
    }


    getSwatchColor(sp: LegoSetPartData): string {
        const hex = sp.brickColour?.hexRgb;
        if (!hex) {
            return 'transparent';
        }
        // Handle both '05131D' and '#05131D' formats
        return hex.startsWith('#') ? hex : '#' + hex;
    }


    getSortIcon(field: string): string {
        if (this.setPartsSortField !== field) {
            return 'bi-chevron-expand';
        }
        return this.setPartsSortDir === 'asc' ? 'bi-chevron-up' : 'bi-chevron-down';
    }


    // ────────────────────────────────────────────────────────────────
    //  Three.js Scene Setup
    // ────────────────────────────────────────────────────────────────

    private async initThreeJsAndLoadModel(): Promise<void> {
        if (this.rendererCanvas == null || this.part == null) {
            return;
        }

        //
        // Initialise the LDraw file cache — hydrates THREE.Cache from IndexedDB
        // so previously-fetched geometry files load instantly without HTTP requests.
        //
        await this.fileCacheService.initialise();

        this.initScene();
        this.startAnimation();
        await this.loadLDrawModel();
    }


    private initScene(): void {
        const canvas = this.rendererCanvas.nativeElement;
        const container = canvas.parentElement!;
        const width = container.clientWidth;
        const height = container.clientHeight;

        // Scene — no background colour set so the canvas is transparent
        this.scene = new THREE.Scene();

        // Camera
        this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 2000);
        this.camera.position.set(100, 80, 150);
        this.camera.lookAt(0, 0, 0);

        // Renderer — alpha:true enables transparency; setClearColor with alpha 0 makes the
        // background fully transparent so the card's CSS background shows through.
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
        this.controls.minDistance = 20;
        this.controls.maxDistance = 500;
        this.controls.autoRotate = true;
        this.controls.autoRotateSpeed = 1.2;

        // Lighting
        this.setupLighting();

        // Ground plane (subtle grid)
        this.addGroundPlane();

        // Responsive resize
        this.resizeObserver = new ResizeObserver(() => this.onResize());
        this.resizeObserver.observe(container);

        this.sceneReady = true;
    }


    private setupLighting(): void {
        // Ambient light for base illumination
        const ambient = new THREE.AmbientLight(0xffffff, 0.4);
        this.scene.add(ambient);

        // Key light (warm, main illumination)
        const keyLight = new THREE.DirectionalLight(0xfff5e6, 1.0);
        keyLight.position.set(100, 150, 100);
        keyLight.castShadow = true;
        keyLight.shadow.mapSize.width = 1024;
        keyLight.shadow.mapSize.height = 1024;
        keyLight.shadow.camera.near = 0.5;
        keyLight.shadow.camera.far = 500;
        this.scene.add(keyLight);

        // Fill light (cool, softer)
        const fillLight = new THREE.DirectionalLight(0xb0d4f1, 0.4);
        fillLight.position.set(-80, 60, -50);
        this.scene.add(fillLight);

        // Rim light (accent highlight)
        const rimLight = new THREE.DirectionalLight(0xffa726, 0.3);
        rimLight.position.set(0, -20, -100);
        this.scene.add(rimLight);

        // Hemisphere light for subtle environment
        const hemisphere = new THREE.HemisphereLight(0x87ceeb, 0x362f2d, 0.3);
        this.scene.add(hemisphere);
    }


    private addGroundPlane(): void {
        const gridHelper = new THREE.GridHelper(300, 30, 0x333355, 0x222244);
        gridHelper.position.y = -0.5;
        (gridHelper.material as THREE.Material).opacity = 0.3;
        (gridHelper.material as THREE.Material).transparent = true;
        this.scene.add(gridHelper);
    }


    // ────────────────────────────────────────────────────────────────
    //  LDraw Model Loading
    // ────────────────────────────────────────────────────────────────

    private loadLDrawModel(): void {
        if (this.part == null || this.part.geometryFilePath == null) {
            return;
        }

        this.isLoadingModel = true;
        this.modelLoadError = null;

        const loader = new LDrawLoader();

        //
        // Required: set the conditional line material type for WebGLRenderer
        //
        loader.setConditionalLineMaterial(LDrawConditionalLineMaterial);

        //
        // Set Bearer token so the loader can fetch files from our authenticated API
        //
        loader.setRequestHeader({
            'Authorization': `Bearer ${this.authService.accessToken}`
        });

        //
        // Point the parts library at our catch-all file endpoint.
        // The server has smart file resolution so the first request always succeeds.
        //
        loader.setPartsLibraryPath(this.baseUrl + 'api/ldraw/file/');

        //
        // Preload LDraw colour configuration, then load the model.
        // Using callback-based load() for more reliable error handling.
        //
        const mainFileUrl = this.baseUrl + 'api/ldraw/file/' + this.part.geometryFilePath;

        console.log('[LDraw] Starting model load:', mainFileUrl);

        //
        // Safety timeout — if loading takes too long, fall back to box geometry
        //
        const timeoutId = setTimeout(() => {
            console.warn('[LDraw] Loading timed out after 30 seconds — falling back to box geometry.');
            this.modelLoadError = 'Model loading timed out. Showing fallback view.';
            this.buildFallbackGeometry();
            this.isLoadingModel = false;
        }, 30000);

        //
        // Try to preload materials first (non-blocking — if it fails, we continue with defaults)
        //
        const preloadUrl = this.baseUrl + 'api/ldraw/file/LDConfig.ldr';

        (loader as any).preloadMaterials(preloadUrl)
            .then(() => {
                console.log('[LDraw] Materials preloaded successfully.');
            })
            .catch((err: any) => {
                console.warn('[LDraw] Material preload failed (using defaults):', err);
            })
            .finally(() => {
                console.log('[LDraw] Starting main file load...');

                loader.load(
                    mainFileUrl,

                    // onLoad — model parsed and ready
                    (group: THREE.Group) => {
                        clearTimeout(timeoutId);
                        console.log('[LDraw] Model loaded successfully:', group.children.length, 'children');

                        //
                        // LDraw coordinate system uses Y-up with Y pointing downward
                        //
                        group.rotation.x = Math.PI;

                        this.scene.add(group);
                        this.centreAndFrameModel(group);

                        //
                        // If a colour was already selected before the model finished loading,
                        // apply it now so the viewer reflects the selection immediately.
                        //
                        if (this.selectedColour != null && this.selectedColour.hexRgb != null) {
                            this.applyColourToScene(this.selectedColour.hexRgb);
                        }

                        this.isLoadingModel = false;
                    },

                    // onProgress
                    undefined,

                    // onError
                    (error: any) => {
                        clearTimeout(timeoutId);
                        console.error('[LDraw] Model load error:', error);
                        this.modelLoadError = 'Could not load 3D model. Showing fallback view.';
                        this.buildFallbackGeometry();
                        this.isLoadingModel = false;
                    }
                );
            });
    }


    private buildFallbackGeometry(): void {
        if (this.part == null) {
            return;
        }

        //
        // Build a simple box from the part dimensions as a fallback.
        // Use the selected colour if one is active, otherwise use the default orange.
        //
        const dims = this.getPartDimensions();
        const geometry = new THREE.BoxGeometry(dims.w, dims.h, dims.d);

        const baseColour = this.selectedColour != null && this.selectedColour.hexRgb != null
            ? this.selectedColour.hexRgb.startsWith('#')
                ? this.selectedColour.hexRgb
                : '#' + this.selectedColour.hexRgb
            : '#ffa726';

        const material = new THREE.MeshStandardMaterial({
            color: new THREE.Color(baseColour),
            metalness: 0.2,
            roughness: 0.5,
            transparent: true,
            opacity: 0.85
        });

        const mesh = new THREE.Mesh(geometry, material);
        mesh.castShadow = true;
        mesh.receiveShadow = true;

        // Add wireframe
        const wireframe = new THREE.LineSegments(
            new THREE.EdgesGeometry(geometry),
            new THREE.LineBasicMaterial({ color: 0xffcc80, linewidth: 1 })
        );

        mesh.add(wireframe);
        this.scene.add(mesh);

        this.centreAndFrameModel(mesh);
    }


    private centreAndFrameModel(object: THREE.Object3D): void {
        // Centre the model
        const box = new THREE.Box3().setFromObject(object);
        const centre = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());

        object.position.sub(centre);

        // Position camera based on model size
        const maxDim = Math.max(size.x, size.y, size.z);
        const fov = this.camera.fov * (Math.PI / 180);
        const distance = maxDim / (2 * Math.tan(fov / 2)) * 2.2;

        this.camera.position.set(distance * 0.7, distance * 0.5, distance * 0.7);
        this.camera.lookAt(0, 0, 0);

        this.controls.target.set(0, 0, 0);
        this.controls.update();
    }


    private getPartDimensions(): { w: number; h: number; d: number } {
        if (this.part == null) {
            return { w: 20, h: 20, d: 20 };
        }

        if (this.part.widthLdu > 0 || this.part.heightLdu > 0 || this.part.depthLdu > 0) {
            return {
                w: Math.max(this.part.widthLdu || 20, 10),
                h: Math.max(this.part.heightLdu || 24, 6),
                d: Math.max(this.part.depthLdu || 20, 10)
            };
        }

        return { w: 40, h: 24, d: 20 };
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
        if (this.rendererCanvas == null || this.sceneReady === false) {
            return;
        }

        const container = this.rendererCanvas.nativeElement.parentElement!;
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

        this.sceneReady = false;
    }


    // ────────────────────────────────────────────────────────────────
    //  Template Helpers
    // ────────────────────────────────────────────────────────────────

    getCategoryName(): string {
        return this.part?.brickCategory?.name || this.part?.ldrawCategory || 'Unknown';
    }

    getPartTypeName(): string {
        return this.part?.partType?.name || 'Unknown';
    }

    getDimensionsText(): string {
        if (this.part == null) {
            return '—';
        }

        const w = this.part.widthLdu;
        const h = this.part.heightLdu;
        const d = this.part.depthLdu;

        if (w > 0 || h > 0 || d > 0) {
            return `${w} × ${h} × ${d} LDU`;
        }

        return 'Not specified';
    }

    getMassText(): string {
        if (this.part == null || this.part.massGrams <= 0) {
            return '—';
        }

        return `${this.part.massGrams.toFixed(2)} g`;
    }


    // ────────────────────────────────────────────────────────────────
    //  Isometric SVG fallback (same logic as parts-catalog)
    // ────────────────────────────────────────────────────────────────

    getIsometricPoints(): { top: string; front: string; side: string } {
        const dims = this.getPartDimensions();
        const maxDim = Math.max(dims.w, dims.h, dims.d);
        const scale = 60 / maxDim;

        const w = dims.w * scale;
        const h = dims.h * scale;
        const d = dims.d * scale;

        const cx = 100;
        const cy = 100;
        const ix = 0.866;
        const iy = 0.5;

        const bflX = cx - w * ix / 2 + d * ix / 2;
        const bflY = cy + h / 2;
        const bfrX = bflX + w * ix;
        const bfrY = bflY + w * iy;
        const bblX = bflX - d * ix;
        const bblY = bflY + d * iy;
        const bbrX = bblX + w * ix;
        const bbrY = bblY + w * iy;

        const tflX = bflX; const tflY = bflY - h;
        const tfrX = bfrX; const tfrY = bfrY - h;
        const tblX = bblX; const tblY = bblY - h;
        const tbrX = bbrX; const tbrY = bbrY - h;

        return {
            top: `${tflX},${tflY} ${tfrX},${tfrY} ${tbrX},${tbrY} ${tblX},${tblY}`,
            front: `${tflX},${tflY} ${tfrX},${tfrY} ${bfrX},${bfrY} ${bflX},${bflY}`,
            side: `${tfrX},${tfrY} ${tbrX},${tbrY} ${bbrX},${bbrY} ${bfrX},${bfrY}`
        };
    }


    getPartColour(): { top: string; front: string; side: string; stroke: string } {
        //
        // When a colour is selected, use it for the SVG fallback preview.
        // Otherwise fall back to the default orange palette.
        //
        if (this.selectedColour != null && this.selectedColour.hexRgb != null) {
            const hex = this.selectedColour.hexRgb.startsWith('#')
                ? this.selectedColour.hexRgb
                : '#' + this.selectedColour.hexRgb;

            //
            // Derive lighter and darker variants for the isometric faces by
            // blending the selected colour with white and black respectively.
            //
            const colour = new THREE.Color(hex);
            const topColour = colour.clone().lerp(new THREE.Color(0xffffff), 0.25);
            const sideColour = colour.clone().lerp(new THREE.Color(0x000000), 0.2);

            return {
                top: '#' + topColour.getHexString(),
                front: hex,
                side: '#' + sideColour.getHexString(),
                stroke: '#' + colour.clone().lerp(new THREE.Color(0x000000), 0.45).getHexString()
            };
        }

        return {
            top: '#ffcc80',
            front: '#ffa726',
            side: '#e09520',
            stroke: '#8d6e3f'
        };
    }
}

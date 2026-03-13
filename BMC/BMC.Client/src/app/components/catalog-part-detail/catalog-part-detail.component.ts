/*
    AI-Developed — This file was significantly developed with AI assistance.
    Original component authored by the project team.
    Colour swatch picker feature (selectColour, applyColourToScene, loadAllColours, colour mode toggle)
    was added with AI assistance — reviewed and adapted to project standards.
*/

import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

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
import { AuthNudgeService } from '../../services/auth-nudge.service';
import { LDrawFileCacheService } from '../../services/ldraw-file-cache.service';
import { lastValueFrom, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';


/**
 * Lightweight DTO returned by the /api/parts-catalog/{partId}/set-appearances endpoint.
 * Flat fields only — no nested navigation properties.
 */
export interface SetAppearanceDto {
    setName: string;
    setNumber: string;
    year: number;
    partCount: number;
    imageUrl: string | null;
    colourName: string | null;
    colourHex: string | null;
    ldrawColourCode: number;
    quantity: number;
    isSpare: boolean;
    legoSetId: number;
}

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
    // Initial colour from query params (colour ID from set detail, or hex from catalog)
    //
    private initialColourId: bigint | number | null = null;
    private initialHex: string | null = null;

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
    setParts: SetAppearanceDto[] = [];
    totalSetPartsCount = 0;
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

    // Grid and shadow plane references
    private gridHelper: THREE.GridHelper | null = null;
    private shadowPlane: THREE.Mesh | null = null;
    private partModel: THREE.Object3D | null = null;

    //
    // When an initialColourId is provided, we defer showing the 3D model
    // until the colour data has loaded and the correct colour can be applied.
    // This prevents the visible flicker from default → correct colour.
    //
    pendingColourReady = false;

    private baseUrl: string;
    private destroy$ = new Subject<void>();

    // ────────────────────────────────────────────────────────────────
    //  Server Render Tab — State
    // ────────────────────────────────────────────────────────────────

    activeViewerTab: '3d' | 'render' = '3d';
    autoRotate = false;  // start in position mode for one-click picture flow

    // Render config
    renderMode: 'simple' | 'advanced' = 'simple';
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
    explodedView = false;
    explosionFactor = 1.0;

    // PBR ray trace options (only apply when rendererType === 'raytrace')
    enablePbr = true;
    exposure = 1.0;
    aperture = 0;
    renderZoom = 1.0;

    // Render output state
    rendering = false;
    renderError = '';
    renderTimeMs = 0;
    renderedImageUrl: SafeUrl | null = null;
    private renderedBlobUrl: string | null = null;
    renderedFormat = 'png';
    batchExporting = false;
    exportingStl = false;
    stlFormat: 'binary' | 'ascii' = 'binary';

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


    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private http: HttpClient,
        public authService: AuthService,
        private authNudgeService: AuthNudgeService,
        private brickPartService: BrickPartService,
        private brickColourService: BrickColourService,
        private fileCacheService: LDrawFileCacheService,
        private sanitizer: DomSanitizer,
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
            const parsed = parseInt(colourId, 10);
            if (parsed > 0) {
                this.initialColourId = parsed;
            }
        }

        //
        // Read optional hex query parameter for pre-selecting a colour
        // when navigating from the parts catalog.
        //
        const hex = this.route.snapshot.queryParamMap.get('hex');
        if (hex) {
            this.initialHex = hex.startsWith('#') ? hex : '#' + hex;
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
        this.revokeRenderBlob();
        this.destroy$.next();
        this.destroy$.complete();
    }


    goBack(): void {
        this.location.back();
    }


    toggleAutoRotate(): void {
        this.autoRotate = !this.autoRotate;

        if (this.controls) {
            this.controls.autoRotate = this.autoRotate;
        }
    }


    /**
     * Show the auth nudge modal when an anonymous user clicks the Server Render tab.
     */
    nudgeServerRender(): void {
        this.authNudgeService.nudge({
            featureName: 'Server-Side Rendering',
            featureIcon: 'bi-camera',
            message: 'Server-side rendering produces high-quality images using the server\'s ray tracer. Sign in to access this feature.'
        });
    }


    /**
     * Show the auth nudge modal when an anonymous user clicks the Export STL button.
     *
     * AI-generated — Mar 2026.
     */
    nudgeStlExport(): void {
        this.authNudgeService.nudge({
            featureName: 'STL Export',
            featureIcon: 'bi-box',
            message: 'STL export generates a 3D-printable mesh from the part geometry on the server. Sign in to access this feature.'
        });
    }


    // ────────────────────────────────────────────────────────────────
    //  Data Loading
    // ────────────────────────────────────────────────────────────────

    private async loadPart(id: number): Promise<void> {
        this.isLoading = true;

        try {
            if (this.authService.isLoggedIn) {
                // Authenticated path — use generated data service
                const part = await lastValueFrom(this.brickPartService.GetBrickPart(id, true));
                this.part = part;
                document.title = `${part.name} — Part Detail`;

                this.loadConnectors();
                this.loadColours();
                this.loadSetParts();

                if (part.geometryOriginalFileName) {
                    this.hasGeometry = true;
                    this.pendingColourReady = true;
                    setTimeout(() => this.initThreeJsAndLoadModel(), 0);
                }
            } else {
                // Anonymous path — use public detail endpoint
                const result = await this.http.get<any>(
                    `/api/public/browse/catalog/${id}/detail`
                ).toPromise();

                if (result?.part) {
                    this.part = {
                        id: result.part.id,
                        name: result.part.name,
                        ldrawPartId: result.part.ldrawPartId,
                        ldrawTitle: result.part.ldrawTitle,
                        ldrawCategory: result.part.ldrawCategory,
                        brickCategoryId: result.part.brickCategoryId,
                        geometryOriginalFileName: result.part.geometryOriginalFileName,
                        keywords: result.part.keywords,
                        author: result.part.author,
                        widthLdu: result.part.widthLdu,
                        heightLdu: result.part.heightLdu,
                        depthLdu: result.part.depthLdu,
                        massGrams: result.part.massGrams,
                        versionNumber: result.part.versionNumber,
                        rebrickableImgUrl: result.part.rebrickableImgUrl,
                        rebrickablePartNum: result.part.rebrickablePartNum,
                        rebrickablePartUrl: result.part.rebrickablePartUrl,
                    } as any;

                    document.title = `${result.part.name} — Part Detail`;

                    // Build part colours from public response
                    this.partColours = (result.colours ?? []).map((c: any) => {
                        const col = new BrickColourData();
                        col.id = c.brickColourId;
                        col.name = c.colourName;
                        col.hexRgb = c.colourHex;
                        col.ldrawColourCode = c.ldrawColourCode;
                        return col;
                    });
                    this.partColoursSourceLoaded = true;

                    // Load set appearances from public endpoint
                    this.loadSetParts();

                    // Enable 3D viewer — uses public LDraw file endpoint (client-side rendering only)
                    if (result.part.geometryOriginalFileName) {
                        this.hasGeometry = true;
                        this.pendingColourReady = true;
                        setTimeout(() => this.initThreeJsAndLoadModel(), 0);
                    }
                }
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
        if (this.part == null) {
            this.setPartsSourceLoaded = true;
            this.buildPartColourList();
            return;
        }

        this.isLoadingSetParts = true;

        try {
            let result: { totalCount: number; items: SetAppearanceDto[] } | undefined;

            if (this.authService.isLoggedIn) {
                const headers = this.authService.GetAuthenticationHeaders();
                const url = `${this.baseUrl}api/parts-catalog/${this.part.id}/set-appearances?limit=100&sortBy=year&sortDir=desc`;
                result = await lastValueFrom(
                    this.http.get<{ totalCount: number; items: SetAppearanceDto[] }>(url, { headers })
                );
            } else {
                const url = `/api/public/browse/catalog/${this.part.id}/set-appearances?limit=100&sortBy=year&sortDir=desc`;
                result = await lastValueFrom(
                    this.http.get<{ totalCount: number; items: SetAppearanceDto[] }>(url)
                );
            }

            this.totalSetPartsCount = result?.totalCount ?? 0;
            this.setParts = result?.items ?? [];
        }
        catch {
            this.setParts = [];
            this.totalSetPartsCount = 0;
        }
        finally {
            this.isLoadingSetParts = false;
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
        // Source 2: Set appearances — each set appearance carries the colour the part
        // was used in.  The set-appearances endpoint returns flat DTOs, so we construct
        // synthetic BrickColourData entries from the flat fields.
        //
        for (const setPart of this.setParts) {
            if (setPart.colourName != null && setPart.ldrawColourCode != null) {
                const key = setPart.ldrawColourCode;
                if (!colourMap.has(key)) {
                    const synthetic = new BrickColourData();
                    synthetic.id = setPart.ldrawColourCode;
                    synthetic.name = setPart.colourName;
                    synthetic.hexRgb = setPart.colourHex;
                    synthetic.ldrawColourCode = setPart.ldrawColourCode;
                    colourMap.set(key, synthetic);
                }
            }
        }

        //
        // Sort the merged list alphabetically by name for a consistent display order.
        //
        this.partColours = Array.from(colourMap.values())
            .sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));

        //
        // Resolve the initial colour to apply.
        //
        // Priority cascade:
        //   1. colourId match by id (BrickColour PK)
        //   2. colourId match by ldrawColourCode (synthetic entries use this as id)
        //   3. hex match against known part colours
        //   4. hex applied directly to the scene (no matching swatch)
        //   5. first available colour
        //   6. default Light Bluish Gray
        //
        let resolved = false;

        if (this.initialColourId != null) {
            // Try matching by BrickColour PK
            let matchingColour = this.partColours.find(c => c.id === this.initialColourId);

            // Try matching by ldrawColourCode (synthetic entries from set appearances)
            if (matchingColour == null) {
                matchingColour = this.partColours.find(c => c.ldrawColourCode === this.initialColourId);
            }

            if (matchingColour != null) {
                this.selectColour(matchingColour);
                resolved = true;
            }
        }

        // Try hex matching (from catalog or set detail fallback)
        if (!resolved && this.initialHex != null) {
            const normHex = this.initialHex.replace('#', '').toUpperCase();
            const matchingColour = this.partColours.find(c => {
                const cHex = (c.hexRgb ?? '').replace('#', '').toUpperCase();
                return cHex === normHex;
            });

            if (matchingColour != null) {
                this.selectColour(matchingColour);
                resolved = true;
            } else {
                // Hex doesn't match any known part colour — apply it directly to the scene
                this.applyColourToScene(this.initialHex);
                resolved = true;
            }
        }

        // Generic fallback — no initial colour provided or nothing matched
        if (!resolved) {
            if (this.partColours.length === 0) {
                this.applyDefaultGray();
            }
        }

        //
        // Colour data is now resolved — clear the pending gate.
        // If the model already finished loading, this makes it visible.
        //
        this.pendingColourReady = false;
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
            if (this.authService.isLoggedIn) {
                //
                // Authenticated path — fetch from generated data service
                //
                this.allColours = await lastValueFrom(
                    this.brickColourService.GetBrickColourList({
                        active: true,
                        deleted: false,
                        pageSize: 500,
                        pageNumber: 1
                    })
                );
            } else {
                //
                // Anonymous path — use public colours endpoint
                //
                const colours = await lastValueFrom(
                    this.http.get<any[]>('/api/public/browse/colours')
                );
                this.allColours = (colours ?? []).map((c: any) => {
                    const col = new BrickColourData();
                    col.id = c.id;
                    col.name = c.name;
                    col.hexRgb = c.hexRgb;
                    col.ldrawColourCode = c.ldrawColourCode;
                    return col;
                });
            }

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
            this.updateRouteColour(null);
            return;
        }

        this.selectedColour = colour;

        //
        // Apply the selected colour to the 3D scene if it is ready
        //
        if (this.sceneReady === true && colour.hexRgb != null) {
            this.applyColourToScene(colour.hexRgb);
        }

        this.updateRouteColour(colour.id);
    }


    //
    // Update the URL query parameter to reflect the currently selected colour.
    // Uses replaceUrl so that each swatch click doesn't pollute browser history.
    //
    private updateRouteColour(colourId: bigint | number | null): void {
        this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { colourId: colourId != null ? colourId.toString() : null },
            queryParamsHandling: 'merge',
            replaceUrl: true
        });
    }


    //
    // Apply LEGO Light Bluish Gray (#C4C8CB) as the default colour when the part
    // has no known colour variants.  Only replaces the LDrawLoader's fallback
    // yellow — intentional sub-part colours are left untouched so multi-colour
    // parts retain their correct appearance.
    //
    private applyDefaultGray(): void {
        if (this.sceneReady !== true || this.scene == null) {
            return;
        }

        const lightBluishGray = new THREE.Color(0xC4C8CB);
        const hsl = { h: 0, s: 0, l: 0 };

        this.scene.traverse(child => {
            if (child instanceof THREE.Mesh) {
                const materials = Array.isArray(child.material) ? child.material : [child.material];

                for (const mat of materials) {
                    if (mat != null && 'color' in mat && mat.color instanceof THREE.Color) {
                        if (mat instanceof THREE.LineBasicMaterial || mat instanceof THREE.LineDashedMaterial) {
                            continue;
                        }
                        mat.color.getHSL(hsl);
                        // Only replace saturated yellows (the LDrawLoader fallback colour)
                        if (hsl.h >= 0.14 && hsl.h <= 0.19 && hsl.s > 0.8) {
                            mat.color.copy(lightBluishGray);
                            mat.needsUpdate = true;
                        }
                    }
                }
            }
        });
    }


    //
    // AI-Developed — Traverses the THREE.js scene and replaces the colour on all
    // mesh materials.  Handles MeshStandardMaterial, MeshPhongMaterial,
    // MeshBasicMaterial, and material arrays.  Line/edge materials are
    // intentionally left unchanged so that part outlines remain visible.
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
                const materials = Array.isArray(child.material) ? child.material : [child.material];

                for (const mat of materials) {
                    if (mat != null && 'color' in mat && mat.color instanceof THREE.Color) {
                        // Skip line/edge materials — only recolour solid meshes
                        if (mat instanceof THREE.LineBasicMaterial || mat instanceof THREE.LineDashedMaterial) {
                            continue;
                        }
                        mat.color.set(colour);
                        mat.needsUpdate = true;
                    }
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
        // Re-add the ground grid and shadow plane
        //
        this.removeGroundPlane();
        this.addGroundPlane();

        //
        // Reload the model with original colours
        //
        if (this.part.geometryOriginalFileName != null) {
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

    get filteredSetParts(): SetAppearanceDto[] {
        if (!this.setPartsSearch.trim()) {
            return this.setParts;
        }

        const q = this.setPartsSearch.toLowerCase();

        return this.setParts.filter(sp => {
            const setName = (sp.setName ?? '').toLowerCase();
            const colourName = (sp.colourName ?? '').toLowerCase();
            return setName.includes(q) || colourName.includes(q);
        });
    }


    get sortedSetParts(): SetAppearanceDto[] {
        const parts = [...this.filteredSetParts];
        const dir = this.setPartsSortDir === 'asc' ? 1 : -1;

        parts.sort((a, b) => {
            switch (this.setPartsSortField) {
                case 'set':
                    return (a.setName ?? '').localeCompare(b.setName ?? '') * dir;
                case 'setNum':
                    return (a.setNumber ?? '').localeCompare(b.setNumber ?? '') * dir;
                case 'colour':
                    return (a.colourName ?? '').localeCompare(b.colourName ?? '') * dir;
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


    navigateToSet(sp: SetAppearanceDto): void {
        if (sp.legoSetId) {
            this.router.navigate(['/lego/sets', sp.legoSetId]);
        }
    }


    getSwatchColor(sp: SetAppearanceDto): string {
        const hex = sp.colourHex;
        if (!hex) {
            return 'transparent';
        }
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
        this.controls.autoRotate = this.autoRotate;
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
        const gridColour = new THREE.Color(0x333355);

        //
        // Grid — starts at y = 0, repositioned to the model floor after loading.
        //
        this.gridHelper = new THREE.GridHelper(300, 30, gridColour, gridColour);
        this.gridHelper.position.y = 0;
        (this.gridHelper.material as THREE.Material).opacity = 0.3;
        (this.gridHelper.material as THREE.Material).transparent = true;
        this.scene.add(this.gridHelper);

        //
        // Shadow plane — transparent surface beneath the model that catches
        // soft shadows.  Gives a subtle grounding cue.
        //
        const shadowGeo = new THREE.PlaneGeometry(600, 600);
        const shadowMat = new THREE.ShadowMaterial({ opacity: 0.15 });
        this.shadowPlane = new THREE.Mesh(shadowGeo, shadowMat);
        this.shadowPlane.rotation.x = -Math.PI / 2;
        this.shadowPlane.position.y = 0;
        this.shadowPlane.receiveShadow = true;
        this.scene.add(this.shadowPlane);
    }


    private removeGroundPlane(): void {
        if (this.gridHelper) {
            this.scene.remove(this.gridHelper);
            this.gridHelper = null;
        }

        if (this.shadowPlane) {
            this.scene.remove(this.shadowPlane);
            this.shadowPlane = null;
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  LDraw Model Loading
    // ────────────────────────────────────────────────────────────────

    private loadLDrawModel(): void {
        if (this.part == null || this.part.geometryOriginalFileName == null) {
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
        // Conditionally set auth headers and endpoint based on login status.
        // Anonymous users get the public browse endpoint (client-side rendering only,
        // no server CPU cost). Authenticated users use the standard endpoint.
        //
        let fileEndpoint: string;

        if (this.authService.isLoggedIn) {
            loader.setRequestHeader({
                'Authorization': `Bearer ${this.authService.accessToken}`
            });
            fileEndpoint = this.baseUrl + 'api/ldraw/file/';
        } else {
            // Public endpoint — no auth header needed
            fileEndpoint = this.baseUrl + 'api/public/browse/ldraw/';
        }

        //
        // Point the parts library at the appropriate file endpoint.
        // The server has smart file resolution so the first request always succeeds.
        //
        loader.setPartsLibraryPath(fileEndpoint);

        //
        // Preload LDraw colour configuration, then load the model.
        // Using callback-based load() for more reliable error handling.
        //
        const mainFileUrl = fileEndpoint + this.part.geometryOriginalFileName;

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
        const preloadUrl = fileEndpoint + 'LDConfig.ldr';

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

                        //
                        // Replace default yellow materials with LEGO Light Bluish Gray (#A0A5A9).
                        // LDraw sub-parts without an explicit colour (code 16 = "inherit") can
                        // fall through to the Three.js default or the LDrawLoader's fallback
                        // yellow.  We use a fuzzy HSL check to catch any saturated yellow shade
                        // rather than comparing against an exact hex value.
                        //
                        const lightBluishGray = new THREE.Color(0xC4C8CB);
                        const hsl = { h: 0, s: 0, l: 0 };

                        group.traverse(child => {
                            if (child instanceof THREE.Mesh) {
                                const materials = Array.isArray(child.material) ? child.material : [child.material];

                                for (const mat of materials) {
                                    if (mat != null && 'color' in mat && mat.color instanceof THREE.Color) {
                                        if (mat instanceof THREE.LineBasicMaterial || mat instanceof THREE.LineDashedMaterial) {
                                            continue;
                                        }
                                        mat.color.getHSL(hsl);
                                        // Detect saturated yellows: hue ~50-70° (0.14–0.19), saturation > 80%
                                        if (hsl.h >= 0.14 && hsl.h <= 0.19 && hsl.s > 0.8) {
                                            mat.color.copy(lightBluishGray);
                                            mat.needsUpdate = true;
                                        }
                                    }
                                }
                            }
                        });

                        this.scene.add(group);
                        this.partModel = group;
                        this.centreAndFrameModel(group);

                        //
                        // If a colour was already selected before the model finished loading,
                        // apply it now so the viewer reflects the selection immediately.
                        // If no colour was selected but colour data has been resolved (part
                        // has no known colours), apply Light Bluish Gray as the default.
                        //
                        if (this.selectedColour != null && this.selectedColour.hexRgb != null) {
                            this.applyColourToScene(this.selectedColour.hexRgb);
                        } else if (this.pendingColourReady === false && this.selectedColour == null) {
                            this.applyDefaultGray();
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
        this.partModel = mesh;

        this.centreAndFrameModel(mesh);
    }


    private centreAndFrameModel(object: THREE.Object3D): void {
        const box = new THREE.Box3().setFromObject(object);
        const centre = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());

        //
        // Centre the model at the world origin
        //
        object.position.sub(centre);

        //
        // Calculate the ideal camera distance so the model fills the viewport.
        // Use the tighter of horizontal/vertical FOV so it doesn't clip on
        // narrow or short viewports.
        //
        const fovV = this.camera.fov * (Math.PI / 180);
        const fovH = 2 * Math.atan(Math.tan(fovV / 2) * this.camera.aspect);

        const distanceV = (size.y / 2) / Math.tan(fovV / 2);
        const distanceH = (Math.max(size.x, size.z) / 2) / Math.tan(fovH / 2);
        const fitDistance = Math.max(distanceV, distanceH);

        //
        // 1.2× padding factor — enough margin so the part doesn't touch
        // the viewport edges, without wasting space.
        //
        const PADDING = 1.2;
        const distance = fitDistance * PADDING;

        this.camera.position.set(distance * 0.7, distance * 0.5, distance * 0.7);
        this.camera.lookAt(0, 0, 0);
        this.camera.near = Math.max(0.1, distance * 0.01);
        this.camera.far = distance * 10;
        this.camera.updateProjectionMatrix();

        this.controls.target.set(0, 0, 0);
        this.controls.minDistance = distance * 0.1;
        this.controls.update();

        //
        // Position the grid and shadow plane at the bottom of the model so they
        // act as a "floor" beneath the part rather than slicing through it.
        //
        const floorY = -size.y / 2;

        if (this.gridHelper) {
            this.gridHelper.position.y = floorY;
        }

        if (this.shadowPlane) {
            this.shadowPlane.position.y = floorY;
        }
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


    // ────────────────────────────────────────────────────────────────
    //  Server Render Tab — Methods
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

    getCameraAngles(): { elevation: number; azimuth: number } {
        if (this.camera == null || this.controls == null) {
            return { elevation: 30, azimuth: -45 };
        }

        const position = this.camera.position.clone().sub(this.controls.target);
        const distance = position.length();

        if (distance === 0) {
            return { elevation: 0, azimuth: 0 };
        }

        //
        // The LDrawLoader applies `group.rotation.x = Math.PI` which transforms:
        //   X_ldraw →  X_threejs
        //   Y_ldraw → -Y_threejs   (flipped)
        //   Z_ldraw → -Z_threejs   (flipped)
        //
        // The server's Camera.AutoFrame uses LDraw-native coordinates where:
        //   EyeX = cx + d * cos(elev) * sin(azim)
        //   EyeY = cy - d * sin(elev)
        //   EyeZ = cz + d * cos(elev) * cos(azim)
        //
        // Converting Three.js camera position to LDraw:
        //   x_ldraw =  x_threejs
        //   y_ldraw = -y_threejs
        //   z_ldraw = -z_threejs
        //
        // Server elevation: positive = above (EyeY more negative in Y-down LDraw).
        //   sin(elev) = -y_ldraw / d  (relative to center)
        //             = -(-y_threejs) / d = y_threejs / d
        //
        // Server azimuth: atan2(x_ldraw, z_ldraw) = atan2(x_threejs, -z_threejs)
        //
        const elevation = Math.round(Math.asin(position.y / distance) * (180 / Math.PI));
        const azimuth = Math.round(Math.atan2(position.x, -position.z) * (180 / Math.PI));

        return { elevation, azimuth };
    }


    applyPoseToRender(): void {
        const angles = this.getCameraAngles();
        this.renderElevation = angles.elevation;
        this.renderAzimuth = angles.azimuth;

        //
        // Capture zoom from camera distance.
        // Zoom = baseline / current distance — closer = higher zoom.
        //
        if (this.camera && this.controls && this.partModel) {
            const pos = this.camera.position;
            const tgt = this.controls.target;
            const currentDist = Math.sqrt(
                (pos.x - tgt.x) ** 2 +
                (pos.y - tgt.y) ** 2 +
                (pos.z - tgt.z) ** 2
            );

            //
            // Compute the server's auto-frame baseline distance using the same
            // FOV-aware geometry as Camera.AutoFrame on the server side.
            // Use partModel (not scene) to exclude grid/shadow/lights from the bbox.
            //
            const box = new THREE.Box3().setFromObject(this.partModel);
            const size = box.getSize(new THREE.Vector3());

            const halfWidth = Math.max(size.x, size.z) * 0.5;
            const halfHeight = size.y * 0.5;

            // Server uses a fixed 45° vertical FOV; horizontal FOV depends on
            // the render image aspect ratio (renderWidth / renderHeight).
            const fovV = 45 * (Math.PI / 180);
            const renderAspect = this.renderWidth / Math.max(this.renderHeight, 1);
            const fovH = 2 * Math.atan(Math.tan(fovV / 2) * renderAspect);

            const distV = halfHeight / Math.tan(fovV / 2);
            const distH = halfWidth / Math.tan(fovH / 2);
            const baselineDist = Math.max(distV, distH) * 1.15;  // 1.15 = server PADDING

            if (currentDist > 0 && baselineDist > 0) {
                this.renderZoom = Math.max(0.5, Math.min(3.0,
                    parseFloat((baselineDist / currentDist).toFixed(2))
                ));
            }
        }

        this.activeViewerTab = 'render';

        //
        // Auto-trigger the render so the user immediately sees the posed view
        // instead of a stale previous render.
        //
        setTimeout(() => this.renderPart(), 0);
    }


    //
    // Server-Side Rendering
    //
    renderPart(): void {
        if (this.part == null || this.rendering) {
            return;
        }

        //
        // Turntable GIF uses a separate endpoint
        //
        if (this.outputFormat === 'gif') {
            this.renderTurntable();
            return;
        }

        this.rendering = true;
        this.renderError = '';
        this.revokeRenderBlob();

        const headers = this.authService.GetAuthenticationHeaders();
        const partNumber = this.part.name;
        const colourCode = this.selectedColour != null ? Number(this.selectedColour.ldrawColourCode) : 71;
        const effectiveAzimuth = this.flipView ? this.renderAzimuth + 180 : this.renderAzimuth;

        let url: string;

        if (this.explodedView) {
            url = `/api/part-renderer/exploded?partNumber=${encodeURIComponent(partNumber)}&colourCode=${colourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.renderElevation}&azimuth=${effectiveAzimuth}&explosionFactor=${this.explosionFactor}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}&renderer=${this.rendererType}&zoom=${this.renderZoom}`;
            if (this.rendererType === 'raytrace') {
                url += `&enablePbr=${this.enablePbr}&exposure=${this.exposure}&aperture=${this.aperture}`;
            }
        } else {
            url = `/api/part-renderer/render?partNumber=${encodeURIComponent(partNumber)}&colourCode=${colourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.renderElevation}&azimuth=${effectiveAzimuth}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}&antiAlias=${this.effectiveAntiAlias}&format=${this.outputFormat}&quality=${this.webpQuality}&renderer=${this.rendererType}&zoom=${this.renderZoom}`;

            if (this.rendererType === 'raytrace') {
                url += `&enablePbr=${this.enablePbr}&exposure=${this.exposure}&aperture=${this.aperture}`;
            }

            if (this.backgroundHex) {
                url += `&backgroundHex=${encodeURIComponent(this.backgroundHex)}`;
            }
            if (this.gradientTopHex && this.gradientBottomHex) {
                url += `&gradientTopHex=${encodeURIComponent(this.gradientTopHex)}&gradientBottomHex=${encodeURIComponent(this.gradientBottomHex)}`;
            }
        }

        const startTime = performance.now();

        this.http.get(url, { headers, responseType: 'blob' })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (blob) => {
                    this.renderTimeMs = Math.round(performance.now() - startTime);
                    this.renderedBlobUrl = URL.createObjectURL(blob);
                    this.renderedImageUrl = this.sanitizer.bypassSecurityTrustUrl(this.renderedBlobUrl);
                    this.renderedFormat = this.explodedView ? 'png' : this.outputFormat;
                    this.rendering = false;
                },
                error: (err) => {
                    this.renderTimeMs = 0;
                    this.renderError = err.status === 404 ? 'Part geometry file not found.' : 'Render failed. Please try again.';
                    this.rendering = false;
                }
            });
    }


    private renderTurntable(): void {
        if (this.part == null || this.rendering) {
            return;
        }

        this.rendering = true;
        this.renderError = '';
        this.revokeRenderBlob();

        const headers = this.authService.GetAuthenticationHeaders();
        const partNumber = this.part.name;
        const colourCode = this.selectedColour != null ? Number(this.selectedColour.ldrawColourCode) : 71;
        const url = `/api/part-renderer/turntable?partNumber=${encodeURIComponent(partNumber)}&colourCode=${colourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.renderElevation}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}&renderer=${this.rendererType}`;

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
                error: (err) => {
                    this.renderTimeMs = 0;
                    this.renderError = err.status === 404 ? 'Part geometry file not found.' : 'Turntable render failed. Please try again.';
                    this.rendering = false;
                }
            });
    }


    downloadRender(): void {
        if (this.renderedBlobUrl == null || this.part == null) {
            return;
        }

        const colourCode = this.selectedColour != null ? Number(this.selectedColour.ldrawColourCode) : 71;
        const ext = this.renderedFormat === 'gif' ? 'gif' : this.renderedFormat === 'webp' ? 'webp' : this.renderedFormat === 'svg' ? 'svg' : 'png';
        const baseName = `${this.part.name}_c${colourCode}_${this.renderWidth}x${this.renderHeight}`;
        const a = document.createElement('a');
        a.href = this.renderedBlobUrl;
        a.download = `${baseName}.${ext}`;
        a.click();
    }


    batchExport(): void {
        if (this.part == null || this.batchExporting) {
            return;
        }

        this.batchExporting = true;
        const headers = this.authService.GetAuthenticationHeaders()
            .set('Content-Type', 'application/json');

        const colourCode = this.selectedColour != null ? Number(this.selectedColour.ldrawColourCode) : 71;
        const effectiveAzimuth = this.flipView ? this.renderAzimuth + 180 : this.renderAzimuth;

        const body = {
            partNumber: this.part.name,
            colourCode: colourCode,
            elevation: this.renderElevation,
            azimuth: effectiveAzimuth,
            renderEdges: this.renderEdges,
            smoothShading: this.smoothShading,
            antiAlias: this.effectiveAntiAlias,
            backgroundHex: this.backgroundHex,
            gradientTopHex: this.gradientTopHex,
            gradientBottomHex: this.gradientBottomHex,
            renderer: this.rendererType,
            sizes: this.activeSizePresets.map(p => ({ width: p.w, height: p.h }))
        };

        this.http.post('/api/part-renderer/batch-render', body, { headers, responseType: 'blob' })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (blob) => {
                    const url = URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `${this.part!.name}_renders.zip`;
                    a.click();
                    URL.revokeObjectURL(url);
                    this.batchExporting = false;
                },
                error: () => {
                    this.batchExporting = false;
                    this.renderError = 'Batch export failed. Please try again.';
                }
            });
    }


    /**
     * Export the part geometry as an STL file via the server-side exporter.
     * Supports both binary (compact) and ASCII (human-readable) formats.
     *
     * AI-generated — Mar 2026.
     */
    exportStl(): void {
        if (this.part == null || this.exportingStl) {
            return;
        }

        this.exportingStl = true;
        const headers = this.authService.GetAuthenticationHeaders();
        const colourCode = this.selectedColour != null ? Number(this.selectedColour.ldrawColourCode) : 71;

        const url = `/api/part-renderer/export-stl?partNumber=${encodeURIComponent(this.part.name)}&colourCode=${colourCode}&format=${this.stlFormat}`;

        this.http.get(url, { headers, responseType: 'blob' })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (blob) => {
                    const blobUrl = URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = blobUrl;
                    const title = this.part!.ldrawTitle || this.part!.name;
                    a.download = `${this.part!.name} - ${title}.stl`;
                    a.click();
                    URL.revokeObjectURL(blobUrl);
                    this.exportingStl = false;
                },
                error: () => {
                    this.exportingStl = false;
                    this.renderError = 'STL export failed. Please try again.';
                }
            });
    }


    getSelectedRenderColourName(): string {
        if (this.selectedColour != null) {
            return this.selectedColour.name || 'Unknown';
        }
        return 'Light Bluish Gray';
    }


    private revokeRenderBlob(): void {
        if (this.renderedBlobUrl != null) {
            URL.revokeObjectURL(this.renderedBlobUrl);
            this.renderedBlobUrl = null;
            this.renderedImageUrl = null;
        }
    }
}

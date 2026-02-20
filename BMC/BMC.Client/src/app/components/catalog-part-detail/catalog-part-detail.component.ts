import { Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';

import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { LDrawLoader } from 'three/examples/jsm/loaders/LDrawLoader.js';
import { LDrawConditionalLineMaterial } from 'three/examples/jsm/materials/LDrawConditionalLineMaterial.js';

import { BrickPartService, BrickPartData } from '../../bmc-data-services/brick-part.service';
import { BrickPartConnectorData } from '../../bmc-data-services/brick-part-connector.service';
import { BrickPartColourData } from '../../bmc-data-services/brick-part-colour.service';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
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

    // Three.js objects
    private scene!: THREE.Scene;
    private camera!: THREE.PerspectiveCamera;
    private renderer!: THREE.WebGLRenderer;
    private controls!: OrbitControls;
    private animationFrameId: number | null = null;
    private resizeObserver: ResizeObserver | null = null;
    private sceneReady = false;

    private baseUrl: string;


    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private http: HttpClient,
        private authService: AuthService,
        private brickPartService: BrickPartService,
        @Inject('BASE_URL') baseUrl: string
    ) {
        this.baseUrl = baseUrl;
    }


    ngOnInit(): void {
        const partId = this.route.snapshot.paramMap.get('partId');

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

            // Load related data
            this.loadConnectors();
            this.loadColours();

            // Check for geometry and initialise 3D viewer
            if (part.geometryFilePath) {
                this.hasGeometry = true;

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
            return;
        }

        this.isLoadingColours = true;

        try {
            this.colours = await this.part.BrickPartColours;
        }
        catch {
            this.colours = [];
        }
        finally {
            this.isLoadingColours = false;
        }
    }


    // ────────────────────────────────────────────────────────────────
    //  Three.js Scene Setup
    // ────────────────────────────────────────────────────────────────

    private async initThreeJsAndLoadModel(): Promise<void> {
        if (this.rendererCanvas == null || this.part == null) {
            return;
        }

        this.initScene();
        this.startAnimation();
        await this.loadLDrawModel();
    }


    private initScene(): void {
        const canvas = this.rendererCanvas.nativeElement;
        const container = canvas.parentElement!;
        const width = container.clientWidth;
        const height = container.clientHeight;

        // Scene
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x1a1a2e);

        // Subtle gradient fog for depth
        this.scene.fog = new THREE.Fog(0x1a1a2e, 400, 900);

        // Camera
        this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 2000);
        this.camera.position.set(100, 80, 150);
        this.camera.lookAt(0, 0, 0);

        // Renderer
        this.renderer = new THREE.WebGLRenderer({
            canvas: canvas,
            antialias: true,
            alpha: true
        });

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
        // Build a simple box from the part dimensions as a fallback
        //
        const dims = this.getPartDimensions();
        const geometry = new THREE.BoxGeometry(dims.w, dims.h, dims.d);

        const material = new THREE.MeshStandardMaterial({
            color: 0xffa726,
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
        return {
            top: '#ffcc80',
            front: '#ffa726',
            side: '#e09520',
            stroke: '#8d6e3f'
        };
    }
}

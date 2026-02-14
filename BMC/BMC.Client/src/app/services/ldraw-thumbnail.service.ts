import { Injectable, Inject } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import Dexie, { Table } from 'dexie';

import * as THREE from 'three';
import { LDrawLoader } from 'three/examples/jsm/loaders/LDrawLoader.js';
import { LDrawConditionalLineMaterial } from 'three/examples/jsm/materials/LDrawConditionalLineMaterial.js';

import { AuthService } from './auth.service';
import { BrickPartData } from '../bmc-data-services/brick-part.service';

/**
 * Thumbnail result emitted for each successfully rendered part.
 */
export interface ThumbnailResult {
    geometryFilePath: string;
    dataUrl: string;
}

/**
 * Persisted thumbnail entry in IndexedDB.
 */
interface ThumbnailCacheEntry {
    geometryFilePath: string;
    dataUrl: string;
    cachedAt: number;
}

/**
 * Dexie database for persistent thumbnail storage.
 */
class ThumbnailCacheDatabase extends Dexie {
    thumbnails!: Table<ThumbnailCacheEntry, string>;

    constructor() {
        super('bmc-thumbnails');
        this.version(1).stores({
            thumbnails: 'geometryFilePath'
        });
    }
}

/**
 * Renders 3D LDraw previews offscreen using a single shared WebGL context,
 * then exports them as data:image/png URLs for use in <img> elements.
 *
 * Thumbnails are persisted in IndexedDB so subsequent visits load instantly
 * without re-rendering. The in-memory Map is pre-loaded from IndexedDB on
 * first use.
 *
 * This approach avoids the browser's WebGL context limit (~8-16) which would
 * be exceeded by putting a separate <canvas> on each catalog card.
 */
@Injectable()
export class LDrawThumbnailService {

    //
    // Offscreen renderer — one shared context for the lifetime of the app
    //
    private renderer!: THREE.WebGLRenderer;
    private scene!: THREE.Scene;
    private camera!: THREE.PerspectiveCamera;
    private loader!: LDrawLoader;
    private canvas!: HTMLCanvasElement;

    //
    // Cache: geometryFilePath → data:image/png
    //
    private cache = new Map<string, string>();

    //
    // Emits results as each part finishes rendering
    //
    private thumbnailSubject = new Subject<ThumbnailResult>();
    public thumbnail$: Observable<ThumbnailResult> = this.thumbnailSubject.asObservable();

    //
    // Persistent IndexedDB cache
    //
    private db: ThumbnailCacheDatabase;
    private dbLoaded = false;
    private dbLoadPromise: Promise<void> | null = null;

    //
    // Internal state
    //
    private initialized = false;
    private materialsPreloaded = false;
    private rendering = false;
    private queue: BrickPartData[] = [];
    private queueGeneration = 0;  // incremented on each renderBatch to abort stale loops
    private baseUrl: string;

    // Thumbnail dimensions (px)
    private readonly WIDTH = 200;
    private readonly HEIGHT = 200;

    // Delay between consecutive renders (ms) — prevents 429s from sub-file requests
    private readonly RENDER_DELAY_MS = 100;


    constructor(
        private authService: AuthService,
        @Inject('BASE_URL') baseUrl: string
    ) {
        this.baseUrl = baseUrl;
        this.db = new ThumbnailCacheDatabase();
    }


    /**
     * Render 3D thumbnails for a batch of parts.
     * Results arrive asynchronously via thumbnail$.
     * Automatically deduplicates (skips already-cached parts).
     */
    renderBatch(parts: BrickPartData[]): void {

        //
        // Ensure IndexedDB thumbnails are loaded into memory first
        //
        if (!this.dbLoaded) {
            if (!this.dbLoadPromise) {
                this.dbLoadPromise = this.loadFromIndexedDB();
            }
            this.dbLoadPromise.then(() => this.renderBatch(parts));
            return;
        }

        //
        // Filter to parts that have a geometry file and aren't already cached
        //
        const needed = parts.filter(p =>
            p.geometryFilePath &&
            !this.cache.has(p.geometryFilePath)
        );

        if (needed.length === 0) {
            //
            // Emit cached results immediately for cards that already have thumbnails
            //
            for (const part of parts) {
                if (part.geometryFilePath && this.cache.has(part.geometryFilePath)) {
                    this.thumbnailSubject.next({
                        geometryFilePath: part.geometryFilePath,
                        dataUrl: this.cache.get(part.geometryFilePath)!
                    });
                }
            }
            return;
        }

        //
        // Replace the queue (new page navigation cancels previous batch)
        //
        this.queue = [...needed];
        this.queueGeneration++;

        if (!this.rendering) {
            this.processQueue(this.queueGeneration);
        }
    }


    /**
     * Get a cached thumbnail URL, or null if not yet rendered.
     */
    getCached(geometryFilePath: string): string | null {
        return this.cache.get(geometryFilePath) ?? null;
    }


    /**
     * Initialise the offscreen WebGL renderer, scene, camera, lights, and loader.
     * Called lazily on first render request.
     */
    private ensureInitialized(): void {
        if (this.initialized) return;

        //
        // Create an offscreen canvas — not attached to the DOM
        //
        this.canvas = document.createElement('canvas');
        this.canvas.width = this.WIDTH;
        this.canvas.height = this.HEIGHT;

        //
        // WebGL renderer with transparent background, preserveDrawingBuffer for toDataURL()
        //
        this.renderer = new THREE.WebGLRenderer({
            canvas: this.canvas,
            antialias: true,
            alpha: true,
            preserveDrawingBuffer: true
        });
        this.renderer.setSize(this.WIDTH, this.HEIGHT);
        this.renderer.setPixelRatio(1); // consistent quality, not device-dependent
        this.renderer.setClearColor(0x000000, 0); // transparent background

        //
        // Scene with lighting
        //
        this.scene = new THREE.Scene();

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
        this.scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.9);
        directionalLight.position.set(50, 100, 80);
        this.scene.add(directionalLight);

        const fillLight = new THREE.DirectionalLight(0xffffff, 0.3);
        fillLight.position.set(-50, 30, -50);
        this.scene.add(fillLight);

        //
        // Camera — will be repositioned per model
        //
        this.camera = new THREE.PerspectiveCamera(35, this.WIDTH / this.HEIGHT, 0.1, 10000);

        //
        // LDrawLoader — shared instance with auth and paths configured
        //
        this.loader = new LDrawLoader();
        (this.loader as any).setConditionalLineMaterial(LDrawConditionalLineMaterial);
        this.loader.setPartsLibraryPath(this.baseUrl + 'api/ldraw/file/');

        this.initialized = true;
    }


    /**
     * Preload the LDraw material/colour definitions (LDConfig.ldr).
     * Done once, before the first model load.
     */
    private async preloadMaterials(): Promise<void> {
        if (this.materialsPreloaded) return;

        //
        // Set auth header before preloading
        //
        this.updateAuthHeaders();

        const materialUrl = this.baseUrl + 'api/ldraw/file/LDConfig.ldr';
        await (this.loader as any).preloadMaterials(materialUrl);
        this.materialsPreloaded = true;
    }


    /**
     * Update auth headers on the loader (token may have refreshed).
     */
    private updateAuthHeaders(): void {
        const token = this.authService.accessToken;
        if (token) {
            this.loader.setRequestHeader({
                'Authorization': `Bearer ${token}`
            });
        }
    }


    /**
     * Process the render queue sequentially — one model at a time.
     * Checks queueGeneration to abort if a new batch was started.
     */
    private async processQueue(generation: number): Promise<void> {
        this.rendering = true;

        this.ensureInitialized();
        await this.preloadMaterials();

        while (this.queue.length > 0) {

            //
            // Abort if a new renderBatch() started a new generation
            //
            if (generation !== this.queueGeneration) {
                break;
            }

            const part = this.queue.shift()!;

            if (!part.geometryFilePath || this.cache.has(part.geometryFilePath)) {
                continue;
            }

            try {
                const dataUrl = await this.renderSinglePart(part);
                if (dataUrl) {
                    this.cache.set(part.geometryFilePath, dataUrl);
                    this.thumbnailSubject.next({
                        geometryFilePath: part.geometryFilePath,
                        dataUrl
                    });

                    //
                    // Persist to IndexedDB (fire-and-forget)
                    //
                    this.persistToIndexedDB(part.geometryFilePath, dataUrl);
                }
            } catch (err) {
                console.warn(`[LDrawThumbnail] Failed to render ${part.geometryFilePath}:`, err);
            }

            //
            // Throttle: wait between renders to avoid hammering the server
            // with sub-file requests (fixes 429 Too Many Requests)
            //
            if (this.queue.length > 0 && generation === this.queueGeneration) {
                await this.delay(this.RENDER_DELAY_MS);
            }
        }

        this.rendering = false;
    }


    /**
     * Load a single part model, frame it in the camera, render one frame, and export.
     */
    private renderSinglePart(part: BrickPartData): Promise<string | null> {
        return new Promise((resolve) => {

            //
            // Refresh auth header in case the token was renewed
            //
            this.updateAuthHeaders();

            const modelUrl = this.baseUrl + 'api/ldraw/file/' + part.geometryFilePath;

            //
            // Safety timeout — skip parts that take too long
            //
            const timeoutId = setTimeout(() => {
                console.warn(`[LDrawThumbnail] Timed out loading ${part.geometryFilePath}`);
                resolve(null);
            }, 15000);

            this.loader.load(
                modelUrl,
                (group: THREE.Group) => {
                    clearTimeout(timeoutId);

                    try {
                        //
                        // Clear scene of previous model (keep lights)
                        //
                        this.clearModel();

                        //
                        // Add model to scene
                        //
                        this.scene.add(group);

                        //
                        // Frame the model — compute bounding box, position camera
                        //
                        this.frameModel(group);

                        //
                        // Render one frame
                        //
                        this.renderer.render(this.scene, this.camera);

                        //
                        // Export to data URL
                        //
                        const dataUrl = this.canvas.toDataURL('image/png');
                        resolve(dataUrl);

                    } catch (err) {
                        console.warn(`[LDrawThumbnail] Render error for ${part.geometryFilePath}:`, err);
                        resolve(null);
                    }
                },
                undefined, // onProgress
                (err: unknown) => {
                    clearTimeout(timeoutId);
                    console.warn(`[LDrawThumbnail] Load error for ${part.geometryFilePath}:`, err);
                    resolve(null);
                }
            );
        });
    }


    /**
     * Remove all model objects from the scene, keeping lights.
     */
    private clearModel(): void {
        const toRemove: THREE.Object3D[] = [];
        this.scene.traverse((child) => {
            if (child instanceof THREE.Group || child instanceof THREE.Mesh || child instanceof THREE.LineSegments) {
                if (child.parent === this.scene) {
                    toRemove.push(child);
                }
            }
        });
        for (const obj of toRemove) {
            this.scene.remove(obj);
            obj.traverse((node: any) => {
                if (node.geometry) node.geometry.dispose();
                if (node.material) {
                    if (Array.isArray(node.material)) {
                        node.material.forEach((m: any) => m.dispose());
                    } else {
                        node.material.dispose();
                    }
                }
            });
        }
    }


    /**
     * Position the camera to frame the model nicely.
     * Uses an isometric-style elevated angle for consistent thumbnails.
     */
    private frameModel(group: THREE.Group): void {
        const box = new THREE.Box3().setFromObject(group);
        const center = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());

        //
        // Get the largest dimension to determine how far the camera should be
        //
        const maxDim = Math.max(size.x, size.y, size.z);
        const fov = this.camera.fov * (Math.PI / 180);
        let cameraDistance = (maxDim / 2) / Math.tan(fov / 2);
        cameraDistance *= 1.6; // add breathing room

        //
        // Position camera at an isometric-ish angle (elevated, slightly rotated)
        //
        const angle = Math.PI / 6; // 30° rotation
        const elevation = Math.PI / 6; // 30° elevation

        this.camera.position.set(
            center.x + cameraDistance * Math.cos(elevation) * Math.sin(angle),
            center.y + cameraDistance * Math.sin(elevation),
            center.z + cameraDistance * Math.cos(elevation) * Math.cos(angle)
        );

        this.camera.lookAt(center);
        this.camera.updateProjectionMatrix();
    }


    /**
     * Load all persisted thumbnails from IndexedDB into the in-memory cache.
     * Called once, lazily on first renderBatch().
     */
    private async loadFromIndexedDB(): Promise<void> {
        try {
            const entries = await this.db.thumbnails.toArray();
            for (const entry of entries) {
                this.cache.set(entry.geometryFilePath, entry.dataUrl);
            }
        } catch (err) {
            console.warn('[LDrawThumbnail] Failed to load from IndexedDB:', err);
        }
        this.dbLoaded = true;
    }


    /**
     * Persist a single rendered thumbnail to IndexedDB (fire-and-forget).
     */
    private async persistToIndexedDB(geometryFilePath: string, dataUrl: string): Promise<void> {
        try {
            await this.db.thumbnails.put({
                geometryFilePath,
                dataUrl,
                cachedAt: Date.now()
            });
        } catch (err) {
            console.warn('[LDrawThumbnail] Failed to persist thumbnail:', err);
        }
    }


    /**
     * Simple promise-based delay for throttling.
     */
    private delay(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}

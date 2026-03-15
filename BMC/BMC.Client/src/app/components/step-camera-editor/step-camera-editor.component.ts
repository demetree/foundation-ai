//
// Step Camera Editor Component
//
// Interactive Three.js orbit camera editor for manual build steps.
// Shows a wireframe bounding box representing the model and lets users
// orbit, pan, and zoom to set the camera view angle for a step render.
//
// Emits camera position/target changes so the parent can update step data.
//

import {
    Component,
    Input,
    Output,
    EventEmitter,
    OnInit,
    OnDestroy,
    AfterViewInit,
    ElementRef,
    ViewChild,
    SimpleChanges,
    OnChanges
} from '@angular/core';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';

export interface CameraState {
    positionX: number;
    positionY: number;
    positionZ: number;
    targetX: number;
    targetY: number;
    targetZ: number;
}

@Component({
    selector: 'app-step-camera-editor',
    templateUrl: './step-camera-editor.component.html',
    styleUrls: ['./step-camera-editor.component.scss']
})
export class StepCameraEditorComponent implements OnInit, AfterViewInit, OnDestroy, OnChanges {

    @ViewChild('rendererCanvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;

    @Input() positionX = 100;
    @Input() positionY = 100;
    @Input() positionZ = -100;
    @Input() targetX = 0;
    @Input() targetY = 0;
    @Input() targetZ = 0;

    @Output() cameraChange = new EventEmitter<CameraState>();

    private renderer!: THREE.WebGLRenderer;
    private scene!: THREE.Scene;
    private camera!: THREE.PerspectiveCamera;
    private controls!: OrbitControls;
    private animationId = 0;
    private isDestroyed = false;

    ngOnInit(): void {}

    ngAfterViewInit(): void {
        this.initScene();
        this.animate();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (!this.camera || !this.controls) return;

        // Update camera position from parent inputs
        if (changes['positionX'] || changes['positionY'] || changes['positionZ']) {
            this.camera.position.set(this.positionX, this.positionY, this.positionZ);
        }
        if (changes['targetX'] || changes['targetY'] || changes['targetZ']) {
            this.controls.target.set(this.targetX, this.targetY, this.targetZ);
        }
        this.controls.update();
    }

    ngOnDestroy(): void {
        this.isDestroyed = true;
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
        }
        this.controls?.dispose();
        this.renderer?.dispose();
    }

    private initScene(): void {
        const canvas = this.canvasRef.nativeElement;
        const w = canvas.clientWidth || 280;
        const h = canvas.clientHeight || 200;

        // Renderer
        this.renderer = new THREE.WebGLRenderer({
            canvas,
            antialias: true,
            alpha: true
        });
        this.renderer.setSize(w, h);
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.setClearColor(0x1a1a2e, 1);

        // Scene
        this.scene = new THREE.Scene();

        // Grid helper for orientation
        const grid = new THREE.GridHelper(200, 20, 0x444488, 0x222244);
        this.scene.add(grid);

        // Axis helper
        const axes = new THREE.AxesHelper(60);
        this.scene.add(axes);

        // Wireframe box representing model bounding box
        const boxGeom = new THREE.BoxGeometry(80, 60, 80);
        const wireframe = new THREE.WireframeGeometry(boxGeom);
        const lineMat = new THREE.LineBasicMaterial({ color: 0x6c8eff, opacity: 0.6, transparent: true });
        const wireBox = new THREE.LineSegments(wireframe, lineMat);
        wireBox.position.y = 30; // Raise above grid
        this.scene.add(wireBox);

        // Solid semi-transparent faces for depth perception
        const solidMat = new THREE.MeshBasicMaterial({
            color: 0x3355aa,
            transparent: true,
            opacity: 0.08,
            side: THREE.DoubleSide
        });
        const solidBox = new THREE.Mesh(boxGeom, solidMat);
        solidBox.position.y = 30;
        this.scene.add(solidBox);

        // Camera
        this.camera = new THREE.PerspectiveCamera(45, w / h, 1, 5000);
        this.camera.position.set(this.positionX, this.positionY, this.positionZ);

        // Controls
        this.controls = new OrbitControls(this.camera, canvas);
        this.controls.target.set(this.targetX, this.targetY, this.targetZ);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.08;
        this.controls.rotateSpeed = 0.8;
        this.controls.zoomSpeed = 1.2;
        this.controls.panSpeed = 0.6;
        this.controls.update();

        // Emit camera changes when user finishes interacting
        this.controls.addEventListener('end', () => {
            this.emitCameraState();
        });

        // Also update on change for real-time feedback
        this.controls.addEventListener('change', () => {
            // Render immediately on change for smooth interaction
        });

        // Handle resize
        const observer = new ResizeObserver(() => {
            if (this.isDestroyed) return;
            const cw = canvas.clientWidth;
            const ch = canvas.clientHeight;
            if (cw > 0 && ch > 0) {
                this.camera.aspect = cw / ch;
                this.camera.updateProjectionMatrix();
                this.renderer.setSize(cw, ch);
            }
        });
        observer.observe(canvas);
    }

    private animate(): void {
        if (this.isDestroyed) return;
        this.animationId = requestAnimationFrame(() => this.animate());
        this.controls?.update();
        this.renderer?.render(this.scene, this.camera);
    }

    private emitCameraState(): void {
        const pos = this.camera.position;
        const tgt = this.controls.target;

        this.cameraChange.emit({
            positionX: Math.round(pos.x * 100) / 100,
            positionY: Math.round(pos.y * 100) / 100,
            positionZ: Math.round(pos.z * 100) / 100,
            targetX: Math.round(tgt.x * 100) / 100,
            targetY: Math.round(tgt.y * 100) / 100,
            targetZ: Math.round(tgt.z * 100) / 100
        });
    }

    /** Reset camera to default position */
    resetCamera(): void {
        this.camera.position.set(100, 100, -100);
        this.controls.target.set(0, 0, 0);
        this.controls.update();
        this.emitCameraState();
    }
}

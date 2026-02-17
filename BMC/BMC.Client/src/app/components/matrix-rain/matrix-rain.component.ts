// ======================================
// AI-DEVELOPED: Canvas-based Matrix rain animation.
// Renders falling columns of glowing green characters (katakana, latin, digits)
// at different speeds and brightnesses to simulate depth.
// Only active when the Matrix theme is selected.
// ======================================

import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { ThemeService } from '../../services/theme.service';


@Component({
    selector: 'app-matrix-rain',
    template: `<canvas #canvas class="matrix-rain-canvas"></canvas>`,
    styles: [`
        :host {
            display: block;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 0;
            pointer-events: none;
        }
        .matrix-rain-canvas {
            width: 100%;
            height: 100%;
            display: block;
        }
    `]
})
export class MatrixRainComponent implements OnInit, OnDestroy {

    @ViewChild('canvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;

    private ctx!: CanvasRenderingContext2D;
    private animationId = 0;
    private columns: number[] = [];
    private columnSpeeds: number[] = [];
    private columnBrightness: number[] = [];
    private fontSize = 16;
    private active = false;
    private themeSub!: Subscription;

    // Character pool: half-width katakana + latin + digits
    private readonly chars =
        'アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン' +
        'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$%^&*()';


    constructor(private themeService: ThemeService) { }


    ngOnInit(): void {
        this.themeSub = this.themeService.currentTheme$.subscribe(theme => {
            if (theme === 'matrix') {
                this.start();
            } else {
                this.stop();
            }
        });
    }


    ngOnDestroy(): void {
        this.stop();
        this.themeSub?.unsubscribe();
    }


    // ── Private ──────────────────────────────────────────────────────────

    private start(): void {
        if (this.active) return;
        this.active = true;

        const canvas = this.canvasRef.nativeElement;
        this.ctx = canvas.getContext('2d')!;

        this.resize();
        window.addEventListener('resize', this.onResize);
        this.animate();
    }


    private stop(): void {
        if (!this.active) return;
        this.active = false;

        cancelAnimationFrame(this.animationId);
        window.removeEventListener('resize', this.onResize);

        // Clear canvas
        const canvas = this.canvasRef.nativeElement;
        this.ctx?.clearRect(0, 0, canvas.width, canvas.height);
    }


    private onResize = (): void => {
        this.resize();
    };


    private resize(): void {
        const canvas = this.canvasRef.nativeElement;
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        const colCount = Math.floor(canvas.width / this.fontSize);

        // Initialize column drop positions at random heights
        this.columns = Array.from({ length: colCount }, () => Math.random() * canvas.height / this.fontSize);

        // Varying speeds for depth: some fast (foreground), some slow (background)
        this.columnSpeeds = Array.from({ length: colCount }, () => 0.3 + Math.random() * 0.9);

        // Varying brightness for depth: 0.25 (far) to 1.0 (near)
        this.columnBrightness = Array.from({ length: colCount }, () => 0.2 + Math.random() * 0.8);
    }


    private lastTime = 0;
    private readonly targetFps = 20; // Intentionally low for the "stepped" matrix look
    private readonly frameInterval = 1000 / this.targetFps;


    private animate = (time: number = 0): void => {
        if (!this.active) return;

        this.animationId = requestAnimationFrame(this.animate);

        // Throttle to target FPS for the classic stepped look
        const delta = time - this.lastTime;
        if (delta < this.frameInterval) return;
        this.lastTime = time - (delta % this.frameInterval);

        this.draw();
    };


    private draw(): void {
        const canvas = this.canvasRef.nativeElement;
        const ctx = this.ctx;

        // Reset shadow state (may be left over from previous frame's glow)
        ctx.shadowBlur = 0;
        ctx.shadowColor = 'transparent';

        // Semi-transparent black overlay to create trails
        ctx.fillStyle = 'rgba(0, 0, 0, 0.12)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.font = `${this.fontSize}px monospace`;

        for (let i = 0; i < this.columns.length; i++) {
            const char = this.chars[Math.floor(Math.random() * this.chars.length)];
            const x = i * this.fontSize;
            const y = this.columns[i] * this.fontSize;
            const brightness = this.columnBrightness[i];

            // Lead character: bright white-green glow
            const isLead = Math.random() > 0.98;
            if (isLead) {
                ctx.shadowBlur = 15;
                ctx.shadowColor = '#00ff41';
                ctx.fillStyle = `rgba(180, 255, 180, ${brightness})`;
            } else {
                ctx.shadowBlur = 0;
                ctx.shadowColor = 'transparent';
                // Green tint varies with depth
                const g = Math.floor(180 + 75 * brightness);
                ctx.fillStyle = `rgba(0, ${g}, 65, ${brightness * 0.9})`;
            }

            ctx.fillText(char, x, y);

            // Advance the column
            this.columns[i] += this.columnSpeeds[i];

            // Reset column when it reaches the bottom (with some randomness)
            if (y > canvas.height && Math.random() > 0.975) {
                this.columns[i] = 0;
                // Re-randomize on reset for variety
                this.columnSpeeds[i] = 0.3 + Math.random() * 0.9;
                this.columnBrightness[i] = 0.2 + Math.random() * 0.8;
            }
        }
    }
}

//
// Manual Generator Component
//
// Upload an LDraw model file (.mpd, .ldr, .dat) to generate a printable build manual.
// The workflow is:
//   1. User uploads a file via drag-and-drop or file picker
//   2. Optionally analyse the file to preview step/part counts
//   3. Configure rendering options (page size, image size, camera angles, etc.)
//   4. Generate the manual — the server streams step renders via SignalR
//   5. Preview the result page-by-page, download as HTML, or print directly
//
// AI-assisted development — reviewed and adapted to project conventions.
//

import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Subscription, firstValueFrom } from 'rxjs';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ManualGeneratorSignalrService, StepProgressEvent, GenerationCompleteEvent, ManualOptionsDto } from '../../services/manual-generator-signalr.service';
import { AuthService } from '../../services/auth.service';

interface StepAnalysis {
    stepIndex: number;
    newParts: {
        fileName: string;
        colourCode: number;
        quantity: number;
        partDescription: string | null;
        colourName: string | null;
        colourHex: string | null;
    }[];
    cumulativePartCount: number;
    cumulativeTriangleCount: number;
}

interface AnalyseResponse {
    fileName: string;
    stepCount: number;
    steps: StepAnalysis[];
    totalParts: number;
    totalTriangleCount: number;
    uniquePartCount: number;
}


@Component({
    selector: 'app-manual-generator',
    templateUrl: './manual-generator.component.html',
    styleUrls: ['./manual-generator.component.scss']
})
export class ManualGeneratorComponent implements OnInit, OnDestroy {

    // ─── Constants ───────────────────────────────────────────────────────
    private readonly MAX_FILE_SIZE_BYTES = 20 * 1024 * 1024;  // 20 MB

    // ─── Upload State ────────────────────────────────────────────────────
    isDragOver = false;
    selectedFile: File | null = null;
    uploadError: string | null = null;

    // ─── Analysis State ──────────────────────────────────────────────────
    isAnalysing = false;
    analysis: AnalyseResponse | null = null;

    // ─── Options ─────────────────────────────────────────────────────────
    options: ManualOptionsDto = {
        pageSize: 'a4',
        imageSize: 800,
        elevation: 30,
        azimuth: -45,
        renderEdges: true,
        smoothShading: true,
        outputFormat: 'html',
        renderer: 'rasterizer'
    };

    // ─── Generation State ────────────────────────────────────────────────
    isGenerating = false;
    generationProgress = 0;
    generationTotal = 0;
    currentPreview: string | null = null;
    generationError: string | null = null;

    // ─── Connection State ────────────────────────────────────────────────
    isReconnecting = false;

    // ─── Result ──────────────────────────────────────────────────────────
    generatedHtml: string | null = null;
    generatedPdf: string | null = null;   // base64-encoded PDF bytes (legacy)
    pdfDownloadUrl: string | null = null; // server-side download URL (preferred)
    htmlDownloadUrl: string | null = null; // server-side download URL for HTML
    resultStats: { totalSteps: number; totalParts: number; renderTimeMs: number } | null = null;

    // ─── Pagination ──────────────────────────────────────────────────────
    currentPage = 0;
    pages: SafeHtml[] = [];

    private subs: Subscription[] = [];

    constructor(
        private http: HttpClient,
        private signalr: ManualGeneratorSignalrService,
        private authService: AuthService,
        private sanitizer: DomSanitizer
    ) { }


    ngOnInit(): void {
        document.title = 'Manual Generator';

        //
        // Subscribe to SignalR events
        //
        this.subs.push(
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
                    this.generatedPdf = e.pdfBase64 ?? null;
                } else if (e.downloadUrl) {
                    // Fetch HTML from server (avoids SignalR size limits)
                    this.htmlDownloadUrl = e.downloadUrl;
                    fetch(e.downloadUrl, {
                        headers: { Authorization: 'Bearer ' + this.authService.accessToken }
                    })
                        .then(r => r.ok ? r.text() : Promise.reject(r.statusText))
                        .then(html => {
                            this.generatedHtml = html;
                            this.splitPages(html);
                        })
                        .catch(err => {
                            console.error('HTML download error:', err);
                            this.generationError = 'Failed to download HTML manual.';
                        });
                } else {
                    this.generatedHtml = e.html;
                    this.splitPages(e.html!);
                }
                this.signalr.disconnect();
            }),
            this.signalr.onError$.subscribe((msg: string) => {
                // Don't show error if generation already completed successfully
                if (this.resultStats != null) return;
                this.isGenerating = false;
                this.generationError = msg;
                this.signalr.disconnect();
            }),

            //
            // Track SignalR connection state for reconnection feedback
            //
            this.signalr.onConnectionChange$.subscribe((connected: boolean) => {
                if (this.isGenerating == true) {
                    this.isReconnecting = (connected == false);
                }
            })
        );
    }


    ngOnDestroy(): void {
        this.subs.forEach(s => s.unsubscribe());
        this.signalr.disconnect();
    }


    // ═══════════════════════════════════════════════════════════════════
    //  File Upload
    // ═══════════════════════════════════════════════════════════════════

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = true;
    }

    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;

        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.selectFile(files[0]);
        }
    }

    //
    // Handle the native file input change event
    //
    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files != null && input.files.length > 0) {
            this.selectFile(input.files[0]);
        }
    }


    //
    // Validate file extension and reset state for a newly selected file
    //
    private selectFile(file: File): void {
        const ext = file.name.toLowerCase().split('.').pop();
        if (ext !== 'mpd' && ext !== 'ldr' && ext !== 'dat') {
            this.uploadError = 'Unsupported file type. Please upload .mpd, .ldr, or .dat files.';
            return;
        }

        //
        // Enforce maximum file size to prevent excessive server memory consumption
        //
        if (file.size > this.MAX_FILE_SIZE_BYTES) {
            const maxMb = Math.round(this.MAX_FILE_SIZE_BYTES / (1024 * 1024));
            this.uploadError = `File is too large. Maximum size is ${maxMb} MB.`;
            return;
        }

        this.selectedFile = file;
        this.uploadError = null;
        this.analysis = null;
        this.generatedHtml = null;
        this.resultStats = null;
        this.pages = [];
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Analyse
    // ═══════════════════════════════════════════════════════════════════

    //
    // Upload the selected file for step analysis (parts counts, cumulative totals)
    //
    analyseFile(): void {
        if (this.selectedFile == null) {
            return;
        }

        this.isAnalysing = true;
        this.uploadError = null;

        const formData = new FormData();
        formData.append('file', this.selectedFile);

        const headers = new HttpHeaders({
            'Authorization': `Bearer ${this.authService.accessToken}`
        });

        this.http.post<AnalyseResponse>('/api/manual-generator/analyse-upload', formData, { headers })
            .subscribe({
                next: (res) => {
                    this.analysis = res;
                    this.isAnalysing = false;
                },
                error: (err) => {
                    this.uploadError = err.error || 'Analysis failed.';
                    this.isAnalysing = false;
                }
            });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Generate Manual
    // ═══════════════════════════════════════════════════════════════════

    //
    // Upload the file, connect SignalR, and start manual generation.
    // Progress streams back via onStepProgress$ subscription.
    //
    async generateManual(): Promise<void> {
        if (this.selectedFile == null) {
            return;
        }

        this.isGenerating = true;
        this.generationProgress = 0;
        this.generationTotal = 0;
        this.currentPreview = null;
        this.generationError = null;
        this.generatedHtml = null;
        this.generatedPdf = null;
        this.resultStats = null;
        this.pages = [];

        // Step 1: Upload file to get generationId
        const formData = new FormData();
        formData.append('file', this.selectedFile);

        const headers = new HttpHeaders({
            'Authorization': `Bearer ${this.authService.accessToken}`
        });

        try {
            const response = await firstValueFrom(
                this.http.post<{ generationId: string }>('/api/manual-generator/generate-upload', formData, { headers })
            );

            if (response?.generationId == null) {
                this.generationError = 'Server did not return a generation ID.';
                this.isGenerating = false;
                return;
            }

            // Step 2: Connect SignalR and start generation
            await this.signalr.connect();
            await this.signalr.generateManual(response.generationId, this.options);

        } catch (err: any) {
            // Don't overwrite a successful completion — the invoke can fail
            // if the connection briefly dropped but the generation still completed.
            if (this.resultStats == null) {
                this.generationError = err?.error || 'Failed to start generation.';
                this.isGenerating = false;
            }
        }
    }


    //
    // Cancel the current generation by disconnecting SignalR.
    // The server detects the disconnection via Context.ConnectionAborted and stops rendering.
    //
    cancelGeneration(): void {
        this.isGenerating = false;
        this.isReconnecting = false;
        this.generationError = 'Generation cancelled.';
        this.signalr.disconnect();
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Preview & Download
    // ═══════════════════════════════════════════════════════════════════

    //
    // Parse the generated HTML into individual page chunks for the preview carousel.
    // Each page's HTML is trusted via DomSanitizer because it contains embedded base64
    // images that Angular's default sanitizer would strip.
    //
    private splitPages(html: string): void {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const pageElements = doc.querySelectorAll('.page');

        this.pages = [];
        pageElements.forEach(page => {
            this.pages.push(this.sanitizer.bypassSecurityTrustHtml(page.outerHTML));
        });

        this.currentPage = 0;
    }

    prevPage(): void {
        if (this.currentPage > 0) {
            this.currentPage--;
        }
    }


    nextPage(): void {
        if (this.currentPage < this.pages.length - 1) {
            this.currentPage++;
        }
    }


    //
    // Allow arrow key navigation through pages when the result preview is visible
    //
    @HostListener('document:keydown', ['$event'])
    onKeyDown(event: KeyboardEvent): void {
        if (this.pages.length === 0) {
            return;
        }

        if (event.key === 'ArrowLeft') {
            this.prevPage();
        }

        if (event.key === 'ArrowRight') {
            this.nextPage();
        }
    }

    //
    // Download the generated manual as a self-contained HTML file
    //
    downloadHtml(): void {
        const downloadName = (this.selectedFile?.name.replace(/\.[^.]+$/, '') || 'manual') + '_build-manual.html';

        // Use in-memory HTML first (already fetched for preview)
        if (this.generatedHtml) {
            const blob = new Blob([this.generatedHtml], { type: 'text/html' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = downloadName;
            link.click();
            URL.revokeObjectURL(url);
            return;
        }

        // Fallback: server-side download URL (if preview wasn't fetched yet)
        if (this.htmlDownloadUrl) {
            fetch(this.htmlDownloadUrl, {
                headers: { Authorization: 'Bearer ' + this.authService.accessToken }
            })
                .then(r => {
                    if (!r.ok) throw new Error('Download failed: ' + r.statusText);
                    return r.blob();
                })
                .then(blob => {
                    const url = URL.createObjectURL(blob);
                    const link = document.createElement('a');
                    link.href = url;
                    link.download = downloadName;
                    link.click();
                    URL.revokeObjectURL(url);
                })
                .catch(err => {
                    console.error('HTML download error:', err);
                    this.generationError = 'Failed to download HTML. Please try again.';
                });
        }
    }

    //
    // Download the generated manual as a PDF file
    //
    downloadPdf(): void {
        // Preferred path: server-side download URL (any size)
        if (this.pdfDownloadUrl) {
            const downloadName = (this.selectedFile?.name.replace(/\.[^.]+$/, '') || 'manual') + '_build-manual.pdf';
            fetch(this.pdfDownloadUrl, {
                headers: { Authorization: 'Bearer ' + this.authService.accessToken }
            })
                .then(r => {
                    if (!r.ok) throw new Error('Download failed: ' + r.statusText);
                    return r.blob();
                })
                .then(blob => {
                    const url = URL.createObjectURL(blob);
                    const link = document.createElement('a');
                    link.href = url;
                    link.download = downloadName;
                    link.click();
                    URL.revokeObjectURL(url);
                })
                .catch(err => {
                    console.error('PDF download error:', err);
                    this.generationError = 'Failed to download PDF. Please try again.';
                });
            return;
        }

        // Legacy fallback: base64 in-memory (small PDFs)
        if (this.generatedPdf == null) {
            return;
        }

        const byteChars = atob(this.generatedPdf);
        const byteArray = new Uint8Array(byteChars.length);
        for (let i = 0; i < byteChars.length; i++) {
            byteArray[i] = byteChars.charCodeAt(i);
        }

        const blob = new Blob([byteArray], { type: 'application/pdf' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = (this.selectedFile?.name.replace(/\.[^.]+$/, '') || 'manual') + '_build-manual.pdf';
        link.click();
        URL.revokeObjectURL(url);
    }

    //
    // Open the manual in a new window and trigger the browser print dialog
    //
    printManual(): void {
        if (this.generatedHtml == null) {
            return;
        }

        const printWindow = window.open('', '_blank');
        if (printWindow) {
            printWindow.document.write(this.generatedHtml);
            printWindow.document.close();
            printWindow.onload = () => {
                printWindow.print();
            };
        }
    }

    get progressPercent(): number {
        if (this.generationTotal === 0) return 0;
        return Math.round((this.generationProgress / this.generationTotal) * 100);
    }

    //
    // Reset all state to allow generating a new manual
    //
    reset(): void {
        this.selectedFile = null;
        this.analysis = null;
        this.generatedHtml = null;
        this.generatedPdf = null;
        this.pdfDownloadUrl = null;
        this.htmlDownloadUrl = null;
        this.resultStats = null;
        this.pages = [];
        this.currentPreview = null;
        this.generationError = null;
        this.uploadError = null;
    }
}

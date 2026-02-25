import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { ManualGeneratorSignalrService, StepProgressEvent, GenerationCompleteEvent, ManualOptionsDto } from '../../services/manual-generator-signalr.service';
import { AuthService } from '../../services/auth.service';

interface StepAnalysis {
    stepIndex: number;
    newParts: { fileName: string; colourCode: number; quantity: number }[];
    cumulativePartCount: number;
    cumulativeTriangleCount: number;
}

interface AnalyseResponse {
    fileName: string;
    stepCount: number;
    steps: StepAnalysis[];
    totalParts: number;
}


@Component({
    selector: 'app-manual-generator',
    templateUrl: './manual-generator.component.html',
    styleUrls: ['./manual-generator.component.scss']
})
export class ManualGeneratorComponent implements OnInit, OnDestroy {

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
        imageSize: 512,
        elevation: 30,
        azimuth: -45,
        renderEdges: true,
        smoothShading: true
    };

    // ─── Generation State ────────────────────────────────────────────────
    isGenerating = false;
    generationProgress = 0;
    generationTotal = 0;
    currentPreview: string | null = null;
    generationError: string | null = null;

    // ─── Result ──────────────────────────────────────────────────────────
    generatedHtml: string | null = null;
    resultStats: { totalSteps: number; totalParts: number; renderTimeMs: number } | null = null;

    // ─── Pagination ──────────────────────────────────────────────────────
    currentPage = 0;
    pages: string[] = [];

    private subs: Subscription[] = [];

    constructor(
        private http: HttpClient,
        private signalr: ManualGeneratorSignalrService,
        private authService: AuthService
    ) { }


    ngOnInit(): void {
        document.title = 'Manual Generator';

        // Subscribe to SignalR events
        this.subs.push(
            this.signalr.onStepProgress$.subscribe((e: StepProgressEvent) => {
                this.generationProgress = e.step;
                this.generationTotal = e.total;
                this.currentPreview = 'data:image/png;base64,' + e.previewBase64;
            }),
            this.signalr.onComplete$.subscribe((e: GenerationCompleteEvent) => {
                this.isGenerating = false;
                this.generatedHtml = e.html;
                this.resultStats = {
                    totalSteps: e.totalSteps,
                    totalParts: e.totalParts,
                    renderTimeMs: e.renderTimeMs
                };
                this.splitPages(e.html);
                this.signalr.disconnect();
            }),
            this.signalr.onError$.subscribe((msg: string) => {
                this.isGenerating = false;
                this.generationError = msg;
                this.signalr.disconnect();
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

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            this.selectFile(input.files[0]);
        }
    }

    private selectFile(file: File): void {
        const ext = file.name.toLowerCase().split('.').pop();
        if (ext !== 'mpd' && ext !== 'ldr' && ext !== 'dat') {
            this.uploadError = 'Unsupported file type. Please upload .mpd, .ldr, or .dat files.';
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

    analyseFile(): void {
        if (!this.selectedFile) return;

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

    async generateManual(): Promise<void> {
        if (!this.selectedFile) return;

        this.isGenerating = true;
        this.generationProgress = 0;
        this.generationTotal = 0;
        this.currentPreview = null;
        this.generationError = null;
        this.generatedHtml = null;
        this.resultStats = null;
        this.pages = [];

        // Step 1: Upload file to get generationId
        const formData = new FormData();
        formData.append('file', this.selectedFile);

        const headers = new HttpHeaders({
            'Authorization': `Bearer ${this.authService.accessToken}`
        });

        try {
            const response = await this.http
                .post<{ generationId: string }>('/api/manual-generator/generate-upload', formData, { headers })
                .toPromise();

            if (!response?.generationId) {
                this.generationError = 'Server did not return a generation ID.';
                this.isGenerating = false;
                return;
            }

            // Step 2: Connect SignalR and start generation
            await this.signalr.connect();
            await this.signalr.generateManual(response.generationId, this.options);

        } catch (err: any) {
            this.generationError = err?.error || 'Failed to start generation.';
            this.isGenerating = false;
        }
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Preview & Download
    // ═══════════════════════════════════════════════════════════════════

    private splitPages(html: string): void {
        // Split by page dividers in the HTML
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const pageElements = doc.querySelectorAll('.page');

        this.pages = [];
        pageElements.forEach(page => {
            this.pages.push(page.outerHTML);
        });

        this.currentPage = 0;
    }

    prevPage(): void {
        if (this.currentPage > 0) this.currentPage--;
    }

    nextPage(): void {
        if (this.currentPage < this.pages.length - 1) this.currentPage++;
    }

    downloadHtml(): void {
        if (!this.generatedHtml) return;

        const blob = new Blob([this.generatedHtml], { type: 'text/html' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = (this.selectedFile?.name.replace(/\.[^.]+$/, '') || 'manual') + '_build-manual.html';
        link.click();
        URL.revokeObjectURL(url);
    }

    printManual(): void {
        if (!this.generatedHtml) return;

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

    reset(): void {
        this.selectedFile = null;
        this.analysis = null;
        this.generatedHtml = null;
        this.resultStats = null;
        this.pages = [];
        this.currentPreview = null;
        this.generationError = null;
        this.uploadError = null;
    }
}

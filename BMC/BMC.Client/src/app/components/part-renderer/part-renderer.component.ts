import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil, switchMap } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';


interface PartResult {
    id: number;
    name: string;
    ldrawPartId: string;
    ldrawTitle: string;
    ldrawCategory: string;
}

interface PartColour {
    ldrawColourCode: number;
    name: string;
    hexRgb: string;
    isTransparent: boolean;
}


@Component({
    selector: 'app-part-renderer',
    templateUrl: './part-renderer.component.html',
    styleUrl: './part-renderer.component.scss'
})
export class PartRendererComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    // Search
    searchTerm = '';
    searchResults: PartResult[] = [];
    searching = false;
    showDropdown = false;

    // Selected part
    selectedPart: PartResult | null = null;

    // Colour modes — mirrors catalog-part-detail's dual-mode pattern
    colourMode: 'part' | 'all' = 'part';
    colours: PartColour[] = [];          // Part-specific colours (from BrickPartColours + LegoSetParts)
    allColours: PartColour[] = [];       // Every colour in the database
    allColoursLoaded = false;
    loadingColours = false;
    loadingAllColours = false;
    colourSearchTerm = '';

    // Render config
    selectedColourCode = 4;    // red default
    selectedColourHex = '#C91A09';
    renderWidth = 512;
    renderHeight = 512;
    selectedElevation = 30;
    selectedAzimuth = -45;
    flipView = false;

    // Advanced rendering options
    showAdvanced = false;
    renderEdges = true;
    smoothShading = true;
    antiAliasMode: 'none' | '2x' | '4x' = 'none';
    outputFormat: 'png' | 'webp' | 'svg' | 'gif' = 'png';
    backgroundHex = '';
    gradientTopHex = '';
    gradientBottomHex = '';
    explodedView = false;
    explosionFactor = 1.0;

    // Render state
    renderedImageUrl: SafeUrl | null = null;
    private renderedBlobUrl: string | null = null;
    renderedFormat = 'png';  // tracks actual format of last render
    rendering = false;
    renderError = '';
    renderTimeMs = 0;

    // Tab mode
    activeTab: 'search' | 'upload' = 'search';

    // Upload state
    uploadedFile: File | null = null;
    uploadedFileName = '';
    isDragOver = false;
    readonly acceptedExtensions = ['.dat', '.ldr', '.mpd'];

    // Size presets — categorised
    sizeCategory: 'standard' | 'desktop' | 'mobile' = 'standard';

    sizeCategories = [
        { key: 'standard' as const, label: 'Standard', icon: 'fa-th-large' },
        { key: 'desktop' as const, label: 'Desktop', icon: 'fa-desktop' },
        { key: 'mobile' as const, label: 'Mobile', icon: 'fa-mobile-alt' },
    ];

    sizePresets: { label: string; w: number; h: number; category: string }[] = [
        // Standard (square)
        { label: '256²', w: 256, h: 256, category: 'standard' },
        { label: '512²', w: 512, h: 512, category: 'standard' },
        { label: '768²', w: 768, h: 768, category: 'standard' },
        { label: '1024²', w: 1024, h: 1024, category: 'standard' },
        // Desktop wallpaper
        { label: 'HD', w: 1920, h: 1080, category: 'desktop' },
        { label: '2K', w: 2560, h: 1440, category: 'desktop' },
        { label: '4K', w: 3840, h: 2160, category: 'desktop' },
        { label: 'Ultrawide', w: 3440, h: 1440, category: 'desktop' },
        // Mobile / tablet
        { label: 'Phone', w: 1080, h: 1920, category: 'mobile' },
        { label: 'Phone+', w: 1284, h: 2778, category: 'mobile' },
        { label: 'Tablet', w: 2048, h: 2732, category: 'mobile' },
        { label: 'Square', w: 1080, h: 1080, category: 'mobile' },
    ];

    get activeSizePresets() {
        return this.sizePresets.filter(p => p.category === this.sizeCategory);
    }

    // View angle presets
    anglePresets = [
        { label: 'Standard', icon: 'fa-cube', elevation: 30, azimuth: -45 },
        { label: 'Front', icon: 'fa-square', elevation: 0, azimuth: 0 },
        { label: 'Top', icon: 'fa-arrow-down', elevation: 90, azimuth: 0 },
        { label: 'Side', icon: 'fa-arrow-right', elevation: 0, azimuth: -90 },
        { label: '3/4 High', icon: 'fa-mountain-sun', elevation: 45, azimuth: -45 },
        { label: '3/4 Low', icon: 'fa-road', elevation: 15, azimuth: -45 },
    ];

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private sanitizer: DomSanitizer
    ) { }


    ngOnInit(): void {
        this.searchSubject.pipe(
            debounceTime(300),
            takeUntil(this.destroy$),
            switchMap(term => {
                if (!term || term.length < 2) {
                    this.searchResults = [];
                    this.showDropdown = false;
                    this.searching = false;
                    return [];
                }
                this.searching = true;
                const headers = this.authService.GetAuthenticationHeaders();
                return this.http.get<PartResult[]>(`/api/part-renderer/search?q=${encodeURIComponent(term)}&take=20`, { headers });
            })
        ).subscribe({
            next: (results) => {
                this.searchResults = results || [];
                this.showDropdown = this.searchResults.length > 0;
                this.searching = false;
            },
            error: () => {
                this.searchResults = [];
                this.searching = false;
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.revokeBlob();
    }


    // ── Search ──

    onSearchInput(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchTerm = term;
        this.searchSubject.next(term);
    }

    onSearchFocus(): void {
        if (this.searchResults.length > 0) {
            this.showDropdown = true;
        }
    }

    onSearchBlur(): void {
        // Delay to allow click on dropdown items
        setTimeout(() => this.showDropdown = false, 200);
    }


    // ── Part Selection ──

    selectPart(part: PartResult): void {
        this.selectedPart = part;
        this.searchTerm = part.ldrawTitle || part.name;
        this.showDropdown = false;
        this.renderedImageUrl = null;
        this.revokeBlob();
        this.renderError = '';
        this.colourMode = 'part';
        this.colourSearchTerm = '';
        this.loadColours(part.name);
    }

    clearSelection(): void {
        this.selectedPart = null;
        this.searchTerm = '';
        this.searchResults = [];
        this.colours = [];
        this.colourMode = 'part';
        this.colourSearchTerm = '';
        this.renderedImageUrl = null;
        this.revokeBlob();
        this.renderError = '';
    }


    // ── Colour Mode Toggle ──

    setColourMode(mode: 'part' | 'all'): void {
        this.colourMode = mode;
        this.colourSearchTerm = '';

        // Fetch all colours the first time the user switches to 'all' mode
        if (mode === 'all' && !this.allColoursLoaded) {
            this.loadAllColours();
        }
    }


    // ── Colours ──

    private loadColours(partNumber: string): void {
        this.loadingColours = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.get<PartColour[]>(`/api/part-renderer/colours/${encodeURIComponent(partNumber)}`, { headers })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (colours) => {
                    this.colours = colours || [];
                    this.loadingColours = false;
                    // Auto-select first colour if available
                    if (this.colours.length > 0) {
                        this.selectColour(this.colours[0]);
                    }
                },
                error: () => {
                    this.colours = [];
                    this.loadingColours = false;
                }
            });
    }

    private loadAllColours(): void {
        this.loadingAllColours = true;
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.get<PartColour[]>('/api/part-renderer/all-colours', { headers })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (colours) => {
                    this.allColours = colours || [];
                    this.allColoursLoaded = true;
                    this.loadingAllColours = false;
                },
                error: () => {
                    this.allColours = [];
                    this.loadingAllColours = false;
                }
            });
    }

    selectColour(colour: PartColour): void {
        this.selectedColourCode = colour.ldrawColourCode;
        this.selectedColourHex = this.normalizeHex(colour.hexRgb) || '#C91A09';
    }

    isSelectedColour(colour: PartColour): boolean {
        return this.selectedColourCode === colour.ldrawColourCode;
    }

    /** Returns colours filtered by the search term for the active mode */
    get filteredColours(): PartColour[] {
        const source = this.colourMode === 'all' ? this.allColours : this.colours;
        if (!this.colourSearchTerm) return source;
        const term = this.colourSearchTerm.toLowerCase();
        return source.filter(c =>
            (c.name && c.name.toLowerCase().includes(term)) ||
            String(c.ldrawColourCode).includes(term)
        );
    }

    /** Number of visible colours after search filter */
    get visibleColourCount(): number {
        return this.filteredColours.length;
    }


    // ── Size ──

    selectSize(preset: { w: number; h: number }): void {
        this.renderWidth = preset.w;
        this.renderHeight = preset.h;
    }

    isSelectedSize(preset: { w: number; h: number }): boolean {
        return this.renderWidth === preset.w && this.renderHeight === preset.h;
    }


    // ── View Angle ──

    selectAngle(preset: { elevation: number; azimuth: number }): void {
        this.selectedElevation = preset.elevation;
        this.selectedAzimuth = preset.azimuth;
    }

    isSelectedAngle(preset: { elevation: number; azimuth: number }): boolean {
        return this.selectedElevation === preset.elevation && this.selectedAzimuth === preset.azimuth;
    }


    // ── Render ──

    render(): void {
        if (!this.selectedPart || this.rendering) return;

        // Turntable GIF uses a separate endpoint
        if (this.outputFormat === 'gif') {
            this.renderTurntable();
            return;
        }

        this.rendering = true;
        this.renderError = '';
        this.revokeBlob();

        const headers = this.authService.GetAuthenticationHeaders();
        const partNumber = this.selectedPart.name;
        const effectiveAzimuth = this.flipView ? this.selectedAzimuth + 180 : this.selectedAzimuth;

        let url: string;

        if (this.explodedView) {
            // Exploded view endpoint
            url = `/api/part-renderer/exploded?partNumber=${encodeURIComponent(partNumber)}&colourCode=${this.selectedColourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.selectedElevation}&azimuth=${effectiveAzimuth}&explosionFactor=${this.explosionFactor}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}`;
        } else {
            // Standard render endpoint with all options
            url = `/api/part-renderer/render?partNumber=${encodeURIComponent(partNumber)}&colourCode=${this.selectedColourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.selectedElevation}&azimuth=${effectiveAzimuth}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}&antiAlias=${this.antiAliasMode}&format=${this.outputFormat}`;

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

    renderTurntable(): void {
        if (!this.selectedPart || this.rendering) return;

        this.rendering = true;
        this.renderError = '';
        this.revokeBlob();

        const headers = this.authService.GetAuthenticationHeaders();
        const partNumber = this.selectedPart.name;
        const url = `/api/part-renderer/turntable?partNumber=${encodeURIComponent(partNumber)}&colourCode=${this.selectedColourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.selectedElevation}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}`;

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


    // ── Download ──

    download(): void {
        if (!this.renderedBlobUrl) return;
        if (!this.selectedPart && !this.uploadedFile) return;

        const ext = this.renderedFormat === 'gif' ? 'gif' : this.renderedFormat === 'webp' ? 'webp' : this.renderedFormat === 'svg' ? 'svg' : 'png';
        const baseName = this.selectedPart
            ? `${this.selectedPart.name}_c${this.selectedColourCode}_${this.renderWidth}x${this.renderHeight}`
            : `${this.uploadedFileName.replace(/\.[^.]+$/, '')}_c${this.selectedColourCode}_${this.renderWidth}x${this.renderHeight}`;
        const a = document.createElement('a');
        a.href = this.renderedBlobUrl;
        a.download = `${baseName}.${ext}`;
        a.click();
    }


    // ── Upload + Render ──

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            this.setUploadedFile(input.files[0]);
        }
    }

    onFileDrop(event: DragEvent): void {
        event.preventDefault();
        this.isDragOver = false;
        if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
            this.setUploadedFile(event.dataTransfer.files[0]);
        }
    }

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        this.isDragOver = true;
    }

    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        this.isDragOver = false;
    }

    private setUploadedFile(file: File): void {
        const ext = '.' + file.name.split('.').pop()?.toLowerCase();
        if (!this.acceptedExtensions.includes(ext)) {
            this.renderError = `Unsupported file type. Accepted: ${this.acceptedExtensions.join(', ')}`;
            return;
        }
        if (file.size > 5 * 1024 * 1024) {
            this.renderError = 'File exceeds the 5 MB size limit.';
            return;
        }
        this.uploadedFile = file;
        this.uploadedFileName = file.name;
        this.renderError = '';
    }

    removeUploadedFile(): void {
        this.uploadedFile = null;
        this.uploadedFileName = '';
    }

    renderUploadedFile(): void {
        if (!this.uploadedFile || this.rendering) return;

        this.rendering = true;
        this.renderError = '';
        this.revokeBlob();

        // Must NOT set Content-Type — the browser auto-sets it to
        // multipart/form-data with the correct boundary for FormData.
        // GetAuthenticationHeaders() forces Content-Type: application/json which
        // prevents ASP.NET Core's IFormFile binder from reading the form body.
        const headers = new HttpHeaders({
            Authorization: `Bearer ${this.authService.accessToken}`
        });
        const effectiveAzimuth = this.flipView ? this.selectedAzimuth + 180 : this.selectedAzimuth;

        const formData = new FormData();
        formData.append('file', this.uploadedFile, this.uploadedFile.name);

        // Build query string with render params
        let url = `/api/part-renderer/render-upload?colourCode=${this.selectedColourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.selectedElevation}&azimuth=${effectiveAzimuth}&renderEdges=${this.renderEdges}&smoothShading=${this.smoothShading}&antiAlias=${this.antiAliasMode}&format=${this.outputFormat}`;

        if (this.backgroundHex) {
            url += `&backgroundHex=${encodeURIComponent(this.backgroundHex)}`;
        }
        if (this.gradientTopHex && this.gradientBottomHex) {
            url += `&gradientTopHex=${encodeURIComponent(this.gradientTopHex)}&gradientBottomHex=${encodeURIComponent(this.gradientBottomHex)}`;
        }

        const startTime = performance.now();

        this.http.post(url, formData, { headers, responseType: 'blob' })
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
                    this.renderError = err.status === 400 ? (err.error?.text || 'Invalid file.') : 'Upload render failed. Please try again.';
                    this.rendering = false;
                }
            });
    }

    // ── Helpers ──

    getSelectedColourName(): string {
        // Check both sources for the selected colour name
        const partMatch = this.colours.find(c => c.ldrawColourCode === this.selectedColourCode);
        if (partMatch) return partMatch.name;
        const allMatch = this.allColours.find(c => c.ldrawColourCode === this.selectedColourCode);
        return allMatch ? allMatch.name : `Colour ${this.selectedColourCode}`;
    }


    // ── Cleanup ──

    private revokeBlob(): void {
        if (this.renderedBlobUrl) {
            URL.revokeObjectURL(this.renderedBlobUrl);
            this.renderedBlobUrl = null;
        }
    }


    /** Normalize a hex value — the DB may store with or without '#' prefix */
    private normalizeHex(hex: string | null | undefined): string | null {
        if (!hex) return null;
        return hex.startsWith('#') ? hex : '#' + hex;
    }

    /** Safe background colour for a swatch — handles null/missing hex gracefully */
    getSwatchBg(colour: PartColour): string {
        return this.normalizeHex(colour.hexRgb) || '#888888';
    }
}

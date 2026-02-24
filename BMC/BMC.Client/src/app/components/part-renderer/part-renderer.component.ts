import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

    // Render state
    renderedImageUrl: SafeUrl | null = null;
    private renderedBlobUrl: string | null = null;
    rendering = false;
    renderError = '';
    renderTimeMs = 0;

    // Size presets
    sizePresets = [
        { label: '256', w: 256, h: 256 },
        { label: '512', w: 512, h: 512 },
        { label: '768', w: 768, h: 768 },
        { label: '1024', w: 1024, h: 1024 },
    ];

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

        this.rendering = true;
        this.renderError = '';
        this.revokeBlob();

        const headers = this.authService.GetAuthenticationHeaders();
        const partNumber = this.selectedPart.name;
        const effectiveAzimuth = this.flipView ? this.selectedAzimuth + 180 : this.selectedAzimuth;
        const url = `/api/part-renderer/render?partNumber=${encodeURIComponent(partNumber)}&colourCode=${this.selectedColourCode}&width=${this.renderWidth}&height=${this.renderHeight}&elevation=${this.selectedElevation}&azimuth=${effectiveAzimuth}`;

        const startTime = performance.now();

        this.http.get(url, { headers, responseType: 'blob' })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (blob) => {
                    this.renderTimeMs = Math.round(performance.now() - startTime);
                    this.renderedBlobUrl = URL.createObjectURL(blob);
                    this.renderedImageUrl = this.sanitizer.bypassSecurityTrustUrl(this.renderedBlobUrl);
                    this.rendering = false;
                },
                error: (err) => {
                    this.renderTimeMs = 0;
                    this.renderError = err.status === 404 ? 'Part geometry file not found.' : 'Render failed. Please try again.';
                    this.rendering = false;
                }
            });
    }


    // ── Download ──

    download(): void {
        if (!this.renderedBlobUrl || !this.selectedPart) return;

        const a = document.createElement('a');
        a.href = this.renderedBlobUrl;
        a.download = `${this.selectedPart.name}_c${this.selectedColourCode}_${this.renderWidth}x${this.renderHeight}.png`;
        a.click();
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

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BrickColourService, BrickColourData } from '../../bmc-data-services/brick-colour.service';
import { ColourFinishService, ColourFinishData } from '../../bmc-data-services/colour-finish.service';
import { AuthService } from '../../services/auth.service';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-colour-library',
    templateUrl: './colour-library.component.html',
    styleUrl: './colour-library.component.scss'
})
export class ColourLibraryComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    allColours: BrickColourData[] = [];
    filteredColours: BrickColourData[] = [];
    finishes: ColourFinishData[] = [];

    loading = true;
    searchTerm = '';
    selectedFinishId: number | bigint | null = null;
    showTransparentOnly = false;
    showMetallicOnly = false;
    selectedColour: BrickColourData | null = null;

    // Stats
    totalCount = 0;
    transparentCount = 0;
    metallicCount = 0;

    constructor(
        private colourService: BrickColourService,
        private finishService: ColourFinishService,
        private authService: AuthService,
        private http: HttpClient,
        private router: Router
    ) { }

    ngOnInit(): void {
        // Debounced search
        this.searchSubject.pipe(
            debounceTime(200),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.applyFilters();
        });

        this.loadData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadData(): void {
        this.loading = true;

        if (this.authService.isLoggedIn) {
            // Authenticated path — use generated data services
            this.finishService.GetColourFinishList({ active: true, deleted: false }).pipe(
                takeUntil(this.destroy$)
            ).subscribe({
                next: (finishes) => this.finishes = finishes,
                error: () => this.finishes = []
            });

            this.colourService.GetBrickColourList({ active: true, deleted: false, includeRelations: true }).pipe(
                takeUntil(this.destroy$)
            ).subscribe({
                next: (colours) => {
                    this.allColours = colours;
                    this.totalCount = colours.length;
                    this.transparentCount = colours.filter(c => c.isTransparent).length;
                    this.metallicCount = colours.filter(c => c.isMetallic).length;
                    this.applyFilters();
                    this.loading = false;
                },
                error: () => {
                    this.allColours = [];
                    this.filteredColours = [];
                    this.loading = false;
                }
            });
        } else {
            // Anonymous path — use public browse endpoint
            this.http.get<any[]>('/api/public/browse/colours').pipe(
                takeUntil(this.destroy$)
            ).subscribe({
                next: (colours) => {
                    // Map public DTOs to BrickColourData shape
                    this.allColours = colours.map((c: any) => ({
                        id: c.id,
                        name: c.name,
                        hexRgb: c.hexRgb,
                        hexEdgeColour: c.hexEdgeColour,
                        ldrawColourCode: c.ldrawColourCode,
                        isTransparent: c.isTransparent,
                        isMetallic: false,
                        alpha: c.isTransparent ? 128 : 255,
                        colourFinish: c.finishName ? { name: c.finishName } : null,
                        colourFinishId: null,
                    })) as any;
                    this.totalCount = this.allColours.length;
                    this.transparentCount = this.allColours.filter(c => c.isTransparent).length;
                    this.metallicCount = 0;

                    // Extract unique finishes from colour data
                    const finishNames = new Set(colours.map((c: any) => c.finishName).filter(Boolean));
                    this.finishes = Array.from(finishNames).map((name, idx) => ({
                        id: idx + 1,
                        name,
                    })) as any;

                    this.applyFilters();
                    this.loading = false;
                },
                error: () => {
                    this.allColours = [];
                    this.filteredColours = [];
                    this.loading = false;
                }
            });
        }
    }

    onSearch(event: Event): void {
        const term = (event.target as HTMLInputElement).value;
        this.searchSubject.next(term);
    }

    applyFilters(): void {
        let result = [...this.allColours];

        // Text search
        if (this.searchTerm) {
            const lower = this.searchTerm.toLowerCase();
            result = result.filter(c =>
                c.name?.toLowerCase().includes(lower) ||
                c.hexRgb?.toLowerCase().includes(lower) ||
                c.ldrawColourCode?.toString().includes(lower)
            );
        }

        // Finish filter
        if (this.selectedFinishId !== null) {
            result = result.filter(c => c.colourFinishId == this.selectedFinishId);
        }

        // Transparent filter
        if (this.showTransparentOnly) {
            result = result.filter(c => c.isTransparent);
        }

        // Metallic filter
        if (this.showMetallicOnly) {
            result = result.filter(c => c.isMetallic);
        }

        this.filteredColours = result;
    }

    toggleTransparent(): void {
        this.showTransparentOnly = !this.showTransparentOnly;
        if (this.showTransparentOnly) this.showMetallicOnly = false;
        this.applyFilters();
    }

    toggleMetallic(): void {
        this.showMetallicOnly = !this.showMetallicOnly;
        if (this.showMetallicOnly) this.showTransparentOnly = false;
        this.applyFilters();
    }

    selectFinish(finishId: number | bigint | null): void {
        this.selectedFinishId = this.selectedFinishId === finishId ? null : finishId;
        this.applyFilters();
    }

    selectColour(colour: BrickColourData): void {
        this.selectedColour = this.selectedColour?.id === colour.id ? null : colour;
    }

    navigateToDetail(colour: BrickColourData): void {
        this.router.navigate(['/brickcolours', colour.id]);
    }

    viewPartsInColour(colour: BrickColourData): void {
        this.router.navigate(['/lego/parts'], { queryParams: { colourId: colour.id } });
    }

    /** Normalize a hex value — the DB may store with or without '#' prefix */
    private normalizeHex(hex: string | null | undefined): string | null {
        if (!hex) return null;
        return hex.startsWith('#') ? hex : '#' + hex;
    }

    /** Strip leading '#' for display purposes */
    getHexDisplay(hex: string | null | undefined): string {
        if (!hex) return '------';
        return hex.startsWith('#') ? hex.substring(1) : hex;
    }

    getSwatchBg(colour: BrickColourData): string {
        return this.normalizeHex(colour.hexRgb) || '#888888';
    }

    getSwatchBorder(colour: BrickColourData): string {
        return this.normalizeHex(colour.hexEdgeColour) || 'rgba(255,255,255,0.1)';
    }

    getSwatchGlow(colour: BrickColourData): string {
        const hex = this.normalizeHex(colour.hexRgb);
        if (!hex) return 'none';
        return `0 0 20px ${hex}40, 0 0 40px ${hex}20`;
    }

    getAlphaPercent(colour: BrickColourData): number {
        if (colour.alpha == null) return 100;
        return Math.round((Number(colour.alpha) / 255) * 100);
    }

    getFinishName(colour: BrickColourData): string {
        if (colour.colourFinish) return colour.colourFinish.name || 'Standard';
        return 'Standard';
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.selectedFinishId = null;
        this.showTransparentOnly = false;
        this.showMetallicOnly = false;
        this.selectedColour = null;
        this.applyFilters();
    }

    hasActiveFilters(): boolean {
        return !!this.searchTerm || this.selectedFinishId !== null ||
            this.showTransparentOnly || this.showMetallicOnly;
    }
}

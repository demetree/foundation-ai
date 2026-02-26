import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { trigger, transition, style, animate, stagger, query } from '@angular/animations';

import { PublicShowcaseService } from '../../services/public-showcase.service';
import { ThemeService, ThemeDefinition } from '../../services/theme.service';


interface FeaturedSet {
    id: number;
    name: string;
    setNumber: string;
    year: number;
    partCount: number;
    imageUrl: string | null;
    themeName: string | null;
}

interface DecadeBucket {
    label: string;
    startYear: number;
    endYear: number;
    setCount: number;
    themeCount: number;
}


@Component({
    selector: 'app-public-landing',
    templateUrl: './public-landing.component.html',
    styleUrl: './public-landing.component.scss',
    animations: [
        trigger('fadeInUp', [
            transition(':enter', [
                style({ opacity: 0, transform: 'translateY(30px)' }),
                animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
            ])
        ]),
        trigger('staggerIn', [
            transition(':enter', [
                query(':enter', [
                    style({ opacity: 0, transform: 'translateY(20px)' }),
                    stagger(80, [
                        animate('400ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
                    ])
                ], { optional: true })
            ])
        ])
    ]
})
export class PublicLandingComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // State
    loading = true;
    error = false;

    // Stats
    displaySets = 0;
    displayParts = 0;
    displayThemes = 0;
    targetSets = 0;
    targetParts = 0;
    targetThemes = 0;

    // Content
    featuredSets: FeaturedSet[] = [];
    recentSets: FeaturedSet[] = [];
    randomSets: FeaturedSet[] = [];
    decades: DecadeBucket[] = [];

    // Hero tagline rotation
    taglines = [
        'Design, simulate, and build with virtual bricks.',
        'Explore 79,000+ unique parts in stunning detail.',
        'Browse the entire LEGO catalogue from 1949 to today.',
        'Generate build manuals for your custom creations.',
        'The ultimate platform for LEGO enthusiasts.'
    ];
    activeTaglineIndex = 0;
    taglineVisible = true;
    private taglineInterval: any;

    // Feature cards
    features = [
        {
            icon: 'fas fa-cube',
            title: 'Parts Catalog',
            description: 'Browse 79,000+ unique brick parts with detailed specifications, colours, and categories.',
            gradient: 'gradient-red'
        },
        {
            icon: 'fas fa-paint-brush',
            title: 'Part Renderer',
            description: 'Render any part in any colour with our software and ray-trace engines. Studio-quality images.',
            gradient: 'gradient-blue'
        },
        {
            icon: 'fas fa-book-open',
            title: 'Manual Generator',
            description: 'Generate step-by-step build instructions for your custom MOC designs.',
            gradient: 'gradient-green'
        },
        {
            icon: 'fas fa-box-open',
            title: 'Set Explorer',
            description: 'Every LEGO set ever made. Search, filter, and discover across decades of bricks.',
            gradient: 'gradient-orange'
        },
        {
            icon: 'fas fa-users',
            title: 'Community',
            description: 'Share your builds, earn achievements, and connect with fellow LEGO enthusiasts.',
            gradient: 'gradient-purple'
        },
        {
            icon: 'fas fa-robot',
            title: 'AI Assistant',
            description: 'Get intelligent help with part identification, build suggestions, and more.',
            gradient: 'gradient-teal'
        }
    ];


    // Theme system
    availableThemes: ThemeDefinition[] = [];
    activeThemeId = '';


    constructor(
        public router: Router,
        private showcaseService: PublicShowcaseService,
        private themeService: ThemeService
    ) {
        this.availableThemes = this.themeService.availableThemes;
    }


    ngOnInit(): void {
        this.loadData();
        this.startTaglineRotation();

        this.themeService.currentTheme$
            .pipe(takeUntil(this.destroy$))
            .subscribe(id => this.activeThemeId = id);
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        if (this.taglineInterval) {
            clearInterval(this.taglineInterval);
        }
    }


    private loadData(): void {
        forkJoin({
            stats: this.showcaseService.getStats(),
            featured: this.showcaseService.getFeaturedSets(),
            recent: this.showcaseService.getRecentSets(),
            decades: this.showcaseService.getDecades(),
            random: this.showcaseService.getRandomDiscovery()
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this.targetSets = result.stats.totalSets;
                    this.targetParts = result.stats.totalParts;
                    this.targetThemes = result.stats.totalThemes;

                    this.featuredSets = result.featured;
                    this.recentSets = result.recent;
                    this.decades = result.decades;
                    this.randomSets = result.random;

                    this.loading = false;

                    // Animate counters
                    setTimeout(() => {
                        this.animateCounter('displaySets', this.targetSets, 1500);
                        this.animateCounter('displayParts', this.targetParts, 2000);
                        this.animateCounter('displayThemes', this.targetThemes, 1200);
                    }, 200);
                },
                error: () => {
                    this.loading = false;
                    this.error = true;
                }
            });
    }


    private animateCounter(prop: 'displaySets' | 'displayParts' | 'displayThemes', target: number, duration: number): void {
        const start = performance.now();
        const step = (timestamp: number) => {
            const progress = Math.min((timestamp - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3); // ease-out cubic
            (this as any)[prop] = Math.round(eased * target);
            if (progress < 1) {
                requestAnimationFrame(step);
            }
        };
        requestAnimationFrame(step);
    }


    private startTaglineRotation(): void {
        this.taglineInterval = setInterval(() => {
            this.taglineVisible = false;
            setTimeout(() => {
                this.activeTaglineIndex = (this.activeTaglineIndex + 1) % this.taglines.length;
                this.taglineVisible = true;
            }, 400);
        }, 4000);
    }


    shuffleDiscovery(): void {
        this.showcaseService.getRandomDiscovery()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (sets) => this.randomSets = sets,
                error: () => { /* silently fail */ }
            });
    }


    setTheme(themeId: string): void {
        this.themeService.setTheme(themeId);
    }


    goToLogin(): void {
        this.router.navigate(['/login']);
    }
}

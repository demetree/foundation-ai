///
/// AI-Generated: Welcome component — post-login landing page.
///
/// Presents two persona pathways to guide new users:
///   - Casual Explorer:  browse sets, themes, minifigs — navigates to /lego
///   - LEGO Designer:    render, build, manage collections — navigates to /dashboard
///
/// Also shows a quick-feature strip and live database stats so users can
/// orient themselves before diving in.
///
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, of } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';
import { trigger, transition, style, animate } from '@angular/animations';

import { AuthService } from '../../services/auth.service';
import { AuthNudgeService } from '../../services/auth-nudge.service';


//
// Pathway card definition — one per persona
//
interface PathwayCard {
    id: string;
    icon: string;
    title: string;
    subtitle: string;
    description: string;
    features: { name: string; route: string }[];
    ctaLabel: string;
    route: string;
    gradient: string;
    accentColor: string;
}


//
// Feature pill shown in the quick-access strip below the persona cards
//
interface FeatureLink {
    icon: string;
    title: string;
    description: string;
    route: string;
    gradient: string;
}


//
// Live stat counter shown at the bottom
//
interface LiveStat {
    icon: string;
    label: string;
    value: number;
    loaded: boolean;
}


@Component({
    selector: 'app-welcome',
    templateUrl: './welcome.component.html',
    styleUrl: './welcome.component.scss',
    animations: [
        trigger('fadeInUp', [
            transition(':enter', [
                style({ opacity: 0, transform: 'translateY(30px)' }),
                animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
            ])
        ])
    ]
})
export class WelcomeComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    //
    // Personalized greeting
    //
    displayName = '';
    greeting = '';

    //
    // Rotating taglines
    //
    taglines = [
        'Your LEGO universe awaits.',
        'Explore, design, and build — all in one place.',
        'From casual browsing to custom creations.',
        'The ultimate platform for LEGO enthusiasts.'
    ];
    activeTaglineIndex = 0;
    taglineVisible = true;
    private taglineInterval: any;


    //
    // Two main persona pathway cards
    //
    pathways: PathwayCard[] = [
        {
            id: 'explorer',
            icon: 'fas fa-globe',
            title: 'Casual Explorer',
            subtitle: 'Discover the LEGO Universe',
            description: 'Browse thousands of sets, themes, and minifigures. Explore by decade, discover fun facts, and get lost in the world of LEGO.',
            features: [
                { name: 'Sets', route: '/lego/sets' },
                { name: 'Themes', route: '/lego/themes' },
                { name: 'Minifigures', route: '/lego/minifigs' },
                { name: 'Colour Library', route: '/colours' },
                { name: 'Set Comparison', route: '/lego/compare' },
                { name: 'AI Search', route: '/ai' }
            ],
            ctaLabel: 'Start Exploring',
            route: '/lego',
            gradient: 'explorer-gradient',
            accentColor: '#3b82f6'
        },
        {
            id: 'designer',
            icon: 'fas fa-drafting-compass',
            title: 'LEGO Designer',
            subtitle: 'Build, Render & Manage',
            description: 'Render parts in any colour with ray tracing, generate step-by-step build manuals, manage your collection, and create custom projects.',
            features: [
                { name: 'Part Renderer', route: '/part-renderer' },
                { name: 'Manual Builder', route: '/manual-generator' },
                { name: 'My Collection', route: '/my-collection' },
                { name: 'Projects', route: '/dashboard' },
                { name: 'Parts Catalog', route: '/parts' }
            ],
            ctaLabel: 'Start Building',
            route: '/dashboard',
            gradient: 'designer-gradient',
            accentColor: '#8b5cf6'
        },
        {
            id: 'investor',
            icon: 'fas fa-chart-line',
            title: 'Brickberg Terminal',
            subtitle: 'LEGO Financial Intelligence',
            description: 'Track your portfolio value, monitor market trends, compare investments, and get real-time pricing across BrickLink, BrickEconomy, and Brick Owl.',
            features: [
                { name: 'Portfolio', route: '/brickberg' },
                { name: 'Market Movers', route: '/brickberg' },
                { name: 'Quick Lookup', route: '/brickberg' },
                { name: 'Integration Health', route: '/brickberg' }
            ],
            ctaLabel: 'Open Terminal',
            route: '/brickberg',
            gradient: 'brickberg-gradient',
            accentColor: '#00B894'
        }
    ];


    //
    // Quick feature links — everything available in one strip
    //
    featureLinks: FeatureLink[] = [
        { icon: 'fas fa-globe', title: 'Universe', description: 'Explore the LEGO universe', route: '/lego', gradient: 'gradient-blue' },
        { icon: 'fas fa-box-open', title: 'Set Explorer', description: 'Search and filter sets', route: '/lego/sets', gradient: 'gradient-orange' },
        { icon: 'fas fa-child-reaching', title: 'Minifig Gallery', description: 'Browse all minifigs', route: '/lego/minifigs', gradient: 'gradient-pink' },
        { icon: 'fas fa-layer-group', title: 'Themes', description: 'Browse by theme', route: '/lego/themes', gradient: 'gradient-teal' },
        { icon: 'fas fa-cubes', title: 'Parts Catalog', description: '79,000+ parts', route: '/parts', gradient: 'gradient-red' },
        { icon: 'fas fa-camera', title: 'Part Renderer', description: '3D rendering', route: '/part-renderer', gradient: 'gradient-purple' },
        { icon: 'fas fa-book', title: 'Manual Builder', description: 'Build instructions', route: '/manual-generator', gradient: 'gradient-green' },
        { icon: 'fas fa-layer-group', title: 'My Collection', description: 'Track your sets', route: '/my-collection', gradient: 'gradient-amber' },
        { icon: 'fas fa-exchange-alt', title: 'Compare Sets', description: 'Side by side', route: '/lego/compare', gradient: 'gradient-indigo' },
        { icon: 'fas fa-palette', title: 'Colours', description: 'Brick colours', route: '/colours', gradient: 'gradient-rainbow' },
        { icon: 'fas fa-robot', title: 'AI Assistant', description: 'Intelligent search', route: '/ai', gradient: 'gradient-cyan' },
        { icon: 'fas fa-chart-line', title: 'Brickberg', description: 'LEGO financial terminal', route: '/brickberg', gradient: 'gradient-green' }
    ];


    //
    // Live stats
    //
    stats: LiveStat[] = [
        { icon: 'fas fa-box-open', label: 'Sets', value: 0, loaded: false },
        { icon: 'fas fa-puzzle-piece', label: 'Parts', value: 0, loaded: false },
        { icon: 'fas fa-palette', label: 'Colours', value: 0, loaded: false },
        { icon: 'fas fa-layer-group', label: 'Themes', value: 0, loaded: false },
        { icon: 'fas fa-child-reaching', label: 'Minifigs', value: 0, loaded: false }
    ];


    constructor(
        private router: Router,
        private http: HttpClient,
        public authService: AuthService,
        private authNudgeService: AuthNudgeService
    ) { }


    ngOnInit(): void {

        //
        // Personalized greeting based on time of day and login status
        //
        const hour = new Date().getHours();

        if (this.authService.isLoggedIn) {
            const name = this.authService.fullName || this.authService.userName || '';
            this.displayName = name ? name.split(' ')[0] : '';

            if (hour < 12) {
                this.greeting = 'Good morning';
            }
            else if (hour < 17) {
                this.greeting = 'Good afternoon';
            }
            else {
                this.greeting = 'Good evening';
            }
        } else {
            // Anonymous user
            this.displayName = '';
            this.greeting = 'Welcome';
        }

        this.startTaglineRotation();
        this.loadStats();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();

        if (this.taglineInterval) {
            clearInterval(this.taglineInterval);
        }
    }


    /// Navigate to the selected persona pathway
    selectPathway(pathway: PathwayCard): void {
        this.router.navigate([pathway.route]);
    }


    /// Navigate to a feature from the quick-access strip
    goToFeature(feature: FeatureLink): void {
        this.router.navigate([feature.route]);
    }


    /// Navigate to a feature pill inside a pathway card
    goToFeaturePill(event: MouseEvent, route: string): void {
        event.stopPropagation();  // don't trigger the card's click
        this.router.navigate([route]);
    }


    /// Rotate taglines with a fade animation
    private startTaglineRotation(): void {
        this.taglineInterval = setInterval(() => {
            this.taglineVisible = false;

            setTimeout(() => {
                this.activeTaglineIndex = (this.activeTaglineIndex + 1) % this.taglines.length;
                this.taglineVisible = true;
            }, 400);
        }, 4000);
    }


    /// Load live database stats
    private loadStats(): void {

        if (this.authService.isLoggedIn) {
            //
            // Authenticated path — use generated RowCount endpoints
            //
            const headers = this.authService.GetAuthenticationHeaders();

            const endpoints = [
                '/api/LegoSets/RowCount',
                '/api/BrickParts/RowCount',
                '/api/BrickColours/RowCount',
                '/api/LegoThemes/RowCount',
                '/api/LegoMinifigs/RowCount'
            ];

            endpoints.forEach((url, index) => {
                this.http.get<any>(url, { headers })
                    .pipe(
                        takeUntil(this.destroy$),
                        catchError(() => of(null))
                    )
                    .subscribe(count => {
                        if (count != null) {
                            this.animateCounter(index, count);
                        }
                        this.stats[index].loaded = true;
                    });
            });
        } else {
            //
            // Anonymous path — use public browse endpoints to get counts
            //
            this.http.get<any[]>('/api/public/browse/sets')
                .pipe(takeUntil(this.destroy$), catchError(() => of([])))
                .subscribe(sets => {
                    this.animateCounter(0, Array.isArray(sets) ? sets.length : 0);
                    this.stats[0].loaded = true;
                });

            this.http.get<any>('/api/public/browse/parts-universe')
                .pipe(takeUntil(this.destroy$), catchError(() => of(null)))
                .subscribe(payload => {
                    if (payload?.stats?.totalUniqueParts) {
                        this.animateCounter(1, payload.stats.totalUniqueParts);
                    }
                    this.stats[1].loaded = true;
                });

            this.http.get<any[]>('/api/public/browse/colours')
                .pipe(takeUntil(this.destroy$), catchError(() => of([])))
                .subscribe(colours => {
                    this.animateCounter(2, Array.isArray(colours) ? colours.length : 0);
                    this.stats[2].loaded = true;
                });

            this.http.get<any[]>('/api/public/browse/themes')
                .pipe(takeUntil(this.destroy$), catchError(() => of([])))
                .subscribe(themes => {
                    this.animateCounter(3, Array.isArray(themes) ? themes.length : 0);
                    this.stats[3].loaded = true;
                });

            this.http.get<any[]>('/api/public/browse/minifigs')
                .pipe(takeUntil(this.destroy$), catchError(() => of([])))
                .subscribe(minifigs => {
                    this.animateCounter(4, Array.isArray(minifigs) ? minifigs.length : 0);
                    this.stats[4].loaded = true;
                });
        }
    }


    /// Animate a stat counter from 0 to target
    private animateCounter(index: number, target: number): void {

        const duration = 1500;
        const start = performance.now();

        const step = (timestamp: number) => {
            const progress = Math.min((timestamp - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            this.stats[index].value = Math.round(eased * target);

            if (progress < 1) {
                requestAnimationFrame(step);
            }
        };

        requestAnimationFrame(step);
    }
}

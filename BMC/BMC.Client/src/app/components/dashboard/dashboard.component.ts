import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Subject, forkJoin, of } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';


interface DashboardStat {
    icon: string;
    label: string;
    value: string | number;
    route?: string;
    loaded: boolean;
}

interface QuickAccessCard {
    icon: string;
    title: string;
    description: string;
    route: string;
    gradient: string;
}


@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    displayName = '';
    greeting = '';
    stats: DashboardStat[] = [];
    isLoading = true;


    quickAccess: QuickAccessCard[] = [
        {
            icon: 'fas fa-globe',
            title: 'Universe',
            description: 'The LEGO Universe — sets, themes, minifigs',
            route: '/lego',
            gradient: 'gradient-blue'
        },
        {
            icon: 'fas fa-cubes',
            title: 'Parts Catalog',
            description: 'Browse 79,000+ parts',
            route: '/parts',
            gradient: 'gradient-red'
        },
        {
            icon: 'fas fa-camera',
            title: 'Part Renderer',
            description: '3D rendering & ray tracing',
            route: '/part-renderer',
            gradient: 'gradient-purple'
        },
        {
            icon: 'fas fa-book',
            title: 'Manual Builder',
            description: 'Step-by-step build instructions',
            route: '/manual-generator',
            gradient: 'gradient-green'
        },
        {
            icon: 'fas fa-search',
            title: 'Set Explorer',
            description: 'Search and compare sets',
            route: '/lego/sets',
            gradient: 'gradient-orange'
        },
        {
            icon: 'fas fa-layer-group',
            title: 'Theme Explorer',
            description: 'Browse sets by theme',
            route: '/lego/themes',
            gradient: 'gradient-teal'
        },
        {
            icon: 'fas fa-child-reaching',
            title: 'Minifig Gallery',
            description: 'Complete minifig catalogue',
            route: '/lego/minifigs',
            gradient: 'gradient-pink'
        },
        {
            icon: 'fas fa-box-open',
            title: 'My Collection',
            description: 'Track your sets & wishlist',
            route: '/my-collection',
            gradient: 'gradient-amber'
        },
        {
            icon: 'fas fa-palette',
            title: 'Colour Library',
            description: 'Browse brick colours',
            route: '/colours',
            gradient: 'gradient-rainbow'
        },
        {
            icon: 'fas fa-exchange-alt',
            title: 'Compare Sets',
            description: 'Side-by-side comparisons',
            route: '/lego/compare',
            gradient: 'gradient-indigo'
        },
        {
            icon: 'fas fa-robot',
            title: 'AI Assistant',
            description: 'Intelligent search',
            route: '/ai',
            gradient: 'gradient-cyan'
        }
    ];


    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    ngOnInit(): void {

        //
        // Personalized greeting
        //
        const name = this.authService.fullName || this.authService.userName || '';
        this.displayName = name ? name.split(' ')[0] : '';

        const hour = new Date().getHours();
        if (hour < 12) { this.greeting = 'Good morning'; }
        else if (hour < 17) { this.greeting = 'Good afternoon'; }
        else { this.greeting = 'Good evening'; }


        //
        // Initialize stat cards with skeleton state
        //
        this.stats = [
            { icon: 'fas fa-box-open', label: 'LEGO Sets', value: '—', route: '/lego/sets', loaded: false },
            { icon: 'fas fa-puzzle-piece', label: 'Parts', value: '—', route: '/parts', loaded: false },
            { icon: 'fas fa-palette', label: 'Colours', value: '—', route: '/colours', loaded: false },
            { icon: 'fas fa-layer-group', label: 'Themes', value: '—', route: '/lego/themes', loaded: false },
            { icon: 'fas fa-child-reaching', label: 'Minifigures', value: '—', route: '/lego/minifigs', loaded: false },
            { icon: 'fas fa-project-diagram', label: 'Projects', value: '—', route: '/projects', loaded: false },
            { icon: 'fas fa-heart', label: 'My Collection', value: '—', route: '/my-collection', loaded: false }
        ];


        this.loadDashboardData();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    private loadDashboardData(): void {

        this.isLoading = true;

        const headers = this.authService.GetAuthenticationHeaders();

        const endpoints = [
            '/api/LegoSets/RowCount',
            '/api/BrickParts/RowCount',
            '/api/BrickColours/RowCount',
            '/api/LegoThemes/RowCount',
            '/api/Minifigs/RowCount',
            '/api/Projects/RowCount',
            '/api/UserCollectionSets/RowCount'
        ];


        //
        // Fire all requests in parallel.  Each one independently
        // updates its stat card so the UI fills in progressively.
        //
        endpoints.forEach((url, index) => {

            this.http.get<any>(url, { headers })
                .pipe(
                    takeUntil(this.destroy$),
                    catchError(() => of(null))
                )
                .subscribe(count => {
                    this.stats[index].value = count ?? '—';
                    this.stats[index].loaded = true;

                    //
                    // Once all have returned, clear the global loading flag.
                    //
                    if (this.stats.every(s => s.loaded)) {
                        this.isLoading = false;
                    }
                });
        });
    }
}

import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { ThemeService } from '../../services/theme.service';
import { filter } from 'rxjs/operators';

interface NavItem {
    icon: string;
    label: string;
    route: string;
    badge?: string;
}

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent {

    @Input() isUserLoggedIn = false;
    @Output() invokeLogout = new EventEmitter<void>();

    themeDropdownOpen = false;
    mobileMenuOpen = false;

    /** Same navigation items as the sidebar */
    navItems: NavItem[] = [
        { icon: 'fas fa-th-large', label: 'Dashboard', route: '/dashboard' },
        { icon: 'fas fa-globe', label: 'Universe', route: '/lego' },
        { icon: 'fas fa-user-circle', label: 'My Profile', route: '/profile' },
        { icon: 'fas fa-cubes', label: 'Parts Catalog', route: '/parts' },
        { icon: 'fas fa-camera', label: 'Part Renderer', route: '/part-renderer' },
        { icon: 'fas fa-book', label: 'Manual Generator', route: '/manual-generator' },
        { icon: 'fas fa-layer-group', label: 'My Collection', route: '/my-collection' },
        { icon: 'fas fa-project-diagram', label: 'Projects', route: '/projects' },
        { icon: 'fas fa-palette', label: 'Colours', route: '/colours' },
        { icon: 'fas fa-robot', label: 'AI Assistant', route: '/ai' },
        { icon: 'fas fa-heartbeat', label: 'System Health', route: '/system-health' },
    ];

    constructor(
        public themeService: ThemeService,
        private router: Router
    ) {
        // Close the mobile menu whenever the route changes
        this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe(() => {
                this.mobileMenuOpen = false;
            });
    }


    /**
     * Emit a logout event to the parent component.
     */
    public logout(): void {
        this.invokeLogout.emit();
    }


    /**
     * Toggle the theme picker dropdown.
     */
    public toggleThemeDropdown(event: Event): void {
        event.stopPropagation();
        this.themeDropdownOpen = !this.themeDropdownOpen;
    }


    /**
     * Switch the active UI theme and close the dropdown.
     */
    public setTheme(themeId: string): void {
        this.themeService.setTheme(themeId);
        this.themeDropdownOpen = false;
    }


    /**
     * Toggle the mobile hamburger menu.
     */
    public toggleMobileMenu(event: Event): void {
        event.stopPropagation();
        this.mobileMenuOpen = !this.mobileMenuOpen;

        // Close the theme dropdown when opening mobile menu
        if (this.mobileMenuOpen) {
            this.themeDropdownOpen = false;
        }
    }


    /**
     * Check whether the given route is the currently active route.
     */
    public isActive(route: string): boolean {
        return this.router.url.startsWith(route);
    }


    /**
     * Close dropdowns when clicking elsewhere.
     */
    @HostListener('document:click')
    onDocumentClick(): void {
        this.themeDropdownOpen = false;
    }
}

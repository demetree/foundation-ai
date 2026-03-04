import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';

interface NavItem {
    icon: string;
    label: string;
    route: string;
    badge?: string;
}

interface NavGroup {
    label: string;
    items: NavItem[];
}

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
    @Input() isCollapsed = false;
    @Output() toggleCollapse = new EventEmitter<boolean>();


    //
    // Grouped navigation items — organized by persona / function
    //
    navGroups: NavGroup[] = [
        {
            label: '',
            items: [
                { icon: 'fas fa-home', label: 'Welcome', route: '/welcome' }
            ]
        },
        {
            label: 'EXPLORE',
            items: [
                { icon: 'fas fa-globe', label: 'Universe', route: '/lego' },
                { icon: 'fas fa-cubes', label: 'Parts Catalog', route: '/parts' },
                { icon: 'fas fa-palette', label: 'Colours', route: '/colours' }
            ]
        },
        {
            label: 'CREATE & BUILD',
            items: [
                { icon: 'fas fa-camera', label: 'Part Renderer', route: '/part-renderer' },
                { icon: 'fas fa-book', label: 'Manual Generator', route: '/manual-generator' },
                { icon: 'fas fa-project-diagram', label: 'Projects', route: '/projects' }
            ]
        },
        {
            label: 'MY STUFF',
            items: [
                { icon: 'fas fa-user-circle', label: 'My Profile', route: '/profile' },
                { icon: 'fas fa-layer-group', label: 'My Collection', route: '/my-collection' },
                { icon: 'fas fa-box-open', label: 'My Sets', route: '/my-sets' },
                { icon: 'fas fa-list-ul', label: 'My Set Lists', route: '/my-set-lists' },
                { icon: 'fas fa-list-check', label: 'My Part Lists', route: '/my-part-lists' },
                { icon: 'fas fa-search-minus', label: 'Lost Parts', route: '/my-lost-parts' }
            ]
        },
        {
            label: 'TOOLS',
            items: [
                { icon: 'fas fa-chart-line', label: 'Brickberg', route: '/brickberg' },
                { icon: 'fas fa-th-large', label: 'Dashboard', route: '/dashboard' },
                { icon: 'fas fa-plug', label: 'Integrations', route: '/integrations' },
                { icon: 'fas fa-robot', label: 'AI Assistant', route: '/ai' },
                { icon: 'fas fa-heartbeat', label: 'System Health', route: '/system-health' }
            ]
        }
    ];


    constructor(public router: Router) { }

    toggle() {
        this.isCollapsed = !this.isCollapsed;
        this.toggleCollapse.emit(this.isCollapsed);
    }

    isActive(route: string): boolean {
        return this.router.url.startsWith(route);
    }
}

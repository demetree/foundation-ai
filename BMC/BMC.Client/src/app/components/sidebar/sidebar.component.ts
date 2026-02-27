import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';

interface NavItem {
    icon: string;
    label: string;
    route: string;
    badge?: string;
}

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
    @Input() isCollapsed = false;
    @Output() toggleCollapse = new EventEmitter<boolean>();

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

    constructor(public router: Router) { }

    toggle() {
        this.isCollapsed = !this.isCollapsed;
        this.toggleCollapse.emit(this.isCollapsed);
    }

    isActive(route: string): boolean {
        return this.router.url.startsWith(route);
    }
}

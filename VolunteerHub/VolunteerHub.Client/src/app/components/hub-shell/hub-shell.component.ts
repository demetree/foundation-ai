import { Component } from '@angular/core';
import { HubAuthService } from '../../services/hub-auth.service';

@Component({
    selector: 'app-hub-shell',
    templateUrl: './hub-shell.component.html',
    styleUrls: ['./hub-shell.component.scss']
})
export class HubShellComponent {

    navItems = [
        { path: '/dashboard', icon: 'fas fa-home', label: 'Home' },
        { path: '/schedule', icon: 'fas fa-calendar', label: 'Schedule' },
        { path: '/hours', icon: 'fas fa-clock', label: 'Hours' },
        { path: '/opportunities', icon: 'fas fa-search', label: 'Opportunities' },
        { path: '/profile', icon: 'fas fa-user', label: 'Profile' }
    ];

    constructor(public auth: HubAuthService) { }

    logout(): void {
        this.auth.logout();
    }
}

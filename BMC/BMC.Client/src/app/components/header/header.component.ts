import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { ThemeService } from '../../services/theme.service';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent {

    @Input() isUserLoggedIn = false;
    @Output() invokeLogout = new EventEmitter<void>();

    themeDropdownOpen = false;

    constructor(public themeService: ThemeService) { }


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
     * Close dropdown when clicking elsewhere.
     */
    @HostListener('document:click')
    onDocumentClick(): void {
        this.themeDropdownOpen = false;
    }
}

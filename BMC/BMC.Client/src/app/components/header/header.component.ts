import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ThemeService } from '../../services/theme.service';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent {

    @Input() isUserLoggedIn = false;
    @Output() invokeLogout = new EventEmitter<void>();


    constructor(public themeService: ThemeService) { }


    /**
     * Emit a logout event to the parent component.
     */
    public logout(): void {
        this.invokeLogout.emit();
    }


    /**
     * Switch the active UI theme.
     */
    public setTheme(themeId: string): void {
        this.themeService.setTheme(themeId);
    }
}

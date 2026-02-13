import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent {
    @Input() isUserLoggedIn = false;
    @Output() invokeLogout = new EventEmitter<void>();

    logout() {
        this.invokeLogout.emit();
    }
}

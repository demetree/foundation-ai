import { Component, OnInit, HostListener } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { ToastaService, ToastaConfig, ToastOptions, ToastData } from 'ngx-toasta';

import { AlertService, AlertCommand, MessageSeverity } from './services/alert.service';
import { AuthService } from './services/auth.service';
import { ConfigurationService } from './services/configuration.service';
import { LocalStoreManager } from './services/local-store-manager.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {

    isAppLoaded = false;
    isUserLoggedIn = false;
    isOnLoginPage = false;
    appTitle = 'BMC';
    isMobile = false;
    isSidebarCollapsed = false;

    private stickyToasties: number[] = [];

    constructor(
        private toastaService: ToastaService,
        private toastaConfig: ToastaConfig,
        private alertService: AlertService,
        private authService: AuthService,
        public configurations: ConfigurationService,
        public router: Router,
        private localStorage: LocalStoreManager,
    ) {
        this.toastaConfig.theme = 'bootstrap';
        this.toastaConfig.position = 'top-right';
        this.toastaConfig.limit = 100;
        this.toastaConfig.showClose = true;
        this.toastaConfig.showDuration = false;
    }


    ngOnInit() {
        this.localStorage.initialiseStorageSyncListener();
        this.isUserLoggedIn = this.authService.isLoggedIn;
        this.checkScreenSize();

        // Display the app after a brief delay for smooth transition
        setTimeout(() => {
            this.isAppLoaded = true;
            const preBootstrap = document.getElementById('pre-bootstrap');
            if (preBootstrap) {
                preBootstrap.style.display = 'none';
            }
        }, 50);

        // Welcome back message
        setTimeout(() => {
            if (this.isUserLoggedIn) {
                this.alertService.showMessage('Welcome', `Welcome back, ${this.userName}`, MessageSeverity.default);
            }
        }, 1500);

        this.alertService.getMessageEvent().subscribe(message => this.showToast(message));

        this.authService.getLoginStatusEvent().subscribe(isLoggedIn => {
            this.isUserLoggedIn = isLoggedIn;

            if (!this.isUserLoggedIn) {
                setTimeout(() => {
                    this.alertService.showMessage('Session Ended', '', MessageSeverity.default);
                }, 500);
            }
        });

        // Toggle body background and track login page state for layout visibility
        this.router.events.subscribe((event) => {
            if (event instanceof NavigationEnd) {
                this.isOnLoginPage = (event.urlAfterRedirects === '/login');

                if (this.isOnLoginPage) {
                    document.body.className = 'pre-login-background no-select';
                } else {
                    document.body.className = 'post-login-background no-select';
                }
            }
        });
    }


    @HostListener('window:resize')
    checkScreenSize() {
        this.isMobile = window.innerWidth < 768;
    }


    onSidebarToggle(collapsed: boolean) {
        this.isSidebarCollapsed = collapsed;
    }


    showToast(alert: AlertCommand) {
        if (alert.operation === 'clear') {
            for (const id of this.stickyToasties.slice(0)) {
                this.toastaService.clear(id);
            }
            return;
        }

        const toastOptions: ToastOptions = {
            title: alert.message?.summary,
            msg: alert.message?.detail,
        };

        if (alert.operation === 'add_sticky') {
            toastOptions.timeout = 0;
            toastOptions.onAdd = (toast: ToastData) => this.stickyToasties.push(toast.id);
            toastOptions.onRemove = (toast: ToastData) => {
                const index = this.stickyToasties.indexOf(toast.id, 0);
                if (index > -1) { this.stickyToasties.splice(index, 1); }
                if (alert.onRemove) { alert.onRemove(); }
            };
        } else {
            toastOptions.timeout = 4000;
        }

        switch (alert.message?.severity) {
            case MessageSeverity.default: this.toastaService.default(toastOptions); break;
            case MessageSeverity.info: this.toastaService.info(toastOptions); break;
            case MessageSeverity.success: this.toastaService.success(toastOptions); break;
            case MessageSeverity.error: this.toastaService.error(toastOptions); break;
            case MessageSeverity.warn: this.toastaService.warning(toastOptions); break;
            case MessageSeverity.wait: this.toastaService.wait(toastOptions); break;
        }
    }


    logout() {
        this.authService.logout();
        this.authService.redirectLogoutUser();
    }

    get userName(): string {
        return this.authService.currentUser?.userName ?? '';
    }

    get fullName(): string {
        return this.authService.currentUser?.fullName ?? '';
    }
}

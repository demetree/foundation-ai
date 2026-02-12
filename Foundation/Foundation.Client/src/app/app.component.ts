import { Component, OnInit, OnDestroy, HostListener, ViewChild, ElementRef, Inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ToastaService, ToastaConfig, ToastOptions, ToastData } from 'ngx-toasta';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import { AlertService, AlertDialog, DialogType, AlertCommand, MessageSeverity } from './services/alert.service';
import { NavigationService } from './utility-services/navigation.service';
import { BodyClassService } from './utility-services/body-class.service';

import { CurrentUserService } from './services/current-user.service';
import { AuditorDataServiceManagerService } from './auditor-data-services/auditor-data-service-manager.service';
import { SecurityDataServiceManagerService } from './security-data-services/security-data-service-manager.service';

import { AppTranslationService } from './services/app-translation.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { AppTitleService } from './services/app-title.service';
import { AuthService } from './services/auth.service';
import { ConfigurationService } from './services/configuration.service';
import { Alertify } from './models/Alertify';
import { LoginComponent } from './components/login/login.component';
import { Modal } from 'bootstrap';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';

declare let alertify: Alertify;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {

  public isAppLoaded = false;
  public isUserLoggedIn = false;
  public newNotificationCount = 0;
  public appTitle = 'Foundation';
  
  private stickyToasties: number[] = [];

  private dataLoadingConsecutiveFailures = 0;
  private notificationsLoadingSubscription: Subscription | undefined;

  private loginControl: LoginComponent | undefined;

  private isloginModalShown: boolean = false;
  isButtonVisible = true;
  
  gT = (key: string | Array<string>, interpolateParams?: object) =>
    this.translationService.getTranslation(key, interpolateParams);


  constructor(
    storageManager: LocalStoreManager,
    private currentUserService: CurrentUserService,
    private auditorDataServiceManagerService: AuditorDataServiceManagerService,
    private securityDataServiceManagerService: SecurityDataServiceManagerService,
    private toastaService: ToastaService,
    private toastaConfig: ToastaConfig,
    private alertService: AlertService,
    private bodyClassService: BodyClassService,
    private modalService: NgbModal,
    private authService: AuthService,
    private translationService: AppTranslationService,
    public configurations: ConfigurationService,
    public router: Router,
    private navigationService: NavigationService,
    @Inject('BASE_URL') private baseUrl: string,
    private http: HttpClient,  ) {

    storageManager.initialiseStorageSyncListener();

    this.toastaConfig.theme = 'bootstrap';
    this.toastaConfig.position = 'top-right';
    this.toastaConfig.limit = 100;
    this.toastaConfig.showClose = true;
    this.toastaConfig.showDuration = false;

    AppTitleService.appName = this.appTitle;
  }
  modalInstance: Modal | undefined;

  isMobile: boolean = false;

  // Set isMobile based on screen width
  checkScreenSize() {
    this.isMobile = window.innerWidth < 768; // Set threshold for mobile screen size
  }
  // Listen for window resize events to check screen size
  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.checkScreenSize();
  }


  ngOnInit() {

    this.isUserLoggedIn = this.authService.isLoggedIn;

    this.auditorDataServiceManagerService.ClearAllCaches();
    this.securityDataServiceManagerService.ClearAllCaches();

    // Extra 1/20 second to display preboot loaded information
    setTimeout(() => {

      this.isAppLoaded = true;

      const preBootstrap = document.getElementById('pre-bootstrap');
      if (preBootstrap) {
        preBootstrap.style.display = 'none';
      }
    }, 50);


    setTimeout(() => {
      if (this.isUserLoggedIn) {

        if (this.authService.isSecurityReader == false ||
            this.authService.isAuditorReader == false) {
          //
          // Display can't read alert
          //
          this.alertService.resetStickyMessage();
          this.alertService.showMessage(this.gT('app.alerts.Login'), "Your account does not have the privilege to read from the Auditor or Security modules", MessageSeverity.error);

        } else {

          //
          // Display regular intro alert if user can read from Foundation
          //
          this.alertService.resetStickyMessage();
          this.alertService.showMessage(this.gT('app.alerts.Login'), this.gT('app.alerts.WelcomeBack', { username: this.userName }), MessageSeverity.default);
        }
      }
    }, 2000);

    this.checkScreenSize();
    this.alertService.getDialogEvent().subscribe(alert => this.showDialog(alert));
    this.alertService.getMessageEvent().subscribe(message => this.showToast(message));

    this.authService.reLoginDelegate = () => this.openLoginModal();

    this.authService.getLoginStatusEvent().subscribe(isLoggedIn => {

      this.auditorDataServiceManagerService.ClearAllCaches();
      this.securityDataServiceManagerService.ClearAllCaches();

      this.isUserLoggedIn = isLoggedIn;


      setTimeout(() => {
        if (!this.isUserLoggedIn) {
          this.alertService.showMessage(this.gT('app.alerts.SessionEnded'), '', MessageSeverity.default);
        }
      }, 500);
    });

    //
    // Use the route to determine the body background.  Show a different background when on the login route.
    //
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        if (event.urlAfterRedirects === '/login') {
          this.bodyClassService.replaceClass('post-login-background', 'pre-login-background');
        } else {
          this.bodyClassService.replaceClass('pre-login-background', 'post-login-background');
        }
      }
    });
  }

  ngOnDestroy() {
    this.unsubscribeNotifications();
  }

  private unsubscribeNotifications() {
    this.notificationsLoadingSubscription?.unsubscribe();
  }


  openLoginModal() {

    //
    // Suppress the modal if user is already on the login page - no value in showing a modal over the login form
    //
    if (this.router.url === '/login') {
      return;
    }


    if (this.isloginModalShown == true) {
      return;
    }

    this.auditorDataServiceManagerService.ClearAllCaches();
    this.securityDataServiceManagerService.ClearAllCaches();


    const loginModalRef = this.modalService.open(LoginComponent, {
      windowClass: 'login-control',
      modalDialogClass: 'h-75 d-flex flex-column justify-content-center my-0',
      size: 'lg',
      backdrop: 'static'
    });

    this.isloginModalShown = true;

    this.loginControl = loginModalRef.componentInstance as LoginComponent;
    this.loginControl.isModal = true;

    this.loginControl.modalClosedCallback = () => {
      loginModalRef.close();
      this.isloginModalShown = false;
    }

    loginModalRef.shown.subscribe(() => {
      this.alertService.showStickyMessage(this.gT('app.alerts.SessionExpired'),  this.gT('app.alerts.SessionExpiredLoginAgain'), MessageSeverity.info);
    });

    loginModalRef.hidden.subscribe(() => {
      this.alertService.resetStickyMessage();
      this.loginControl?.reset();

      if (this.authService.isSessionExpired) {
        this.alertService.showStickyMessage(this.gT('app.alerts.SessionExpired'),  this.gT('app.alerts.SessionExpiredLoginToRenewSession'), MessageSeverity.warn);
      }
    });
  }

  showDialog(dialog: AlertDialog) {
    alertify.set({
      labels: {
        ok: dialog.okLabel || this.gT('app.alerts.OK'),
        cancel: dialog.cancelLabel || this.gT('app.alerts.Cancel')
      }
    });

    switch (dialog.type) {
      case DialogType.alert:
        alertify.alert(dialog.message);
        break;
      case DialogType.confirm:
        alertify.confirm(dialog.message, ok => {
          if (ok) {
            if (dialog.okCallback)
              dialog.okCallback();
          } else {
            if (dialog.cancelCallback) {
              dialog.cancelCallback();
            }
          }
        });
        break;
      case DialogType.prompt:
        alertify.prompt(dialog.message, (ok, val) => {
          if (ok) {
            if (dialog.okCallback)
              dialog.okCallback(val);
          } else {
            if (dialog.cancelCallback) {
              dialog.cancelCallback();
            }
          }
        }, dialog.defaultValue);
        break;
    }
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

      toastOptions.onAdd = (toast: ToastData) => {
        this.stickyToasties.push(toast.id);
      };

      toastOptions.onRemove = (toast: ToastData) => {
        const index = this.stickyToasties.indexOf(toast.id, 0);

        if (index > -1) {
          this.stickyToasties.splice(index, 1);
        }

        if (alert.onRemove) {
          alert.onRemove();
        }

        toast.onAdd = undefined;
        toast.onRemove = undefined;
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

    this.auditorDataServiceManagerService.ClearAllCaches();
    this.securityDataServiceManagerService.ClearAllCaches();

    this.authService.logout();
    this.authService.redirectLogoutUser();
  }

  getYear() {
    return new Date().getUTCFullYear();
  }

  get userName(): string {
    return this.authService.currentUser?.userName ?? '';
  }

  get fullName(): string {
    return this.authService.currentUser?.fullName ?? '';
  }

  get tenantName(): string {
    return this.authService.currentUser?.tenantName ?? '';
  }
}

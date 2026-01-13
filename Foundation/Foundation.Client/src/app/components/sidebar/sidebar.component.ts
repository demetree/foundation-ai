import {
  Component, HostListener, Input, OnDestroy, AfterViewInit,
  ViewChildren, QueryList, ViewChild, EventEmitter, Output
} from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuditorDataServiceManagerService } from '../../auditor-data-services/auditor-data-service-manager.service';
import { SecurityDataServiceManagerService } from '../../security-data-services/security-data-service-manager.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnDestroy, AfterViewInit {
  @Input() isUserLoggedIn!: boolean;
  @Output() helpClicked = new EventEmitter<void>();
  @ViewChildren(NgbTooltip) tips!: QueryList<NgbTooltip>;
  @ViewChild('customerSupport') customerSupport: any; // optional

  isExpanded = false;
  isUserFoundationAdmin = false; // keep if you gate items elsewhere
  private navSub?: Subscription;

  constructor(
    private router: Router,
    private auditorDataServiceManagerService: AuditorDataServiceManagerService,
    private securityDataServiceManagerService: SecurityDataServiceManagerService,
    private authService: AuthService
  ) {
    // Close side panel + tooltips on navigation
    this.navSub = this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(() => {
        this.closeAllTooltips();
        this.close();
      });

    this.isUserFoundationAdmin = this.authService?.isFoundationAdmin ?? false;
  }

  ngAfterViewInit(): void {
    this.closeAllTooltips();
  }

  toggle(): void {
    this.closeAllTooltips();
    this.isExpanded = !this.isExpanded;
  }

  close(): void {
    this.closeAllTooltips();
    this.isExpanded = false;
  }

  openHelp(): void {
    this.closeAllTooltips();
    this.helpClicked.emit();
    this.customerSupport?.opencustomerSupportModal?.();
    if (this.isExpanded) this.close();
  }

  logout(): void {
    this.auditorDataServiceManagerService.ClearAllCaches();
    this.securityDataServiceManagerService.ClearAllCaches();
    this.authService?.logout?.();
    this.authService?.redirectLogoutUser?.();
  }

  private closeAllTooltips(): void {
    this.tips?.forEach(t => t.close());
  }

  @HostListener('window:keydown.escape')
  onEsc(): void {
    if (this.isExpanded) this.close();
    else this.closeAllTooltips();
  }

  ngOnDestroy(): void {
    this.navSub?.unsubscribe();
  }
}

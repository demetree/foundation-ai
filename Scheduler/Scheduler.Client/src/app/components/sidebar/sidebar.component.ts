import { Component, HostListener, Input, OnDestroy, AfterViewInit, ViewChildren, QueryList, ViewChild, EventEmitter, Output } from '@angular/core';
import { MessagingComponent } from '../messaging/messaging.component';
import { Router, NavigationEnd } from '@angular/router';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { SchedulerHelperService } from '../../services/scheduler-helper.service';
import { SchedulerModeService } from '../../services/scheduler-mode.service';
import { FeatureConfigService } from '../../services/feature-config.service';
import { TerminologyService } from '../../services/terminology.service';

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
  @ViewChild('messagingPanel') messagingPanel?: MessagingComponent;

  public isExpanded = false;
  public volunteersExpanded = false;
  public setupExpanded = false;
  public isUserFoundationAdmin = false; // keep if you gate items elsewhere
  public isSimpleMode = true;
  private navSub?: Subscription;
  private modeSub?: Subscription;


  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;


  constructor(
    private router: Router,
    private authService: AuthService,
    private schedulerHelperService: SchedulerHelperService,
    private schedulerModeService: SchedulerModeService,
    private featureConfigService: FeatureConfigService,
    public terminology: TerminologyService) {

    //
    // Close side panel + tooltips on navigation
    //
    this.navSub = this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(() => {
        this.closeAllTooltips();
        this.close();
      });

    this.isUserFoundationAdmin = this.authService?.isFoundationAdmin ?? false;

    //
    // Subscribe to the mode service for the sidebar component
    //
    this.modeSub = this.schedulerModeService.isSimpleMode('sidebar')
      .subscribe(simple => {
        this.isSimpleMode = simple;
      });
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
    this.authService?.logout?.();
    this.authService?.redirectLogoutUser?.();
  }

  public get isUserSchedulerAdmin(): boolean {
    return this.authService.isSchedulerAdministrator;
  } 


  // AI-Developed — Feature toggle visibility properties added with AI assistance.
  public isVolunteerEnabled$ = this.featureConfigService.isVolunteerEnabled$;
  public isFundraisingEnabled$ = this.featureConfigService.isFundraisingEnabled$;
  public isFinancialEnabled$ = this.featureConfigService.isFinancialEnabled$;
  public isCrewEnabled$ = this.featureConfigService.isCrewEnabled$;
  public isMessagingEnabled$ = this.featureConfigService.isMessagingEnabled$;

  public get isVolunteerManager(): boolean {
    return this.authService.isVolunteerManager;
  }

  public get isFundraisingManager(): boolean {
    return this.authService.isFundraisingManager;
  }

  public get isFinancialManager(): boolean {
    return this.authService.isFinancialManager;
  }

  public get isMessagingManager(): boolean {
    return this.authService.isMessagingManager;
  }

  

  private closeAllTooltips(): void {
    this.tips?.forEach(t => t.close());
  }

  @HostListener('window:keydown.escape')
  onEsc(): void {
    if (this.isExpanded) this.close();
    else this.closeAllTooltips();
  }

  /**
   * Toggles the global scheduler mode between simple and advanced.
   */
  public toggleSchedulerMode(): void {
    this.schedulerModeService.toggleGlobalMode();
  }


  /**
   * Toggle the messaging panel (off-canvas slide-out).
   * Called from the header's messaging button via the app component.
   */
  public toggleMessaging(): void {
    if (this.messagingPanel) {
      this.messagingPanel.isOpen = !this.messagingPanel.isOpen;
    }
  }

  /**
   * Total unread message count for badge display.
   */
  public get unreadMessageCount(): number {
    return this.messagingPanel?.totalUnreadCount ?? 0;
  }


  ngOnDestroy(): void {
    this.navSub?.unsubscribe();
    this.modeSub?.unsubscribe();
  }
}

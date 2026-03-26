import { Component, HostListener, Input, OnDestroy, OnChanges, SimpleChanges, AfterViewInit, ViewChildren, QueryList, ViewChild, EventEmitter, Output } from '@angular/core';
import { MessagingComponent } from '../messaging/messaging.component';
import { Router, NavigationEnd } from '@angular/router';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
import { Observable, Subscription, of } from 'rxjs';
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
export class SidebarComponent implements OnDestroy, OnChanges, AfterViewInit {
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
  private _loginInitialized = false;


  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$: Observable<number> = of(0);


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
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isUserLoggedIn'] && this.isUserLoggedIn && !this._loginInitialized) {
      this._loginInitialized = true;
      this.initLoggedInSubscriptions();
    }
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
  // Gated behind isUserLoggedIn to prevent server requests before authentication.
  public isVolunteerEnabled$: Observable<boolean> = of(false);
  public isFundraisingEnabled$: Observable<boolean> = of(false);
  public isFinancialEnabled$: Observable<boolean> = of(false);
  public isCrewEnabled$: Observable<boolean> = of(false);
  public isMessagingEnabled$: Observable<boolean> = of(false);
  public isDispatchEnabled$: Observable<boolean> = of(false);

  /**
   * Activates feature config, office count, and mode subscriptions
   * only after the user is fully logged in.
   */
  private initLoggedInSubscriptions(): void {
    this.isVolunteerEnabled$ = this.featureConfigService.isVolunteerEnabled$;
    this.isFundraisingEnabled$ = this.featureConfigService.isFundraisingEnabled$;
    this.isFinancialEnabled$ = this.featureConfigService.isFinancialEnabled$;
    this.isCrewEnabled$ = this.featureConfigService.isCrewEnabled$;
    this.isMessagingEnabled$ = this.featureConfigService.isMessagingEnabled$;
    this.isDispatchEnabled$ = this.featureConfigService.isDispatchEnabled$;

    this.officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

    this.modeSub = this.schedulerModeService.isSimpleMode('sidebar')
      .subscribe(simple => {
        this.isSimpleMode = simple;
      });
  }

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

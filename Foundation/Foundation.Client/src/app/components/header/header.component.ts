import { Component, OnInit, EventEmitter, Output, Input, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AlertService } from '../../services/alert.service';
import { HttpClient } from '@angular/common/http';
import { NgbDropdown } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  @Input() isUserLoggedIn!: boolean;
  @Output() toggle = new EventEmitter();
  @Output() invokeLogout = new EventEmitter();
  @Input() projectId: number | null = null;

  /** Show in the dropdown footer. Pass from root if you want CI-stamped values (e.g., v1.12.3+abcd123). */
  @Input() versionInfo: string = 'v0.0.0';

  showNotifications: boolean = false;
  totalEventCount: number = 0;
  public filterText: string | null = null;

  // PHONE PART: mobile menu state
  isMobileMenuOpen = false;

  constructor(
    public authService: AuthService,
    private alertService: AlertService,
    @Inject('BASE_URL') private baseUrl: string,
    private http: HttpClient,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.LoadData();
  }

  public LoadData(): void {
    // (left intentionally as-is; you can re-enable when needed)
  }

  toggleNotifications(event: MouseEvent) {
    event.stopPropagation();
    this.showNotifications = !this.showNotifications;
  }

  closeNotificationsPanel() {
    this.showNotifications = false;
  }

  logout() {
    this.invokeLogout.emit();
    // ensure mobile menu closes when logging out on small screens
    this.isMobileMenuOpen = false;
  }

  goToSettings(dropdown: NgbDropdown) {
    this.router.navigate(['/settings']);
    dropdown.close();
  }

  // PHONE PART: mobile menu controls
  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
  }

  get tenantName(): string {
    return this.authService.currentUser?.tenantName ?? '';
  }

  get tenantAndUserName(): string {
    const u = this.authService.currentUser;
    return u ? `${u.tenantName} - ${u.fullName}` : '';
  }

  get currentUserFirstName(): string {
    const name = this.authService.currentUser?.fullName || '';
    return name.replace(/,/g, '');
  }

  get userInitials(): string {
    const name = this.authService.currentUser?.fullName || '';
    const initials = name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0])
      .join('');
    return initials || 'U';
  }
}

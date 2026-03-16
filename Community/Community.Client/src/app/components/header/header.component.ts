import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { PublicContentService, MenuData, MenuItemData, SiteSettings } from '../../services/public-content.service';


@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  siteName: string = 'Community';
  menuItems: MenuItemData[] = [];
  mobileMenuOpen: boolean = false;
  isScrolled: boolean = false;
  currentUrl: string = '/';

  constructor(
    private contentService: PublicContentService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Load site settings
    this.contentService.getSiteSettings().subscribe(settings => {
      this.siteName = settings['siteName'] || 'Community';
    });

    // Load header menu
    this.contentService.getMenuByLocation('header').subscribe(menu => {
      this.menuItems = menu.items || [];
    });

    // Track current route for active state
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.currentUrl = event.urlAfterRedirects || event.url;
      this.mobileMenuOpen = false; // close mobile menu on navigate
    });

    // Scroll listener for header transparency
    if (typeof window !== 'undefined') {
      window.addEventListener('scroll', () => {
        this.isScrolled = window.scrollY > 20;
      });
    }
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  isActive(item: MenuItemData): boolean {
    const url = item.pageSlug ? `/${item.pageSlug}` : item.url;
    return this.currentUrl === url;
  }

  getItemUrl(item: MenuItemData): string {
    return item.pageSlug ? `/${item.pageSlug}` : (item.url || '/');
  }
}

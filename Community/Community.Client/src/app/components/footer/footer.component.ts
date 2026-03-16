import { Component, OnInit } from '@angular/core';
import { PublicContentService, SiteSettings, MenuItemData } from '../../services/public-content.service';


@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit {

  siteName: string = 'Community';
  footerText: string = '';
  footerMenuItems: MenuItemData[] = [];
  currentYear: number = new Date().getFullYear();

  constructor(private contentService: PublicContentService) { }

  ngOnInit(): void {
    this.contentService.getSiteSettings().subscribe(settings => {
      this.siteName = settings['siteName'] || 'Community';
      this.footerText = settings['footerText'] || `© ${this.currentYear} All rights reserved.`;
    });

    this.contentService.getMenuByLocation('footer').subscribe(menu => {
      this.footerMenuItems = menu.items || [];
    });
  }

  getItemUrl(item: MenuItemData): string {
    return item.pageSlug ? `/${item.pageSlug}` : (item.url || '/');
  }
}

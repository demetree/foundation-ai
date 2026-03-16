import { Component, OnInit } from '@angular/core';
import { PublicContentService, SiteSettings, PostData, AnnouncementData } from '../../services/public-content.service';
import { SeoService } from '../../services/seo.service';


@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss']
})
export class HomePageComponent implements OnInit {

  settings: SiteSettings = {};
  announcements: AnnouncementData[] = [];
  latestPosts: PostData[] = [];
  loading: boolean = true;

  constructor(
    private contentService: PublicContentService,
    private seoService: SeoService
  ) { }

  ngOnInit(): void {
    this.seoService.setPage();

    // Load site settings
    this.contentService.getSiteSettings().subscribe(settings => {
      this.settings = settings;
      this.seoService.setPage(
        settings['heroTitle'] || undefined,
        settings['heroSubtitle'] || undefined
      );
    });

    // Load active announcements
    this.contentService.getActiveAnnouncements().subscribe(announcements => {
      this.announcements = announcements;
    });

    // Load latest posts
    this.contentService.getPublishedPosts(1, 3).subscribe(response => {
      this.latestPosts = response.items || [];
      this.loading = false;
    });
  }
}

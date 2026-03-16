import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PublicContentService, PageData } from '../../services/public-content.service';
import { SeoService } from '../../services/seo.service';


@Component({
  selector: 'app-page-view',
  template: `
    <div class="page-enter" *ngIf="page">
      <div class="page-hero">
        <div class="container-site">
          <h1>{{ page.title }}</h1>
        </div>
      </div>
      <div class="container-site">
        <article class="page-body cms-content" [innerHTML]="page.body"></article>
      </div>
    </div>
    <div *ngIf="notFound" class="page-enter">
      <div class="container-site not-found-container">
        <i class="bi bi-geo-alt"></i>
        <h2>Page Not Found</h2>
        <p>Sorry, we couldn't find that page.</p>
        <a routerLink="/" class="btn-accent">
          <i class="bi bi-house"></i> Back to Home
        </a>
      </div>
    </div>
  `,
  styles: [`
    .page-hero {
      background: linear-gradient(135deg, var(--color-primary), var(--color-secondary));
      padding: calc(var(--header-height) + var(--space-3xl)) 0 var(--space-2xl);

      h1 {
        color: white;
        font-size: 2.5rem;
      }
    }
    .page-body {
      padding: var(--space-2xl) 0 var(--space-3xl);
      max-width: 800px;
    }
    .not-found-container {
      text-align: center;
      padding: calc(var(--header-height) + var(--space-3xl)) 0 var(--space-3xl);
      color: var(--color-text-muted);

      i { font-size: 4rem; opacity: 0.3; display: block; margin-bottom: var(--space-lg); }
      h2 { margin-bottom: var(--space-sm); }
      p { margin-bottom: var(--space-xl); }
    }
  `]
})
export class PageViewComponent implements OnInit {

  page: PageData | null = null;
  notFound: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private contentService: PublicContentService,
    private seoService: SeoService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const slug = params['slug'];
      if (slug) {
        this.loadPage(slug);
      }
    });
  }

  private loadPage(slug: string): void {
    this.page = null;
    this.notFound = false;

    this.contentService.getPageBySlug(slug).subscribe({
      next: (page) => {
        this.page = page;
        this.seoService.setPage(page.title, page.metaDescription);
      },
      error: () => {
        this.notFound = true;
        this.seoService.setPage('Page Not Found');
      }
    });
  }
}

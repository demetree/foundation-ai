import { Component, OnInit } from '@angular/core';
import { PublicContentService, PostData } from '../../services/public-content.service';
import { SeoService } from '../../services/seo.service';


@Component({
  selector: 'app-post-list',
  template: `
    <div class="page-enter">
      <!-- Header -->
      <div class="list-hero">
        <div class="container-site">
          <h1>News & Updates</h1>
          <p>Stay connected with what's happening in our community.</p>
        </div>
      </div>

      <div class="container-site section">
        <!-- Posts Grid -->
        <div class="posts-grid" *ngIf="posts.length > 0">
          <a *ngFor="let post of posts" [routerLink]="['/news', post.slug]" class="card-community post-card">
            <div class="post-card__image" *ngIf="post.featuredImageUrl"
                 [style.backgroundImage]="'url(' + post.featuredImageUrl + ')'">
            </div>
            <div class="card-community__body">
              <div class="card-community__meta">
                <i class="bi bi-calendar3 me-1"></i>
                {{ post.publishedDate | date:'mediumDate' }}
              </div>
              <h3 class="card-community__title">{{ post.title }}</h3>
              <p class="post-excerpt">{{ post.excerpt }}</p>
              <span class="read-more">Read more <i class="bi bi-arrow-right"></i></span>
            </div>
          </a>
        </div>

        <!-- Empty State -->
        <div class="no-content" *ngIf="!loading && posts.length === 0">
          <i class="bi bi-newspaper"></i>
          <p>No news posts yet. Check back soon!</p>
        </div>

        <!-- Pagination -->
        <div class="pagination-bar" *ngIf="totalPages > 1">
          <button class="btn-outline" (click)="loadPage(currentPage - 1)" [disabled]="currentPage <= 1">
            <i class="bi bi-chevron-left"></i> Previous
          </button>
          <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
          <button class="btn-outline" (click)="loadPage(currentPage + 1)" [disabled]="currentPage >= totalPages">
            Next <i class="bi bi-chevron-right"></i>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .list-hero {
      background: linear-gradient(135deg, var(--color-primary), var(--color-secondary));
      padding: calc(var(--header-height) + var(--space-3xl)) 0 var(--space-2xl);
      text-align: center;

      h1 { color: white; }
      p { color: rgba(255, 255, 255, 0.7); font-size: 1.1rem; margin-top: var(--space-sm); }
    }
    .posts-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: var(--space-xl);
    }
    .post-card {
      text-decoration: none;
      color: inherit;
      display: flex;
      flex-direction: column;
    }
    .post-card__image {
      height: 200px;
      background-size: cover;
      background-position: center;
    }
    .post-excerpt {
      color: var(--color-text-muted);
      font-size: 0.95rem;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
      margin-bottom: var(--space-md);
    }
    .read-more {
      color: var(--color-accent);
      font-weight: 600;
      font-size: 0.9rem;
    }
    .no-content {
      text-align: center;
      padding: var(--space-3xl);
      color: var(--color-text-muted);
      i { font-size: 3rem; opacity: 0.3; display: block; margin-bottom: var(--space-md); }
    }
    .pagination-bar {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: var(--space-lg);
      margin-top: var(--space-2xl);
    }
    .page-info {
      color: var(--color-text-muted);
      font-size: 0.9rem;
    }
  `]
})
export class PostListComponent implements OnInit {

  posts: PostData[] = [];
  currentPage: number = 1;
  totalPages: number = 0;
  loading: boolean = true;

  constructor(
    private contentService: PublicContentService,
    private seoService: SeoService
  ) { }

  ngOnInit(): void {
    this.seoService.setPage('News & Updates');
    this.loadPage(1);
  }

  loadPage(page: number): void {
    if (page < 1) return;
    this.loading = true;
    this.currentPage = page;

    this.contentService.getPublishedPosts(page, 9).subscribe(response => {
      this.posts = response.items || [];
      this.totalPages = response.totalPages || 0;
      this.loading = false;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    });
  }
}

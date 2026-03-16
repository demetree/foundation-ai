import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PublicContentService, PostData } from '../../services/public-content.service';
import { SeoService } from '../../services/seo.service';


@Component({
  selector: 'app-post-detail',
  template: `
    <div class="page-enter" *ngIf="post">
      <div class="post-hero">
        <div class="container-site">
          <div class="post-meta-top">
            <a routerLink="/news" class="back-link">
              <i class="bi bi-arrow-left"></i> Back to News
            </a>
          </div>
          <h1>{{ post.title }}</h1>
          <div class="post-meta">
            <span><i class="bi bi-calendar3 me-1"></i>{{ post.publishedDate | date:'longDate' }}</span>
            <span *ngIf="post.authorName"><i class="bi bi-person me-1"></i>{{ post.authorName }}</span>
          </div>
        </div>
      </div>

      <div class="container-site">
        <article class="post-body cms-content" [innerHTML]="post.body"></article>
      </div>
    </div>
  `,
  styles: [`
    .post-hero {
      background: linear-gradient(135deg, var(--color-primary), var(--color-secondary));
      padding: calc(var(--header-height) + var(--space-2xl)) 0 var(--space-2xl);

      h1 {
        color: white;
        font-size: 2.5rem;
        max-width: 800px;
      }
    }
    .post-meta-top {
      margin-bottom: var(--space-lg);
    }
    .back-link {
      color: rgba(255, 255, 255, 0.7);
      text-decoration: none;
      font-size: 0.9rem;
      display: inline-flex;
      align-items: center;
      gap: var(--space-xs);
      transition: color var(--transition-fast);

      &:hover {
        color: white;
      }
    }
    .post-meta {
      display: flex;
      gap: var(--space-lg);
      color: rgba(255, 255, 255, 0.6);
      font-size: 0.9rem;
      margin-top: var(--space-md);
    }
    .post-body {
      padding: var(--space-2xl) 0 var(--space-3xl);
      max-width: 800px;
    }
  `]
})
export class PostDetailComponent implements OnInit {

  post: PostData | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private contentService: PublicContentService,
    private seoService: SeoService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const slug = params['slug'];
      if (slug) {
        this.loadPost(slug);
      }
    });
  }

  private loadPost(slug: string): void {
    this.contentService.getPostBySlug(slug).subscribe({
      next: (post) => {
        this.post = post;
        this.seoService.setPage(post.title, post.excerpt);
        this.seoService.setOpenGraph(post.title, post.excerpt || '', post.featuredImageUrl);
      },
      error: () => {
        this.router.navigate(['/news']);
      }
    });
  }
}

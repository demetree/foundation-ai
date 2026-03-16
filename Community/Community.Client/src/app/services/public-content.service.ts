import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';


// ─────────────────────────────────────────────
// Interfaces
// ─────────────────────────────────────────────

export interface PageData {
  title: string;
  slug: string;
  body: string;
  metaDescription: string;
  isPublished: boolean;
}

export interface PostData {
  title: string;
  slug: string;
  body: string;
  excerpt: string;
  authorName: string;
  isPublished: boolean;
  publishedDate: string;
  category?: PostCategoryData;
  featuredImageUrl?: string;
}

export interface PostListResponse {
  items: PostData[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PostCategoryData {
  name: string;
  slug: string;
}

export interface AnnouncementData {
  title: string;
  body: string;
  severity: string;
  startDate: string;
  endDate: string;
  isPinned: boolean;
}

export interface MenuData {
  name: string;
  location: string;
  items: MenuItemData[];
}

export interface MenuItemData {
  label: string;
  url: string;
  pageSlug?: string;
  sortOrder: number;
  children?: MenuItemData[];
}

export interface SiteSettings {
  [key: string]: string;
}

export interface GalleryAlbumData {
  title: string;
  slug: string;
  description: string;
  coverImageUrl?: string;
  images?: GalleryImageData[];
}

export interface GalleryImageData {
  caption: string;
  url: string;
  sortOrder: number;
}

export interface ContactFormData {
  name: string;
  email: string;
  subject: string;
  message: string;
}


// ─────────────────────────────────────────────
// Service
// ─────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class PublicContentService {

  private readonly baseUrl = `${environment.apiBaseUrl}/PublicContent`;

  constructor(private http: HttpClient) { }


  // Pages
  getPageBySlug(slug: string): Observable<PageData> {
    return this.http.get<PageData>(`${this.baseUrl}/Pages/${slug}`);
  }

  getPublishedPages(): Observable<PageData[]> {
    return this.http.get<PageData[]>(`${this.baseUrl}/Pages`);
  }


  // Posts
  getPublishedPosts(page: number = 1, pageSize: number = 10, category?: string): Observable<PostListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (category) {
      params = params.set('category', category);
    }

    return this.http.get<PostListResponse>(`${this.baseUrl}/Posts`, { params });
  }

  getPostBySlug(slug: string): Observable<PostData> {
    return this.http.get<PostData>(`${this.baseUrl}/Posts/${slug}`);
  }


  // Announcements
  getActiveAnnouncements(): Observable<AnnouncementData[]> {
    return this.http.get<AnnouncementData[]>(`${this.baseUrl}/Announcements`);
  }


  // Menus
  getMenuByLocation(location: string): Observable<MenuData> {
    return this.http.get<MenuData>(`${this.baseUrl}/Menu/${location}`);
  }


  // Site Settings
  getSiteSettings(): Observable<SiteSettings> {
    return this.http.get<SiteSettings>(`${this.baseUrl}/Settings`);
  }


  // Gallery
  getGalleryAlbums(): Observable<GalleryAlbumData[]> {
    return this.http.get<GalleryAlbumData[]>(`${this.baseUrl}/Gallery`);
  }

  getGalleryAlbumBySlug(slug: string): Observable<GalleryAlbumData> {
    return this.http.get<GalleryAlbumData>(`${this.baseUrl}/Gallery/${slug}`);
  }


  // Documents
  getDocuments(category?: string): Observable<any[]> {
    let params = new HttpParams();
    if (category) {
      params = params.set('category', category);
    }
    return this.http.get<any[]>(`${this.baseUrl}/Documents`, { params });
  }


  // Contact
  submitContactForm(data: ContactFormData): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/Contact`, data);
  }
}

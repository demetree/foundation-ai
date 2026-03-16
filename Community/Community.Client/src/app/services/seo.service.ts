import { Injectable } from '@angular/core';
import { Title, Meta } from '@angular/platform-browser';


@Injectable({ providedIn: 'root' })
export class SeoService {

  private readonly defaultTitle = 'Petty Harbour-Maddox Cove';
  private readonly defaultDescription = 'Official website for the Town of Petty Harbour-Maddox Cove, Newfoundland and Labrador.';

  constructor(
    private titleService: Title,
    private metaService: Meta
  ) { }


  /**
   * Set the page title and meta description.
   */
  setPage(title?: string, description?: string): void {
    const fullTitle = title
      ? `${title} — ${this.defaultTitle}`
      : this.defaultTitle;

    this.titleService.setTitle(fullTitle);

    this.metaService.updateTag({
      name: 'description',
      content: description || this.defaultDescription
    });
  }


  /**
   * Set Open Graph tags for social sharing.
   */
  setOpenGraph(title: string, description: string, imageUrl?: string): void {
    this.metaService.updateTag({ property: 'og:title', content: title });
    this.metaService.updateTag({ property: 'og:description', content: description });
    this.metaService.updateTag({ property: 'og:type', content: 'website' });

    if (imageUrl) {
      this.metaService.updateTag({ property: 'og:image', content: imageUrl });
    }
  }
}

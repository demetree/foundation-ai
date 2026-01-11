import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BodyClassService {
  private renderer: Renderer2;

  constructor(rendererFactory: RendererFactory2) {
    this.renderer = rendererFactory.createRenderer(null, null);
  }

  addClass(className: string): void {
    this.renderer.addClass(document.body, className);
  }

  removeClass(className: string): void {
    this.renderer.removeClass(document.body, className);
  }

  replaceClass(oldClass: string, newClass: string): void {
    this.removeClass(oldClass);
    this.addClass(newClass);
  }
}

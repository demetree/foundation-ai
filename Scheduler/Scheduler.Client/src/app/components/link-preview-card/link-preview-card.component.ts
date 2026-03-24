import { Component, Input, Output, EventEmitter } from '@angular/core';
import { LinkPreviewSummary } from '../../services/messaging-api.service';


@Component({
  selector: 'app-link-preview-card',
  templateUrl: './link-preview-card.component.html',
  styleUrls: ['./link-preview-card.component.scss']
})
export class LinkPreviewCardComponent {

  @Input() preview!: LinkPreviewSummary;
  @Input() canDismiss = false;

  @Output() dismissed = new EventEmitter<number>();
  @Output() opened = new EventEmitter<string>();

  onOpen(): void {
    if (this.preview?.url) {
      window.open(this.preview.url, '_blank', 'noopener,noreferrer');
      this.opened.emit(this.preview.url);
    }
  }

  onDismiss(event: MouseEvent): void {
    event.stopPropagation();
    this.dismissed.emit(this.preview.id);
  }

  getDomain(): string {
    try {
      const url = new URL(this.preview.url);
      return url.hostname.replace('www.', '');
    } catch {
      return '';
    }
  }
}

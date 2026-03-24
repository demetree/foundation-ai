import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';


/**
 * MessageFormatPipe
 * ─────────────────
 * Transforms message text into displayable HTML.
 *
 * When the message source is the Tiptap rich text editor, messages are already
 * valid HTML and are passed through with sanitisation bypass (safe because the
 * server sanitises on ingest).
 *
 * Supports:
 *   HTML content  → passed through (from Tiptap editor)
 *   Plain text    → legacy regex formatting (mentions, bold, italic, code, links)
 *
 * Usage:
 *   <span [innerHTML]="msg.message | messageFormat:currentUserDisplayName"></span>
 */
@Pipe({
  name: 'messageFormat'
})
export class MessageFormatPipe implements PipeTransform {

  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string | null, currentUserDisplayName?: string): SafeHtml {
    if (!value) return '';

    //
    // If the content starts with an HTML tag, treat it as rich text from Tiptap.
    // Just pass through with bypassSecurityTrustHtml (server-side sanitised).
    //
    if (this.isHtmlContent(value)) {
      // Still highlight self-mentions within the HTML
      let html = value;
      if (currentUserDisplayName) {
        const escapedName = currentUserDisplayName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        const mentionRegex = new RegExp(
          `(<span[^>]*class="mention"[^>]*>@)(${escapedName})(</span>)`,
          'gi'
        );
        html = html.replace(mentionRegex, (_match, before, name, after) => {
          return before.replace('class="mention"', 'class="mention mention-self"') + name + after;
        });
      }
      return this.sanitizer.bypassSecurityTrustHtml(html);
    }

    //
    // Legacy plain text formatting (pre-Tiptap messages)
    //
    let html = this.escapeHtml(value);

    // 1. @mentions — @[Display Name]
    html = html.replace(/@\[([^\]]+)\]/g, (_match: string, name: string) => {
      const isSelf = currentUserDisplayName &&
        name.toLowerCase() === currentUserDisplayName.toLowerCase();
      const classes = isSelf ? 'mention mention-self' : 'mention';
      return `<span class="${classes}">@${name}</span>`;
    });

    // 2. Code (inline) — `code`
    html = html.replace(/`([^`]+)`/g, '<code class="inline-code">$1</code>');

    // 3. Bold — **text**
    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');

    // 4. Italic — *text* (but not inside bold **)
    html = html.replace(/(?<!\*)\*([^*]+)\*(?!\*)/g, '<em>$1</em>');

    // 5. URLs — auto-link http/https
    html = html.replace(
      /(https?:\/\/[^\s<]+)/g,
      '<a href="$1" target="_blank" rel="noopener noreferrer" class="message-link">$1</a>'
    );

    // 6. Newlines → <br>
    html = html.replace(/\n/g, '<br>');

    return this.sanitizer.bypassSecurityTrustHtml(html);
  }


  /**
   * Detect whether content is HTML (from Tiptap) vs plain text.
   * Tiptap always wraps content in tags like <p>, <ul>, <h1>, etc.
   */
  private isHtmlContent(text: string): boolean {
    return /^\s*<[a-z][\s\S]*>/i.test(text);
  }

  private escapeHtml(text: string): string {
    const div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
  }
}

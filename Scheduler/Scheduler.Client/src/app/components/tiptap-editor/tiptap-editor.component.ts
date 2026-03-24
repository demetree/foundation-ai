import { Component, OnDestroy, ElementRef, ViewChild, AfterViewInit, EventEmitter, Output, Input, ViewEncapsulation } from '@angular/core';
import { Editor } from '@tiptap/core';
import StarterKit from '@tiptap/starter-kit';
import Link from '@tiptap/extension-link';
import Placeholder from '@tiptap/extension-placeholder';

@Component({
  selector: 'app-tiptap-editor',
  templateUrl: './tiptap-editor.component.html',
  styleUrls: ['./tiptap-editor.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TiptapEditorComponent implements AfterViewInit, OnDestroy {

  @ViewChild('editorContainer', { static: true }) editorContainer!: ElementRef;

  @Input() placeholder = 'Type a message...';
  @Input() content = '';

  /** Emits the current HTML every time content changes */
  @Output() contentChanged = new EventEmitter<string>();

  /** Emits when the user presses Enter (without Shift) to send */
  @Output() enterPressed = new EventEmitter<void>();

  /** Emits when @ is typed — carries the query text so the parent can show the mention picker */
  @Output() mentionTriggered = new EventEmitter<{ query: string; from: number }>();

  /** Emits when mention context is lost (cursor moved away from @) */
  @Output() mentionDismissed = new EventEmitter<void>();

  /** Emits whenever a key is pressed — for typing indicator */
  @Output() keyPressed = new EventEmitter<void>();

  editor!: Editor;

  private mentionActive = false;
  private mentionFrom = 0;

  ngAfterViewInit(): void {
    this.editor = new Editor({
      element: this.editorContainer.nativeElement,
      extensions: [
        StarterKit.configure({
          heading: { levels: [1, 2, 3] },
          codeBlock: { HTMLAttributes: { class: 'code-block' } },
          code: { HTMLAttributes: { class: 'inline-code' } },
        }),
        Link.configure({
          openOnClick: false,
          autolink: true,
          HTMLAttributes: { target: '_blank', rel: 'noopener noreferrer' },
        }),
        Placeholder.configure({
          placeholder: this.placeholder,
        }),
      ],
      content: this.content,
      editorProps: {
        handleKeyDown: (_view, event) => {
          // Enter without shift = send
          if (event.key === 'Enter' && !event.shiftKey) {
            // Don't send if a list or blockquote is active (let Tiptap handle continuation)
            if (this.editor.isActive('bulletList') || this.editor.isActive('orderedList') || this.editor.isActive('codeBlock')) {
              return false; // let Tiptap handle it
            }
            event.preventDefault();
            this.enterPressed.emit();
            return true;
          }
          return false;
        },
      },
      onUpdate: ({ editor }) => {
        const html = editor.getHTML();
        this.contentChanged.emit(html);
        this.checkMentionTrigger(editor);
      },
      onSelectionUpdate: ({ editor }) => {
        this.checkMentionTrigger(editor);
      },
      onCreate: () => {
        // emit typing indicator for any key
      },
    });

    // Detect keystrokes for typing indicator
    this.editorContainer.nativeElement.addEventListener('keydown', () => {
      this.keyPressed.emit();
    });
  }


  // ─── Toolbar Actions ──────────────────────────────────────────────────────

  toggleBold(): void { this.editor.chain().focus().toggleBold().run(); }
  toggleItalic(): void { this.editor.chain().focus().toggleItalic().run(); }
  toggleStrike(): void { this.editor.chain().focus().toggleStrike().run(); }
  toggleCode(): void { this.editor.chain().focus().toggleCode().run(); }
  toggleCodeBlock(): void { this.editor.chain().focus().toggleCodeBlock().run(); }
  toggleBulletList(): void { this.editor.chain().focus().toggleBulletList().run(); }
  toggleOrderedList(): void { this.editor.chain().focus().toggleOrderedList().run(); }
  toggleBlockquote(): void { this.editor.chain().focus().toggleBlockquote().run(); }

  insertLink(): void {
    const url = prompt('Enter URL:');
    if (url) {
      this.editor.chain().focus().setLink({ href: url }).run();
    }
  }

  removeLink(): void {
    this.editor.chain().focus().unsetLink().run();
  }


  // ─── Public API for parent ────────────────────────────────────────────────

  /** Get the HTML content */
  getHTML(): string {
    return this.editor?.getHTML() ?? '';
  }

  /** Get plain text content (for fallback / empty checks) */
  getText(): string {
    return this.editor?.getText() ?? '';
  }

  /** Check if editor has meaningful content */
  isEmpty(): boolean {
    return this.editor?.isEmpty ?? true;
  }

  /** Clear the editor */
  clearContent(): void {
    this.editor?.commands.clearContent(true);
  }

  /** Focus the editor */
  focus(): void {
    this.editor?.commands.focus();
  }

  /** Set content (e.g. for editing existing messages) */
  setContent(html: string): void {
    this.editor?.commands.setContent(html);
  }

  /** Insert a mention token at the current cursor position */
  insertMentionAtCursor(displayName: string): void {
    if (!this.editor || !this.mentionActive) return;

    const { state } = this.editor;
    const { from } = state.selection;

    // Delete from the @ character to current cursor
    this.editor
      .chain()
      .focus()
      .deleteRange({ from: this.mentionFrom, to: from })
      .insertContent(`<span class="mention" data-mention="${displayName}">@${displayName}</span>&nbsp;`)
      .run();

    this.mentionActive = false;
    this.mentionDismissed.emit();
  }


  // ─── @Mention detection ───────────────────────────────────────────────────

  private checkMentionTrigger(editor: Editor): void {
    const { state } = editor;
    const { from } = state.selection;

    // Get text before cursor in current text block
    const $pos = state.doc.resolve(from);
    const textBefore = $pos.parent.textBetween(0, $pos.parentOffset, undefined, '\ufffc');

    const mentionMatch = textBefore.match(/(^|\s)@([\w\s]*)$/);

    if (mentionMatch) {
      const query = mentionMatch[2].toLowerCase();
      const matchStart = from - mentionMatch[2].length - 1; // position of the @

      if (!this.mentionActive || this.mentionFrom !== matchStart) {
        this.mentionActive = true;
        this.mentionFrom = matchStart;
      }

      this.mentionTriggered.emit({ query, from: matchStart });
    } else if (this.mentionActive) {
      this.mentionActive = false;
      this.mentionDismissed.emit();
    }
  }


  ngOnDestroy(): void {
    this.editor?.destroy();
  }
}

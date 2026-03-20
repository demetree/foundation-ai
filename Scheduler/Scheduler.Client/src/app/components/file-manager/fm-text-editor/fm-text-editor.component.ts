//
// fm-text-editor.component.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Reusable markdown text editor component with auto-save, preview mode,
// and version history integration.  Uses the FileManager Content endpoints
// to load and save document text.
//
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef, HostListener } from '@angular/core';
import { Subject, Subscription, interval } from 'rxjs';
import { debounceTime, switchMap, takeUntil } from 'rxjs/operators';
import { FileManagerService, DocumentDTO } from '../../../services/file-manager.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';


@Component({
    selector: 'fm-text-editor',
    templateUrl: './fm-text-editor.component.html',
    styleUrls: ['./fm-text-editor.component.scss']
})
export class FmTextEditorComponent implements OnInit, OnDestroy {

    @Input() documentId!: number;
    @Input() autoSaveInterval = 10;    // seconds
    @Input() readOnly = false;

    @Output() saved = new EventEmitter<DocumentDTO>();
    @Output() contentChanged = new EventEmitter<boolean>();

    @ViewChild('editorTextarea') editorTextarea!: ElementRef<HTMLTextAreaElement>;

    // Editor state
    content = '';
    originalContent = '';
    isDirty = false;
    isLoading = true;
    isSaving = false;
    versionNumber = 0;

    // Auto-save countdown
    countdown = 0;
    private countdownInterval: any = null;
    private autoSaveTimeout: any = null;

    // Preview
    showPreview = false;
    previewHtml = '';

    // Version history
    showVersions = false;
    showHelp = false;
    versions: any[] = [];
    isLoadingVersions = false;

    // Undo/redo stacks
    undoStack: string[] = [];
    redoStack: string[] = [];
    private lastSnapshot = '';

    // Cursor position
    cursorLine = 1;
    cursorCol = 1;

    private destroy$ = new Subject<void>();

    constructor(
        private fileManagerService: FileManagerService,
        private alertService: AlertService
    ) {}


    ngOnInit(): void {
        this.loadContent();
    }


    ngOnDestroy(): void {
        // Auto-save any unsaved changes when leaving (tab switch, navigation, etc.)
        if (this.isDirty && !this.isSaving && !this.readOnly) {
            this.save();
        }
        this.destroy$.next();
        this.destroy$.complete();
        this.clearTimers();
    }


    // ─── Content Loading ─────────────────────────────────────────────

    loadContent(): void {
        this.isLoading = true;
        this.fileManagerService.getDocumentContent(this.documentId).subscribe({
            next: (result) => {
                this.content = result.content;
                this.originalContent = result.content;
                this.versionNumber = result.versionNumber;
                this.lastSnapshot = result.content;
                this.isDirty = false;
                this.isLoading = false;
                this.contentChanged.emit(false);
            },
            error: (err) => {
                console.error('Error loading document content', err);
                this.alertService.showMessage('Error', 'Could not load document content.', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    // ─── Editor Input ────────────────────────────────────────────────

    onContentInput(): void {
        if (this.readOnly) return;

        // Track dirty state
        const wasDirty = this.isDirty;
        this.isDirty = this.content !== this.originalContent;
        if (wasDirty !== this.isDirty) {
            this.contentChanged.emit(this.isDirty);
        }

        // Update preview if visible
        if (this.showPreview) {
            this.renderPreview();
        }

        // Reset auto-save timer
        if (this.isDirty) {
            this.startAutoSaveCountdown();
        } else {
            this.clearTimers();
        }
    }

    onKeyDown(event: KeyboardEvent): void {
        // Ctrl+S / Cmd+S → save now
        if ((event.ctrlKey || event.metaKey) && event.key === 's') {
            event.preventDefault();
            this.saveNow();
            return;
        }

        // Ctrl+Z → undo
        if ((event.ctrlKey || event.metaKey) && event.key === 'z' && !event.shiftKey) {
            event.preventDefault();
            this.undo();
            return;
        }

        // Ctrl+Shift+Z or Ctrl+Y → redo
        if ((event.ctrlKey || event.metaKey) && (event.key === 'y' || (event.key === 'z' && event.shiftKey))) {
            event.preventDefault();
            this.redo();
            return;
        }

        // Tab → insert spaces
        if (event.key === 'Tab') {
            event.preventDefault();
            this.insertAtCursor('    ');
        }
    }

    onCursorMove(): void {
        if (!this.editorTextarea?.nativeElement) return;
        const ta = this.editorTextarea.nativeElement;
        const pos = ta.selectionStart;
        const textBefore = this.content.substring(0, pos);
        const lines = textBefore.split('\n');
        this.cursorLine = lines.length;
        this.cursorCol = lines[lines.length - 1].length + 1;
    }

    /** Snapshot for undo on significant changes (called periodically). */
    takeSnapshot(): void {
        if (this.content !== this.lastSnapshot) {
            this.undoStack.push(this.lastSnapshot);
            this.redoStack = [];
            if (this.undoStack.length > 100) {
                this.undoStack.shift();
            }
            this.lastSnapshot = this.content;
        }
    }

    undo(): void {
        if (this.undoStack.length === 0) return;
        this.takeSnapshot();
        this.redoStack.push(this.content);
        this.content = this.undoStack.pop()!;
        this.lastSnapshot = this.content;
        this.onContentInput();
    }

    redo(): void {
        if (this.redoStack.length === 0) return;
        this.undoStack.push(this.content);
        this.content = this.redoStack.pop()!;
        this.lastSnapshot = this.content;
        this.onContentInput();
    }


    // ─── Auto-Save ───────────────────────────────────────────────────

    private startAutoSaveCountdown(): void {
        this.clearTimers();
        this.countdown = this.autoSaveInterval;
        this.takeSnapshot();

        // Countdown ticks every second
        this.countdownInterval = setInterval(() => {
            this.countdown--;
            if (this.countdown <= 0) {
                this.clearTimers();
                this.performAutoSave();
            }
        }, 1000);
    }

    private performAutoSave(): void {
        if (!this.isDirty || this.isSaving) return;
        this.save();
    }

    saveNow(): void {
        if (!this.isDirty || this.isSaving) return;
        this.clearTimers();
        this.save();
    }

    private save(): void {
        this.isSaving = true;
        this.fileManagerService.saveDocumentContent(this.documentId, this.content).subscribe({
            next: (doc) => {
                this.versionNumber = doc.versionNumber;
                this.originalContent = this.content;
                this.isDirty = false;
                this.isSaving = false;
                this.countdown = 0;
                this.contentChanged.emit(false);
                this.saved.emit(doc);
            },
            error: (err) => {
                console.error('Error saving document content', err);
                this.alertService.showMessage('Save Failed', 'Could not save. Will retry on next auto-save.', MessageSeverity.error);
                this.isSaving = false;
                // Restart countdown to retry
                this.startAutoSaveCountdown();
            }
        });
    }

    private clearTimers(): void {
        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
            this.countdownInterval = null;
        }
        if (this.autoSaveTimeout) {
            clearTimeout(this.autoSaveTimeout);
            this.autoSaveTimeout = null;
        }
        this.countdown = 0;
    }


    // ─── Markdown Preview ────────────────────────────────────────────

    togglePreview(): void {
        this.showPreview = !this.showPreview;
        if (this.showPreview) {
            this.renderPreview();
        }
    }

    /**
     * Lightweight markdown → HTML renderer.
     * Handles headings, bold, italic, code blocks, inline code, links, lists,
     * blockquotes, and horizontal rules.
     */
    renderPreview(): void {
        let html = this.escapeHtml(this.content);

        // Fenced code blocks (```...```)
        html = html.replace(/```(\w*)\n([\s\S]*?)```/g, '<pre><code class="lang-$1">$2</code></pre>');

        // Headings (# to ######)
        html = html.replace(/^######\s+(.+)$/gm, '<h6>$1</h6>');
        html = html.replace(/^#####\s+(.+)$/gm, '<h5>$1</h5>');
        html = html.replace(/^####\s+(.+)$/gm, '<h4>$1</h4>');
        html = html.replace(/^###\s+(.+)$/gm, '<h3>$1</h3>');
        html = html.replace(/^##\s+(.+)$/gm, '<h2>$1</h2>');
        html = html.replace(/^#\s+(.+)$/gm, '<h1>$1</h1>');

        // Horizontal rules
        html = html.replace(/^---+$/gm, '<hr>');

        // Blockquotes
        html = html.replace(/^&gt;\s+(.+)$/gm, '<blockquote>$1</blockquote>');

        // Bold and italic
        html = html.replace(/\*\*\*(.+?)\*\*\*/g, '<strong><em>$1</em></strong>');
        html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
        html = html.replace(/\*(.+?)\*/g, '<em>$1</em>');

        // Inline code
        html = html.replace(/`([^`]+)`/g, '<code>$1</code>');

        // Links [text](url)
        html = html.replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>');

        // Unordered lists
        html = html.replace(/^[-*]\s+(.+)$/gm, '<li>$1</li>');
        html = html.replace(/(<li>.*<\/li>\n?)+/g, '<ul>$&</ul>');

        // Numbered lists
        html = html.replace(/^\d+\.\s+(.+)$/gm, '<li>$1</li>');

        // Paragraphs (double newlines)
        html = html.replace(/\n\n/g, '</p><p>');
        html = '<p>' + html + '</p>';

        // Clean up empty paragraphs
        html = html.replace(/<p><\/p>/g, '');
        html = html.replace(/<p>(<h[1-6]>)/g, '$1');
        html = html.replace(/(<\/h[1-6]>)<\/p>/g, '$1');
        html = html.replace(/<p>(<hr>)<\/p>/g, '$1');
        html = html.replace(/<p>(<pre>)/g, '$1');
        html = html.replace(/(<\/pre>)<\/p>/g, '$1');
        html = html.replace(/<p>(<ul>)/g, '$1');
        html = html.replace(/(<\/ul>)<\/p>/g, '$1');
        html = html.replace(/<p>(<blockquote>)/g, '$1');
        html = html.replace(/(<\/blockquote>)<\/p>/g, '$1');

        // Single newlines → <br>
        html = html.replace(/\n/g, '<br>');

        this.previewHtml = html;
    }

    private escapeHtml(text: string): string {
        return text
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }


    // ─── Version History ─────────────────────────────────────────────

    toggleVersionHistory(): void {
        this.showVersions = !this.showVersions;
        if (this.showVersions && this.versions.length === 0) {
            this.loadVersions();
        }
    }

    private loadVersions(): void {
        this.isLoadingVersions = true;
        this.fileManagerService.getVersions(this.documentId).subscribe({
            next: (versions) => {
                this.versions = versions;
                this.isLoadingVersions = false;
            },
            error: (err) => {
                console.error('Error loading versions', err);
                this.isLoadingVersions = false;
            }
        });
    }

    /** Refreshes version list (called after a save). */
    refreshVersions(): void {
        if (this.showVersions) {
            this.loadVersions();
        }
    }


    // ─── Helpers ─────────────────────────────────────────────────────

    private insertAtCursor(text: string): void {
        if (!this.editorTextarea?.nativeElement) return;
        const ta = this.editorTextarea.nativeElement;
        const start = ta.selectionStart;
        const end = ta.selectionEnd;
        this.takeSnapshot();
        this.content = this.content.substring(0, start) + text + this.content.substring(end);
        // Set cursor position after inserted text
        setTimeout(() => {
            ta.selectionStart = ta.selectionEnd = start + text.length;
            ta.focus();
        });
        this.onContentInput();
    }

    /** Format countdown as MM:SS or just seconds. */
    get countdownDisplay(): string {
        if (this.countdown <= 0) return '';
        if (this.countdown >= 60) {
            const m = Math.floor(this.countdown / 60);
            const s = this.countdown % 60;
            return `${m}:${s.toString().padStart(2, '0')}`;
        }
        return `${this.countdown}s`;
    }

    get saveStatus(): string {
        if (this.isSaving) return 'Saving…';
        if (this.isDirty) return 'Unsaved changes';
        if (this.versionNumber > 0) return `Saved v${this.versionNumber}`;
        return '';
    }

    /** Warn when leaving with unsaved changes. */
    @HostListener('window:beforeunload', ['$event'])
    onBeforeUnload(event: BeforeUnloadEvent): void {
        if (this.isDirty) {
            event.preventDefault();
            event.returnValue = '';
        }
    }
}

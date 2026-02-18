import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AiService, AiSearchResult, AiChatResponse } from '../../services/ai.service';
import { AiChatSignalrService } from '../../services/ai-chat-signalr.service';


interface ChatMessage {
    role: 'user' | 'assistant' | 'error';
    content: string;
    sources?: { docId: string; excerpt: string; score: number }[];
    isStreaming?: boolean;
}


@Component({
    selector: 'app-ai-assistant',
    templateUrl: './ai-assistant.component.html',
    styleUrl: './ai-assistant.component.scss'
})
export class AiAssistantComponent implements OnInit, OnDestroy {

    @ViewChild('chatScroll') chatScrollRef!: ElementRef;

    // Tab state
    activeTab: 'search' | 'chat' = 'search';

    // Search state
    searchQuery = '';
    searchMode: 'parts' | 'sets' = 'parts';
    searchResults: AiSearchResult[] = [];
    isSearching = false;
    searchError: string | null = null;

    // Chat state
    chatInput = '';
    chatMessages: ChatMessage[] = [];
    isChatConnected = false;
    isChatStreaming = false;

    private destroy$ = new Subject<void>();

    constructor(
        private aiService: AiService,
        private signalr: AiChatSignalrService
    ) { }

    ngOnInit(): void {
        // Wire up SignalR events
        this.signalr.onToken$
            .pipe(takeUntil(this.destroy$))
            .subscribe(token => this.handleToken(token));

        this.signalr.onComplete$
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => this.handleChatComplete());

        this.signalr.onError$
            .pipe(takeUntil(this.destroy$))
            .subscribe(err => this.handleChatError(err));

        this.signalr.onConnectionChange$
            .pipe(takeUntil(this.destroy$))
            .subscribe(connected => this.isChatConnected = connected);
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.signalr.disconnect();
    }


    // ───────────────────────────── Search ─────────────────────────────

    search(): void {
        if (!this.searchQuery.trim()) return;

        this.isSearching = true;
        this.searchError = null;
        this.searchResults = [];

        const obs = this.searchMode === 'parts'
            ? this.aiService.searchParts(this.searchQuery)
            : this.aiService.searchSets(this.searchQuery);

        obs.subscribe({
            next: (results) => {
                this.searchResults = results;
                this.isSearching = false;
            },
            error: (err) => {
                this.searchError = err?.error?.message || 'Search failed. Please try again.';
                this.isSearching = false;
            }
        });
    }

    onSearchKeydown(event: KeyboardEvent): void {
        if (event.key === 'Enter') {
            this.search();
        }
    }

    getScorePercent(score: number): number {
        return Math.round(score * 100);
    }

    getScoreClass(score: number): string {
        if (score >= 0.8) return 'score-high';
        if (score >= 0.5) return 'score-mid';
        return 'score-low';
    }


    // ───────────────────────────── Chat ─────────────────────────────

    async sendChat(): Promise<void> {
        const question = this.chatInput.trim();
        if (!question || this.isChatStreaming) return;

        // Add user message
        this.chatMessages.push({ role: 'user', content: question });
        this.chatInput = '';

        // Add placeholder for AI response
        this.chatMessages.push({ role: 'assistant', content: '', isStreaming: true });
        this.isChatStreaming = true;

        this.scrollToBottom();

        await this.signalr.sendMessage(question);
    }

    onChatKeydown(event: KeyboardEvent): void {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            this.sendChat();
        }
    }

    private handleToken(token: string): void {
        const lastMsg = this.chatMessages[this.chatMessages.length - 1];
        if (lastMsg?.role === 'assistant' && lastMsg.isStreaming) {
            lastMsg.content += token;
            this.scrollToBottom();
        }
    }

    private handleChatComplete(): void {
        const lastMsg = this.chatMessages[this.chatMessages.length - 1];
        if (lastMsg?.role === 'assistant') {
            lastMsg.isStreaming = false;
        }
        this.isChatStreaming = false;
    }

    private handleChatError(message: string): void {
        const lastMsg = this.chatMessages[this.chatMessages.length - 1];

        // If we have a streaming placeholder, replace it with the error
        if (lastMsg?.role === 'assistant' && lastMsg.isStreaming && !lastMsg.content) {
            lastMsg.role = 'error';
            lastMsg.content = message;
            lastMsg.isStreaming = false;
        } else {
            this.chatMessages.push({ role: 'error', content: message });
        }

        this.isChatStreaming = false;
        this.scrollToBottom();
    }

    clearChat(): void {
        this.chatMessages = [];
    }

    private scrollToBottom(): void {
        setTimeout(() => {
            const el = this.chatScrollRef?.nativeElement;
            if (el) {
                el.scrollTop = el.scrollHeight;
            }
        }, 50);
    }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// AI Service
//
// Angular service for the AiController endpoints.
// Provides semantic search over parts/sets, synchronous RAG chat,
// and an admin-only index trigger.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface AiSearchResult {
    id: string;
    score: number;
    name: string;
    type: string;
    category: string;
    year: string;
}

export interface AiChatSource {
    docId: string;
    excerpt: string;
    score: number;
}

export interface AiChatResponse {
    answer: string;
    sources: AiChatSource[];
}

export interface AiIndexResult {
    success: boolean;
    message: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class AiService {
    private readonly baseUrl = '/api/ai';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }

    /** Semantic search for parts matching a natural language query. */
    searchParts(query: string, topK: number = 10): Observable<AiSearchResult[]> {
        let params = new HttpParams()
            .set('query', query)
            .set('topK', topK.toString());

        return this.http.get<AiSearchResult[]>(
            `${this.baseUrl}/search/parts`,
            { headers: this.headers, params }
        );
    }

    /** Semantic search for sets matching a natural language query. */
    searchSets(query: string, topK: number = 10): Observable<AiSearchResult[]> {
        let params = new HttpParams()
            .set('query', query)
            .set('topK', topK.toString());

        return this.http.get<AiSearchResult[]>(
            `${this.baseUrl}/search/sets`,
            { headers: this.headers, params }
        );
    }

    /** Synchronous RAG chat (returns full answer at once). */
    chat(question: string): Observable<AiChatResponse> {
        return this.http.post<AiChatResponse>(
            `${this.baseUrl}/chat`,
            { question },
            { headers: this.headers }
        );
    }

    /** Admin-only: trigger re-indexing of parts and sets. */
    triggerIndex(): Observable<AiIndexResult> {
        return this.http.post<AiIndexResult>(
            `${this.baseUrl}/index`,
            {},
            { headers: this.headers }
        );
    }
}

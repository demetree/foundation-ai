import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Project Service
//
// Custom Angular service for MOC project management.
// Calls the custom MocImportController endpoints and auto-generated Project CRUD.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface ProjectSummary {
    id: number;
    name: string;
    description: string;
    notes: string;
    thumbnailImagePath: string | null;
    partCount: number | null;
    lastBuildDate: string | null;
    versionNumber: number;
    userId: number | null;
}

export interface UploadResult {
    projectId: number;
    projectName: string;
    totalPartCount: number;
    submodelCount: number;
    stepCount: number;
    resolvedPartCount: number;
    unresolvedPartCount: number;
    unresolvedParts: string[];
    sourceFormat: string;
}

export interface ImportFormat {
    extension: string;
    name: string;
    description: string;
    mimeType: string;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class ProjectService {
    private readonly projectUrl = '/api/Project';
    private readonly importUrl = '/api/moc/import';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** GET /api/Project — all projects for the current tenant */
    getMyProjects(): Observable<ProjectSummary[]> {
        return this.http.get<ProjectSummary[]>(
            this.projectUrl,
            { headers: this.headers }
        );
    }


    /** POST /api/moc/import/upload — upload a model file (.ldr, .mpd, .io) */
    uploadModel(file: File): Observable<HttpEvent<UploadResult>> {
        const formData = new FormData();
        formData.append('file', file, file.name);

        const req = new HttpRequest('POST', `${this.importUrl}/upload`, formData, {
            headers: this.headers,
            reportProgress: true
        });

        return this.http.request<UploadResult>(req);
    }


    /** GET /api/moc/import/formats — supported import formats */
    getSupportedFormats(): Observable<ImportFormat[]> {
        return this.http.get<ImportFormat[]>(
            `${this.importUrl}/formats`,
            { headers: this.headers }
        );
    }


    /** DELETE /api/Project/{id} — soft-delete a project */
    deleteProject(id: number): Observable<any> {
        return this.http.delete(
            `${this.projectUrl}/${id}`,
            { headers: this.headers }
        );
    }
}

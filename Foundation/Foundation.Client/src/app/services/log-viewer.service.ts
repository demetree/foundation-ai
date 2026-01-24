import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Log Viewer Service
//
// Angular service for interacting with the LogViewer API endpoints.
//

export interface LogFolder {
    name: string;
}

export interface LogFileInfo {
    fileName: string;
    lastModified: Date;
    sizeBytes: number;
    sizeDisplay: string;
    errorCount: number;
    warningCount: number;
}

export interface LogEntry {
    timestamp: Date;
    timeOfDay: string;
    level: string;
    threadName: string;
    message: string;
    fileName: string;
    lineNumber: number;
}

export interface LogEntriesResponse {
    entries: LogEntry[];
    totalCount: number;
    levelCounts: { [level: string]: number };
    skip: number;
    take: number;
}

@Injectable({
    providedIn: 'root'
})
export class LogViewerService {
    private readonly baseUrl = '/api/LogViewer';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    /**
     * Get list of configured log folders
     */
    getFolders(): Observable<LogFolder[]> {
        return this.http.get<LogFolder[]>(
            `${this.baseUrl}/folders`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get list of log files in a folder
     */
    getFiles(folderName: string): Observable<LogFileInfo[]> {
        return this.http.get<LogFileInfo[]>(
            `${this.baseUrl}/files/${encodeURIComponent(folderName)}`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get log entries from a file with filtering and pagination
     */
    getEntries(
        folderName: string,
        fileName: string,
        skip: number = 0,
        take: number = 100,
        level?: string,
        search?: string
    ): Observable<LogEntriesResponse> {
        let params = new HttpParams()
            .set('skip', skip.toString())
            .set('take', take.toString());

        if (level && level !== 'All') {
            params = params.set('level', level);
        }
        if (search) {
            params = params.set('search', search);
        }

        return this.http.get<LogEntriesResponse>(
            `${this.baseUrl}/entries/${encodeURIComponent(folderName)}/${encodeURIComponent(fileName)}`,
            { params, headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get most recent log entries (for live tailing)
     */
    tail(
        folderName: string,
        fileName: string,
        count: number = 50,
        level?: string
    ): Observable<{ entries: LogEntry[]; totalCount: number }> {
        let params = new HttpParams().set('count', count.toString());

        if (level && level !== 'All') {
            params = params.set('level', level);
        }

        return this.http.get<{ entries: LogEntry[]; totalCount: number }>(
            `${this.baseUrl}/tail/${encodeURIComponent(folderName)}/${encodeURIComponent(fileName)}`,
            { params, headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Download a single log file (returns blob for saving)
     */
    downloadFile(folderName: string, fileName: string): Observable<Blob> {
        return this.http.get(
            `${this.baseUrl}/download/${encodeURIComponent(folderName)}/${encodeURIComponent(fileName)}`,
            {
                headers: this.authService.GetAuthenticationHeaders(),
                responseType: 'blob'
            }
        );
    }

    /**
     * Download all log files in a folder as ZIP (returns blob for saving)
     */
    downloadAllFiles(folderName: string): Observable<Blob> {
        return this.http.get(
            `${this.baseUrl}/download-all/${encodeURIComponent(folderName)}`,
            {
                headers: this.authService.GetAuthenticationHeaders(),
                responseType: 'blob'
            }
        );
    }


    //
    // ============================================================================
    // Remote Application Log Viewing
    // ============================================================================
    //

    /**
     * Get list of remote applications that support log viewing
     */
    getRemoteApplications(): Observable<RemoteApplication[]> {
        return this.http.get<RemoteApplication[]>(
            `${this.baseUrl}/applications`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get log folders from a remote application
     */
    getRemoteFolders(appName: string): Observable<LogFolder[]> {
        return this.http.get<LogFolder[]>(
            `${this.baseUrl}/remote/${encodeURIComponent(appName)}/folders`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get log files from a remote application's folder
     */
    getRemoteFiles(appName: string, folderName: string): Observable<LogFileInfo[]> {
        return this.http.get<LogFileInfo[]>(
            `${this.baseUrl}/remote/${encodeURIComponent(appName)}/files/${encodeURIComponent(folderName)}`,
            { headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get log entries from a remote application's file
     */
    getRemoteEntries(
        appName: string,
        folderName: string,
        fileName: string,
        skip: number = 0,
        take: number = 100,
        level?: string,
        search?: string
    ): Observable<LogEntriesResponse> {
        let params = new HttpParams()
            .set('skip', skip.toString())
            .set('take', take.toString());

        if (level && level !== 'All') {
            params = params.set('level', level);
        }
        if (search) {
            params = params.set('search', search);
        }

        return this.http.get<LogEntriesResponse>(
            `${this.baseUrl}/remote/${encodeURIComponent(appName)}/entries/${encodeURIComponent(folderName)}/${encodeURIComponent(fileName)}`,
            { params, headers: this.authService.GetAuthenticationHeaders() }
        );
    }

    /**
     * Get recent log entries from a remote application's file (live tail)
     */
    tailRemote(
        appName: string,
        folderName: string,
        fileName: string,
        count: number = 50
    ): Observable<{ entries: LogEntry[]; totalCount: number }> {
        let params = new HttpParams().set('lines', count.toString());

        return this.http.get<{ entries: LogEntry[]; totalCount: number }>(
            `${this.baseUrl}/remote/${encodeURIComponent(appName)}/tail/${encodeURIComponent(folderName)}/${encodeURIComponent(fileName)}`,
            { params, headers: this.authService.GetAuthenticationHeaders() }
        );
    }
}


//
// Remote Application Interface
//
export interface RemoteApplication {
    name: string;
    url: string;
    isSelf: boolean;
}

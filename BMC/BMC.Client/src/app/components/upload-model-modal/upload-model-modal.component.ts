import { Component, NgZone } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ProjectService, UploadResult } from '../../services/project.service';


type ModalState = 'select' | 'uploading' | 'success' | 'error';


@Component({
    selector: 'app-upload-model-modal',
    templateUrl: './upload-model-modal.component.html',
    styleUrl: './upload-model-modal.component.scss'
})
export class UploadModelModalComponent {

    state: ModalState = 'select';
    selectedFile: File | null = null;
    uploadProgress = 0;
    statusMessage = '';
    result: UploadResult | null = null;
    errorMessage = '';

    //
    // Track whether any upload succeeded during this modal session.
    // This ensures the parent refreshes the project list even if the user
    // clicks "Upload Another" and then dismisses instead of clicking "Done".
    //
    hasSuccessfulUpload = false;

    // Drag state
    isDragOver = false;

    // Validation
    readonly maxFileSize = 50 * 1024 * 1024; // 50MB
    readonly acceptedExtensions = ['.ldr', '.mpd', '.io', '.lxf'];


    constructor(
        public activeModal: NgbActiveModal,
        private projectService: ProjectService,
        private ngZone: NgZone
    ) { }


    // ───────────────────── Drag & Drop ─────────────────────

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = true;
    }

    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.isDragOver = false;

        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.selectFile(files[0]);
        }
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            this.selectFile(input.files[0]);
        }
    }


    // ───────────────────── File Validation ─────────────────────

    selectFile(file: File): void {
        // Validate extension
        const ext = this.getExtension(file.name);
        if (!this.acceptedExtensions.includes(ext)) {
            this.errorMessage = `Unsupported file type "${ext}". Accepted formats: ${this.acceptedExtensions.join(', ')}`;
            this.state = 'error';
            return;
        }

        // Validate size
        if (file.size > this.maxFileSize) {
            this.errorMessage = `File is too large (${this.formatFileSize(file.size)}). Maximum size is 50 MB.`;
            this.state = 'error';
            return;
        }

        this.selectedFile = file;
    }


    // ───────────────────── Upload with SSE ─────────────────────

    startUpload(): void {
        if (!this.selectedFile) return;

        this.state = 'uploading';
        this.uploadProgress = 0;
        this.statusMessage = `Uploading ${this.selectedFile.name}…`;

        const formData = new FormData();
        formData.append('file', this.selectedFile, this.selectedFile.name);

        const url = this.projectService.getUploadUrl();
        const authHeaders = this.projectService.getAuthHeadersRecord();

        //
        // Use fetch() instead of HttpClient so we can stream SSE events
        // from the server during processing. The server responds with
        // Content-Type: text/event-stream and sends real-time progress.
        //
        fetch(url, {
            method: 'POST',
            headers: authHeaders,
            body: formData
        }).then(response => {
            if (!response.ok && !response.body) {
                throw new Error(`Upload failed with status ${response.status}`);
            }

            const reader = response.body!.getReader();
            const decoder = new TextDecoder();
            let buffer = '';

            const processStream = (): Promise<void> => {
                return reader.read().then(({ done, value }): Promise<void> | void => {
                    if (done) return;

                    buffer += decoder.decode(value, { stream: true });

                    //
                    // Parse SSE events from the buffer.
                    // Events are separated by double newlines.
                    //
                    const events = buffer.split('\n\n');
                    buffer = events.pop() || ''; // Keep incomplete last chunk

                    for (const eventBlock of events) {
                        if (!eventBlock.trim()) continue;

                        const lines = eventBlock.split('\n');
                        let eventType = 'message';
                        let data = '';

                        for (const line of lines) {
                            if (line.startsWith('event: ')) {
                                eventType = line.substring(7).trim();
                            } else if (line.startsWith('data: ')) {
                                data = line.substring(6);
                            }
                        }

                        if (!data) continue;

                        try {
                            const parsed = JSON.parse(data);

                            //
                            // Run inside NgZone so Angular detects the changes
                            //
                            this.ngZone.run(() => {
                                if (eventType === 'complete') {
                                    this.uploadProgress = 100;
                                    this.statusMessage = 'Import complete!';
                                    this.result = parsed.result as UploadResult;
                                    this.state = 'success';
                                    this.hasSuccessfulUpload = true;
                                } else if (eventType === 'error') {
                                    this.errorMessage = parsed.error || 'Import failed.';
                                    this.state = 'error';
                                } else {
                                    // Progress event
                                    this.uploadProgress = parsed.percent || 0;
                                    this.statusMessage = parsed.step || 'Processing…';
                                }
                            });
                        } catch {
                            // Skip malformed JSON
                        }
                    }

                    return processStream();
                });
            };

            return processStream();
        }).catch(err => {
            this.ngZone.run(() => {
                this.errorMessage = err?.message || 'Upload failed. Please try again.';
                this.state = 'error';
            });
        });
    }


    // ───────────────────── Actions ─────────────────────

    reset(): void {
        this.state = 'select';
        this.selectedFile = null;
        this.uploadProgress = 0;
        this.statusMessage = '';
        this.result = null;
        this.errorMessage = '';
    }

    close(): void {
        this.activeModal.close(this.result);
    }

    dismiss(): void {
        //
        // If the user had at least one successful upload, close with the result
        // so the parent component refreshes the project list.
        //
        if (this.hasSuccessfulUpload == true) {
            this.activeModal.close(this.result);
        } else {
            this.activeModal.dismiss();
        }
    }


    // ───────────────────── Utility ─────────────────────

    getExtension(filename: string): string {
        const idx = filename.lastIndexOf('.');
        return idx >= 0 ? filename.substring(idx).toLowerCase() : '';
    }

    formatFileSize(bytes: number): string {
        if (bytes < 1024) return `${bytes} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    }

    getFormatName(ext: string): string {
        switch (ext) {
            case '.ldr': return 'LDraw Model';
            case '.mpd': return 'LDraw Multi-Part Document';
            case '.io': return 'BrickLink Studio';
            case '.lxf': return 'LEGO Digital Designer';
            default: return 'Unknown';
        }
    }
}

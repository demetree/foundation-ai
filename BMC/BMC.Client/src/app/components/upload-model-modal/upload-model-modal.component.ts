import { Component } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { HttpEventType } from '@angular/common/http';
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
    result: UploadResult | null = null;
    errorMessage = '';

    // Drag state
    isDragOver = false;

    // Validation
    readonly maxFileSize = 50 * 1024 * 1024; // 50MB
    readonly acceptedExtensions = ['.ldr', '.mpd', '.io', '.lxf'];

    constructor(
        public activeModal: NgbActiveModal,
        private projectService: ProjectService
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


    // ───────────────────── Upload ─────────────────────

    startUpload(): void {
        if (!this.selectedFile) return;

        this.state = 'uploading';
        this.uploadProgress = 0;

        this.projectService.uploadModel(this.selectedFile).subscribe({
            next: (event) => {
                if (event.type === HttpEventType.UploadProgress && event.total) {
                    this.uploadProgress = Math.round(100 * event.loaded / event.total);
                } else if (event.type === HttpEventType.Response) {
                    this.result = event.body as UploadResult;
                    this.state = 'success';
                }
            },
            error: (err) => {
                this.errorMessage = err?.error?.detail || err?.error?.title || 'Upload failed. Please try again.';
                this.state = 'error';
            }
        });
    }


    // ───────────────────── Actions ─────────────────────

    reset(): void {
        this.state = 'select';
        this.selectedFile = null;
        this.uploadProgress = 0;
        this.result = null;
        this.errorMessage = '';
    }

    close(): void {
        this.activeModal.close(this.result);
    }

    dismiss(): void {
        this.activeModal.dismiss();
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

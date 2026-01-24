//
// User Image Upload Component
//
// Modal component for uploading/managing user profile images.
// Supports drag-drop, file picker, preview, and removal.
//

import { Component, Input, OnInit, ElementRef, ViewChild } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { HttpClient } from '@angular/common/http';
import { SecurityUserData } from '../../../security-data-services/security-user.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-user-image-upload',
    templateUrl: './user-image-upload.component.html',
    styleUrls: ['./user-image-upload.component.scss']
})
export class UserImageUploadComponent implements OnInit {

    @Input() user: SecurityUserData | null = null;
    @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

    //
    // State
    //
    public previewUrl: string | null = null;
    public dragOver: boolean = false;
    public uploading: boolean = false;
    public removing: boolean = false;
    public hasExistingImage: boolean = false;

    constructor(
        public activeModal: NgbActiveModal,
        private http: HttpClient,
        private authService: AuthService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        //
        // Check if user already has an image
        //
        if (this.user?.image) {
            this.hasExistingImage = true;
            this.previewUrl = 'data:image/png;base64,' + this.user.image;
        }
    }


    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            this.processFile(input.files[0]);
        }
    }


    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.dragOver = true;
    }


    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.dragOver = false;
    }


    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.dragOver = false;

        if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
            this.processFile(event.dataTransfer.files[0]);
        }
    }


    private processFile(file: File): void {
        //
        // Validate file type
        //
        if (!file.type.startsWith('image/')) {
            this.alertService.showMessage('Invalid File', 'Please select an image file', MessageSeverity.warn);
            return;
        }

        //
        // Validate file size (max 5MB)
        //
        const maxSize = 5 * 1024 * 1024;
        if (file.size > maxSize) {
            this.alertService.showMessage('File Too Large', 'Image must be less than 5MB', MessageSeverity.warn);
            return;
        }

        //
        // Read file and create preview
        //
        const reader = new FileReader();
        reader.onload = (e) => {
            this.previewUrl = e.target?.result as string;
        };
        reader.readAsDataURL(file);
    }


    selectFile(): void {
        this.fileInput.nativeElement.click();
    }


    clearPreview(): void {
        this.previewUrl = null;
        if (this.fileInput) {
            this.fileInput.nativeElement.value = '';
        }
    }


    async upload(): Promise<void> {
        if (!this.user || !this.previewUrl) return;

        this.uploading = true;

        try {
            const headers = this.authService.GetAuthenticationHeaders();
            const response = await this.http.put(
                `api/SecurityUsers/${this.user.id}/image`,
                { imageData: this.previewUrl },
                { headers }
            ).toPromise();

            this.alertService.showMessage('Success', 'Profile image updated', MessageSeverity.success);
            this.activeModal.close(true);

        } catch (error: any) {
            console.error('Error uploading image:', error);
            const message = error.error?.message || error.error || 'Failed to upload image';
            this.alertService.showMessage('Error', message, MessageSeverity.error);
        } finally {
            this.uploading = false;
        }
    }


    async removeImage(): Promise<void> {
        if (!this.user) return;

        this.removing = true;

        try {
            const headers = this.authService.GetAuthenticationHeaders();
            await this.http.delete(
                `api/SecurityUsers/${this.user.id}/image`,
                { headers }
            ).toPromise();

            this.alertService.showMessage('Success', 'Profile image removed', MessageSeverity.success);
            this.activeModal.close(true);

        } catch (error: any) {
            console.error('Error removing image:', error);
            this.alertService.showMessage('Error', 'Failed to remove image', MessageSeverity.error);
        } finally {
            this.removing = false;
        }
    }


    cancel(): void {
        this.activeModal.dismiss();
    }


    canUpload(): boolean {
        return this.previewUrl != null && !this.uploading && !this.removing;
    }
}

// AI-Developed — This file was significantly developed with AI assistance.
import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';


/**
 *
 * Custom invoice helper service — wraps the business-logic endpoints
 * on InvoicesController that go beyond auto-generated CRUD.
 *
 * Endpoints:
 *   POST  /api/Invoices/CreateFromEvent/{eventId}
 *   GET   /api/Invoices/NextInvoiceNumber
 *   GET   /api/Invoices/GeneratePdf/{id}
 *
 */
@Injectable({ providedIn: 'root' })
export class InvoiceHelperService extends SecureEndpointBase {

    constructor(
        http: HttpClient,
        alertService: AlertService,
        authService: AuthService,
        @Inject('BASE_URL') private baseUrl: string
    ) {
        super(http, alertService, authService);
    }


    private getAuthHeaders(): HttpHeaders {
        return new HttpHeaders({
            'Authorization': 'Bearer ' + this.authService.accessToken
        });
    }


    /**
     * Creates a draft invoice from all active event charges on the specified event.
     */
    public createFromEvent(eventId: number): Observable<{ invoiceId: number; invoiceNumber: string }> {
        const url = `${this.baseUrl}api/Invoices/CreateFromEvent/${eventId}`;

        return this.http.post<{ invoiceId: number; invoiceNumber: string }>(url, null, {
            headers: this.getAuthHeaders()
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.createFromEvent(eventId)))
        );
    }


    /**
     * Returns the next sequential invoice number for the tenant.
     */
    public getNextInvoiceNumber(): Observable<{ invoiceNumber: string }> {
        const url = `${this.baseUrl}api/Invoices/NextInvoiceNumber`;

        return this.http.get<{ invoiceNumber: string }>(url, {
            headers: this.getAuthHeaders()
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getNextInvoiceNumber()))
        );
    }


    /**
     * Generates a PDF for the specified invoice, stores it as a Document,
     * and returns the binary PDF as a Blob for download.
     */
    public generatePdf(invoiceId: number): Observable<Blob> {
        const url = `${this.baseUrl}api/Invoices/GeneratePdf/${invoiceId}`;

        return this.http.get(url, {
            headers: this.getAuthHeaders(),
            responseType: 'blob'
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.generatePdf(invoiceId)))
        );
    }


    /**
     * Triggers a browser download of the PDF blob.
     */
    public downloadPdf(blob: Blob, invoiceNumber: string): void {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Invoice-${invoiceNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
    }
}

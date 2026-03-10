// AI-Developed — This file was significantly developed with AI assistance.
import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';


/**
 *
 * Custom receipt helper service — wraps the business-logic endpoints
 * on ReceiptsController that go beyond auto-generated CRUD.
 *
 * Endpoints:
 *   POST  /api/Receipts/CreateFromPayment/{paymentTransactionId}
 *   POST  /api/Receipts/CreateFromInvoicePayment/{invoiceId}?amount=X
 *   GET   /api/Receipts/NextReceiptNumber
 *   GET   /api/Receipts/GeneratePdf/{id}
 *
 */
@Injectable({ providedIn: 'root' })
export class ReceiptHelperService extends SecureEndpointBase {

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
     * Creates a receipt from a payment transaction.
     */
    public createFromPayment(paymentTransactionId: number): Observable<{ receiptId: number; receiptNumber: string }> {
        const url = `${this.baseUrl}api/Receipts/CreateFromPayment/${paymentTransactionId}`;

        return this.http.post<{ receiptId: number; receiptNumber: string }>(url, null, {
            headers: this.getAuthHeaders()
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.createFromPayment(paymentTransactionId)))
        );
    }


    /**
     * Creates a receipt linked to an invoice payment.
     */
    public createFromInvoicePayment(invoiceId: number, amount: number): Observable<{ receiptId: number; receiptNumber: string }> {
        const url = `${this.baseUrl}api/Receipts/CreateFromInvoicePayment/${invoiceId}?amount=${amount}`;

        return this.http.post<{ receiptId: number; receiptNumber: string }>(url, null, {
            headers: this.getAuthHeaders()
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.createFromInvoicePayment(invoiceId, amount)))
        );
    }


    /**
     * Returns the next sequential receipt number for the tenant.
     */
    public getNextReceiptNumber(): Observable<{ receiptNumber: string }> {
        const url = `${this.baseUrl}api/Receipts/NextReceiptNumber`;

        return this.http.get<{ receiptNumber: string }>(url, {
            headers: this.getAuthHeaders()
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getNextReceiptNumber()))
        );
    }


    /**
     * Generates a PDF for the specified receipt, stores it as a Document,
     * and returns the binary PDF as a Blob for download.
     */
    public generatePdf(receiptId: number): Observable<Blob> {
        const url = `${this.baseUrl}api/Receipts/GeneratePdf/${receiptId}`;

        return this.http.get(url, {
            headers: this.getAuthHeaders(),
            responseType: 'blob'
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.generatePdf(receiptId)))
        );
    }


    /**
     * Triggers a browser download of the PDF blob.
     */
    public downloadPdf(blob: Blob, receiptNumber: string): void {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Receipt-${receiptNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
    }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { saveAs } from 'file-saver';
import { AuthService } from './auth.service'; // Adjust path to your AuthService

@Injectable({
  providedIn: 'root', // Makes the service available app-wide
})
export class TenantHelperService {

  // URL for the API endpoint to export the database to Excel
  private excelExportApiUrl = 'api/Data/ExportDatabaseToExcel';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  /**
   * Exports tenant data to an Excel file and triggers a download.
   * @returns An Observable that emits when the download is triggered or errors if the request fails.
   */
  exportToExcel(): Observable<void> {

    const headers = this.authService.GetAuthenticationHeaders();


    // Explicitly type the response as Blob
    //const options = {
    //  headers: httpHeaders,
    //  responseType: 'blob' as 'blob' // Use 'blob' directly
    //};

    return this.http.get(this.excelExportApiUrl, {
      headers: headers,
      responseType: 'blob'      // responseType: 'blob' as 'blob' // Use 'blob' directly
    }).pipe(
      map((response: Blob) => {
        const blob = new Blob([response], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const timestamp = new Date().toISOString().replace(/T/, '_').replace(/:/g, '').substring(0, 15);
        const fileName = `AlertingExport_${timestamp}.xlsx`;
        saveAs(blob, fileName);
      }),
      catchError((error) => {
        console.error('Error downloading Excel file:', error);
        return throwError(() => error);
      })
    );
  }
}

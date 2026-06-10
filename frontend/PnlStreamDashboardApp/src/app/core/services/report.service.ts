import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { PnlNotificationDto } from '../models/pnl-notification.model';
import { PaginationRequest, PaginatedResponse } from '../models/pagination.model';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private apiBaseUrl = 'https://localhost:7162/api'; // Replace with actual API URL

  constructor(private http: HttpClient) {}

  /**
   * Fetches initial/historical valid records from the database
   * @returns Observable<PnlNotificationDto[]> containing valid records
   */
  public getInitialValidRecords(): Observable<PnlNotificationDto[]> {
    return this.http
      .get<PnlNotificationDto[]>(`${this.apiBaseUrl}/pnl/valid`)
      .pipe(
        catchError((error) => {
          console.error('Error fetching initial valid records:', error);
          return of([]);
        })
      );
  }

  /**
   * Fetches initial/historical invalid records from the database
   * @returns Observable<PnlNotificationDto[]> containing invalid records
   */
  public getInitialInvalidRecords(): Observable<PnlNotificationDto[]> {
    return this.http
      .get<PnlNotificationDto[]>(`${this.apiBaseUrl}/pnl/invalid`)
      .pipe(
        catchError((error) => {
          console.error('Error fetching initial invalid records:', error);
          return of([]);
        })
      );
  }

  /**
   * Fetches paginated valid records from the database
   * @param paginationRequest Pagination parameters (page, pageSize, sortBy, sortOrder)
   * @returns Observable<PaginatedResponse<PnlNotificationDto>> containing paginated valid records
   */
  public getValidRecordsPaginated(paginationRequest: PaginationRequest): Observable<PaginatedResponse<PnlNotificationDto>> {
    const params = this.buildPaginationParams(paginationRequest);

    return this.http
      .get<PaginatedResponse<PnlNotificationDto>>(`${this.apiBaseUrl}/pnl/valid/paginated`, { params })
      .pipe(
        catchError((error) => {
          console.error('Error fetching paginated valid records:', error);
          return of(this.createEmptyPaginatedResponse(paginationRequest));
        })
      );
  }

  /**
   * Fetches paginated invalid records from the database
   * @param paginationRequest Pagination parameters (page, pageSize, sortBy, sortOrder)
   * @returns Observable<PaginatedResponse<PnlNotificationDto>> containing paginated invalid records
   */
  public getInvalidRecordsPaginated(paginationRequest: PaginationRequest): Observable<PaginatedResponse<PnlNotificationDto>> {
    const params = this.buildPaginationParams(paginationRequest);

    return this.http
      .get<PaginatedResponse<PnlNotificationDto>>(`${this.apiBaseUrl}/pnl/invalid/paginated`, { params })
      .pipe(
        catchError((error) => {
          console.error('Error fetching paginated invalid records:', error);
          return of(this.createEmptyPaginatedResponse(paginationRequest));
        })
      );
  }

  /**
   * Downloads a valid records report for the specified date range
   * @param startDate Start date for the report
   * @param endDate End date for the report
   * @returns Observable<Blob> containing the report file
   */
  public downloadValidReport(startDate: Date, endDate: Date): Observable<Blob> {
    const params = {
      startDate: this.formatDate(startDate),
      endDate: this.formatDate(endDate),
    };

    return this.http.get(`${this.apiBaseUrl}/download/validreport`, {
      params: params,
      responseType: 'blob',
    });
  }

  /**
   * Downloads an invalid records report for the specified date range
   * @param startDate Start date for the report
   * @param endDate End date for the report
   * @returns Observable<Blob> containing the report file
   */
  public downloadInvalidReport(startDate: Date, endDate: Date): Observable<Blob> {
    const params = {
      startDate: this.formatDate(startDate),
      endDate: this.formatDate(endDate),
    };

    return this.http.get(`${this.apiBaseUrl}/download/excludedreport`, {
      params: params,
      responseType: 'blob',
    });
  }

  /**
   * Trigger browser file download
   * @param blob The file blob to download
   * @param filename The name of the file to save as
   */
  public downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  /**
   * Format date to YYYY-MM-DD format for API
   * @param date The date to format
   * @returns Formatted date string
   */
  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * Build HTTP params from PaginationRequest
   * @param request Pagination request
   * @returns HttpParams
   */
  private buildPaginationParams(request: PaginationRequest): HttpParams {
    let params = new HttpParams();
    params = params.set('page', request.page.toString());
    params = params.set('pageSize', request.pageSize.toString());

    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }

    if (request.sortOrder) {
      params = params.set('sortOrder', request.sortOrder);
    }

    return params;
  }

  private createEmptyPaginatedResponse(request: PaginationRequest): PaginatedResponse<PnlNotificationDto> {
    const pageSize = request.pageSize || 10;
    const pageIndex = request.page || 0;

    return {
      data: [],
      totalRecords: 0,
      totalPages: 0,
      currentPage: pageIndex,
      pageSize,
      hasNext: false,
      hasPrevious: false,
    };
  }
}

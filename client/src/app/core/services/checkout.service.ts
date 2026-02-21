import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CheckoutRecord, CheckoutRequest, DashboardStats } from '../models/checkout.model';
import { PagedResult, QueryParameters } from '../models/book.model';

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private readonly apiUrl = `${environment.apiUrl}/checkouts`;
  private readonly dashboardUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getCheckouts(params: QueryParameters): Observable<PagedResult<CheckoutRecord>> {
    let httpParams = new HttpParams();
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.filters) {
      Object.entries(params.filters).forEach(([key, value]) => {
        httpParams = httpParams.set(`filters[${key}]`, value);
      });
    }
    return this.http.get<PagedResult<CheckoutRecord>>(this.apiUrl, { params: httpParams });
  }

  checkoutBook(request: CheckoutRequest): Observable<CheckoutRecord> {
    return this.http.post<CheckoutRecord>(this.apiUrl, request);
  }

  returnBook(checkoutId: string, notes?: string): Observable<CheckoutRecord> {
    return this.http.post<CheckoutRecord>(`${this.apiUrl}/${checkoutId}/return`, { checkoutId, notes });
  }

  getOverdueBooks(): Observable<CheckoutRecord[]> {
    return this.http.get<CheckoutRecord[]>(`${this.apiUrl}/overdue`);
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.dashboardUrl}/stats`);
  }
}

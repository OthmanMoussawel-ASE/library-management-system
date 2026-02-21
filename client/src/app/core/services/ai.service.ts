import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RecommendedBook {
  id: string;
  title: string;
  author: string;
  isAvailable: boolean;
}

export interface BookRecommendationsResponse {
  fromLibrary: RecommendedBook[];
  discoverMore: string[];
}

@Injectable({ providedIn: 'root' })
export class AIService {
  private readonly apiUrl = `${environment.apiUrl}/ai`;

  constructor(private http: HttpClient) {}

  getRecommendations(): Observable<BookRecommendationsResponse> {
    return this.http.get<BookRecommendationsResponse>(`${this.apiUrl}/recommendations`);
  }

  smartSearch(query: string): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/smart-search`, { query });
  }

  generateDescription(title: string, author: string): Observable<{ description: string }> {
    return this.http.post<{ description: string }>(`${this.apiUrl}/generate-description`, { title, author });
  }

  getStatus(): Observable<{ available: boolean }> {
    return this.http.get<{ available: boolean }>(`${this.apiUrl}/status`);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Book, CreateBookRequest, UpdateBookRequest, Author, Category, PagedResult, QueryParameters } from '../models/book.model';

@Injectable({ providedIn: 'root' })
export class BookService {
  private readonly apiUrl = `${environment.apiUrl}/books`;
  private readonly authorsUrl = `${environment.apiUrl}/authors`;
  private readonly categoriesUrl = `${environment.apiUrl}/categories`;
  private readonly aiUrl = `${environment.apiUrl}/ai`;

  constructor(private http: HttpClient) {}

  getBooks(params: QueryParameters): Observable<PagedResult<Book>> {
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
    return this.http.get<PagedResult<Book>>(this.apiUrl, { params: httpParams });
  }

  getBook(id: string): Observable<Book> {
    return this.http.get<Book>(`${this.apiUrl}/${id}`);
  }

  createBook(request: CreateBookRequest): Observable<Book> {
    return this.http.post<Book>(this.apiUrl, request);
  }

  updateBook(id: string, request: UpdateBookRequest): Observable<Book> {
    return this.http.put<Book>(`${this.apiUrl}/${id}`, request);
  }

  deleteBook(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getAuthors(): Observable<Author[]> {
    return this.http.get<Author[]>(`${this.authorsUrl}/all`);
  }

  getAuthorsPaged(params: QueryParameters): Observable<PagedResult<Author>> {
    let httpParams = this.buildParams(params);
    return this.http.get<PagedResult<Author>>(this.authorsUrl, { params: httpParams });
  }

  createAuthor(author: { firstName: string; lastName: string; biography?: string }): Observable<Author> {
    return this.http.post<Author>(this.authorsUrl, author);
  }

  updateAuthor(id: string, author: { firstName: string; lastName: string; biography?: string }): Observable<Author> {
    return this.http.put<Author>(`${this.authorsUrl}/${id}`, author);
  }

  deleteAuthor(id: string): Observable<void> {
    return this.http.delete<void>(`${this.authorsUrl}/${id}`);
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.categoriesUrl}/all`);
  }

  getCategoriesPaged(params: QueryParameters): Observable<PagedResult<Category>> {
    let httpParams = this.buildParams(params);
    return this.http.get<PagedResult<Category>>(this.categoriesUrl, { params: httpParams });
  }

  createCategory(category: { name: string; description?: string }): Observable<Category> {
    return this.http.post<Category>(this.categoriesUrl, category);
  }

  updateCategory(id: string, category: { name: string; description?: string }): Observable<Category> {
    return this.http.put<Category>(`${this.categoriesUrl}/${id}`, category);
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.categoriesUrl}/${id}`);
  }

  private buildParams(params: QueryParameters): HttpParams {
    let httpParams = new HttpParams();
    if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.sortDirection) httpParams = httpParams.set('sortDirection', params.sortDirection);
    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    return httpParams;
  }

  generateDescription(title: string, author: string): Observable<{ description: string }> {
    return this.http.post<{ description: string }>(`${this.aiUrl}/generate-description`, { title, author });
  }

  suggestCategories(title: string, author: string, description?: string): Observable<{ existing: string[]; suggested: string[] }> {
    return this.http.post<{ existing: string[]; suggested: string[] }>(`${this.aiUrl}/categorize`, { title, author, description });
  }

  smartSearch(query: string): Observable<{ title?: string; author?: string; genre?: string; keywords?: string }> {
    return this.http.post<{ title?: string; author?: string; genre?: string; keywords?: string }>(`${this.aiUrl}/smart-search`, { query });
  }
}

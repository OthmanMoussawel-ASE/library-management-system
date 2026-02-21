import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(this.getStoredUser());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  get currentUser(): AuthResponse | null {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    const user = this.currentUser;
    if (!user) return false;
    return new Date(user.expiresAt) > new Date();
  }

  get isAdmin(): boolean {
    return this.currentUser?.role === 'Admin';
  }

  get isLibrarian(): boolean {
    return this.currentUser?.role === 'Librarian' || this.isAdmin;
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.setUser(response))
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.setUser(response))
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.currentUser?.refreshToken;
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap(response => this.setUser(response))
    );
  }

  logout(): void {
    const refreshToken = this.currentUser?.refreshToken;
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/revoke`, { refreshToken }).subscribe();
    }
    localStorage.removeItem('auth_user');
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  private setUser(response: AuthResponse): void {
    localStorage.setItem('auth_user', JSON.stringify(response));
    this.currentUserSubject.next(response);
  }

  private getStoredUser(): AuthResponse | null {
    const stored = localStorage.getItem('auth_user');
    if (!stored) return null;
    try {
      const user = JSON.parse(stored) as AuthResponse;
      if (new Date(user.expiresAt) <= new Date()) {
        localStorage.removeItem('auth_user');
        return null;
      }
      return user;
    } catch {
      return null;
    }
  }
}

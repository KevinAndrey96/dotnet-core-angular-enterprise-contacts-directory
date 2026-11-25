import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Login, AuthResponse } from '../models/Login';
import { User } from '../models/User';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = 'http://localhost:5000/api';
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'current_user';

  constructor(private http: HttpClient) {}

  login(credentials: Login): Observable<any> {
    return this.http.post<any>(`${this.API_URL}/Login`, credentials).pipe(
      tap(res => {
        if (res && res.token) {
          this.saveSession(res);
        }
      })
    );
  }

  register(userData: User): Observable<any> {
    return this.http.post<any>(`${this.API_URL}/Users`, userData);
  }

  saveSession(auth: any): void {
    if (auth.token) {
      localStorage.setItem(this.TOKEN_KEY, auth.token);
      localStorage.setItem(this.USER_KEY, JSON.stringify(auth));
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getCurrentUser(): any {
    const raw = localStorage.getItem(this.USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }
}

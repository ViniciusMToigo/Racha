import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { APP_CONFIG } from '../config/app-config';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/api.models';
import { SessionStorage } from './session-storage';

/** Holds the logged-in user as a signal; components read it, only this service changes it. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);
  private readonly storage = inject(SessionStorage);

  private readonly session = signal<AuthResponse | null>(this.restoreSession());

  readonly currentUser = computed<User | null>(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);

  login(request: LoginRequest): Observable<User> {
    return this.http.post<AuthResponse>(`${this.config.authApiUrl}/auth/login`, request).pipe(
      tap((session) => this.start(session)),
      map((session) => session.user),
    );
  }

  register(request: RegisterRequest): Observable<User> {
    return this.http.post<AuthResponse>(`${this.config.authApiUrl}/auth/register`, request).pipe(
      tap((session) => this.start(session)),
      map((session) => session.user),
    );
  }

  logout(): void {
    this.session.set(null);
    this.storage.clear();
  }

  /** The token to send to the APIs, or null when logged out or expired. */
  accessToken(): string | null {
    return this.hasValidSession() ? this.session()!.accessToken : null;
  }

  /** Checks expiry too: a token that expired while the tab was open ends the session. */
  hasValidSession(): boolean {
    const session = this.session();
    if (session && isExpired(session)) {
      this.logout();
      return false;
    }
    return session !== null;
  }

  private start(session: AuthResponse): void {
    this.session.set(session);
    this.storage.save(session);
  }

  private restoreSession(): AuthResponse | null {
    const stored = this.storage.load();
    if (stored && isExpired(stored)) {
      this.storage.clear();
      return null;
    }
    return stored;
  }
}

function isExpired(session: AuthResponse): boolean {
  return new Date(session.expiresAt).getTime() <= Date.now();
}

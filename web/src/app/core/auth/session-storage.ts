import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/api.models';

const STORAGE_KEY = 'racha.session';

/**
 * Persists the session so a page refresh keeps the user logged in.
 *
 * Trade-off: localStorage is readable by any script on the page, so an XSS bug could leak the token.
 * The alternative is an httpOnly cookie set by the API, which needs CSRF protection instead.
 * For this exercise the token is short-lived (60 min) and Angular escapes all template output.
 */
@Injectable({ providedIn: 'root' })
export class SessionStorage {
  load(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as AuthResponse) : null;
    } catch {
      return null;
    }
  }

  save(session: AuthResponse): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } catch {
      // Storage can be unavailable (private mode, quota). The session still works until a refresh.
    }
  }

  clear(): void {
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      // Nothing to clear.
    }
  }
}

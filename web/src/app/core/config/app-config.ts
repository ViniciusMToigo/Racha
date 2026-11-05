import { InjectionToken } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface AppConfig {
  /** Base URL of the Auth API (register, login, current user). */
  authApiUrl: string;
  /** Base URL of the Expenses API (groups, expenses, balances). */
  apiUrl: string;
}

/** Injected instead of importing `environment` everywhere, so tests can provide their own URLs. */
export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG', {
  providedIn: 'root',
  factory: () => environment,
});

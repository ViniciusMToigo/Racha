import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { ALICE, TEST_CONFIG, sessionFor } from '../../../testing/test-config';
import { APP_CONFIG } from '../config/app-config';
import { authGuard, guestGuard } from './auth.guards';

describe('auth guards', () => {
  function setUp(loggedIn: boolean): void {
    localStorage.clear();
    if (loggedIn) {
      localStorage.setItem('racha.session', JSON.stringify(sessionFor(ALICE)));
    }
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
  }

  const route = {} as ActivatedRouteSnapshot;
  const state = (url: string) => ({ url }) as RouterStateSnapshot;
  const serialize = (result: unknown) => TestBed.inject(Router).serializeUrl(result as UrlTree);

  it('authGuard lets a logged-in user through', () => {
    setUp(true);

    const result = TestBed.runInInjectionContext(() => authGuard(route, state('/groups/42')));

    expect(result).toBe(true);
  });

  it('authGuard sends a visitor to login and remembers where they were going', () => {
    setUp(false);

    const result = TestBed.runInInjectionContext(() => authGuard(route, state('/groups/42')));

    expect(serialize(result)).toBe('/login?returnUrl=%2Fgroups%2F42');
  });

  it('guestGuard sends a logged-in user to their groups', () => {
    setUp(true);

    const result = TestBed.runInInjectionContext(() => guestGuard(route, state('/login')));

    expect(serialize(result)).toBe('/groups');
  });

  it('guestGuard lets a visitor see the login page', () => {
    setUp(false);

    const result = TestBed.runInInjectionContext(() => guestGuard(route, state('/login')));

    expect(result).toBe(true);
  });
});

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ALICE, TEST_CONFIG, sessionFor } from '../../../testing/test-config';
import { APP_CONFIG } from '../config/app-config';
import { AuthService } from './auth.service';
import { SessionStorage } from './session-storage';

describe('AuthService', () => {
  let httpTesting: HttpTestingController;

  function createService(): AuthService {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    return TestBed.inject(AuthService);
  }

  beforeEach(() => localStorage.clear());
  afterEach(() => httpTesting.verify());

  it('logs in, exposes the user and keeps the session across reloads', () => {
    const auth = createService();
    let loggedIn = '';

    auth
      .login({ email: ALICE.email, password: 'Demo123!' })
      .subscribe((user) => (loggedIn = user.name));
    const request = httpTesting.expectOne(`${TEST_CONFIG.authApiUrl}/auth/login`);
    expect(request.request.method).toBe('POST');
    request.flush(sessionFor(ALICE));

    expect(loggedIn).toBe('Alice');
    expect(auth.isAuthenticated()).toBe(true);
    expect(auth.currentUser()?.id).toBe(ALICE.id);
    expect(auth.accessToken()).toBe(`token-for-${ALICE.id}`);
    expect(TestBed.inject(SessionStorage).load()?.user.id).toBe(ALICE.id);
  });

  it('restores a stored session on startup', () => {
    localStorage.setItem('racha.session', JSON.stringify(sessionFor(ALICE)));

    const auth = createService();

    expect(auth.currentUser()?.name).toBe('Alice');
  });

  it('ignores and clears an expired stored session', () => {
    localStorage.setItem('racha.session', JSON.stringify(sessionFor(ALICE, -5)));

    const auth = createService();

    expect(auth.isAuthenticated()).toBe(false);
    expect(localStorage.getItem('racha.session')).toBeNull();
  });

  it('logout clears the user and the stored session', () => {
    localStorage.setItem('racha.session', JSON.stringify(sessionFor(ALICE)));
    const auth = createService();

    auth.logout();

    expect(auth.currentUser()).toBeNull();
    expect(auth.accessToken()).toBeNull();
    expect(localStorage.getItem('racha.session')).toBeNull();
  });

  it('does not start a session when login fails', () => {
    const auth = createService();

    auth.login({ email: ALICE.email, password: 'wrong' }).subscribe({ error: () => undefined });
    httpTesting
      .expectOne(`${TEST_CONFIG.authApiUrl}/auth/login`)
      .flush({ detail: 'Invalid email or password.' }, { status: 401, statusText: 'Unauthorized' });

    expect(auth.isAuthenticated()).toBe(false);
  });
});

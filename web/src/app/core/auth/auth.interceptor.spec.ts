import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ALICE, TEST_CONFIG, sessionFor } from '../../../testing/test-config';
import { APP_CONFIG } from '../config/app-config';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;

  function setUp(loggedIn: boolean): void {
    localStorage.clear();
    if (loggedIn) {
      localStorage.setItem('racha.session', JSON.stringify(sessionFor(ALICE)));
    }
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  }

  afterEach(() => httpTesting.verify());

  it('adds the bearer token to requests for our APIs', () => {
    setUp(true);

    http.get(`${TEST_CONFIG.apiUrl}/groups`).subscribe();

    const request = httpTesting.expectOne(`${TEST_CONFIG.apiUrl}/groups`);
    expect(request.request.headers.get('Authorization')).toBe(`Bearer token-for-${ALICE.id}`);
    request.flush([]);
  });

  it('never sends the token to other hosts', () => {
    setUp(true);

    http.get('https://example.com/data').subscribe();

    const request = httpTesting.expectOne('https://example.com/data');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('sends no Authorization header when logged out', () => {
    setUp(false);

    http.get(`${TEST_CONFIG.apiUrl}/groups`).subscribe({ error: () => undefined });

    const request = httpTesting.expectOne(`${TEST_CONFIG.apiUrl}/groups`);
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush(null, { status: 401, statusText: 'Unauthorized' });
  });

  it('logs out and redirects to login when the API rejects the token', () => {
    setUp(true);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    http.get(`${TEST_CONFIG.apiUrl}/groups`).subscribe({ error: () => undefined });
    httpTesting
      .expectOne(`${TEST_CONFIG.apiUrl}/groups`)
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(TestBed.inject(AuthService).isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login'], expect.anything());
  });
});

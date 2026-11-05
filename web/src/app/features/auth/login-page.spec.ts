import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ALICE, TEST_CONFIG, sessionFor } from '../../../testing/test-config';
import { APP_CONFIG } from '../../core/config/app-config';
import { DEMO_ACCOUNT, LoginPage, safeReturnUrl } from './login-page';

describe('LoginPage', () => {
  let httpTesting: HttpTestingController;

  async function render() {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(LoginPage);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  function type(element: HTMLElement, selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  afterEach(() => httpTesting.verify());

  it('fills the form with the demo account', async () => {
    const { fixture, element } = await render();

    element.querySelector<HTMLButtonElement>('.demo-hint button')!.click();
    await fixture.whenStable();

    expect(element.querySelector<HTMLInputElement>('input[type=email]')!.value).toBe(
      DEMO_ACCOUNT.email,
    );
  });

  it('does not call the API while the form is invalid', async () => {
    const { fixture, element } = await render();

    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();

    httpTesting.expectNone(`${TEST_CONFIG.authApiUrl}/auth/login`);
    expect(element.textContent).toContain('Email is required.');
  });

  it('logs in and goes to the groups page', async () => {
    const { fixture, element } = await render();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);

    type(element, 'input[type=email]', ALICE.email);
    type(element, 'input[type=password]', 'Demo123!');
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    httpTesting.expectOne(`${TEST_CONFIG.authApiUrl}/auth/login`).flush(sessionFor(ALICE));
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith('/groups');
  });

  it('shows the API error when the credentials are wrong', async () => {
    const { fixture, element } = await render();

    type(element, 'input[type=email]', ALICE.email);
    type(element, 'input[type=password]', 'wrong');
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    httpTesting
      .expectOne(`${TEST_CONFIG.authApiUrl}/auth/login`)
      .flush({ detail: 'Invalid email or password.' }, { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();

    expect(element.querySelector('[role=alert]')?.textContent).toContain(
      'Invalid email or password.',
    );
  });

  it('only follows in-app return URLs', () => {
    expect(safeReturnUrl('/groups/42')).toBe('/groups/42');
    expect(safeReturnUrl(undefined)).toBe('/groups');
    expect(safeReturnUrl('https://evil.example')).toBe('/groups');
    expect(safeReturnUrl('//evil.example')).toBe('/groups');
  });
});

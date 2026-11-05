import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TEST_CONFIG } from '../../../testing/test-config';
import { APP_CONFIG } from '../config/app-config';
import { ExpensesApi } from './expenses-api';
import { GroupsApi } from './groups-api';

/** Checks that every client method hits the right URL with the right verb and body. */
describe('API clients', () => {
  let httpTesting: HttpTestingController;
  const api = TEST_CONFIG.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  function expectCall(method: string, url: string, body: unknown = null): void {
    const request = httpTesting.expectOne(url);
    expect(request.request.method).toBe(method);
    expect(request.request.body).toEqual(body);
    request.flush(null);
  }

  it('GroupsApi', () => {
    const groups = TestBed.inject(GroupsApi);

    groups.listMine().subscribe();
    expectCall('GET', `${api}/groups`);

    groups.get('g1').subscribe();
    expectCall('GET', `${api}/groups/g1`);

    groups.create('Trip').subscribe();
    expectCall('POST', `${api}/groups`, { name: 'Trip' });

    groups.rename('g1', 'Beach').subscribe();
    expectCall('PUT', `${api}/groups/g1`, { name: 'Beach' });

    groups.delete('g1').subscribe();
    expectCall('DELETE', `${api}/groups/g1`);

    groups.addMember('g1', 'bob@racha.dev').subscribe();
    expectCall('POST', `${api}/groups/g1/members`, { email: 'bob@racha.dev' });
  });

  it('ExpensesApi', () => {
    const expenses = TestBed.inject(ExpensesApi);
    const request = { description: 'Dinner', amount: 90, date: '2026-11-04', paidByUserId: 'u1' };

    expenses.list('g1').subscribe();
    expectCall('GET', `${api}/groups/g1/expenses`);

    expenses.create('g1', request).subscribe();
    expectCall('POST', `${api}/groups/g1/expenses`, request);

    expenses.update('e1', request).subscribe();
    expectCall('PUT', `${api}/expenses/e1`, request);

    expenses.delete('e1').subscribe();
    expectCall('DELETE', `${api}/expenses/e1`);

    expenses.balances('g1').subscribe();
    expectCall('GET', `${api}/groups/g1/balances`);
  });
});

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ALICE, BOB, TEST_CONFIG, TRIP, sessionFor } from '../../../testing/test-config';
import { APP_CONFIG } from '../../core/config/app-config';
import { Expense } from '../../core/models/api.models';
import { ExpensesPanel } from './expenses-panel';

describe('ExpensesPanel', () => {
  let httpTesting: HttpTestingController;

  const dinner: Expense = {
    id: 'e1',
    groupId: TRIP.id,
    description: 'Dinner',
    amount: 90,
    date: '2026-11-04',
    paidByUserId: BOB.id,
    shares: [
      { userId: ALICE.id, amount: 45 },
      { userId: BOB.id, amount: 45 },
    ],
  };

  async function renderWith(expenses: Expense[]) {
    localStorage.setItem('racha.session', JSON.stringify(sessionFor(ALICE)));
    TestBed.configureTestingModule({
      imports: [ExpensesPanel],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);

    const fixture = TestBed.createComponent(ExpensesPanel);
    fixture.componentRef.setInput('group', TRIP);
    TestBed.tick();

    httpTesting.expectOne(`${TEST_CONFIG.apiUrl}/groups/${TRIP.id}/expenses`).flush(expenses);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  it('lists expenses with the payer, the total and the current user share', async () => {
    const element = await renderWith([
      dinner,
      { ...dinner, id: 'e2', description: 'Taxi', amount: 30, shares: [] },
    ]);

    const items = element.querySelectorAll('.expense');
    expect(items).toHaveLength(2);
    expect(items[0].textContent).toContain('Paid by Bob');
    expect(items[0].textContent).toContain('Your share: R$45.00');
    expect(element.querySelector('.total .value')!.textContent).toContain('R$120.00');
  });

  it('shows an empty state when there are no expenses', async () => {
    const element = await renderWith([]);

    expect(element.textContent).toContain('No expenses yet');
    expect(element.querySelector('.total .value')!.textContent).toContain('R$0.00');
  });
});

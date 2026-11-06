import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
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

// Regression: the Cancel button closes the dialog with '' (not undefined), which used to be
// posted to the API as a text/plain body and came back as 415 Unsupported Media Type.
describe('ExpensesPanel dialog results', () => {
  let httpTesting: HttpTestingController;
  const expensesUrl = `${TEST_CONFIG.apiUrl}/groups/${TRIP.id}/expenses`;

  async function renderWithDialogResult(result: unknown) {
    localStorage.setItem('racha.session', JSON.stringify(sessionFor(ALICE)));
    TestBed.configureTestingModule({
      imports: [ExpensesPanel],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => of(result) }) } },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);

    const fixture = TestBed.createComponent(ExpensesPanel);
    fixture.componentRef.setInput('group', TRIP);
    TestBed.tick();
    httpTesting.expectOne({ method: 'GET', url: expensesUrl }).flush([]);
    await fixture.whenStable();

    const addButton = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLButtonElement>('button'),
    ).find((button) => button.textContent?.includes('Add expense'))!;
    addButton.click();
    await fixture.whenStable();
  }

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  it.each([
    ['Cancel button', ''],
    ['Escape or backdrop click', undefined],
  ])('does not call the API when the dialog is dismissed (%s)', async (_how, result) => {
    await renderWithDialogResult(result);

    httpTesting.expectNone({ method: 'POST', url: expensesUrl });
  });

  it('posts the expense as JSON when the dialog is saved', async () => {
    const request = {
      description: 'Dinner',
      amount: 90,
      date: '2026-11-04',
      paidByUserId: ALICE.id,
    };

    await renderWithDialogResult(request);

    const post = httpTesting.expectOne({ method: 'POST', url: expensesUrl });
    expect(post.request.body).toEqual(request);
    post.flush({ ...request, id: 'e1', groupId: TRIP.id, shares: [] });
    TestBed.tick(); // the list reloads after a successful save
    httpTesting.expectOne({ method: 'GET', url: expensesUrl }).flush([]);
  });
});

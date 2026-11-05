import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ALICE, BOB, TEST_CONFIG } from '../../../testing/test-config';
import { APP_CONFIG } from '../../core/config/app-config';
import { GroupBalances } from '../../core/models/api.models';
import { BalancesPanel } from './balances-panel';

describe('BalancesPanel', () => {
  let httpTesting: HttpTestingController;

  async function renderWith(balances: GroupBalances) {
    TestBed.configureTestingModule({
      imports: [BalancesPanel],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_CONFIG, useValue: TEST_CONFIG },
      ],
    });
    httpTesting = TestBed.inject(HttpTestingController);

    const fixture = TestBed.createComponent(BalancesPanel);
    fixture.componentRef.setInput('groupId', 'trip-id');
    TestBed.tick(); // lets the resource start its request

    httpTesting.expectOne(`${TEST_CONFIG.apiUrl}/groups/trip-id/balances`).flush(balances);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  afterEach(() => httpTesting.verify());

  it('shows who pays whom and each balance', async () => {
    const element = await renderWith({
      groupId: 'trip-id',
      balances: [
        { userId: ALICE.id, name: 'Alice', amount: 30 },
        { userId: BOB.id, name: 'Bob', amount: -30 },
      ],
      suggestedTransfers: [
        { fromUserId: BOB.id, fromName: 'Bob', toUserId: ALICE.id, toName: 'Alice', amount: 30 },
      ],
    });

    const transfer = element.querySelector('.transfer')!.textContent!.replace(/\s+/g, ' ');
    expect(transfer).toContain('Bob');
    expect(transfer).toContain('Alice');
    expect(transfer).toContain('R$30.00');

    const balances = Array.from(element.querySelectorAll('.balance')).map((li) =>
      li.textContent!.replace(/\s+/g, ' ').trim(),
    );
    expect(balances).toEqual(['Alice gets back R$30.00', 'Bob owes R$30.00']);
  });

  it('says everyone is settled when there is nothing to pay', async () => {
    const element = await renderWith({
      groupId: 'trip-id',
      balances: [{ userId: ALICE.id, name: 'Alice', amount: 0 }],
      suggestedTransfers: [],
    });

    expect(element.textContent).toContain('Everyone is settled up.');
    expect(element.querySelector('.balance')!.textContent).toContain('settled');
  });
});

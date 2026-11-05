import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ALICE, BOB } from '../../../testing/test-config';
import { ExpenseDialog, ExpenseDialogData, todayIso } from './expense-dialog';

describe('ExpenseDialog', () => {
  async function render(data: ExpenseDialogData) {
    const close = vi.fn();
    TestBed.configureTestingModule({
      imports: [ExpenseDialog],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: { close } },
      ],
    });
    const fixture = TestBed.createComponent(ExpenseDialog);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement, close };
  }

  function type(element: HTMLElement, selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  it('formats today as yyyy-MM-dd in local time', () => {
    expect(todayIso(new Date(2026, 0, 5, 23, 59))).toBe('2026-01-05');
  });

  it('returns a trimmed request with a numeric amount', async () => {
    const { fixture, element, close } = await render({
      members: [ALICE, BOB],
      defaultPayerId: ALICE.id,
    });

    type(element, 'input[formControlName=description]', '  Groceries  ');
    type(element, 'input[formControlName=amount]', '42.50');
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();

    expect(close).toHaveBeenCalledWith({
      description: 'Groceries',
      amount: 42.5,
      date: todayIso(),
      paidByUserId: ALICE.id,
    });
  });

  it('rejects amounts with more than two decimals', async () => {
    const { fixture, element, close } = await render({
      members: [ALICE, BOB],
      defaultPayerId: ALICE.id,
    });

    type(element, 'input[formControlName=description]', 'Coffee');
    type(element, 'input[formControlName=amount]', '10.001');
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();

    expect(close).not.toHaveBeenCalled();
    expect(element.textContent).toContain('Use at most two decimal places.');
  });

  it('starts with the existing values when editing', async () => {
    const { element } = await render({
      members: [ALICE, BOB],
      defaultPayerId: ALICE.id,
      expense: {
        id: 'e1',
        groupId: 'g1',
        description: 'Dinner',
        amount: 90,
        date: '2026-11-01',
        paidByUserId: BOB.id,
        shares: [],
      },
    });

    expect(element.querySelector('h2')!.textContent).toContain('Edit expense');
    expect(
      element.querySelector<HTMLInputElement>('input[formControlName=description]')!.value,
    ).toBe('Dinner');
    expect(element.querySelector<HTMLInputElement>('input[type=date]')!.value).toBe('2026-11-01');
  });
});

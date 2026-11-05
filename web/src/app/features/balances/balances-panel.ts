import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ExpensesApi } from '../../core/api/expenses-api';
import { errorMessage } from '../../core/http/problem-details';
import { MemberBalance } from '../../core/models/api.models';

/** Who is owed, who owes, and the fewest payments that settle the group (computed by the API). */
@Component({
  selector: 'app-balances-panel',
  imports: [CurrencyPipe, MatIconModule, MatButtonModule, MatProgressBarModule],
  templateUrl: './balances-panel.html',
  styleUrl: './balances-panel.scss',
})
export class BalancesPanel {
  private readonly expensesApi = inject(ExpensesApi);

  readonly groupId = input.required<string>();

  protected readonly balances = rxResource({
    params: () => this.groupId(),
    stream: ({ params: groupId }) => this.expensesApi.balances(groupId),
  });

  protected readonly settled = computed(
    () => this.balances.hasValue() && this.balances.value().suggestedTransfers.length === 0,
  );

  protected readonly errorMessage = errorMessage;

  protected status(balance: MemberBalance): 'owed' | 'owes' | 'settled' {
    return balance.amount > 0 ? 'owed' : balance.amount < 0 ? 'owes' : 'settled';
  }

  protected abs(amount: number): number {
    return Math.abs(amount);
  }
}

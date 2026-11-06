import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Observable, filter, switchMap } from 'rxjs';
import { ExpensesApi } from '../../core/api/expenses-api';
import { AuthService } from '../../core/auth/auth.service';
import { errorMessage } from '../../core/http/problem-details';
import { Expense, ExpenseRequest, Group } from '../../core/models/api.models';
import { ConfirmDialog, ConfirmDialogData } from '../../shared/confirm-dialog';
import { ExpenseDialog, ExpenseDialogData } from './expense-dialog';

/** The group's expenses: list, add, edit and delete. */
@Component({
  selector: 'app-expenses-panel',
  imports: [
    CurrencyPipe,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTooltipModule,
  ],
  templateUrl: './expenses-panel.html',
  styleUrl: './expenses-panel.scss',
})
export class ExpensesPanel {
  private readonly expensesApi = inject(ExpensesApi);
  private readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly group = input.required<Group>();

  protected readonly expenses = rxResource({
    params: () => this.group().id,
    stream: ({ params: groupId }) => this.expensesApi.list(groupId),
  });

  protected readonly total = computed(() =>
    this.expenses.hasValue() ? this.expenses.value().reduce((sum, e) => sum + e.amount, 0) : 0,
  );

  private readonly memberNames = computed(
    () => new Map(this.group().members.map((member) => [member.id, member.name])),
  );

  protected readonly errorMessage = errorMessage;

  protected payerName(expense: Expense): string {
    return this.memberNames().get(expense.paidByUserId) ?? 'Former member';
  }

  /** What the current user owes for this expense (0 if they're not part of the split). */
  protected myShare(expense: Expense): number {
    const me = this.auth.currentUser()?.id;
    return expense.shares.find((share) => share.userId === me)?.amount ?? 0;
  }

  protected add(): void {
    this.openDialog(
      undefined,
      (request) => this.expensesApi.create(this.group().id, request),
      'Expense added.',
    );
  }

  protected edit(expense: Expense): void {
    this.openDialog(
      expense,
      (request) => this.expensesApi.update(expense.id, request),
      'Expense updated.',
    );
  }

  protected remove(expense: Expense): void {
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, {
        data: {
          title: 'Delete expense?',
          message: `"${expense.description}" will be removed from the group.`,
        },
      })
      .afterClosed()
      .pipe(
        filter((confirmed) => confirmed === true),
        switchMap(() => this.expensesApi.delete(expense.id)),
      )
      .subscribe({
        next: () => this.done('Expense deleted.'),
        error: (error: unknown) => this.snackBar.open(errorMessage(error), 'Dismiss'),
      });
  }

  private openDialog(
    expense: Expense | undefined,
    send: (request: ExpenseRequest) => Observable<Expense>,
    successMessage: string,
  ): void {
    const data: ExpenseDialogData = {
      members: this.group().members,
      expense,
      defaultPayerId: this.auth.currentUser()?.id ?? this.group().members[0].id,
    };

    this.dialog
      .open<ExpenseDialog, ExpenseDialogData, ExpenseRequest>(ExpenseDialog, { data })
      .afterClosed()
      .pipe(
        // Cancel closes with '' (mat-dialog-close has no value) and Escape with undefined: only a real request goes on.
        filter(
          (request): request is ExpenseRequest => typeof request === 'object' && request !== null,
        ),
        switchMap(send),
      )
      .subscribe({
        next: () => this.done(successMessage),
        error: (error: unknown) => this.snackBar.open(errorMessage(error), 'Dismiss'),
      });
  }

  private done(message: string): void {
    this.expenses.reload();
    this.snackBar.open(message, undefined, { duration: 3000 });
  }
}

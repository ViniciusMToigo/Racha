import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Expense, ExpenseRequest, User } from '../../core/models/api.models';

export interface ExpenseDialogData {
  members: User[];
  /** Present when editing, absent when adding. */
  expense?: Expense;
  defaultPayerId: string;
}

/** Add/edit form for an expense. Closes with the ExpenseRequest to send, or nothing if cancelled. */
@Component({
  selector: 'app-expense-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './expense-dialog.html',
  styles: `
    .fields {
      display: flex;
      flex-direction: column;
      gap: 4px;
      min-width: min(380px, 80vw);
    }
    .row {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
    }
    .row mat-form-field {
      flex: 1;
      min-width: 150px;
    }
  `,
})
export class ExpenseDialog {
  private readonly dialogRef = inject(MatDialogRef<ExpenseDialog, ExpenseRequest>);
  protected readonly data = inject<ExpenseDialogData>(MAT_DIALOG_DATA);
  protected readonly isEdit = this.data.expense !== undefined;

  protected readonly form = inject(FormBuilder).group({
    description: [
      this.data.expense?.description ?? '',
      [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)],
    ],
    amount: [
      this.data.expense?.amount ?? (null as number | null),
      [Validators.required, Validators.min(0.01), Validators.max(1_000_000), atMostTwoDecimals],
    ],
    date: [this.data.expense?.date ?? todayIso(), Validators.required],
    paidByUserId: [
      this.data.expense?.paidByUserId ?? this.data.defaultPayerId,
      Validators.required,
    ],
  });

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      description: value.description!.trim(),
      amount: Number(value.amount),
      date: value.date!,
      paidByUserId: value.paidByUserId!,
    });
  }
}

function atMostTwoDecimals(control: AbstractControl<number | null>): ValidationErrors | null {
  const value = control.value;
  return value === null || Math.round(value * 100) / 100 === value ? null : { decimals: true };
}

/** Today's date in the user's time zone, as the API expects it (yyyy-MM-dd). */
export function todayIso(now = new Date()): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}

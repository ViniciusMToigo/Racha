import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../config/app-config';
import { Expense, ExpenseRequest, GroupBalances } from '../models/api.models';

/** Expenses are created and listed under their group, then addressed by their own id. */
@Injectable({ providedIn: 'root' })
export class ExpensesApi {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(APP_CONFIG).apiUrl;

  list(groupId: string): Observable<Expense[]> {
    return this.http.get<Expense[]>(`${this.apiUrl}/groups/${groupId}/expenses`);
  }

  create(groupId: string, request: ExpenseRequest): Observable<Expense> {
    return this.http.post<Expense>(`${this.apiUrl}/groups/${groupId}/expenses`, request);
  }

  update(expenseId: string, request: ExpenseRequest): Observable<Expense> {
    return this.http.put<Expense>(`${this.apiUrl}/expenses/${expenseId}`, request);
  }

  delete(expenseId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/expenses/${expenseId}`);
  }

  balances(groupId: string): Observable<GroupBalances> {
    return this.http.get<GroupBalances>(`${this.apiUrl}/groups/${groupId}/balances`);
  }
}

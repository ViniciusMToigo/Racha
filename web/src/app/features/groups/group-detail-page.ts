import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { Router, RouterLink } from '@angular/router';
import { filter, switchMap } from 'rxjs';
import { GroupsApi } from '../../core/api/groups-api';
import { AuthService } from '../../core/auth/auth.service';
import { errorMessage } from '../../core/http/problem-details';
import { Group } from '../../core/models/api.models';
import { ConfirmDialog, ConfirmDialogData } from '../../shared/confirm-dialog';
import { PromptDialog, PromptDialogData } from '../../shared/prompt-dialog';
import { BalancesPanel } from '../balances/balances-panel';
import { ExpensesPanel } from '../expenses/expenses-panel';
import { MembersPanel } from './members-panel';

/** One group: its expenses, balances and members, in tabs. */
@Component({
  selector: 'app-group-detail-page',
  imports: [
    RouterLink,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatProgressBarModule,
    ExpensesPanel,
    BalancesPanel,
    MembersPanel,
  ],
  templateUrl: './group-detail-page.html',
  styleUrl: './group-detail-page.scss',
})
export class GroupDetailPage {
  private readonly groupsApi = inject(GroupsApi);
  private readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  /** The :groupId route parameter, bound by withComponentInputBinding. */
  readonly groupId = input.required<string>();

  protected readonly group = rxResource({
    params: () => this.groupId(),
    stream: ({ params: id }) => this.groupsApi.get(id),
  });

  protected readonly isCreator = computed(
    () =>
      this.group.hasValue() && this.group.value().createdByUserId === this.auth.currentUser()?.id,
  );

  protected readonly notFound = computed(() => {
    const error = this.group.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  protected readonly errorMessage = errorMessage;

  /** The members tab returns the updated group; no need to fetch it again. */
  protected onGroupChanged(group: Group): void {
    this.group.set(group);
  }

  protected rename(group: Group): void {
    this.dialog
      .open<PromptDialog, PromptDialogData, string>(PromptDialog, {
        data: { title: 'Rename group', label: 'Group name', value: group.name },
      })
      .afterClosed()
      .pipe(
        filter((name): name is string => !!name && name !== group.name),
        switchMap((name) => this.groupsApi.rename(group.id, name)),
      )
      .subscribe({
        next: (updated) => {
          this.group.set(updated);
          this.snackBar.open('Group renamed.', undefined, { duration: 3000 });
        },
        error: (error: unknown) => this.snackBar.open(errorMessage(error), 'Dismiss'),
      });
  }

  protected delete(group: Group): void {
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, {
        data: {
          title: `Delete "${group.name}"?`,
          message: 'All of its expenses will be deleted too. This cannot be undone.',
          confirmLabel: 'Delete group',
        },
      })
      .afterClosed()
      .pipe(
        filter((confirmed) => confirmed === true),
        switchMap(() => this.groupsApi.delete(group.id)),
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Group deleted.', undefined, { duration: 3000 });
          void this.router.navigate(['/groups']);
        },
        error: (error: unknown) => this.snackBar.open(errorMessage(error), 'Dismiss'),
      });
  }
}

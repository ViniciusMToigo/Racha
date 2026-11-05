import { Component, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { GroupsApi } from '../../core/api/groups-api';
import { errorMessage } from '../../core/http/problem-details';
import { Group } from '../../core/models/api.models';

/** Shows the group's members and adds registered users by e-mail. */
@Component({
  selector: 'app-members-panel',
  imports: [
    ReactiveFormsModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  template: `
    <form class="add-form" [formGroup]="form" (ngSubmit)="add()">
      <mat-form-field subscriptSizing="dynamic">
        <mat-label>Add a member by email</mat-label>
        <input matInput type="email" formControlName="email" placeholder="bob@racha.dev" />
        @if (email.touched && email.invalid) {
          <mat-error>Enter a valid email.</mat-error>
        }
        @if (error(); as message) {
          <mat-hint class="hint-error">{{ message }}</mat-hint>
        } @else {
          <mat-hint>The person needs a Racha account first.</mat-hint>
        }
      </mat-form-field>
      <button mat-flat-button type="submit" [disabled]="adding()">
        <mat-icon>person_add</mat-icon>
        Add
      </button>
    </form>

    <mat-list>
      @for (member of group().members; track member.id) {
        <mat-list-item>
          <mat-icon matListItemIcon>person</mat-icon>
          <span matListItemTitle>
            {{ member.name }}
            @if (member.id === group().createdByUserId) {
              <span class="badge">creator</span>
            }
          </span>
          <span matListItemLine>{{ member.email }}</span>
        </mat-list-item>
      }
    </mat-list>
  `,
  styles: `
    :host {
      display: block;
      padding-top: 16px;
    }
    .add-form {
      display: flex;
      flex-wrap: wrap;
      align-items: flex-start;
      gap: 12px;
      margin-bottom: 16px;
    }
    .add-form mat-form-field {
      flex: 1;
      min-width: 220px;
    }
    .add-form button {
      height: 56px;
    }
    .hint-error {
      color: var(--mat-sys-error);
    }
    .badge {
      margin-left: 8px;
      padding: 2px 8px;
      border-radius: 8px;
      background: var(--mat-sys-secondary-container);
      color: var(--mat-sys-on-secondary-container);
      font: var(--mat-sys-label-small);
    }
  `,
})
export class MembersPanel {
  private readonly groupsApi = inject(GroupsApi);
  private readonly snackBar = inject(MatSnackBar);

  readonly group = input.required<Group>();
  readonly groupChange = output<Group>();

  protected readonly adding = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
  });
  protected readonly email = this.form.controls.email;

  protected add(): void {
    if (this.email.invalid) {
      this.email.markAsTouched();
      return;
    }

    this.adding.set(true);
    this.error.set(null);

    this.groupsApi.addMember(this.group().id, this.email.value.trim()).subscribe({
      next: (group) => {
        this.groupChange.emit(group);
        this.email.reset();
        this.adding.set(false);
        this.snackBar.open('Member added.', undefined, { duration: 3000 });
      },
      error: (error: unknown) => {
        this.error.set(errorMessage(error));
        this.adding.set(false);
      },
    });
  }
}

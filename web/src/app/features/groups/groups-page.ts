import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router, RouterLink } from '@angular/router';
import { GroupsApi } from '../../core/api/groups-api';
import { errorMessage } from '../../core/http/problem-details';

/** Lists the user's groups and creates new ones. */
@Component({
  selector: 'app-groups-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
  ],
  templateUrl: './groups-page.html',
  styleUrl: './groups-page.scss',
})
export class GroupsPage {
  private readonly groupsApi = inject(GroupsApi);
  private readonly router = inject(Router);

  /** Loading, error and data states come from one resource instead of three hand-managed flags. */
  protected readonly groups = rxResource({ stream: () => this.groupsApi.listMine() });

  protected readonly creating = signal(false);
  protected readonly createError = signal<string | null>(null);
  // Wrapped in a FormGroup so the <form> gets the [formGroup] directive, which is what emits (ngSubmit).
  protected readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)],
    }),
  });
  protected readonly newGroupName = this.form.controls.name;

  protected readonly errorMessage = errorMessage;

  protected create(): void {
    if (this.newGroupName.invalid) {
      this.newGroupName.markAsTouched();
      return;
    }

    this.creating.set(true);
    this.createError.set(null);

    this.groupsApi.create(this.newGroupName.value.trim()).subscribe({
      next: (group) => void this.router.navigate(['/groups', group.id]),
      error: (error: unknown) => {
        this.createError.set(errorMessage(error));
        this.creating.set(false);
      },
    });
  }
}

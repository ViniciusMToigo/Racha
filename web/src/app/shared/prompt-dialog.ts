import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface PromptDialogData {
  title: string;
  label: string;
  value?: string;
  maxLength?: number;
}

/** Asks for one line of text (e.g. a new group name). Closes with the trimmed text. */
@Component({
  selector: 'app-prompt-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <form [formGroup]="form" (ngSubmit)="save()">
      <mat-dialog-content>
        <mat-form-field class="full-width">
          <mat-label>{{ data.label }}</mat-label>
          <input matInput formControlName="text" [maxlength]="maxLength" cdkFocusInitial />
          @if (text.hasError('required')) {
            <mat-error>This field is required.</mat-error>
          }
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close>Cancel</button>
        <button mat-flat-button type="submit">Save</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    .full-width {
      width: 100%;
      min-width: 260px;
    }
  `,
})
export class PromptDialog {
  private readonly dialogRef = inject(MatDialogRef<PromptDialog, string>);
  protected readonly data = inject<PromptDialogData>(MAT_DIALOG_DATA);
  protected readonly maxLength = this.data.maxLength ?? 100;
  protected readonly form = new FormGroup({
    text: new FormControl(this.data.value ?? '', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/)],
    }),
  });
  protected readonly text = this.form.controls.text;

  protected save(): void {
    if (this.text.invalid) {
      this.text.markAsTouched();
      return;
    }
    this.dialogRef.close(this.text.value.trim());
  }
}

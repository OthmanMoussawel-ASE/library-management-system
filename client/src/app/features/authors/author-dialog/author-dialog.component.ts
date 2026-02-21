import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Author } from '../../../core/models/book.model';
import { BookService } from '../../../core/services/book.service';

@Component({
  selector: 'app-author-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEditMode ? 'Edit Author' : 'Add Author' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="author-form">
        <mat-form-field appearance="outline">
          <mat-label>First Name</mat-label>
          <input matInput formControlName="firstName" placeholder="First name" />
          @if (form.get('firstName')?.hasError('required') && form.get('firstName')?.touched) {
            <mat-error>First name is required</mat-error>
          }
          @if (form.get('firstName')?.hasError('maxlength')) {
            <mat-error>Max 100 characters</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Last Name</mat-label>
          <input matInput formControlName="lastName" placeholder="Last name" />
          @if (form.get('lastName')?.hasError('required') && form.get('lastName')?.touched) {
            <mat-error>Last name is required</mat-error>
          }
          @if (form.get('lastName')?.hasError('maxlength')) {
            <mat-error>Max 100 characters</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Biography</mat-label>
          <textarea matInput formControlName="biography" placeholder="Biography (optional)" rows="4"></textarea>
          @if (form.get('biography')?.hasError('maxlength')) {
            <mat-error>Max 2000 characters</mat-error>
          }
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">Cancel</button>
      <button mat-flat-button color="primary" (click)="save()" [disabled]="isSaving || form.invalid">
        {{ isSaving ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .author-form {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      min-width: 400px;
    }
    mat-form-field {
      width: 100%;
    }
  `],
})
export class AuthorDialogComponent {
  private fb = inject(FormBuilder);
  private bookService = inject(BookService);
  private snackBar = inject(MatSnackBar);

  form: FormGroup;
  isEditMode: boolean;
  isSaving = false;

  constructor(
    public dialogRef: MatDialogRef<AuthorDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Author | null
  ) {
    this.isEditMode = !!data;
    this.form = this.fb.group({
      firstName: [data?.fullName?.split(' ')[0] || '', [Validators.required, Validators.maxLength(100)]],
      lastName: [data?.fullName?.split(' ').slice(1).join(' ') || '', [Validators.required, Validators.maxLength(100)]],
      biography: [data?.biography || '', Validators.maxLength(2000)],
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      firstName: this.form.value.firstName.trim(),
      lastName: this.form.value.lastName.trim(),
      biography: this.form.value.biography?.trim() || undefined,
    };

    this.isSaving = true;

    const request$ = this.isEditMode
      ? this.bookService.updateAuthor(this.data!.id, payload)
      : this.bookService.createAuthor(payload);

    request$.subscribe({
      next: () => {
        this.snackBar.open(`Author ${this.isEditMode ? 'updated' : 'created'}`, 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.isSaving = false;
        this.snackBar.open(err?.error?.message || 'Save failed', 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
    });
  }
}

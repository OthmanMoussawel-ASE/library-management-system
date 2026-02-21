import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Category } from '../../../core/models/book.model';
import { BookService } from '../../../core/services/book.service';

@Component({
  selector: 'app-category-dialog',
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
    <h2 mat-dialog-title>{{ isEditMode ? 'Edit Category' : 'Add Category' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="category-form">
        <mat-form-field appearance="outline">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" placeholder="Category name" />
          @if (form.get('name')?.hasError('required') && form.get('name')?.touched) {
            <mat-error>Name is required</mat-error>
          }
          @if (form.get('name')?.hasError('maxlength')) {
            <mat-error>Max 100 characters</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" placeholder="Description (optional)" rows="4"></textarea>
          @if (form.get('description')?.hasError('maxlength')) {
            <mat-error>Max 500 characters</mat-error>
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
    .category-form {
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
export class CategoryDialogComponent {
  private fb = inject(FormBuilder);
  private bookService = inject(BookService);
  private snackBar = inject(MatSnackBar);

  form: FormGroup;
  isEditMode: boolean;
  isSaving = false;

  constructor(
    public dialogRef: MatDialogRef<CategoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Category | null
  ) {
    this.isEditMode = !!data;
    this.form = this.fb.group({
      name: [data?.name || '', [Validators.required, Validators.maxLength(100)]],
      description: [data?.description || '', Validators.maxLength(500)],
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      name: this.form.value.name.trim(),
      description: this.form.value.description?.trim() || undefined,
    };

    this.isSaving = true;

    const request$ = this.isEditMode
      ? this.bookService.updateCategory(this.data!.id, payload)
      : this.bookService.createCategory(payload);

    request$.subscribe({
      next: () => {
        this.snackBar.open(`Category ${this.isEditMode ? 'updated' : 'created'}`, 'Close', {
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

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Book } from '../../../core/models/book.model';
import { BookService } from '../../../core/services/book.service';
import { AuthService } from '../../../core/services/auth.service';
import { CheckoutService } from '../../../core/services/checkout.service';

@Component({
  selector: 'app-book-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatSnackBarModule,
  ],
  templateUrl: './book-detail.component.html',
  styleUrl: './book-detail.component.scss',
})
export class BookDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private bookService = inject(BookService);
  private authService = inject(AuthService);
  private checkoutService = inject(CheckoutService);
  private snackBar = inject(MatSnackBar);

  book: Book | null = null;
  isLoading = true;

  get isLibrarian(): boolean {
    return this.authService.isLibrarian;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/books']);
      return;
    }
    this.bookService.getBook(id).subscribe({
      next: (book) => {
        this.book = book;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.snackBar.open('Book not found', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.router.navigate(['/books']);
      },
    });
  }

  checkout(): void {
    if (!this.book || this.book.availableCopies <= 0) return;
    this.checkoutService
      .checkoutBook({ bookId: this.book.id, dueDays: 14 })
      .subscribe({
        next: () => {
          this.snackBar.open('Book checked out successfully', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          });
          this.bookService.getBook(this.book!.id).subscribe((b) => (this.book = b));
        },
        error: (err) => {
          this.snackBar.open(
            err?.error?.message || 'Checkout failed',
            'Close',
            {
              duration: 5000,
              horizontalPosition: 'end',
              verticalPosition: 'top',
            }
          );
        },
      });
  }

  deleteBook(): void {
    if (!this.book) return;
    if (!confirm(`Are you sure you want to delete "${this.book.title}"?`)) return;
    this.bookService.deleteBook(this.book.id).subscribe({
      next: () => {
        this.snackBar.open('Book deleted', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.router.navigate(['/books']);
      },
      error: (err) => {
        this.snackBar.open(
          err?.error?.message || 'Delete failed',
          'Close',
          {
            duration: 5000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          }
        );
      },
    });
  }
}

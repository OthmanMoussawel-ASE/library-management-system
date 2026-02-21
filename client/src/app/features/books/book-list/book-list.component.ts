import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Book } from '../../../core/models/book.model';
import { BookService } from '../../../core/services/book.service';
import { AuthService } from '../../../core/services/auth.service';
import { CheckoutService } from '../../../core/services/checkout.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-book-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './book-list.component.html',
  styleUrl: './book-list.component.scss',
})
export class BookListComponent implements OnInit {
  private bookService = inject(BookService);
  private authService = inject(AuthService);
  private checkoutService = inject(CheckoutService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  displayedColumns: string[] = ['title', 'authorName', 'availableCopies', 'categories', 'actions'];
  dataSource = new MatTableDataSource<Book>([]);
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  sortBy = 'title';
  sortDirection = 'asc';
  searchTerm = '';
  isLoading = false;
  isSmartSearching = false;
  smartSearchMode = false;

  private searchSubject = new Subject<string>();

  get isLibrarian(): boolean {
    return this.authService.isLibrarian;
  }

  ngOnInit(): void {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((term) => {
        this.searchTerm = term;
        this.pageIndex = 0;
        this.loadBooks();
      });
    this.loadBooks();
  }

  onSearchChange(value: string): void {
    this.searchSubject.next(value);
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadBooks();
  }

  onSortChange(sort: Sort): void {
    if (sort.active && sort.direction) {
      this.sortBy = sort.active;
      this.sortDirection = sort.direction;
      this.pageIndex = 0;
      this.loadBooks();
    }
  }

  loadBooks(): void {
    this.isLoading = true;
    this.bookService
      .getBooks({
        pageNumber: this.pageIndex + 1,
        pageSize: this.pageSize,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
        searchTerm: this.searchTerm || undefined,
      })
      .subscribe({
        next: (result) => {
          this.dataSource.data = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.snackBar.open('Failed to load books', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          });
        },
      });
  }

  checkout(book: Book): void {
    if (book.availableCopies <= 0) return;
    this.checkoutService
      .checkoutBook({ bookId: book.id, dueDays: 14 })
      .subscribe({
        next: () => {
          this.snackBar.open('Book checked out successfully', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          });
          this.loadBooks();
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

  deleteBook(book: Book): void {
    if (!confirm(`Are you sure you want to delete "${book.title}"?`)) return;
    this.bookService.deleteBook(book.id).subscribe({
      next: () => {
        this.snackBar.open('Book deleted', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.loadBooks();
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

  toggleSmartSearch(): void {
    this.smartSearchMode = !this.smartSearchMode;
    if (!this.smartSearchMode) {
      this.searchTerm = '';
      this.pageIndex = 0;
      this.loadBooks();
    }
  }

  onSmartSearch(query: string): void {
    if (!query.trim()) return;

    this.isSmartSearching = true;
    this.bookService.smartSearch(query).subscribe({
      next: (result) => {
        const terms: string[] = [];
        
        // Prioritize author - most reliable match
        if (result.author) {
          // Take just the last name if it's a full name
          const authorParts = result.author.trim().split(' ');
          terms.push(authorParts[authorParts.length - 1]);
        }
        
        // Add genre if present (single word)
        if (result.genre) {
          const genre = result.genre.split(/[\s,]+/)[0];
          if (genre && genre.length > 2) terms.push(genre);
        }
        
        // Only add first keyword if we have very few terms
        if (result.keywords && terms.length < 2) {
          const keyword = result.keywords.split(/[\s,]+/)[0];
          if (keyword && keyword.length > 2) terms.push(keyword);
        }

        this.searchTerm = terms.slice(0, 3).join(' ');
        this.pageIndex = 0;
        this.isSmartSearching = false;
        this.loadBooks();

        this.snackBar.open(`Searching for: ${this.searchTerm || 'all books'}`, 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
      error: () => {
        this.isSmartSearching = false;
        this.snackBar.open('Smart search failed, using regular search', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.searchTerm = query;
        this.loadBooks();
      },
    });
  }
}

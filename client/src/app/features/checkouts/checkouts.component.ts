import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CheckoutService } from '../../core/services/checkout.service';
import { AuthService } from '../../core/services/auth.service';
import { CheckoutRecord } from '../../core/models/checkout.model';

@Component({
  selector: 'app-checkouts',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './checkouts.component.html',
  styleUrl: './checkouts.component.scss',
})
export class CheckoutsComponent implements OnInit {
  private checkoutService = inject(CheckoutService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  displayedColumns: string[] = [];

  private initColumns(): void {
    this.displayedColumns = this.isLibrarian
      ? ['bookTitle', 'patron', 'checkedOutAt', 'dueDate', 'status', 'actions']
      : ['bookTitle', 'checkedOutAt', 'dueDate', 'status', 'actions'];
  }
  dataSource = new MatTableDataSource<CheckoutRecord>([]);
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  sortBy = 'checkedoutat';
  sortDirection = 'desc';
  searchTerm = '';
  isLoading = false;

  private searchSubject = new Subject<string>();

  get isLibrarian(): boolean {
    return this.authService.isLibrarian;
  }

  ngOnInit(): void {
    this.initColumns();
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.pageIndex = 0;
        this.loadCheckouts();
      });
    this.loadCheckouts();
  }

  onSearch(): void {
    this.searchSubject.next(this.searchTerm);
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadCheckouts();
  }

  onSortChange(sort: Sort): void {
    if (sort.active && sort.direction) {
      this.sortBy = sort.active;
      this.sortDirection = sort.direction;
      this.pageIndex = 0;
      this.loadCheckouts();
    }
  }

  loadCheckouts(): void {
    this.isLoading = true;
    this.checkoutService
      .getCheckouts({
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
          this.snackBar.open('Failed to load checkouts', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          });
        },
      });
  }

  returnBook(checkout: CheckoutRecord): void {
    if (checkout.status !== 'Active') return;

    this.checkoutService.returnBook(checkout.id).subscribe({
      next: () => {
        this.snackBar.open('Book returned successfully', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.loadCheckouts();
      },
      error: (err) => {
        this.snackBar.open(
          err?.error?.message || 'Return failed',
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

  getStatusChipClass(status: string, isOverdue: boolean): string {
    if (isOverdue) return 'chip-overdue';
    switch (status) {
      case 'Active':
        return 'chip-active';
      case 'Returned':
        return 'chip-returned';
      default:
        return 'chip-default';
    }
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}

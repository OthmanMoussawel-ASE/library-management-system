import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { environment } from '../../../../environments/environment';
import { User } from '../../../core/models/auth.model';

interface UserApiResponse {
  items?: User[];
  totalCount?: number;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
})
export class UsersComponent implements OnInit {
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);

  private readonly apiUrl = `${environment.apiUrl}/users`;

  displayedColumns: string[] = ['email', 'firstName', 'lastName', 'role', 'createdAt', 'actions'];
  users: User[] = [];
  isLoading = false;
  readonly roles = ['Admin', 'Librarian', 'Patron'];

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.http.get<User[] | UserApiResponse>(this.apiUrl).subscribe({
      next: (response) => {
        this.users = Array.isArray(response) ? response : (response.items ?? []);
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.snackBar.open('Failed to load users', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
    });
  }

  onRoleChange(user: User, newRole: string): void {
    if (user.role === newRole) return;

    this.http
      .put(`${this.apiUrl}/${user.id}/role`, { role: newRole })
      .subscribe({
        next: () => {
          user.role = newRole;
          this.snackBar.open('Role updated successfully', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
          });
        },
        error: (err) => {
          this.snackBar.open(
            err?.error?.error || 'Failed to update role',
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

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}

import { Routes } from '@angular/router';
import { librarianGuard } from '../../core/guards/auth.guard';

export const BOOKS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./book-list/book-list.component').then((m) => m.BookListComponent),
  },
  {
    path: 'create',
    canActivate: [librarianGuard],
    loadComponent: () =>
      import('./book-form/book-form.component').then((m) => m.BookFormComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./book-detail/book-detail.component').then((m) => m.BookDetailComponent),
  },
  {
    path: ':id/edit',
    canActivate: [librarianGuard],
    loadComponent: () =>
      import('./book-form/book-form.component').then((m) => m.BookFormComponent),
  },
];

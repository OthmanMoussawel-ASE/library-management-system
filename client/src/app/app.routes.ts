import { Routes } from '@angular/router';
import { authGuard, adminGuard, librarianGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'books',
    canActivate: [authGuard],
    loadChildren: () => import('./features/books/books.routes').then(m => m.BOOKS_ROUTES)
  },
  {
    path: 'checkouts',
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkouts/checkouts.component').then(m => m.CheckoutsComponent)
  },
  {
    path: 'authors',
    canActivate: [librarianGuard],
    loadChildren: () => import('./features/authors/authors.routes').then(m => m.authorsRoutes)
  },
  {
    path: 'categories',
    canActivate: [librarianGuard],
    loadChildren: () => import('./features/categories/categories.routes').then(m => m.categoriesRoutes)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
  },
  { path: '**', redirectTo: '/dashboard' }
];

import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./users/users.component').then((m) => m.UsersComponent),
  },
];

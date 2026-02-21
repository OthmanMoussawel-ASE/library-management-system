import { Routes } from '@angular/router';

export const authorsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./author-list/author-list.component').then((m) => m.AuthorListComponent),
  },
];

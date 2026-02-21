import { Routes } from '@angular/router';

export const categoriesRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./category-list/category-list.component').then((m) => m.CategoryListComponent),
  },
];

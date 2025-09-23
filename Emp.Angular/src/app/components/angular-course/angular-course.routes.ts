import { Routes } from '@angular/router';

export const angularCourseRoutes: Routes = [
  { path: '', redirectTo: 'profile-card', pathMatch: 'full' },
  {
    path: 'profile-card',
    loadComponent: () =>
      import('./profile-card/profile-card').then((m) => m.ProfileCard),
  },
  {
    path: 'counter',
    loadComponent: () => import('./counter/counter').then((m) => m.Counter),
  },
];

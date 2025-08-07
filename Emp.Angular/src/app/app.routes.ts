import { Routes } from '@angular/router';
import { LandingPage } from './components/landing-page/landing-page';

export const routes: Routes = [
  // landing page is eagerly loaded route
  { path: '', component: LandingPage },

  // lazy loaded routes
  {
    path: 'login',
    loadComponent: () =>
      import('./components/login/login').then((x) => x.Login),
  },

  {
    path: 'about-me',
    loadComponent: () =>
      import('./components/about-me/about-me').then((x) => x.AboutMe),
  },

  {
    path: 'about-demo',
    loadComponent: () =>
      import('./components/about-demo/about-demo').then((x) => x.AboutDemo),
  },
];

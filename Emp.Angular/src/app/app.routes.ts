import { Routes } from '@angular/router';
import { LandingPage } from './components/landing-page/landing-page';
import { UserRoles } from './models/userRoles';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  // landing page is eagerly loaded route
  { path: '', component: LandingPage },

  // lazy loaded routes
  // anonymouse
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

  // staff and admin
  {
    path: 'manage-departments',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin, UserRoles.Staff] },
    loadComponent: () =>
      import('./components/auth/manage-departments/manage-departments').then(
        (x) => x.ManageDepartments
      ),
  },

  {
    path: 'manage-departments/create',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin, UserRoles.Staff] },
    loadComponent: () =>
      import(
        './components/auth/manage-departments/create-departments/create-departments'
      ).then((x) => x.CreateDepartments),
  },

  {
    path: 'manage-departments/edit/:id',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin, UserRoles.Staff] },
    loadComponent: () =>
      import(
        './components/auth/manage-departments/edit-departments/edit-departments'
      ).then((x) => x.EditDepartments),
  },

  {
    path: 'manage-employees',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin, UserRoles.Staff] },
    loadComponent: () =>
      import('./components/auth/manage-employees/manage-employees').then(
        (x) => x.ManageEmployees
      ),
  },
  {
    path: 'manage-profile',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin, UserRoles.Staff] },
    loadComponent: () =>
      import('./components/auth/manage-profile/manage-profile').then(
        (x) => x.ManageProfile
      ),
  },

  // admin only
  {
    path: 'manage-app-users',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin] },
    loadComponent: () =>
      import('./components/auth/admin/manage-app-users/manage-app-users').then(
        (x) => x.ManageAppUsers
      ),
  },

  {
    path: 'manage-app-users/create',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin] },
    loadComponent: () =>
      import(
        './components/auth/admin/manage-app-users/create-users/create-users'
      ).then((x) => x.CreateUsers),
  },

  {
    path: 'manage-app-users/edit/:id',
    canActivate: [authGuard],
    data: { roles: [UserRoles.Admin] },
    loadComponent: () =>
      import(
        './components/auth/admin/manage-app-users/edit-users/edit-users'
      ).then((x) => x.EditUsers),
  },

  // catch all route
  {
    path: '**',
    redirectTo: '', // landing page
  },
];

import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guards';

// Pages are lazy-loaded: each one is only downloaded when the user first visits it.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'groups' },
  {
    path: 'login',
    canActivate: [guestGuard],
    title: 'Log in · Racha',
    loadComponent: () => import('./features/auth/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    title: 'Create account · Racha',
    loadComponent: () => import('./features/auth/register-page').then((m) => m.RegisterPage),
  },
  {
    path: 'groups',
    canActivate: [authGuard],
    title: 'My groups · Racha',
    loadComponent: () => import('./features/groups/groups-page').then((m) => m.GroupsPage),
  },
  {
    path: 'groups/:groupId',
    canActivate: [authGuard],
    title: 'Group · Racha',
    loadComponent: () =>
      import('./features/groups/group-detail-page').then((m) => m.GroupDetailPage),
  },
  { path: '**', redirectTo: 'groups' },
];

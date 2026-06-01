import { Routes } from '@angular/router';
import { HomeComponent } from './features/user/home/home.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AdminSystemComponent } from './features/admin-system/admin-system.component';
import { AdminTheaterComponent } from './features/admin-theater/admin-theater.component';
import { UnauthorizedComponent } from './shared/components/unauthorized/unauthorized.component';
import { systemAdminGuard, theaterOwnerGuard } from './core/guards/auth.guards';

export const routes: Routes = [
  // User/Anonymous routes
  { path: '', component: HomeComponent },

  // Auth routes
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },

  // Unauthorized error page
  { path: 'unauthorized', component: UnauthorizedComponent },

  // System Admin (Super Admin) Section
  {
    path: 'admin-system',
    component: AdminSystemComponent,
    // canActivate: [systemAdminGuard],
    children: [
      { path: '', redirectTo: 'movies', pathMatch: 'full' },
      {
        path: 'movies',
        loadComponent: () => import('./features/admin-system/movies/list/movies-list.component').then(m => m.AdminMoviesListComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/admin-system/categories/list/categories-list.component').then(m => m.AdminCategoriesListComponent)
      },
      {
        path: 'theaters',
        loadComponent: () => import('./features/admin-system/theaters/list/theaters-list.component').then(m => m.AdminTheatersListComponent)
      },
      {
        path: 'accounts',
        loadComponent: () => import('./features/admin-system/accounts/list/accounts-list.component').then(m => m.AdminAccountsListComponent)
      },
      {
        path: 'roles',
        loadComponent: () => import('./features/admin-system/roles/list/roles-list.component').then(m => m.AdminRolesListComponent)
      }
    ]
  },

  // Theater Owner Admin Section
  {
    path: 'admin-theater',
    component: AdminTheaterComponent,
    // canActivate: [theaterOwnerGuard],
    children: [
      { path: '', redirectTo: 'rooms', pathMatch: 'full' },
      {
        path: 'rooms',
        loadComponent: () => import('./features/admin-theater/rooms/list/rooms-list.component').then(m => m.TheaterRoomsListComponent)
      },
      {
        path: 'showtimes',
        loadComponent: () => import('./features/admin-theater/showtimes/list/showtimes-list.component').then(m => m.TheaterShowtimesListComponent)
      }
    ]
  },

  // Fallback
  { path: '**', redirectTo: '' }
];

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const userGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Redirect to login page
  router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

export const systemAdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.isSystemAdmin()) {
    return true;
  }

  router.navigate(['/unauthorized']);
  return false;
};

export const theaterOwnerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.isTheaterOwner()) {
    return true;
  }

  router.navigate(['/unauthorized']);
  return false;
};

export const payingGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const navigation = router.getCurrentNavigation();

  // Verify navigation state from the booking page
  const hasState = navigation?.extras.state && 
                   navigation.extras.state['showingId'] && 
                   navigation.extras.state['seatIds'] && 
                   navigation.extras.state['seatIds'].length > 0;

  if (hasState) {
    return true;
  }

  // Also check if history.state is populated (e.g. page refresh)
  const historyState = window.history.state;
  if (historyState && historyState.showingId && historyState.seatIds && historyState.seatIds.length > 0) {
    return true;
  }

  // Redirect to home if direct URL access is attempted
  router.navigate(['/']);
  return false;
};


import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { JwtTokenService } from '../services/jwt.token.service';
import { UserRoles } from '../models/userRoles';

export const authGuard: CanActivateFn = (route, state) => {
  const expectedRoles = route.data['roles'] as UserRoles[];
  if (!expectedRoles || expectedRoles.length === 0) {
    return true; // anonymous
  }

  const jwtService = inject(JwtTokenService);
  const role: UserRoles | null = jwtService.getUserRole();

  if (!role) {
    inject(Router).navigate(['/login']);
    return false; // we expect a role, but token does not have a role
  }

  return expectedRoles.includes(role);
};

import { Injectable } from '@angular/core';
import { UserDto } from '../models/user.dto';
import { jwtDecode } from 'jwt-decode';
import { UserRoles } from '../models/userRoles';

@Injectable({
  providedIn: 'root',
})
export class JwtTokenService {
  // local storage key for jwt auth token
  private readonly jwtTokenKey: string = 'jwtToken';

  setJwtToken(token: string): void {
    localStorage.setItem(this.jwtTokenKey, token);
  }

  readJwtAuthToken(): string | null {
    return localStorage.getItem(this.jwtTokenKey);
  }

  removeToken(): void {
    localStorage.removeItem(this.jwtTokenKey);
  }

  // extract claims from JWT token
  decodeToken(token: string): UserDto | null {
    try {
      const decodedToken: any = jwtDecode(token);
      let user: UserDto = {
        id: parseInt(decodedToken.nameid),
        username: decodedToken.unique_name,
        firstName: decodedToken.given_name || '',
        lastName: decodedToken.family_name || '',
        role: decodedToken.role as UserRoles,
        isActive: decodedToken.is_active === true,
      };
      return user;
    } catch (e) {
      console.error('Error decoding token:', e);
      return null;
    }
  }

  isTokenExpired(token: string): boolean {
    try {
      const decoded: any = jwtDecode(token);
      const exp = decoded.exp;

      if (!exp) return true; // No expiration claim — treat as expired

      const currentTime = Math.floor(Date.now() / 1000); // Current time in seconds
      return exp < currentTime; // true if expired
    } catch (e) {
      console.error('Error decoding token:', e);
      return true; // If decoding fails, treat as expired
    }
  }

  getUserRole(): UserRoles | null {
    const token = this.readJwtAuthToken();
    if (!token || this.isTokenExpired(token)) {
      return null;
    }
    const userDto = this.decodeToken(token);
    if (!userDto) {
      return null;
    }
    return userDto.role;
  }
}

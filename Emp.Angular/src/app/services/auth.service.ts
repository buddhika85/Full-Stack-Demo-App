import { inject, Injectable, signal } from '@angular/core';
import { LoginResponseDto } from '../models/loginResponse.dto';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginDto } from '../models/login.dto';
import { UserDto } from '../models/user.dto';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private httpClient: HttpClient = inject(HttpClient);
  private baseUrl: string = `${environment.apiUrl}`;

  currentUser = signal<UserDto | null>(null);

  constructor() {
    const token = localStorage.getItem('jwtToken');

    if (token && !this.isTokenExpired(token)) {
      let user: UserDto | null = this.decodeToken(token);
      if (user) {
        this.currentUser.set(user);
      } else {
        this.removeToken();
      }
    } else {
      console.log('Token expired - so remove from local storage');
      this.removeToken();
    }
  }

  // extract claims from JWT token
  private decodeToken(token: string): UserDto | null {
    try {
      const decodedToken: any = jwtDecode(token);
      let user: UserDto = {
        id: parseInt(decodedToken.nameid),
        username: decodedToken.unique_name,
        firstName: decodedToken.given_name || '',
        lastName: decodedToken.family_name || '',
        role: decodedToken.role,
        isActive: decodedToken.is_active === true,
      };
      return user;
    } catch (e) {
      console.error('Error decoding token:', e);
      return null;
    }
  }

  private isTokenExpired(token: string): boolean {
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

  login(loginDto: LoginDto): Observable<LoginResponseDto> {
    return this.httpClient
      .post<LoginResponseDto>(`${this.baseUrl}/auth/login`, loginDto)
      .pipe(
        tap((response) => {
          localStorage.setItem('jwtToken', response.token);
          this.currentUser.set(response.user);
        }),
        catchError((error) => {
          console.error('Login failed:', error);
          this.removeToken();
          return of(error);
        })
      );
  }

  removeToken(): void {
    localStorage.removeItem('jwtToken');
    this.currentUser.set(null);
  }

  logout(): void {
    this.removeToken();
  }
}

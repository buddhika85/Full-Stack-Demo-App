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
    debugger;
    const token = localStorage.getItem('jwtToken');
    let user: UserDto | null = null;
    if (token) {
      try {
        const decodedToken: any = jwtDecode(token);
        user = {
          id: parseInt(decodedToken.nameid),
          username: decodedToken.name,
          firstName: decodedToken.given_name || '',
          lastName: decodedToken.family_name || '',
          role: decodedToken.role,
          isActive: true,
        };
      } catch (e) {
        console.error('Error decoding token:', e);
        localStorage.removeItem('jwtToken');
      }
    }
    this.currentUser.set(user);
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
          this.logout();
          return of(error);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('jwtToken');
    this.currentUser.set(null);
  }
}

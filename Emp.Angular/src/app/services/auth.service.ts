import { inject, Injectable, signal } from '@angular/core';
import { LoginResponseDto } from '../models/loginResponse.dto';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginDto } from '../models/login.dto';
import { UserDto } from '../models/user.dto';
import { JwtTokenService } from './jwt.token.service';
import { LogoutResponseDto } from '../models/logoutResponse.dto';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl: string = `${environment.apiUrl}`;

  private readonly jwtTokenService: JwtTokenService = inject(JwtTokenService);

  currentUser = signal<UserDto | null>(null);

  // on refresh of pages - auth service will decode token if exists
  constructor() {
    const token = this.jwtTokenService.readJwtAuthToken();

    if (token) {
      if (this.jwtTokenService.isTokenExpired(token)) {
        console.log('Token expired - so remove from local storage');
        this.jwtTokenService.removeToken();
      } else {
        let user: UserDto | null = this.jwtTokenService.decodeToken(token);
        if (user) {
          this.currentUser.set(user);
        } else {
          this.jwtTokenService.removeToken();
        }
      }
    }
  }

  login(loginDto: LoginDto): Observable<LoginResponseDto> {
    return this.httpClient
      .post<LoginResponseDto>(`${this.baseUrl}/auth/login`, loginDto)
      .pipe(
        tap((response) => {
          this.jwtTokenService.setJwtToken(response.token);
          this.currentUser.set(response.user);
        }),
        catchError((error) => {
          console.error('Login failed:', error);
          this.cleanTokenUser();
          return of(error);
        })
      );
  }

  cleanTokenUser(): void {
    this.jwtTokenService.removeToken();
    this.currentUser.set(null);
  }

  logout(): Observable<LogoutResponseDto> {
    return this.httpClient
      .post<LogoutResponseDto>(`${this.baseUrl}/auth/logout`, null)
      .pipe(
        tap((response) => {
          if (response.loggedOut) {
            this.cleanTokenUser();
          } else {
            console.error('Logout failed:');
          }
        }),
        catchError((error) => {
          console.error('Logout failed:', error);
          this.cleanTokenUser();
          return of(error);
        })
      );
  }
}

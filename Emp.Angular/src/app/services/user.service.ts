import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { UserDto } from '../models/user.dto';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl: string = `${environment.apiUrl}/Users`;

  getUsers(): Observable<UserDto[]> {
    return this.httpClient.get<UserDto[]>(this.baseUrl);
  }

  getUser(id: number): Observable<UserDto> {
    return this.httpClient.get<UserDto>(`${this.baseUrl}/${id}`);
  }
}

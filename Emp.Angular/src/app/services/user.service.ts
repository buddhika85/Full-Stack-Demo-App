import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { UserDto } from '../models/user.dto';
import { CreateUserDto } from '../models/createUser.dto';
import { UpdateUserDto } from './../models/updateUser.dto';

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

  createUser(createUserDto: CreateUserDto): Observable<UserDto> {
    return this.httpClient.post<UserDto>(this.baseUrl, createUserDto);
  }

  updateUser(id: number, updateUserDto: UpdateUserDto): Observable<void> {
    // returns no content - 204 result
    return this.httpClient.put<void>(`${this.baseUrl}/${id}`, updateUserDto);
  }

  activateDeactivateUser(id: number): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
  }
}

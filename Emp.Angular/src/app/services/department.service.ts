import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DepartmentDto } from '../models/department.dto';
import { CreateDepartmentDto } from './../models/createDepartment.dto';
import { UpdateDepartmentDto } from './../models/updateDepartment.dto';

@Injectable({
  providedIn: 'root',
})
export class DepartmentService {
  private readonly baseUrl: string = `${environment.apiUrl}/department`;
  private readonly httpClient: HttpClient = inject(HttpClient);

  getDepartments(): Observable<DepartmentDto[]> {
    return this.httpClient.get<DepartmentDto[]>(this.baseUrl);
  }

  getDepartmentById(id: number): Observable<DepartmentDto> {
    return this.httpClient.get<DepartmentDto>(`${this.baseUrl}/${id}`);
  }

  createDepartment(
    createDepartmentDto: CreateDepartmentDto
  ): Observable<DepartmentDto> {
    return this.httpClient.post<DepartmentDto>(
      this.baseUrl,
      createDepartmentDto
    );
  }

  updateDepartment(
    id: number,
    updateDepartmentDto: UpdateDepartmentDto
  ): Observable<void> {
    return this.httpClient.put<void>(
      `${this.baseUrl}/${id}`,
      updateDepartmentDto
    );
  }

  deleteDepartment(id: number): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
  }
}

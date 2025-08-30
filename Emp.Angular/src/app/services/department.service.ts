import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DepartmentDto } from '../models/department.dto';

@Injectable({
  providedIn: 'root',
})
export class DepartmentService {
  private readonly baseUrl: string = `${environment.apiUrl}/department`;
  private readonly httpClient: HttpClient = inject(HttpClient);

  getDepartments(): Observable<DepartmentDto[]> {
    return this.httpClient.get<DepartmentDto[]>(this.baseUrl);
  }
}

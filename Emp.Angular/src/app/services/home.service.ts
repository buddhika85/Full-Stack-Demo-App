import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { LandingDto } from '../models/landing.dtos';

@Injectable({
  providedIn: 'root',
})
export class HomeService {
  private httpClient: HttpClient = inject(HttpClient);
  private baseUrl: string = `${environment.apiUrl}`;

  landingContent(): Observable<LandingDto> {
    return this.httpClient.get<LandingDto>(`${this.baseUrl}/Home`);
  }
}

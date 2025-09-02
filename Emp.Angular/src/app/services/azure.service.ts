import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AzPostToAzureFunc } from '../models/azPostToAzureFunc.dto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AzureService {
  private readonly httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl: string = environment.apiUrl;

  // Posts a number to API, which inturn posts it to a Azure Function - Publish_To_Service_Bus_Function
  // This azure function will post that number to Azure Service Bus Topic
  postToAzureFunction(postData: AzPostToAzureFunc): Observable<string> {
    return this.httpClient.post<string>(this.baseUrl, postData);
  }
}

import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AzPostToAzureFunc } from '../models/azPostToAzureFunc.dto';
import { environment } from '../../environments/environment';
import { AzPayloadReceivedDto } from '../models/azPayloadReceived.dto';
import { AzNumberListDto } from '../models/azNumberListDto';

@Injectable({
  providedIn: 'root',
})
export class AzureService {
  private readonly httpClient: HttpClient = inject(HttpClient);
  private readonly baseUrl: string = environment.apiUrl;

  // Posts a number to API, which inturn posts it to a Azure Function - Publish_To_Service_Bus_Function
  // This azure function will post that number to Azure Service Bus Topic
  postToAzureFunction(
    postData: AzPostToAzureFunc
  ): Observable<AzPayloadReceivedDto> {
    return this.httpClient.post<AzPayloadReceivedDto>(
      `${this.baseUrl}/AzureIntegration/CallPublishToAzureService_Fn`,
      postData
    );
  }

  // Calls .NET backend API
  // It calls another API hosted on Azure App Service which only returns Even numbers published to Azure Service Bus Topic
  getAllEvenNumbers(): Observable<AzNumberListDto> {
    return this.httpClient.get<AzNumberListDto>(
      `${this.baseUrl}/AzureIntegration/GetAllFromAzureApiAppService`
    );
  }

  // Calls .NET backend API
  // It calls another API hosted on Azure Container Instance which only returns Odd numbers published to Azure Service Bus Topic
  getAllOddNumbers(): Observable<AzNumberListDto> {
    return this.httpClient.get<AzNumberListDto>(
      `${this.baseUrl}/AzureIntegration/GetAllFromAzureContainerInstanceApi`
    );
  }
}

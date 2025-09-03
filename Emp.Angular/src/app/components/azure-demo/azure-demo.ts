import { Component, inject } from '@angular/core';
import { AzureService } from '../../services/azure.service';
import { AzureDataForm } from './azure-data-form/azure-data-form';

@Component({
  selector: 'app-azure-demo',
  imports: [AzureDataForm],
  templateUrl: './azure-demo.html',
  styleUrl: './azure-demo.scss',
})
export class AzureDemo {
  private readonly azureService: AzureService = inject(AzureService);
}

import { Component, inject } from '@angular/core';
import { AzureService } from '../../services/azure.service';

@Component({
  selector: 'app-azure-demo',
  imports: [],
  templateUrl: './azure-demo.html',
  styleUrl: './azure-demo.scss',
})
export class AzureDemo {
  private readonly azureService: AzureService = inject(AzureService);
}

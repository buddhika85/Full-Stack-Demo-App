import { Component } from '@angular/core';
import { AzureDataForm } from './azure-data-form/azure-data-form';

@Component({
  selector: 'app-azure-demo',
  imports: [AzureDataForm],
  templateUrl: './azure-demo.html',
  styleUrl: './azure-demo.scss',
})
export class AzureDemo {}

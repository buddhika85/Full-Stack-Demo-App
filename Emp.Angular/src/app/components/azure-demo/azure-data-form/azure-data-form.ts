import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { AzureService } from '../../../services/azure.service';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AzPostToAzureFunc } from '../../../models/azPostToAzureFunc.dto';
import { Subscription } from 'rxjs';
import { AzPayloadReceivedDto } from '../../../models/azPayloadReceived.dto';
import { AzNumberListDto } from '../../../models/azNumberListDto';
import { AzureNumbersDisplayGrid } from '../azure-numbers-display-grid/azure-numbers-display-grid';

@Component({
  selector: 'app-azure-data-form',
  imports: [ReactiveFormsModule, AzureNumbersDisplayGrid],
  templateUrl: './azure-data-form.html',
  styleUrl: './azure-data-form.scss',
})
export class AzureDataForm implements OnInit, OnDestroy {
  private readonly compositeSubscription: Subscription = new Subscription();
  private readonly azureService: AzureService = inject(AzureService);

  private formDto: AzPostToAzureFunc = {
    number: null,
  };
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  formGroup!: FormGroup<{ number: FormControl<number | null> }>;

  oddNumbersDto!: AzNumberListDto;
  evenNumbersDto!: AzNumberListDto;

  evenNumbersHeading: string =
    'Even Numbers Published to Azure Service Bus Topic';
  oddNumbersHeading: string =
    'Odd Numbers Published to Azure Service Bus Topic';

  ngOnInit(): void {
    this.getAllEvenNumbersPosted();
    this.getAllOddNumbersPosted();
    this.createForm();
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }

  get numberInput(): FormControl<number | null> {
    return this.formGroup.controls.number;
  }

  onSubmit(): void {
    if (this.formGroup.valid) {
      this.formDto = this.formGroup.getRawValue();
      // console.log(this.formDto);

      this.postToAzureFunction();
    }
  }

  onReset(): void {
    this.formGroup.reset();
  }

  private postToAzureFunction(): void {
    const sub = this.azureService.postToAzureFunction(this.formDto).subscribe({
      next: (value: AzPayloadReceivedDto) => {
        if (value && value.isSuccess) {
          console.log(value.message);
          this.getAllEvenNumbersPosted();
          this.getAllOddNumbersPosted();
          this.formGroup.reset();
        }
      },
      error: (error: any) => {
        console.error(error);
      },
    });

    this.compositeSubscription.add(sub);
  }

  private getAllOddNumbersPosted(): void {
    const sub = this.azureService.getAllOddNumbers().subscribe({
      next: (value: AzNumberListDto) => {
        if (value && value.isSuccess) {
          //console.log('Odd: ', value.items);
          this.oddNumbersDto = value;
        }
      },
      error: (error: any) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  private getAllEvenNumbersPosted(): void {
    const sub = this.azureService.getAllEvenNumbers().subscribe({
      next: (value: AzNumberListDto) => {
        if (value && value.isSuccess) {
          //console.log('Even: ', value.items);
          this.evenNumbersDto = value;
        }
      },
      error: (error: any) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  private createForm(): void {
    this.formGroup = this.formBuilder.group({
      number: new FormControl(this.formDto.number, {
        nonNullable: false,
        validators: [Validators.required],
      }),
    });
  }
}

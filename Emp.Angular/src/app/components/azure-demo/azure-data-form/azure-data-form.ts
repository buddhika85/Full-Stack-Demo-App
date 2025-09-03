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

@Component({
  selector: 'app-azure-data-form',
  imports: [ReactiveFormsModule],
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

  ngOnInit(): void {
    this.formGroup = this.formBuilder.group({
      number: new FormControl(this.formDto.number, {
        nonNullable: false,
        validators: [Validators.required],
      }),
    });
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

      const sub = this.azureService
        .postToAzureFunction(this.formDto)
        .subscribe({
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
  }

  getAllOddNumbersPosted(): void {
    const sub = this.azureService.getAllOddNumbers().subscribe({
      next: (value: AzNumberListDto) => {
        if (value && value.isSuccess) {
          console.log('Odd: ', value.items);
        }
      },
      error: (error: any) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  getAllEvenNumbersPosted(): void {
    const sub = this.azureService.getAllEvenNumbers().subscribe({
      next: (value: AzNumberListDto) => {
        if (value && value.isSuccess) {
          console.log('Even: ', value.items);
        }
      },
      error: (error: any) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  onReset(): void {
    this.formGroup.reset();
  }
}

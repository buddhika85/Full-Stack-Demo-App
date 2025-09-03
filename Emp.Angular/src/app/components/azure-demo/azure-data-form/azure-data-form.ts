import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { AzureService } from '../../../services/azure.service';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { AzPostToAzureFunc } from '../../../models/azPostToAzureFunc.dto';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-azure-data-form',
  imports: [],
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
      console.log(this.formDto);
    }
  }

  onReset(): void {
    this.formGroup.reset();
  }
}

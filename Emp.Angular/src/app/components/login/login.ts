import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { LoginDto } from '../../models/login.dto';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  private loginDto: LoginDto = { username: '', password: '' };
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  formGroup!: FormGroup<{
    username: FormControl<string>;
    password: FormControl<string>;
  }>;

  ngOnInit(): void {
    this.formGroup = this.formBuilder.group({
      username: new FormControl(this.loginDto.username, {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(5)],
      }),
      password: new FormControl(this.loginDto.password, {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(5)],
      }),
    });
  }

  get username(): FormControl<string> {
    return this.formGroup.controls.username;
  }

  get password(): FormControl<string> {
    return this.formGroup.controls.password;
  }

  onSubmit(): void {
    if (this.formGroup.valid) {
      this.loginDto = this.formGroup.getRawValue();
      console.log(this.loginDto);
    }
  }
}

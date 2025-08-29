import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { LoginDto } from '../../models/login.dto';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { LoginResponseDto } from '../../models/loginResponse.dto';
import { ProblemDetailsDto } from '../../models/problemDetails.dto';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit, OnDestroy {
  private readonly router: Router = inject(Router);
  private readonly authService: AuthService = inject(AuthService);
  private readonly compositeSubscription: Subscription = new Subscription();
  private loginDto: LoginDto = {
    username: '',
    password: '',
  };
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
      this.login();
    }
  }

  reset(): void {
    this.formGroup.reset(this.loginDto);
  }

  login() {
    const sub = this.authService.login(this.loginDto).subscribe({
      next: (response: LoginResponseDto) => {
        this.router.navigate(['']);
      },
      error: (error: ProblemDetailsDto) => {
        console.error(error.detail);
      },
    });
    this.compositeSubscription.add(sub);
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}

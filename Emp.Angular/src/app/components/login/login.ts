import { Component, inject, OnInit } from '@angular/core';
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

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  private readonly router: Router = inject(Router);
  private readonly authService: AuthService = inject(AuthService);

  private loginDto: LoginDto = {
    username: 'admin@emp.com',
    password: 'Admin@123',
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

  login() {
    this.authService.login(this.loginDto).subscribe({
      next: (response) => {
        console.log('Login Success');
        this.router.navigate(['']);
      },
      error: (error) => {
        console.error(error);
      },
    });
  }
}

import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { UserRoles } from '../../../../../models/userRoles';

@Component({
  selector: 'app-users-form',
  imports: [ReactiveFormsModule],
  templateUrl: './users-form.html',
  styleUrl: './users-form.scss',
})
export class UsersForm implements OnInit {
  onSubmit() {
    throw new Error('Method not implemented.');
  }

  UserRolesEnum = UserRoles; // expose enum to template
  userRoles: (keyof typeof UserRoles)[] = Object.keys(UserRoles).filter((key) =>
    isNaN(Number(key))
  ) as (keyof typeof UserRoles)[];

  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  formGroup!: FormGroup<{
    username: FormControl<string>;
    password: FormControl<string>;
    firstName: FormControl<string>;
    lastName: FormControl<string>;
    role: FormControl<UserRoles>;
  }>;

  ngOnInit(): void {
    this.formGroup = this.formBuilder.group({
      username: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.email,
          Validators.minLength(6),
          Validators.maxLength(50),
        ],
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(50),
        ],
      }),
      firstName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(50),
        ],
      }),
      lastName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(50),
        ],
      }),
      role: new FormControl(UserRoles.Staff, {
        nonNullable: true,
        validators: [Validators.required],
      }),
    });
  }

  get username(): FormControl<string> {
    return this.formGroup.controls.username;
  }

  get password(): FormControl<string> {
    return this.formGroup.controls.password;
  }

  get firstName(): FormControl<string> {
    return this.formGroup.controls.firstName;
  }

  get lastName(): FormControl<string> {
    return this.formGroup.controls.lastName;
  }

  get role(): FormControl<UserRoles> {
    return this.formGroup.controls.role;
  }

  reset() {
    this.formGroup.reset();
  }
}
